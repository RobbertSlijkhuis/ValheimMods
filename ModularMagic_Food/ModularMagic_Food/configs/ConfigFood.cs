using ModularMagic_Food.Models;
using System;

namespace ModularMagic_Food.Configs
{
    internal static class ConfigFood
    {
        public static string mushroom1Name = "Magical Mushroom";
        public static string mushroom1Recipe = "MMF_MagicalMushroom:2";
        public static FoodConfig mushroom1 = new FoodConfig();

        public static string mushroom1CookedName = $"Cooked {mushroom1Name}";
        public static FoodConfig mushroom1Cooked = new FoodConfig();

        public static string mushroom2Name = "Gribsnow";
        public static string mushroom2Recipe = "MMF_GribSnow:2";
        public static FoodConfig mushroom2 = new FoodConfig();

        public static string mushroom3Name = "Bog Mushroom";
        public static string mushroom3Recipe = "MMF_BogMushroom:2";
        public static FoodConfig mushroom3 = new FoodConfig();

        public static string mushroom4Name = "Pirced Mushroom";
        public static string mushroom4Recipe = "MMF_PircedMushroom:2";
        public static FoodConfig mushroom4 = new FoodConfig();

        public static string mushroom5Name = "Violet Cloud Mushroom";
        public static string mushroom5Recipe = "MMF_VioletCloudMushroom:2";
        public static FoodConfig mushroom5 = new FoodConfig();

        public static string mushroom1SoupName = "Magical Mushroom soup";
        public static string mushroom1SoupRecipe = "MMF_MagicalMushroom:2";
        public static FoodConfig mushroom1Soup = new FoodConfig();

        public static string mushroom2SoupName = "GribSnow soup";
        public static string mushroom2SoupRecipe = "MMF_GribSnow:2";
        public static FoodConfig mushroom2Soup = new FoodConfig();

        public static string mushroom3SoupName = "Bog Mushroom soup";
        public static string mushroom3SoupRecipe = "MMF_BogMushroom:2";
        public static FoodConfig mushroom3Soup = new FoodConfig();

        public static string mushroom4SoupName = "Pirced Mushroom soup";
        public static string mushroom4SoupRecipe = "MMF_PircedMushroom:2";
        public static FoodConfig mushroom4Soup = new FoodConfig();

        public static string mushroom5SoupName = "Violet Cloud Mushroom soup";
        public static string mushroom5SoupRecipe = "MMF_VioletCloudMushroom:2";
        public static FoodConfig mushroom5Soup = new FoodConfig();

        // Other
        private static int sectionIndex = 1;

        public static void Init()
        {
            InitMushroom1Config();
            InitMushroom1CookedConfig();
            InitMushroom2Config();
            InitMushroom3Config();
            InitMushroom4Config();
            InitMushroom5Config();
            InitMushroomSoup1Config();
            InitMushroomSoup2Config();
            InitMushroomSoup3Config();
            InitMushroomSoup4Config();
            InitMushroomSoup5Config();
        }

        private static void InitMushroom1Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom1Prefab, mushroom1Name, sectionIndex)
                {
                    description = "An oddly blue colored mushroom with a soft glow.",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.1f,
                    health = 15f,
                    stamina = 15f,
                    eitr = 20f,
                    healthRegen = 1f,
                    burnTime = 600f,
                };
                mushroom1.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom1Name + " config: " + error);
            }
        }

        private static void InitMushroom1CookedConfig()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom1CookedPrefab, mushroom1CookedName, sectionIndex)
                {
                    description = "They say you should be carefull eating unknown mushrooms... but it looks so delicious!",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.1f,
                    health = 20f,
                    stamina = 20f,
                    eitr = 25f,
                    healthRegen = 1f,
                    burnTime = 900f,
                };
                mushroom1Cooked.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom1CookedName + " config: " + error);
            }
        }

        private static void InitMushroom2Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom2Prefab, mushroom2Name, sectionIndex)
                {
                    description = "The name completely does not imply its effect... does it?",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.1f,
                    health = 8f,
                    stamina = 25f,
                    eitr = 20f,
                    healthRegen = 1f,
                    burnTime = 600f,
                };
                mushroom2.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom2Name + " config: " + error);
            }
        }

        private static void InitMushroom3Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom3Prefab, mushroom3Name, sectionIndex)
                {
                    description = "They taste as vile as they look, you better cook these first!",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.1f,
                    health = 10f,
                    stamina = 10f,
                    eitr = 25f,
                    healthRegen = 1f,
                    burnTime = 600f,
                };
                mushroom3.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom3Name + " config: " + error);
            }
        }

        private static void InitMushroom4Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom4Prefab, mushroom4Name, sectionIndex)
                {
                    description = "They have a nice crunch and taste oddly sweet.",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.1f,
                    health = 30f,
                    stamina = 25f,
                    eitr = 35f,
                    healthRegen = 2f,
                    burnTime = 900f,
                };
                mushroom4.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom4Name + " config: " + error);
            }
        }

        private static void InitMushroom5Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom5Prefab, mushroom5Name, sectionIndex)
                {
                    description = "A distinct violet mushroom with swirls!",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.1f,
                    health = 30f,
                    stamina = 25f,
                    eitr = 55f,
                    healthRegen = 3f,
                    burnTime = 900f,
                };
                mushroom5.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom5Name + " config: " + error);
            }
        }

        private static void InitMushroomSoup1Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom1SoupPrefab, mushroom1SoupName, sectionIndex, mushroom1SoupRecipe)
                {
                    description = $"A soup made of ${mushroom1Name}s",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.2f,
                    health = 35f,
                    stamina = 35f,
                    eitr = 30f,
                    healthRegen = 2f,
                    burnTime = 900f,
                };
                mushroom1Soup.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom1SoupName + " config: " + error);
            }
        }

        private static void InitMushroomSoup2Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom2SoupPrefab, mushroom2SoupName, sectionIndex, mushroom2SoupRecipe)
                {
                    description = $"A soup made of ${mushroom2Name}s",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.2f,
                    health = 18f,
                    stamina = 35f,
                    eitr = 30f,
                    healthRegen = 2f,
                    burnTime = 1500f,
                };
                mushroom2Soup.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom2SoupName + " config: " + error);
            }
        }

        private static void InitMushroomSoup3Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom3SoupPrefab, mushroom3SoupName, sectionIndex, mushroom3SoupRecipe)
                {
                    description = $"A soup made of ${mushroom3Name}s",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.2f,
                    health = 20f,
                    stamina = 20f,
                    eitr = 35f,
                    healthRegen = 2f,
                    burnTime = 1500f,
                };
                mushroom3Soup.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom3SoupName + " config: " + error);
            }
        }

        private static void InitMushroomSoup4Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom4SoupPrefab, mushroom4SoupName, sectionIndex, mushroom4SoupRecipe)
                {
                    description = $"A soup made of ${mushroom4Name}s",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.2f,
                    health = 40f,
                    stamina = 35f,
                    eitr = 45f,
                    healthRegen = 3f,
                    burnTime = 1500f,
                };
                mushroom4Soup.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom4SoupName + " config: " + error);
            }
        }

        private static void InitMushroomSoup5Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom5SoupPrefab, mushroom5SoupName, sectionIndex, mushroom5SoupRecipe)
                {
                    description = $"A soup made of ${mushroom5Name}s",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    weight = 0.2f,
                    health = 40f,
                    stamina = 35f,
                    eitr = 65f,
                    healthRegen = 4f,
                    burnTime = 1500f,
                };
                mushroom5Soup.GenerateConfig(options);
                IncrementSectionIndex();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom5SoupName + " config: " + error);
            }
        }

        private static void IncrementSectionIndex()
        {
            sectionIndex = sectionIndex + 1;
        }
    }
}
