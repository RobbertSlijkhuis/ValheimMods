using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;

namespace PavedRoadNoLevel.Configs
{
    internal static class PluginConfig
    {
        public static string sectionGeneral = "General";

        public static ConfigEntry<bool> configEnable;
        public static ConfigEntry<bool> configRequireStoncutter;

        // Other
        private static int entryCount = 1000;
        private static FileSystemWatcher configWatcher;

        public static void Init()
        {
            InitGeneralConfig();
        }

        /**
         * Initialise config entries and add the necessary events
         */
        public static void InitGeneralConfig()
        {
            try
            {
                PavedRoadNoLevel.Instance.Config.SaveOnConfigSet = false;

                configEnable = PavedRoadNoLevel.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                configEnable.SettingChanged += (obj, attr) => { PavedRoadNoLevel.Instance.ApplyConfigChanges(); };

                PavedRoadNoLevel.Instance.Config.SaveOnConfigSet = true;

                configRequireStoncutter = PavedRoadNoLevel.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Stonecutter requirement"), true,
                    new ConfigDescription("Enable the Stonecutter as a requirement (to pave roads)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
                configRequireStoncutter.SettingChanged += (obj, attr) => { PavedRoadNoLevel.Instance.ApplyConfigChanges(); };

                configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, PavedRoadNoLevel.configFileName);
                configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
                configWatcher.IncludeSubdirectories = true;
                configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
                configWatcher.EnableRaisingEvents = true;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise the config & events: " + error);
            }
        }

        private static void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(PavedRoadNoLevel.configFileFullPath))
                return;

            try
            {
                PavedRoadNoLevel.Instance.Config.Reload();
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
