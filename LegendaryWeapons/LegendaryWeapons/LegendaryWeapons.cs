using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static EffectList;

namespace LegendaryWeapons
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class LegendaryWeapons : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.LegendaryWeapons";
        public const string PluginName = "Legendary Weapons";
        public const string PluginVersion = "1.0.3";
        private static string configFileName = PluginGUID + ".cfg";
        private static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;

        private AssetBundle legendaryWeaponsBundle;
        private GameObject demoHammerHammerPrefab;
        private GameObject demoHammerAtgeirPrefab;
        private GameObject cultivatorAtgeirAtgeirPrefab;
        private GameObject cultivatorAtgeirSpearPrefab;
        private GameObject cultivatorProjectilePrefab;
        private GameObject triSwordLightningPrefab;
        private GameObject triSwordFirePrefab;
        private GameObject triSwordFrostPrefab;

        private Sprite demolisherSprite;
        private Sprite himminAflSprite;
        private Sprite lightningSprite;
        private Sprite swordFireSprite;
        private Sprite frostSprite;
        private Sprite spearCarapaceSprite;

        private string[] configCraftingStationOptions = new string[] { "None", "Disabled", "Workbench", "Forge", "Stonecutter", "Cauldron", "ArtisanTable", "BlackForge", "GaldrTable" };
        private string defaultRecipeDemoHammer = "YggdrasilWood:15, BlackMarble:20, Eitr:15, Thunderstone:10";
        private string defaultUpgradeRecipeDemoHammer = "YggdrasilWood:5,BlackMarble:5,Eitr:5,Thunderstone:5";
        private string defaultRecipeTriSword = "YggdrasilWood:10,Thunderstone:10,Flametal:10,FreezeGland:30";
        private string defaultUpgradeRecipeTriSword = "YggdrasilWood:5,Thunderstone:5,Flametal:2,FreezeGland:10";
        private string defaultRecipeCultivatorAtgeir = "YggdrasilWood:15,Silver:25,Eitr:15,Thunderstone:10";
        private string defaultUpgradeRecipeCultivatorAtgeir = "YggdrasilWood:5,Silver:5,Eitr:5,Thunderstone:5";

        private string sectionGeneral = "1. General";
        private ConfigEntry<bool> configEnable;
        private ConfigEntry<KeyboardShortcut> configWeaponModeKey;

        private string sectionDemoHammer = "2. Demolition Hammer";
        private static ConfigEntry<bool> configDemoHammerEnable;
        private static ConfigEntry<string> configDemoHammerName;
        private static ConfigEntry<string> configDemoHammerDescription;
        private static ConfigEntry<string> configDemoHammerCraftingStation;
        private static ConfigEntry<int> configDemoHammerMinStationLevel;
        private static ConfigEntry<string> configDemoHammerRecipe;
        private static ConfigEntry<string> configDemoHammerRecipeUpgrade;
        private static ConfigEntry<int> configDemoHammerRecipeMultiplier;
        private static ConfigEntry<int> configDemoHammerMaxQuality;
        private static ConfigEntry<float> configDemoHammerMovementSpeed;
        private static ConfigEntry<float> configDemoHammerDamageMultiplier;
        private static ConfigEntry<int> configDemoHammerBlockArmor;
        private static ConfigEntry<int> configDemoHammerBlockForce;
        private static ConfigEntry<int> configDemoHammerKnockBack;
        private static ConfigEntry<int> configDemoHammerBackStab;
        private static ConfigEntry<int> configDemoHammerUseStamina;
        private static ConfigEntry<int> configDemoHammerUseStaminaHammer;
        private static ConfigEntry<int> configDemoHammerUseStaminaAtgeir;

        private string sectionTriSword = "3. Tri Sword";
        private static ConfigEntry<bool> configTriSwordEnable;
        private static ConfigEntry<string> configTriSwordName;
        private static ConfigEntry<string> configTriSwordDescription;
        private static ConfigEntry<string> configTriSwordCraftingStation;
        private static ConfigEntry<int> configTriSwordMinStationLevel;
        private static ConfigEntry<string> configTriSwordRecipe;
        private static ConfigEntry<string> configTriSwordRecipeUpgrade;
        private static ConfigEntry<int> configTriSwordRecipeMultiplier;
        private static ConfigEntry<int> configTriSwordMaxQuality;
        private static ConfigEntry<float> configTriSwordMovementSpeed;
        private static ConfigEntry<float> configTriSwordDamageMultiplier;
        private static ConfigEntry<int> configTriSwordBlockArmor;
        private static ConfigEntry<int> configTriSwordBlockForce;
        private static ConfigEntry<int> configTriSwordKnockBack;
        private static ConfigEntry<int> configTriSwordFrostKnockBack;
        private static ConfigEntry<int> configTriSwordBackStab;
        private static ConfigEntry<int> configTriSwordUseStamina;
        private static ConfigEntry<int> configTriSwordUseStaminaLightning;
        private static ConfigEntry<int> configTriSwordUseStaminaFire;
        private static ConfigEntry<int> configTriSwordUseStaminaFrost;
         
        private string sectionCultivatorAtgeir = "4. Cultivator Atgeir";
        private static ConfigEntry<bool> configCultivatorAtgeirEnable;
        private static ConfigEntry<string> configCultivatorAtgeirName;
        private static ConfigEntry<string> configCultivatorAtgeirDescription;
        private static ConfigEntry<string> configCultivatorAtgeirCraftingStation;
        private static ConfigEntry<int> configCultivatorAtgeirMinStationLevel;
        private static ConfigEntry<string> configCultivatorAtgeirRecipe;
        private static ConfigEntry<string> configCultivatorAtgeirRecipeUpgrade;
        private static ConfigEntry<int> configCultivatorAtgeirRecipeMultiplier;
        private static ConfigEntry<int> configCultivatorAtgeirMaxQuality;
        private static ConfigEntry<float> configCultivatorAtgeirMovementSpeed;
        private static ConfigEntry<float> configCultivatorAtgeirDamageMultiplier;
        private static ConfigEntry<int> configCultivatorAtgeirBlockArmor;
        private static ConfigEntry<int> configCultivatorAtgeirBlockForce;
        private static ConfigEntry<int> configCultivatorAtgeirKnockBack;
        private static ConfigEntry<int> configCultivatorAtgeirBackStab;
        private static ConfigEntry<int> configCultivatorAtgeirUseStamina;
        private static ConfigEntry<int> configCultivatorAtgeirUseStaminaAtgeir;
        private static ConfigEntry<int> configCultivatorAtgeirUseStaminaSpear;

        private ButtonConfig weaponModeButton;

        private CustomStatusEffect demoHammerHammerStatusEffect;
        private CustomStatusEffect demoHammerAtgeirStatusEffect;
        private CustomStatusEffect triSwordLightningStatusEffect;
        private CustomStatusEffect triSwordFireStatusEffect;
        private CustomStatusEffect triSwordFrostStatusEffect;
        private CustomStatusEffect cultivatorAtgeirAtgeirStatusEffect;
        private CustomStatusEffect cultivatorAtgeirSpearStatusEffect;

        /**
         * Called when the mod is being initialised
         */
        private void Awake()
        {
            InitConfig();

            if (!configEnable.Value) return;
            if (configEnable.Value && (!configDemoHammerEnable.Value && !configTriSwordEnable.Value && !configCultivatorAtgeirEnable.Value))
            {
                Jotunn.Logger.LogWarning("Mod is enabled but all weapons are disabled, skipping initialisation...");
                return;
            }

            InitAssetBundle();
            InitInputs();
            InitStatusEffects();

            if (configDemoHammerEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddDemoHammer;

            if (configTriSwordEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddTriSword;

            if (configCultivatorAtgeirEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddCultivatorAtgeir;
        }

        /**
         * Called when the mod is unloaded
         */
        private void OnDestroy()
        {
            Config.Save();
        }

        /**
         * Called on every update
         */
        private void Update()
        {
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance != null)
            {
                // KeyboardShortcuts are also injected into the ZInput system
                if (weaponModeButton != null && MessageHud.instance != null)
                {
                    if (ZInput.GetButtonDown(weaponModeButton.Name) && MessageHud.instance.m_msgQeue.Count == 0)
                    {
                        if (Player.m_localPlayer)
                        {
                            ItemDrop.ItemData weapon = Player.m_localPlayer.GetCurrentWeapon();

                            if (weapon != null)
                            {
                                string weaponModeName = null;
                                float weaponId = weapon.m_shared.m_attack.m_drawDurationMin;

                                // Demo Hammer
                                if (weaponId == 8900f)
                                {
                                    ItemDrop itemDrop = demoHammerAtgeirPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = demoHammerAtgeirPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weaponModeName = "Atgeir";
                                }
                                else if (weaponId == 8901)
                                {
                                    ItemDrop itemDrop = demoHammerHammerPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = demoHammerHammerPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weaponModeName = "Hammer";
                                }

                                // Trisword
                                else if (weaponId == 8902f)
                                {
                                    ItemDrop itemDrop = triSwordFirePrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = triSwordFirePrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weaponModeName = "Fire";
                                }
                                else if (weaponId == 8903f)
                                {
                                    ItemDrop itemDrop = triSwordFrostPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = triSwordFrostPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weaponModeName = "Frost";
                                }
                                else if (weaponId == 8904f)
                                {
                                    ItemDrop itemDrop = triSwordLightningPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = triSwordLightningPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weaponModeName = "Lightning";
                                }

                                // Cultivator Atgeir
                                else if (weaponId == 8905f)
                                {
                                    ItemDrop itemDrop = cultivatorAtgeirSpearPrefab.GetComponent<ItemDrop>();
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weaponModeName = "Spear";
                                }
                                else if (weaponId == 8906f)
                                {
                                    ItemDrop itemDrop = cultivatorAtgeirAtgeirPrefab.GetComponent<ItemDrop>();
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weaponModeName = "Atgeir";
                                }

                                if (weaponModeName != null)
                                {
                                    Player.m_localPlayer.UnequipItem(weapon);
                                    Player.m_localPlayer.EquipItem(weapon);
                                    // Player.m_localPlayer.StartEmote("Cheer");
                                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + weaponModeName);
                                }
                            }
                        }
                    }
                }
            }
        }

        /**
         * Adds the Demolition Hammer to the game
         */
        private void AddDemoHammer()
        {
            try
            {
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.CraftingStation = configDemoHammerCraftingStation.Value;
                itemConfig.MinStationLevel = configDemoHammerMinStationLevel.Value;
                itemConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(configDemoHammerRecipe.Value, configDemoHammerRecipeUpgrade.Value, configDemoHammerRecipeMultiplier.Value);

                ItemDrop itemDropHammer = demoHammerHammerPrefab.GetComponent<ItemDrop>();
                GameObject lightningAOEPrefab = PrefabManager.Instance.CreateClonedPrefab("lightningAOE_Hammer_DW", "lightningAOE");
                Transform rod = lightningAOEPrefab.transform.Find("AOE_ROD");
                Transform area = lightningAOEPrefab.transform.Find("AOE_AREA");
                rod.gameObject.SetActive(false);
                area.gameObject.SetActive(false);
                GameObject demolisherHitPrefab = new GameObject();
                demolisherHitPrefab.name = "JVLmock_fx_sledge_demolisher_hit";

                EffectData effectDemolisher = new EffectData();
                effectDemolisher.m_prefab = demolisherHitPrefab;
                effectDemolisher.m_enabled = true;
                effectDemolisher.m_variant = -1;

                EffectData effectLightningAOE = new EffectData();
                effectLightningAOE.m_prefab = lightningAOEPrefab;
                effectLightningAOE.m_enabled = true;
                effectLightningAOE.m_variant = -1;

                List<EffectData> effectList = new List<EffectData> { effectDemolisher, effectLightningAOE };
                itemDropHammer.m_itemData.m_shared.m_secondaryAttack.m_triggerEffect.m_effectPrefabs = effectList.ToArray();

                PatchHammerStats();

                ItemManager.Instance.AddItem(new CustomItem(demoHammerHammerPrefab, true, itemConfig));
                ItemManager.Instance.AddItem(new CustomItem(demoHammerAtgeirPrefab, true, new ItemConfig()));
                PrefabManager.OnVanillaPrefabsAvailable -= AddDemoHammer;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise the Demolition Hammer: " + error);
            }
        }

        /**
         * Adds the Trisword to the game
         */
        private void AddTriSword()
        {
            try
            {
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.CraftingStation = configTriSwordCraftingStation.Value;
                itemConfig.MinStationLevel = configTriSwordMinStationLevel.Value;
                itemConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(configTriSwordRecipe.Value, configTriSwordRecipeUpgrade.Value, configTriSwordRecipeMultiplier.Value);

                PatchTriSwordStats();

                ItemManager.Instance.AddItem(new CustomItem(triSwordLightningPrefab, true, itemConfig));
                ItemManager.Instance.AddItem(new CustomItem(triSwordFirePrefab, true, new ItemConfig()));
                ItemManager.Instance.AddItem(new CustomItem(triSwordFrostPrefab, true, new ItemConfig()));
                PrefabManager.OnVanillaPrefabsAvailable -= AddTriSword;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise the Tri Sword: " + error);
            }
        }

        /**
         * Adds the Cultivator Atgeir to the game
         */
        private void AddCultivatorAtgeir()
        {
            try
            {
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.CraftingStation = configCultivatorAtgeirCraftingStation.Value;
                itemConfig.MinStationLevel = configCultivatorAtgeirMinStationLevel.Value;
                itemConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(configCultivatorAtgeirRecipe.Value, configCultivatorAtgeirRecipeUpgrade.Value, configCultivatorAtgeirRecipeMultiplier.Value);

                Projectile projectileComp = cultivatorProjectilePrefab.GetComponent<Projectile>();
                GameObject lightningAOEPrefab = PrefabManager.Instance.CreateClonedPrefab("lightningAOE_Projectile_DW", "lightningAOE");
                Transform rod = lightningAOEPrefab.transform.Find("AOE_ROD");
                Transform area = lightningAOEPrefab.transform.Find("AOE_AREA");
                Aoe aoeComp = area.GetComponent<Aoe>();
                rod.gameObject.SetActive(false);
                aoeComp.m_damage.m_lightning = 27;
                aoeComp.m_radius = 3;

                EffectData effectLightningAOE = new EffectData();
                effectLightningAOE.m_prefab = lightningAOEPrefab;
                effectLightningAOE.m_enabled = true;
                effectLightningAOE.m_variant = -1;

                List<EffectData> effectList = new List<EffectData> { effectLightningAOE };
                projectileComp.m_hitEffects.m_effectPrefabs = effectList.ToArray();

                PatchCultivatorAtgeirStats();

                ItemManager.Instance.AddItem(new CustomItem(cultivatorAtgeirAtgeirPrefab, true, itemConfig));
                ItemManager.Instance.AddItem(new CustomItem(cultivatorAtgeirSpearPrefab, true, new ItemConfig()));
                PrefabManager.OnVanillaPrefabsAvailable -= AddCultivatorAtgeir;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise the Cultivator Atgeir: " + error);
            }
        }

        /**
         * Patch the DemoHammer prefabs with config values
         */
        private void PatchHammerStats()
        {
            if (!configDemoHammerEnable.Value) return;

            ItemDrop itemDropHammer = demoHammerHammerPrefab.GetComponent<ItemDrop>();
            itemDropHammer.m_itemData.m_shared.m_name = configDemoHammerName.Value;
            itemDropHammer.m_itemData.m_shared.m_description = configDemoHammerDescription.Value;
            itemDropHammer.m_itemData.m_shared.m_maxQuality = configDemoHammerMaxQuality.Value;
            itemDropHammer.m_itemData.m_shared.m_equipStatusEffect = demoHammerHammerStatusEffect.StatusEffect;
            itemDropHammer.m_itemData.m_shared.m_movementModifier = configDemoHammerMovementSpeed.Value;
            itemDropHammer.m_itemData.m_shared.m_damages.m_blunt = itemDropHammer.m_itemData.m_shared.m_damages.m_blunt * configDemoHammerDamageMultiplier.Value;
            itemDropHammer.m_itemData.m_shared.m_damages.m_lightning = itemDropHammer.m_itemData.m_shared.m_damages.m_lightning * configDemoHammerDamageMultiplier.Value;
            itemDropHammer.m_itemData.m_shared.m_blockPower = configDemoHammerBlockArmor.Value;
            itemDropHammer.m_itemData.m_shared.m_deflectionForce = configDemoHammerBlockForce.Value;
            itemDropHammer.m_itemData.m_shared.m_attackForce = configDemoHammerKnockBack.Value;
            itemDropHammer.m_itemData.m_shared.m_backstabBonus = configDemoHammerBackStab.Value;
            itemDropHammer.m_itemData.m_shared.m_attack.m_attackStamina = configDemoHammerUseStamina.Value;
            itemDropHammer.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configDemoHammerUseStaminaHammer.Value;

            ItemDrop itemDropAtgeir = demoHammerAtgeirPrefab.GetComponent<ItemDrop>();
            itemDropAtgeir.m_itemData.m_shared.m_name = configDemoHammerName.Value;
            itemDropAtgeir.m_itemData.m_shared.m_description = configDemoHammerDescription.Value;
            itemDropAtgeir.m_itemData.m_shared.m_maxQuality = configDemoHammerMaxQuality.Value;
            itemDropAtgeir.m_itemData.m_shared.m_equipStatusEffect = demoHammerAtgeirStatusEffect.StatusEffect;
            itemDropAtgeir.m_itemData.m_shared.m_movementModifier = configDemoHammerMovementSpeed.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt = itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt * configDemoHammerDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * configDemoHammerDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_blockPower = configDemoHammerBlockArmor.Value;
            itemDropAtgeir.m_itemData.m_shared.m_deflectionForce = configDemoHammerBlockForce.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attackForce = configDemoHammerKnockBack.Value;
            itemDropAtgeir.m_itemData.m_shared.m_backstabBonus = configDemoHammerBackStab.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attack.m_attackStamina = configDemoHammerUseStamina.Value;
            itemDropAtgeir.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configDemoHammerUseStaminaAtgeir.Value;
        }

        /**
         * Patch the TriSword prefabs with config values
         */
        private void PatchTriSwordStats()
        {
            if (!configTriSwordEnable.Value) return;

            ItemDrop itemDropLightning = triSwordLightningPrefab.GetComponent<ItemDrop>();
            itemDropLightning.m_itemData.m_shared.m_name = configTriSwordName.Value;
            itemDropLightning.m_itemData.m_shared.m_description = configTriSwordDescription.Value;
            itemDropLightning.m_itemData.m_shared.m_maxQuality = configTriSwordMaxQuality.Value;
            itemDropLightning.m_itemData.m_shared.m_equipStatusEffect = triSwordLightningStatusEffect.StatusEffect;
            itemDropLightning.m_itemData.m_shared.m_movementModifier = configTriSwordMovementSpeed.Value;
            itemDropLightning.m_itemData.m_shared.m_damages.m_slash = itemDropLightning.m_itemData.m_shared.m_damages.m_slash * configTriSwordDamageMultiplier.Value;
            itemDropLightning.m_itemData.m_shared.m_damages.m_lightning = itemDropLightning.m_itemData.m_shared.m_damages.m_lightning * configTriSwordDamageMultiplier.Value;
            itemDropLightning.m_itemData.m_shared.m_blockPower = configTriSwordBlockArmor.Value;
            itemDropLightning.m_itemData.m_shared.m_deflectionForce = configTriSwordBlockForce.Value;
            itemDropLightning.m_itemData.m_shared.m_attackForce = configTriSwordKnockBack.Value;
            itemDropLightning.m_itemData.m_shared.m_backstabBonus = configTriSwordBackStab.Value;
            itemDropLightning.m_itemData.m_shared.m_attack.m_attackStamina = configTriSwordUseStamina.Value;
            itemDropLightning.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configTriSwordUseStaminaLightning.Value;

            ItemDrop itemDropFire = triSwordFirePrefab.GetComponent<ItemDrop>();
            itemDropFire.m_itemData.m_shared.m_name = configTriSwordName.Value;
            itemDropFire.m_itemData.m_shared.m_description = configTriSwordDescription.Value;
            itemDropFire.m_itemData.m_shared.m_maxQuality = configTriSwordMaxQuality.Value;
            itemDropFire.m_itemData.m_shared.m_equipStatusEffect = triSwordFireStatusEffect.StatusEffect;
            itemDropFire.m_itemData.m_shared.m_movementModifier = configTriSwordMovementSpeed.Value;
            itemDropFire.m_itemData.m_shared.m_damages.m_slash = itemDropFire.m_itemData.m_shared.m_damages.m_slash * configTriSwordDamageMultiplier.Value;
            itemDropFire.m_itemData.m_shared.m_damages.m_fire = itemDropFire.m_itemData.m_shared.m_damages.m_fire * configTriSwordDamageMultiplier.Value;
            itemDropFire.m_itemData.m_shared.m_blockPower = configTriSwordBlockArmor.Value;
            itemDropFire.m_itemData.m_shared.m_deflectionForce = configTriSwordBlockForce.Value;
            itemDropFire.m_itemData.m_shared.m_attackForce = configTriSwordKnockBack.Value;
            itemDropFire.m_itemData.m_shared.m_backstabBonus = configTriSwordBackStab.Value;
            itemDropFire.m_itemData.m_shared.m_attack.m_attackStamina = configTriSwordUseStamina.Value;
            itemDropFire.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configTriSwordUseStaminaFire.Value;

            ItemDrop itemDropFrost = triSwordFrostPrefab.GetComponent<ItemDrop>();
            itemDropFrost.m_itemData.m_shared.m_name = configTriSwordName.Value;
            itemDropFrost.m_itemData.m_shared.m_description = configTriSwordDescription.Value;
            itemDropFrost.m_itemData.m_shared.m_maxQuality = configTriSwordMaxQuality.Value;
            itemDropFrost.m_itemData.m_shared.m_equipStatusEffect = triSwordFrostStatusEffect.StatusEffect;
            itemDropFrost.m_itemData.m_shared.m_movementModifier = configTriSwordMovementSpeed.Value;
            itemDropFrost.m_itemData.m_shared.m_damages.m_slash = itemDropFrost.m_itemData.m_shared.m_damages.m_slash * configTriSwordDamageMultiplier.Value;
            itemDropFrost.m_itemData.m_shared.m_damages.m_frost = itemDropFrost.m_itemData.m_shared.m_damages.m_frost * configTriSwordDamageMultiplier.Value;
            itemDropFrost.m_itemData.m_shared.m_blockPower = configTriSwordBlockArmor.Value;
            itemDropFrost.m_itemData.m_shared.m_deflectionForce = configTriSwordBlockForce.Value;
            itemDropFrost.m_itemData.m_shared.m_attackForce = configTriSwordFrostKnockBack.Value;
            itemDropFrost.m_itemData.m_shared.m_backstabBonus = configTriSwordBackStab.Value;
            itemDropFrost.m_itemData.m_shared.m_attack.m_attackStamina = configTriSwordUseStamina.Value;
            itemDropFrost.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configTriSwordUseStaminaFrost.Value;
        }

        /**
         * Patch the CultivatorAtgeir prefabs with config values
         */
        private void PatchCultivatorAtgeirStats()
        {
            if (!configCultivatorAtgeirEnable.Value) return;

            ItemDrop itemDropAtgeir = cultivatorAtgeirAtgeirPrefab.GetComponent<ItemDrop>();
            itemDropAtgeir.m_itemData.m_shared.m_name = configCultivatorAtgeirName.Value;
            itemDropAtgeir.m_itemData.m_shared.m_description = configCultivatorAtgeirDescription.Value;
            itemDropAtgeir.m_itemData.m_shared.m_maxQuality = configCultivatorAtgeirMaxQuality.Value;
            itemDropAtgeir.m_itemData.m_shared.m_equipStatusEffect = cultivatorAtgeirAtgeirStatusEffect.StatusEffect;
            itemDropAtgeir.m_itemData.m_shared.m_movementModifier = configCultivatorAtgeirMovementSpeed.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_pierce = itemDropAtgeir.m_itemData.m_shared.m_damages.m_pierce * configCultivatorAtgeirDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * configCultivatorAtgeirDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_blockPower = configCultivatorAtgeirBlockArmor.Value;
            itemDropAtgeir.m_itemData.m_shared.m_deflectionForce = configCultivatorAtgeirBlockForce.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attackForce = configCultivatorAtgeirKnockBack.Value;
            itemDropAtgeir.m_itemData.m_shared.m_backstabBonus = configCultivatorAtgeirBackStab.Value;
            itemDropAtgeir.m_itemData.m_shared.m_attack.m_attackStamina = configCultivatorAtgeirUseStamina.Value;
            itemDropAtgeir.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configCultivatorAtgeirUseStaminaAtgeir.Value;

            ItemDrop itemDropSpear = cultivatorAtgeirSpearPrefab.GetComponent<ItemDrop>();
            itemDropSpear.m_itemData.m_shared.m_name = configCultivatorAtgeirName.Value;
            itemDropSpear.m_itemData.m_shared.m_description = configCultivatorAtgeirDescription.Value;
            itemDropSpear.m_itemData.m_shared.m_maxQuality = configCultivatorAtgeirMaxQuality.Value;
            itemDropSpear.m_itemData.m_shared.m_equipStatusEffect = cultivatorAtgeirSpearStatusEffect.StatusEffect;
            itemDropSpear.m_itemData.m_shared.m_movementModifier = configCultivatorAtgeirMovementSpeed.Value;
            itemDropSpear.m_itemData.m_shared.m_damages.m_pierce = itemDropSpear.m_itemData.m_shared.m_damages.m_pierce * configCultivatorAtgeirDamageMultiplier.Value;
            itemDropSpear.m_itemData.m_shared.m_damages.m_lightning = itemDropSpear.m_itemData.m_shared.m_damages.m_lightning * configCultivatorAtgeirDamageMultiplier.Value;
            itemDropSpear.m_itemData.m_shared.m_blockPower = configCultivatorAtgeirBlockArmor.Value;
            itemDropSpear.m_itemData.m_shared.m_deflectionForce = configCultivatorAtgeirBlockForce.Value;
            itemDropSpear.m_itemData.m_shared.m_attackForce = configCultivatorAtgeirKnockBack.Value;
            itemDropSpear.m_itemData.m_shared.m_backstabBonus = configCultivatorAtgeirBackStab.Value;
            itemDropSpear.m_itemData.m_shared.m_attack.m_attackStamina = configCultivatorAtgeirUseStamina.Value;
            itemDropSpear.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configCultivatorAtgeirUseStaminaSpear.Value;
        }

        /**
         * Update recipe related fields when the config changes
         */
        private void PatchRecipe(WeaponType weaponType, RecipeUpdateType updateType = RecipeUpdateType.Recipe, bool disableOverride = false)
        {
            try
            {
                CustomRecipe recipe;
                bool isEnabled;
                string configCraftingStation;
                int configRequiredStationLevel;
                string configRecipe;
                string configUpgrade;
                int configMultiplier;

                if (disableOverride)
                {
                    switch (weaponType)
                    {
                        case WeaponType.Hammer:
                            recipe = ItemManager.Instance.GetRecipe("Recipe_Demo_Hammer_Hammer_DW");
                            break;
                        case WeaponType.TriSword:
                            recipe = ItemManager.Instance.GetRecipe("Recipe_Trisword_Lightning_DW");
                            break;
                        case WeaponType.CultivatorAtgeir:
                            recipe = ItemManager.Instance.GetRecipe("Recipe_Cultivator_Atgeir_Atgeir_DW");
                            break;
                        default:
                            throw new Exception("Could not find weapon type!");
                    }

                    if (recipe == null)
                        throw new Exception("Could not find recipe!");

                    recipe.Recipe.m_craftingStation = null;
                    recipe.Recipe.m_enabled = false;
                    return;
                }

                switch (weaponType)
                {
                    case WeaponType.Hammer:
                        recipe = ItemManager.Instance.GetRecipe("Recipe_Demo_Hammer_Hammer_DW");
                        isEnabled = configDemoHammerEnable.Value;
                        configCraftingStation = configDemoHammerCraftingStation.Value;
                        configRequiredStationLevel = configDemoHammerMinStationLevel.Value;
                        configRecipe = configDemoHammerRecipe.Value;
                        configUpgrade = configDemoHammerRecipeUpgrade.Value;
                        configMultiplier = configDemoHammerRecipeMultiplier.Value;
                        break;
                    case WeaponType.TriSword:
                        recipe = ItemManager.Instance.GetRecipe("Recipe_Trisword_Lightning_DW");
                        isEnabled = configTriSwordEnable.Value;
                        configCraftingStation = configTriSwordCraftingStation.Value;
                        configRequiredStationLevel = configTriSwordMinStationLevel.Value;
                        configRecipe = configTriSwordRecipe.Value;
                        configUpgrade = configTriSwordRecipeUpgrade.Value;
                        configMultiplier = configTriSwordRecipeMultiplier.Value;
                        break;
                    case WeaponType.CultivatorAtgeir:
                        recipe = ItemManager.Instance.GetRecipe("Recipe_Cultivator_Atgeir_Atgeir_DW");
                        isEnabled = configCultivatorAtgeirEnable.Value;
                        configCraftingStation = configCultivatorAtgeirCraftingStation.Value;
                        configRequiredStationLevel = configCultivatorAtgeirMinStationLevel.Value;
                        configRecipe = configCultivatorAtgeirRecipe.Value;
                        configUpgrade = configCultivatorAtgeirRecipeUpgrade.Value;
                        configMultiplier = configCultivatorAtgeirRecipeMultiplier.Value;
                        break;
                    default:
                        throw new Exception("Could not find weapon type!");
                }
                if (!isEnabled)
                    return;

                if (recipe == null)
                    throw new Exception("Could not find recipe!");

                switch (updateType)
                {
                    case RecipeUpdateType.Recipe:
                        Piece.Requirement[] requirements = RecipeHelper.GetAsPieceRequirementArray(configRecipe, configUpgrade, configMultiplier);

                        if (requirements == null)
                            throw new Exception("Requirements is null");

                        recipe.Recipe.m_resources = requirements;
                        break;
                    case RecipeUpdateType.CraftingStation:
                        if (configCraftingStation == "None")
                        {
                            recipe.Recipe.m_craftingStation = null;
                            recipe.Recipe.m_enabled = true;
                        }
                        else if (configCraftingStation == "Disabled")
                        {
                            recipe.Recipe.m_craftingStation = null;
                            recipe.Recipe.m_enabled = false;
                        }
                        else
                        {
                            string pieceName = CraftingStations.GetInternalName(configCraftingStation);
                            recipe.Recipe.m_enabled = true;
                            recipe.Recipe.m_craftingStation = PrefabManager.Instance.GetPrefab(pieceName).GetComponent<CraftingStation>();
                        }
                        break;
                    case RecipeUpdateType.MinRequiredLevel:
                        recipe.Recipe.m_minStationLevel = configRequiredStationLevel;
                        break;
                }               
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not update recipe: " + error);
            }
        }

        /**
         * Initialise config entries and add the necessary events
         */
        private void InitConfig()
        {
            try
            {
                Config.SaveOnConfigSet = false;

                // General
                configEnable = Config.Bind(new ConfigDefinition(sectionGeneral, "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configEnable.SettingChanged += (obj, attr) => {
                    if (configEnable.Value)
                    {
                        PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation);
                        PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation);
                        PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation);
                    }
                    else
                    {
                        PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation, true);
                        PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation, true);
                        PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("LegendaryWeapons is now disabled! The weapons can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                // Will automatically update in game, no event SettingChanged required
                configWeaponModeKey = Config.Bind(sectionGeneral, "Weapon mode key", new KeyboardShortcut(KeyCode.Y),
                    new ConfigDescription("Key to change the weaponmode (applies to all weapons)", null));

                // Demolition Hammer
                configDemoHammerEnable = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Enable"), true,
                    new ConfigDescription("Enable the Demolition Hammer", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerEnable.SettingChanged += (obj, attr) => {
                    if (configDemoHammerEnable.Value)
                        PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation);
                    else
                    {
                        PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("The " + configDemoHammerName.Value + " is now disabled! The weapon can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                configDemoHammerName = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Name"), "Tordenvær",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerName.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerDescription = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Description"), "Its might not be Mjölnir, but it still hits like a thunderstorm!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerDescription.SettingChanged += (obj, attr) => { PatchHammerStats(); }; ;

                configDemoHammerCraftingStation = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configCraftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerCraftingStation.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.Hammer, RecipeUpdateType.CraftingStation); };

                configDemoHammerMinStationLevel = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerMinStationLevel.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.Hammer, RecipeUpdateType.MinRequiredLevel); };

                configDemoHammerRecipe = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Crafting costs"), defaultRecipeDemoHammer,
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerRecipe.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.Hammer); };

                configDemoHammerRecipeUpgrade = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Upgrade costs"), defaultUpgradeRecipeDemoHammer,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerRecipeUpgrade.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.Hammer); };

                configDemoHammerRecipeMultiplier = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerRecipeMultiplier.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.Hammer); };

                configDemoHammerMaxQuality = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerMaxQuality.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerMovementSpeed = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerMovementSpeed.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerDamageMultiplier = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (90 blunt, 30 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerDamageMultiplier.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerBlockArmor = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Block armor"), 47,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerBlockArmor.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerBlockForce = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Block force"), 30,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerBlockForce.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerKnockBack = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Knockback"), 75,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerKnockBack.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerBackStab = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Backstab"), 3,
                    new ConfigDescription("The backstab on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerBackStab.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerUseStamina = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Attack stamina"), 22,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerUseStamina.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerUseStaminaHammer = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Secondary hammer ability stamina"), 32,
                    new ConfigDescription("The secondary hammer attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerUseStaminaHammer.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                configDemoHammerUseStaminaAtgeir = Config.Bind(new ConfigDefinition(sectionDemoHammer, "Secondary atgeir ability stamina"), 40,
                    new ConfigDescription("The secondary atgeir attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configDemoHammerUseStaminaAtgeir.SettingChanged += (obj, attr) => { PatchHammerStats(); };

                // Tri Sword
                configTriSwordEnable = Config.Bind(new ConfigDefinition(sectionTriSword, "Enable"), true,
                    new ConfigDescription("Enable the Tri Sword", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordEnable.SettingChanged += (obj, attr) => {
                    if (configTriSwordEnable.Value)
                        PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation);
                    else
                    {
                        PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("The " + configTriSwordName.Value + " is now disabled! The weapon can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                configTriSwordName = Config.Bind(new ConfigDefinition(sectionTriSword, "Name"), "Tresverd",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordName.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordDescription = Config.Bind(new ConfigDefinition(sectionTriSword, "Description"), "Sometimes it seems as if the blade is phasing in and out from different dimensions...",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordDescription.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordCraftingStation = Config.Bind(new ConfigDefinition(sectionTriSword, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configCraftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordCraftingStation.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.TriSword, RecipeUpdateType.CraftingStation); };

                configTriSwordMinStationLevel = Config.Bind(new ConfigDefinition(sectionTriSword, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordMinStationLevel.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.TriSword, RecipeUpdateType.MinRequiredLevel); };

                configTriSwordRecipe = Config.Bind(new ConfigDefinition(sectionTriSword, "Crafting costs"), defaultRecipeTriSword,
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordRecipe.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.TriSword); };

                configTriSwordRecipeUpgrade = Config.Bind(new ConfigDefinition(sectionTriSword, "Upgrade costs"), defaultUpgradeRecipeTriSword,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordRecipeUpgrade.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.TriSword); };

                configTriSwordRecipeMultiplier = Config.Bind(new ConfigDefinition(sectionTriSword, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordRecipeMultiplier.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.TriSword); };

                configTriSwordMaxQuality = Config.Bind(new ConfigDefinition(sectionTriSword, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordMaxQuality.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordMovementSpeed = Config.Bind(new ConfigDefinition(sectionTriSword, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordMovementSpeed.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordDamageMultiplier = Config.Bind(new ConfigDefinition(sectionTriSword, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (65-75-55 slash, 40 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordDamageMultiplier.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordBlockArmor = Config.Bind(new ConfigDefinition(sectionTriSword, "Block armor"), 48,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordBlockArmor.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordBlockForce = Config.Bind(new ConfigDefinition(sectionTriSword, "Block force"), 20,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordBlockForce.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordKnockBack = Config.Bind(new ConfigDefinition(sectionTriSword, "Knockback"), 40,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordKnockBack.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordFrostKnockBack = Config.Bind(new ConfigDefinition(sectionTriSword, "Frost Knockback"), 100,
                    new ConfigDescription("The knockback on the item (Frost variant)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordFrostKnockBack.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordBackStab = Config.Bind(new ConfigDefinition(sectionTriSword, "Backstab"), 3,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordBackStab.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordUseStamina = Config.Bind(new ConfigDefinition(sectionTriSword, "Attack stamina"), 16,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordUseStamina.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordUseStaminaLightning = Config.Bind(new ConfigDefinition(sectionTriSword, "Secondary lightning ability stamina"), 32,
                    new ConfigDescription("The secondary attack stamina usage (Lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordUseStaminaLightning.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordUseStaminaFire = Config.Bind(new ConfigDefinition(sectionTriSword, "Secondary Fire ability stamina"), 32,
                    new ConfigDescription("The secondary attack stamina usage (Fire)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordUseStaminaFire.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                configTriSwordUseStaminaFrost = Config.Bind(new ConfigDefinition(sectionTriSword, "Secondary Frost ability stamina"), 24,
                    new ConfigDescription("The secondary attack stamina usage (Frost)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configTriSwordUseStaminaFrost.SettingChanged += (obj, attr) => { PatchTriSwordStats(); };

                // Cultivator Atgeir
                configCultivatorAtgeirEnable = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Enable"), true,
                    new ConfigDescription("Enable the Cultivator Atgeir", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirEnable.SettingChanged += (obj, attr) => {
                    if (configCultivatorAtgeirEnable.Value)
                        PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation);
                    else
                    {
                        PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation, true);
                        Jotunn.Logger.LogWarning("The " + configCultivatorAtgeirName.Value + " is now disabled! The weapon can no longer be crafted or upgraded and will be deleted when players relog!");
                    }
                };

                configCultivatorAtgeirName = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Name"), "Lynrake",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirName.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirDescription = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Description"), "Apparently a cousin to Tordenvær, how peculiar! Seems to be very well made and sharp.",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirDescription.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirCraftingStation = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(configCraftingStationOptions),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirCraftingStation.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.CraftingStation); };

                configCultivatorAtgeirMinStationLevel = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirMinStationLevel.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.CultivatorAtgeir, RecipeUpdateType.MinRequiredLevel); };

                configCultivatorAtgeirRecipe = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Crafting costs"), defaultRecipeCultivatorAtgeir,
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirRecipe.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.CultivatorAtgeir); };

                configCultivatorAtgeirRecipeUpgrade = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Upgrade costs"), defaultUpgradeRecipeCultivatorAtgeir,
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirRecipeUpgrade.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.CultivatorAtgeir); };

                configCultivatorAtgeirRecipeMultiplier = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirRecipeMultiplier.SettingChanged += (obj, attr) => { PatchRecipe(WeaponType.CultivatorAtgeir); };

                configCultivatorAtgeirMaxQuality = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirMaxQuality.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirMovementSpeed = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Movement speed"), 0f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirMovementSpeed.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirDamageMultiplier = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (85, 40 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirDamageMultiplier.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirBlockArmor = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Block armor"), 64,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirBlockArmor.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirBlockForce = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Block force"), 40,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirBlockForce.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirKnockBack = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Knockback"), 40,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirKnockBack.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirBackStab = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Backstab"), 3,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirBackStab.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirUseStamina = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Attack stamina"), 20,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirUseStamina.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                configCultivatorAtgeirUseStaminaAtgeir = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Secondary atgeir ability stamina"), 40,
                    new ConfigDescription("The secondary atgeir attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirUseStaminaAtgeir.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                // Enable SaveOnConfigSet before the last bind allowing the config file to be created on first run
                Config.SaveOnConfigSet = true;

                configCultivatorAtgeirUseStaminaSpear = Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Secondary spear ability stamina"), 20,
                    new ConfigDescription("The secondary spear attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
                configCultivatorAtgeirUseStaminaSpear.SettingChanged += (obj, attr) => { PatchCultivatorAtgeirStats(); };

                FileSystemWatcher configWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, configFileName);
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

        /**
         * Event handler for when the config file changes
         */
        private void OnConfigFileChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(configFileFullPath))
                return;

            try
            {
                Config.Reload();
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Something went wrong while reloading the config, please check if the file exists and the entries are valid! " + error);
            }
        }

        /**
         * Initialise the status effects of the weapons
         */
        private void InitStatusEffects()
        {
            try
            {
                StatusEffect demoHammerHammerEffect = ScriptableObject.CreateInstance<StatusEffect>();
                demoHammerHammerEffect.name = "DemoHammerHammerEffect";
                demoHammerHammerEffect.m_name = "Hammer Mode";
                demoHammerHammerEffect.m_icon = demolisherSprite;
                demoHammerHammerEffect.m_startMessageType = MessageHud.MessageType.Center;
                demoHammerHammerEffect.m_startMessage = "";
                demoHammerHammerEffect.m_stopMessageType = MessageHud.MessageType.Center;
                demoHammerHammerEffect.m_stopMessage = "";
                demoHammerHammerEffect.m_tooltip = "Hammer Time!";
                demoHammerHammerStatusEffect = new CustomStatusEffect(demoHammerHammerEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(demoHammerHammerStatusEffect);

                StatusEffect demoHammerAtgeirEffect = ScriptableObject.CreateInstance<StatusEffect>();
                demoHammerAtgeirEffect.name = "DemoHammerAtgeirEffect";
                demoHammerAtgeirEffect.m_name = "Atgeir Mode";
                demoHammerAtgeirEffect.m_icon = himminAflSprite;
                demoHammerAtgeirEffect.m_startMessageType = MessageHud.MessageType.Center;
                demoHammerAtgeirEffect.m_startMessage = "";
                demoHammerAtgeirEffect.m_stopMessageType = MessageHud.MessageType.Center;
                demoHammerAtgeirEffect.m_stopMessage = "";
                demoHammerAtgeirEffect.m_tooltip = "Swirl that Hammer around as if it was an Atgeir!";
                demoHammerAtgeirStatusEffect = new CustomStatusEffect(demoHammerAtgeirEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(demoHammerAtgeirStatusEffect);

                StatusEffect triSwordLightningEffect = ScriptableObject.CreateInstance<StatusEffect>();
                triSwordLightningEffect.name = "TriSwordLightningEffect";
                triSwordLightningEffect.m_name = "Lightning Mode";
                triSwordLightningEffect.m_icon = lightningSprite;
                triSwordLightningEffect.m_startMessageType = MessageHud.MessageType.Center;
                triSwordLightningEffect.m_startMessage = "";
                triSwordLightningEffect.m_stopMessageType = MessageHud.MessageType.Center;
                triSwordLightningEffect.m_stopMessage = "";
                triSwordLightningEffect.m_tooltip = "The power of Thor flows through the blade!";
                triSwordLightningStatusEffect = new CustomStatusEffect(triSwordLightningEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(triSwordLightningStatusEffect);

                StatusEffect triSwordFireEffect = ScriptableObject.CreateInstance<StatusEffect>();
                triSwordFireEffect.name = "TriSwordFireEffect";
                triSwordFireEffect.m_name = "Fire Mode";
                triSwordFireEffect.m_icon = swordFireSprite;
                triSwordFireEffect.m_startMessageType = MessageHud.MessageType.Center;
                triSwordFireEffect.m_startMessage = "";
                triSwordFireEffect.m_stopMessageType = MessageHud.MessageType.Center;
                triSwordFireEffect.m_stopMessage = "";
                triSwordFireEffect.m_tooltip = "Your blade is on fire!";
                triSwordFireStatusEffect = new CustomStatusEffect(triSwordFireEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(triSwordFireStatusEffect);

                StatusEffect triSwordFrostEffect = ScriptableObject.CreateInstance<StatusEffect>();
                triSwordFrostEffect.name = "TriSwordFrostEffect";
                triSwordFrostEffect.m_name = "Frost Mode";
                triSwordFrostEffect.m_icon = frostSprite;
                triSwordFrostEffect.m_startMessageType = MessageHud.MessageType.Center;
                triSwordFrostEffect.m_startMessage = "";
                triSwordFrostEffect.m_stopMessageType = MessageHud.MessageType.Center;
                triSwordFrostEffect.m_stopMessage = "";
                triSwordFrostEffect.m_tooltip = "Your blade feels as cold as Jotunheim!";
                triSwordFrostStatusEffect = new CustomStatusEffect(triSwordFrostEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(triSwordFrostStatusEffect);

                StatusEffect cultivatorAtgeirEffect = ScriptableObject.CreateInstance<StatusEffect>();
                cultivatorAtgeirEffect.name = "CultivatorAtgeirAtgeirEffect";
                cultivatorAtgeirEffect.m_name = "Atgeir Mode";
                cultivatorAtgeirEffect.m_icon = himminAflSprite;
                cultivatorAtgeirEffect.m_startMessageType = MessageHud.MessageType.Center;
                cultivatorAtgeirEffect.m_startMessage = "";
                cultivatorAtgeirEffect.m_stopMessageType = MessageHud.MessageType.Center;
                cultivatorAtgeirEffect.m_stopMessage = "";
                cultivatorAtgeirEffect.m_tooltip = "Its not a reaper... but just as effective!";
                cultivatorAtgeirAtgeirStatusEffect = new CustomStatusEffect(cultivatorAtgeirEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(cultivatorAtgeirAtgeirStatusEffect);

                StatusEffect cultivatorSpearEffect = ScriptableObject.CreateInstance<StatusEffect>();
                cultivatorSpearEffect.name = "CultivatorAtgeirSpearEffect";
                cultivatorSpearEffect.m_name = "Spear Mode";
                cultivatorSpearEffect.m_icon = spearCarapaceSprite;
                cultivatorSpearEffect.m_startMessageType = MessageHud.MessageType.Center;
                cultivatorSpearEffect.m_startMessage = "";
                cultivatorSpearEffect.m_stopMessageType = MessageHud.MessageType.Center;
                cultivatorSpearEffect.m_stopMessage = "";
                cultivatorSpearEffect.m_tooltip = "You feel like throwing this!";
                cultivatorAtgeirSpearStatusEffect = new CustomStatusEffect(cultivatorSpearEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(cultivatorAtgeirSpearStatusEffect);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise status effects: " + error);
            }
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            try
            {
                legendaryWeaponsBundle = AssetUtils.LoadAssetBundleFromResources("legendaryweapons_dw");
                demoHammerHammerPrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("Demo_Hammer_Hammer_DW");
                demoHammerAtgeirPrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("Demo_Hammer_Atgeir_DW");
                cultivatorAtgeirAtgeirPrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("Cultivator_Atgeir_Atgeir_DW");
                cultivatorAtgeirSpearPrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("Cultivator_Atgeir_Spear_DW");
                cultivatorProjectilePrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("projectile_cultivator_DW");
                triSwordLightningPrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("TriSword_Lightning_DW");
                triSwordFirePrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("TriSword_Fire_DW");
                triSwordFrostPrefab = legendaryWeaponsBundle.LoadAsset<GameObject>("TriSword_Frost_DW");

                demolisherSprite = legendaryWeaponsBundle.LoadAsset<Sprite>("SledgeDemolisher_DW");
                himminAflSprite = legendaryWeaponsBundle.LoadAsset<Sprite>("AtgeirHimminAfl_DW");
                lightningSprite = legendaryWeaponsBundle.LoadAsset<Sprite>("Lightning_DW");
                swordFireSprite = legendaryWeaponsBundle.LoadAsset<Sprite>("SwordFire_DW");
                frostSprite = legendaryWeaponsBundle.LoadAsset<Sprite>("Frost_DW");
                spearCarapaceSprite = legendaryWeaponsBundle.LoadAsset<Sprite>("SpearCarapace_DW");
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise asset bundle: " + error);
            }
        }

        /**
         * Initialise the inputs of this mod
         */
        private void InitInputs()
        {
            try
            {
                weaponModeButton = new ButtonConfig
                {
                    Name = "Weapon mode",
                    ShortcutConfig = configWeaponModeKey,
                };

                InputManager.Instance.AddButton(PluginGUID, weaponModeButton);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: " + error);
            }
        }
    }
}

