using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using UnityEngine;
using WizshBoneTwitchIntegration.Harmony;

namespace Glimuleikar.Configs
{
    internal static class PluginConfig
    {
        public static string sectionDamage = "Damage";
        public static string sectionLox = "Lox";
        public static string sectionBoat = "Boat";

        public static ConfigEntry<bool> configDamageStructuresEnable;
        public static ConfigEntry<bool> configDamagePlayersEnable;
        public static ConfigEntry<bool> configDamageFallEnable;
        public static ConfigEntry<bool> configDamageMonstersEnable;

        public static ConfigEntry<bool> configTamedLoxDamageFriendliesEnable;
        public static ConfigEntry<float> configTamedLoxSpeedThreshold;
        public static ConfigEntry<float> configTamedLoxTurningSpeed;
        public static ConfigEntry<Vector3> configTamedLoxDamageBoxPosition;
        public static ConfigEntry<Vector3> configTamedLoxDamageBoxScale;

        public static ConfigEntry<float> configHarpoonMaxRopeLength;
        public static ConfigEntry<float> configHarpoonPullDuration;

        public static ConfigEntry<float> configSpeedBoat;

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

            configDamagePlayersEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable player damage", true,
                new ConfigDescription("Whether the damage from players is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configDamageFallEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable fall damage", true,
                new ConfigDescription("Whether fall damage is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            configDamageMonstersEnable = Glimuleikar.Instance.Config.Bind(sectionDamage, "Enable monster damage", true,
                new ConfigDescription("Whether the damage from monsters is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));


            configTamedLoxDamageFriendliesEnable = Glimuleikar.Instance.Config.Bind(sectionLox, "Enable tamed Lox damage to players", false,
                new ConfigDescription("Whether tamed Lox can damage players by running into them", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configTamedLoxDamageFriendliesEnable.SettingChanged += (sender, args) => { LoxPatchesWBTI.OnTamedLoxChange(); };

            configTamedLoxSpeedThreshold = Glimuleikar.Instance.Config.Bind(sectionLox, "Lox speed threshold", 5.0f,
                new ConfigDescription("The speed threshold at which tamed Lox can damage entities", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configTamedLoxSpeedThreshold.SettingChanged += (sender, args) => { LoxPatchesWBTI.OnTamedLoxChange(); };

            configTamedLoxTurningSpeed = Glimuleikar.Instance.Config.Bind(sectionLox, "Lox turning speed", 70f,
                new ConfigDescription("The turning speed of a tamed lox", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configTamedLoxTurningSpeed.SettingChanged += (sender, args) => { LoxPatchesWBTI.OnTamedLoxChange(); };

            configTamedLoxDamageBoxPosition = Glimuleikar.Instance.Config.Bind(sectionLox, "Lox damage box position", new Vector3(0f, 0f, 0f),
                new ConfigDescription("The position of the damage box relative to the lox's center", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configTamedLoxDamageBoxPosition.SettingChanged += (sender, args) => { LoxPatchesWBTI.OnTamedLoxChange(); };

            configTamedLoxDamageBoxScale = Glimuleikar.Instance.Config.Bind(sectionLox, "Lox damage box scale", new Vector3(2.73f, 2.43f, 1f),
                new ConfigDescription("The scale of the damage box", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            configTamedLoxDamageBoxScale.SettingChanged += (sender, args) => { LoxPatchesWBTI.OnTamedLoxChange(); };


            configSpeedBoat = Glimuleikar.Instance.Config.Bind(sectionBoat, "Boat speed", 5.0f,
                new ConfigDescription("The speed for boats", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, Glimuleikar.configFileName);
            configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
            configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
            configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
            configWatcher.IncludeSubdirectories = true;
            configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            configWatcher.EnableRaisingEvents = true;
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
