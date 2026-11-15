using BepInEx.Configuration;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;

namespace ModularMagic_Core.Configs
{
    /// <summary>
    /// The config of one staff of a staff mod: its recipe and its stats. The stats are bound only when the options give a value for them, so
    /// each staff has the entries it needs. A changed value is applied to the prefab of the staff, and (through the SettingChanged hook of
    /// the plugin) to the staff in the hand.
    /// </summary>
    public class StaffConfig
    {
        public static string[] craftingStationOptions = new string[] {
            CraftingStationType.None, CraftingStationType.Disabled, CraftingStationType.Workbench, CraftingStationType.Forge, CraftingStationType.Stonecutter,
            CraftingStationType.Cauldron, CraftingStationType.ArtisanTable, CraftingStationType.BlackForge, CraftingStationType.GaldrTable };

        public ConfigEntry<bool> enable;
        public ConfigEntry<string> name;
        public ConfigEntry<string> description;
        public ConfigEntry<string> craftingStation;
        public ConfigEntry<int> minStationLevel;
        public ConfigEntry<string> recipe;
        public ConfigEntry<string> recipeUpgrade;
        public ConfigEntry<int> recipeMultiplier;
        public ConfigEntry<int> maxQuality;
        public ConfigEntry<float> weight;
        public ConfigEntry<float> maxDurability;
        public ConfigEntry<float> movementSpeed;
        public ConfigEntry<float> damageBlunt;
        public ConfigEntry<float> damageBluntPerLevel;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damageChopPerLevel;
        public ConfigEntry<float> damageFire;
        public ConfigEntry<float> damageFrost;
        public ConfigEntry<float> damageLightning;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> damagePickaxePerLevel;
        public ConfigEntry<float> damagePierce;
        public ConfigEntry<float> damagePiercePerLevel;
        public ConfigEntry<float> damagePoison;
        public ConfigEntry<float> damagePoisonPerLevel;
        public ConfigEntry<float> damageSlash;
        public ConfigEntry<float> damageSlashPerLevel;
        public ConfigEntry<float> damageSpirit;
        public ConfigEntry<float> damageSpiritPerLevel;
        public ConfigEntry<int> blockArmor;
        public ConfigEntry<int> deflectionForce;
        public ConfigEntry<int> attackForce;
        public ConfigEntry<float> backStab;
        public ConfigEntry<int> useEitr;
        public ConfigEntry<int> attackStamina;
        public ConfigEntry<float> projectileVelocity;
        public ConfigEntry<float> projectileAccuracy;
        public ConfigEntry<float> projectileBurst;

        private int entryCount = 100;

        // The entries are bound in this order, which is also the order they have in the config file and the config manager
        public void GenerateConfig(ConfigFile config, StaffConfigOptions options)
        {
            enable = Bind(config, options, "Enable", options.enable, "Whether the recipe for this item is enabled", () => UpdateRecipe(options, RecipeUpdateType.ENABLE));

            name = Bind(config, options, "Name", options.name, "The name given to the item", () => Update(options, new UpdateItemDataOptions() { name = name.Value }));

            description = Bind(config, options, "Description", options.description ?? "", "The description given to the item",
                () => Update(options, new UpdateItemDataOptions() { description = description.Value }));

            craftingStation = config.Bind(new ConfigDefinition(options.sectionName, "Crafting station"), options.craftingStation,
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(craftingStationOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            craftingStation.SettingChanged += (obj, attr) => UpdateRecipe(options, RecipeUpdateType.CRAFTINGSTATION);

            minStationLevel = Bind(config, options, "Required station level to craft", options.minStationLevel ?? 1, "The required station level to craft",
                () => UpdateRecipe(options, RecipeUpdateType.MINREQUIREDSTATIONLEVEL));

            recipe = Bind(config, options, "Crafting costs", options.recipe, "The items required to craft", () => UpdateRecipe(options, RecipeUpdateType.RECIPE));
            recipeUpgrade = Bind(config, options, "Upgrade costs", options.recipeUpgrade, "The costs to upgrade the item", () => UpdateRecipe(options, RecipeUpdateType.RECIPE));
            recipeMultiplier = Bind(config, options, "Upgrade multiplier", options.recipeMultiplier ?? 1, "The multiplier applied to the upgrade costs",
                () => UpdateRecipe(options, RecipeUpdateType.RECIPE));

            if (options.maxQuality != null)
                maxQuality = Bind(config, options, "Max quality", (int)options.maxQuality, "The maximum quality the item can become", () => Update(options, new UpdateItemDataOptions() { maxQuality = maxQuality.Value }));

            if (options.weight != null)
                weight = Bind(config, options, "Weight", (float)options.weight, "The weight of the item", () => Update(options, new UpdateItemDataOptions() { weight = weight.Value }));

            if (options.maxDurability != null)
                maxDurability = Bind(config, options, "Max durability", (float)options.maxDurability, "The maximum durability of the item", () => Update(options, new UpdateItemDataOptions() { maxDurability = maxDurability.Value }));

            if (options.movementSpeed != null)
                movementSpeed = Bind(config, options, "Movement speed", (float)options.movementSpeed, "The movement speed stat on the item", () => Update(options, new UpdateItemDataOptions() { movementModifier = movementSpeed.Value }));

            if (options.damageBlunt != null)
                damageBlunt = Bind(config, options, "Attack damage blunt", (float)options.damageBlunt, "Blunt damage on the item", () => Update(options, new UpdateItemDataOptions() { damageBlunt = damageBlunt.Value }));

            if (options.damageBluntPerLevel != null)
                damageBluntPerLevel = Bind(config, options, "Attack damage blunt per level", (float)options.damageBluntPerLevel, "Blunt damage per upgrade level on the item", () => Update(options, new UpdateItemDataOptions() { damageBluntPerLevel = damageBluntPerLevel.Value }));

            if (options.damageChop != null)
                damageChop = Bind(config, options, "Attack damage chop", (float)options.damageChop, "Chop damage on the item (not visible ingame, effects wood cutting damage)", () => Update(options, new UpdateItemDataOptions() { damageChop = damageChop.Value }));

            if (options.damageChopPerLevel != null)
                damageChopPerLevel = Bind(config, options, "Attack damage chop per level", (float)options.damageChopPerLevel, "Chop damage per upgrade level on the item (not visible ingame, effects wood cutting damage)", () => Update(options, new UpdateItemDataOptions() { damageChopPerLevel = damageChopPerLevel.Value }));

            if (options.damageFire != null)
                damageFire = Bind(config, options, "Attack damage fire", (float)options.damageFire, "Fire damage on the item", () => Update(options, new UpdateItemDataOptions() { damageFire = damageFire.Value }));

            if (options.damageFrost != null)
                damageFrost = Bind(config, options, "Attack damage frost", (float)options.damageFrost, "Frost damage on the item", () => Update(options, new UpdateItemDataOptions() { damageFrost = damageFrost.Value }));

            if (options.damageLightning != null)
                damageLightning = Bind(config, options, "Attack damage lightning", (float)options.damageLightning, "Lightning damage on the item", () => Update(options, new UpdateItemDataOptions() { damageLightning = damageLightning.Value }));

            if (options.damagePickaxe != null)
                damagePickaxe = Bind(config, options, "Attack damage pickaxe", (float)options.damagePickaxe, "Pickaxe damage on the item (not visible ingame, effects mining damage)", () => Update(options, new UpdateItemDataOptions() { damagePickaxe = damagePickaxe.Value }));

            if (options.damagePickaxePerLevel != null)
                damagePickaxePerLevel = Bind(config, options, "Attack damage pickaxe per level", (float)options.damagePickaxePerLevel, "Pickaxe damage per upgrade level on the item (not visible ingame, effects mining damage)", () => Update(options, new UpdateItemDataOptions() { damagePickaxePerLevel = damagePickaxePerLevel.Value }));

            if (options.damagePierce != null)
                damagePierce = Bind(config, options, "Attack damage pierce", (float)options.damagePierce, "Pierce damage on the item", () => Update(options, new UpdateItemDataOptions() { damagePierce = damagePierce.Value }));

            if (options.damagePiercePerLevel != null)
                damagePiercePerLevel = Bind(config, options, "Attack damage pierce per level", (float)options.damagePiercePerLevel, "Pierce damage per upgrade level on the item", () => Update(options, new UpdateItemDataOptions() { damagePiercePerLevel = damagePiercePerLevel.Value }));

            if (options.damagePoison != null)
                damagePoison = Bind(config, options, "Attack damage poison", (float)options.damagePoison, "Poison damage on the item", () => Update(options, new UpdateItemDataOptions() { damagePoison = damagePoison.Value }));

            if (options.damagePoisonPerLevel != null)
                damagePoisonPerLevel = Bind(config, options, "Attack damage poison per level", (float)options.damagePoisonPerLevel, "Poison damage per upgrade level on the item", () => Update(options, new UpdateItemDataOptions() { damagePoisonPerLevel = damagePoisonPerLevel.Value }));

            if (options.damageSlash != null)
                damageSlash = Bind(config, options, "Attack damage slash", (float)options.damageSlash, "Slash damage on the item", () => Update(options, new UpdateItemDataOptions() { damageSlash = damageSlash.Value }));

            if (options.damageSlashPerLevel != null)
                damageSlashPerLevel = Bind(config, options, "Attack damage slash per level", (float)options.damageSlashPerLevel, "Slash damage per upgrade level on the item", () => Update(options, new UpdateItemDataOptions() { damageSlashPerLevel = damageSlashPerLevel.Value }));

            if (options.damageSpirit != null)
                damageSpirit = Bind(config, options, "Attack damage spirit", (float)options.damageSpirit, "Spirit damage on the item", () => Update(options, new UpdateItemDataOptions() { damageSpirit = damageSpirit.Value }));

            if (options.damageSpiritPerLevel != null)
                damageSpiritPerLevel = Bind(config, options, "Attack damage spirit per level", (float)options.damageSpiritPerLevel, "Spirit damage per upgrade level on the item", () => Update(options, new UpdateItemDataOptions() { damageSpiritPerLevel = damageSpiritPerLevel.Value }));

            if (options.blockArmor != null)
                blockArmor = Bind(config, options, "Block armor", (int)options.blockArmor, "The block armor on the item", () => Update(options, new UpdateItemDataOptions() { blockPower = blockArmor.Value }));

            if (options.deflectionForce != null)
                deflectionForce = Bind(config, options, "Block force", (int)options.deflectionForce, "The block force on the item", () => Update(options, new UpdateItemDataOptions() { deflectionForce = deflectionForce.Value }));

            if (options.attackForce != null)
                attackForce = Bind(config, options, "Knockback", (int)options.attackForce, "The knockback on the item", () => Update(options, new UpdateItemDataOptions() { attackForce = attackForce.Value }));

            if (options.backStab != null)
                backStab = Bind(config, options, "Backstab", (float)options.backStab, "The backstab on the item", () => Update(options, new UpdateItemDataOptions() { backstabBonus = backStab.Value }));

            if (options.useEitr != null)
                useEitr = Bind(config, options, "Attack eitr cost", (int)options.useEitr, "Normal attack eitr cost", () => Update(options, new UpdateItemDataOptions() { attackEitr = useEitr.Value }));

            if (options.attackStamina != null)
                attackStamina = Bind(config, options, "Attack stamina cost", (int)options.attackStamina, "Normal attack stamina cost", () => Update(options, new UpdateItemDataOptions() { attackStamina = attackStamina.Value }));

            if (options.projectileVelocity != null)
                projectileVelocity = Bind(config, options, "Projectile velocity", (float)options.projectileVelocity, "The velocity of the projectile of the normal attack", () => Update(options, new UpdateItemDataOptions() { projectileVelocity = projectileVelocity.Value }));

            if (options.projectileAccuracy != null)
                projectileAccuracy = Bind(config, options, "Projectile accuracy", (float)options.projectileAccuracy, "The accuracy (spread) of the projectile of the normal attack", () => Update(options, new UpdateItemDataOptions() { projectileAccuracy = projectileAccuracy.Value }));

            if (options.projectileBurst != null)
                projectileBurst = Bind(config, options, "Projectile burst", (float)options.projectileBurst, "The burst (attack speed) of the projectiles of the normal attack", () => Update(options, new UpdateItemDataOptions() { burstInterval = projectileBurst.Value }));
        }

        // Binds an admin only entry and runs the handler whenever its value changes
        private ConfigEntry<T> Bind<T>(ConfigFile config, StaffConfigOptions options, string name, T defaultValue, string description, Action onChanged)
        {
            ConfigEntry<T> entry = config.Bind(new ConfigDefinition(options.sectionName, name), defaultValue,
                new ConfigDescription(description, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            entry.SettingChanged += (obj, attr) => onChanged();

            return entry;
        }

        // The recipe is updated from the values of all the recipe entries
        private void UpdateRecipe(StaffConfigOptions options, RecipeUpdateType updateType)
        {
            UpdateRecipeOptions recipeOptions = new UpdateRecipeOptions()
            {
                name = options.recipeName,
                updateType = updateType,
            };

            switch (updateType)
            {
                case RecipeUpdateType.ENABLE:
                    recipeOptions.enable = enable.Value;
                    break;
                case RecipeUpdateType.CRAFTINGSTATION:
                    recipeOptions.craftingStation = craftingStation.Value;
                    break;
                case RecipeUpdateType.MINREQUIREDSTATIONLEVEL:
                    recipeOptions.requiredStationLevel = minStationLevel.Value;
                    break;
                case RecipeUpdateType.RECIPE:
                    recipeOptions.requirements = recipe.Value;
                    recipeOptions.upgradeRequirements = recipeUpgrade.Value;
                    recipeOptions.upgradeMultiplier = recipeMultiplier.Value;
                    break;
            }

            RecipeHelper.UpdateRecipe(recipeOptions);
        }

        // Applies a changed value to the prefab of the staff
        private static void Update(StaffConfigOptions options, UpdateItemDataOptions itemOptions)
        {
            UpdateHelper.UpdateItemData(options.prefab, itemOptions);
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
