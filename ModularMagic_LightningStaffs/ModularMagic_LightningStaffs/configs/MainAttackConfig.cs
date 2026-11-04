using BepInEx.Configuration;
using ModularMagic_LightningStaffs.Helpers;
using ModularMagic_LightningStaffs.Models;

namespace ModularMagic_LightningStaffs.Configs
{
    /// <summary>
    /// The config of the cone main attack. The attack of the wrapper prefab replaces the main attack of the staff on equip,
    /// and the spawned AOE has its own damage, so these values do not depend on the stats of the staff.
    /// </summary>
    internal class MainAttackConfig
    {
        public ConfigEntry<int> useEitr;
        public ConfigEntry<float> burstInterval;
        public ConfigEntry<float> damageLightning;
        public ConfigEntry<float> attackForce;

        private int entryCount = 100;

        public void GenerateConfig(MainAttackConfigOptions options)
        {
            ConfigFile config = ModularMagic_LightningStaffs.Instance.Config;

            // Read when the runes are applied to the staff, the held staff is updated by the SettingChanged hook of the plugin
            useEitr = config.Bind(new ConfigDefinition(options.sectionName, "Main attack eitr cost per burst"), options.useEitr,
                new ConfigDescription("The eitr cost of every burst of the cone. This replaces the eitr cost of the staff while the cone rune is on it", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            burstInterval = config.Bind(new ConfigDefinition(options.sectionName, "Burst interval"), options.burstInterval,
                new ConfigDescription("The time in seconds between two bursts of the cone while the attack is held", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            burstInterval.SettingChanged += (obj, attr) => UpdateBurstInterval(options);

            damageLightning = config.Bind(new ConfigDefinition(options.sectionName, "Lightning damage"), options.damageLightning,
                new ConfigDescription("The lightning damage of the cone (not visible ingame)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageLightning.SettingChanged += (obj, attr) => UpdateHelper.UpdateAoe(options.aoePrefab, new UpdateAoeOptions() { damageLightning = damageLightning.Value });

            attackForce = config.Bind(new ConfigDefinition(options.sectionName, "Attack force"), options.attackForce,
                new ConfigDescription("The attack force of the cone applied to targets hit", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            attackForce.SettingChanged += (obj, attr) => UpdateHelper.UpdateAoe(options.aoePrefab, new UpdateAoeOptions() { attackForce = attackForce.Value });

            UpdateBurstInterval(options);
        }

        // The attack of the wrapper prefab is copied to the staff on equip, so this is the base burst interval of the cone
        private void UpdateBurstInterval(MainAttackConfigOptions options)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                burstInterval = burstInterval.Value,
            });
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
