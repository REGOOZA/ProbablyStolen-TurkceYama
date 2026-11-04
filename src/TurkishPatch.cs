// Probably Stolen - Turkce Yama
// Replaces the English (en) string tables with Turkish text loaded from tr.tsv in the mod folder.
// Keys missing from tr.tsv keep the game's own text, so new lines from game updates show in English.
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TurkishPatchMod : IMod
{
    const string TargetLocaleCode = "en";
    const string TranslationFile = "tr.tsv";
    // Dynamic TMP font that ships with the game and covers all Turkish letters.
    const string FallbackFontName = "LiberationSans SDF - Fallback";

    ModManifest manifest;
    ModLog log;
    Dictionary<string, Dictionary<long, string>> translations;
    // Held on purpose: keeps the patched tables loaded so a reload returns the same (patched) objects.
    AsyncOperationHandle<IList<StringTable>> tablesHandle;
    bool hasTablesHandle;
    bool subscribed;

    public void Init(ModManifest manifest)
    {
        this.manifest = manifest;
        log = new ModLog(manifest);
    }

    // Mod files can only be read once the loader has registered the mod, which happens after Init/OnEnable.
    public void OnEnable()
    {
        ModHook.OnModAssetsLoaded += OnModAssetsLoaded;
    }

    public void OnDisable()
    {
        ModHook.OnModAssetsLoaded -= OnModAssetsLoaded;
        if (subscribed)
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        subscribed = false;
    }

    void OnModAssetsLoaded(LoadedMod mod)
    {
        if (mod == null || mod.Manifest == null || mod.Manifest.ID != manifest.ID || translations != null)
            return;

        translations = LoadTranslations();
        if (translations == null)
            return;

        AddFontFallback();
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        subscribed = true;

        var init = LocalizationSettings.InitializationOperation;
        if (init.IsDone)
            PatchTables();
        else
            init.Completed += delegate { PatchTables(); };
    }

    void OnSelectedLocaleChanged(Locale locale)
    {
        bool turkish = locale != null && locale.Identifier.Code == TargetLocaleCode;
        TurkishCase.Active = turkish;
        RefreshUppercaseText();
        if (turkish)
            PatchTables();
    }

    void PatchTables()
    {
        if (hasTablesHandle && tablesHandle.IsValid() && tablesHandle.IsDone)
        {
            ApplyTranslations(tablesHandle);
            return;
        }

        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(TargetLocaleCode);
        if (locale == null)
        {
            log.Warning("Locale '" + TargetLocaleCode + "' not found; Turkish patch not applied.");
            return;
        }

        tablesHandle = LocalizationSettings.StringDatabase.GetAllTables(locale);
        hasTablesHandle = true;
        tablesHandle.Completed += ApplyTranslations;
    }

    void ApplyTranslations(AsyncOperationHandle<IList<StringTable>> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
        {
            log.Warning("Could not load the '" + TargetLocaleCode + "' string tables: " + handle.OperationException);
            return;
        }

        int replaced = 0, added = 0, tables = 0;
        foreach (StringTable table in handle.Result)
        {
            if (table == null)
                continue;
            Dictionary<long, string> map;
            if (!translations.TryGetValue(table.TableCollectionName, out map))
                continue;
            tables++;
            var present = new HashSet<long>();
            foreach (StringTableEntry entry in table.Values)
            {
                if (entry == null)
                    continue;
                present.Add(entry.KeyId);
                string text;
                if (map.TryGetValue(entry.KeyId, out text) && entry.Value != text)
                {
                    entry.Value = text;
                    replaced++;
                }
            }
            // Add keys the target table lacks so they don't fall back to another language.
            foreach (KeyValuePair<long, string> kv in map)
            {
                if (!present.Contains(kv.Key))
                {
                    table.AddEntry(kv.Key, kv.Value);
                    added++;
                }
            }
        }

        log.Log("Turkish patch: " + replaced + " strings replaced, " + added + " added, in " + tables + " tables.");
        replaced += added;

        Locale selected = LocalizationSettings.SelectedLocale;
        bool turkishSelected = selected != null && selected.Identifier.Code == TargetLocaleCode;
        TurkishCase.Active = turkishSelected;
        if (replaced > 0 && turkishSelected)
            RefreshVisibleText();
        RefreshUppercaseText();
    }

    static void RefreshVisibleText()
    {
        foreach (LocalizeStringEvent ev in Object.FindObjectsOfType<LocalizeStringEvent>())
            ev.RefreshString();
    }

    // Upper-cased text already on screen keeps its old glyphs until TMP rebuilds it.
    static void RefreshUppercaseText()
    {
        foreach (TMP_Text text in Object.FindObjectsOfType<TMP_Text>())
            if ((text.fontStyle & (FontStyles.UpperCase | FontStyles.SmallCaps)) != 0)
                text.SetAllDirty();
    }

    void AddFontFallback()
    {
        List<TMP_FontAsset> global = TMP_Settings.fallbackFontAssets;
        if (global == null)
            return;
        foreach (TMP_FontAsset font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (font != null && font.name == FallbackFontName && !global.Contains(font))
            {
                global.Add(font);
                log.Log("Added '" + FallbackFontName + "' as a global fallback font.");
            }
        }
    }

    // tr.tsv: one entry per line, "Collection<TAB>KeyId<TAB>Text"; \n, \t and \\ are escaped; '#' starts a comment.
    Dictionary<string, Dictionary<long, string>> LoadTranslations()
    {
        string content;
        try
        {
            content = ModHelper.ReadModFileText(manifest.ID, TranslationFile);
        }
        catch (System.Exception e)
        {
            log.Error("Could not read " + TranslationFile + ": " + e.Message);
            return null;
        }
        if (string.IsNullOrEmpty(content))
        {
            log.Error(TranslationFile + " is missing or empty; Turkish patch disabled.");
            return null;
        }

        var result = new Dictionary<string, Dictionary<long, string>>();
        int count = 0, bad = 0;
        foreach (string rawLine in content.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.Length == 0 || line[0] == '#')
                continue;
            string[] parts = line.Split(new[] { '\t' }, 3);
            long keyId;
            if (parts.Length != 3 || !long.TryParse(parts[1], out keyId))
            {
                bad++;
                continue;
            }
            Dictionary<long, string> map;
            if (!result.TryGetValue(parts[0], out map))
            {
                map = new Dictionary<long, string>();
                result[parts[0]] = map;
            }
            map[keyId] = Unescape(parts[2]);
            count++;
        }

        log.Log("Loaded " + count + " Turkish strings" + (bad > 0 ? " (" + bad + " malformed lines skipped)" : "") + ".");
        return result;
    }

    static string Unescape(string s)
    {
        if (s.IndexOf('\\') < 0)
            return s;
        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '\\' && i + 1 < s.Length)
            {
                char n = s[++i];
                if (n == 'n') sb.Append('\n');
                else if (n == 't') sb.Append('\t');
                else if (n == 'r') sb.Append('\r');
                else sb.Append(n);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
