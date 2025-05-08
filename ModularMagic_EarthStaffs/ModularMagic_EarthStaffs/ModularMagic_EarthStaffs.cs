using BepInEx;
// using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
// using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
// using static EffectList;

namespace ModularMagic_EarthStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_EarthStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_EarthStaffs";
        public const string PluginName = "ModularMagic_EarthStaffs";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_EarthStaffs Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            ConfigStaffs.Init();
            // InitStatusEffects();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddEarthStaffs;
            Jotunn.Logger.LogInfo("ModularMagic_EarthStaffs has been initialised");
            ItemManager.OnItemsRegistered += LogRecipes;
        }

        private void LogRecipes()
        {
            ObjectDB.instance.m_recipes.ForEach(r =>
            {
                if (r.name.Contains("MMES"))
                    Jotunn.Logger.LogInfo(r.name);
            });

            ItemManager.OnItemsRegistered -= LogRecipes;
        }

        private void AddEarthStaffs()
        {
            //// Create a copy of the demolisher hit fx and make it bigger and more awesome
            //GameObject earthSledgePrefab = PrefabManager.Instance.CreateClonedPrefab("fx_staff_earth_impact_MMES", "fx_sledge_demolisher_hit");
            //Transform bubble = earthSledgePrefab.transform.Find("bubble wave");
            //Transform cloud = earthSledgePrefab.transform.Find("Cloud");
            //Transform rocks = earthSledgePrefab.transform.Find("vfx_troll_rock_destroyed");
            //Transform splosh = earthSledgePrefab.transform.Find("vfx_Splosh (1)");
            //bubble.localScale = new Vector3(2, 2, 2);
            //cloud.gameObject.SetActive(true);
            //rocks.gameObject.SetActive(true);
            //splosh.gameObject.SetActive(true);
            //PrefabManager.Instance.AddPrefab(earthSledgePrefab);

            //// Get the prefab used for the big rock (secondary attack) and apply our fx
            //GameObject secondaryProjectilePrefab = PrefabManager.Instance.GetPrefab(prefabs.projectileBigStonePrefab.name);
            //Projectile projComp = secondaryProjectilePrefab.GetComponent<Projectile>();
            //EffectData effect = new EffectData();
            //effect.m_prefab = earthSledgePrefab;
            //effect.m_enabled = true;
            //effect.m_variant = -1;
            //List<EffectData> effectList = new List<EffectData> { effect };
            //projComp.m_hitEffects.m_effectPrefabs = effectList.ToArray();

            ItemHelper.CreateStaff(prefabs.staffEarth0Prefab, ConfigStaffs.staffEarth0);
            ItemHelper.CreateStaff(prefabs.staffEarth1Prefab, ConfigStaffs.staffEarth1);
            ItemHelper.CreateStaff(prefabs.staffEarth2Prefab, ConfigStaffs.staffEarth2);
            ItemHelper.CreateStaff(prefabs.staffEarth3Prefab, ConfigStaffs.staffEarth3);

            PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaffs;
        }

        //private void InitStatusEffects()
        //{
        //    StatusEffect cooldownEffect = ScriptableObject.CreateInstance<StatusEffect>();
        //    cooldownEffect.name = ConfigStaffs.staffEarth3CooldownStatusEffectName;
        //    cooldownEffect.m_name = "Summon roots cooldown";
        //    cooldownEffect.m_icon = assetBundle.LoadAsset<Sprite>("StaffEarth3Sprite_DW");
        //    cooldownEffect.m_startMessageType = MessageHud.MessageType.Center;
        //    cooldownEffect.m_startMessage = "";
        //    cooldownEffect.m_stopMessageType = MessageHud.MessageType.Center;
        //    cooldownEffect.m_stopMessage = "";
        //    cooldownEffect.m_tooltip = "Be patient!";
        //    cooldownEffect.m_ttl = ConfigStaffs.staffEarth3.secondaryCooldown.Value;
        //    ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(cooldownEffect, fixReference: false));

        //    StatusEffect exhaustAndFhoulMagicEffect = ScriptableObject.CreateInstance<StatusEffect>();
        //    exhaustAndFhoulMagicEffect.name = "ExhaustAndFoulMagicEffect_DW";
        //    exhaustAndFhoulMagicEffect.m_name = "Exhausted by foul magic";
        //    exhaustAndFhoulMagicEffect.m_icon = assetBundle.LoadAsset<Sprite>("staffEarth3.Sprite");
        //    exhaustAndFhoulMagicEffect.m_startMessageType = MessageHud.MessageType.Center;
        //    exhaustAndFhoulMagicEffect.m_startMessage = "";
        //    exhaustAndFhoulMagicEffect.m_stopMessageType = MessageHud.MessageType.Center;
        //    exhaustAndFhoulMagicEffect.m_stopMessage = "";
        //    exhaustAndFhoulMagicEffect.m_tooltip = "You are exhausted by the use of foul magic, reducing your strength and magic effectiveness";
        //    exhaustAndFhoulMagicEffect.m_ttl = 300;
        //    ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(exhaustAndFhoulMagicEffect, fixReference: false));
        //}

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_earthstaffs_dw");

            // Earth assets
            prefabs.staffEarth0Prefab = assetBundle.LoadAsset<GameObject>("MMES_TheForestFlinger");
            prefabs.staffEarth1Prefab = assetBundle.LoadAsset<GameObject>("MMES_Tremorbranch");
            prefabs.staffEarth2Prefab = assetBundle.LoadAsset<GameObject>("MMES_EarthwhorlTheAvalanche");
            prefabs.staffEarth3Prefab = assetBundle.LoadAsset<GameObject>("MMES_OakrendTheEarthcaller");
            prefabs.projectileMushroomPrefab = assetBundle.LoadAsset<GameObject>("staff_earth_projectile_mushroom_MMES");
            //prefabs.projectileBigStonePrefab = assetBundle.LoadAsset<GameObject>("staff_earth_projectile_big_stone_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_earth_projectile_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileMushroomPrefab, true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileBigStonePrefab, true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_earth_projectile_spawn_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_earth_script_big_stone_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("staff_earth_script_roots_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("tenta_root_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spores_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spikes_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_windup_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_mushroom_projectile_hit_MMES"), true));
        }
    }
}

