using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// One entry in <see cref="RedeemEffectCatalog"/>: a redeem type's display label and a
    /// one-line description of what it does in-game.
    /// </summary>
    internal readonly struct RedeemEffectInfo
    {
        public readonly string Type;
        public readonly string Label;
        public readonly string Description;

        public RedeemEffectInfo(string type, string label, string description)
        {
            Type = type;
            Label = label;
            Description = description;
        }
    }

    /// <summary>
    /// Single source of truth for the 17 selectable redeem effect types' display labels and
    /// descriptions - shared by <see cref="RedeemWizard"/>'s step-1 effect picker and
    /// <see cref="Tabs.RedeemsTab"/>'s list-row type-hover tooltip, rather than duplicating this
    /// text in both places. Order/membership matches GUI_OLD/tabs/RedeemsTab.cs's RedeemTypes
    /// array minus RedeemType.Undefined (that one has no redeem-facing meaning, so it's never a
    /// choosable effect) and minus RedeemType.SpawnAbility (the raw/generic type - only its 8
    /// curated sub-types, e.g. Smite/Door/Windmill, are choosable here; SpawnAbility itself stays
    /// editable only via GUI_OLD/F4 for existing redeems).
    /// </summary>
    internal static class RedeemEffectCatalog
    {
        public static readonly List<RedeemEffectInfo> All = new List<RedeemEffectInfo>
        {
            new RedeemEffectInfo(RedeemType.Detonate, "Detonate",
                "Spawns an explosive charge on the streamer that detonates after a short fuse, dealing area damage."),
            new RedeemEffectInfo(RedeemType.Flashbang, "Flashbang",
                "Triggers a blinding flash and loud bang near the streamer, disorienting them briefly."),
            new RedeemEffectInfo(RedeemType.Mist, "Mist",
                "Spawns a lingering mist cloud around the streamer."),
            new RedeemEffectInfo(RedeemType.Door, "Doors of Doom",
                "Spawns a stack of doors around the streamer, boxing them in until they break out."),
            new RedeemEffectInfo(RedeemType.Windmill, "Windmills of Death",
                "Spawns spinning windmill blades around the streamer."),
            new RedeemEffectInfo(RedeemType.Smite, "Smite",
                "Calls down a lightning strike on the streamer."),
            new RedeemEffectInfo(RedeemType.Rain, "Rain",
                "Drops a damaging rain effect around the streamer."),
            new RedeemEffectInfo(RedeemType.LogRain, "Log Rain",
                "Drops a shower of logs from the sky above the streamer."),
            new RedeemEffectInfo(RedeemType.Meteor, "Meteors",
                "Calls down a meteor strike near the streamer's position."),
            new RedeemEffectInfo(RedeemType.Trap, "Trap",
                "Spawns a hidden trap near the streamer."),
            new RedeemEffectInfo(RedeemType.Root, "Roots",
                "Entangles the streamer in roots, temporarily immobilizing them."),
            new RedeemEffectInfo(RedeemType.SpawnCreature, "Spawn Creature",
                "Spawns one or more creatures from a chosen group near the streamer."),
            new RedeemEffectInfo(RedeemType.StatusEffect, "Status Effect",
                "Applies a status effect to the streamer."),
            new RedeemEffectInfo(RedeemType.SurpriseChest, "Surprise Chest",
                "Spawns a chest near the streamer with random loot."),
            new RedeemEffectInfo(RedeemType.TerrainEdit, "Terrain Edit",
                "Raises, lowers, or otherwise edits the terrain around the streamer."),
            new RedeemEffectInfo(RedeemType.TimeStop, "Time Stop",
                "Temporarily freezes creatures and objects in place around the streamer."),
            new RedeemEffectInfo(RedeemType.Weather, "Weather",
                "Changes the weather around the streamer."),
        };

        private static readonly Dictionary<string, RedeemEffectInfo> ByType = BuildLookup();

        private static Dictionary<string, RedeemEffectInfo> BuildLookup()
        {
            var lookup = new Dictionary<string, RedeemEffectInfo>();
            foreach (RedeemEffectInfo info in All)
                lookup[info.Type] = info;
            return lookup;
        }

        /// <summary>
        /// Returns the label for <paramref name="type"/>, or the raw type string itself if it's
        /// not one of the 18 catalog entries (e.g. an older/unrecognized value read from disk).
        /// </summary>
        public static string LabelFor(string type)
        {
            return ByType.TryGetValue(type ?? "", out RedeemEffectInfo info) ? info.Label : type;
        }

        /// <summary>
        /// Returns the description for <paramref name="type"/>, or an empty string if it's not a
        /// catalog entry.
        /// </summary>
        public static string DescriptionFor(string type)
        {
            return ByType.TryGetValue(type ?? "", out RedeemEffectInfo info) ? info.Description : "";
        }
    }
}
