using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace CardBeautify;

[HarmonyPatch]
internal static class CardNodePortraitPatch
{
    private sealed class PortraitState
    {
        public Texture2D? Texture { get; init; }
        public TextureRect.StretchModeEnum StretchMode { get; init; }
    }

    internal const string SelectorNodeName = "CardBeautifyPerCardSelector";
    private static readonly FieldInfo? PortraitField = typeof(NCard).GetField("_portrait", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly FieldInfo? AncientPortraitField = typeof(NCard).GetField("_ancientPortrait", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly Dictionary<string, PortraitState> DefaultPortraitStates = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, PortraitState> DefaultAncientPortraitStates = new(StringComparer.OrdinalIgnoreCase);
    private static ulong _lastScopeScanMsec;
    private static ulong _lastScopeGridInstanceId;
    private static bool _lastScopeHasOutsideCard;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    private static void UpdateVisualsPostfix(NCard __instance, PileType pileType, CardPreviewMode previewMode) => ApplyToCard(__instance);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(NCard), "_EnterTree")]
    private static void EnterTreePostfix(NCard __instance) => ApplyToCard(__instance);

    internal static void ApplyToCard(NCard cardNode)
    {
        var model = cardNode.Model;
        if (model == null) return;

        var cardKey = CardArtCatalog.GetCardKey(model);
        var rect = PortraitField?.GetValue(cardNode) as TextureRect;
        var ancientRect = AncientPortraitField?.GetValue(cardNode) as TextureRect;

        RememberDefaultState(cardKey, rect, DefaultPortraitStates);
        RememberDefaultState(cardKey, ancientRect, DefaultAncientPortraitStates);

        var texture = CardArtCatalog.GetTextureForCard(cardKey);
        if (texture == null)
        {
            RestoreDefaultState(cardKey, rect, DefaultPortraitStates);
            RestoreDefaultState(cardKey, ancientRect, DefaultAncientPortraitStates);
        }
        else
        {
            ApplyTexture(rect, texture);
            ApplyTexture(ancientRect, texture);
        }

        InstallOrUpdateSelector(cardNode, cardKey);
    }

    private static void ApplyTexture(TextureRect? rect, Texture2D texture)
    {
        if (rect == null) return;
        rect.Texture = texture;
        rect.StretchMode = (TextureRect.StretchModeEnum)6;
        rect.QueueRedraw();
    }

    private static void RememberDefaultState(string cardKey, TextureRect? rect, Dictionary<string, PortraitState> cache)
    {
        if (!string.IsNullOrWhiteSpace(cardKey) && rect != null && !cache.ContainsKey(cardKey) && !CardArtCatalog.IsReplacementTexture(rect.Texture))
        {
            cache[cardKey] = new PortraitState { Texture = rect.Texture, StretchMode = rect.StretchMode };
        }
    }

    private static void RestoreDefaultState(string cardKey, TextureRect? rect, Dictionary<string, PortraitState> cache)
    {
        if (!string.IsNullOrWhiteSpace(cardKey) && rect != null && cache.TryGetValue(cardKey, out var state))
        {
            rect.Texture = state.Texture;
            rect.StretchMode = state.StretchMode;
            rect.QueueRedraw();
        }
    }

    private static void InstallOrUpdateSelector(NCard cardNode, string cardKey)
    {
        var existing = cardNode.GetNodeOrNull<Button>(SelectorNodeName);
        if (!IsInCardLibrary(cardNode) || CardArtCatalog.GetAvailablePackIds(cardKey).Count == 0)
        {
            RemoveSelector(cardNode);
            return;
        }

        var button = existing;
        if (button == null)
        {
            button = new Button
            {
                Name = SelectorNodeName,
                FocusMode = Control.FocusModeEnum.None,
                MouseFilter = Control.MouseFilterEnum.Stop,
                TooltipText = "切换这张卡的卡图包",
                CustomMinimumSize = new Vector2(176f, 56f),
                ZIndex = 4096,
                Position = new Vector2(28f, -14f),
                Scale = Vector2.One,
                ClipText = true,
            };
            ApplyMobileSpireStyle(button);
            button.Pressed += () =>
            {
                var model = cardNode.Model;
                if (model == null) return;
                CardArtCatalog.SelectNextForCard(CardArtCatalog.GetCardKey(model));
                var root = cardNode.GetTree()?.Root ?? (Node)cardNode;
                RefreshCards(root);
            };
            cardNode.AddChild(button);
        }
        else
        {
            ApplyMobileSpireStyle(button);
        }

        button.Text = CardArtCatalog.GetSelectorText(cardKey);
        button.Visible = true;
    }

    private static void ApplyMobileSpireStyle(Button button)
    {
        button.CustomMinimumSize = new Vector2(176f, 56f);
        button.Position = new Vector2(28f, -14f);
        button.Scale = Vector2.One;
        button.AddThemeFontSizeOverride("font_size", 18);
        button.AddThemeColorOverride("font_color", new Color(1.0f, 0.91f, 0.58f));
        button.AddThemeColorOverride("font_hover_color", new Color(1.0f, 0.98f, 0.75f));
        button.AddThemeColorOverride("font_pressed_color", new Color(0.88f, 0.73f, 0.38f));
        button.AddThemeColorOverride("font_disabled_color", new Color(0.55f, 0.45f, 0.32f));
        button.AddThemeStyleboxOverride("normal", MakeButtonStyle(new Color(0.16f, 0.105f, 0.065f, 0.92f), new Color(0.78f, 0.58f, 0.28f, 1f), 3));
        button.AddThemeStyleboxOverride("hover", MakeButtonStyle(new Color(0.22f, 0.145f, 0.075f, 0.96f), new Color(1.0f, 0.78f, 0.36f, 1f), 4));
        button.AddThemeStyleboxOverride("pressed", MakeButtonStyle(new Color(0.105f, 0.07f, 0.045f, 0.98f), new Color(0.66f, 0.46f, 0.22f, 1f), 3));
        button.AddThemeStyleboxOverride("focus", MakeButtonStyle(new Color(0.22f, 0.145f, 0.075f, 0.36f), new Color(1.0f, 0.84f, 0.42f, 1f), 5));
    }

    private static StyleBoxFlat MakeButtonStyle(Color bg, Color border, int borderWidth)
    {
        var style = new StyleBoxFlat
        {
            BgColor = bg,
            BorderColor = border,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = 12,
            CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12,
            CornerRadiusBottomRight = 12,
            ShadowColor = new Color(0f, 0f, 0f, 0.55f),
            ShadowSize = 5,
            ShadowOffset = new Vector2(0f, 3f),
            ContentMarginLeft = 10f,
            ContentMarginRight = 10f,
            ContentMarginTop = 5f,
            ContentMarginBottom = 5f,
        };
        return style;
    }

    private static bool IsInCardLibrary(Node node)
    {
        NCardLibrary? library = null;
        NCardLibraryGrid? grid = null;
        for (var current = node; current != null; current = current.GetParent())
        {
            if (grid is null && current is NCardLibraryGrid candidateGrid)
                grid = candidateGrid;
            if (current is NCardLibrary candidateLibrary)
            {
                library = candidateLibrary;
                break;
            }
        }

        if (library is null || grid is null || !library.IsVisibleInTree() || !grid.IsVisibleInTree())
            return false;
        if (!IsUnderCurrentScene(library))
            return false;

        try
        {
            var ownedGrid = typeof(NCardLibrary).GetField("_grid", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(library);
            return ReferenceEquals(ownedGrid, grid) && !HasVisibleCardOutsideGrid(grid);
        }
        catch { return false; }
    }

    // The encyclopedia detail popup leaves the library grid alive underneath
    // an enlarged NCard. Hide every grid selector while that popup is visible.
    private static bool HasVisibleCardOutsideGrid(NCardLibraryGrid grid)
    {
        try
        {
            var now = Time.GetTicksMsec();
            var gridId = grid.GetInstanceId();
            if (_lastScopeGridInstanceId == gridId && now - _lastScopeScanMsec < 100)
                return _lastScopeHasOutsideCard;
            if (Engine.GetMainLoop() is not SceneTree tree || tree.Root is null)
                return false;

            _lastScopeHasOutsideCard = HasVisibleCardOutsideGrid(tree.Root, grid);
            _lastScopeGridInstanceId = gridId;
            _lastScopeScanMsec = now;
            return _lastScopeHasOutsideCard;
        }
        catch { return false; }
    }

    private static bool HasVisibleCardOutsideGrid(Node root, NCardLibraryGrid grid)
    {
        try
        {
            if (ReferenceEquals(root, grid))
                return false;
            if (root is NCard card && IsVisibleInTreeStrict(card))
                return true;
            foreach (var child in root.GetChildren())
                if (child is Node childNode && HasVisibleCardOutsideGrid(childNode, grid))
                    return true;
        }
        catch { }
        return false;
    }

    private static bool IsVisibleInTreeStrict(Node node)
    {
        try
        {
            return node is CanvasItem item && item.IsInsideTree() && item.Visible && item.IsVisibleInTree();
        }
        catch { return false; }
    }

    private static bool IsUnderCurrentScene(Node node)
    {
        try
        {
            if (Engine.GetMainLoop() is not SceneTree tree || tree.CurrentScene is null)
                return false;
            for (var current = node; current != null; current = current.GetParent())
                if (ReferenceEquals(current, tree.CurrentScene)) return true;
        }
        catch { }
        return false;
    }

    internal static void RemoveSelector(NCard cardNode)
    {
        try
        {
            var selector = cardNode.GetNodeOrNull<Button>(SelectorNodeName);
            if (selector is null || !GodotObject.IsInstanceValid(selector)) return;
            selector.Visible = false;
            selector.MouseFilter = Control.MouseFilterEnum.Ignore;
            selector.QueueFree();
        }
        catch { }
    }

    internal static void CleanupAllSelectors(Node root)
    {
        try
        {
            if (root is NCard card)
                RemoveSelector(card);
            foreach (var child in root.GetChildren())
                if (child is Node childNode) CleanupAllSelectors(childNode);
        }
        catch { }
    }

    private static void RefreshCards(Node node)
    {
        if (node is NCard card) ApplyToCard(card);
        foreach (var child in node.GetChildren()) RefreshCards(child);
        if (node is CanvasItem item) item.QueueRedraw();
    }
}
