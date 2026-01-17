using BepInEx.Configuration;
using Jotunn.Managers;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Configs
{
    internal static class PluginConfig
    {
        public static string sectionChatting = "Chatting";
        public static string sectionGeneral = "General";
        public static string sectionRemoveTheCountry = "Remove the Country";
        public static string sectionTrap = "Trap Field";
        public static string sectionWard = "Twitchy Ward";

        public static ConfigEntry<KeyboardShortcut> configWizshBoneWindow;
        public static ConfigEntry<bool> configAutoResolveRedeems;

        public static ConfigEntry<bool> configChattingEnabled;
        public static ConfigEntry<float> configChattingRadius;
        public static ConfigEntry<float> configChattingInterval;
        public static ConfigEntry<string> configChattingBlackList;

        public static ConfigEntry<float> configRaiseRadius;
        public static ConfigEntry<float> configRaisePower;
        public static ConfigEntry<float> configRaiseDelta;

        public static ConfigEntry<float> configTrapArmTimer;

        public static ConfigEntry<string> configWardRecipe;
        public static ConfigEntry<bool> configWardBurnCreatures;

        // Other
        private static int entryCount = 1000;

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            configWizshBoneWindow = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionGeneral, "Open WizshBone window", new KeyboardShortcut(KeyCode.F3),
                new ConfigDescription("Settings of the WizshBone Twitch Integration", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configAutoResolveRedeems = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionGeneral, "Auto resolve redeems", true,
                new ConfigDescription("Wether the redeems are automaticly resolved", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));


            configWardRecipe = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionWard, "Twitchy Ward recipe", "FineWood:5, GreydwarfEye:5, SurtlingCore:1",
                new ConfigDescription("The recipe to build the Twitchy Ward", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configWardRecipe.SettingChanged += (obj, attr) =>
            {
                GameObject guardStone = PrefabManager.Instance.GetPrefab("WBTI_guard_stone");
                Piece piece = guardStone.GetComponent<Piece>();
                piece.m_resources = RecipeHelper.GetAsPieceRequirementArray(configWardRecipe.Value, null, null);
            };

            configWardBurnCreatures = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionWard, "Twitchy Ward burn spawned creatures", true,
                new ConfigDescription("Wether the Twitchy Ward will burn creatures (only spawned by the mod)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));


            configChattingEnabled = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Enable in-game chatting feature", true,
                new ConfigDescription("Wether viewer chat messages are shown above creatures in-game", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingEnabled.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.m_enabled = configChattingEnabled.Value;
            };

            configChattingRadius = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting scan radius", 20f,
                new ConfigDescription("The radius that the chatting system will scan for creatures", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingRadius.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.m_scanRadius = configChattingRadius.Value;
            };

            configChattingInterval = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting scan interval", 60f,
                new ConfigDescription("The interval that the chatting system will scan for creatures", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingInterval.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.m_scanInterval = configChattingInterval.Value;
            };

            configChattingBlackList = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting black list", "Nightbot, StreamElements",
                new ConfigDescription("Blacklist for the in-game chatting feature to prevent bots or viewers from being chosen", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingBlackList.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.DeserializeUserBlackList(configChattingBlackList.Value);
            };


            configRaiseRadius = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionRemoveTheCountry, "Remove The Country Rdeem: raise radius", 8f,
                new ConfigDescription("How big the radius is of the Remove The Country Rdeem", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configRaiseRadius.SettingChanged += (obj, attr) =>
            {
                if (configRaiseRadius.Value == float.NaN)
                {
                    Jotunn.Logger.LogWarning("Incorrect value");
                    return;
                }

                GameObject prefab = WizshBoneTwitchIntegration.Instance.prefabs.RemoveTheCountry;
                TerrainOp terrain = prefab.GetComponent<TerrainOp>();
                terrain.m_settings.m_raiseRadius = configRaiseRadius.Value;
            };

            configRaisePower = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionRemoveTheCountry, "Remove The Country Rdeem: raise power", 0f,
                new ConfigDescription("How big the power is of the Remove The Country Rdeem", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configRaisePower.SettingChanged += (obj, attr) =>
            {
                if (configRaisePower.Value == float.NaN)
                {
                    Jotunn.Logger.LogWarning("Incorrect value");
                    return;
                }

                GameObject prefab = WizshBoneTwitchIntegration.Instance.prefabs.RemoveTheCountry;
                TerrainOp terrain = prefab.GetComponent<TerrainOp>();
                terrain.m_settings.m_raisePower = configRaisePower.Value;
            };

            configRaiseDelta = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionRemoveTheCountry, "Remove The Country Rdeem: raise delta", -8f,
                new ConfigDescription("How big the delta is of the Remove The Country Rdeem", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configRaiseDelta.SettingChanged += (obj, attr) =>
            {
                if (configRaiseDelta.Value == float.NaN)
                {
                    Jotunn.Logger.LogWarning("Incorrect value");
                    return;
                }

                GameObject prefab = WizshBoneTwitchIntegration.Instance.prefabs.RemoveTheCountry;
                TerrainOp terrain = prefab.GetComponent<TerrainOp>();
                terrain.m_settings.m_raiseDelta = configRaiseDelta.Value;
            };

            configTrapArmTimer = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionTrap, "Trap arming timer", 1f,
                new ConfigDescription("How long it takes the traps will be armed after being spawned", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
