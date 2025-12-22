namespace WizshBoneTwitchIntegration.Types
{
    internal class StatusEffectType
    {
        // Negative effects
        public static int Burning => WizshBoneTwitchIntegration.Instance.effects.Burning.NameHash();
        public static int Freezing => WizshBoneTwitchIntegration.Instance.effects.Freezing.NameHash();
        public static int Poison => WizshBoneTwitchIntegration.Instance.effects.Poison.NameHash();
        public static int Puke => -629027225;
        public static int Tarred => -1779147092;
        public static int Wet => -1273337594;

        // Positive effects
        public static int Rested => -2079273775;

        // Meads: effects
        public static int AntiSting => -1157133715;
        public static int BarlyWine => 1458612846;
        public static int BrewOfAnimalWispers => -1944522317;
        public static int Bzerker => -1325774533;
        public static int FrostResist => -1768438774;
        public static int LightFoot => 2062111878;
        public static int LovePotion => -716013329;
        public static int PoisonResist => -568360536;
        public static int Ratatosk => 1965486703;
        public static int TrollStrength => 370641789;
        public static int Vananidir => -1907265002;

        // Meads: resources
        public static int EitrMinor => 1437258388;
        public static int EitrLingering => -1996024552;
        public static int HealingMinor => -590058386;
        public static int HealingMedium => -67041294;
        public static int HealingMajor => 1251702474;
        public static int HealingLingering => -414643894;
        public static int StaminaMinor => 899527613;
        public static int StaminaMedium => 685847919;
        public static int StaminaLingering => 1930415553;

        public static int GetByString(string name)
        {
            switch (name.ToLower())
            {
                case "burning": return Burning;
                case "freezing": return Freezing;
                case "poison": return Poison;
                case "puke": return Puke;
                case "tarred": return Tarred;
                case "wet": return Wet;

                case "rested": return Rested;

                case "antisting": return AntiSting;
                case "barlywine": return BarlyWine;
                case "brewofanimalwispers": return BrewOfAnimalWispers;
                case "bzerker": return Bzerker;
                case "frostresist": return FrostResist;
                case "lightfoot": return LightFoot;
                case "lovepotion": return LovePotion;
                case "poisonresist": return PoisonResist;
                case "ratatosk": return Ratatosk;
                case "trollstrength": return TrollStrength;
                case "vananidir": return Vananidir;

                case "eitrminor": return EitrMinor;
                case "eitrlingering": return EitrLingering;
                case "healingminor": return HealingMinor;
                case "healingmedium": return HealingMedium;
                case "healingmajor": return HealingMajor;
                case "healinglingering": return HealingLingering;
                case "staminaminor": return StaminaMinor;
                case "staminamedium": return StaminaMedium;
                case "staminalingering": return StaminaLingering;
                default: return -1;
            }
        }
    }
}
