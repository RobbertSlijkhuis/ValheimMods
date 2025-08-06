using BepInEx;
using Jotunn.Configs;

// using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_EarthStaffs.Components;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using System.Collections.Generic;

// using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static AudioMan;
using static EffectList;
// using static EffectList;

namespace ModularMagic_EarthStaffs
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("DeathWizsh.ModularMagic_Core")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ModularMagic_EarthStaffs : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.ModularMagic_EarthStaffs";
        public const string PluginName = "ModularMagic_EarthStaffs";
        public const string PluginVersion = "0.0.1";
        public static ModularMagic_EarthStaffs Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        private AssetBundle assetBundle;
        public CustomPrefabs prefabs = new CustomPrefabs();
        public CustomStatusEffects effects = new CustomStatusEffects();
        public CustomSprites sprites = new CustomSprites();
        public ItemDataSnaphot snapshots = new ItemDataSnaphot();
        public static Skills.SkillType customSkill;

        public static readonly string imbuementDataKey = "Imbuements_MMES";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            InitAssetBundle();
            PluginConfig.Init();
            // InitStatusEffects();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += AddSkill;
            PrefabManager.OnVanillaPrefabsAvailable += AddEarthStaffs;
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

        private void AddSkill()
        {
            SkillConfig skillConfig = new SkillConfig();
            skillConfig.Identifier = PluginGUID;
            skillConfig.Name = "Earth magic proficiency";
            skillConfig.Description = "This skill shows how proficient you are with Earth magic and unlocks imbuements";
            skillConfig.IncreaseStep = 1;
            skillConfig.Icon = sprites.RootsCooldown;

            customSkill = SkillManager.Instance.AddSkill(skillConfig);
            PrefabManager.OnVanillaPrefabsAvailable -= AddSkill;
        }

        private void AddEarthStaffs()
        {
            snapshots.staffEarth1 = prefabs.staffEarth1.GetComponent<ItemDrop>().m_itemData.Clone();
            snapshots.staffEarth2 = prefabs.staffEarth2.GetComponent<ItemDrop>().m_itemData.Clone();
            snapshots.staffEarth3 = prefabs.staffEarth3.GetComponent<ItemDrop>().m_itemData.Clone();

            prefabs.staffEarth1.AddComponent<Imbuements>();
            prefabs.staffEarth2.AddComponent<Imbuements>();
            prefabs.staffEarth3.AddComponent<Imbuements>();

            ItemHelper.CreateStaff(prefabs.staffEarth0, PluginConfig.staffEarth0);
            ItemHelper.CreateStaff(prefabs.staffEarth1, PluginConfig.staffEarth1);
            ItemHelper.CreateStaff(prefabs.staffEarth2, PluginConfig.staffEarth2);
            ItemHelper.CreateStaff(prefabs.staffEarth3, PluginConfig.staffEarth3);

            PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaffs;
        }

        private void InitStatusEffects()
        {
            //effects.Staff2Cooldown = ScriptableObject.CreateInstance<StatusEffect>();
            //effects.Staff2Cooldown.name = PluginConfig.staffEarth2.secondaryAttackConfig.cooldownStatusEffectName;
            //effects.Staff2Cooldown.m_name = "Summon boulder cooldown";
            //effects.Staff2Cooldown.m_icon = assetBundle.LoadAsset<Sprite>("StaffEarth2Sprite_MMES");
            //effects.Staff2Cooldown.m_startMessageType = MessageHud.MessageType.Center;
            //effects.Staff2Cooldown.m_startMessage = "";
            //effects.Staff2Cooldown.m_stopMessageType = MessageHud.MessageType.Center;
            //effects.Staff2Cooldown.m_stopMessage = "";
            //effects.Staff2Cooldown.m_tooltip = "Be patient!";
            //effects.Staff2Cooldown.m_ttl = PluginConfig.staffEarth2.secondaryAttackConfig.cooldown.Value;
            //ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Staff2Cooldown, fixReference: false));

            //effects.Staff3Cooldown = ScriptableObject.CreateInstance<StatusEffect>();
            //effects.Staff3Cooldown.name = PluginConfig.staffEarth3.secondaryAttackConfig.cooldownStatusEffectName;
            //effects.Staff3Cooldown.m_name = "Summon roots cooldown";
            //effects.Staff3Cooldown.m_icon = assetBundle.LoadAsset<Sprite>("StaffEarth3Sprite_MMES");
            //effects.Staff3Cooldown.m_startMessageType = MessageHud.MessageType.Center;
            //effects.Staff3Cooldown.m_startMessage = "";
            //effects.Staff3Cooldown.m_stopMessageType = MessageHud.MessageType.Center;
            //effects.Staff3Cooldown.m_stopMessage = "";
            //effects.Staff3Cooldown.m_tooltip = "Be patient!";
            //effects.Staff3Cooldown.m_ttl = PluginConfig.staffEarth3.secondaryAttackConfig.cooldown.Value;
            //ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.Staff3Cooldown, fixReference: false));

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
        }

        /**
         * Initialise the asset bundle of the mod
         */
        private void InitAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("modularmagic_earthstaffs_dw");

            prefabs.staffEarth0 = assetBundle.LoadAsset<GameObject>("MMES_TheForestFlinger");
            prefabs.staffEarth1 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth1");
            prefabs.staffEarth2 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth2");
            prefabs.staffEarth3 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth3");
            prefabs.projectileMushroom = assetBundle.LoadAsset<GameObject>("projectile_mushroom_MMES");
            //prefabs.projectileBoulder = assetBundle.LoadAsset<GameObject>("projectile_boulder_MMES");
            //prefabs.Root = assetBundle.LoadAsset<GameObject>("Root_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileMushroom, true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.projectileBoulder, true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.Root, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("projectile_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("projectile_spawn_boulder_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("script_boulder_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("script_roots_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("root_attack_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_mushroom_projectile_hit_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_sledge_demolisher_hit_small_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_sledge_demolisher_hit_large_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spores_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spikes_MMES"), true));
            //PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_windup_MMES"), true));

            //sprites.BoulderCooldown = assetBundle.LoadAsset<Sprite>("StaffEarth2Sprite_MMES");
            sprites.RootsCooldown = assetBundle.LoadAsset<Sprite>("StaffEarth3Sprite_MMES");
        }
    }
}

