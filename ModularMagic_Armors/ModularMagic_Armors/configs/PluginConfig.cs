using BepInEx.Configuration;
using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Models;
using System;

namespace ModularMagic_Armors.Configs
{
    internal static class PluginConfig
    {
        // Shaman
        public static string armor1HelmetName = "Shaman Hood";
        public static string armor1HelmetRecipe = "TrollHide:5, BoneFragments:3, Resin:5, TrophyGreydwarfShaman:1";
        public static string armor1HelmetRecipeUpgrade = "TrollHide:2, BoneFragments:1, Resin:1";
        public static ArmorConfig armor1Helmet = new ArmorConfig();

        public static string armor1CapeName = "Shaman Cape";
        public static string armor1CapeRecipe = "TrollHide:10, BoneFragments:10, Resin:5, AncientSeed:2";
        public static string armor1CapeRecipeUpgrade = "TrollHide:2, BoneFragments:1, Resin:1";
        public static ArmorConfig armor1Cape = new ArmorConfig();

        public static string armor1ChestName = "Shaman Chest";
        public static string armor1ChestRecipe = "TrollHide:5, RoundLog:2, Resin:5, AncientSeed:1";
        public static string armor1ChestRecipeUpgrade = "TrollHide:2, RoundLog:1, BoneFragments:1, Resin:1";
        public static ArmorConfig armor1Chest = new ArmorConfig();

        public static string armor1LegsName = "Shaman Legs";
        public static string armor1LegsRecipe = "TrollHide:5, RoundLog:2, Resin:5, AncientSeed:1";
        public static string armor1LegsRecipeUpgrade = "TrollHide:2, RoundLog:1, BoneFragments:1, Resin:1";
        public static ArmorConfig armor1Legs = new ArmorConfig();

        // Wraith
        public static string armor2HelmetName = "Wraith Hood";
        public static string armor2HelmetRecipe = "Iron:10, DeerHide:6, Guck:4, Coal:4";
        public static string armor2HelmetRecipeUpgrade = "Iron:2, DeerHide:2, Guck:1, Coal:1";
        public static ArmorConfig armor2Helmet = new ArmorConfig();

        public static string armor2CapeName = "Wraith Cape";
        public static string armor2CapeRecipe = "Iron:4, DeerHide:10, Guck:2, Coal:8";
        public static string armor2CapeRecipeUpgrade = "Iron:1, DeerHide:2, Guck:1, Coal:1";
        public static ArmorConfig armor2Cape = new ArmorConfig();

        public static string armor2ChestName = "Wraith Chest";
        public static string armor2ChestRecipe = "Iron:10, DeerHide:6, Guck:4, Coal:4";
        public static string armor2ChestRecipeUpgrade = "Iron:2, DeerHide:2, Guck:1, Coal:1";
        public static ArmorConfig armor2Chest = new ArmorConfig();

        public static string armor2LegsName = "Wraith Legs";
        public static string armor2LegsRecipe = "Iron:10, DeerHide:6, Guck:4, Coal:4";
        public static string armor2LegsRecipeUpgrade = "Iron:2, DeerHide:2, Guck:1, Coal:1";
        public static ArmorConfig armor2Legs = new ArmorConfig();

        // Wolf
        public static string armor3HelmetName = "Frostwolf Hood";
        public static string armor3HelmetRecipe = "Silver:10, WolfPelt:6, Obsidian:4, Crystal:4";
        public static string armor3HelmetRecipeUpgrade = "Silver:2, WolfPelt:2, Obsidian:2, Crystal:2";
        public static ArmorConfig armor3Helmet = new ArmorConfig();

        public static string armor3CapeName = "Frostwolf Cape";
        public static string armor3CapeRecipe = "Silver:4, WolfPelt:6, TrophyWolf:1, Obsidian: 4";
        public static string armor3CapeRecipeUpgrade = "Silver:2, WolfPelt:4, Obsidian: 2";
        public static ArmorConfig armor3Cape = new ArmorConfig();

        public static string armor3ChestName = "Frostwolf Chest";
        public static string armor3ChestRecipe = "Silver:10, WolfPelt:6, Obsidian:4, Crystal:6";
        public static string armor3ChestRecipeUpgrade = "WolfPelt:2, Silver:1, Obsidian:2, Crystal:2";
        public static ArmorConfig armor3Chest = new ArmorConfig();

        public static string armor3LegsName = "Frostwolf Legs";
        public static string armor3LegsRecipe = "Silver:10, WolfPelt:6, Obsidian:4, Crystal:6";
        public static string armor3LegsRecipeUpgrade = "WolfPelt:2, Silver:1, Obsidian:2, Crystal:2";
        public static ArmorConfig armor3Legs = new ArmorConfig();

        // Dark Wizard
        public static string armor4HelmetName = "Dark Wizard Hood";
        public static string armor4HelmetRecipe = "BlackMetal:6, LinenThread:8, LoxPelt:4, Tar:4";
        public static string armor4HelmetRecipeUpgrade = "BlackMetal:2, LinenThread:2, LoxPelt:2, Tar:2";
        public static ArmorConfig armor4Helmet = new ArmorConfig();

        public static string armor4CapeName = "Dark Wizard Cape";
        public static string armor4CapeRecipe = "LinenThread:12, LoxPelt:6, Tar:8, GoblinTotem:1";
        public static string armor4CapeRecipeUpgrade = "LinenThread:2, LoxPelt:2, Tar:2";
        public static ArmorConfig armor4Cape = new ArmorConfig();

        public static string armor4ChestName = "Dark Wizard Chest";
        public static string armor4ChestRecipe = "BlackMetal:6, LinenThread:8, LoxPelt:6, Tar:4";
        public static string armor4ChestRecipeUpgrade = "BlackMetal:2, LinenThread:2, LoxPelt:2, Tar:2";
        public static ArmorConfig armor4Chest = new ArmorConfig();

        public static string armor4LegsName = "Dark Wizard Legs";
        public static string armor4LegsRecipe = "BlackMetal:6, LinenThread:8, LoxPelt:6, Tar:4";
        public static string armor4LegsRecipeUpgrade = "BlackMetal:2, LinenThread:2, LoxPelt:2, Tar:2";
        public static ArmorConfig armor4Legs = new ArmorConfig();

        // Eitr-weave
        public static string armor5HelmetName = "Eitr-weave Hood";
        public static string armor5HelmetRecipe = "LinenThread:16, Eitr:15, Iron:2";
        public static string armor5HelmetRecipeUpgrade = "LinenThread:8, Eitr:5";
        public static ArmorConfig armor5Helmet;

        public static string armor5ChestName = "Eitr-weave Robes";
        public static string armor5ChestRecipe = "LinenThread:20, Eitr:20, Feathers:10, ScaleHide:5";
        public static string armor5ChestRecipeUpgrade = "LinenThread:10, Eitr:5";
        public static ArmorConfig armor5Chest;

        public static string armor5LegsName = "Eitr-weave Trousers";
        public static string armor5LegsRecipe = "LinenThread:20, Eitr:20, ScaleHide:10";
        public static string armor5LegsRecipeUpgrade = "LinenThread:10, Eitr:5";
        public static ArmorConfig armor5Legs;

        // Embla
        public static string armor6HelmetName = "Hood of Embla";
        public static string armor6HelmetRecipe = "LinenThread:16, Eitr:15, AskHide:2";
        public static string armor6HelmetRecipeUpgrade = "LinenThread:8, Eitr:5";
        public static ArmorConfig armor6Helmet = new ArmorConfig();

        public static string armor6ChestName = "Robes of Embla";
        public static string armor6ChestRecipe = "LinenThread:20, Eitr:20, AskHide:10, FlametalNew:5";
        public static string armor6ChestRecipeUpgrade = "LinenThread:10, Eitr:5, FlametalNew:2";
        public static ArmorConfig armor6Chest = new ArmorConfig();

        public static string armor6LegsName = "Trousers of Embla";
        public static string armor6LegsRecipe = "LinenThread:20, Eitr:20, AskHide:10";
        public static string armor6LegsRecipeUpgrade = "LinenThread:10, Eitr:5";
        public static ArmorConfig armor6Legs = new ArmorConfig();

        // Other
        public static string generalSectionname = "General";
        public static ConfigEntry<bool> adjustEitrWeave;
        public static ConfigEntry<bool> adjustEmbla;
        public static ConfigEntry<bool> reskinEmbla;

        public static void Init()
        {
            _InitGeneralConfig();
            _InitArmor1Config();
            _InitArmor2Config();
            _InitArmor3Config();
            _InitArmor4Config();
        }

        private static void _InitGeneralConfig()
        {
            try
            {
                adjustEitrWeave = ModularMagic_Armors.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Adjust Eitr-weave set"), true,
                  new ConfigDescription("Adjust the Eitr-weave set to be in line with the rest. Will apply the set bonus effect and config settings. If disabled the set will be as vanilla (requires restart)", null,
                  new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 3 }));
                adjustEitrWeave.SettingChanged += (obj, attr) =>
                {
                    if (adjustEitrWeave.Value)
                    {
                        ArmorSetOptions armorSetOptions = new ArmorSetOptions()
                        {
                            name = "EitrWeaveSet_MMA",
                            size = 3,
                            statusEffect = ModularMagic_Armors.Instance.effects.EitrWeaveArmorSetSE,
                        };

                        ItemHelper.Adjust(ModularMagic_Armors.Instance.prefabs.EitrWeaveHelmetPrefab, armor5Helmet, armorSetOptions);
                        ItemHelper.Adjust(ModularMagic_Armors.Instance.prefabs.EitrWeaveChestPrefab, armor5Chest, armorSetOptions);
                        ItemHelper.Adjust(ModularMagic_Armors.Instance.prefabs.EitrWeaveLegsPrefab, armor5Legs, armorSetOptions);
                    }
                    else
                    {
                        ItemHelper.ResetToVanilla(ModularMagic_Armors.Instance.prefabs.EitrWeaveHelmetPrefab, ModularMagic_Armors.Instance.itemSnapShots.EitrWeaveHelmetStats, ModularMagic_Armors.Instance.itemSnapShots.EitrWeaveHelmetRecipe);
                        ItemHelper.ResetToVanilla(ModularMagic_Armors.Instance.prefabs.EitrWeaveChestPrefab, ModularMagic_Armors.Instance.itemSnapShots.EitrWeaveChestStats, ModularMagic_Armors.Instance.itemSnapShots.EitrWeaveChestRecipe);
                        ItemHelper.ResetToVanilla(ModularMagic_Armors.Instance.prefabs.EitrWeaveLegsPrefab, ModularMagic_Armors.Instance.itemSnapShots.EitrWeaveLegsStats, ModularMagic_Armors.Instance.itemSnapShots.EitrWeaveLegsRecipe);
                    }

                    armor5Helmet.ShowConfig(adjustEitrWeave.Value);
                    armor5Chest.ShowConfig(adjustEitrWeave.Value);
                    armor5Legs.ShowConfig(adjustEitrWeave.Value, true);
                };

                adjustEmbla = ModularMagic_Armors.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Adjust Embla set"), true,
                  new ConfigDescription("Adjust the Embla set to be in line with the rest. Will apply the set bonus effect and config settings. If disabled the set will be as vanilla (requires restart)", null,
                  new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 2 }));
                adjustEmbla.SettingChanged += (obj, attr) =>
                {
                    if (adjustEmbla.Value)
                    {
                        ArmorSetOptions armorSetOptions = new ArmorSetOptions()
                        {
                            name = "EmblaSet_MMA",
                            size = 3,
                            statusEffect = ModularMagic_Armors.Instance.effects.EmblaArmorSetSE,
                        };

                        ItemHelper.Adjust(ModularMagic_Armors.Instance.prefabs.EmblaHelmetPrefab, armor6Helmet, armorSetOptions);
                        ItemHelper.Adjust(ModularMagic_Armors.Instance.prefabs.EmblaChestPrefab, armor6Chest, armorSetOptions);
                        ItemHelper.Adjust(ModularMagic_Armors.Instance.prefabs.EmblaLegsPrefab, armor6Legs, armorSetOptions);
                    }
                    else
                    {
                        ItemHelper.ResetToVanilla(ModularMagic_Armors.Instance.prefabs.EmblaHelmetPrefab, ModularMagic_Armors.Instance.itemSnapShots.EmblaHelmetStats, ModularMagic_Armors.Instance.itemSnapShots.EmblaHelmetRecipe);
                        ItemHelper.ResetToVanilla(ModularMagic_Armors.Instance.prefabs.EmblaChestPrefab, ModularMagic_Armors.Instance.itemSnapShots.EmblaChestStats, ModularMagic_Armors.Instance.itemSnapShots.EmblaChestRecipe);
                        ItemHelper.ResetToVanilla(ModularMagic_Armors.Instance.prefabs.EmblaLegsPrefab, ModularMagic_Armors.Instance.itemSnapShots.EmblaLegsStats, ModularMagic_Armors.Instance.itemSnapShots.EmblaLegsRecipe);
                    }

                    armor6Helmet.ShowConfig(adjustEmbla.Value);
                    armor6Chest.ShowConfig(adjustEmbla.Value);
                    armor6Legs.ShowConfig(adjustEmbla.Value, true);
                };

                reskinEmbla = ModularMagic_Armors.Instance.Config.Bind(new ConfigDefinition(generalSectionname, "Retexture Embla set"), true,
                  new ConfigDescription("Due to the FrostWolf set using the same models this mod retextures the Embla set to be red with dark gray ash. If disabled the set will look as vanilla (requires restart)", null,
                  new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 1 }));
                reskinEmbla.SettingChanged += (obj, attr) =>
                {
                    Jotunn.Logger.LogWarning("ReskinEmbla: " + reskinEmbla.Value);
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + generalSectionname + " config: " + e);
            }
        }

        private static void _InitArmor1Config()
        {
            try
            {
                ArmorConfigOptions optionsHelmet = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.ShamanHelmetPrefab, armor1HelmetName, armor1HelmetRecipe, armor1HelmetRecipeUpgrade)
                {
                    description = "A hood crafted from tough troll hide, adorned with a shaman mask. When worn, the whispers of the wilds echo softly in your ears.",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    armor = 6,
                    maxDurability = 500,
                    eitr = 2f,
                    eitrRegen = 0.05f,
                };
                armor1Helmet.GenerateConfig(optionsHelmet);

                ArmorConfigOptions optionsCape = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.ShamanCapePrefab, armor1CapeName, armor1CapeRecipe, armor1CapeRecipeUpgrade)
                {
                    description = "A tattered cloak crafted from troll leather. Ancient seeds, embedded within the fabric, pulse with a faint glow, drawing power from the very heart of the forest’s magic.",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    maxDurability = 500,
                    eitr = 2f,
                    weight = 4f,
                    eitrRegen = 0.05f,
                };
                armor1Cape.GenerateConfig(optionsCape);

                ArmorConfigOptions optionsChest = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.ShamanChestPrefab, armor1ChestName, armor1ChestRecipe, armor1ChestRecipeUpgrade)
                {
                    description = "A rugged chestpiece crafted from tough troll leather. Embedded with ancient seeds, enhancing the wearer’s connection to the wild magic of the forest.",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    armor = 6,
                    weight = 5f,
                    maxDurability = 500,
                    eitr = 2f,
                    eitrRegen = 0.05f,
                };
                armor1Chest.GenerateConfig(optionsChest);

                ArmorConfigOptions optionsLegs = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.ShamanLegsPrefab, armor1LegsName, armor1LegsRecipe, armor1LegsRecipeUpgrade)
                {
                    description = "A rugged leggings craft from troll leather, reinforced with ancient seeds. Each step draws strength from the earth, grounding the wearer in the forest’s power.",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    armor = 6,
                    weight = 5f,
                    maxDurability = 500,
                    eitr = 2f,
                    eitrRegen = 0.05f,
                };
                armor1Legs.GenerateConfig(optionsLegs);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + armor1HelmetName + " config: " + e);
            }
        }

        private static void _InitArmor2Config()
        {
            try
            {
                ArmorConfigOptions optionsHelmet = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.WraithHelmetPrefab, armor2HelmetName, armor2HelmetRecipe, armor2HelmetRecipeUpgrade)
                {
                    description = "A dark hood crafted from shadowed cloth, its deep black fabric almost swallowing light. Faint whispers seem to stir when worn...",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    armor = 8,
                    maxDurability = 800,
                    eitr = 4f,
                    eitrRegen = 0.07f,
                };
                armor2Helmet.GenerateConfig(optionsHelmet);

                ArmorConfigOptions optionsCape = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.WraithCapePrefab, armor2CapeName, armor2CapeRecipe, armor2CapeRecipeUpgrade)
                {
                    description = "A sleek, flowing cloak made from deep, shadowy fabric that seems to absorb light. It drapes smoothly over the wearer, emanating an aura of foreboding.",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    weight = 4f,
                    maxDurability = 800,
                    eitr = 4f,
                    eitrRegen = 0.07f,
                };
                armor2Cape.GenerateConfig(optionsCape);

                ArmorConfigOptions optionsChest = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.WraithChestPrefab, armor2ChestName, armor2ChestRecipe, armor2ChestRecipeUpgrade)
                {
                    description = "A chestpiece made from, shadowy cloth, imbued with the lingering power of Wraiths. The fabric pulses with dark energy, enhancing the wearer’s magic as it draws upon the restless spirits of the past.",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    armor = 8,
                    weight = 5f,
                    maxDurability = 800,
                    eitr = 4f,
                    eitrRegen = 0.08f,
                };
                armor2Chest.GenerateConfig(optionsChest);

                ArmorConfigOptions optionsLegs = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.WraithLegsPrefab, armor2LegsName, armor2LegsRecipe, armor2LegsRecipeUpgrade)
                {
                    description = "Durable leggings crafted from shadowy cloth, infused with the essence of Wraiths. The fabric emanates dark energy, like the restless spirits that haunt the night.",
                    craftingStation = "Workbench",
                    minStationLevel = 2,
                    armor = 8,
                    weight = 5f,
                    maxDurability = 800,
                    eitr = 4f,
                    eitrRegen = 0.08f,
                };
                armor2Legs.GenerateConfig(optionsLegs);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + armor2HelmetName + " config: " + e);
            }
        }

        private static void _InitArmor3Config()
        {
            try
            {
                ArmorConfigOptions optionsHelmet = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.FrostWolfHelmetPrefab, armor3HelmetName, armor3HelmetRecipe, armor3HelmetRecipeUpgrade)
                {
                    description = "A hood crafted from enchanted wolf pelt, trimmed with shimmering silver chainmail. Infused with ancient magic, it draws on the wolf’s legendary endurance.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    armor = 10,
                    eitr = 8f,
                    eitrRegen = 0.1f,
                };
                armor3Helmet.GenerateConfig(optionsHelmet);

                ArmorConfigOptions optionsCape = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.FrostWolfCapePrefab, armor3CapeName, armor3CapeRecipe, armor3CapeRecipeUpgrade)
                {
                    description = "A cape crafted from wolf pelts granting protection from the elements. It enhances the wearer’s speed and agility, allowing them to move with the swiftness and grace of a wolf.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    weight = 4f,
                    eitr = 8f,
                    eitrRegen = 0.1f,
                };
                armor3Cape.GenerateConfig(optionsCape);

                ArmorConfigOptions optionsChest = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.FrostWolfChestPrefab, armor3ChestName, armor3ChestRecipe, armor3ChestRecipeUpgrade)
                {
                    description = "A chestpiece made of silver chainmail and adorned with wolf pelt, enchanted with strength and endurance. The robe channels the power of the wolf.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    weight = 5f,
                    armor = 10,
                    movementSpeed = -0.02f,
                    eitr = 8f,
                    eitrRegen = 0.1f,
                };
                armor3Chest.GenerateConfig(optionsChest);

                ArmorConfigOptions optionsLegs = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.FrostWolfLegsPrefab, armor3LegsName, armor3LegsRecipe, armor3LegsRecipeUpgrade)
                {
                    description = "Leggings crafted from silver chainmail and reinforced with wolf pelt, imbued with magical energy to enhance speed and agility.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    armor = 10,
                    weight = 5f,
                    movementSpeed = -0.02f,
                    eitr = 8f,
                    eitrRegen = 0.1f,
                };
                armor3Legs.GenerateConfig(optionsLegs);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + armor3HelmetName + " config: " + e);
            }
        }

        private static void _InitArmor4Config()
        {
            try
            {
                ArmorConfigOptions optionsHelmet = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.DarkWizardHelmetPrefab, armor4HelmetName, armor4HelmetRecipe, armor4HelmetRecipeUpgrade)
                {
                    description = "A tall, pointed hat crafted from black fabric, embroidered with arcane runes of forbidden magic. The hat enhances the wearer’s focus and concentration, amplifying their spellcasting abilities.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    armor = 13,
                    eitr = 12f,
                    eitrRegen = 0.15f,
                };
                armor4Helmet.GenerateConfig(optionsHelmet);

                ArmorConfigOptions optionsCape = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.DarkWizardCapePrefab, armor4CapeName, armor4CapeRecipe, armor4CapeRecipeUpgrade)
                {
                    description = "A cape made from black fabric, adorned with arcane symbols boosting the wearer’s magical power. A constant reminder of the forbidden magic that courses through them.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    weight = 4f,
                    eitr = 12f,
                    eitrRegen = 0.15f,
                };
                armor4Cape.GenerateConfig(optionsCape);

                ArmorConfigOptions optionsChest = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.DarkWizardChestPrefab, armor4ChestName, armor4ChestRecipe, armor4ChestRecipeUpgrade)
                {
                    description = "A chestpiece crafted from black fabric, woven with arcane symbols and infused with forbidden runes. The robe enhances the wearer’s mastery over dark magic.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    armor = 13,
                    weight = 5f,
                    movementSpeed = -0.02f,
                    eitr = 12f,
                    eitrRegen = 0.15f,
                };
                armor4Chest.GenerateConfig(optionsChest);

                ArmorConfigOptions optionsLegs = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.DarkWizardLegsPrefab, armor4LegsName, armor4LegsRecipe, armor4LegsRecipeUpgrade)
                {
                    description = "Leggings crafted from black fabric, reinforced with dark runes. Infused with forbidden power, they grant the wearer greater control over their magic, making their spells more precise and potent.",
                    craftingStation = "Forge",
                    minStationLevel = 3,
                    armor = 13,
                    weight = 5f,
                    movementSpeed = -0.02f,
                    eitr = 12f,
                    eitrRegen = 0.15f,
                };
                armor4Legs.GenerateConfig(optionsLegs);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + armor4HelmetName + " config: " + e);
            }
        }

        public static void InitArmor5Config(bool browsable = true)
        {
            try
            {
                ArmorConfigOptions optionsHelmet = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.EitrWeaveHelmetPrefab, armor5HelmetName, armor5HelmetRecipe, armor5HelmetRecipeUpgrade)
                {
                    description = "$item_helmet_mage_description",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    armor = 16,
                    eitr = 20f,
                    eitrRegen = 0.2f,
                };
                armor5Helmet = new ArmorConfig(browsable);
                armor5Helmet.GenerateConfig(optionsHelmet);

                ArmorConfigOptions optionsChest = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.EitrWeaveChestPrefab, armor5ChestName, armor5ChestRecipe, armor5ChestRecipeUpgrade)
                {
                    description = "$item_chest_mage_description",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    armor = 16,
                    weight = 5f,
                    movementSpeed = -0.02f,
                    eitr = 20f,
                    eitrRegen = 0.4f,
                };
                armor5Chest = new ArmorConfig(browsable);
                armor5Chest.GenerateConfig(optionsChest);

                ArmorConfigOptions optionsLegs = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.EitrWeaveLegsPrefab, armor5LegsName, armor5LegsRecipe, armor5LegsRecipeUpgrade)
                {
                    description = "$item_legs_mage_description",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    armor = 16,
                    weight = 5f,
                    movementSpeed = -0.02f,
                    eitr = 20f,
                    eitrRegen = 0.4f,
                };
                armor5Legs = new ArmorConfig(browsable);
                armor5Legs.GenerateConfig(optionsLegs);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + armor5HelmetName + " config: " + e);
            }
        }

        public static void InitArmor6Config(bool browsable = true)
        {
            try
            {
                ArmorConfigOptions optionsHelmet = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.EmblaHelmetPrefab, armor6HelmetName, armor6HelmetRecipe, armor6HelmetRecipeUpgrade)
                {
                    description = "$item_helmet_mage_ashlands_description",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    armor = 19,
                    eitr = 24f,
                    eitrRegen = 0.3f,
                };
                armor6Helmet = new ArmorConfig(browsable);
                armor6Helmet.GenerateConfig(optionsHelmet);

                ArmorConfigOptions optionsChest = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.EmblaChestPrefab, armor6ChestName, armor6ChestRecipe, armor6ChestRecipeUpgrade)
                {
                    description = "$item_chest_mage_ashlands_description",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    armor = 19,
                    weight = 5f,
                    movementSpeed = -0.02f,
                    eitr = 24f,
                    eitrRegen = 0.5f,
                };
                armor6Chest = new ArmorConfig(browsable);
                armor6Chest.GenerateConfig(optionsChest);

                ArmorConfigOptions optionsLegs = new ArmorConfigOptions(ModularMagic_Armors.Instance.prefabs.EmblaLegsPrefab, armor6LegsName, armor6LegsRecipe, armor6LegsRecipeUpgrade)
                {
                    description = "$item_legs_mage_ashlands_description",
                    craftingStation = "GaldrTable",
                    minStationLevel = 1,
                    armor = 19,
                    weight = 5f,
                    movementSpeed = -0.02f,
                    eitr = 24f,
                    eitrRegen = 0.5f,
                };
                armor6Legs = new ArmorConfig(browsable);
                armor6Legs.GenerateConfig(optionsLegs);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not initialise " + armor6HelmetName + " config: " + e);
            }
        }
    }
}
