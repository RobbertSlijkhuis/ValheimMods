using BepInEx.Configuration;
using ModularMagic_Core.Models;

namespace ModularMagic_Core.Configs
{
    /// <summary>
    /// The config of the values of the runes of a staff mod: how much each level of a rune changes. The attack speed rune has one of two
    /// entries, depending on whether it speeds up the attack animation or lowers the burst interval of the attack.
    /// </summary>
    public class ImbuementConfig
    {
        public string sectionName = "Imbuement";
        public ConfigEntry<float> EitrCost;
        public ConfigEntry<float> ProjectileAccuracy;
        // Only bound when the Attack Speed rune speeds up the attack animation (ImbuementConfigOptions.attackSpeedAnimation)
        public ConfigEntry<float> AttackSpeedAnimation;
        // Only bound when the Attack Speed rune lowers the burst interval of the attack
        public ConfigEntry<float> AttackSpeed;
        public ConfigEntry<float> ProjectileSpeed;
        public ConfigEntry<float> SecondaryAttackEitr;

        private int entryCount = 100;

        public void GenerateConfig(ConfigFile config, ImbuementConfigOptions options)
        {
            sectionName = options.sectionName;

            EitrCost = config.Bind(new ConfigDefinition(sectionName, "Eitr Cost Rune"), options.eitrCost,
                new ConfigDescription("The amount of eitr that is reduced per level with the Eitr Cost rune", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            ProjectileAccuracy = config.Bind(new ConfigDefinition(sectionName, "Accuracy Rune"), 0.125f,
                new ConfigDescription("The amount of accuracy improvement per level of the Accuracy rune", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            if (options.attackSpeedAnimation)
            {
                AttackSpeedAnimation = config.Bind(new ConfigDefinition(sectionName, "Attack Speed Rune Animation Speed"), 0.05f,
                    new ConfigDescription("The part the attack animation of the normal attack is faster per level of the Attack Speed rune (0.05 is 5%, so level 4 is 20% faster). Only used when the staff does not have the Cone rune",
                    new AcceptableValueRange<float>(0f, 0.25f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }
            else
            {
                AttackSpeed = config.Bind(new ConfigDefinition(sectionName, "Attack Speed Rune"), 0.0125f,
                    new ConfigDescription("The amount of attack speed improvement per level of the Attack Speed rune", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }

            ProjectileSpeed = config.Bind(new ConfigDefinition(sectionName, "Projectile Speed Rune"), 1f,
                new ConfigDescription("The amount of projectile speed improvement per level of the Projectile Speed rune", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            SecondaryAttackEitr = config.Bind(new ConfigDefinition(sectionName, "Secondary Attack Rune Eitr Reduction"), 0.125f,
                new ConfigDescription($"The part of the secondary attack eitr cost that is saved per level above 1 with the {options.secondaryAttackRunes} runes (0.125 is 12.5%, so level 3 costs 75%)",
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
