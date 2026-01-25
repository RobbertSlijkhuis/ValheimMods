using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using Jotunn.Utils;
using PavedRoadNoLevel.Helpers;
using System;
using System.IO;
using UnityEngine;

namespace PavedRoadNoLevel
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.Minor)]
    internal class PavedRoadNoLevel : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.PavedRoadNoLevel";
        public const string PluginName = "Paved Road No Level";
        public const string PluginVersion = "1.0.8";
        private static string configFileName = PluginGUID + ".cfg";
        private static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;
        public static PavedRoadNoLevel Instance;

        public ConfigEntry<bool> configEnable;
        public ConfigEntry<bool> configRequireStoncutter;

        public CraftingStation stonecutterPiece;
        private bool firstPatch = true;

        /**
         * Called when the plugin is being initialised
         */
        public void Awake()
        {
            Instance = this;
            InitConfig();

            if (!configEnable.Value) return;

            PrefabManager.OnVanillaPrefabsAvailable += PatchOriginal;
        }

        /**
         * Called when the plugin is unloaded
         */
        public void OnDestroy()
        {
            Config.Save();
        }

        /**
         * Patches the original paved_road_v2 prefab
         */
        private void PatchOriginal()
        {
            try
            {
                GameObject pavedRoadV2 = PrefabManager.Instance.GetPrefab("paved_road_v2");
                GameObject cultivateV2 = PrefabManager.Instance.GetPrefab("cultivate_v2");

                if (firstPatch)
                {
                    Piece piece = pavedRoadV2.GetComponent<Piece>();
                    stonecutterPiece = piece.m_craftingStation;
                    firstPatch = false;
                }

                TerrainToolHelper.SetSmooth(pavedRoadV2, false);
                TerrainToolHelper.SetSmooth(cultivateV2, false);
                TerrainToolHelper.SetStonecutter(pavedRoadV2, false);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not patch original: " + error);
            }
        }

        /**
         * Undo the changes done to the paved_road_v2 prefab
         */
        private void UnpatchOriginal()
        {
            try
            {
                GameObject pavedRoadV2 = PrefabManager.Instance.GetPrefab("paved_road_v2");
                GameObject cultivateV2 = PrefabManager.Instance.GetPrefab("cultivate_v2");

                TerrainToolHelper.SetSmooth(pavedRoadV2, true);
                TerrainToolHelper.SetSmooth(cultivateV2, true);
                TerrainToolHelper.SetStonecutter(pavedRoadV2, true);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not unpatch original: " + error);
            }
        }

        /**
         * Apply changes when the config has changed
         */
        private void ApplyConfigChanges()
        {
            if (configEnable.Value)
            {
                PatchOriginal();
            }
            else
            {
                UnpatchOriginal();
            }
        }

        /**
         * Initialise config entries and add the necessary events
         */
        private void InitConfig()
        {
            try
            {
                Config.SaveOnConfigSet = false;

                configEnable = Config.Bind(new ConfigDefinition("General", "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false }));
                configEnable.SettingChanged += (obj, attr) => { ApplyConfigChanges(); };

                Config.SaveOnConfigSet = true;

                configRequireStoncutter = Config.Bind(new ConfigDefinition("General", "Stonecutter requirement"), true,
                    new ConfigDescription("Enable the Stonecutter as a requirement (to pave roads)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false }));
                configRequireStoncutter.SettingChanged += (obj, attr) => { ApplyConfigChanges(); };

                FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, configFileName);
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

        /**
         * Event handler for when the config file changes
         */
        private void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(configFileFullPath))
                return;

            try
            {
                Config.Reload();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
            }
        }
    }
}
