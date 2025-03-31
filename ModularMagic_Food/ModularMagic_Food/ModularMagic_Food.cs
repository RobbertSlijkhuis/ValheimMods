using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_Food.Models;
using ModularMagic_Food.Configs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ModularMagic_Food.Helpers;

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
            ConfigFood.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddFood;
            ZoneManager.OnVanillaVegetationAvailable += AddLocations;
            Jotunn.Logger.LogInfo("ModularMagic_Food has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMF_"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddFood()
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

                ItemHelper.CreateFoodItem(prefabs.mushroom1Prefab, ConfigFood.mushroom1);
                ItemHelper.CreateFoodItem(prefabs.mushroom1CookedPrefab, ConfigFood.mushroom1Cooked);
                ItemHelper.CreateFoodItem(prefabs.mushroom2Prefab, ConfigFood.mushroom2);
                ItemHelper.CreateFoodItem(prefabs.mushroom3Prefab, ConfigFood.mushroom3);
                ItemHelper.CreateFoodItem(prefabs.mushroom4Prefab, ConfigFood.mushroom4);
                ItemHelper.CreateFoodItem(prefabs.mushroom5Prefab, ConfigFood.mushroom5);
                ItemHelper.CreateFoodItem(prefabs.mushroom1SoupPrefab, ConfigFood.mushroom1Soup);
                ItemHelper.CreateFoodItem(prefabs.mushroom2SoupPrefab, ConfigFood.mushroom2Soup);
                ItemHelper.CreateFoodItem(prefabs.mushroom3SoupPrefab, ConfigFood.mushroom3Soup);
                ItemHelper.CreateFoodItem(prefabs.mushroom4SoupPrefab, ConfigFood.mushroom4Soup);
                ItemHelper.CreateFoodItem(prefabs.mushroom5SoupPrefab, ConfigFood.mushroom5Soup);

                CookingConversionConfig cookedMushroomConfig = new CookingConversionConfig();
                cookedMushroomConfig.FromItem = prefabs.mushroom1Prefab.name;
                cookedMushroomConfig.ToItem = prefabs.mushroom1CookedPrefab.name;
                cookedMushroomConfig.Station = CookingStations.CookingStation;
                cookedMushroomConfig.CookTime = 20f;
                ItemManager.Instance.AddItemConversion(new CustomItemConversion(cookedMushroomConfig));

                PrefabManager.OnVanillaPrefabsAvailable -= AddFood;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not add food: " + error);
            }
        }

        private void AddLocations()
        {
            // Magical Mushroom
            List<Heightmap.Biome> magicMushroomBiomeList = new List<Heightmap.Biome>();
            magicMushroomBiomeList.Add(Heightmap.Biome.Meadows);

            VegetationConfig magicMushroomVegetationConfig = new VegetationConfig();
            magicMushroomVegetationConfig.Biome = ZoneManager.AnyBiomeOf(magicMushroomBiomeList.ToArray());
            magicMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
            magicMushroomVegetationConfig.BlockCheck = true;
            magicMushroomVegetationConfig.GroupRadius = 5;
            magicMushroomVegetationConfig.GroupSizeMin = 3;
            magicMushroomVegetationConfig.GroupSizeMax = 6;
            magicMushroomVegetationConfig.ScaleMin = 1f;
            magicMushroomVegetationConfig.ScaleMax = 1.5f;
            magicMushroomVegetationConfig.InForest = true;
            magicMushroomVegetationConfig.ForestThresholdMin = 0;
            magicMushroomVegetationConfig.ForestThresholdMax = 1;
            magicMushroomVegetationConfig.Min = 1;
            magicMushroomVegetationConfig.Max = 2;
            magicMushroomVegetationConfig.MinAltitude = 1f;
            magicMushroomVegetationConfig.MaxAltitude = 1000f;
            magicMushroomVegetationConfig.MinTerrainDelta = 0f;
            magicMushroomVegetationConfig.MaxTerrainDelta = 2f;
            magicMushroomVegetationConfig.TerrainDeltaRadius = 0f;
            magicMushroomVegetationConfig.MinOceanDepth = 0f;
            magicMushroomVegetationConfig.MaxOceanDepth = 2f;
            magicMushroomVegetationConfig.MinTilt = 0f;
            magicMushroomVegetationConfig.MaxTilt = 25;
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefabs.mushroom1PickablePrefab, true, magicMushroomVegetationConfig));

            // GribSnow
            List<Heightmap.Biome> gribSnowBiomeList = new List<Heightmap.Biome>();
            gribSnowBiomeList.Add(Heightmap.Biome.BlackForest);

            VegetationConfig gribSnowVegetationConfig = new VegetationConfig();
            gribSnowVegetationConfig.Biome = ZoneManager.AnyBiomeOf(gribSnowBiomeList.ToArray());
            gribSnowVegetationConfig.BiomeArea = Heightmap.BiomeArea.Everything;
            gribSnowVegetationConfig.BlockCheck = true;
            gribSnowVegetationConfig.GroupRadius = 5;
            gribSnowVegetationConfig.GroupSizeMin = 2;
            gribSnowVegetationConfig.GroupSizeMax = 4;
            gribSnowVegetationConfig.ScaleMin = 1f;
            gribSnowVegetationConfig.ScaleMax = 1.5f;
            gribSnowVegetationConfig.InForest = true;
            gribSnowVegetationConfig.ForestThresholdMin = 0;
            gribSnowVegetationConfig.ForestThresholdMax = 1;
            gribSnowVegetationConfig.Min = 1;
            gribSnowVegetationConfig.Max = 2;
            gribSnowVegetationConfig.MinAltitude = 1f;
            gribSnowVegetationConfig.MaxAltitude = 1000f;
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
            bogMushroomVegetationConfig.BiomeArea = Heightmap.BiomeArea.Median;
            bogMushroomVegetationConfig.BlockCheck = true;
            bogMushroomVegetationConfig.GroupRadius = 4;
            bogMushroomVegetationConfig.GroupSizeMin = 2;
            bogMushroomVegetationConfig.GroupSizeMax = 5;
            bogMushroomVegetationConfig.ScaleMin = 0.7f;
            bogMushroomVegetationConfig.ScaleMax = 1f;
            bogMushroomVegetationConfig.InForest = false;
            bogMushroomVegetationConfig.ForestThresholdMin = 1f;
            bogMushroomVegetationConfig.ForestThresholdMax = 1.15f;
            bogMushroomVegetationConfig.Min = 1;
            bogMushroomVegetationConfig.Max = 2;
            bogMushroomVegetationConfig.MinAltitude = 0f;
            bogMushroomVegetationConfig.MaxAltitude = 1000f;
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
            pircedMushroomVegetationConfig.GroupSizeMin = 2;
            pircedMushroomVegetationConfig.GroupSizeMax = 5;
            pircedMushroomVegetationConfig.ScaleMin = 0.8f;
            pircedMushroomVegetationConfig.ScaleMax = 1f;
            pircedMushroomVegetationConfig.InForest = false;
            pircedMushroomVegetationConfig.ForestThresholdMin = 1f;
            pircedMushroomVegetationConfig.ForestThresholdMax = 1.15f;
            pircedMushroomVegetationConfig.Min = 1;
            pircedMushroomVegetationConfig.Max = 2;
            pircedMushroomVegetationConfig.MinAltitude = 0f;
            pircedMushroomVegetationConfig.MaxAltitude = 1000f;
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
            cloudMushroomVegetationConfig.GroupSizeMin = 2;
            cloudMushroomVegetationConfig.GroupSizeMax = 5;
            cloudMushroomVegetationConfig.ScaleMin = 0.7f;
            cloudMushroomVegetationConfig.ScaleMax = 1f;
            cloudMushroomVegetationConfig.InForest = false;
            cloudMushroomVegetationConfig.ForestThresholdMin = 1f;
            cloudMushroomVegetationConfig.ForestThresholdMax = 1.15f;
            cloudMushroomVegetationConfig.Min = 1;
            cloudMushroomVegetationConfig.Max = 2;
            cloudMushroomVegetationConfig.MinAltitude = 0f;
            cloudMushroomVegetationConfig.MaxAltitude = 1000f;
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

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_food_dw");

            // Food
            prefabs.mushroom1Prefab = assetBundle.LoadAsset<GameObject>("MMF_MagicalMushroom");
            prefabs.mushroom1CookedPrefab = assetBundle.LoadAsset<GameObject>("MMF_CookedMagicalMushroom");
            prefabs.mushroom1PickablePrefab = assetBundle.LoadAsset<GameObject>("MMF_Pickable_MagicalMushroom");
            prefabs.mushroom2Prefab = assetBundle.LoadAsset<GameObject>("MMF_GribSnow");
            prefabs.mushroom2PickablePrefab = assetBundle.LoadAsset<GameObject>("MMF_Pickable_GribSnow");
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

            prefabs.mushroom1SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_MagicalMushroomSoup");
            prefabs.mushroom2SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_GribSnowMushroomSoup");
            prefabs.mushroom3SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_BogMushroomSoup");
            prefabs.mushroom4SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_PircedMushroomSoup");
            prefabs.mushroom5SoupPrefab = assetBundle.LoadAsset<GameObject>("MMF_VioletCloudMushroomSoup");
        }
    }
}

