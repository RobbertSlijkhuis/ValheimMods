using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using UnityEngine;

namespace ValheimModding
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class PavedRoad : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.PavedRoad";
        public const string PluginName = "Paved Road";
        public const string PluginVersion = "1.0.0";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private AssetBundle pavedRoadBundle;
        private GameObject pavedRoad;
        private GameObject pavedRoadStonecutter;

        private ConfigEntry<bool> configEnable;
        private ConfigEntry<bool> configRequireStoncutter;

        private void Awake()
        {   
            InitConfig();

            if (!configEnable.Value) return;

            InitAssetBundle();

            PieceManager.OnPiecesRegistered += RemoveOrginalPavedRoad;
            PrefabManager.OnVanillaPrefabsAvailable += AddCustomPavedRoad;
        }

        private void AddCustomPavedRoad()
        {
            try
            {
                PieceConfig config = new PieceConfig();
                config.Name = "Paved road";
                config.PieceTable = PieceTables.Hoe;
                config.Category = PieceCategories.Misc;
                GameObject prefab = configRequireStoncutter.Value == true ? pavedRoadStonecutter : pavedRoad;

                PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, config));
                PrefabManager.OnVanillaPrefabsAvailable -= AddCustomPavedRoad;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError(error);
            }
        }

        private void RemoveOrginalPavedRoad()
        {
            try
            {
                PieceTable hoe = PieceManager.Instance.GetPieceTable(PieceTables.Hoe);
                GameObject pieceToRemove = null;

                foreach (var piece in hoe.m_pieces)
                {
                    if (piece.name == "paved_road_v2")
                    {
                        pieceToRemove = piece;
                        break;
                    }
                }

                if (pieceToRemove != null)
                    hoe.m_pieces.Remove(pieceToRemove);
                else
                    throw new Exception("Could not find the original paved roud prefab!");
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError(error);
            }
        }

        private void InitConfig()
        {
            try
            {
                Config.SaveOnConfigSet = true;

                configEnable = base.Config.Bind(new ConfigDefinition("General", "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configRequireStoncutter = base.Config.Bind(new ConfigDefinition("General", "Stonecutter requirement"), true,
                    new ConfigDescription("Enable the Stonecutter as a requirement (to pave roads)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                SynchronizationManager.OnConfigurationSynchronized += (obj, attr) =>
                {
                    if (attr.InitialSynchronization)
                    {
                        Jotunn.Logger.LogMessage("Initial Config sync event received");
                    }
                    else
                    {
                        Jotunn.Logger.LogMessage("Config sync event received");
                    }
                };
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong with initialising the config or config events: "+ error);
            }
        }

        private void InitAssetBundle()
        {
            pavedRoadBundle = AssetUtils.LoadAssetBundleFromResources("pavedroad_dw");
            pavedRoad = pavedRoadBundle.LoadAsset<GameObject>("paved_road_DW");
            pavedRoadStonecutter = pavedRoadBundle.LoadAsset<GameObject>("paved_road_stonecutter_DW");
        }
    }
}
