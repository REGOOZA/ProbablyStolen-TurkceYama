// Puts "Türkçe" in the game's language menu and maps it onto the English locale.
// The menu is built from MainMenuUIController.LOCALE_DROPDOWN (code, label); picking an entry saves its code as
// the "locale" setting and calls LocHelper.SetLocale(code). The game only has string tables for its own locales,
// so SetLocale("tr") is turned into SetLocale("en") with the Turkish text switched on.
using System;
using System.Collections.Generic;
using HarmonyLib;

public static class TurkishLanguage
{
    public const string Code = "tr";
    public const string Label = "Türkçe";
    // Mod data file that marks the one-time switch of existing players from English to Türkçe (v1.6).
    const string MigratedFile = "dil_menusu_v16.txt";

    public static ValueTuple<string, string>[] WithTurkish(ValueTuple<string, string>[] list)
    {
        if (list == null)
            return list;
        foreach (ValueTuple<string, string> entry in list)
            if (entry.Item1 == Code)
                return list;
        // Türkçe goes first. Only the two menu methods patched below read the list, and both use this copy,
        // so the dropdown index and the language code always agree.
        var result = new ValueTuple<string, string>[list.Length + 1];
        result[0] = new ValueTuple<string, string>(Code, Label);
        Array.Copy(list, 0, result, 1, list.Length);
        return result;
    }

    // Before v1.6 the patch replaced English outright, so its players have "en" saved. Move them to Türkçe once;
    // after that, a saved "en" means the player chose English.
    public static string MigrateSavedLocale(string saved)
    {
        TurkishPatchMod mod = TurkishPatchMod.Instance;
        if (mod == null || saved != TurkishPatchMod.BaseLocaleCode)
            return saved;
        try
        {
            if (ModHelper.ModDataFileExists(mod.ModId, MigratedFile))
                return saved;
            ModHelper.WriteModDataFileText(mod.ModId, MigratedFile, "Dil menusune Turkce eklendi; kayitli English bir kez Turkce yapildi.");
            ES3.Save<string>("locale", Code);
            mod.Log("Saved language moved from English to Türkçe (one time).");
            return Code;
        }
        catch (Exception e)
        {
            mod.Log("Could not move the saved language to Türkçe: " + e.Message);
            return saved;
        }
    }
}

// LOCALE_DROPDOWN is static readonly, and Mono folds such arrays into the compiled code, so swapping the field
// does not reach the game's methods. Both menu methods are therefore re-run here on a list that includes Türkçe,
// doing exactly what the originals do (fill the dropdown, select the saved language / save the chosen one).
[HarmonyPatch(typeof(MainMenuUIController), "HandleLocaleDropdown")]
static class TurkishLanguageDropdownPatch
{
    static bool Prefix(MainMenuUIController __instance, ValueTuple<string, string>[] ___LOCALE_DROPDOWN)
    {
        ValueTuple<string, string>[] list = TurkishLanguage.WithTurkish(___LOCALE_DROPDOWN);
        if (list == null || list.Length == 0 || __instance.localeDropdown == null)
            return true;
        string saved = LocHelper.ResolveStartupLocale();
        var labels = new List<string>(list.Length);
        int selected = 0;
        for (int i = 0; i < list.Length; i++)
        {
            labels.Add(list[i].Item2);
            if (list[i].Item1 == saved)
                selected = i;
        }
        __instance.localeDropdown.ClearOptions();
        __instance.localeDropdown.AddOptions(labels);
        __instance.localeDropdown.SetValueWithoutNotify(selected);
        __instance.StartCoroutine(LocHelper.SetLocale(list[selected].Item1));
        return false;
    }
}

[HarmonyPatch(typeof(MainMenuUIController), "OnLocaleChanged")]
static class TurkishLanguageDropdownChangedPatch
{
    static bool Prefix(MainMenuUIController __instance, int localeIndex, ValueTuple<string, string>[] ___LOCALE_DROPDOWN)
    {
        ValueTuple<string, string>[] list = TurkishLanguage.WithTurkish(___LOCALE_DROPDOWN);
        if (list == null || localeIndex < 0 || localeIndex >= list.Length)
            return true;
        string code = list[localeIndex].Item1;
        __instance.StartCoroutine(LocHelper.SetLocale(code));
        ES3.Save<string>("locale", code);
        return false;
    }
}

[HarmonyPatch(typeof(LocHelper), "ResolveStartupLocale")]
static class TurkishLanguageStartupPatch
{
    static void Postfix(ref string __result)
    {
        __result = TurkishLanguage.MigrateSavedLocale(__result);
    }
}

[HarmonyPatch(typeof(LocHelper), "SetLocale")]
static class TurkishLanguageSetLocalePatch
{
    static void Prefix(ref string localeCode)
    {
        bool turkish = localeCode == TurkishLanguage.Code;
        if (turkish)
            localeCode = TurkishPatchMod.BaseLocaleCode;
        if (TurkishPatchMod.Instance != null)
            TurkishPatchMod.Instance.SetTurkish(turkish);
        else
            TurkishPatchMod.TurkishWanted = turkish;
    }
}
