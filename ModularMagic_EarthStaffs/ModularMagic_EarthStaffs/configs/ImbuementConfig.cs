using BepInEx.Configuration;

namespace ModularMagic_EarthStaffs.configs
{
    internal class ImbuementConfig
    {
        public string sectionName = "Imbuement";
        public ConfigEntry<float> EitrCost;
        public ConfigEntry<float> ProjectileAccuracy;
        public ConfigEntry<float> ProjectileBurst;
        public ConfigEntry<float> ProjectileSpeed;
        public ConfigEntry<float> SecondaryAttackEitr;

        private int entryCount = 100;

        public void GenerateConfig()
        {
            ConfigFile Config = ModularMagic_EarthStaffs.Instance.Config;

            EitrCost = Config.Bind(new ConfigDefinition(sectionName, "Eitr Cost Rune"), 0.25f,
                new ConfigDescription("The amount of eitr that is reduced per level with the Eitr Cost rune", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            ProjectileAccuracy = Config.Bind(new ConfigDefinition(sectionName, "Accuracy Rune"), 0.125f,
                new ConfigDescription("The amount of accuracy improvement per level of the Accuracy rune", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            ProjectileBurst = Config.Bind(new ConfigDefinition(sectionName, "Attack Speed Rune"), 0.0125f,
                new ConfigDescription("The amount of attack speed improvement per level of the Attack Speed rune", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            ProjectileSpeed = Config.Bind(new ConfigDefinition(sectionName, "Projectile Speed Rune"), 1f,
                new ConfigDescription("The amount of projectile speed improvement per level of the Projectile Speed rune", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            SecondaryAttackEitr = Config.Bind(new ConfigDefinition(sectionName, "Secondary Attack Rune Eitr Reduction"), 0.125f,
                new ConfigDescription("The part of the secondary attack eitr cost that is saved per level above 1 with the Nova, Rain and Summon runes (0.125 is 12.5%, so level 3 costs 75%)",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
