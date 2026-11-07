// Switching between Türkçe and English keeps the selected locale on "en", so Unity fires no locale-changed event
// and the game's own text components would keep the old language until the scene reloads. This re-runs their
// locale handlers. Reflection is used only on the game's own types with literal names, which the mod scanner
// allows without a capability.
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class TurkishUiRefresher
{
    static readonly MethodInfo TmpLocaleChanged = AccessTools.Method(typeof(TMPLocalizer), "OnLocaleChanged");
    static readonly MethodInfo InputLocaleChanged = AccessTools.Method(typeof(TMPInputFieldLocalizer), "OnLocaleChanged");

    public static void RefreshGameLocalizers()
    {
        Locale locale = LocalizationSettings.SelectedLocale;
        object[] args = { locale };
        Invoke<TMPLocalizer>(TmpLocaleChanged, args);
        Invoke<TMPInputFieldLocalizer>(InputLocaleChanged, args);
        foreach (DropdownLocalizer d in Object.FindObjectsOfType<DropdownLocalizer>())
            Safe(d.UpdateOptions);
        foreach (ResolutionLocalizer r in Object.FindObjectsOfType<ResolutionLocalizer>())
            Safe(r.UpdateOptions);
    }

    static void Invoke<T>(MethodInfo method, object[] args) where T : Object
    {
        if (method == null)
            return;
        foreach (T component in Object.FindObjectsOfType<T>())
        {
            try
            {
                method.Invoke(component, args);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Türkçe Yama] Could not refresh " + typeof(T).Name + ": " + e.Message);
            }
        }
    }

    static void Safe(System.Action action)
    {
        try
        {
            action();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Türkçe Yama] Could not refresh a dropdown: " + e.Message);
        }
    }
}
