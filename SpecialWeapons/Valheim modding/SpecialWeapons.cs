using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Linq;
using UnityEngine;
using static ItemDrop;
using static ItemSets;
using static MonoMod.InlineRT.MonoModRule;

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

        private ConfigEntry<bool> configLoadSpecials;

        // Variable BepInEx Shortcut backed by a config
        private ConfigEntry<KeyboardShortcut> ShortcutConfig;
        private ButtonConfig ShortcutButton;

        private int weaponModeKimetsu = 0;
        private CustomStatusEffect weaponModeEffect;
        private AssetBundle marksHarkBundle;

        private void Awake()
        {
            ModQuery.Enable();

            AddStatusEffects();
            HandleConfigValues();
            AddInputs();

            if (configLoadSpecials.Value)
            {
                PrefabManager.OnVanillaPrefabsAvailable += AddKimetsusSpecial;
                PrefabManager.OnVanillaPrefabsAvailable += AddDirksBoner;
                PrefabManager.OnVanillaPrefabsAvailable += AddMarksHark;
            }
        }

        // Called every frame
        private void Update()
        {
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance != null)
            {
                // KeyboardShortcuts are also injected into the ZInput system
                if (ShortcutButton != null && MessageHud.instance != null)
                {
                    if (ZInput.GetButtonDown(ShortcutButton.Name) && MessageHud.instance.m_msgQeue.Count == 0)
                    {
                        GameObject itemObject = PrefabManager.Instance.GetPrefab("KimetsusSpecial_DW");
                        ItemDrop item = itemObject.GetComponent<ItemDrop>();
                        GameObject targetItemObject;

                        if (weaponModeKimetsu == 0)
                        {
                            targetItemObject = PrefabManager.Instance.GetPrefab("AtgeirHimminAflClone");
                            ItemDrop targetItem = targetItemObject.GetComponent<ItemDrop>();

                            if (Player.m_localPlayer)
                            {
                                ItemDrop.ItemData weapon = Player.m_localPlayer.GetCurrentWeapon();
                                if (weapon != null && weapon.m_shared.m_name == "Kimetsu's Special")
                                {
                                    weapon.m_shared.m_secondaryAttack = targetItem.m_itemData.m_shared.m_secondaryAttack;
                                    weaponModeKimetsu = 1;
                                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weaponModeKimetsu == 0 ? "Hammer" : "Atgeir"));
                                }
                            }
                        }
                        else
                        {
                            // Create a clone of the original GameObject that was used to create the item (to prevent other changes from other mods)
                            targetItemObject = PrefabManager.Instance.GetPrefab("SledgeDemolisherClone");
                            ItemDrop targetItem = targetItemObject.GetComponent<ItemDrop>();

                            if (Player.m_localPlayer)
                            {
                                ItemDrop.ItemData weapon = Player.m_localPlayer.GetCurrentWeapon();
                                if (weapon != null && weapon.m_shared.m_name == "Kimetsu's Special")
                                {
                                    weapon.m_shared.m_secondaryAttack = targetItem.m_itemData.m_shared.m_attack;
                                    weaponModeKimetsu = 0;
                                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weaponModeKimetsu == 0 ? "Hammer" : "Atgeir"));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void HandleConfigValues()
        {
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };
            configLoadSpecials = base.Config.Bind(new ConfigDefinition("General", "Special weapons"), false, new ConfigDescription("Wether or not to enable special weapons I created", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));

            // BepInEx' KeyboardShortcut class is supported, too
            ShortcutConfig = Config.Bind("Special mode", "Keycodes with modifiers", new KeyboardShortcut(KeyCode.Y), new ConfigDescription("Secret key combination", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        private void AddInputs()
        {
            // Supply your KeyboardShortcut configs to ShortcutConfig instead.
            ShortcutButton = new ButtonConfig
            {
                Name = "SecretShortcut",
                ShortcutConfig = ShortcutConfig,
                HintToken = "$lulzcut"
            };
            InputManager.Instance.AddButton(PluginGUID, ShortcutButton);
        }

        private void AddKimetsusSpecial()
        {
            bool warfareInstalled = false;
            Jotunn.Logger.LogInfo($"Modded prefabs:");
            foreach (var moddedPrefab in ModQuery.GetPrefabs())
            {
                Jotunn.Logger.LogInfo($"  {moddedPrefab.Prefab.name} added by {moddedPrefab.SourceMod.Name}");
                if (moddedPrefab.Prefab.name == "BattlehammerDvergr_TW")
                {
                    warfareInstalled = true;
                    Jotunn.Logger.LogInfo("Found Battlehammer!");
                }
            }

            // Create item config for custom item
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.CraftingStation = CraftingStations.Forge;
            itemConfig.AddRequirement(new RequirementConfig("Silver", 25, 5));
            itemConfig.AddRequirement(new RequirementConfig("ElderBark", 20, 5));
            itemConfig.AddRequirement(new RequirementConfig("WolfFang", 10, 5));
            itemConfig.AddRequirement(new RequirementConfig("TrophyCultist", 1));

            // Create our custom item based of another
            CustomItem item = new CustomItem("KimetsusSpecial_DW", warfareInstalled ? "BattlehammerDvergr_TW" : "SledgeDemolisher", itemConfig);

            // Clone the GameObject of our target item
            GameObject targetItemObject = PrefabManager.Instance.CreateClonedPrefab("BattleaxeCrystalClone", "BattleaxeCrystal");
            ItemDrop targetItem = targetItemObject.GetComponent<ItemDrop>();

            // Create a clone of the original GameObject that was used to create the item (to prevent other changes from other mods)
            GameObject originalItemObject = PrefabManager.Instance.CreateClonedPrefab("SledgeDemolisherClone", warfareInstalled ? "BattlehammerDvergr_TW" : "SledgeDemolisher");
            ItemDrop originalItem = originalItemObject.GetComponent<ItemDrop>();

            GameObject effectItemObject = PrefabManager.Instance.CreateClonedPrefab("AtgeirHimminAflClone", "AtgeirHimminAfl");
            ItemDrop effectItem = effectItemObject.GetComponent<ItemDrop>();

            EffectList.EffectData fxHimminAflHit = new EffectList.EffectData();
            fxHimminAflHit.m_prefab = PrefabManager.Cache.GetPrefab<GameObject>("fx_himminafl_hit");
            fxHimminAflHit.m_enabled = true;
            fxHimminAflHit.m_variant = 0;

            //// Retrieve asset bundle
            marksHarkBundle = AssetUtils.LoadAssetBundleFromResources("markshark_dw");
            var lightningAOEObject = marksHarkBundle.LoadAsset<GameObject>("lightningAOE_no_damage");

            EffectList.EffectData lightningAOE = new EffectList.EffectData();
            //lightningAOE.m_prefab = PrefabManager.Cache.GetPrefab<GameObject>("lightningAOE");
            lightningAOE.m_prefab = lightningAOEObject;
            lightningAOE.m_enabled = true;
            lightningAOE.m_variant = -1;

            EffectList.EffectData[] onlyFxHimminAflHit = new EffectList.EffectData[] { fxHimminAflHit };
            EffectList.EffectData[] onlyLightningAOE = new EffectList.EffectData[] { lightningAOE };

            // Apply target/original/effect item values to custom item
            item.ItemDrop.m_itemData.m_shared = targetItem.m_itemData.m_shared;
            item.ItemDrop.m_itemData.m_shared.m_name = "Kimetsu's Special";
            item.ItemDrop.m_itemData.m_shared.m_description = "May the bones of you enemies forever be crushed, my friend... to dust!";
            item.ItemDrop.m_itemData.m_shared.m_icons = originalItem.m_itemData.m_shared.m_icons;
            item.ItemDrop.m_itemData.m_shared.m_equipStatusEffect = weaponModeEffect.StatusEffect;
            item.ItemDrop.m_itemData.m_shared.m_attack.m_hitEffect.m_effectPrefabs = onlyFxHimminAflHit;

            // Override second attack
            item.ItemDrop.m_itemData.m_shared.m_secondaryAttack = originalItem.m_itemData.m_shared.m_attack;
            item.ItemDrop.m_itemData.m_shared.m_secondaryAttack.m_triggerEffect = originalItem.m_itemData.m_shared.m_triggerEffect;
            item.ItemDrop.m_itemData.m_shared.m_secondaryAttack.m_hitEffect.m_effectPrefabs = onlyLightningAOE;

            // Override other stats
            item.ItemDrop.m_itemData.m_shared.m_skillType = Skills.SkillType.Clubs;
            item.ItemDrop.m_itemData.m_shared.m_movementModifier = -0.05f;
            item.ItemDrop.m_itemData.m_shared.m_damages.m_damage = 0;
            item.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 0;
            item.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 75;
            item.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 5;
            item.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_spirit = 3;
            item.ItemDrop.m_itemData.m_shared.m_attack.m_attackStamina = 18;
            item.ItemDrop.m_itemData.m_shared.m_attackForce = 75;
            item.ItemDrop.m_itemData.m_shared.m_blockPower = 40;
            item.ItemDrop.m_itemData.m_shared.m_blockPowerPerLevel = 5;
            item.ItemDrop.m_itemData.m_durability = 200f;
            item.ItemDrop.m_itemData.m_shared.m_maxDurability = 200f;
            item.ItemDrop.m_itemData.m_shared.m_durabilityPerLevel = 50;
            item.ItemDrop.m_itemData.m_shared.m_maxQuality = 10;

            // Add item to instance and remove this function from the event
            ItemManager.Instance.AddItem(item);
            PrefabManager.OnVanillaPrefabsAvailable -= AddKimetsusSpecial;
        }

        private void AddDirksBoner()
        {
            ItemConfig DirkBonerConfig = new ItemConfig();
            DirkBonerConfig.Name = "Dirk's Boner";
            DirkBonerConfig.Description = "I'm going in hard and I'm going in fast... gotta assert my DOMINANCE!";
            DirkBonerConfig.CraftingStation = CraftingStations.Forge;
            DirkBonerConfig.AddRequirement(new RequirementConfig("Silver", 25, 5));
            DirkBonerConfig.AddRequirement(new RequirementConfig("ElderBark", 10, 5));
            DirkBonerConfig.AddRequirement(new RequirementConfig("FreezeGland", 5, 3));
            DirkBonerConfig.AddRequirement(new RequirementConfig("YmirRemains", 5));

            CustomItem DirksBoner = new CustomItem("DirksBoner_DW", "SwordMistwalker", DirkBonerConfig);

            // Stats
            DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_damage = 0;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 35;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_frost = 40;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_spirit = 20;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_attack.m_attackStamina = 12;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_blockPower = 30;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_attackForce = 80;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_deflectionForce = 30;

            // Quality
            DirksBoner.ItemDrop.m_itemData.m_shared.m_maxQuality = 10;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 3;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_frost = 3;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_spirit = 3;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_blockPowerPerLevel = 5;
            DirksBoner.ItemDrop.m_itemData.m_shared.m_durabilityPerLevel = 50;

            ItemManager.Instance.AddItem(DirksBoner);
            PrefabManager.OnVanillaPrefabsAvailable -= AddDirksBoner;
        }

        private void AddMarksHark()
        {
            // var EmbeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("lightningaoe_no_damage");
            // GameObject lightningAOEPrefab = EmbeddedResourceBundle.LoadAsset<GameObject>("lightningaoe_no_damage");

            // Create item config for custom item
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.CraftingStation = CraftingStations.Forge;
            itemConfig.AddRequirement(new RequirementConfig("Silver", 30, 5));
            itemConfig.AddRequirement(new RequirementConfig("ElderBark", 15, 5));
            itemConfig.AddRequirement(new RequirementConfig("FreezeGland", 7, 3));
            itemConfig.AddRequirement(new RequirementConfig("YmirRemains", 5));

            ////// Create our custom item based of another
            ////CustomItem item = new CustomItem("MarksHark_DW", "Cultivator", itemConfig);

            //// Clone the GameObject of our target item
            //GameObject targetItemObject = PrefabManager.Instance.CreateClonedPrefab("AtgeirHimminAflClone", "AtgeirHimminAfl");
            //ItemDrop targetItem = targetItemObject.GetComponent<ItemDrop>();

            //// Create a clone of the original GameObject that was used to create the item (to prevent other changes from other mods)
            //GameObject originalItemObject = PrefabManager.Instance.CreateClonedPrefab("CultivatorClone", "Cultivator");
            //ItemDrop originalItem = originalItemObject.GetComponent<ItemDrop>();

            //// Create a clone of the original GameObject that was used to create the item (to prevent other changes from other mods)
            //GameObject itemObject = PrefabManager.Instance.CreateClonedPrefab("MarksHark_DW", "Cultivator");
            //ItemDrop item = itemObject.GetComponent<ItemDrop>();

            //EffectList.EffectData lightningAOE = new EffectList.EffectData();
            //// lightningAOE.m_prefab = PrefabManager.Cache.GetPrefab<GameObject>("fx_himminafl_aoe");
            //lightningAOE.m_prefab = lightningAOEPrefab;
            //lightningAOE.m_enabled = true;
            //lightningAOE.m_variant = -1;

            //EffectList.EffectData[] onlyLightningAOE = new EffectList.EffectData[] { lightningAOE };

            //// Apply target/original item values to custom item
            //item.m_itemData.m_shared = targetItem.m_itemData.m_shared;
            //item.m_itemData.m_shared.m_name = "Mark's Hark";
            //item.m_itemData.m_shared.m_description = "I'mma steal them sweet lewds from Durk! Grabby grabby!";
            //item.m_itemData.m_shared.m_icons = originalItem.m_itemData.m_shared.m_icons;
            //item.m_itemData.m_shared.m_secondaryAttack.m_triggerEffect.m_effectPrefabs = onlyLightningAOE;

            //// Reduce range on attack
            //item.m_itemData.m_shared.m_attack.m_attackRange = 2.5f;

            // Retrieve asset bundle
            // AssetBundle marksHarkBundle = AssetUtils.LoadAssetBundleFromResources("markshark_dw");

            // Add item to instance and remove this function from the event
            ItemManager.Instance.AddItem(new CustomItem(marksHarkBundle, "MarksHark_DW", true, itemConfig));
            PrefabManager.OnVanillaPrefabsAvailable -= AddMarksHark;
        }

        // Add new status effects
        private void AddStatusEffects()
        {
            StatusEffect effect = ScriptableObject.CreateInstance<StatusEffect>();
            effect.name = "WeaponMode";
            effect.m_name = "Weapon Mode";
            effect.m_icon = AssetUtils.LoadSpriteFromFile("Package/Assets/rune_test.png");
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = "Start";
            effect.m_stopMessageType = MessageHud.MessageType.Center;
            effect.m_stopMessage = "Stop";

            Jotunn.Logger.LogInfo(effect.m_icon != null);

            weaponModeEffect = new CustomStatusEffect(effect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(weaponModeEffect);
        }
    }
}
