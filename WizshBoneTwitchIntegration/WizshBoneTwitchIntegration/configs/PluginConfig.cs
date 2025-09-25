using BepInEx.Configuration;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Configs
{
    internal static class PluginConfig
    {
        public static string generalSectionname = "General";
        public static ConfigEntry<KeyboardShortcut> configLoginButton;
        public static ConfigEntry<float> configMiniMeDuration;
        public static ConfigEntry<float> configBurningDamage;
        public static ConfigEntry<float> configFreezingDamage;
        public static ConfigEntry<float> configPoisonDamage;

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            configLoginButton = WizshBoneTwitchIntegration.Instance.Config.Bind(generalSectionname, "Open Twitch Login window", new KeyboardShortcut(KeyCode.F3),
                new ConfigDescription("Settings of the WizshBone Twitch Integration", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 1000 }));

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
