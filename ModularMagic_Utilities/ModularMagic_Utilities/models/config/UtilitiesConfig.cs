using BepInEx.Configuration;
using ModularMagic_Utilities.helpers;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.StatusEffects;
using UnityEngine;

namespace ModularMagic_Utilities.Models
{
    internal class UtilitiesConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };

        // The  fields to generate
        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<int> minStationLevel;
        public ConfigEntry<string> recipe;
        public ConfigEntry<float> eitr;
        public ConfigEntry<float> eitrRegen;
        public ConfigEntry<float> elementalMagic;
        public ConfigEntry<float> bloodMagic;
        public ConfigEntry <int> demister;

        // Other
        public string cooldownStatusEffectName;
        public string magicStatusEffectName;

        public void GenerateConfig(UtilitiesConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Utilities.Instance.Config;
            cooldownStatusEffectName = options.cooldownStatusEffectName;
            magicStatusEffectName = options.magicStatusEffectName;

            this.enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), (bool)options.enable,
               new ConfigDescription("Enable " + this.name, null,
               new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.enable.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = this.enable.Value,
                });
            };

            this.name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.name.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = this.name.Value,
                });
            };

            this.description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.description.SettingChanged += (obj, attr) =>
            {
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = this.description.Value,
                });
            }; ;

            this.craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.craftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.CRAFTINGSTATION,
                    craftingStation = this.craftingStation.Value,
                });
            };

            this.minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level to craft"), (int)options.minStationLevel,
                new ConfigDescription("The required station level to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.minStationLevel.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogWarning("demister: " + this.minStationLevel.Value);
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = this.minStationLevel.Value,
                });
            };

            this.recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting costs"), options.recipe,
                new ConfigDescription("The items required to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.recipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = this.recipe.Value,
                });
            };

            this.eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr"), (float)options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.eitr.SettingChanged += (obj, attr) =>
            {
                if (this.eitr.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetEitr(this.eitr.Value);
            };

            this.eitrRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr regen"), (float)options.eitrRegen,
                new ConfigDescription("The amount of eitr regen the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.eitrRegen.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogWarning("eitreRegen: " + this.eitrRegen.Value);
                ConfigHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    eitrRegen = this.eitrRegen.Value,
                });

                UpdateHelper.updateEitrRegenOnPlayer(options.name, this.eitrRegen.Value);
            };

            this.elementalMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Elemental magic"), (float)options.elementalMagic,
                new ConfigDescription("The amount of Elemental magic the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.elementalMagic.SettingChanged += (obj, attr) =>
            {
                if (this.elementalMagic.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetElementalMagic(this.elementalMagic.Value);
            };

            this.bloodMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Blood magic"), (float)options.bloodMagic,
                new ConfigDescription("The amount of Blood magic the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            this.bloodMagic.SettingChanged += (obj, attr) =>
            {
                if (this.bloodMagic.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetBloodMagic(this.bloodMagic.Value);
            };

            if (options.demister != null)
            {
                this.demister = Config.Bind(new ConfigDefinition(options.sectionName, "Demister effect range"), (int)options.demister,
                    new ConfigDescription("The range of the demister effect (push mist away), 0 disables this effect", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                this.demister.SettingChanged += (obj, attr) =>
                {
                    Jotunn.Logger.LogWarning("demister: " + this.demister.Value);
                    UpdateHelper.updateDemisterOnBoth(options.prefab, this.demister.Value);
                };
            }
        }
    }
}
