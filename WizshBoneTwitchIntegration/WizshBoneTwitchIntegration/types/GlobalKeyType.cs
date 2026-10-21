using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Types
{
    internal class GlobalKeyType
    {
        public static string DefeatedNothing => "DefeatedNothing";
        public static string DefeatedNoBoss => "DefeatedNoBoss";
        public static string DefeatedEikthyr => "defeated_eikthyr";
        public static string DefeatedElder => "defeated_gdking";
        public static string DefeatedBonemass => "defeated_bonemass";
        public static string DefeatedModer => "defeated_dragon";
        public static string DefeatedYagluth => "defeated_goblinking";
        public static string DefeatedQueen => "defeated_queen";
        public static string DefeatedFader => "defeated_fader";
        public static string KilledBat => "killedBat";
        public static string KilledTroll => "killedtroll";
        public static string killedSurtling => "killed_surtling";

        /// <summary>
        /// The global keys a redeem can be tied to, with a readable name matching vanilla boss naming. A small
        /// fixed list, not a runtime scan. Excludes <see cref="DefeatedNothing"/>/<see cref="DefeatedNoBoss"/>
        /// (sentinels never read by ZoneSystem, not real global keys).
        /// </summary>
        public static readonly IReadOnlyList<KeyValuePair<string, string>> Labeled = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(DefeatedEikthyr, "Defeated Eikthyr"),
            new KeyValuePair<string, string>(DefeatedElder, "Defeated the Elder"),
            new KeyValuePair<string, string>(DefeatedBonemass, "Defeated Bonemass"),
            new KeyValuePair<string, string>(DefeatedModer, "Defeated Moder"),
            new KeyValuePair<string, string>(DefeatedYagluth, "Defeated Yagluth"),
            new KeyValuePair<string, string>(DefeatedQueen, "Defeated the Queen"),
            new KeyValuePair<string, string>(DefeatedFader, "Defeated Fader"),
            new KeyValuePair<string, string>(KilledBat, "Killed a Bat"),
            new KeyValuePair<string, string>(KilledTroll, "Killed a Troll"),
            new KeyValuePair<string, string>(killedSurtling, "Killed a Surtling"),
        };
    }
}
