using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace CardBeautify;

[HarmonyPatch(typeof(SaveManager), "InitProgressData")]
internal static class CardBeautifyWarmupPatch
{
    private static bool _logged;

    private static void Postfix()
    {
        if (_logged) return;
        _logged = true;
        CardArtCatalog.LogCoverage();
    }
}
