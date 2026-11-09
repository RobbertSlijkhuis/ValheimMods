using BepInEx.Bootstrap;

namespace ModularMagic_EarthStaffs.Helpers
{
    /**
     * Detects the mods this one can optionally work with.
     * Only reliable from Awake onwards, and for mods that are a soft BepInDependency of the plugin, so they load first.
     */
    internal static class OptionalModHelper
    {
        public const string FoodGuid = "DeathWizsh.ModularMagic_Food";

        // Prefab name of the Magical Mushroom in ModularMagic_Food
        public const string FoodMagicalMushroom = "MMF_MagicalMushroom";

        // Prefab name of the Gribsnow Mushroom in ModularMagic_Food
        public const string FoodGribSnowMushroom = "MMF_GribSnowMushroom";

        // Prefab name of the Pirced Mushroom in ModularMagic_Food
        public const string FoodPircedMushroom = "MMF_PircedMushroom";

        public static bool IsFoodInstalled => Chainloader.PluginInfos.ContainsKey(FoodGuid);
    }
}
