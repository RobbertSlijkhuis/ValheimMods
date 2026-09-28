using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jotunn.Managers;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Every live-scanned option list the redeem wizard's step-2 forms need, built eagerly once at
    /// a safe lifecycle moment instead of lazily on first GUI access - GUI_OLD/editors/
    /// FieldUIBuilder.cs's per-call-site lazy-cache pattern (scan on first access, guarded only by
    /// a null check) is deliberately not reused here, since opening the GUI before the scene/world
    /// is fully populated could permanently cache an incomplete or empty result.
    ///
    /// Prefab-registry scans (<see cref="CreaturePrefabs"/>/<see cref="DoorPrefabs"/>/
    /// <see cref="PlaceablePieces"/>/<see cref="LogPrefabs"/>/<see cref="StatusEffects"/>) populate
    /// via <see cref="BuildPrefabCatalogs"/>, hooked to <c>PrefabManager.OnPrefabsRegistered</c> in
    /// <c>WizshBoneTwitchIntegration.Awake()</c> - the same hook <c>AddPersistentComponents</c>
    /// already relies on for the same reason: <c>ZNetScene.instance</c> isn't populated yet at the
    /// earlier <c>OnVanillaPrefabsAvailable</c> event, only by the time prefabs are fully
    /// registered. <see cref="WeatherNames"/> needs an actual loaded world (<see cref="EnvMan"/>
    /// doesn't exist until then either), so it populates separately via
    /// <see cref="BuildWeatherCatalogOnce"/>, hooked to the first <c>Player.OnSpawned</c> - see
    /// <c>harmony/RedeemCatalogPatchesWBTI.cs</c>.
    ///
    /// Every widget (SearchableDropdown/SearchableChecklist instance) should read from these
    /// pre-built static lists rather than scanning on its own.
    /// </summary>
    internal static class RedeemPrefabCatalog
    {
        public static List<DropdownOption> CreaturePrefabs { get; private set; } = new List<DropdownOption>();
        public static List<DropdownOption> DoorPrefabs { get; private set; } = new List<DropdownOption>();
        public static List<DropdownOption> PlaceablePieces { get; private set; } = new List<DropdownOption>();
        public static List<string> LogPrefabs { get; private set; } = new List<string>();
        public static List<DropdownOption> StatusEffects { get; private set; } = new List<DropdownOption>();
        public static List<DropdownOption> WeatherNames { get; private set; } = new List<DropdownOption>();

        private static bool s_weatherBuilt;

        // Log prefabs have no shared identifying component to scan for (unlike Door/Humanoid+
        // MonsterAI), so this is a curated candidate list - ported as-is from GUI_OLD/editors/
        // FieldUIBuilder.cs's own LogPrefabCandidates - filtered to prefabs that actually resolve.
        private static readonly string[] LogPrefabCandidates =
        {
            "beech_log_half", "PineTree_log_half", "SwampTree1_log", "FirTree_log_half",
            "Birch_log_half", "yggashoot_log_half", "AshlandsTreeLogHalf2", "Oak_log_half"
        };

        /// <summary>
        /// Selected value in a log-prefab checklist meaning "replace the selection with the 8
        /// default per-biome logs and switch to biome-based selection" - ported from GUI_OLD's
        /// FieldUIBuilder.BiomeSpecificSentinel, used by LogRainForm.
        /// </summary>
        public const string BiomeSpecificSentinel = "Biome specific (sets 8 default per-biome logs)";

        /// <summary>The 8 per-biome logs LogRainView.OnBiomeSpecificSelected bulk-assigns - same order/values.</summary>
        public static readonly string[] DefaultBiomeLogs =
        {
            "beech_log_half", "PineTree_log_half", "SwampTree1_log", "FirTree_log_half",
            "Birch_log_half", "yggashoot_log_half", "AshlandsTreeLogHalf2", "Oak_log_half"
        };

        private static readonly List<string> FallbackWeatherNames = new List<string>
        {
            "Clear", "Twilight_Clear", "Misty", "Darklands_dark", "Heath clear",
            "DeepForest Mist", "GDKing", "Rain", "LightRain", "ThunderStorm",
            "Eikthyr", "GoblinKing", "nofogts", "SwampRain", "Bonemass",
            "Snow", "Twilight_Snow", "Twilight_SnowStorm", "SnowStorm",
            "Moder", "Ashrain", "Crypt", "SunkenCrypt"
        };

        /// <summary>
        /// Hook target for <c>PrefabManager.OnPrefabsRegistered</c> - builds every
        /// prefab-registry-backed catalog once, then unsubscribes itself (same
        /// subscribe-once/unsubscribe-in-handler pattern <c>WizshBoneTwitchIntegration.SetupPieces</c>/
        /// <c>AddEffectLists</c> use for the earlier <c>OnVanillaPrefabsAvailable</c> event).
        /// </summary>
        public static void BuildPrefabCatalogs()
        {
            PrefabManager.OnPrefabsRegistered -= BuildPrefabCatalogs;

            if (ZNetScene.instance == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] RedeemPrefabCatalog: ZNetScene not ready at OnPrefabsRegistered, prefab catalogs will stay empty.");
                return;
            }

            HashSet<string> placeableNames = GetPlaceablePrefabNames();

            var creatures = new List<DropdownOption>();
            var doors = new List<DropdownOption>();
            var pieces = new List<DropdownOption>();

            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                if (prefab == null)
                    continue;

                Humanoid humanoid = prefab.GetComponent<Humanoid>();
                MonsterAI monsterAI = prefab.GetComponent<MonsterAI>();
                if (humanoid != null && monsterAI != null)
                {
                    string creatureLabel = !string.IsNullOrEmpty(humanoid.m_name)
                        ? CreatureHelper.StripColorTags(Localization.instance.Localize(humanoid.m_name))
                        : name;
                    creatures.Add(new DropdownOption(name, creatureLabel));
                }

                if (placeableNames.Contains(name))
                {
                    Piece piece = prefab.GetComponent<Piece>();
                    string pieceLabel = piece != null && !string.IsNullOrEmpty(piece.m_name)
                        ? Localization.instance.Localize(piece.m_name)
                        : name;
                    pieces.Add(new DropdownOption(name, pieceLabel));

                    if (prefab.GetComponent<Door>() != null)
                        doors.Add(new DropdownOption(name, pieceLabel));
                }
            }

            creatures.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            doors.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            pieces.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));

            CreaturePrefabs = creatures;
            DoorPrefabs = doors;
            PlaceablePieces = pieces;
            LogPrefabs = LogPrefabCandidates.Where(name => PrefabManager.Instance.GetPrefab(name) != null).ToList();
            StatusEffects = BuildStatusEffectOptions();

            Jotunn.Logger.LogWarning($"[WBTI] RedeemPrefabCatalog built: {creatures.Count} creatures, {doors.Count} doors, {pieces.Count} placeable pieces, {LogPrefabs.Count} log prefabs, {StatusEffects.Count} status effects.");
        }

        /// <summary>
        /// Hook target for the first <c>Player.OnSpawned</c> - builds the weather-names catalog,
        /// which needs an actual loaded world (<see cref="EnvMan"/> only exists in-session, unlike
        /// prefab registration which is already available at the main menu). Guarded so repeat
        /// respawns don't rescan.
        /// </summary>
        public static void BuildWeatherCatalogOnce()
        {
            if (s_weatherBuilt)
                return;

            List<string> names;
            try
            {
                if (EnvMan.instance == null || EnvMan.instance.m_environments == null || EnvMan.instance.m_environments.Count == 0)
                {
                    names = FallbackWeatherNames;
                }
                else
                {
                    names = EnvMan.instance.m_environments
                        .Select(env => env.m_name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] RedeemPrefabCatalog: weather scan failed, using fallback list: {e}");
                names = FallbackWeatherNames;
            }

            WeatherNames = names.Select(n => new DropdownOption(n, n)).ToList();
            s_weatherBuilt = true;
            Jotunn.Logger.LogWarning($"[WBTI] RedeemPrefabCatalog: weather catalog built with {WeatherNames.Count} entries.");
        }

        /// <summary>
        /// Every prefab reachable from a build tool's piece list (Hammer, Cultivator, Hoe, artisan
        /// tables, ...) - ported from GUI_OLD/editors/FieldUIBuilder.cs's GetPlaceablePrefabNames.
        /// </summary>
        private static HashSet<string> GetPlaceablePrefabNames()
        {
            var names = new HashSet<string>();
            foreach (string name in ZNetScene.instance.GetPrefabNames())
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                PieceTable pieceTable = prefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces;
                if (pieceTable == null)
                    continue;

                foreach (GameObject piece in pieceTable.m_pieces)
                {
                    if (piece != null)
                        names.Add(piece.name);
                }
            }
            return names;
        }

        private static List<DropdownOption> BuildStatusEffectOptions()
        {
            if (ObjectDB.instance == null)
                return new List<DropdownOption>();

            var names = new List<string>();
            foreach (PropertyInfo prop in typeof(Types.StatusEffectType).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (prop.PropertyType != typeof(string))
                    continue;

                try
                {
                    string value = prop.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(value))
                        names.Add(value);
                }
                catch
                {
                    // Skip properties whose backing assets aren't loaded yet.
                }
            }

            var displayNames = new Dictionary<string, string>();
            foreach (StatusEffect se in ObjectDB.instance.m_StatusEffects)
            {
                if (se != null && !string.IsNullOrEmpty(se.m_name))
                    displayNames[se.name] = Localization.instance.Localize(se.m_name);
            }

            var options = names
                .Select(name => new DropdownOption(name, displayNames.TryGetValue(name, out string label) ? label : name))
                .ToList();
            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            return options;
        }

        /// <summary>
        /// Resolves a creature prefab's display label from <see cref="CreaturePrefabs"/>, falling
        /// back to the raw prefab name if it's not (or not yet) in the catalog - used by
        /// SpawnCreatureForm's entry-list row labels.
        /// </summary>
        public static string GetCreatureDisplayName(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return prefabName;

            foreach (DropdownOption option in CreaturePrefabs)
            {
                if (option.Value == prefabName)
                    return option.Label;
            }
            return prefabName;
        }

        /// <summary>
        /// Real status-effect TTL for <paramref name="name"/> (falls back to 10s if unknown/not yet
        /// loaded) - used by StatusEffectForm to auto-fill an entry's Duration when Name changes.
        /// </summary>
        public static float LookupStatusEffectTTL(string name)
        {
            if (ObjectDB.instance == null || string.IsNullOrEmpty(name))
                return 10f;

            foreach (StatusEffect se in ObjectDB.instance.m_StatusEffects)
            {
                if (se != null && se.name == name)
                    return se.m_ttl > 0f ? se.m_ttl : 10f;
            }
            return 10f;
        }

        /// <summary>
        /// Returns <paramref name="options"/> with <paramref name="currentValue"/> added in
        /// (sorted) if it's missing, so a saved value the live scan doesn't currently produce (a
        /// renamed/removed prefab, or a value from another mod) still displays instead of silently
        /// disappearing. Ported from GUI_OLD/editors/FieldUIBuilder.cs's method of the same name.
        /// </summary>
        public static List<DropdownOption> EnsureIncludesCurrentValue(List<DropdownOption> options, string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue) || options.Any(o => o.Value == currentValue))
                return options;

            var withCurrent = new List<DropdownOption>(options) { new DropdownOption(currentValue, currentValue) };
            withCurrent.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            return withCurrent;
        }
    }
}
