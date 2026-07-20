using System;
using System.Threading;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

namespace CardBeautify;

// Android v103 occasionally throws the base-game "You monster!" guard while
// NCardGrid is building deck/draw/discard/exhaust pile cards.  The old workaround
// disabled beautified portraits in those screens, which defeats the point of this
// mod.  This compat layer keeps portraits enabled and only suppresses that very
// narrow vanilla visual-refresh exception for cards that are actually inside the
// pile/card-grid screens.
[HarmonyPatch(typeof(NCard), "Reload")]
internal static class CardBeautifyNCardReloadCompatPatch
{
    private static Exception? Finalizer(Exception __exception, NCard __instance)
        => CardPileVisualCompat.Handle(__exception, __instance, "Reload");
}

[HarmonyPatch(typeof(NCard), "UpdateVisuals", typeof(PileType), typeof(CardPreviewMode))]
internal static class CardBeautifyNCardUpdateVisualsCompatPatch
{
    private static Exception? Finalizer(Exception __exception, NCard __instance, PileType pileType)
        => CardPileVisualCompat.Handle(__exception, __instance, $"UpdateVisuals({pileType})", pileType);
}

internal static class CardPileVisualCompat
{
    private static int _swallowedCount;

    public static Exception? Handle(Exception? exception, NCard? card, string source, PileType? pileType = null)
    {
        if (exception == null) return null;
        if (card == null || !IsTargetException(exception) || !ShouldSuppressInGridContext(card, pileType)) return exception;

        try
        {
            CardNodePortraitPatch.ApplyToCard(card);
        }
        catch (Exception artException)
        {
            MainFile.Logger.Warn($"[CardBeautify] pile-screen portrait fallback failed after {source}: {artException}");
        }

        var count = Interlocked.Increment(ref _swallowedCount);
        if (count <= 5)
        {
            MainFile.Logger.Warn($"[CardBeautify] suppressed Android pile-screen NCard visual exception in {source}: {exception}");
        }
        else if (count <= 20)
        {
            MainFile.Logger.Warn($"[CardBeautify] suppressed Android pile-screen NCard visual exception in {source}: {exception.Message}");
        }
        else if (count == 21)
        {
            MainFile.Logger.Warn("[CardBeautify] further pile-screen NCard visual exceptions will be suppressed silently.");
        }

        return null;
    }

    private static bool IsTargetException(Exception exception)
        => exception is InvalidOperationException
           && (exception.Message?.IndexOf("You monster", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;

    private static bool ShouldSuppressInGridContext(Node node, PileType? pileType)
    {
        // Combat hand visuals should keep throwing real errors.  Deck, draw,
        // discard, exhaust and master-deck grid cards are display-only nodes;
        // the Android v103 guard can be suppressed there.
        if (pileType.HasValue && pileType.Value != PileType.Hand) return true;
        return IsInCardPileScreen(node) || IsInCardGrid(node);
    }

    private static bool IsInCardPileScreen(Node node)
    {
        for (var current = node; current != null; current = current.GetParent())
        {
            if (current is NCardPileScreen) return true;
        }

        try
        {
            return NCapstoneContainer.Instance?.CurrentCapstoneScreen is NCardPileScreen;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsInCardGrid(Node node)
    {
        for (var current = node; current != null; current = current.GetParent())
        {
            if (current is NCardGrid) return true;
        }
        return false;
    }
}
