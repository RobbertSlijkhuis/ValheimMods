using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Types
{
    internal class RedeemType
    {
        public static string Detonate => "Detonate";
        public static string Flashbang => "Flashbang";
        public static string SpawnCreature => "SpawnCreature";
        public static string Mist => "Mist";
        public static string SpawnAbility => "SpawnAbility";
        public static string StatusEffect => "StatusEffect";
        public static string SurpriseChest => "SurpriseChest";
        public static string TerrainEdit => "TerrainEdit";
        public static string Undefined => "Undefined";
        public static string TimeStop => "TimeStop";
        public static string Weather => "Weather";
        public static string Door => "Door";
        public static string Windmill => "Windmill";
        public static string Smite => "Smite";
        public static string Rain => "Rain";
        public static string Meteor => "Meteor";
        public static string Trap => "Trap";
        public static string Root => "Root";

        // SpawnAbility plus the simplified per-effect types - all of these share the
        // same underlying SpawnAbilityData and SpawnAbilityHelper runtime handler.
        public static readonly HashSet<string> SpawnAbilityFamily = new HashSet<string>
        {
            SpawnAbility, Door, Windmill, Smite, Rain, Meteor, Trap, Root,
        };
    }
}
