using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace CardBeautify;

[HarmonyPatch(typeof(CardModel), "get_Portrait")]
internal static class CardModelPortraitPatch
{
    private static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        var cardKey = CardArtCatalog.GetCardKey(__instance);
        var texture = CardArtCatalog.GetTextureForCard(cardKey);
        if (texture != null) __result = CardNodePortraitPatch.GetDisplayTexture(cardKey, texture);
    }
}
