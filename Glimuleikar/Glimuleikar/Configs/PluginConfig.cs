using BepInEx.Configuration;
using WizshBoneTwitchIntegration.Harmony;

namespace Glimuleikar.Configs
{
    internal static class PluginConfig
    {
        public static string sectionDamage = "Damage";
        public static string sectionHarpoon = "Harpoon";

        public static ConfigEntry<bool> configDamageStructuresEnable;
        public static ConfigEntry<bool> configDamagePlayersEnable;
        public static ConfigEntry<bool> configDamageFallEnable;
        public static ConfigEntry<bool> configDamageMonstersEnable;
        public static ConfigEntry<bool> configDamageTamedLoxEnable;
        public static ConfigEntry<float> configHarpoonMaxRopeLength;
        public static ConfigEntry<float> configHarpoonPullDuration;

        // Other
        private static int entryCount = 1000;

        public static void Init()
        {
            InitGeneralConfig();
            InitSettingChangedHandlers();
        }

        public static void InitGeneralConfig()
        {
            configDamageStructuresEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable damage to structures", true,
                new ConfigDescription("Whether the damage to structures is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configDamagePlayersEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable player damage", true,
                new ConfigDescription("Whether the damage from players is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configDamageFallEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable fall damage", true,
                new ConfigDescription("Whether fall damage is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configDamageMonstersEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable monster damage", true,
                new ConfigDescription("Whether the damage from monsters is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configDamageTamedLoxEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable tamed Lox damage to players", false,
                new ConfigDescription("Whether tamed Lox can damage players by running into them", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configHarpoonMaxRopeLength = Glimuleikar.Instance.Config.Bind(sectionHarpoon, "Max rope length", 10f,
                new ConfigDescription("Maximum harpoon rope length in units. Targets hit from further away will be pulled in to this distance.", new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configHarpoonPullDuration = Glimuleikar.Instance.Config.Bind(sectionHarpoon, "Pull duration", 0.5f,
                new ConfigDescription("Time in seconds to pull the target to the max rope length.", new AcceptableValueRange<float>(0.1f, 5f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

        }

        private static void InitSettingChangedHandlers()
        {
            configDamageTamedLoxEnable.SettingChanged += (sender, e) => LoxPatchesWBTI.OnTamedLoxDamageSettingChanged();
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
