using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Helpers;
using ModularMagic_BloodMagic.Models;
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
            SpawnAbility? spawnAbility = prefabs.ScytheSkeletonSpawn.GetComponent<SpawnAbility>();
            if (spawnAbility == null)
            {
                Jotunn.Logger.LogError("ScytheSkeletonSpawn is missing a SpawnAbility component.");
                PrefabManager.OnVanillaPrefabsAvailable -= SetupPrefabs;
                return;
            }

            string summonPrefabName = PluginConfig.scythe1.summonPrefab?.Value ?? "Skeleton";

            GameObject? originalPrefab = PrefabManager.Instance.GetPrefab(summonPrefabName);
            if (originalPrefab == null)
            {
                Jotunn.Logger.LogError($"Could not find summon prefab '{summonPrefabName}' via PrefabManager.");
                PrefabManager.OnVanillaPrefabsAvailable -= SetupPrefabs;
                return;
            }

            GameObject friendlyPrefab = PrefabManager.Instance.CreateClonedPrefab(
                $"{summonPrefabName}_MMBM_Friendly", summonPrefabName);

            Tameable tameable                       = friendlyPrefab.AddComponent<Tameable>();
            tameable.m_fedDuration                  = 0f;
            tameable.m_tamingTime                   = 0f;
            tameable.m_startsTamed                  = true;
            tameable.m_unsummonDistance             = 150f;
            tameable.m_unsummonOnOwnerLogoutSeconds = 120f;
            tameable.m_levelUpOwnerSkill            = Skills.SkillType.BloodMagic;
            tameable.m_levelUpFactor                = 0.5f;

            spawnAbility.m_spawnPrefab = new GameObject[1] { friendlyPrefab };

            Jotunn.Logger.LogInfo($"SetupPrefabs complete — spawn prefab set to '{friendlyPrefab.name}'");
            PrefabManager.OnVanillaPrefabsAvailable -= SetupPrefabs;
        }

        private void AddItems()
        {
            ItemHelper.Create(prefabs.Scythe1, PluginConfig.scythe1);
            ItemHelper.Create(prefabs.Scythe2, PluginConfig.scythe2);

            PrefabManager.OnVanillaPrefabsAvailable -= AddItems;
        }

        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_bloodmagic_dw");

            prefabs.Scythe1             = assetBundle.LoadAsset<GameObject>("MMBM_Scythe1");
            prefabs.Scythe2             = assetBundle.LoadAsset<GameObject>("MMBM_Scythe2");
            prefabs.ScytheSkeletonSpawn = assetBundle.LoadAsset<GameObject>("spawn_skeleton_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ScytheSkeletonSpawn, true));
            prefabs.ScytheHitEffect     = assetBundle.LoadAsset<GameObject>("vfx_scythe_hit_MMBM");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ScytheHitEffect, true));
        }
    }
}

