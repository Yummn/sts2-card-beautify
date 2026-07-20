using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace CardBeautify;

internal static class CardArtCatalog
{
    private sealed class CatalogManifest
    {
        [JsonPropertyName("packs")]
        public List<PackDefinition> Packs { get; set; } = new();
    }

    private sealed class PackDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("shortLabel")]
        public string ShortLabel { get; set; } = string.Empty;

        [JsonPropertyName("entries")]
        public Dictionary<string, string> Entries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed record PackEntry(string PackId, string ResourcePath);

    private const string AssetsPckName = "CardBeautifyAssets.pck";
    private const string ManifestFileName = "Data/CardBeautifyCatalog.catalog";
    private const string SelectionFileName = "CardBeautify.per-card.dat";
    private const string LegacySelectionFileName = "CardBeautify.per-card.json";

    private static readonly string[] PreferredPackOrder =
    {
        "Original", "AnimeDefectMinimal", "AnimeDefectSpire", "Aduare", "Diana"
    };

    private static readonly Lazy<string> ModDirectory = new(() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "");
    private static readonly Dictionary<string, Texture2D?> TextureCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> PerCardSelections = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReplacementResourcePaths = new(StringComparer.OrdinalIgnoreCase);

    private static CatalogManifest? _manifest;
    private static bool _assetsMounted;
    private static bool _selectionsLoaded;
    private static bool _betterDefectPortraitFolderLogged;

    private static string SelectionFilePath => Path.Combine(GetRuntimeDataDirectory(), SelectionFileName);

    public static void InitializeStorage()
    {
        EnsureSelectionsLoaded();
        MainFile.Logger.Info($"[CardBeautify] per-card art selection storage ready: {SelectionFilePath}; selected={PerCardSelections.Count}.");
    }

    public static void MountAssets()
    {
        if (_assetsMounted) return;
        _assetsMounted = true;

        var pck = Path.Combine(ModDirectory.Value, AssetsPckName);
        if (!File.Exists(pck))
        {
            MainFile.Logger.Warn("[CardBeautify] asset pack not found: " + pck);
            return;
        }

        var ok = ProjectSettings.LoadResourcePack(pck);
        MainFile.Logger.Info("[CardBeautify] asset pack mount " + (ok ? "succeeded" : "failed") + ": " + pck);
        LogBetterDefectPortraitFolder();
    }

    public static string GetCardKey(CardModel model) => ToSnakeCase(model.GetType().Name);

    public static Texture2D? GetTextureForCard(string cardKey)
    {
        var entry = GetEntryForCard(cardKey);
        if (entry == null) return TryLoadBetterDefectPortrait(cardKey);

        if (TextureCache.TryGetValue(entry.ResourcePath, out var cached)) return cached;

        try
        {
            var texture = GD.Load<Texture2D>(entry.ResourcePath);
            TextureCache[entry.ResourcePath] = texture;
            return texture;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[CardBeautify] failed to load '{entry.ResourcePath}' for {cardKey}: {ex.Message}");
            TextureCache[entry.ResourcePath] = null;
            return null;
        }
    }

    private static Texture2D? TryLoadBetterDefectPortrait(string cardKey)
    {
        var normalized = NormalizeBetterDefectPortraitKey(cardKey);
        if (normalized == null) return null;

        var cacheKey = "BetterDefect:" + normalized;
        if (TextureCache.TryGetValue(cacheKey, out var cached)) return cached;

        try
        {
            var modDir = ModDirectory.Value;
            var modsDir = Directory.GetParent(modDir)?.FullName;
            if (string.IsNullOrWhiteSpace(modsDir))
            {
                TextureCache[cacheKey] = null;
                return null;
            }

            var path = Path.Combine(modsDir, "BetterDefect", "Data", "Portraits", normalized + ".png");
            if (!File.Exists(path))
            {
                TextureCache[cacheKey] = null;
                return null;
            }

            var image = new Image();
            var error = image.Load(path);
            if (error != Error.Ok)
            {
                MainFile.Logger.Warn($"[CardBeautify] failed to load BetterDefect portrait '{path}' for {cardKey}: {error}");
                TextureCache[cacheKey] = null;
                return null;
            }

            var texture = ImageTexture.CreateFromImage(image);
            TextureCache[cacheKey] = texture;
            MainFile.Logger.Info($"[CardBeautify] loaded BetterDefect restored StS1 portrait for {cardKey}: {Path.GetFileName(path)}");
            return texture;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[CardBeautify] failed to load BetterDefect portrait for {cardKey}: {ex.Message}");
            TextureCache[cacheKey] = null;
            return null;
        }
    }

    private static void LogBetterDefectPortraitFolder()
    {
        if (_betterDefectPortraitFolderLogged) return;
        _betterDefectPortraitFolderLogged = true;

        try
        {
            var modDir = ModDirectory.Value;
            var modsDir = Directory.GetParent(modDir)?.FullName;
            var portraitsDir = string.IsNullOrWhiteSpace(modsDir)
                ? ""
                : Path.Combine(modsDir, "BetterDefect", "Data", "Portraits");

            var count = Directory.Exists(portraitsDir)
                ? Directory.GetFiles(portraitsDir, "*.png", SearchOption.TopDirectoryOnly).Length
                : 0;
            MainFile.Logger.Info($"[CardBeautify] BetterDefect portrait folder: {portraitsDir}; png={count}.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[CardBeautify] failed to inspect BetterDefect portrait folder: {ex.Message}");
        }
    }

    private static string? NormalizeBetterDefectPortraitKey(string cardKey)
    {
        if (string.IsNullOrWhiteSpace(cardKey)) return null;
        if (cardKey.StartsWith("bd_", StringComparison.OrdinalIgnoreCase)) return cardKey.Substring(3);

        return cardKey switch
        {
            "hello_world" or "rebound" or "rip_and_tear" or "stack" => cardKey,
            _ => null
        };
    }

    public static bool IsReplacementTexture(Texture2D? texture)
    {
        EnsureManifestLoaded();
        var path = texture?.ResourcePath?.Trim();
        return !string.IsNullOrWhiteSpace(path) && ReplacementResourcePaths.Contains(path);
    }

    public static IReadOnlyList<string> GetAvailablePackIds(string cardKey)
    {
        EnsureManifestLoaded();
        if (_manifest == null) return Array.Empty<string>();
        return GetOrderedPacks().Where(p => p.Entries.ContainsKey(cardKey)).Select(p => p.Id).ToList();
    }

    public static string GetSelectorText(string cardKey)
    {
        EnsureSelectionsLoaded();
        var selected = GetSelectedPack(cardKey);
        return selected == null ? "卡图: 自动" : "卡图: " + selected.ShortLabel;
    }

    public static void SelectNextForCard(string cardKey)
    {
        EnsureSelectionsLoaded();
        EnsureManifestLoaded();

        var available = GetAvailablePackIds(cardKey).ToList();
        if (available.Count == 0) return;

        var choices = new List<string?> { null };
        choices.AddRange(available);
        PerCardSelections.TryGetValue(cardKey, out var current);
        var index = choices.FindIndex(choice => string.Equals(choice, current, StringComparison.OrdinalIgnoreCase));
        var next = choices[(index + 1 + choices.Count) % choices.Count];

        if (string.IsNullOrWhiteSpace(next)) PerCardSelections.Remove(cardKey);
        else PerCardSelections[cardKey] = next;

        TextureCache.Clear();
        SaveSelections();
        MainFile.Logger.Info("[CardBeautify] " + cardKey + " art source: " + (next ?? "Auto"));
    }

    public static void LogCoverage()
    {
        try
        {
            EnsureManifestLoaded();
            EnsureSelectionsLoaded();
            if (_manifest == null)
            {
                MainFile.Logger.Warn("[CardBeautify] no manifest loaded.");
                return;
            }

            var total = 0;
            var covered = 0;
            foreach (var card in ModelDb.AllCards)
            {
                total++;
                if (GetEntryForCard(GetCardKey(card)) != null) covered++;
            }

            var packs = string.Join(", ", GetOrderedPacks().Select(pack => $"{pack.Label}={pack.Entries.Count}"));
            MainFile.Logger.Info($"[CardBeautify] packs: {packs}; covered={covered}/{total}; per-card selections={PerCardSelections.Count}.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn("[CardBeautify] coverage check failed: " + ex.Message);
        }
    }

    private static PackEntry? GetEntryForCard(string cardKey)
    {
        EnsureManifestLoaded();
        EnsureSelectionsLoaded();
        if (_manifest == null) return null;

        var selected = GetSelectedPack(cardKey);
        if (selected != null && selected.Entries.TryGetValue(cardKey, out var selectedPath))
            return new PackEntry(selected.Id, selectedPath);

        foreach (var pack in GetOrderedPacks())
        {
            if (pack.Entries.TryGetValue(cardKey, out var path)) return new PackEntry(pack.Id, path);
        }
        return null;
    }

    private static PackDefinition? GetSelectedPack(string cardKey)
    {
        if (_manifest == null) return null;
        if (!PerCardSelections.TryGetValue(cardKey, out var packId)) return null;
        var pack = _manifest.Packs.FirstOrDefault(p => string.Equals(p.Id, packId, StringComparison.OrdinalIgnoreCase));
        return pack != null && pack.Entries.ContainsKey(cardKey) ? pack : null;
    }

    private static IEnumerable<PackDefinition> GetOrderedPacks()
    {
        EnsureManifestLoaded();
        if (_manifest == null) yield break;

        foreach (var id in PreferredPackOrder)
        {
            var pack = _manifest.Packs.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
            if (pack != null) yield return pack;
        }

        foreach (var pack in _manifest.Packs.OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (!PreferredPackOrder.Contains(pack.Id, StringComparer.OrdinalIgnoreCase)) yield return pack;
        }
    }

    private static void EnsureManifestLoaded()
    {
        if (_manifest != null) return;
        try
        {
            var path = Path.Combine(ModDirectory.Value, ManifestFileName);
            var json = File.ReadAllText(path);
            _manifest = JsonSerializer.Deserialize<CatalogManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CatalogManifest();
            ReplacementResourcePaths.Clear();
            foreach (var pathValue in _manifest.Packs.SelectMany(p => p.Entries.Values)) ReplacementResourcePaths.Add(pathValue);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn("[CardBeautify] failed to load manifest: " + ex.Message);
            _manifest = new CatalogManifest();
        }
    }

    private static void EnsureSelectionsLoaded()
    {
        if (_selectionsLoaded) return;
        _selectionsLoaded = true;
        PerCardSelections.Clear();
        try
        {
            var loaded = LoadSelectionDictionary(SelectionFilePath);
            if (loaded == null)
            {
                foreach (var legacyPath in GetLegacySelectionPaths())
                {
                    loaded = LoadSelectionDictionary(legacyPath);
                    if (loaded == null) continue;

                    MainFile.Logger.Info($"[CardBeautify] migrated per-card art selections: {legacyPath} -> {SelectionFilePath}");
                    break;
                }
            }

            if (loaded == null)
            {
                SaveSelections();
                return;
            }

            foreach (var kv in loaded)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value)) PerCardSelections[kv.Key] = kv.Value;
            }

            SaveSelections();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn("[CardBeautify] failed to load per-card selections: " + ex.Message);
        }
    }

    private static void SaveSelections()
    {
        try
        {
            var dir = Path.GetDirectoryName(SelectionFilePath);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(PerCardSelections, new JsonSerializerOptions { WriteIndented = true });
            var tmp = SelectionFilePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(SelectionFilePath)) File.Delete(SelectionFilePath);
            File.Move(tmp, SelectionFilePath);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn("[CardBeautify] failed to save per-card selections: " + ex.Message);
        }
    }

    private static Dictionary<string, string>? LoadSelectionDictionary(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            if (loaded == null) return null;
            return new Dictionary<string, string>(
                loaded.Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value)),
                StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[CardBeautify] failed to read per-card selection file '{path}': {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<string> GetLegacySelectionPaths()
    {
        var paths = new List<string>();
        void Add(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (string.Equals(path, SelectionFilePath, StringComparison.OrdinalIgnoreCase)) return;
            if (paths.Contains(path, StringComparer.OrdinalIgnoreCase)) return;
            paths.Add(path);
        }

        try { Add(Path.Combine(GetRuntimeDataDirectory(), LegacySelectionFileName)); } catch { }
        try { Add(Path.Combine(ModDirectory.Value, LegacySelectionFileName)); } catch { }
        try { Add(Path.Combine(ModDirectory.Value, "Data", "Runtime", LegacySelectionFileName)); } catch { }

        try
        {
            var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrWhiteSpace(appData))
            {
                Add(Path.Combine(appData, "SlayTheSpire2", LegacySelectionFileName));
                Add(Path.Combine(appData, "SlayTheSpire2", "CardBeautify", LegacySelectionFileName));
                Add(Path.Combine(appData, "CardBeautify", LegacySelectionFileName));
            }
        }
        catch { }

        return paths;
    }

    private static string GetRuntimeDataDirectory()
    {
        try
        {
            var userData = Godot.OS.GetUserDataDir();
            if (!string.IsNullOrWhiteSpace(userData))
                return Path.Combine(userData, "CardBeautify");
        }
        catch { }

        try
        {
            var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrWhiteSpace(appData))
                return Path.Combine(appData, "SlayTheSpire2", "CardBeautify");
        }
        catch { }

        try
        {
            if (!string.IsNullOrWhiteSpace(ModDirectory.Value))
                return Path.Combine(ModDirectory.Value, "Data", "Runtime");
        }
        catch { }

        return System.Environment.CurrentDirectory;
    }

    private static string ToSnakeCase(string value)
    {
        var sb = new StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c is ' ' or '-')
            {
                sb.Append('_');
                continue;
            }

            if (char.IsUpper(c) && i > 0 && value[i - 1] != '_' && !char.IsUpper(value[i - 1])) sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}

