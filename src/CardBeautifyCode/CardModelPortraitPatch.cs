using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace CardBeautify;

[HarmonyPatch(typeof(CardModel), "get_Portrait")]
internal static class CardModelPortraitPatch
{
    private static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        var texture = CardArtCatalog.GetTextureForCard(CardArtCatalog.GetCardKey(__instance));
        if (texture != null) __result = texture;
    }
}
