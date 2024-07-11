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
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffEarth0RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffEarth0.enable.Value : false,

                });
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffEarth1RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffEarth1.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffEarth2RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffEarth2.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffEarth3RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffEarth3.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffFire1RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffFire1.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffFire2RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffFire2.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffFire3RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffFire3.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffFrost2RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffFrost2.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffFrost3RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffFrost3.enable.Value : false,
                });

                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = ConfigStaffs.staffLightning3RecipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = configEnable.Value ? ConfigStaffs.staffLightning3.enable.Value : false,
                });

                //RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                //{
                //    name = ConfigSpellbooks.simpleSpellbookRecipeName,
                //    updateType = RecipeUpdateType.Enable,
                //    enable = configEnable.Value ? ConfigSpellbooks.simpleSpellbookEnable.Value : false,
                //});

                //RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                //{
                //    name = ConfigSpellbooks.advancedSpellbookRecipeName,
                //    updateType = RecipeUpdateType.Enable,
                //    enable = configEnable.Value ? ConfigSpellbooks.advancedSpellbookEnable.Value : false,
                //});

                //RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                //{
                //    name = ConfigSpellbooks.masterSpellbookRecipeName,
                //    updateType = RecipeUpdateType.Enable,
                //    enable = configEnable.Value ? ConfigSpellbooks.masterSpellbookEnable.Value : false,
                //});
            };

            ConfigMaterials.Init();
            ConfigStaffs.Init();
            ConfigSpellbooks.Init();
        }
    }
}
