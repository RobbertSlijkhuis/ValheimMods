using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using LegendaryWeapons.Configs;
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
        public const string PluginVersion = "1.1.0";
        public static string configFileName = PluginGUID + ".cfg";
        public static string configFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar.ToString() + configFileName;
        public static LegendaryWeapons Instance;

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

        private ButtonConfig weaponModeButton;

        private CustomStatusEffect demoHammerHammerStatusEffect;
        private CustomStatusEffect demoHammerAtgeirStatusEffect;
        private CustomStatusEffect triSwordLightningStatusEffect;
        private CustomStatusEffect triSwordFireStatusEffect;
        private CustomStatusEffect triSwordFrostStatusEffect;
        private CustomStatusEffect cultivatorAtgeirAtgeirStatusEffect;
        private CustomStatusEffect cultivatorAtgeirSpearStatusEffect;

        // Maps each weapon variant's current m_dropPrefab reference to the prefab/mode name to switch to next.
        // Keyed by m_dropPrefab (a GameObject reference to the prefab asset) rather than m_shared: crafting an
        // item goes through UnityEngine.Object.Instantiate(itemPrefab) (see Inventory.AddItem in the decompiled
        // assembly), which deep-copies plain [Serializable] fields like ItemData.SharedData into a new instance,
        // but leaves UnityEngine.Object reference fields like m_dropPrefab pointing at the original prefab asset.
        private Dictionary<GameObject, (GameObject nextPrefab, string modeName)> weaponModeTransitions;

        private void Awake()
        {
            Instance = this;
            PluginConfig.Init();

            if (!PluginConfig.configEnable.Value) return;
            if (PluginConfig.configEnable.Value && (!PluginConfig.configDemoHammerEnable.Value && !PluginConfig.configTriSwordEnable.Value && !PluginConfig.configCultivatorAtgeirEnable.Value))
            {
                Jotunn.Logger.LogWarning("Mod is enabled but all weapons are disabled, skipping initialisation...");
                return;
            }

            InitAssetBundle();
            InitInputs();
            InitStatusEffects();
            InitWeaponModeTransitions();

            if (PluginConfig.configDemoHammerEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddDemoHammer;

            if (PluginConfig.configTriSwordEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddTriSword;

            if (PluginConfig.configCultivatorAtgeirEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddCultivatorAtgeir;
        }

        private void Update()
        {
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance == null) return;

            // KeyboardShortcuts are also injected into the ZInput system
            if (weaponModeButton == null || MessageHud.instance == null) return;
            if (!ZInput.GetButtonDown(weaponModeButton.Name)) return;
            if (!Player.m_localPlayer) return;

            ItemDrop.ItemData weapon = Player.m_localPlayer.GetCurrentWeapon();
            if (weapon == null) return;

            if (!weaponModeTransitions.TryGetValue(weapon.m_dropPrefab, out var next)) return;

            weapon.m_dropPrefab = next.nextPrefab;
            weapon.m_shared = next.nextPrefab.GetComponent<ItemDrop>().m_itemData.m_shared;

            Player.m_localPlayer.UnequipItem(weapon);
            Player.m_localPlayer.EquipItem(weapon);
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + next.modeName);
        }

        private void AddDemoHammer()
        {
            try
            {
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.CraftingStation = PluginConfig.configDemoHammerCraftingStation.Value;
                itemConfig.MinStationLevel = PluginConfig.configDemoHammerMinStationLevel.Value;
                itemConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(PluginConfig.configDemoHammerRecipe.Value, PluginConfig.configDemoHammerRecipeUpgrade.Value, PluginConfig.configDemoHammerRecipeMultiplier.Value);

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

        private void AddTriSword()
        {
            try
            {
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.CraftingStation = PluginConfig.configTriSwordCraftingStation.Value;
                itemConfig.MinStationLevel = PluginConfig.configTriSwordMinStationLevel.Value;
                itemConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(PluginConfig.configTriSwordRecipe.Value, PluginConfig.configTriSwordRecipeUpgrade.Value, PluginConfig.configTriSwordRecipeMultiplier.Value);

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

        private void AddCultivatorAtgeir()
        {
            try
            {
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.CraftingStation = PluginConfig.configCultivatorAtgeirCraftingStation.Value;
                itemConfig.MinStationLevel = PluginConfig.configCultivatorAtgeirMinStationLevel.Value;
                itemConfig.Requirements = RecipeHelper.GetAsRequirementConfigArray(PluginConfig.configCultivatorAtgeirRecipe.Value, PluginConfig.configCultivatorAtgeirRecipeUpgrade.Value, PluginConfig.configCultivatorAtgeirRecipeMultiplier.Value);

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

        private void ApplyWeaponStats(ItemDrop item, StatusEffect equipStatusEffect, string name, string description, int maxQuality,
            float movementSpeed, int blockArmor, int blockForce, int knockback, int backstab, int attackStamina, int secondaryAttackStamina)
        {
            ItemDrop.ItemData.SharedData shared = item.m_itemData.m_shared;
            shared.m_name = name;
            shared.m_description = description;
            shared.m_maxQuality = maxQuality;
            shared.m_equipStatusEffect = equipStatusEffect;
            shared.m_movementModifier = movementSpeed;
            shared.m_blockPower = blockArmor;
            shared.m_deflectionForce = blockForce;
            shared.m_attackForce = knockback;
            shared.m_backstabBonus = backstab;
            shared.m_attack.m_attackStamina = attackStamina;
            shared.m_secondaryAttack.m_attackStamina = secondaryAttackStamina;
        }

        public void PatchHammerStats()
        {
            if (!PluginConfig.configDemoHammerEnable.Value) return;

            ItemDrop itemDropHammer = demoHammerHammerPrefab.GetComponent<ItemDrop>();
            ApplyWeaponStats(itemDropHammer, demoHammerHammerStatusEffect.StatusEffect, PluginConfig.configDemoHammerName.Value, PluginConfig.configDemoHammerDescription.Value,
                PluginConfig.configDemoHammerMaxQuality.Value, PluginConfig.configDemoHammerMovementSpeed.Value, PluginConfig.configDemoHammerBlockArmor.Value,
                PluginConfig.configDemoHammerBlockForce.Value, PluginConfig.configDemoHammerKnockBack.Value, PluginConfig.configDemoHammerBackStab.Value,
                PluginConfig.configDemoHammerUseStamina.Value, PluginConfig.configDemoHammerUseStaminaHammer.Value);
            itemDropHammer.m_itemData.m_shared.m_damages.m_blunt = itemDropHammer.m_itemData.m_shared.m_damages.m_blunt * PluginConfig.configDemoHammerDamageMultiplier.Value;
            itemDropHammer.m_itemData.m_shared.m_damages.m_lightning = itemDropHammer.m_itemData.m_shared.m_damages.m_lightning * PluginConfig.configDemoHammerDamageMultiplier.Value;

            ItemDrop itemDropAtgeir = demoHammerAtgeirPrefab.GetComponent<ItemDrop>();
            ApplyWeaponStats(itemDropAtgeir, demoHammerAtgeirStatusEffect.StatusEffect, PluginConfig.configDemoHammerName.Value, PluginConfig.configDemoHammerDescription.Value,
                PluginConfig.configDemoHammerMaxQuality.Value, PluginConfig.configDemoHammerMovementSpeed.Value, PluginConfig.configDemoHammerBlockArmor.Value,
                PluginConfig.configDemoHammerBlockForce.Value, PluginConfig.configDemoHammerKnockBack.Value, PluginConfig.configDemoHammerBackStab.Value,
                PluginConfig.configDemoHammerUseStamina.Value, PluginConfig.configDemoHammerUseStaminaAtgeir.Value);
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt = itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt * PluginConfig.configDemoHammerDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * PluginConfig.configDemoHammerDamageMultiplier.Value;
        }

        public void PatchTriSwordStats()
        {
            if (!PluginConfig.configTriSwordEnable.Value) return;

            ItemDrop itemDropLightning = triSwordLightningPrefab.GetComponent<ItemDrop>();
            ApplyWeaponStats(itemDropLightning, triSwordLightningStatusEffect.StatusEffect, PluginConfig.configTriSwordName.Value, PluginConfig.configTriSwordDescription.Value,
                PluginConfig.configTriSwordMaxQuality.Value, PluginConfig.configTriSwordMovementSpeed.Value, PluginConfig.configTriSwordBlockArmor.Value,
                PluginConfig.configTriSwordBlockForce.Value, PluginConfig.configTriSwordKnockBack.Value, PluginConfig.configTriSwordBackStab.Value,
                PluginConfig.configTriSwordUseStamina.Value, PluginConfig.configTriSwordUseStaminaLightning.Value);
            itemDropLightning.m_itemData.m_shared.m_damages.m_slash = itemDropLightning.m_itemData.m_shared.m_damages.m_slash * PluginConfig.configTriSwordDamageMultiplier.Value;
            itemDropLightning.m_itemData.m_shared.m_damages.m_lightning = itemDropLightning.m_itemData.m_shared.m_damages.m_lightning * PluginConfig.configTriSwordDamageMultiplier.Value;

            ItemDrop itemDropFire = triSwordFirePrefab.GetComponent<ItemDrop>();
            ApplyWeaponStats(itemDropFire, triSwordFireStatusEffect.StatusEffect, PluginConfig.configTriSwordName.Value, PluginConfig.configTriSwordDescription.Value,
                PluginConfig.configTriSwordMaxQuality.Value, PluginConfig.configTriSwordMovementSpeed.Value, PluginConfig.configTriSwordBlockArmor.Value,
                PluginConfig.configTriSwordBlockForce.Value, PluginConfig.configTriSwordKnockBack.Value, PluginConfig.configTriSwordBackStab.Value,
                PluginConfig.configTriSwordUseStamina.Value, PluginConfig.configTriSwordUseStaminaFire.Value);
            itemDropFire.m_itemData.m_shared.m_damages.m_slash = itemDropFire.m_itemData.m_shared.m_damages.m_slash * PluginConfig.configTriSwordDamageMultiplier.Value;
            itemDropFire.m_itemData.m_shared.m_damages.m_fire = itemDropFire.m_itemData.m_shared.m_damages.m_fire * PluginConfig.configTriSwordDamageMultiplier.Value;

            ItemDrop itemDropFrost = triSwordFrostPrefab.GetComponent<ItemDrop>();
            ApplyWeaponStats(itemDropFrost, triSwordFrostStatusEffect.StatusEffect, PluginConfig.configTriSwordName.Value, PluginConfig.configTriSwordDescription.Value,
                PluginConfig.configTriSwordMaxQuality.Value, PluginConfig.configTriSwordMovementSpeed.Value, PluginConfig.configTriSwordBlockArmor.Value,
                PluginConfig.configTriSwordBlockForce.Value, PluginConfig.configTriSwordFrostKnockBack.Value, PluginConfig.configTriSwordBackStab.Value,
                PluginConfig.configTriSwordUseStamina.Value, PluginConfig.configTriSwordUseStaminaFrost.Value);
            itemDropFrost.m_itemData.m_shared.m_damages.m_slash = itemDropFrost.m_itemData.m_shared.m_damages.m_slash * PluginConfig.configTriSwordDamageMultiplier.Value;
            itemDropFrost.m_itemData.m_shared.m_damages.m_frost = itemDropFrost.m_itemData.m_shared.m_damages.m_frost * PluginConfig.configTriSwordDamageMultiplier.Value;
        }

        public void PatchCultivatorAtgeirStats()
        {
            if (!PluginConfig.configCultivatorAtgeirEnable.Value) return;

            ItemDrop itemDropAtgeir = cultivatorAtgeirAtgeirPrefab.GetComponent<ItemDrop>();
            ApplyWeaponStats(itemDropAtgeir, cultivatorAtgeirAtgeirStatusEffect.StatusEffect, PluginConfig.configCultivatorAtgeirName.Value, PluginConfig.configCultivatorAtgeirDescription.Value,
                PluginConfig.configCultivatorAtgeirMaxQuality.Value, PluginConfig.configCultivatorAtgeirMovementSpeed.Value, PluginConfig.configCultivatorAtgeirBlockArmor.Value,
                PluginConfig.configCultivatorAtgeirBlockForce.Value, PluginConfig.configCultivatorAtgeirKnockBack.Value, PluginConfig.configCultivatorAtgeirBackStab.Value,
                PluginConfig.configCultivatorAtgeirUseStamina.Value, PluginConfig.configCultivatorAtgeirUseStaminaAtgeir.Value);
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_pierce = itemDropAtgeir.m_itemData.m_shared.m_damages.m_pierce * PluginConfig.configCultivatorAtgeirDamageMultiplier.Value;
            itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * PluginConfig.configCultivatorAtgeirDamageMultiplier.Value;

            ItemDrop itemDropSpear = cultivatorAtgeirSpearPrefab.GetComponent<ItemDrop>();
            ApplyWeaponStats(itemDropSpear, cultivatorAtgeirSpearStatusEffect.StatusEffect, PluginConfig.configCultivatorAtgeirName.Value, PluginConfig.configCultivatorAtgeirDescription.Value,
                PluginConfig.configCultivatorAtgeirMaxQuality.Value, PluginConfig.configCultivatorAtgeirMovementSpeed.Value, PluginConfig.configCultivatorAtgeirBlockArmor.Value,
                PluginConfig.configCultivatorAtgeirBlockForce.Value, PluginConfig.configCultivatorAtgeirKnockBack.Value, PluginConfig.configCultivatorAtgeirBackStab.Value,
                PluginConfig.configCultivatorAtgeirUseStamina.Value, PluginConfig.configCultivatorAtgeirUseStaminaSpear.Value);
            itemDropSpear.m_itemData.m_shared.m_damages.m_pierce = itemDropSpear.m_itemData.m_shared.m_damages.m_pierce * PluginConfig.configCultivatorAtgeirDamageMultiplier.Value;
            itemDropSpear.m_itemData.m_shared.m_damages.m_lightning = itemDropSpear.m_itemData.m_shared.m_damages.m_lightning * PluginConfig.configCultivatorAtgeirDamageMultiplier.Value;
        }

        public void PatchRecipe(WeaponType weaponType, RecipeUpdateType updateType = RecipeUpdateType.Recipe, bool disableOverride = false)
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
                        isEnabled = PluginConfig.configDemoHammerEnable.Value;
                        configCraftingStation = PluginConfig.configDemoHammerCraftingStation.Value;
                        configRequiredStationLevel = PluginConfig.configDemoHammerMinStationLevel.Value;
                        configRecipe = PluginConfig.configDemoHammerRecipe.Value;
                        configUpgrade = PluginConfig.configDemoHammerRecipeUpgrade.Value;
                        configMultiplier = PluginConfig.configDemoHammerRecipeMultiplier.Value;
                        break;
                    case WeaponType.TriSword:
                        recipe = ItemManager.Instance.GetRecipe("Recipe_Trisword_Lightning_DW");
                        isEnabled = PluginConfig.configTriSwordEnable.Value;
                        configCraftingStation = PluginConfig.configTriSwordCraftingStation.Value;
                        configRequiredStationLevel = PluginConfig.configTriSwordMinStationLevel.Value;
                        configRecipe = PluginConfig.configTriSwordRecipe.Value;
                        configUpgrade = PluginConfig.configTriSwordRecipeUpgrade.Value;
                        configMultiplier = PluginConfig.configTriSwordRecipeMultiplier.Value;
                        break;
                    case WeaponType.CultivatorAtgeir:
                        recipe = ItemManager.Instance.GetRecipe("Recipe_Cultivator_Atgeir_Atgeir_DW");
                        isEnabled = PluginConfig.configCultivatorAtgeirEnable.Value;
                        configCraftingStation = PluginConfig.configCultivatorAtgeirCraftingStation.Value;
                        configRequiredStationLevel = PluginConfig.configCultivatorAtgeirMinStationLevel.Value;
                        configRecipe = PluginConfig.configCultivatorAtgeirRecipe.Value;
                        configUpgrade = PluginConfig.configCultivatorAtgeirRecipeUpgrade.Value;
                        configMultiplier = PluginConfig.configCultivatorAtgeirRecipeMultiplier.Value;
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

        private void InitWeaponModeTransitions()
        {
            weaponModeTransitions = new Dictionary<GameObject, (GameObject nextPrefab, string modeName)>
            {
                { demoHammerHammerPrefab, (demoHammerAtgeirPrefab, "Atgeir") },
                { demoHammerAtgeirPrefab, (demoHammerHammerPrefab, "Hammer") },

                { triSwordLightningPrefab, (triSwordFirePrefab, "Fire") },
                { triSwordFirePrefab, (triSwordFrostPrefab, "Frost") },
                { triSwordFrostPrefab, (triSwordLightningPrefab, "Lightning") },

                { cultivatorAtgeirSpearPrefab, (cultivatorAtgeirAtgeirPrefab, "Atgeir") },
                { cultivatorAtgeirAtgeirPrefab, (cultivatorAtgeirSpearPrefab, "Spear") },
            };
        }

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
                PrefabManager.Instance.AddPrefab(new CustomPrefab(cultivatorProjectilePrefab, true));
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

        private void InitInputs()
        {
            try
            {
                weaponModeButton = new ButtonConfig
                {
                    Name = "Weapon mode",
                    ShortcutConfig = PluginConfig.configWeaponModeKey,
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
