using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace CardBeautify;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "CardBeautify";
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        CardArtCatalog.MountAssets();
        CardArtCatalog.InitializeStorage();
        // CardModel.Portrait is the only detour required for replacing art.
        // PatchAll previously installed six ARM64 detours, including two NCard
        // finalizers which could abort Mono while the game was starting.
        new Harmony(ModId).CreateClassProcessor(typeof(CardModelPortraitPatch)).Patch();
        CardBeautifyLibraryWatcher.Install();
        CardArtCatalog.LogCoverage();
        Logger.Info("[CardBeautify] loaded v0.5.0: the card-art selector is restricted to the active encyclopedia grid and removed before pooled cards enter a run; selected art persists across launches.");
    }
}
