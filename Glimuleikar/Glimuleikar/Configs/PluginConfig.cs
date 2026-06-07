using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using System;
using System.IO;
using WizshBoneTwitchIntegration.Harmony;

namespace Glimuleikar.Configs
{
    internal static class PluginConfig
    {
        public static string sectionDamage = "Damage";

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
        }

        public static void InitGeneralConfig()
        {
            configDamageStructuresEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable damage to structures", true,
                new ConfigDescription("Whether the damage to structures is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configDamageStructuresEnable.SettingChanged += (sender, e) =>
            {
                Jotunn.Logger.LogWarning("Structure damage setting changed to: " + configDamageStructuresEnable.Value);
            };

            configDamagePlayersEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable player damage", true,
                new ConfigDescription("Whether the damage from players is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configDamagePlayersEnable.SettingChanged += (sender, e) =>
            {
                Jotunn.Logger.LogWarning("Player damage setting changed to: " + configDamagePlayersEnable.Value);
            };

            configDamageFallEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable fall damage", true,
                new ConfigDescription("Whether fall damage is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configDamageFallEnable.SettingChanged += (sender, e) =>
            {
                Jotunn.Logger.LogWarning("Fall damage setting changed to: " + configDamageFallEnable.Value);
            };

            configDamageMonstersEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable monster damage", true,
                new ConfigDescription("Whether the damage from monsters is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configDamageMonstersEnable.SettingChanged += (sender, e) =>
                {
                    Jotunn.Logger.LogWarning("Monster damage setting changed to: " + configDamageMonstersEnable.Value);
                };

            configDamageTamedLoxEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable tamed Lox damage to players", false,
                new ConfigDescription("Whether tamed Lox can damage players by running into them", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configDamageTamedLoxEnable.SettingChanged += (sender, e) =>
            {
                Jotunn.Logger.LogWarning("Tamed Lox damage setting changed to: " + configDamageTamedLoxEnable.Value);
                LoxPatchesWBTI.OnTamedLoxDamageSettingChanged();
            };

            FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, Glimuleikar.configFileName);
            configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
            configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
            configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
            configWatcher.IncludeSubdirectories = true;
            configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            configWatcher.EnableRaisingEvents = true;

            SynchronizationManager.OnConfigurationSynchronized += (obj, attr) =>
            {
                if (attr.InitialSynchronization)
                {
                    Jotunn.Logger.LogWarning("Initial Config sync event received");
                }
                else
                {
                    Jotunn.Logger.LogWarning("Config sync event received");
                }
            };
        }

        /**
         * Event handler for when the config file changes
         */
        private static void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (!File.Exists(Glimuleikar.configFileFullPath))
                    return;

                Glimuleikar.Instance.Config.Reload();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
            }
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
