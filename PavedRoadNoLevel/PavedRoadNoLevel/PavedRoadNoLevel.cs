using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using PavedRoadNoLevel.Configs;
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
        public const string PluginVersion = "1.0.9";
        public static string configFileName = PluginGUID + ".cfg";
        public static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;
        public static PavedRoadNoLevel Instance;

        public CraftingStation stonecutterPiece;
        private bool firstPatch = true;

        public void Awake()
        {
            Instance = this;
            PluginConfig.Init();

            if (!PluginConfig.configEnable.Value) return;

            PrefabManager.OnVanillaPrefabsAvailable += PatchOriginal;
        }

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

        public void ApplyConfigChanges()
        {
            if (PluginConfig.configEnable.Value)
            {
                PatchOriginal();
            }
            else
            {
                UnpatchOriginal();
            }
        }
    }
}
