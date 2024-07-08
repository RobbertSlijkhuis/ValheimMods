using BepInEx.Configuration;
using MagicExtended.Helpers;
using MagicExtended.Models;

namespace MagicExtended.Configs
{
    internal static class ConfigPlugin
    {
        public static string sectionGeneral = "01. General";
        public static ConfigEntry<bool> configEnable;

        // General options
        public static string[] craftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };

        public static void Init()
        {
            ConfigFile Config = MagicExtended.Instance.Config;

            // General
            configEnable = Config.Bind(new ConfigDefinition(sectionGeneral, "Enable"), true,
                new ConfigDescription("Enable this mod", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configEnable.SettingChanged += (obj, attr) => {
                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffEarth1RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffEarth1Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffEarth2RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffEarth2Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffEarth3RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffEarth3Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffFire1RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffFire1Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffFire2RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffFire2Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffFire3RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffFire3Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffFrost2RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffFrost2Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffFrost3RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffFrost3Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigStaffs.staffLightning3RecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigStaffs.staffLightning3Enable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigSpellbooks.simpleSpellbookRecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigSpellbooks.simpleSpellbookEnable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigSpellbooks.advancedSpellbookRecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigSpellbooks.advancedSpellbookEnable.Value : false,
                });

                RecipeHelper.PatchRecipe(new PatchRecipeOptions()
                {
                    name = ConfigSpellbooks.masterSpellbookRecipeName,
                    updateType = RecipeUpdateType.Enable,
                    enable = configEnable.Value ? ConfigSpellbooks.masterSpellbookEnable.Value : false,
                });
            };

            ConfigMaterials.Init();
            ConfigStaffs.Init();
            ConfigSpellbooks.Init();
        }
    }
}
