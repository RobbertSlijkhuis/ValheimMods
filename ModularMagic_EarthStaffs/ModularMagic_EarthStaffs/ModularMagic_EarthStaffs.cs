using BepInEx;
using Jotunn.Configs;

using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ModularMagic_EarthStaffs.Components;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using System.Reflection;
using UnityEngine;

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
            InitStatusEffects();
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
            skillConfig.Identifier = "Skill_MMES";
            skillConfig.Name = "Earth magig";
            skillConfig.Description = "This skill shows how proficient you are with Earth magic and unlocks imbuements";
            skillConfig.IncreaseStep = 1;
            skillConfig.Icon = sprites.RootsCooldown;

            customSkill = SkillManager.Instance.AddSkill(skillConfig);
            PrefabManager.OnVanillaPrefabsAvailable -= AddSkill;
        }

        private void AddEarthStaffs()
        {
            snapshots.staffEarth1 = prefabs.StaffEarth1.GetComponent<ItemDrop>().m_itemData.Clone();
            snapshots.staffEarth2 = prefabs.StaffEarth2.GetComponent<ItemDrop>().m_itemData.Clone();
            snapshots.staffEarth3 = prefabs.StaffEarth3.GetComponent<ItemDrop>().m_itemData.Clone();

            prefabs.StaffEarth1.AddComponent<Imbuements>();
            prefabs.StaffEarth2.AddComponent<Imbuements>();
            prefabs.StaffEarth3.AddComponent<Imbuements>();

            ItemHelper.CreateStaff(prefabs.StaffEarth0, PluginConfig.staffEarth0);
            ItemHelper.CreateStaff(prefabs.StaffEarth1, PluginConfig.staffEarth1);
            ItemHelper.CreateStaff(prefabs.StaffEarth2, PluginConfig.staffEarth2);
            ItemHelper.CreateStaff(prefabs.StaffEarth3, PluginConfig.staffEarth3);

            AttackHelper.UpdateBoulder(prefabs.SecondaryAttackBoulder, prefabs.ProjectileBoulder, PluginConfig.secondaryAttackBoulder);
            AttackHelper.UpdateRoots(prefabs.SecondaryAttackRoots, prefabs.Root, PluginConfig.secondaryAttackRoots);

            PrefabManager.OnVanillaPrefabsAvailable -= AddEarthStaffs;
        }

        private void InitStatusEffects()
        {
            effects.BoulderCooldown = ScriptableObject.CreateInstance<StatusEffect>();
            effects.BoulderCooldown.name = PluginConfig.secondaryAttackBoulder.cooldownStatusEffectName;
            effects.BoulderCooldown.m_name = "Boulder cooldown";
            effects.BoulderCooldown.m_icon = sprites.BoulderCooldown;
            effects.BoulderCooldown.m_startMessageType = MessageHud.MessageType.Center;
            effects.BoulderCooldown.m_startMessage = "";
            effects.BoulderCooldown.m_stopMessageType = MessageHud.MessageType.Center;
            effects.BoulderCooldown.m_stopMessage = "";
            effects.BoulderCooldown.m_tooltip = "Be patient!";
            effects.BoulderCooldown.m_ttl = PluginConfig.secondaryAttackBoulder.cooldown.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.BoulderCooldown, fixReference: false));

            effects.RootsCooldown = ScriptableObject.CreateInstance<StatusEffect>();
            effects.RootsCooldown.name = PluginConfig.secondaryAttackRoots.cooldownStatusEffectName;
            effects.RootsCooldown.m_name = "Summon roots cooldown";
            effects.RootsCooldown.m_icon = sprites.RootsCooldown;
            effects.RootsCooldown.m_startMessageType = MessageHud.MessageType.Center;
            effects.RootsCooldown.m_startMessage = "";
            effects.RootsCooldown.m_stopMessageType = MessageHud.MessageType.Center;
            effects.RootsCooldown.m_stopMessage = "";
            effects.RootsCooldown.m_tooltip = "Be patient!";
            effects.RootsCooldown.m_ttl = PluginConfig.secondaryAttackRoots.cooldown.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effects.RootsCooldown, fixReference: false));

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

            prefabs.StaffEarth0 = assetBundle.LoadAsset<GameObject>("MMES_TheForestFlinger");
            prefabs.StaffEarth1 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth1");
            prefabs.StaffEarth2 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth2");
            prefabs.StaffEarth3 = assetBundle.LoadAsset<GameObject>("MMES_StaffEarth3");
            prefabs.ProjectileDefault = assetBundle.LoadAsset<GameObject>("projectile_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileDefault, true));

            prefabs.ProjectileMushroom = assetBundle.LoadAsset<GameObject>("projectile_mushroom_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileMushroom, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_mushroom_projectile_hit_MMES"), true));

            prefabs.SecondaryAttackBoulder = assetBundle.LoadAsset<GameObject>("secondary_boulder_MMES");
            prefabs.ProjectileBoulder = assetBundle.LoadAsset<GameObject>("projectile_boulder_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.ProjectileBoulder, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("projectile_spawn_boulder_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("script_boulder_MMES"), true));

            prefabs.SecondaryAttackRoots = assetBundle.LoadAsset<GameObject>("secondary_root_MMES");
            prefabs.Root = assetBundle.LoadAsset<GameObject>("Root_MMES");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefabs.Root, true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("root_attack_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("script_roots_MMES"), true));

            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_sledge_demolisher_hit_small_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_sledge_demolisher_hit_large_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spores_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_spikes_MMES"), true));
            PrefabManager.Instance.AddPrefab(new CustomPrefab(assetBundle.LoadAsset<GameObject>("fx_staff_earth_windup_MMES"), true));

            sprites.BoulderCooldown = assetBundle.LoadAsset<Sprite>("StaffEarth2Sprite_MMES");
            sprites.RootsCooldown = assetBundle.LoadAsset<Sprite>("StaffEarth3Sprite_MMES");
        }
    }
}

