namespace WizshBoneTwitchIntegration.Types
{
    internal class StatusEffectType
    {
        // Custom effects
        public static string WindInBack => "WindInBack";

        // Negative effects
        public static string Burning => WizshBoneTwitchIntegration.Instance.effects.Burning.name;
        public static string Encumbered => "Encumbered";
        public static string Freezing => WizshBoneTwitchIntegration.Instance.effects.Freezing.name;
        public static string Poison => WizshBoneTwitchIntegration.Instance.effects.Poison.name;
        public static string Puke => "Puke";
        public static string Tarred => "Tared";
        public static string Wet => "Wet";

        // Positive effects
        public static string BeltStrength => "BeltStrength";
        public static string Demister => "Demister";
        public static string DvergrDamage => "SE_Dvergr_buff";
        public static string DvergrHeal => "SE_Dvergr_heal";
        public static string GoblinShamanShield => "GoblinShaman_shield";
        public static string GP_Bonemass => "GP_Bonemass";
        public static string GP_Eikthyr => "GP_Eikthyr";
        public static string GP_Fader => "GP_Fader";
        public static string GP_Moder => "GP_Moder";
        public static string GP_Queen => "GP_Queen";
        public static string GP_TheElder => "GP_TheElder";
        public static string GP_Yagluth => "GP_Yagluth";
        public static string Rested => "Rested";
        public static string SlowFall => "SlowFall";
        public static string StaffShield => "Staff_shield";
        public static string WindRun => "WindRun";
        public static string Wishbone => "Wishbone";

        // Meads: effects
        public static string AntiSting => "Potion_BugRepellent";
        public static string BarlyWine => "Potion_barleywine";
        public static string BrewOfAnimalWispers => "Potion_tamer";
        public static string Bzerker => "Potion_bzerker";
        public static string FrostResist => "Potion_frostresist";
        public static string LightFoot => "Potion_LightFoot";
        public static string LovePotion => "Potion_TrollPheromones";
        public static string PoisonResist => "Potion_poisonresist";
        public static string Ratatosk => "Potion_hasty";
        public static string TrollStrength => "Potion_strength";
        public static string Vananidir => "Potion_swimmer";

        // Meads: resources
        public static string EitrMinor => "Potion_eitr_minor";
        public static string EitrLingering => "Potion_eitr_lingering";
        public static string HealingMinor => "Potion_health_minor";
        public static string HealingMedium => "Potion_health_medium";
        public static string HealingMajor => "Potion_health_major";
        public static string HealingLingering => "Potion_health_lingering";
        public static string StaminaMinor => "Potion_stamina_minor";
        public static string StaminaMedium => "Potion_stamina_medium";
        public static string StaminaLingering => "Potion_stamina_lingering";

        //public static int PlayerShrink => 1043640966;
        public static string PlayerShrink => "PlayerShrink";
        //public static int PlayerGrow => 822944700;
        public static string PlayerGrow => "PlayerGrow";
    }
}
