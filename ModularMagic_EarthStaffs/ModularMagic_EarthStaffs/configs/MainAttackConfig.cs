using BepInEx.Configuration;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;

namespace ModularMagic_EarthStaffs.Configs
{
    /// <summary>
    /// The config of the cone main attack. The attack of the wrapper prefab replaces the main attack of the staff on equip.
    /// The hit of a burst uses the damage of the staff, scaled by the damage multiplier of the attack (see MMAoe). The attack force
    /// of the cone is 0, set by the force multiplier of the wrapper attack.
    /// </summary>
    internal class MainAttackConfig
    {
        public ConfigEntry<float> eitrMultiplier;
        public ConfigEntry<float> burstInterval;
        public ConfigEntry<float> damageMultiplier;

        private int entryCount = 100;

        public void GenerateConfig(MainAttackConfigOptions options)
        {
            ConfigFile config = ModularMagic_EarthStaffs.Instance.Config;

            // Read when the runes are applied to the staff, the held staff is updated by the SettingChanged hook of the plugin
            eitrMultiplier = config.Bind(new ConfigDefinition(options.sectionName, "Eitr cost multiplier"), options.eitrMultiplier,
                new ConfigDescription("The eitr cost of every burst of the cone as a multiplier of the eitr cost of the staff's attack. This replaces the eitr cost of the staff while the cone rune is on it (0.5 is half, so a staff of 6 eitr costs 3 per burst)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            // Read when the runes are applied to the staff
            burstInterval = config.Bind(new ConfigDefinition(options.sectionName, "Burst interval"), options.burstInterval,
                new ConfigDescription("The time in seconds between two bursts of the cone while the attack is held", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            damageMultiplier = config.Bind(new ConfigDefinition(options.sectionName, "Damage multiplier"), options.damageMultiplier,
                new ConfigDescription("The poison damage and the spirit damage of every burst of the cone, each as a multiplier of the main damage of the staff (blunt, or pierce or slash when a rune changed it, including quality). 1 is the main damage of the staff, 0.2 is a fifth",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageMultiplier.SettingChanged += (obj, attr) => UpdateAttack(options);

            UpdateAttack(options);
        }

        // The attack of the wrapper prefab is copied to the staff on equip, so this is the base damage multiplier of the cone
        private void UpdateAttack(MainAttackConfigOptions options)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                damageMultiplier = damageMultiplier.Value,
            });
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
