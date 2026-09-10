using BepInEx;
using CraftingStationTweakz.Configs;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections;
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
        public const string PluginVersion = "1.0.0";
        public static CraftingStationTweakz Instance;

        private const float DebounceSeconds = 0.3f;
        private Coroutine extensionUpdateRoutine;
        private Coroutine craftingStationUpdateRoutine;

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

        // Coalesces rapid-fire SettingChanged events (e.g. dragging a slider in ConfigurationManager)
        // into a single update once the value stops changing for DebounceSeconds.
        public void RequestUpdateExtensionPieces()
        {
            if (extensionUpdateRoutine != null)
                StopCoroutine(extensionUpdateRoutine);
            extensionUpdateRoutine = StartCoroutine(DebounceUpdateExtensionPieces());
        }

        private IEnumerator DebounceUpdateExtensionPieces()
        {
            yield return new WaitForSeconds(DebounceSeconds);
            extensionUpdateRoutine = null;
            UpdateExtensionPieces();
        }

        public void RequestUpdateCraftingStation()
        {
            if (craftingStationUpdateRoutine != null)
                StopCoroutine(craftingStationUpdateRoutine);
            craftingStationUpdateRoutine = StartCoroutine(DebounceUpdateCraftingStation());
        }

        private IEnumerator DebounceUpdateCraftingStation()
        {
            yield return new WaitForSeconds(DebounceSeconds);
            craftingStationUpdateRoutine = null;
            UpdateCraftingStation();
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

                extension.m_maxStationDistance = PluginConfig.extensionRange.Value;
                piece.m_spaceRequirement = PluginConfig.extensionSpaceRange.Value;
            }

            // Already-placed/loaded extensions in the current world
            foreach (var extension in StationExtension.m_allExtensions)
            {
                if (extension == null)
                    continue;

                Piece piece = extension.GetComponent<Piece>();
                if (piece == null)
                    continue;

                Jotunn.Logger.LogWarning($"Adjusting live upgrade instance {extension.name}...");
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

                craftingStation.m_rangeBuild = PluginConfig.stationBuildRange.Value;
                craftingStation.m_extraRangePerLevel = PluginConfig.stationBuildRangePerUpgrade.Value;
            }

            // Already-placed/loaded crafting stations in the current world
            foreach (var craftingStation in CraftingStation.m_allStations)
            {
                if (craftingStation == null)
                    continue;

                Jotunn.Logger.LogWarning($"Adjusting live crafting station instance {craftingStation.name}...");
                craftingStation.m_rangeBuild = PluginConfig.stationBuildRange.Value;
                craftingStation.m_extraRangePerLevel = PluginConfig.stationBuildRangePerUpgrade.Value;
            }
        }
    }
}

