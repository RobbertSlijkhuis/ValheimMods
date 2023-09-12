using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SpecialWeapons;
using UnityEngine;

namespace ValheimModding
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SpecialWeapons : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.SpecialWeapons";
        public const string PluginName = "Special Weapons";
        public const string PluginVersion = "1.0.0";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private AssetBundle specialWeaponsBundle;
        private GameObject hammerPrefabHammer;
        private GameObject hammerPrefabAtgeir;
        private GameObject atgeirPrefabAtgeir;
        private GameObject atgeirPrefabBattleAxe;
        private GameObject swordPrefabLightning;
        private GameObject swordPrefabFire;
        private GameObject swordPrefabFrost;

        private Sprite hammerHammerSprite;
        private Sprite hammerAtgeirSprite;
        private Sprite swordLightningSprite;
        private Sprite swordFireSprite;
        private Sprite swordFrostSprite;

        private ConfigEntry<bool> configHammerEnable;
        private ConfigEntry<string> configHammerName;
        private ConfigEntry<string> configHammerDescription;
        private ConfigEntry<string> configHammerCraftingStation;
        private ConfigEntry<int> configHammerMinStationLevel;
        private ConfigEntry<string> configHammerRecipe;
        private ConfigEntry<string> configHammerRecipeUpgrade;
        private ConfigEntry<int> configHammerRecipeMultiplier;
        private ConfigEntry<int> configHammerMaxQuality;
        private ConfigEntry<float> configHammerMovementSpeed;
        private ConfigEntry<float> configHammerDamageMultiplier;
        private ConfigEntry<int> configHammerBlockArmor;
        private ConfigEntry<int> configHammerBlockForce;
        private ConfigEntry<int> configHammerKnockBack;
        private ConfigEntry<int> configHammerBackStab;
        private ConfigEntry<int> configHammerUseStamina;
        private ConfigEntry<int> configHammerUseStaminaHammer;
        private ConfigEntry<int> configHammerUseStaminaAtgeir;

        private ConfigEntry<bool> configAtgeirEnable;
        private ConfigEntry<string> configAtgeirName;
        private ConfigEntry<string> configAtgeirDescription;
        private ConfigEntry<string> configAtgeirCraftingStation;
        private ConfigEntry<int> configAtgeirMinStationLevel;
        private ConfigEntry<string> configAtgeirRecipe;
        private ConfigEntry<string> configAtgeirRecipeUpgrade;
        private ConfigEntry<int> configAtgeirRecipeMultiplier;
        private ConfigEntry<int> configAtgeirMaxQuality;
        private ConfigEntry<float> configAtgeirMovementSpeed;
        private ConfigEntry<float> configAtgeirDamageMultiplier;
        private ConfigEntry<int> configAtgeirBlockArmor;
        private ConfigEntry<int> configAtgeirBlockForce;
        private ConfigEntry<int> configAtgeirKnockBack;
        private ConfigEntry<int> configAtgeirBackStab;
        private ConfigEntry<int> configAtgeirUseStamina;
        private ConfigEntry<int> configAtgeirUseStaminaAtgeir;
        private ConfigEntry<int> configAtgeirUseStaminaPoke;

        private ConfigEntry<bool> configSwordEnable;
        private ConfigEntry<string> configSwordName;
        private ConfigEntry<string> configSwordDescription;
        private ConfigEntry<string> configSwordCraftingStation;
        private ConfigEntry<int> configSwordMinStationLevel;
        private ConfigEntry<string> configSwordRecipe;
        private ConfigEntry<string> configSwordRecipeUpgrade;
        private ConfigEntry<int> configSwordRecipeMultiplier;
        private ConfigEntry<int> configSwordMaxQuality;

        private ConfigEntry<KeyboardShortcut> configWeaponModeKey;

        private ButtonConfig hammerWeaponModeButton;

        private CustomStatusEffect hammerHammerStatusEffect;
        private CustomStatusEffect hammerAtgeirStatusEffect;
        private CustomStatusEffect swordLightningStatusEffect;
        private CustomStatusEffect swordFireStatusEffect;
        private CustomStatusEffect swordFrostStatusEffect;

        private void Awake()
        {
            ModQuery.Enable();

            InitConfig();
            InitAssetBundle();
            InitInputs();
            InitStatusEffects();

            if (configHammerEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddHammer;

            if (configSwordEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddSword;

            if (configAtgeirEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddAtgeir;
        }

        private void Update()
        {
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance != null)
            {
                // KeyboardShortcuts are also injected into the ZInput system
                if (hammerWeaponModeButton != null && MessageHud.instance != null)
                {
                    if (ZInput.GetButtonDown(hammerWeaponModeButton.Name) && MessageHud.instance.m_msgQeue.Count == 0)
                    {
                        if (Player.m_localPlayer)
                        {
                            ItemDrop.ItemData weapon = Player.m_localPlayer.GetCurrentWeapon();
                            if (weapon != null && weapon.m_shared.m_name == configHammerName.Value)
                            {
                                if (weapon.m_shared.m_attack.m_drawDurationMin == 0)
                                {
                                    ItemDrop itemDrop = hammerPrefabAtgeir.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = hammerPrefabAtgeir;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 1;
                                }
                                else
                                {
                                    ItemDrop itemDrop = hammerPrefabHammer.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = hammerPrefabHammer;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 0;
                                }

                                Player.m_localPlayer.UnequipItem(weapon);
                                Player.m_localPlayer.EquipItem(weapon);
                                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weapon.m_shared.m_attack.m_drawDurationMin == 0 ? "Hammer" : "Atgeir"));
                            }

                            else if (weapon != null && weapon.m_shared.m_name == configAtgeirName.Value)
                            {
                                if (weapon.m_shared.m_attack.m_drawDurationMin == 0)
                                {
                                    ItemDrop itemDrop = atgeirPrefabBattleAxe.GetComponent<ItemDrop>();
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 1;
                                }
                                else
                                {
                                    ItemDrop itemDrop = atgeirPrefabAtgeir.GetComponent<ItemDrop>();
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 0;
                                }

                                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weapon.m_shared.m_attack.m_drawDurationMin == 0 ? "Atgeir" : "Poke"));
                            }

                            else if (weapon != null && weapon.m_shared.m_name == "Dirks Dominance")
                            {
                                if (weapon.m_shared.m_attack.m_drawDurationMin == 0)
                                {
                                    ItemDrop itemDrop = swordPrefabFire.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = swordPrefabFire;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 1;
                                }
                                else if (weapon.m_shared.m_attack.m_drawDurationMin == 1)
                                {
                                    ItemDrop itemDrop = swordPrefabFrost.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = swordPrefabFrost;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 2;
                                }
                                else
                                {
                                    ItemDrop itemDrop = swordPrefabLightning.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = swordPrefabLightning;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 0;
                                }

                                Player.m_localPlayer.UnequipItem(weapon);
                                Player.m_localPlayer.EquipItem(weapon);
                                // Player.m_localPlayer.StartEmote("Cheer");
                                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weapon.m_shared.m_attack.m_drawDurationMin == 0 ? "Lightning" : weapon.m_shared.m_attack.m_drawDurationMin == 1 ? "Fire" : "Frost"));
                            }
                        }
                    }
                }
            }
        }

        private void AddHammer()
        {
            ConfigRecipe recipe = new ConfigRecipe(configHammerRecipe.Value, configHammerRecipeUpgrade.Value);
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = configHammerName.Value;
            itemConfig.Description = configHammerDescription.Value;
            itemConfig.CraftingStation = configHammerCraftingStation.Value;
            itemConfig.MinStationLevel = configHammerMinStationLevel.Value;

            foreach (var requirement in recipe.requirements)
            {
                int multiplier = configHammerRecipeMultiplier.Value != 0 ? configHammerRecipeMultiplier.Value : 1;
                int amountPerlevel = requirement.amountPerLevel * multiplier;
                itemConfig.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
            }

            ItemDrop itemDropHammer = hammerPrefabHammer.GetComponent<ItemDrop>();
            itemDropHammer.m_itemData.m_shared.m_maxQuality = configHammerMaxQuality.Value;
            itemDropHammer.m_itemData.m_shared.m_equipStatusEffect = hammerHammerStatusEffect.StatusEffect;
            itemDropHammer.m_itemData.m_shared.m_movementModifier = configHammerMovementSpeed.Value;
            itemDropHammer.m_itemData.m_shared.m_damages.m_blunt = itemDropHammer.m_itemData.m_shared.m_damages.m_blunt * configHammerDamageMultiplier.Value;
            itemDropHammer.m_itemData.m_shared.m_damages.m_lightning = itemDropHammer.m_itemData.m_shared.m_damages.m_lightning * configHammerDamageMultiplier.Value;
            itemDropHammer.m_itemData.m_shared.m_blockPower = configHammerBlockArmor.Value;
            itemDropHammer.m_itemData.m_shared.m_deflectionForce = configHammerBlockForce.Value;
            itemDropHammer.m_itemData.m_shared.m_attackForce = configHammerKnockBack.Value;
            itemDropHammer.m_itemData.m_shared.m_backstabBonus = configHammerBackStab.Value;
            itemDropHammer.m_itemData.m_shared.m_attack.m_attackStamina = configHammerUseStamina.Value;
            itemDropHammer.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configHammerUseStaminaHammer.Value;

            ItemDrop itemDropAtgeir = hammerPrefabAtgeir.GetComponent<ItemDrop>();
            itemDropAtgeir.m_itemData.m_shared.m_maxQuality = configHammerMaxQuality.Value;
            itemDropAtgeir.m_itemData.m_shared.m_equipStatusEffect = hammerAtgeirStatusEffect.StatusEffect;
            itemDropAtgeir.m_itemData.m_shared.m_movementModifier = configHammerMovementSpeed.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt = itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt * configHammerDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * configHammerDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_blockPower = configHammerBlockArmor.Value;
            itemDropAtgeir.m_itemData.m_shared.m_deflectionForce = configHammerBlockForce.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attackForce = configHammerKnockBack.Value;
            itemDropAtgeir.m_itemData.m_shared.m_backstabBonus = configHammerBackStab.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attack.m_attackStamina = configHammerUseStamina.Value;
            itemDropAtgeir.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configHammerUseStaminaAtgeir.Value;

            ItemManager.Instance.AddItem(new CustomItem(hammerPrefabHammer, true, itemConfig));
            ItemManager.Instance.AddItem(new CustomItem(hammerPrefabAtgeir, true, new ItemConfig()));
            PrefabManager.OnVanillaPrefabsAvailable -= AddHammer;
        }

        private void AddSword()
        {
            ConfigRecipe recipe = new ConfigRecipe(configSwordRecipe.Value, configSwordRecipeUpgrade.Value);
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = configSwordName.Value;
            itemConfig.Description = configSwordDescription.Value;
            itemConfig.CraftingStation = configSwordCraftingStation.Value;
            itemConfig.MinStationLevel = configSwordMinStationLevel.Value;

            foreach (var requirement in recipe.requirements)
            {
                int multiplier = configSwordRecipeMultiplier.Value != 0 ? configSwordRecipeMultiplier.Value : 1;
                int amountPerlevel = requirement.amountPerLevel * multiplier;
                itemConfig.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
            }

            ItemDrop itemDropLightning = swordPrefabLightning.GetComponent<ItemDrop>();
            itemDropLightning.m_itemData.m_shared.m_maxQuality = configSwordMaxQuality.Value;
            itemDropLightning.m_itemData.m_shared.m_equipStatusEffect = swordLightningStatusEffect.StatusEffect;
            ItemDrop itemDropFire = swordPrefabFire.GetComponent<ItemDrop>();
            itemDropFire.m_itemData.m_shared.m_maxQuality = configSwordMaxQuality.Value;
            itemDropFire.m_itemData.m_shared.m_equipStatusEffect = swordFireStatusEffect.StatusEffect;
            ItemDrop itemDropFrost = swordPrefabFrost.GetComponent<ItemDrop>();
            itemDropFrost.m_itemData.m_shared.m_maxQuality = configSwordMaxQuality.Value;
            itemDropFrost.m_itemData.m_shared.m_equipStatusEffect = swordFrostStatusEffect.StatusEffect;

            ItemManager.Instance.AddItem(new CustomItem(swordPrefabLightning, true, itemConfig));
            ItemManager.Instance.AddItem(new CustomItem(swordPrefabFire, true, new ItemConfig()));
            ItemManager.Instance.AddItem(new CustomItem(swordPrefabFrost, true, new ItemConfig()));

            //CustomItem DirksBoner = new CustomItem("DirksBoner_DW", "SwordMistwalker", DirkBonerConfig);

            //// Stats
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_damage = 0;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 35;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_frost = 40;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_spirit = 20;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_attack.m_attackStamina = 12;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_blockPower = 30;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_attackForce = 80;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_deflectionForce = 30;

            //// Quality
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_maxQuality = 10;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 3;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_frost = 3;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_spirit = 3;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_blockPowerPerLevel = 5;
            //DirksBoner.ItemDrop.m_itemData.m_shared.m_durabilityPerLevel = 50;

            //ItemManager.Instance.AddItem(DirksBoner);


            PrefabManager.OnVanillaPrefabsAvailable -= AddSword;
        }

        private void AddAtgeir()
        {
            ConfigRecipe recipe = new ConfigRecipe(configAtgeirRecipe.Value, configAtgeirRecipeUpgrade.Value);
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = configAtgeirName.Value;
            itemConfig.Description = configAtgeirDescription.Value;
            itemConfig.CraftingStation = configAtgeirCraftingStation.Value;
            itemConfig.MinStationLevel = configAtgeirMinStationLevel.Value;

            foreach (var requirement in recipe.requirements)
            {
                int multiplier = configAtgeirRecipeMultiplier.Value != 0 ? configAtgeirRecipeMultiplier.Value : 1;
                int amountPerlevel = requirement.amountPerLevel * multiplier;
                itemConfig.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
            }

            ItemDrop itemDropAtgeir = atgeirPrefabAtgeir.GetComponent<ItemDrop>();
            itemDropAtgeir.m_itemData.m_shared.m_maxQuality = configAtgeirMaxQuality.Value;
            itemDropAtgeir.m_itemData.m_shared.m_movementModifier = configAtgeirMovementSpeed.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_pierce = itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt * configAtgeirDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * configAtgeirDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_blockPower = configAtgeirBlockArmor.Value;
            itemDropAtgeir.m_itemData.m_shared.m_deflectionForce = configAtgeirBlockForce.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attackForce = configAtgeirKnockBack.Value;
            itemDropAtgeir.m_itemData.m_shared.m_backstabBonus = configAtgeirBackStab.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attack.m_attackStamina = configAtgeirUseStamina.Value;
            itemDropAtgeir.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configAtgeirUseStaminaAtgeir.Value;

            ItemDrop itemDropBattleAxe = atgeirPrefabBattleAxe.GetComponent<ItemDrop>();
            itemDropBattleAxe.m_itemData.m_shared.m_maxQuality = configAtgeirMaxQuality.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_movementModifier = configAtgeirMovementSpeed.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_damages.m_pierce = itemDropBattleAxe.m_itemData.m_shared.m_damages.m_blunt * configAtgeirDamageMultiplier.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_damages.m_lightning = itemDropBattleAxe.m_itemData.m_shared.m_damages.m_lightning * configAtgeirDamageMultiplier.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_blockPower = configAtgeirBlockArmor.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_deflectionForce = configAtgeirBlockForce.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_attackForce = configAtgeirKnockBack.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_backstabBonus = configAtgeirBackStab.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_attack.m_attackStamina = configAtgeirUseStamina.Value;
            itemDropBattleAxe.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configAtgeirUseStaminaPoke.Value;

            ItemManager.Instance.AddItem(new CustomItem(atgeirPrefabAtgeir, true, itemConfig));
            ItemManager.Instance.AddItem(new CustomItem(atgeirPrefabBattleAxe, true, new ItemConfig()));
            PrefabManager.OnVanillaPrefabsAvailable -= AddAtgeir;
        }

        private void InitStatusEffects()
        {
            StatusEffect hammerHammerEffect = ScriptableObject.CreateInstance<StatusEffect>();
            hammerHammerEffect.name = "HammerHammerEffect";
            hammerHammerEffect.m_name = "Hammer Mode";
            hammerHammerEffect.m_icon = hammerHammerSprite;
            hammerHammerEffect.m_startMessageType = MessageHud.MessageType.Center;
            hammerHammerEffect.m_startMessage = "";
            hammerHammerEffect.m_stopMessageType = MessageHud.MessageType.Center;
            hammerHammerEffect.m_stopMessage = "";
            hammerHammerEffect.m_tooltip = "Hammer Time!";
            hammerHammerStatusEffect = new CustomStatusEffect(hammerHammerEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(hammerHammerStatusEffect);

            StatusEffect hammerAtgeirEffect = ScriptableObject.CreateInstance<StatusEffect>();
            hammerAtgeirEffect.name = "HammerAtgeirEffect";
            hammerAtgeirEffect.m_name = "Atgeir Mode";
            hammerAtgeirEffect.m_icon = hammerAtgeirSprite;
            hammerAtgeirEffect.m_startMessageType = MessageHud.MessageType.Center;
            hammerAtgeirEffect.m_startMessage = "";
            hammerAtgeirEffect.m_stopMessageType = MessageHud.MessageType.Center;
            hammerAtgeirEffect.m_stopMessage = "";
            hammerAtgeirEffect.m_tooltip = "Swirl that Hammer around as if it was an Atgeir!";
            hammerAtgeirStatusEffect = new CustomStatusEffect(hammerAtgeirEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(hammerAtgeirStatusEffect);

            StatusEffect swordLightningEffect = ScriptableObject.CreateInstance<StatusEffect>();
            swordLightningEffect.name = "SwordLightningEffect";
            swordLightningEffect.m_name = "Lightning Mode";
            swordLightningEffect.m_icon = swordLightningSprite;
            swordLightningEffect.m_startMessageType = MessageHud.MessageType.Center;
            swordLightningEffect.m_startMessage = "";
            swordLightningEffect.m_stopMessageType = MessageHud.MessageType.Center;
            swordLightningEffect.m_stopMessage = "";
            swordLightningEffect.m_tooltip = "The power of Thor flows through the blade!";
            swordLightningStatusEffect = new CustomStatusEffect(swordLightningEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(swordLightningStatusEffect);

            StatusEffect swordFireEffect = ScriptableObject.CreateInstance<StatusEffect>();
            swordFireEffect.name = "SwordFireEffect";
            swordFireEffect.m_name = "Fire Mode";
            swordFireEffect.m_icon = swordFireSprite;
            swordFireEffect.m_startMessageType = MessageHud.MessageType.Center;
            swordFireEffect.m_startMessage = "";
            swordFireEffect.m_stopMessageType = MessageHud.MessageType.Center;
            swordFireEffect.m_stopMessage = "";
            swordFireEffect.m_tooltip = "Your blade is on fire!";
            swordFireStatusEffect = new CustomStatusEffect(swordFireEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(swordFireStatusEffect);

            StatusEffect swordFrostEffect = ScriptableObject.CreateInstance<StatusEffect>();
            swordFrostEffect.name = "SwordFrostEffect";
            swordFrostEffect.m_name = "Frost Mode";
            swordFrostEffect.m_icon = swordFrostSprite;
            swordFrostEffect.m_startMessageType = MessageHud.MessageType.Center;
            swordFrostEffect.m_startMessage = "";
            swordFrostEffect.m_stopMessageType = MessageHud.MessageType.Center;
            swordFrostEffect.m_stopMessage = "";
            swordFrostEffect.m_tooltip = "Your blade feels as cold as Jotunheim!";
            swordFrostStatusEffect = new CustomStatusEffect(swordFrostEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(swordFrostStatusEffect);
        }

        private void InitConfig()
        {
            Config.SaveOnConfigSet = true;

            // General
            configWeaponModeKey = Config.Bind("1. General", "Weapon mode key", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Key to change the weaponmode (applies to all weapons)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            // Hammer
            configHammerEnable = base.Config.Bind(new ConfigDefinition("2. Hammer", "Enable"), true,
                new ConfigDescription("Wether or not to enable the hammer", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerName = base.Config.Bind(new ConfigDefinition("2. Hammer", "Name"), "Kimetsus Special",
                new ConfigDescription("The name given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerDescription = base.Config.Bind(new ConfigDefinition("2. Hammer", "Description"), "May the bones of you enemies forever be crushed, my friend... to dust!",
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerCraftingStation = base.Config.Bind(new ConfigDefinition("2. Hammer", "Crafting station"), "Forge",
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(new string[] { "Disabled", "Inventory", "Workbench", "Cauldron", "Forge", "ArtisanTable", "StoneCutter", "MageTable", "BlackForge" }),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerMinStationLevel = base.Config.Bind(new ConfigDefinition("2. Hammer", "Required station level"), 1,
                new ConfigDescription("The required station level to craft the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerRecipe = base.Config.Bind(new ConfigDefinition("2. Hammer", "Crafting costs"), "Silver:30,ElderBark:10,Thunderstone:5,YmirRemains:5",
                new ConfigDescription("The items required to craft the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerRecipeUpgrade = base.Config.Bind(new ConfigDefinition("2. Hammer", "Upgrade costs"), "Silver:5,ElderBark:3,Thunderstone:1",
                new ConfigDescription("The costs to upgrade the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerRecipeMultiplier = base.Config.Bind(new ConfigDefinition("2. Hammer", "Upgrade multiplier"), 1,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerMaxQuality = base.Config.Bind(new ConfigDefinition("2. Hammer", "Max quality"), 4,
                new ConfigDescription("The maximum quality the item can become", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerMovementSpeed = base.Config.Bind(new ConfigDefinition("2. Hammer", "Movement speed"), -0.05f,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerDamageMultiplier = base.Config.Bind(new ConfigDefinition("2. Hammer", "Damage multiplier"), 1f,
                new ConfigDescription("Multiplier to adjust the damage on the item (90 blunt, 30 lightning)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerBlockArmor = base.Config.Bind(new ConfigDefinition("2. Hammer", "Block armor"), 40,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerBlockForce = base.Config.Bind(new ConfigDefinition("2. Hammer", "Block force"), 70,
                new ConfigDescription("The block force on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerKnockBack = base.Config.Bind(new ConfigDefinition("2. Hammer", "Knockback"), 75,
                new ConfigDescription("The knockback on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerBackStab = base.Config.Bind(new ConfigDefinition("2. Hammer", "Backstab"), 3,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerUseStamina = base.Config.Bind(new ConfigDefinition("2. Hammer", "Attack stamina"), 22,
                new ConfigDescription("Normal attack stamina usage", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerUseStaminaHammer = base.Config.Bind(new ConfigDefinition("2. Hammer", "Secondary hammer ability stamina"), 28,
                new ConfigDescription("The secondary hammer attack stamina usage", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configHammerUseStaminaAtgeir = base.Config.Bind(new ConfigDefinition("2. Hammer", "Secondary atgeir ability stamina"), 40,
                new ConfigDescription("The secondary atgeir attack stamina usage", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            // Sword
            configSwordEnable = base.Config.Bind(new ConfigDefinition("3. Sword", "Enable"), true,
                new ConfigDescription("Wether or not to enable the sword", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordName = base.Config.Bind(new ConfigDefinition("3. Sword", "Name"), "Dirks Dominance",
                new ConfigDescription("The name given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordDescription = base.Config.Bind(new ConfigDefinition("3. Sword", "Description"), "Got to assert your DOMINANCE!",
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordCraftingStation = base.Config.Bind(new ConfigDefinition("3. Sword", "Crafting station"), "Forge",
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(new string[] { "Disabled", "Inventory", "Workbench", "Cauldron", "Forge", "ArtisanTable", "StoneCutter", "MageTable", "BlackForge" }),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordMinStationLevel = base.Config.Bind(new ConfigDefinition("3. Sword", "Required station level"), 1,
                new ConfigDescription("The required station level to craft the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordRecipe = base.Config.Bind(new ConfigDefinition("3. Sword", "Crafting costs"), "Silver:25,ElderBark:10,FreezeGland:5,YmirRemains:5",
                new ConfigDescription("The items required to craft the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordRecipeUpgrade = base.Config.Bind(new ConfigDefinition("3. Sword", "Upgrade costs"), "Silver:5,ElderBark:5,FreezeGland:3",
                new ConfigDescription("The costs to upgrade the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordRecipeMultiplier = base.Config.Bind(new ConfigDefinition("3. Sword", "Upgrade multiplier"), 1,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configSwordMaxQuality = base.Config.Bind(new ConfigDefinition("3. Sword", "Max quality"), 4,
                new ConfigDescription("The maximum quality the item can become", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            // Atgeir
            configAtgeirEnable = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Enable"), true,
                new ConfigDescription("Wether or not to enable the atgeir", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirName = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Name"), "Marks Hark",
                new ConfigDescription("The name given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirDescription = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Description"), "Imma steal them sweet lewds from Durk! Grabby grabby!",
                new ConfigDescription("The description given to the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirCraftingStation = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Crafting station"), "Forge",
                new ConfigDescription("The crafting station the item can be created in",
                new AcceptableValueList<string>(new string[] { "Disabled", "Inventory", "Workbench", "Cauldron", "Forge", "ArtisanTable", "StoneCutter", "MageTable", "BlackForge" }),
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirMinStationLevel = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Required station level"), 1,
                new ConfigDescription("The required station level to craft the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirRecipe = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Crafting costs"), "Silver:25,ElderBark:15,FreezeGland:5,YmirRemains:5",
                new ConfigDescription("The items required to craft the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirRecipeUpgrade = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Upgrade costs"), "Silver:5,ElderBark:5,FreezeGland:3",
                new ConfigDescription("The costs to upgrade the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirRecipeMultiplier = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Upgrade multiplier"), 1,
                new ConfigDescription("The multiplier applied to the upgrade costs", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirMaxQuality = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Max quality"), 4,
                new ConfigDescription("The maximum quality the item can become", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirMovementSpeed = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Movement speed"), 0f,
                new ConfigDescription("The movement speed stat on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirDamageMultiplier = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Damage multiplier"), 1f,
                new ConfigDescription("Multiplier to adjust the damage on the item (85, 40 lightning)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirBlockArmor = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Block armor"), 64,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirBlockForce = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Block force"), 40,
                new ConfigDescription("The block force on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirKnockBack = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Knockback"), 40,
                new ConfigDescription("The knockback on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirBackStab = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Backstab"), 3,
                new ConfigDescription("The block armor on the item", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirUseStamina = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Attack stamina"), 20,
                new ConfigDescription("Normal attack stamina usage", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirUseStaminaAtgeir = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Secondary atgeir ability stamina"), 40,
                new ConfigDescription("The secondary atgeir attack stamina usage", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            configAtgeirUseStaminaPoke = base.Config.Bind(new ConfigDefinition("4. Atgeir", "Secondary poke ability stamina"), 9,
                new ConfigDescription("The secondary poke attack stamina usage", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        private void InitAssetBundle()
        {
            specialWeaponsBundle = AssetUtils.LoadAssetBundleFromResources("specialweapons_dw");
            hammerPrefabHammer = specialWeaponsBundle.LoadAsset<GameObject>("KimetsusSpecial_Hammer_DW");
            hammerPrefabAtgeir = specialWeaponsBundle.LoadAsset<GameObject>("KimetsusSpecial_Atgeir_DW");
            atgeirPrefabAtgeir = specialWeaponsBundle.LoadAsset<GameObject>("MarksHark_Atgeir_DW");
            atgeirPrefabBattleAxe = specialWeaponsBundle.LoadAsset<GameObject>("MarksHark_BattleAxe_DW");
            swordPrefabLightning = specialWeaponsBundle.LoadAsset<GameObject>("DirksDominance_Lightning_DW");
            swordPrefabFire = specialWeaponsBundle.LoadAsset<GameObject>("DirksDominance_Fire_DW");
            swordPrefabFrost = specialWeaponsBundle.LoadAsset<GameObject>("DirksDominance_Frost_DW");

            hammerHammerSprite = specialWeaponsBundle.LoadAsset<Sprite>("SledgeDemolisher");
            hammerAtgeirSprite = specialWeaponsBundle.LoadAsset<Sprite>("AtgeirHimminAfl");
            swordLightningSprite = specialWeaponsBundle.LoadAsset<Sprite>("Lightning");
            swordFireSprite = specialWeaponsBundle.LoadAsset<Sprite>("SwordFire");
            swordFrostSprite = specialWeaponsBundle.LoadAsset<Sprite>("Frost");
        }

        private void InitInputs()
        {
            hammerWeaponModeButton = new ButtonConfig
            {
                Name = "Weapon mode",
                ShortcutConfig = configWeaponModeKey,
                ActiveInGUI = true,
                ActiveInCustomGUI = true,
            };
            InputManager.Instance.AddButton(PluginGUID, hammerWeaponModeButton);

            //KeyHintConfig KHC = new KeyHintConfig
            //{
            //    Item = "KimetsusSpecial_Hammer_DW",
            //    ButtonConfigs = new[]
            //    {
            //        hammerWeaponModeButton,
            //    }
            //};
            //KeyHintManager.Instance.AddKeyHint(KHC);
        }
    }
}
