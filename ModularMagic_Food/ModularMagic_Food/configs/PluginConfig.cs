using ModularMagic_Food.Models;
using System;
using System.Threading.Tasks;

namespace ModularMagic_Food.Configs
{
    internal static class PluginConfig
    {
        public static string mushroom1Name = "Magical Mushroom";
        public static FoodConfig mushroom1 = new FoodConfig();

        public static string mushroom1CookedName = $"Cooked {mushroom1Name}";
        public static FoodConfig mushroom1Cooked = new FoodConfig();

        public static string mushroom2Name = "Gribsnow Mushroom";
        public static FoodConfig mushroom2 = new FoodConfig();

        public static string mushroom3Name = "Bog Mushroom";
        public static FoodConfig mushroom3 = new FoodConfig();

        public static string mushroom4Name = "Pirced Mushroom";
        public static FoodConfig mushroom4 = new FoodConfig();

        public static string mushroom5Name = "Violet Cloud Mushroom";
        public static FoodConfig mushroom5 = new FoodConfig();

        public static string mushroom1SoupName = "Magical Mushroom Soup";
        public static string mushroom1SoupRecipe = $"{ModularMagic_Food.Instance.prefabs.mushroom1Prefab.name}:3";
        public static FoodConfig mushroom1Soup = new FoodConfig();

        public static string mushroom2SoupName = "Gribsnow Mushroom Soup";
        public static string mushroom2SoupRecipe = $"Carrot:2, {ModularMagic_Food.Instance.prefabs.mushroom2Prefab.name}:2";
        public static FoodConfig mushroom2Soup = new FoodConfig();

        public static string mushroom3SoupName = "Bog Mushroom Soup";
        public static string mushroom3SoupRecipe = $"Turnip:2, {ModularMagic_Food.Instance.prefabs.mushroom3Prefab.name}:2";
        public static FoodConfig mushroom3Soup = new FoodConfig();

        public static string mushroom4SoupName = "Pirced Mushroom Soup";
        public static string mushroom4SoupRecipe = $"Onion:2, {ModularMagic_Food.Instance.prefabs.mushroom4Prefab.name}:2";
        public static FoodConfig mushroom4Soup = new FoodConfig();

        public static string mushroom5SoupName = "Violet Cloud Mushroom Soup";
        public static string mushroom5SoupRecipe = $"Cloudberry:3, {ModularMagic_Food.Instance.prefabs.mushroom5Prefab.name}:2";
        public static FoodConfig mushroom5Soup = new FoodConfig();

        public static string jerky1Name = "Seeker Jerky";
        public static string jerky1Recipe = $"BugMeat:1, Honey:2";
        public static FoodConfig jerky1 = new FoodConfig();

        public static void Init()
        {
            InitMushroom1Config();
            InitMushroom2Config();
            InitMushroom3Config();
            InitMushroom4Config();
            InitMushroom5Config();
            InitMushroom1CookedConfig();
            InitMushroomSoup1Config();
            InitMushroomSoup2Config();
            InitMushroomSoup3Config();
            InitMushroomSoup4Config();
            InitMushroomSoup5Config();
            InitJerky1Config();
        }

        private static void InitMushroom1Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom1Prefab, mushroom1Name, null, ModularMagic_Food.Instance.prefabs.mushroom1PickablePrefab)
                {
                    description = "An oddly blue colored mushroom with a soft glow.",
                    health = 15f,
                    stamina = 15f,
                    eitr = 15f,
                    healthRegen = 1f,
                    burnTime = 600f,
                    groupMin = 3,
                    groupMax = 5,
                    scaleMin = 1f,
                    scaleMax = 1.5f,
                    minAltitude = 0f,
                    maxAltitude = 1000f,
                    amount = 1,
                    respawnTimeMinutes = 240f,
                };
                mushroom1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom1Name + " config: " + error);
            }
        }

        private static void InitMushroom2Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom2Prefab, mushroom2Name, null, ModularMagic_Food.Instance.prefabs.mushroom2PickablePrefab)
                {
                    description = "The green glow is natural. The nausea isn’t!",
                    health = 17f,
                    stamina = 17f,
                    eitr = 17f,
                    healthRegen = 1f,
                    burnTime = 600f,
                    groupMin = 2,
                    groupMax = 4,
                    scaleMin = 1f,
                    scaleMax = 1.5f,
                    minAltitude = 0f,
                    maxAltitude = 1000f,
                    amount = 1,
                    respawnTimeMinutes = 240f,
                };
                mushroom2.GenerateConfig(options);
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
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom3Prefab, mushroom3Name, null, ModularMagic_Food.Instance.prefabs.mushroom3PickablePrefab)
                {
                    description = "They taste as vile as they look, you better cook these first!",
                    health = 19f,
                    stamina = 19f,
                    eitr = 19f,
                    healthRegen = 1f,
                    burnTime = 900f,
                    groupMin = 2,
                    groupMax = 5,
                    scaleMin = 1f,
                    scaleMax = 1f,
                    minAltitude = 0f,
                    maxAltitude = 1000f,
                    amount = 1,
                    respawnTimeMinutes = 240f,
                };
                mushroom3.GenerateConfig(options);
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
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom4Prefab, mushroom4Name, null, ModularMagic_Food.Instance.prefabs.mushroom4PickablePrefab)
                {
                    description = "They have a nice crunch and taste oddly sweet.",
                    health = 22f,
                    stamina = 22f,
                    eitr = 22,
                    healthRegen = 1f,
                    burnTime = 900f,
                    groupMin = 2,
                    groupMax = 5,
                    scaleMin = 0.7f, 
                    scaleMax = 1f,
                    minAltitude = 0f,
                    maxAltitude = 1000f,
                    amount = 1,
                    respawnTimeMinutes = 240f,
                };
                mushroom4.GenerateConfig(options);
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
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom5Prefab, mushroom5Name, null, ModularMagic_Food.Instance.prefabs.mushroom5PickablePrefab)
                {
                    description = "A distinct violet mushroom with a swirling cap pulsing with latent magic!",
                    health = 25f,
                    stamina = 25f,
                    eitr = 25f,
                    healthRegen = 1f,
                    burnTime = 900f,
                    groupMin = 2,
                    groupMax = 5,
                    scaleMin = 0.7f,
                    scaleMax = 1f,
                    minAltitude = 0f,
                    maxAltitude = 1000f,
                    amount = 1,
                    respawnTimeMinutes = 240f,
                };
                mushroom5.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom5Name + " config: " + error);
            }
        }

        private static void InitMushroom1CookedConfig()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom1CookedPrefab, mushroom1CookedName)
                {
                    description = "Saturated with raw magic and a nice sear. Consuming it may open your mind... or melt it.",
                    health = 20f,
                    stamina = 20f,
                    eitr = 25f,
                    healthRegen = 1f,
                    burnTime = 900f,
                };
                mushroom1Cooked.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom1CookedName + " config: " + error);
            }
        }

        private static void InitMushroomSoup1Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom1SoupPrefab, mushroom1SoupName, mushroom1SoupRecipe)
                {
                    description = "A warm tasty soup made of mostly of " + mushroom1Name + "s.",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    maxStackSize = 10,
                    health = 15f,
                    stamina = 10f,
                    eitr = 30f,
                    healthRegen = 2f,
                    burnTime = 1200,
                };
                mushroom1Soup.GenerateConfig(options);
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
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom2SoupPrefab, mushroom2SoupName, mushroom2SoupRecipe)
                {
                    description = "A warm tasty soup made of mostly of " + mushroom2Name + "s.",
                    craftingStation = "Cauldron",
                    minStationLevel = 1,
                    maxStackSize = 10,
                    health = 16f,
                    stamina = 10f,
                    eitr = 40f,
                    healthRegen = 2f,
                    burnTime = 1200,
                };
                mushroom2Soup.GenerateConfig(options);
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
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom3SoupPrefab, mushroom3SoupName, mushroom3SoupRecipe)
                {
                    description = "A warm tasty soup made of mostly of " + mushroom3Name + "s.",
                    craftingStation = "Cauldron",
                    minStationLevel = 2,
                    maxStackSize = 10,
                    health = 18f,
                    stamina = 10f,
                    eitr = 50f,
                    healthRegen = 2f,
                    burnTime = 1500f,
                };
                mushroom3Soup.GenerateConfig(options);
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
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom4SoupPrefab, mushroom4SoupName, mushroom4SoupRecipe)
                {
                    description = "A warm tasty soup made of mostly of " + mushroom4Name + "s.",
                    craftingStation = "Cauldron",
                    minStationLevel = 3,
                    maxStackSize = 10,
                    health = 20f,
                    stamina = 10f,
                    eitr = 60f,
                    healthRegen = 3f,
                    burnTime = 1500f,
                };
                mushroom4Soup.GenerateConfig(options);
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
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.mushroom5SoupPrefab, mushroom5SoupName, mushroom5SoupRecipe)
                {
                    description = "A warm tasty soup made of mostly of " + mushroom5Name + "s.",
                    craftingStation = "Cauldron",
                    minStationLevel = 4,
                    maxStackSize = 10,
                    health = 23f,
                    stamina = 12f,
                    eitr = 70f,
                    healthRegen = 3f,
                    burnTime = 1500f,
                };
                mushroom5Soup.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + mushroom5SoupName + " config: " + error);
            }
        }

        private static void InitJerky1Config()
        {
            try
            {
                FoodConfigOptions options = new FoodConfigOptions(ModularMagic_Food.Instance.prefabs.jerky1Prefab, jerky1Name, jerky1Recipe)
                {
                    description = "Lean and salty... its not bug I swear!",
                    craftingStation = "Cauldron",
                    minStationLevel = 5,
                    maxStackSize = 20,
                    health = 53f,
                    stamina = 53f,
                    eitr = 0f,
                    healthRegen = 3f,
                    burnTime = 1800f,
                };
                jerky1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + jerky1Name + " config: " + error);
            }
        }
    }
}
