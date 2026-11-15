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
using LightColorPresetType = ModularMagic_Core.Types.LightColorPresetType;
using LocaleEnglish = ModularMagic_BloodMagic.Locale.LocaleEnglish;
using RuneId = ModularMagic_Core.Types.RuneId;
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
        // Core speeds up the swing for the Attack Speed rune. The Eitr Cost rune lowers the eitr of a charging hit, which this mod does
        // itself (SoulReaperHelper.GetChargeEitr): a swing of a scythe costs no eitr, so there is nothing for Core to lower
        private void RegisterStaffHooks()
        {
            definition = new StaffDefinition()
            {
                effectName = "Imbuements_MMBM",
                weaponType = WeaponType.MMBM,
                colorPreset = LightColorPresetType.Purple,
                allowedRunes = new string[] { RuneId.AttackSpeed, RuneId.EitrCost },
                imbuementConfig = PluginConfig.imbuementConfig,
                attackSpeedAnimation = true,
            };

            StaffImbuementHelper.Register(definition);
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
                // The slots and tiers follow the other staff mods. The config is bound in Awake, so it is ready here
                definition.AddStaff(prefabs.Scythe1, PluginConfig.scythe1, 2, 1);
                definition.AddStaff(prefabs.Scythe2, PluginConfig.scythe2, 3, 2);
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
        }
    }
}

