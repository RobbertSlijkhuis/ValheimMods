using BepInEx.Configuration;
using ModularMagic_FireStaffs.Helpers;
using ModularMagic_FireStaffs.Models;

namespace ModularMagic_FireStaffs.Configs
{
    /// <summary>
    /// The config of the cone main attack. The attack of the wrapper prefab replaces the main attack of the staff on equip.
    /// The spawned AOE uses the damage of the staff, scaled by the damage multiplier of the attack. The attack force of the
    /// cone is 0, set by the force multiplier of the wrapper attack.
    /// </summary>
    internal class MainAttackConfig
    {
        public ConfigEntry<float> eitrMultiplier;
        public ConfigEntry<float> burstInterval;
        public ConfigEntry<float> damageMultiplier;

        private int entryCount = 100;

        public void GenerateConfig(MainAttackConfigOptions options)
        {
            ConfigFile config = ModularMagic_FireStaffs.Instance.Config;

            // Read when the runes are applied to the staff, the held staff is updated by the SettingChanged hook of the plugin
            eitrMultiplier = config.Bind(new ConfigDefinition(options.sectionName, "Eitr cost multiplier"), options.eitrMultiplier,
                new ConfigDescription("The eitr cost of every burst of the cone as a multiplier of the eitr cost of the staff's attack. This replaces the eitr cost of the staff while the cone rune is on it (0.1 is a tenth, so a staff of 20 eitr costs 2 per burst)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            burstInterval = config.Bind(new ConfigDefinition(options.sectionName, "Burst interval"), options.burstInterval,
                new ConfigDescription("The time in seconds between two bursts of the cone while the attack is held", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            burstInterval.SettingChanged += (obj, attr) => UpdateAttack(options);

            damageMultiplier = config.Bind(new ConfigDefinition(options.sectionName, "Damage multiplier"), options.damageMultiplier,
                new ConfigDescription("The damage of every burst of the cone as a multiplier of the damage of the staff (fire and blunt, including quality). 1 is the damage of the staff, 0.1 is a tenth",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageMultiplier.SettingChanged += (obj, attr) => UpdateAttack(options);

            UpdateAttack(options);
        }

        // The attack of the wrapper prefab is copied to the staff on equip, so these are the base burst interval and damage multiplier of the cone
        private void UpdateAttack(MainAttackConfigOptions options)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                burstInterval = burstInterval.Value,
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
