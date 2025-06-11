using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Food.Models;
using ModularMagic_Food.Configs;
using System;
using System.Collections.Generic;
using UnityEngine;
using ModularMagic_Food.Helpers;
using System.Reflection;

namespace ModularMagic_Food
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_Food : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_Food";
        public const string PluginName = "ModularMagic_Food";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_Food Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddFood;
            ZoneManager.OnVanillaVegetationAvailable += AddLocations;
            //ItemManager.OnItemsRegistered += LogRecipes;
        }

        //private void LogRecipes()
        //{
        //    ObjectDB.instance.m_recipes.ForEach(r =>
        //    {
        //        if (r.name.Contains("MMF_"))
        //            Jotunn.Logger.LogInfo(r.name);
        //    });

        //    ItemManager.OnItemsRegistered -= LogRecipes;
        //}

        private void AddFood()
        {
            try
            {
                ItemHelper.Create(prefabs.mushroom1Prefab, PluginConfig.mushroom1, prefabs.mushroom1PickablePrefab);
                ItemHelper.Create(prefabs.mushroom2Prefab, PluginConfig.mushroom2, prefabs.mushroom2PickablePrefab);
                ItemHelper.Create(prefabs.mushroom3Prefab, PluginConfig.mushroom3, prefabs.mushroom3PickablePrefab);
                ItemHelper.Create(prefabs.mushroom4Prefab, PluginConfig.mushroom4, prefabs.mushroom4PickablePrefab);
                ItemHelper.Create(prefabs.mushroom5Prefab, PluginConfig.mushroom5, prefabs.mushroom5PickablePrefab);
                ItemHelper.Create(prefabs.mushroom1CookedPrefab, PluginConfig.mushroom1Cooked);
                ItemHelper.Create(prefabs.mushroom1SoupPrefab, PluginConfig.mushroom1Soup);
                ItemHelper.Create(prefabs.mushroom2SoupPrefab, PluginConfig.mushroom2Soup);
                ItemHelper.Create(prefabs.mushroom3SoupPrefab, PluginConfig.mushroom3Soup);
                ItemHelper.Create(prefabs.mushroom4SoupPrefab, PluginConfig.mushroom4Soup);
                ItemHelper.Create(prefabs.mushroom5SoupPrefab, PluginConfig.mushroom5Soup);
                ItemHelper.Create(prefabs.jerky1Prefab, PluginConfig.jerky1);

                CookingConversionConfig cookedMushroomConfig = new CookingConversionConfig();
                cookedMushroomConfig.FromItem = prefabs.mushroom1Prefab.name;
                cookedMushroomConfig.ToItem = prefabs.mushroom1CookedPrefab.name;
                cookedMushroomConfig.Station = CookingStations.CookingStation;
                cookedMushroomConfig.CookTime = 20f;
                ItemManager.Instance.AddItemConversion(new CustomItemConversion(cookedMushroomConfig));

                PrefabManager.OnVanillaPrefabsAvailable -= AddFood;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add food: " + e);
            }
        }

        private void AddLocations()
        {
            try
            {
                //// Increase size to find them better
                //Transform visual = magicalMushroomPickablePrefab.transform.Find("visual");
                //visual.localScale = new Vector3(3, 3, 3);

                //Transform visualGribSnow = gribSnowMushroomPickablePrefab.transform.Find("visual");
                //visualGribSnow.localScale = new Vector3(3, 3, 3);

                //Transform visualBog = bogMushroomPickablePrefab.transform.Find("visual");
                //Transform visualBogFoot = bogMushroomPickablePrefab.transform.Find("bogfoot");
                //visualBog.localScale = new Vector3(3, 3, 3);
                //visualBogFoot.localScale = new Vector3(3, 3, 3);

                // Magical Mushroom
                List<Heightmap.Biome> magicMushroomBiomeList = new List<Heightmap.Biome>();
                magicMushroomBiomeList.Add(Heightmap.Biome.Meadows);

                VegetationConfig magicMushroomVegetationConfig = new VegetationConfig();
                magicMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(magicMushroomBiomeList.ToArray());
                magicMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
                magicMushroomVegetationConfig.BlockCheck = true;
                magicMushroomVegetationConfig.GroupRadius = 5;
                magicMushroomVegetationConfig.GroupSizeMin = PluginConfig.mushroom1.groupMin.Value;
                magicMushroomVegetationConfig.GroupSizeMax = PluginConfig.mushroom1.groupMax.Value;
                //magicMushroomVegetationConfig.ScaleMin = PluginConfig.mushroom1.scaleMin.Value;
                //magicMushroomVegetationConfig.ScaleMax = PluginConfig.mushroom1.scaleMax.Value;
                magicMushroomVegetationConfig.ScaleMin = 1f;
                magicMushroomVegetationConfig.ScaleMax = 1.5f;
                magicMushroomVegetationConfig.InForest = true;
                magicMushroomVegetationConfig.ForestThresholdMin = 0f;
                magicMushroomVegetationConfig.ForestThresholdMax = 1f;
                magicMushroomVegetationConfig.Min = 1;
                magicMushroomVegetationConfig.Max = 2;
                magicMushroomVegetationConfig.MinAltitude = PluginConfig.mushroom1.minAltitude.Value;
                magicMushroomVegetationConfig.MaxAltitude = PluginConfig.mushroom1.maxAltitude.Value;
                magicMushroomVegetationConfig.MinTerrainDelta = 0f;
                magicMushroomVegetationConfig.MaxTerrainDelta = 2f;
                magicMushroomVegetationConfig.TerrainDeltaRadius = 0f;
                magicMushroomVegetationConfig.MinOceanDepth = 0f;
                magicMushroomVegetationConfig.MaxOceanDepth = 2f;
                magicMushroomVegetationConfig.MinTilt = 0f;
                magicMushroomVegetationConfig.MaxTilt = 25;
                ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.mushroom1PickablePrefab, true, magicMushroomVegetationConfig));

                // Gribsnow Mushroom
                List<Heightmap.Biome> gribSnowBiomeList = new List<Heightmap.Biome>();
                gribSnowBiomeList.Add(Heightmap.Biome.BlackForest);

                VegetationConfig gribSnowVegetationConfig = new VegetationConfig();
                gribSnowVegetationConfig.Biome = ZoneManager.AnyBiomeOf(gribSnowBiomeList.ToArray());
                gribSnowVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
                gribSnowVegetationConfig.BlockCheck = true;
                gribSnowVegetationConfig.GroupRadius = 5;
                gribSnowVegetationConfig.GroupSizeMin = PluginConfig.mushroom2.groupMin.Value;
                gribSnowVegetationConfig.GroupSizeMax = PluginConfig.mushroom2.groupMax.Value;
                //gribSnowVegetationConfig.ScaleMin = PluginConfig.mushroom2.scaleMin.Value;
                //gribSnowVegetationConfig.ScaleMax = PluginConfig.mushroom2.scaleMax.Value;
                gribSnowVegetationConfig.ScaleMin = 1f;
                gribSnowVegetationConfig.ScaleMax = 1.5f;
                gribSnowVegetationConfig.InForest = false;
                gribSnowVegetationConfig.ForestThresholdMin = 0f;
                gribSnowVegetationConfig.ForestThresholdMax = 1f;
                gribSnowVegetationConfig.Min = 1;
                gribSnowVegetationConfig.Max = 2;
                gribSnowVegetationConfig.MinAltitude = PluginConfig.mushroom2.minAltitude.Value;
                gribSnowVegetationConfig.MaxAltitude = PluginConfig.mushroom2.maxAltitude.Value;
                gribSnowVegetationConfig.MinTerrainDelta = 0f;
                gribSnowVegetationConfig.MaxTerrainDelta = 2f;
                gribSnowVegetationConfig.TerrainDeltaRadius = 0f;
                gribSnowVegetationConfig.MinOceanDepth = 0f;
                gribSnowVegetationConfig.MaxOceanDepth = 2f;
                gribSnowVegetationConfig.MinTilt = 0f;
                gribSnowVegetationConfig.MaxTilt = 25;
                ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.mushroom2PickablePrefab, true, gribSnowVegetationConfig));

                // Bog Mushroom
                List<Heightmap.Biome> bogMushroomBiomeList = new List<Heightmap.Biome>();
                bogMushroomBiomeList.Add(Heightmap.Biome.Swamp);

                VegetationConfig bogMushroomVegetationConfig = new VegetationConfig();
                bogMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(bogMushroomBiomeList.ToArray());
                bogMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
                bogMushroomVegetationConfig.BlockCheck = true;
                bogMushroomVegetationConfig.GroupRadius = 4;
                bogMushroomVegetationConfig.GroupSizeMin = PluginConfig.mushroom3.groupMin.Value;
                bogMushroomVegetationConfig.GroupSizeMax = PluginConfig.mushroom3.groupMax.Value;
                //bogMushroomVegetationConfig.ScaleMin = PluginConfig.mushroom3.scaleMin.Value;
                //bogMushroomVegetationConfig.ScaleMax = PluginConfig.mushroom3.scaleMax.Value;
                bogMushroomVegetationConfig.ScaleMin = 1f;
                bogMushroomVegetationConfig.ScaleMax = 1f;
                bogMushroomVegetationConfig.InForest = false;
                bogMushroomVegetationConfig.ForestThresholdMin = 0f;
                bogMushroomVegetationConfig.ForestThresholdMax = 1f;
                bogMushroomVegetationConfig.Min = 1;
                bogMushroomVegetationConfig.Max = 2;
                bogMushroomVegetationConfig.MinAltitude = PluginConfig.mushroom3.minAltitude.Value;
                bogMushroomVegetationConfig.MaxAltitude = PluginConfig.mushroom3.maxAltitude.Value;
                bogMushroomVegetationConfig.MinTerrainDelta = 0f;
                bogMushroomVegetationConfig.MaxTerrainDelta = 2f;
                bogMushroomVegetationConfig.TerrainDeltaRadius = 0f;
                bogMushroomVegetationConfig.MinOceanDepth = 0f;
                bogMushroomVegetationConfig.MaxOceanDepth = 0f;
                bogMushroomVegetationConfig.MinTilt = 0f;
                bogMushroomVegetationConfig.MaxTilt = 20;
                ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.mushroom3PickablePrefab, true, bogMushroomVegetationConfig));

                // Pirced Mushroom
                List<Heightmap.Biome> pircedMushroomBiomeList = new List<Heightmap.Biome>();
                pircedMushroomBiomeList.Add(Heightmap.Biome.Mountain);

                VegetationConfig pircedMushroomVegetationConfig = new VegetationConfig();
                pircedMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(pircedMushroomBiomeList.ToArray());
                pircedMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Median;
                pircedMushroomVegetationConfig.BlockCheck = true;
                pircedMushroomVegetationConfig.GroupRadius = 4;
                pircedMushroomVegetationConfig.GroupSizeMin = PluginConfig.mushroom4.groupMin.Value;
                pircedMushroomVegetationConfig.GroupSizeMax = PluginConfig.mushroom4.groupMax.Value;
                //pircedMushroomVegetationConfig.ScaleMin = PluginConfig.mushroom4.scaleMin.Value;
                //pircedMushroomVegetationConfig.ScaleMax = PluginConfig.mushroom4.scaleMax.Value;                
                pircedMushroomVegetationConfig.ScaleMin = 0.8f;
                pircedMushroomVegetationConfig.ScaleMax = 1f;
                pircedMushroomVegetationConfig.InForest = false;
                pircedMushroomVegetationConfig.ForestThresholdMin = 0f;
                pircedMushroomVegetationConfig.ForestThresholdMax = 1f;
                pircedMushroomVegetationConfig.Min = 1;
                pircedMushroomVegetationConfig.Max = 2;
                pircedMushroomVegetationConfig.MinAltitude = PluginConfig.mushroom4.minAltitude.Value;
                pircedMushroomVegetationConfig.MaxAltitude = PluginConfig.mushroom4.maxAltitude.Value;
                pircedMushroomVegetationConfig.MinTerrainDelta = 0f;
                pircedMushroomVegetationConfig.MaxTerrainDelta = 2f;
                pircedMushroomVegetationConfig.TerrainDeltaRadius = 0f;
                pircedMushroomVegetationConfig.MinOceanDepth = 0f;
                pircedMushroomVegetationConfig.MaxOceanDepth = 0f;
                pircedMushroomVegetationConfig.MinTilt = 0f;
                pircedMushroomVegetationConfig.MaxTilt = 20;
                ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.mushroom4PickablePrefab, true, pircedMushroomVegetationConfig));

                // Violet Cloud Mushroom
                List<Heightmap.Biome> cloudCloudMushroomBiomeList = new List<Heightmap.Biome>();
                cloudCloudMushroomBiomeList.Add(Heightmap.Biome.Plains);

                VegetationConfig cloudMushroomVegetationConfig = new VegetationConfig();
                cloudMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(cloudCloudMushroomBiomeList.ToArray());
                cloudMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Median;
                cloudMushroomVegetationConfig.BlockCheck = true;
                cloudMushroomVegetationConfig.GroupRadius = 4;
                cloudMushroomVegetationConfig.GroupSizeMin = PluginConfig.mushroom5.groupMin.Value;
                cloudMushroomVegetationConfig.GroupSizeMax = PluginConfig.mushroom5.groupMax.Value;
                //cloudMushroomVegetationConfig.ScaleMin = PluginConfig.mushroom5.scaleMin.Value;
                //cloudMushroomVegetationConfig.ScaleMax = PluginConfig.mushroom5.scaleMax.Value;
                cloudMushroomVegetationConfig.ScaleMin = 0.7f;
                cloudMushroomVegetationConfig.ScaleMax = 1f;
                cloudMushroomVegetationConfig.InForest = false;
                cloudMushroomVegetationConfig.ForestThresholdMin = 0f;
                cloudMushroomVegetationConfig.ForestThresholdMax = 1f;
                cloudMushroomVegetationConfig.Min = 1;
                cloudMushroomVegetationConfig.Max = 2;
                cloudMushroomVegetationConfig.MinAltitude = PluginConfig.mushroom5.minAltitude.Value;
                cloudMushroomVegetationConfig.MaxAltitude = PluginConfig.mushroom5.maxAltitude.Value;
                cloudMushroomVegetationConfig.MinTerrainDelta = 0f;
                cloudMushroomVegetationConfig.MaxTerrainDelta = 2f;
                cloudMushroomVegetationConfig.TerrainDeltaRadius = 0f;
                cloudMushroomVegetationConfig.MinOceanDepth = 0f;
                cloudMushroomVegetationConfig.MaxOceanDepth = 0f;
                cloudMushroomVegetationConfig.MinTilt = 0f;
                cloudMushroomVegetationConfig.MaxTilt = 20;
                ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.mushroom5PickablePrefab, true, cloudMushroomVegetationConfig));
                ZoneManager.OnVanillaVegetationAvailable -= AddLocations;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set locations: " + e);
            }
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_food_dw");

            // Mushrooms
            prefabs.mushroom1Prefab = assetBundle.LoadAsset<GameObject>("MMF_MagicalMushroom");
            prefabs.mushroom1CookedPrefab = assetBundle.LoadAsset<GameObject>("MMF_CookedMagicalMushroom");
            prefabs.mushroom1PickablePrefab = assetBundle.LoadAsset<GameObject>("MMF_Pickable_MagicalMushroom");
            prefabs.mushroom2Prefab = assetBundle.LoadAsset<GameObject>("MMF_GribSnowMushroom");
            prefabs.mushroom2PickablePrefab = assetBundle.LoadAsset<GameObject>("MMF_Pickable_GribSnowMushroom");
            prefabs.mushroom3Prefab = assetBundle.LoadAsset<GameObject>("MMF_BogMushroom");
            prefabs.mushroom3PickablePrefab = assetBundle.LoadAsset<GameObject>("MMF_Pickable_BogMushroom");
            prefabs.mushroom4Prefab = assetBundle.LoadAsset<GameObject>("MMF_PircedMushroom");
            prefabs.mushroom4PickablePrefab = assetBundle.LoadAsset<GameObject>("MMF_Pickable_PircedMushroom");
            prefabs.mushroom5Prefab = assetBundle.LoadAsset<GameObject>("MMF_VioletCloudMushroom");
            prefabs.mushroom5PickablePrefab = assetBundle.LoadAsset<GameObject>("MMF_Pickable_VioletCloudMushroom");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.mushroom1PickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.mushroom2PickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.mushroom3PickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.mushroom4PickablePrefab, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.mushroom5PickablePrefab, true));

            // Soups
            prefabs.mushroom1SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_MagicalMushroomSoup");
            prefabs.mushroom2SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_GribSnowMushroomSoup");
            prefabs.mushroom3SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_BogMushroomSoup");
            prefabs.mushroom4SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_PircedMushroomSoup");
            prefabs.mushroom5SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_VioletCloudMushroomSoup");
            prefabs.jerky1Prefab = assetBundle.LoadAsset<GameObject>("MMF_SeekerJerky");

            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(assetBundle.LoadAsset<StatusEffect>("Puke_MMF"), true));
        }
    }
}

