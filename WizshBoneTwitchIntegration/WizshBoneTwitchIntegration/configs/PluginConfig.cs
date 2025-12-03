using BepInEx.Configuration;
using UnityEngine;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Configs
{
    internal static class PluginConfig
    {
        public static string generalSectionname = "General";
        public static ConfigEntry<KeyboardShortcut> configWizshBoneWindow;
        public static ConfigEntry<string> configChannelName;
        public static ConfigEntry<bool> configChattingEnabled;
        public static ConfigEntry<float> configMiniMeDuration;

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            configWizshBoneWindow = WizshBoneTwitchIntegration.Instance.Config.Bind(generalSectionname, "Open WizshBone window", new KeyboardShortcut(KeyCode.F3),
                new ConfigDescription("Settings of the WizshBone Twitch Integration", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 1000 }));

            configChattingEnabled = WizshBoneTwitchIntegration.Instance.Config.Bind(generalSectionname, "Enable in-game chatting feature", true,
                new ConfigDescription("Wether viewer chat messages are shown above creatures in-game", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 1000 }));
            configChattingEnabled.SettingChanged += (obj, attr) =>
            {
                TwitchChatting twitchChatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                twitchChatting.isEnabled = configChattingEnabled.Value;
            };

            configMiniMeDuration = WizshBoneTwitchIntegration.Instance.Config.Bind(generalSectionname, "MiniMe duration", 180f,
                new ConfigDescription("The duration applied to the MiniMe redeem", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 1000 }));
            configMiniMeDuration.SettingChanged += (obj, attr) =>
            {
                WizshBoneTwitchIntegration.Instance.effects.MiniMe.m_ttl = configMiniMeDuration.Value;
            };
        }
    }
}
