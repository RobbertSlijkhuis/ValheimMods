using BepInEx.Configuration;
using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Models;
using ModularMagic_Armors.StatusEffects;

namespace ModularMagic_Armors.Configs
{
    internal class ArmorConfig
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
        public ConfigEntry<string> recipeUpgrade;
        public ConfigEntry<int> recipeMultiplier;
        public ConfigEntry<float> armor;
        public ConfigEntry<float> armorPerLevel;
        public ConfigEntry<float> weight;
        public ConfigEntry<float> maxDurability;
        public ConfigEntry<int> maxQuality;
        public ConfigEntry<float> movementSpeed;
        public ConfigEntry<float> eitr;
        public ConfigEntry<float> eitrRegen;
        public ConfigEntry<float> elementalMagic;
        public ConfigEntry<float> bloodMagic;
        public ConfigEntry<int> demister;

        // Other
        public string cooldownStatusEffectName;
        public string magicStatusEffectName;

        public void GenerateConfig(ArmorConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Armors.Instance.Config;
            cooldownStatusEffectName = options.cooldownStatusEffectName;
            magicStatusEffectName = options.magicStatusEffectName;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
               new ConfigDescription("Enable " + name, null,
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

            name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
              new ConfigDescription("The name given to the item", null,
              new ConfigurationManagerAttributes { IsAdminOnly = true }));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = description.Value,
                });
            }; ;

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
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

            minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level to craft"), options.minStationLevel,
                new ConfigDescription("The required station level to craft", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            minStationLevel.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogWarning("demister: " + minStationLevel.Value);
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = minStationLevel.Value,
                });
            };

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting costs"), options.recipe,
                new ConfigDescription("The items required to craft", null,
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

            recipeUpgrade = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade costs"), options.recipeUpgrade,
                new ConfigDescription("The costs to upgrade the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            recipeUpgrade.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                    upgradeRequirements = recipeUpgrade.Value,
                    upgradeMultiplier = recipeMultiplier.Value,
                });
            };

            recipeMultiplier = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade multiplier"), options.recipeMultiplier,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            recipeMultiplier.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                    upgradeRequirements = recipeUpgrade.Value,
                    upgradeMultiplier = recipeMultiplier.Value,
                });
            };

            armor = Config.Bind(new ConfigDefinition(options.sectionName, "Armor"), options.armor,
                new ConfigDescription("The armor given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            armor.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    armor = armor.Value,
                });
            };

            armorPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Armor per level"), options.armorPerLevel,
                new ConfigDescription("The armor per level given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            armorPerLevel.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    armorPerLevel = armorPerLevel.Value,
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

            maxDurability = Config.Bind(new ConfigDefinition(options.sectionName, "Max durability"), options.maxDurability,
                new ConfigDescription("The maximum durability applied to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            maxDurability.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    maxDurability = maxDurability.Value,
                });
            };

            maxQuality = Config.Bind(new ConfigDefinition(options.sectionName, "Max quality"), options.maxQuality,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            maxQuality.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    maxQuality = maxQuality.Value,
                });
            };

            movementSpeed = Config.Bind(new ConfigDefinition(options.sectionName, "Movement speed"), options.movementSpeed,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            movementSpeed.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    movementSpeed = movementSpeed.Value,
                });
            };

            eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr"), options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            eitr.SettingChanged += (obj, attr) =>
            {
                if (eitr.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetEitr(eitr.Value);
            };

            eitrRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr regen"), options.eitrRegen,
                new ConfigDescription("The amount of eitr regen the item has", null,
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

            elementalMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Elemental magic"), options.elementalMagic,
                new ConfigDescription("The amount of Elemental magic the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            elementalMagic.SettingChanged += (obj, attr) =>
            {
                if (elementalMagic.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetElementalMagic(elementalMagic.Value);
            };

            bloodMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Blood magic"), options.bloodMagic,
                new ConfigDescription("The amount of Blood magic the item has", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            bloodMagic.SettingChanged += (obj, attr) =>
            {
                if (bloodMagic.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetBloodMagic(bloodMagic.Value);
            };
        }
    }
}
