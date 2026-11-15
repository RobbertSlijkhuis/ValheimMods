using BepInEx;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Helpers;
using ModularMagic_BloodMagic.Models;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaponType = ModularMagic_Core.Types.WeaponType;
using ImbuedStaffHelper = ModularMagic_Core.Helpers.ImbuedStaffHelper;
using ItemUpgradeHelper = ModularMagic_Core.Helpers.ItemUpgradeHelper;
using LightColorPresetType = ModularMagic_Core.Types.LightColorPresetType;
using LocaleEnglish = ModularMagic_BloodMagic.Locale.LocaleEnglish;
using RuneId = ModularMagic_Core.Types.RuneId;
using SecondaryAttackHelper = ModularMagic_Core.Helpers.SecondaryAttackHelper;
using StaffDefinition = ModularMagic_Core.Models.StaffDefinition;
using StaffImbuementHelper = ModularMagic_Core.Helpers.StaffImbuementHelper;

namespace ModularMagic_BloodMagic
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_BloodMagic : BaseUnityPlugin
    {
        public const string PluginGUID    = "DeathWizsh.ModularMagic_BloodMagic";
        public const string PluginName    = "ModularMagic_BloodMagic";
        public const string PluginVersion = "0.0.3";
        public static ModularMagic_BloodMagic Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();
        // What Core needs to know about the scythes, see RegisterStaffHooks
        public StaffDefinition definition;
        // The texts of the runes on the scythes, see LocaleEnglish
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            LocaleEnglish.Init();
            PluginConfig.Init();
            // Every entry forwards its change to its own handler from this event in the order they were created,
            // so this runs after the handlers that update the prefabs, and the scythe in the hand gets the new values with its runes
            Config.SettingChanged += (sender, args) => ImbuedStaffHelper.ReapplyOnHeldStaff();
            RegisterStaffHooks();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += SetupPrefabs;
            PrefabManager.OnVanillaPrefabsAvailable += AddItems;
        }

        // The map markers of the husks of the local player, see HuskMarkerHelper
        private void Update()
        {
            HuskMarkerHelper.Update();
        }

        // Core gives the scythes their rune slots and applies the runes when a scythe is equipped. It only needs to know what belongs to this mod.
        // Core speeds up the swing for the Attack Speed rune and swaps the animations for the Greatsword rune. The Eitr Cost rune lowers the eitr
        // of a charging hit, which this mod does itself (SoulReaperHelper.GetChargeEitr): a swing of a scythe costs no eitr, so there is nothing
        // for Core to lower. The Range and Legion runes are read by this mod too (ScytheRangeHelper, ApparitionHelper.GetMaxApparitions)
        private void RegisterStaffHooks()
        {
            List<string> runes = new List<string>() { RuneId.AttackSpeed, RuneId.EitrCost, RuneId.Range, RuneId.Legion, RuneId.Greatsword };

            // A secondary attack whose prefabs are not in the asset bundle has no config (see PluginConfig), its rune is then left out
            bool nova = PluginConfig.secondaryAttackNova != null;
            bool bloodTap = PluginConfig.secondaryAttackBloodTap != null;

            if (nova)
                runes.Add(RuneId.Nova);

            if (bloodTap)
                runes.Add(RuneId.BloodTap);

            definition = new StaffDefinition()
            {
                effectName = "Imbuements_MMBM",
                weaponType = WeaponType.MMBM,
                colorPreset = LightColorPresetType.Blood,
                allowedRunes = runes.ToArray(),
                imbuementConfig = PluginConfig.imbuementConfig,
                attackSpeedAnimation = true,
            };

            // The swing of a greatsword also has a chain of three hits, like the swing of the scythe
            definition.AddMainAttackAnimation("Greatsword", "greatsword", 3, ItemDrop.ItemData.AnimationState.Greatsword);

            // Core gives a registered secondary attack its own eitr cost and the cooldown, the attack is recognised by the object it spawns.
            // Both pay per burst, so the cooldown starts when the burst fired (a Blood Tap without the health for it starts no cooldown)
            if (nova)
            {
                definition.AddSecondaryAttack("Nova", prefabs.SecondaryAttackNova, PluginConfig.secondaryAttackNova);
                SecondaryAttackHelper.Register(prefabs.AttackNova.name, PluginConfig.secondaryAttackNova.cooldownStatusEffectName, "Nova cooldown",
                    () => PluginConfig.secondaryAttackNova.cooldown.Value, NovaHelper.PrepareStart);
            }

            if (bloodTap)
            {
                definition.AddSecondaryAttack("BloodTap", prefabs.SecondaryAttackBloodTap, PluginConfig.secondaryAttackBloodTap);
                SecondaryAttackHelper.Register(prefabs.AttackBloodTap.name, PluginConfig.secondaryAttackBloodTap.cooldownStatusEffectName, "Blood Tap cooldown",
                    () => PluginConfig.secondaryAttackBloodTap.cooldown.Value, BloodTapHelper.PrepareStart);
            }

            StaffImbuementHelper.Register(definition);

            // An upgraded scythe is a new item, its id moves along so the limit of apparitions follows the upgrade
            ItemUpgradeHelper.OnUpgraded += ApparitionHelper.OnScytheUpgraded;
        }

        private void SetupPrefabs()
        {
            materials.Charred.FixReferences();

            List<string> prefabNames = new List<string>() { "Charred_Melee", "Charred_Twitcher", "Charred_Archer" };
            List<GameObject> apparitionPrefabs = new List<GameObject>();

            foreach (string prefabName in prefabNames)
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);
                if (prefab == null)
                {
                    Jotunn.Logger.LogError($"Could not find '{prefabName}' prefab via PrefabManager.");
                    PrefabManager.OnVanillaPrefabsAvailable -= SetupPrefabs;
                    return;
                }

                if (prefab.GetComponent<ApparitionController>() == null)
                    prefab.AddComponent<ApparitionController>();

                // Added to vanilla prefab — high taming time prevents natural taming
                // MMBM apparitions bypass this via MakeTame() in ApparitionController
                Tameable tameable                       = prefab.AddComponent<Tameable>();
                // The config "Despawn when away" can turn both off for apparitions, see ApparitionHelper.CanUnsummon. Its description
                // names these values, so change it with them
                tameable.m_unsummonDistance             = 150f;
                tameable.m_unsummonOnOwnerLogoutSeconds = 120f;
                tameable.m_levelUpOwnerSkill            = Skills.SkillType.BloodMagic;
                tameable.m_levelUpFactor                = 0.5f;

                // Clear consumable items on MonsterAI so the creature cannot be naturally tamed
                MonsterAI? monsterAI = prefab.GetComponent<MonsterAI>();
                if (monsterAI != null)
                    monsterAI.m_consumeItems = new List<ItemDrop>();

                apparitionPrefabs.Add(prefab);
            }

            GameObject melee    = apparitionPrefabs[0];
            GameObject twitcher = apparitionPrefabs[1];
            GameObject archer   = apparitionPrefabs[2];

            prefabs.SpawnAbilityScythe1.GetComponent<SpawnAbility>().m_spawnPrefab = new GameObject[2] { melee, twitcher };
            prefabs.SpawnAbilityScythe2.GetComponent<SpawnAbility>().m_spawnPrefab = new GameObject[3] { melee, twitcher, archer };
            prefabs.SpawnAbilityScythe3.GetComponent<SpawnAbility>().m_spawnPrefab = new GameObject[3] { melee, twitcher, archer };

            // The spawn abilities spawn the apparitions, ApparitionHelper sets them up from the scythe
            ApparitionHelper.Setup();

            Jotunn.Logger.LogInfo("SetupPrefabs complete — apparition prefabs assigned to all scythes.");
            PrefabManager.OnVanillaPrefabsAvailable -= SetupPrefabs;
        }

        private void AddItems()
        {
            try
            {
                // The slots follow the other staff mods. The scythes start a biome later than the staffs (swamp, plains and ashlands), so the
                // first two have a tier more; 4 is the highest tier. The config is bound in Awake, so it is ready here
                definition.AddStaff(prefabs.Scythe1, PluginConfig.scythe1, 2, 2);
                definition.AddStaff(prefabs.Scythe2, PluginConfig.scythe2, 3, 3);
                definition.AddStaff(prefabs.Scythe3, PluginConfig.scythe3, 4, 4);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in adding the scythes: " + e);
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddItems;
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_bloodmagic_dw");

            materials.Charred = assetBundle.LoadAsset<Material>("Charred_mat_MMBM");
            prefabs.Scythe1 = assetBundle.LoadAsset<GameObject>("MMBM_Scythe1");
            prefabs.Scythe2 = assetBundle.LoadAsset<GameObject>("MMBM_Scythe2");
            prefabs.Scythe3 = assetBundle.LoadAsset<GameObject>("MMBM_Scythe3");
            prefabs.SpawnAbilityScythe1 = assetBundle.LoadAsset<GameObject>("spawn_scythe1_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SpawnAbilityScythe1, true));
            prefabs.SpawnAbilityScythe2 = assetBundle.LoadAsset<GameObject>("spawn_scythe2_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SpawnAbilityScythe2, true));
            prefabs.SpawnAbilityScythe3 = assetBundle.LoadAsset<GameObject>("spawn_scythe3_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SpawnAbilityScythe3, true));
            prefabs.SporeEffect = assetBundle.LoadAsset<GameObject>("fx_spores_blood_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SporeEffect, true));
            prefabs.SpikeEffect = assetBundle.LoadAsset<GameObject>("fx_spikes_blood_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SpikeEffect, true));

            // The secondary attacks of the Nova and Blood Tap runes. The wrapper holds the attack that is copied onto the scythe, the attack
            // spawns the trigger locally on the client of the caster when it fires. Neither is registered, the trigger has no ZNetView
            prefabs.SecondaryAttackNova = LoadSecondaryAttack("secondary_nova_MMBM", "attack_nova_MMBM", out prefabs.AttackNova);
            prefabs.SecondaryAttackBloodTap = LoadSecondaryAttack("secondary_bloodtap_MMBM", "attack_bloodtap_MMBM", out prefabs.AttackBloodTap);

            if (prefabs.AttackNova != null)
                prefabs.AttackNova.AddComponent<NovaTrigger>();

            if (prefabs.AttackBloodTap != null)
                prefabs.AttackBloodTap.AddComponent<BloodTapTrigger>();

            // The start effects of the attacks above. Registered so Jotunn fixes their references to vanilla assets (the mocks)
            prefabs.NovaEffect = AddEffect("fx_nova_MMBM");
            prefabs.BloodTapEffect = AddEffect("fx_bloodtap_MMBM");
        }

        // Loads an effect and registers it with its references fixed. Null and logged when it is not in the asset bundle
        private GameObject? AddEffect(string name)
        {
            GameObject? effect = assetBundle.LoadAsset<GameObject>(name);

            if (effect == null)
            {
                Jotunn.Logger.LogError($"'{name}' is not in the asset bundle");
                return null;
            }

            PrefabManager.Instance.AddPrefab(new CustomPrefab(effect, true));
            return effect;
        }

        // Loads the wrapper and the trigger of a secondary attack. Both are null when one of them is not in the asset bundle, the rune of the
        // attack is then left out (see RegisterStaffHooks)
        private GameObject? LoadSecondaryAttack(string wrapperName, string triggerName, out GameObject? trigger)
        {
            GameObject? wrapper = assetBundle.LoadAsset<GameObject>(wrapperName);
            trigger = assetBundle.LoadAsset<GameObject>(triggerName);

            if (wrapper == null || trigger == null)
            {
                Jotunn.Logger.LogError($"'{wrapperName}' or '{triggerName}' is not in the asset bundle, its rune is left out");
                trigger = null;
                return null;
            }

            if (trigger.GetComponent<ZNetView>() != null)
                Jotunn.Logger.LogError($"'{triggerName}' has a ZNetView, it is only spawned by the caster and must not have one");

            // The attack spawns the trigger and pays its costs when it fires, not when it starts. The game then checks the health of Blood Tap
            // before it pays (without enough it stops the attack), and Core starts the cooldown only when it fired. Set here, so the prefab
            // can not get it wrong. Core copies this attack onto the scythe on equip
            Attack attack = wrapper.GetComponent<ItemDrop>().m_itemData.m_shared.m_attack;

            if (attack.m_attackType != Attack.AttackType.Projectile)
                Jotunn.Logger.LogError($"The attack of '{wrapperName}' must be of the type Projectile, or it does not spawn '{triggerName}'");

            attack.m_attackProjectile = trigger;
            attack.m_perBurstResourceUsage = true;
            attack.m_attackHealthLowBlockUse = true;

            return wrapper;
        }
    }
}

