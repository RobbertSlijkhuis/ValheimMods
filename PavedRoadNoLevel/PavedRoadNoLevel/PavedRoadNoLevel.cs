using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.IO;
using UnityEngine;

namespace PavedRoadNoLevel
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class PavedRoadNoLevel : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.PavedRoadNoLevel";
        public const string PluginName = "Paved Road No Level";
        public const string PluginVersion = "1.0.4";
        private static string configFileName = PluginGUID + ".cfg";
        private static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;

        private ConfigEntry<bool> configEnable;
        private ConfigEntry<bool> configRequireStoncutter;

        private CraftingStation stonecutterPiece;
        private bool firstPatch = true;

        /**
         * Called when the plugin is being initialised
         */
        private void Awake()
        {
            InitConfig();

            if (!configEnable.Value) return;

            PrefabManager.OnVanillaPrefabsAvailable += PatchOriginal;
        }

        /**
         * Called when the plugin is unloaded
         */
        private void OnDestroy()
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
                GameObject original = PrefabManager.Instance.GetPrefab("paved_road_v2");
                Piece pieceComp = original.GetComponent<Piece>();
                pieceComp.m_allowAltGroundPlacement = false;

                if (firstPatch)
                {
                    stonecutterPiece = pieceComp.m_craftingStation;
                    firstPatch = false;
                }

                if (!configRequireStoncutter.Value)
                    pieceComp.m_craftingStation = null;
                else if (pieceComp.m_craftingStation == null)
                    pieceComp.m_craftingStation = stonecutterPiece;

                TerrainOp terrainComp = original.GetComponent<TerrainOp>();
                terrainComp.m_settings.m_smooth = false;

                Jotunn.Logger.LogInfo("Successfully patched Paved Road, enjoy!");
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
                GameObject original = PrefabManager.Instance.GetPrefab("paved_road_v2");
                Piece pieceComp = original.GetComponent<Piece>();
                pieceComp.m_allowAltGroundPlacement = true;

                if (pieceComp.m_craftingStation == null)
                    pieceComp.m_craftingStation = stonecutterPiece;

                TerrainOp terrainComp = original.GetComponent<TerrainOp>();
                terrainComp.m_settings.m_smooth = true;

                Jotunn.Logger.LogInfo("Successfully unpatched Paved Road, Why u do this?!");
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
                Config.SaveOnConfigSet = true;

                configEnable = Config.Bind(new ConfigDefinition("General", "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configEnable.SettingChanged += (obj, attr) => { ApplyConfigChanges(); };

                configRequireStoncutter = Config.Bind(new ConfigDefinition("General", "Stonecutter requirement"), true,
                    new ConfigDescription("Enable the Stonecutter as a requirement (to pave roads)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
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
