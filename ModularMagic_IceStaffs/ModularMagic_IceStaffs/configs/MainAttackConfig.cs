using BepInEx.Configuration;
using ModularMagic_IceStaffs.Helpers;
using ModularMagic_IceStaffs.Models;

namespace ModularMagic_IceStaffs.configs
{
    /// <summary>
    /// The config of the cone main attack. The attack of the wrapper prefab replaces the main attack of the staff on equip,
    /// and the spawned AOE has its own damage, so these values do not depend on the stats of the staff.
    /// </summary>
    internal class MainAttackConfig
    {
        public ConfigEntry<int> useEitr;
        public ConfigEntry<float> burstInterval;
        public ConfigEntry<float> damageFrost;
        public ConfigEntry<float> attackForce;

        private int entryCount = 100;

        public void GenerateConfig(MainAttackConfigOptions options)
        {
            ConfigFile config = ModularMagic_IceStaffs.Instance.Config;

            // Read when the runes are applied to the staff, the held staff is updated by the SettingChanged hook of the plugin
            useEitr = config.Bind(new ConfigDefinition(options.sectionName, "Main attack eitr cost per burst"), options.useEitr,
                new ConfigDescription("The eitr cost of every burst of the cone. This replaces the eitr cost of the staff while the cone rune is on it", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            // Read when the runes are applied to the staff, so the Attack Speed rune is taken off this value
            burstInterval = config.Bind(new ConfigDefinition(options.sectionName, "Burst interval"), options.burstInterval,
                new ConfigDescription("The time in seconds between two bursts of the cone while the attack is held", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            damageFrost = config.Bind(new ConfigDefinition(options.sectionName, "Frost damage"), options.damageFrost,
                new ConfigDescription("The frost damage of the cone (not visible ingame)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            damageFrost.SettingChanged += (obj, attr) => UpdateHelper.UpdateAoe(options.aoePrefab, new UpdateAoeOptions() { damageFrost = damageFrost.Value });

            attackForce = config.Bind(new ConfigDefinition(options.sectionName, "Attack force"), options.attackForce,
                new ConfigDescription("The attack force of the cone applied to targets hit", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            attackForce.SettingChanged += (obj, attr) => UpdateHelper.UpdateAoe(options.aoePrefab, new UpdateAoeOptions() { attackForce = attackForce.Value });
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
