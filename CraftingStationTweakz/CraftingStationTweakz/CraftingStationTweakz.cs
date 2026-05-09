using BepInEx;
using CraftingStationTweakz.Configs;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace CraftingStationTweakz
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
    internal class CraftingStationTweakz : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.CraftingStationTweakz";
        public const string PluginName = "CraftingStationTweakz";
        public const string PluginVersion = "0.0.1";
        public static CraftingStationTweakz Instance;

        private void Awake()
        {
            Instance = this;
            PluginConfig.Init();

            PrefabManager.OnPrefabsRegistered += PatchPieces;
        }

        private void PatchPieces()
        {
            UpdateExtensionPieces();
            UpdateCraftingStation();
            PrefabManager.OnPrefabsRegistered -= PatchPieces;
        }

        public void UpdateExtensionPieces()
        {
            List<GameObject> extensions = ZNetScene.instance.m_prefabs.FindAll(item => item.name.Contains("_ext"));

            foreach (var ext in extensions)
            {
                StationExtension extension = ext.GetComponent<StationExtension>();
                Piece piece = ext.GetComponent<Piece>();

                if (extension == null || piece == null)
                    continue;

                //Jotunn.Logger.LogWarning($"Adjusting upgrade {ext.name}...");
                extension.m_maxStationDistance = PluginConfig.extensionRange.Value;
                piece.m_spaceRequirement = PluginConfig.extensionSpaceRange.Value;
            }
        }

        public void UpdateCraftingStation()
        {
            List<GameObject> extensions = ZNetScene.instance.m_prefabs.FindAll(item => item.GetComponent<CraftingStation>());

            foreach (var ext in extensions)
            {
                CraftingStation craftingStation = ext.GetComponent<CraftingStation>();
                Piece piece = ext.GetComponent<Piece>();

                if (craftingStation == null || piece == null)
                    continue;

                //Jotunn.Logger.LogWarning($"Adjusting craftin station {ext.name}...");
                craftingStation.m_rangeBuild = PluginConfig.stationBuildRange.Value;
                craftingStation.m_extraRangePerLevel = PluginConfig.stationBuildRangePerUpgrade.Value;
            }
        }
    }
}

