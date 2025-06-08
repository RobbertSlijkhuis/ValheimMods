using BepInEx.Configuration;
using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Models;
using ModularMagic_Armors.StatusEffects;
using ModularMagic_Armors.Types;
using ModularMagic_Utilities.Types;

namespace ModularMagic_Armors.Configs
{
    internal class ArmorConfig
    {
        // General options
        public static string[] craftingStationOptions = new string[] { 
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter,
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable };

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

        public ConfigurationManagerAttributes enableAttributes;
        public ConfigurationManagerAttributes nameAttributes;
        public ConfigurationManagerAttributes descriptionAttributes;
        public ConfigurationManagerAttributes craftingStationAttributes;
        public ConfigurationManagerAttributes minStationLevelAttributes;
        public ConfigurationManagerAttributes recipeAttributes;
        public ConfigurationManagerAttributes recipeUpgradeAttributes;
        public ConfigurationManagerAttributes recipeMultiplierAttributes;
        public ConfigurationManagerAttributes armorAttributes;
        public ConfigurationManagerAttributes armorPerLevelAttributes;
        public ConfigurationManagerAttributes weightAttributes;
        public ConfigurationManagerAttributes maxDurabilityAttributes;
        public ConfigurationManagerAttributes maxQualityAttributes;
        public ConfigurationManagerAttributes movementSpeedAttributes;
        public ConfigurationManagerAttributes eitrAttributes;
        public ConfigurationManagerAttributes eitrRegenAttributes;
        public ConfigurationManagerAttributes elementalMagicAttributes;
        public ConfigurationManagerAttributes bloodMagicAttributes;

        // Other
        public string cooldownStatusEffectName;
        public string magicStatusEffectName;
        private int entryCount = 18;

        public ArmorConfig(bool browsable = true)
        {
            enableAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            nameAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            descriptionAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            craftingStationAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            minStationLevelAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            recipeAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            recipeUpgradeAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            recipeMultiplierAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            armorAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            armorPerLevelAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            weightAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            maxDurabilityAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            maxQualityAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            movementSpeedAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            eitrAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            eitrRegenAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            elementalMagicAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
            bloodMagicAttributes = new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder(), Browsable = browsable };
        }

        public void GenerateConfig(ArmorConfigOptions options, bool browsable = true)
        {
            ConfigFile Config = ModularMagic_Armors.Instance.Config;
            cooldownStatusEffectName = options.cooldownStatusEffectName;
            magicStatusEffectName = options.magicStatusEffectName;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, "Enable"), options.enable,
                new ConfigDescription("Wether the recipe for this item is enabled", null,
                enableAttributes));
            enable.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogInfo(options.recipeName);
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    prefab = options.prefab,
                    name = options.recipeName,
                    updateType = RecipeUpdateType.ENABLE,
                    enable = enable.Value,
                });
            };

            name = Config.Bind(new ConfigDefinition(options.sectionName, "Name"), options.name,
                new ConfigDescription("The name of the item", null,
                nameAttributes));
            name.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    name = name.Value,
                });
            };

            description = Config.Bind(new ConfigDefinition(options.sectionName, "Description"), options.description,
                new ConfigDescription("The description of the item", null,
                descriptionAttributes));
            description.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    description = description.Value,
                });
            }; ;

            craftingStation = Config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be crafted in",
                new AcceptableValueList<string>(craftingStationOptions),
                craftingStationAttributes));
            craftingStation.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    prefab = options.prefab,
                    name = options.recipeName,
                    updateType = RecipeUpdateType.CRAFTINGSTATION,
                    craftingStation = craftingStation.Value,
                });
            };

            minStationLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Required station level"), options.minStationLevel,
                new ConfigDescription("The required station level to craft this item", null,
                minStationLevelAttributes));
            minStationLevel.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    prefab = options.prefab,
                    name = options.recipeName,
                    updateType = RecipeUpdateType.MINREQUIREDSTATIONLEVEL,
                    requiredStationLevel = minStationLevel.Value,
                });
            };

            recipe = Config.Bind(new ConfigDefinition(options.sectionName, "Recipe"), options.recipe,
                new ConfigDescription("The items required to craft this item", null,
                recipeAttributes));
            recipe.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    prefab = options.prefab,
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                });
            };

            recipeUpgrade = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade recipe"), options.recipeUpgrade,
                new ConfigDescription("The items required to upgrade this item", null,
                recipeUpgradeAttributes));
            recipeUpgrade.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    prefab = options.prefab,
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                    upgradeRequirements = recipeUpgrade.Value,
                    upgradeMultiplier = recipeMultiplier.Value,
                });
            };

            recipeMultiplier = Config.Bind(new ConfigDefinition(options.sectionName, "Upgrade recipe multiplier"), options.recipeMultiplier,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                recipeMultiplierAttributes));
            recipeMultiplier.SettingChanged += (obj, attr) =>
            {
                RecipeHelper.UpdateRecipe(new UpdateRecipeOptions()
                {
                    prefab = options.prefab,
                    name = options.recipeName,
                    updateType = RecipeUpdateType.RECIPE,
                    requirements = recipe.Value,
                    upgradeRequirements = recipeUpgrade.Value,
                    upgradeMultiplier = recipeMultiplier.Value,
                });
            };

            armor = Config.Bind(new ConfigDefinition(options.sectionName, "Armor"), options.armor,
                new ConfigDescription("The armor of the item", null,
                armorAttributes));
            armor.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    armor = armor.Value,
                });
            };

            armorPerLevel = Config.Bind(new ConfigDefinition(options.sectionName, "Armor per level"), options.armorPerLevel,
                new ConfigDescription("The armor per level of the item", null,
                armorPerLevelAttributes));
            armorPerLevel.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    armorPerLevel = armorPerLevel.Value,
                });
            };

            weight = Config.Bind(new ConfigDefinition(options.sectionName, "Weight"), options.weight,
                new ConfigDescription("The weight of the item", null,
                weightAttributes));
            weight.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    weight = weight.Value,
                });
            };

            maxDurability = Config.Bind(new ConfigDefinition(options.sectionName, "Max durability"), options.maxDurability,
                new ConfigDescription("The maximum durability of the item", null,
                maxDurabilityAttributes));
            maxDurability.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    maxDurability = maxDurability.Value,
                });
            };

            maxQuality = Config.Bind(new ConfigDefinition(options.sectionName, "Max quality"), options.maxQuality,
                new ConfigDescription("The maximum quality the item can become", null,
                maxQualityAttributes));
            maxQuality.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    maxQuality = maxQuality.Value,
                });
            };

            movementSpeed = Config.Bind(new ConfigDefinition(options.sectionName, "Movement speed"), options.movementSpeed,
                new ConfigDescription("The movement speed stat on the item (example: 1 = 100% or 0.05 = 5% etc.)", null,
                movementSpeedAttributes));
            movementSpeed.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    movementSpeed = movementSpeed.Value,
                });
            };

            eitr = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr"), options.eitr,
                new ConfigDescription("The amount of eitr the item gives", null,
                eitrAttributes));
            eitr.SettingChanged += (obj, attr) =>
            {
                if (eitr.Value < 0f) return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetEitr(eitr.Value);
            };

            eitrRegen = Config.Bind(new ConfigDefinition(options.sectionName, "Eitr regen"), options.eitrRegen,
                new ConfigDescription("The amount of eitr regen the item gives (example: 1 = 100% or 0.05 = 5% etc.)", null,
                eitrRegenAttributes));
            eitrRegen.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemDropStats(options.prefab, new UpdateItemDropStatsOptions()
                {
                    eitrRegen = eitrRegen.Value,
                });

                UpdateHelper.UpdateEitrRegenOnPlayer(options.name, eitrRegen.Value);
            };

            elementalMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Elemental magic"), options.elementalMagic,
                new ConfigDescription("The amount of Elemental magic skill the item gives", null,
                elementalMagicAttributes));
            elementalMagic.SettingChanged += (obj, attr) =>
            {
                if (elementalMagic.Value < 0f)
                    return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetElementalMagic(elementalMagic.Value);
            };

            bloodMagic = Config.Bind(new ConfigDefinition(options.sectionName, "Blood magic"), options.bloodMagic,
                new ConfigDescription("The amount of Blood magic skill the item gives", null,
                bloodMagicAttributes));
            bloodMagic.SettingChanged += (obj, attr) =>
            {
                if (bloodMagic.Value < 0f)
                    return;

                ItemDrop itemDrop = options.prefab.GetComponent<ItemDrop>();
                MagicStatusEffect statusEffect = (MagicStatusEffect)itemDrop.m_itemData.m_shared.m_equipStatusEffect;
                statusEffect.SetBloodMagic(bloodMagic.Value);
            };
        }
        public void ShowConfig(bool value, bool refreshConfigManager = false)
        {
            enableAttributes.Browsable = value;
            nameAttributes.Browsable = value;
            descriptionAttributes.Browsable = value;
            craftingStationAttributes.Browsable = value;
            minStationLevelAttributes.Browsable = value;
            recipeAttributes.Browsable = value;
            recipeUpgradeAttributes.Browsable = value;
            recipeMultiplierAttributes.Browsable = value;
            armorAttributes.Browsable = value;
            armorPerLevelAttributes.Browsable = value;
            weightAttributes.Browsable = value;
            maxDurabilityAttributes.Browsable = value;
            maxQualityAttributes.Browsable = value;
            movementSpeedAttributes.Browsable = value;
            eitrAttributes.Browsable = value;
            eitrRegenAttributes.Browsable = value;
            elementalMagicAttributes.Browsable = value;
            bloodMagicAttributes.Browsable = value;

            if (refreshConfigManager)
                ModularMagic_Armors.Instance.RefreshConfigManager();
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
