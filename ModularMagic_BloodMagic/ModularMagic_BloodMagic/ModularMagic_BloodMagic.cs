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

namespace ModularMagic_BloodMagic
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class ModularMagic_BloodMagic : BaseUnityPlugin
    {
        public const string PluginGUID    = "DeathWizsh.ModularMagic_BloodMagic";
        public const string PluginName    = "ModularMagic_BloodMagic";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_BloodMagic Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomMaterials materials = new CustomMaterials();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += SetupPrefabs;
            PrefabManager.OnVanillaPrefabsAvailable += AddItems;
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
            prefabs.SpawnAbilityScythe4.GetComponent<SpawnAbility>().m_spawnPrefab = new GameObject[1] { twitcher };

            Jotunn.Logger.LogInfo("SetupPrefabs complete — apparition prefabs assigned to all scythes.");
            PrefabManager.OnVanillaPrefabsAvailable -= SetupPrefabs;
        }

        private void AddItems()
        {
            ItemHelper.Create(prefabs.Scythe1, PluginConfig.scythe1);
            ItemHelper.Create(prefabs.Scythe2, PluginConfig.scythe2);
            ItemHelper.Create(prefabs.Scythe3, PluginConfig.scythe3);
            ItemHelper.Create(prefabs.Scythe4, PluginConfig.scythe4);

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

            prefabs.Scythe4             = assetBundle.LoadAsset<GameObject>("MMBM_Scythe4");
            prefabs.SpawnAbilityScythe4 = assetBundle.LoadAsset<GameObject>("spawn_scythe4_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.SpawnAbilityScythe4, true));
        }
    }
}

