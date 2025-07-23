using BepInEx.Configuration;
using PlayAsSkeleton.Components;
using PlayAsSkeleton.Helpers;
using PlayAsSkeleton.Models;
using PlayAsSkeleton.Types;
using UnityEngine;

namespace PlayAsSkeleton.Configs
{
    internal class SkeletonConfig
    {
        // General options
        public static string[] skinOptions = new string[] { SkinType.Normal, SkinType.Poison, SkinType.Burned, SkinType.Dark, SkinType.LoyalBones };

        // The config fields to generate
        public ConfigEntry<bool> enabled;
        public ConfigEntry<string> skin;
        public ConfigEntry<bool> canSwim;
        public ConfigEntry<bool> hideHelmet;
        public ConfigEntry<bool> hideCape;
        public ConfigEntry<bool> hideChest;
        public ConfigEntry<bool> hideUtility;
        public ConfigEntry<bool> hideLegs;

        // Other
        private int entryCount = 100;

        public void GenerateConfig(SkeletonConfigOptions options)
        {
            ConfigFile Config = PlayAsSkeleton.Instance.Config;

            enabled = Config.Bind(new ConfigDefinition(options.sectionName, "Enabled"), options.enabled,
                new ConfigDescription("Wether the skeleton model is enabled", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            enabled.SettingChanged += (obj, attr) =>
            {
                Player player = Player.GetPlayer(PlayAsSkeleton.playerID);
                SkeletonPAS comp = player.gameObject.GetComponent<SkeletonPAS>();
                comp.SetIsSkeleton(enabled.Value);
            };

            skin = Config.Bind(new ConfigDefinition(options.sectionName, "Skin"), options.skin,
                new ConfigDescription("The skin of the skeleton",
                new AcceptableValueList<string>(skinOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            skin.SettingChanged += (obj, attr) =>
            {
                Player player = Player.GetPlayer(PlayAsSkeleton.playerID);
                SkeletonPAS comp = player.gameObject.GetComponent<SkeletonPAS>();
                comp.SetSkin(skin.Value);
            };

            canSwim = Config.Bind(new ConfigDefinition(options.sectionName, "Can swim"), options.canSwim,
                new ConfigDescription("Wether the skeleton model can swim or walk on the floor (like skeletons do)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            canSwim.SettingChanged += (obj, attr) =>
            {
                Player player = Player.GetPlayer(PlayAsSkeleton.playerID);
                SkeletonHelper.SetCanSwim(player, canSwim.Value);
            };

            //hideHelmet = Config.Bind(new ConfigDefinition(options.sectionName, "Hide Helmet"), options.hideHelmet,
            //    new ConfigDescription("Hide the helmet visual", null,
            //    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            //hideHelmet.SettingChanged += (obj, attr) =>
            //{

            //};

            //hideCape = Config.Bind(new ConfigDefinition(options.sectionName, "Hide Cape"), options.hideCape,
            //    new ConfigDescription("Hide the cape visual", null,
            //    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            //hideCape.SettingChanged += (obj, attr) =>
            //{

            //};

            //hideChest = Config.Bind(new ConfigDefinition(options.sectionName, "Hide Chest"), options.hideChest,
            //    new ConfigDescription("Hide the chest visual", null,
            //    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            //hideChest.SettingChanged += (obj, attr) =>
            //{

            //};

            //hideUtility = Config.Bind(new ConfigDefinition(options.sectionName, "Hide Utility"), options.hideUtility,
            //    new ConfigDescription("Hide the utility visual", null,
            //    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            //hideUtility.SettingChanged += (obj, attr) =>
            //{

            //};

            //hideLegs = Config.Bind(new ConfigDefinition(options.sectionName, "Hide Legs"), options.hideLegs,
            //    new ConfigDescription("Hide the chest visual", null,
            //    new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            //hideLegs.SettingChanged += (obj, attr) =>
            //{

            //};
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
