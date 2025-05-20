using ModularMagic_EarthStaffs.Models;
using System;

namespace ModularMagic_EarthStaffs.Configs
{
    internal static class ConfigStaffs
    {
        public static string staff0Name = "The Forest Flinger";
        public static string staff0Recipe = "Wood:20, Stone:20, Resin:20, LeatherScraps:6";
        public static string staff0UpgradeRecipe = "Wood:5, Stone:5, DeerHide:2";
        public static StaffConfig staffEarth0 = new StaffConfig();

        public static string staff1Name = "Tremorbranch";
        public static string staff1Recipe = "RoundLog:20, Stone:20, Resin:20, Thistle:10";
        public static string staff1UpgradeRecipe = "RoundLog:5, Stone:5, Thistle:3";
        public static StaffConfig staffEarth1 = new StaffConfig();

        public static string staff2Name = "Earthwhorl the Avalanche";
        public static string staff2Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1Prefab.name}:1, FineWood:20, Root:10, Guck:10";
        public static string staff2UpgradeRecipe = "FineWood:5, Root:3, Guck:3";
        public static StaffConfig staffEarth2 = new StaffConfig();

        public static string staff3Name = "Oakrend the Earthcaller";
        public static string staff3Recipe = $"{ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2Prefab.name}:1, ElderBark:20, FineWood:20, YmirRemains:5";
        public static string staff3UpgradeRecipe = "ElderBark:5, FineWood:5, YmirRemains:1";
        // public static string staff3CooldownStatusEffectName = "StaffEarth3Cooldown_DW";
        public static StaffConfig staffEarth3 = new StaffConfig();

        private static int sectionIndex = 1;

        public static void Init()
        {
            InitStaffEarth0Config();
            InitStaffEarth1Config();
            InitStaffEarth2Config();
            InitStaffEarth3Config();
        }

        private static void InitStaffEarth0Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth0Prefab, staff0Name, staff0Recipe, staff0UpgradeRecipe, sectionIndex)
                {
                    description = "Held together by nothing but wishful thinking, this staff hurls whatever the forest has lying around. Mushrooms? Sure. Berrie bushes? Why not!",
                    craftingStation = "Workbench",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 20f,
                    damageSpirit = 6f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 10,
                };
                staffEarth0.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff0Name  + " config: " + error);
            }
        }

        private static void InitStaffEarth1Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1Prefab, staff1Name, staff1Recipe, staff1UpgradeRecipe, sectionIndex)
                {
                    description = "The staff equivalent of shouting ‘rock!’ and hoping for the best! Chance for broken limbs: optimal",
                    craftingStation = "Workbench",
                    minStationLevel = 3,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 13f,
                    damageSpirit = 3f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 3,
                };
                staffEarth1.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff1Name  + " config: " + error);
            }
        }

        private static void InitStaffEarth2Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2Prefab, staff2Name, staff2Recipe, staff2UpgradeRecipe, sectionIndex)
                {
                    description = "This staff flings boulders with all the subtlety of an avalanche. Not great for diplomacy. Excellent for everything else!",
                    craftingStation = "Workbench",
                    minStationLevel = 5,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 19f,
                    damageSpirit = 4f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 5,
                };
                staffEarth2.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff2Name  + " config: " + error);
            }
        }

        private static void InitStaffEarth3Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3Prefab, staff3Name, staff3Recipe, staff3UpgradeRecipe, sectionIndex)
                {
                    description = "Side effects may include dizziness, confusion, and being buried under several metric tons of stone. Nature just doesn’t negotiate. It bends, breaks, and buries!",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    recipeMultiplier = 1,
                    maxQuality = 4,
                    movementSpeed = -0.05f,
                    damageBlunt = 25f,
                    damageSpirit = 5f,
                    blockArmor = 48,
                    deflectionForce = 20,
                    attackForce = 35,
                    useEitr = 7,
                };
                staffEarth3.GenerateConfig(options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + staff3Name  + " config: " + error);
            }
        }
    }
}
