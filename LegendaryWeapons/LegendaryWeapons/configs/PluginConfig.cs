using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using UnityEngine;

namespace LegendaryWeapons.Configs
{
    internal static class PluginConfig
    {
        public static string[] configCraftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };
        public static string defaultRecipeDemoHammer = "YggdrasilWood:15, BlackMarble:20, Eitr:15, Thunderstone:10";
        public static string defaultUpgradeRecipeDemoHammer = "YggdrasilWood:5,BlackMarble:5,Eitr:5,Thunderstone:5";
        public static string defaultRecipeTriSword = "YggdrasilWood:10,Thunderstone:10,Bilebag:10,FreezeGland:30";
        public static string defaultUpgradeRecipeTriSword = "YggdrasilWood:5,Thunderstone:5,Bilebag:2,FreezeGland:10";
        public static string defaultRecipeCultivatorAtgeir = "YggdrasilWood:15,Silver:25,Eitr:15,Thunderstone:10";
        public static string defaultUpgradeRecipeCultivatorAtgeir = "YggdrasilWood:5,Silver:5,Eitr:5,Thunderstone:5";

        public static string sectionGeneral = "1. General";
        public static ConfigEntry<bool> configEnable;
        public static ConfigEntry<KeyboardShortcut> configWeaponModeKey;

        public static string sectionDemoHammer = "2. Demolition Hammer";
        public static ConfigEntry<bool> configDemoHammerEnable;
        public static ConfigEntry<string> configDemoHammerName;
        public static ConfigEntry<string> configDemoHammerDescription;
        public static ConfigEntry<string> configDemoHammerCraftingStation;
        public static ConfigEntry<int> configDemoHammerMinStationLevel;
        public static ConfigEntry<string> configDemoHammerRecipe;
        public static ConfigEntry<string> configDemoHammerRecipeUpgrade;
        public static ConfigEntry<int> configDemoHammerRecipeMultiplier;
        public static ConfigEntry<int> configDemoHammerMaxQuality;
        public static ConfigEntry<float> configDemoHammerMovementSpeed;
        public static ConfigEntry<float> configDemoHammerDamageMultiplier;
        public static ConfigEntry<int> configDemoHammerBlockArmor;
        public static ConfigEntry<int> configDemoHammerBlockForce;
        public static ConfigEntry<int> configDemoHammerKnockBack;
        public static ConfigEntry<int> configDemoHammerBackStab;
        public static ConfigEntry<int> configDemoHammerUseStamina;
        public static ConfigEntry<int> configDemoHammerUseStaminaHammer;
        public static ConfigEntry<int> configDemoHammerUseStaminaAtgeir;

        public static string sectionTriSword = "3. Tri Sword";
        public static ConfigEntry<bool> configTriSwordEnable;
        public static ConfigEntry<string> configTriSwordName;
        public static ConfigEntry<string> configTriSwordDescription;
        public static ConfigEntry<string> configTriSwordCraftingStation;
        public static ConfigEntry<int> configTriSwordMinStationLevel;
        public static ConfigEntry<string> configTriSwordRecipe;
        public static ConfigEntry<string> configTriSwordRecipeUpgrade;
        public static ConfigEntry<int> configTriSwordRecipeMultiplier;
        public static ConfigEntry<int> configTriSwordMaxQuality;
        public static ConfigEntry<float> configTriSwordMovementSpeed;
        public static ConfigEntry<float> configTriSwordDamageMultiplier;
        public static ConfigEntry<int> configTriSwordBlockArmor;
        public static ConfigEntry<int> configTriSwordBlockForce;
        public static ConfigEntry<int> configTriSwordKnockBack;
        public static ConfigEntry<int> configTriSwordFrostKnockBack;
        public static ConfigEntry<int> configTriSwordBackStab;
        public static ConfigEntry<int> configTriSwordUseStamina;
        public static ConfigEntry<int> configTriSwordUseStaminaLightning;
        public static ConfigEntry<int> configTriSwordUseStaminaFire;
        public static ConfigEntry<int> configTriSwordUseStaminaFrost;

        public static string sectionCultivatorAtgeir = "4. Cultivator Atgeir";
        public static ConfigEntry<bool> configCultivatorAtgeirEnable;
        public static ConfigEntry<string> configCultivatorAtgeirName;
        public static ConfigEntry<string> configCultivatorAtgeirDescription;
        public static ConfigEntry<string> configCultivatorAtgeirCraftingStation;
        public static ConfigEntry<int> configCultivatorAtgeirMinStationLevel;
        public static ConfigEntry<string> configCultivatorAtgeirRecipe;
        public static ConfigEntry<string> configCultivatorAtgeirRecipeUpgrade;
        public static ConfigEntry<int> configCultivatorAtgeirRecipeMultiplier;
        public static ConfigEntry<int> configCultivatorAtgeirMaxQuality;
        public static ConfigEntry<float> configCultivatorAtgeirMovementSpeed;
        public static ConfigEntry<float> configCultivatorAtgeirDamageMultiplier;
        public static ConfigEntry<int> configCultivatorAtgeirBlockArmor;
        public static ConfigEntry<int> configCultivatorAtgeirBlockForce;
        public static ConfigEntry<int> configCultivatorAtgeirKnockBack;
        public static ConfigEntry<int> configCultivatorAtgeirBackStab;
        public static ConfigEntry<int> configCultivatorAtgeirUseStamina;
        public static ConfigEntry<int> configCultivatorAtgeirUseStaminaAtgeir;
        public static ConfigEntry<int> configCultivatorAtgeirUseStaminaSpear;

        // Other
        private static int entryCount = 1000;
        private static FileSystemWatcher configWatcher;

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            try
            {
                LegendaryWeapons.Instance.Config.SaveOnConfigSet = false;

                // General
                configEnable = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionGeneral, "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configEnable.SettingChanged += (obj, attr) => {
                    if (configEnable.Value)
                    {
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation);
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation);
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation);
                    }
                    else
                    {
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation, true);
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation, true);
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("LegendaryWeapons is now disabled! The weapons can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                // Will automatically update in game, no event SettingChanged required
                configWeaponModeKey = LegendaryWeapons.Instance.Config.Bind(sectionGeneral, "Weapon mode key", new KeyboardShortcut(KeyCode.Y),
                    new ConfigDescription("Key to change the weaponmode (applies to all weapons)", null,
                    new ConfigurationManagerAttributes { Order = HandleOrder() }));

                // Demolition Hammer
                configDemoHammerEnable = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Enable"), true,
                    new ConfigDescription("Enable the Demolition Hammer", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerEnable.SettingChanged += (obj, attr) => {
                    if (configDemoHammerEnable.Value)
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation);
                    else
                    {
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("The " + configDemoHammerName.Value + " is now disabled! The weapon can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                configDemoHammerName = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Name"), "Tordenv�r",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerName.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerDescription = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Description"), "It might not be Mj�lnir, but it still hits like a thunderstorm!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerDescription.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); }; ;

                configDemoHammerCraftingStation = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configCraftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerCraftingStation.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation); };

                configDemoHammerMinStationLevel = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerMinStationLevel.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer, RecipeUpdateType.MinRequiredLevel); };

                configDemoHammerRecipe = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Crafting costs"), defaultRecipeDemoHammer,
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerRecipe.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer); };

                configDemoHammerRecipeUpgrade = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Upgrade costs"), defaultUpgradeRecipeDemoHammer,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerRecipeUpgrade.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer); };

                configDemoHammerRecipeMultiplier = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerRecipeMultiplier.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.Hammer); };

                configDemoHammerMaxQuality = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerMaxQuality.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerMovementSpeed = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerMovementSpeed.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerDamageMultiplier = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (90 blunt, 30 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerDamageMultiplier.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerBlockArmor = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Block armor"), 47,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerBlockArmor.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerBlockForce = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Block force"), 30,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerBlockForce.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerKnockBack = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Knockback"), 75,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerKnockBack.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerBackStab = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Backstab"), 3,
                    new ConfigDescription("The backstab on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerBackStab.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerUseStamina = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Attack stamina"), 22,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerUseStamina.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerUseStaminaHammer = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Secondary hammer ability stamina"), 32,
                    new ConfigDescription("The secondary hammer attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerUseStaminaHammer.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                configDemoHammerUseStaminaAtgeir = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Secondary atgeir ability stamina"), 40,
                    new ConfigDescription("The secondary atgeir attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configDemoHammerUseStaminaAtgeir.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchHammerStats(); };

                // Tri Sword
                configTriSwordEnable = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Enable"), true,
                    new ConfigDescription("Enable the Tri Sword", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordEnable.SettingChanged += (obj, attr) => {
                    if (configTriSwordEnable.Value)
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation);
                    else
                    {
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("The " + configTriSwordName.Value + " is now disabled! The weapon can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                configTriSwordName = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Name"), "Tresverd",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordName.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordDescription = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Description"), "Sometimes it seems as if the blade is phasing in and out from different dimensions...",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordDescription.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordCraftingStation = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configCraftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordCraftingStation.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation); };

                configTriSwordMinStationLevel = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordMinStationLevel.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword, RecipeUpdateType.MinRequiredLevel); };

                configTriSwordRecipe = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Crafting costs"), defaultRecipeTriSword,
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordRecipe.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword); };

                configTriSwordRecipeUpgrade = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Upgrade costs"), defaultUpgradeRecipeTriSword,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordRecipeUpgrade.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword); };

                configTriSwordRecipeMultiplier = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordRecipeMultiplier.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.TriSword); };

                configTriSwordMaxQuality = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordMaxQuality.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordMovementSpeed = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordMovementSpeed.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordDamageMultiplier = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (65-75-55 slash, 40 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordDamageMultiplier.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordBlockArmor = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordBlockArmor.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordBlockForce = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordBlockForce.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordKnockBack = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Knockback"), 40,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordKnockBack.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordFrostKnockBack = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Frost Knockback"), 100,
                    new ConfigDescription("The knockback on the item (Frost variant)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordFrostKnockBack.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordBackStab = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Backstab"), 3,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordBackStab.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordUseStamina = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Attack stamina"), 16,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordUseStamina.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordUseStaminaLightning = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Secondary lightning ability stamina"), 32,
                    new ConfigDescription("The secondary attack stamina usage (Lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordUseStaminaLightning.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordUseStaminaFire = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Secondary Fire ability stamina"), 32,
                    new ConfigDescription("The secondary attack stamina usage (Fire)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordUseStaminaFire.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                configTriSwordUseStaminaFrost = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionTriSword, "Secondary Frost ability stamina"), 24,
                    new ConfigDescription("The secondary attack stamina usage (Frost)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configTriSwordUseStaminaFrost.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchTriSwordStats(); };

                // Cultivator Atgeir
                configCultivatorAtgeirEnable = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Enable"), true,
                    new ConfigDescription("Enable the Cultivator Atgeir", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirEnable.SettingChanged += (obj, attr) => {
                    if (configCultivatorAtgeirEnable.Value)
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation);
                    else
                    {
                        LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("The " + configCultivatorAtgeirName.Value + " is now disabled! The weapon can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                configCultivatorAtgeirName = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Name"), "Lynrake",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirName.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirDescription = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Description"), "Apparently a cousin to Tordenv�r, how peculiar! Seems to be very well made and sharp.",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirDescription.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirCraftingStation = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configCraftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirCraftingStation.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation); };

                configCultivatorAtgeirMinStationLevel = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirMinStationLevel.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.MinRequiredLevel); };

                configCultivatorAtgeirRecipe = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Crafting costs"), defaultRecipeCultivatorAtgeir,
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirRecipe.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir); };

                configCultivatorAtgeirRecipeUpgrade = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Upgrade costs"), defaultUpgradeRecipeCultivatorAtgeir,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirRecipeUpgrade.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir); };

                configCultivatorAtgeirRecipeMultiplier = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirRecipeMultiplier.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchRecipe(WeaponType.CultivatorAtgeir); };

                configCultivatorAtgeirMaxQuality = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirMaxQuality.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirMovementSpeed = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Movement speed"), 0f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirMovementSpeed.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirDamageMultiplier = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (85, 40 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirDamageMultiplier.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirBlockArmor = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Block armor"), 64,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirBlockArmor.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirBlockForce = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Block force"), 40,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirBlockForce.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirKnockBack = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Knockback"), 40,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirKnockBack.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirBackStab = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Backstab"), 3,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirBackStab.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirUseStamina = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Attack stamina"), 20,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirUseStamina.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirUseStaminaAtgeir = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Secondary atgeir ability stamina"), 40,
                    new ConfigDescription("The secondary atgeir attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirUseStaminaAtgeir.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                // Enable SaveOnConfigSet before the last bind allowing the config file to be created on first run
                LegendaryWeapons.Instance.Config.SaveOnConfigSet = true;

                configCultivatorAtgeirUseStaminaSpear = LegendaryWeapons.Instance.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Secondary spear ability stamina"), 20,
                    new ConfigDescription("The secondary spear attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                configCultivatorAtgeirUseStaminaSpear.SettingChanged += (obj, attr) => { LegendaryWeapons.Instance.PatchCultivatorAtgeirStats(); };

                FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, LegendaryWeapons.configFileName);
                configWatcher.Changed += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Created += new FileSystemEventHandler(OnConfigFileChange);
                configWatcher.Renamed += new RenamedEventHandler(OnConfigFileChange);
                configWatcher.IncludeSubdirectories = true;
                configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
                configWatcher.EnableRaisingEvents = true;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise config: " + error);
            }
        }

        private static void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(LegendaryWeapons.configFileFullPath))
                return;

            try
            {
                LegendaryWeapons.Instance.Config.Reload();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
            }
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
