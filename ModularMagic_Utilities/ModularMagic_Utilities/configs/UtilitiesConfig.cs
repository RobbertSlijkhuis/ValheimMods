using BepInEx.Configuration;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
//using UnityEngine;

namespace ModularMagic_Utilities.Configs
{
    internal class UtilitiesConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };
        public static string[] lanterColorOptions = new string[] { "Red", "Orange", "Yellow", "Green", "LemonGreen", "LightBlue", "Blue", "Pink", "Purple", "White" };

        // The  fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<int> minStationLevel;
        public ConfigEntry<string> recipe;
        public ConfigEntry<float> weight;
        public ConfigEntry<float> eitr;
        public ConfigEntry<float> eitrRegen;
        public ConfigEntry<float> elementalMagic;
        public ConfigEntry<float> bloodMagic;
        public ConfigEntry<float> demister;
        //public ConfigEntry<Color> flareColor;
        //public ConfigEntry<Color> lightColor;
        public ConfigEntry<string> lightColorPreset;
        public ConfigEntry<float> lightIntensity;
        public ConfigEntry<float> lightRange;
        //public ConfigEntry<bool> enableCustomGlassColor;
        //public ConfigEntry<string> customGlassColor;

        // Other
        public string cooldownStatusEffectName;
        public string magicStatusEffectName;

        public void GenerateConfig(UtilitiesConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Utilities.Instance.Config;
            cooldownStatusEffectName = options.cooldownStatusEffectName;
            magicStatusEffectName = options.magicStatusEffectName;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Item: Enable"), options.enable,
               new ConfigDescription("Wether the recipe for this item is enabled" + name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true }));
            enable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = enable.Value,
                });
            };

            name = Config.Bind(new ConfigDefinition(options.sectionName, "Item: Name"), options.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Item: Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = description.Value,
                });
            }; ;

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe: Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            craftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.CRAFTINGSTATION,
                    craftingStation = craftingStation.Value,
                });
            };

            minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe: Required station level"), options.minStationLevel,
                new ConfigDescription("The required station level to craft this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            minStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = minStationLevel.Value,
                });
            };

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe"), options.recipe,
                new ConfigDescription("The items required to craft this item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            recipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                });
            };

            weight = Config.Bind(new ConfigDefinition(options.sectionName, "Weight"), options.weight,
                new ConfigDescription("The weight applied to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            weight.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    weight = weight.Value,
                });
            };

            eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Stat: Eitr"), options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            eitr.SettingChanged += (obj, attr) =>
            {
                if (eitr.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetEitr(eitr.Value);
            };

            eitrRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Stat: Eitr regen"), options.eitrRegen,
                new ConfigDescription("The amount of eitr regen the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            eitrRegen.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogWarning("eitreRegen: " + eitrRegen.Value);
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    eitrRegen = eitrRegen.Value,
                });

                UpdateHelper.UpdateEitrRegenOnPlayer(options.name, eitrRegen.Value);
            };

            elementalMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Skill: Elemental magic"), options.elementalMagic,
                new ConfigDescription("The amount of Elemental magic skill the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            elementalMagic.SettingChanged += (obj, attr) =>
            {
                if (elementalMagic.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetElementalMagic(elementalMagic.Value);
            };

            bloodMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Skill: Blood magic"), options.bloodMagic,
                new ConfigDescription("The amount of Blood magic skill the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            bloodMagic.SettingChanged += (obj, attr) =>
            {
                if (bloodMagic.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetBloodMagic(bloodMagic.Value);
            };

            if (options.demister != null)
            {
                demister = Config.Bind(new ConfigDefinition(options.sectionName, "Stat: Demister effect range"), (float)options.demister,
                    new ConfigDescription("The range of the demister effect (push mist away). 0 disables this effect, maximum 50 for performance reasons", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                demister.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateDemisterOnBoth(options.prefab, demister.Value);
                };
            }

            //if (options.flareColor != null)
            //{
            //    flareColor = Config.Bind(new ConfigDefinition(options.sectionName, "Lantern: Flare color"), (Color)options.flareColor,
            //        new ConfigDescription("The color of the flare effect around the lantern (This will be updated when you turn off/on the lantern)", null,
            //        new ConfigurationManagerAttributes { IsAdminOnly = true }));
            //    flareColor.SettingChanged += (obj, attr) =>
            //    {
            //        Jotunn.Logger.LogWarning("New flare color: " + flareColor.Value);
            //    };
            //}

            //if (options.lightColor != null)
            //{
            //    lightColor = Config.Bind(new ConfigDefinition(options.sectionName, "Lantern: Light Color"), (Color)options.lightColor,
            //        new ConfigDescription("The color of the light emitted by the lantern (This will be updated when you turn off/on the lantern)", null,
            //        new ConfigurationManagerAttributes { IsAdminOnly = true }));
            //    lightColor.SettingChanged += (obj, attr) =>
            //    {
            //        Jotunn.Logger.LogWarning("New color: " + lightColor.Value);
            //    };
            //}

            if (options.lightColorPreset != null)
            {
                lightColorPreset = Config.Bind(new ConfigDefinition(options.sectionName, "Lantern: Color preset"), options.lightColorPreset,
                    new ConfigDescription("A preset of light, flare and glass color (This will be updated when you turn off/on the lantern)",
                    new AcceptableValueList<string>(lanterColorOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                lightColorPreset.SettingChanged += (obj, attr) =>
                {
                    Jotunn.Logger.LogWarning("New light color preset: " + lightColorPreset.Value);
                };
            }

            if (options.lightRange != null)
            {
                lightRange = Config.Bind(new ConfigDefinition(options.sectionName, "Lantern: Light Range"), (float)options.lightRange,
                    new ConfigDescription("The range of the light emitted by the lantern (This will be updated when you turn off/on the lantern)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            }

            if (options.lightIntensity != null)
            {
                lightIntensity = Config.Bind(new ConfigDefinition(options.sectionName, "Lantern: Light Intensity"), (float)options.lightIntensity,
                    new ConfigDescription("The intensity of the light emitted by the lantern (This will be updated when you turn off/on the lantern)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            }

            //if (options.enableCustomGlassColor != null)
            //{
            //    enableCustomGlassColor = Config.Bind(new ConfigDefinition(options.sectionName, "Lantern custom: Enable custom glass color"), (bool)options.enableCustomGlassColor,
            //        new ConfigDescription("Apply the custom glass color instead", null,
            //        new ConfigurationManagerAttributes { IsAdminOnly = true }));
            //    enableCustomGlassColor.SettingChanged += (obj, attr) =>
            //    {
            //        Jotunn.Logger.LogWarning("New material color: " + enableCustomGlassColor.Value);
            //    };
            //}

            //if (options.customGlassColor != null)
            //{
            //    customGlassColor = Config.Bind(new ConfigDefinition(options.sectionName, "Lantern custom: Glass color"), options.customGlassColor,
            //        new ConfigDescription("A custom glass color from a RGB float/unit string USE AT OWN RISK (This will be updated when you turn off/on the lantern)", null,
            //        new ConfigurationManagerAttributes { IsAdminOnly = true }));
            //    customGlassColor.SettingChanged += (obj, attr) =>
            //    {
            //        Jotunn.Logger.LogWarning("New material color: " + customGlassColor.Value);
            //    };
            //}
        }
    }
}
