// TextMeshPro upper-cases UPPERCASE/SmallCaps styled text with char.ToUpper, which leaves 'ı' lowercase
// and turns 'i' into 'I'. These transpilers route those calls through TurkishCase.ToUpper while the
// Turkish text is active, so "Başlangıç Paketi" renders as "BAŞLANGIÇ PAKETİ".
// The mod scanner treats any System.Reflection.Emit use (even comparing OpCodes) as runtime code
// generation, so the transpiler matches instructions only by their method operand.
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;

public static class TurkishCase
{
    public static volatile bool Active;

    static readonly MethodInfo CharToUpper = new Func<char, char>(char.ToUpper).Method;
    static readonly MethodInfo Replacement = new Func<char, char>(ToUpper).Method;

    public static char ToUpper(char c)
    {
        if (Active)
        {
            if (c == 'i') return 'İ';
            if (c == 'ı') return 'I';
        }
        return char.ToUpper(c);
    }

    public static IEnumerable<CodeInstruction> ReplaceToUpper(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction ci in instructions)
        {
            if (CharToUpper.Equals(ci.operand))
                ci.operand = Replacement;
            yield return ci;
        }
    }
}

[HarmonyPatch(typeof(TextMeshProUGUI), "SetArraySizes")]
static class TurkishCaseUguiSetArraySizes
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) { return TurkishCase.ReplaceToUpper(instructions); }
}

[HarmonyPatch(typeof(TextMeshProUGUI), "GenerateTextMesh")]
static class TurkishCaseUguiGenerateTextMesh
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) { return TurkishCase.ReplaceToUpper(instructions); }
}

[HarmonyPatch(typeof(TextMeshPro), "SetArraySizes")]
static class TurkishCaseWorldSetArraySizes
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) { return TurkishCase.ReplaceToUpper(instructions); }
}

[HarmonyPatch(typeof(TextMeshPro), "GenerateTextMesh")]
static class TurkishCaseWorldGenerateTextMesh
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) { return TurkishCase.ReplaceToUpper(instructions); }
}

[HarmonyPatch(typeof(TMP_Text), "CalculatePreferredValues")]
static class TurkishCaseCalculatePreferredValues
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) { return TurkishCase.ReplaceToUpper(instructions); }
}
