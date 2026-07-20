using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using System.Reflection;

namespace CardBeautify;

internal sealed partial class CardBeautifyLibraryWatcher : Node
{
    private const double PollInterval = 0.45;
    private static CardBeautifyLibraryWatcher? _instance;
    private static readonly FieldInfo? LibraryGridField = typeof(NCardLibrary).GetField("_grid", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? CardRowsField = typeof(NCardGrid).GetField("_cardRows", BindingFlags.Instance | BindingFlags.NonPublic);
    private double _elapsed;
    private NCardLibrary? _library;
    private bool _loggedVisibleLibrary;

    public static void Install()
    {
        if (GodotObject.IsInstanceValid(_instance)) return;
        if (Engine.GetMainLoop() is not SceneTree tree) return;
        _instance = new CardBeautifyLibraryWatcher { Name = "CardBeautifyLibraryWatcher" };
        tree.Root.CallDeferred(Node.MethodName.AddChild, _instance);
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed < PollInterval) return;
        _elapsed = 0;
        if (!GodotObject.IsInstanceValid(_library) || !_library!.IsVisibleInTree())
        {
            _library = FindVisibleLibrary(GetTree()?.Root);
            _loggedVisibleLibrary = false;
        }
        if (!GodotObject.IsInstanceValid(_library) || !_library!.IsVisibleInTree()) return;
        var stats = ApplyCards(_library);
        if (!_loggedVisibleLibrary && stats.Cards > 0)
        {
            _loggedVisibleLibrary = true;
            MainFile.Logger.Info($"[CardBeautify] encyclopedia watcher active: cards={stats.Cards}, eligible={stats.Eligible}, selectors={stats.Selectors}.");
        }
    }

    private static NCardLibrary? FindVisibleLibrary(Node? node)
    {
        if (node is NCardLibrary library && library.IsVisibleInTree()) return library;
        if (node == null) return null;
        foreach (var child in node.GetChildren())
        {
            var found = FindVisibleLibrary(child);
            if (found != null) return found;
        }
        return null;
    }

    private static (int Cards, int Eligible, int Selectors) ApplyCards(NCardLibrary library)
    {
        var cards = 0;
        var eligible = 0;
        var selectors = 0;
        if (LibraryGridField?.GetValue(library) is not NCardLibraryGrid grid)
            return (0, 0, 0);
        if (CardRowsField?.GetValue(grid) is not IEnumerable<List<NGridCardHolder>> rows)
            return (0, 0, 0);

        foreach (var row in rows)
        {
            foreach (var holder in row)
            {
                var card = holder?.CardNode;
                if (!GodotObject.IsInstanceValid(card)) continue;
                cards++;
                var model = card!.Model;
                if (model != null && CardArtCatalog.GetAvailablePackIds(CardArtCatalog.GetCardKey(model)).Count > 0) eligible++;
                CardNodePortraitPatch.ApplyToCard(card);
                if (card.GetNodeOrNull<Button>("CardBeautifyPerCardSelector") != null) selectors++;
            }
        }
        return (cards, eligible, selectors);
    }
}
