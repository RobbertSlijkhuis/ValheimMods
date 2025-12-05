using BepInEx.Configuration;
using UnityEngine;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Configs
{
    internal static class PluginConfig
    {
        public static string sectionGeneral = "General";
        public static string sectionBuffsAndDebuffs = "Buffs and debuffs";
        public static string sectionRemoveTheCountry = "Remove the Country";

        public static ConfigEntry<KeyboardShortcut> configWizshBoneWindow;
        public static ConfigEntry<bool> configChattingEnabled;
        public static ConfigEntry<string> configChattingBlackList;

        public static ConfigEntry<float> configMiniMeDuration;

        public static ConfigEntry<float> configRaiseRadius;
        public static ConfigEntry<float> configRaisePower;
        public static ConfigEntry<float> configRaiseDelta;

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

            configChattingEnabled = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionGeneral, "Enable in-game chatting feature", true,
                new ConfigDescription("Wether viewer chat messages are shown above creatures in-game", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingEnabled.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.m_enabled = configChattingEnabled.Value;
            };

            configChattingBlackList = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionGeneral, "Chatting black list", "Nightbot, StreamElements",
                new ConfigDescription("Blacklist for the in-game chatting feature to prevent bots or viewers from being chosen", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingBlackList.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.DeserializeUserBlackList(configChattingBlackList.Value);
            };

            configMiniMeDuration = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionBuffsAndDebuffs, "MiniMe duration", 180f,
                new ConfigDescription("The duration applied to the MiniMe redeem", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configMiniMeDuration.SettingChanged += (obj, attr) =>
            {
                WizshBoneTwitchIntegration.Instance.effects.MiniMe.m_ttl = configMiniMeDuration.Value;
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
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
