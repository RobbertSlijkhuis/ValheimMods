using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace ValheimModding
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class UpgradeAntlerPickaxe : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.upgrade_antler_pickaxe";
        public const string PluginName = "Upgrade Antler Pickaxe";
        public const string PluginVersion = "1.0.0";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private ConfigEntry<bool> configEnable;
        private ConfigEntry<bool> configLoadSpecials;

        private void Awake()
        {
            HandleConfigValues();

            if (configEnable.Value)
            {
                PrefabManager.OnVanillaPrefabsAvailable += PatchAntlerPickaxe;

                if (configLoadSpecials.Value)
                {
                    PrefabManager.OnVanillaPrefabsAvailable += AddKimetsusSpecial;
                    PrefabManager.OnVanillaPrefabsAvailable += AddDirksBoner;
                }
            }
        }

        private void PatchAntlerPickaxe()
        {
            GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");

            if (antlerPickaxeObject != null)
            {
                var antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
                antlerPickaxe.m_itemData.m_shared.m_maxQuality = 4;
                antlerPickaxe.m_itemData.m_shared.m_damagesPerLevel.m_pickaxe = 5;
                antlerPickaxe.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 5;
                antlerPickaxe.m_itemData.m_shared.m_blockPowerPerLevel = 0;
                antlerPickaxe.m_itemData.m_shared.m_deflectionForcePerLevel = 5;
                antlerPickaxe.m_itemData.m_shared.m_durabilityPerLevel = 50;

                PrefabManager.OnVanillaPrefabsAvailable -= PatchAntlerPickaxe;
                Jotunn.Logger.LogInfo("Successfully patched the antler pickaxe, enjoy!");
            }
            else
            {
                Jotunn.Logger.LogInfo("Unable to patch the antler pickaxe :S");
            }
        }

        private void UnpatchAntlerPickaxe()
        {
            GameObject antlerPickaxeObject = PrefabManager.Instance.GetPrefab("PickaxeAntler");

            if (antlerPickaxeObject != null)
            {
                var antlerPickaxe = antlerPickaxeObject.GetComponent<ItemDrop>();
                antlerPickaxe.m_itemData.m_shared.m_maxQuality = 1;
                Jotunn.Logger.LogInfo("Successfully unpatched the antler pickaxe, Why u do this?!");
            }
        }

        private void HandleConfigValues()
        {
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };
            configEnable = base.Config.Bind(new ConfigDefinition("General", "Enable"), true, new ConfigDescription("Wether or not to enable the mod", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
            configLoadSpecials = base.Config.Bind(new ConfigDefinition("General", "Special weapons"), false, new ConfigDescription("Wether or not to enable special weapons I created", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));

            //SynchronizationManager.OnConfigurationSynchronized += (obj, attr) =>
            //{
            //    if (attr.InitialSynchronization)
            //    {
            //        Jotunn.Logger.LogMessage("Initial Config sync event received for Upgrade Antler Pickaxe");
            //    }
            //    else
            //    {
            //        Jotunn.Logger.LogMessage("Config sync event received for Upgrade Antler Pickaxe");
            //        if (configEnable.Value)
            //            PatchAntlerPickaxe();
            //        else
            //            UnpatchAntlerPickaxe();
            //    }
            //};
        }

        private void AddKimetsusSpecial()
        {
            ItemConfig kimetsuHammerConfig = new ItemConfig();
            kimetsuHammerConfig.Name = "Kimetsu's Special";
            kimetsuHammerConfig.Description = "May the bones of you enemies forever be crushed, my friend";
            kimetsuHammerConfig.CraftingStation = CraftingStations.Forge;
            kimetsuHammerConfig.AddRequirement(new RequirementConfig("Silver", 25, 5));
            kimetsuHammerConfig.AddRequirement(new RequirementConfig("ElderBark", 20, 5));
            kimetsuHammerConfig.AddRequirement(new RequirementConfig("WolfFang", 10, 5));
            kimetsuHammerConfig.AddRequirement(new RequirementConfig("TrophyCultist", 1));

            CustomItem kimetsuHammer = new CustomItem("KimetsusSpecial", "SledgeDemolisher", kimetsuHammerConfig);

            // Stats
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_damages.m_damage = 0;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 75;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_damages.m_spirit = 30;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_attack.m_attackStamina = 18;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_blockPower = 40;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_attackForce = 75;
            kimetsuHammer.ItemDrop.m_itemData.m_durability = 200f;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_maxDurability = 200f;

            // Quality
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_maxQuality = 10;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 5;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_spirit = 3;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_blockPowerPerLevel = 5;
            kimetsuHammer.ItemDrop.m_itemData.m_shared.m_durabilityPerLevel = 50;

            ItemManager.Instance.AddItem(kimetsuHammer);
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

            CustomItem DirksBoner = new CustomItem("DirksBoner", "SwordMistwalker", DirkBonerConfig);

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
    }
}
