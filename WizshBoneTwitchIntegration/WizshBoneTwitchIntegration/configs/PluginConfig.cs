using BepInEx.Configuration;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Configs
{
    internal static class PluginConfig
    {
        // Chatting/Creatures/Indestructible/Redeems/Twitchy Ward/HUD moved to per-profile
        // settings (profiles/<name>/profile.yaml's "settings:" key, see
        // ProfileSettingsHelper/ProfileSettingsData).

        public static string sectionKeybinds = "Keybinds";
        public static string sectionDebug = "Debug";
        public static string sectionPerformance = "Performance";

        public static ConfigEntry<KeyboardShortcut> configWizshBoneWindow;
        public static ConfigEntry<KeyboardShortcut> configQuickTestRedeemKey;

        // Debug
        public static ConfigEntry<bool> configShowSafeZoneDebug;

        // Performance
        public static ConfigEntry<int> configShipIdlePhysicsThrottle;

        // Other
        private static int entryCount = 1000;

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            configWizshBoneWindow = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionKeybinds, "Open WizshBone window", new KeyboardShortcut(KeyCode.F3),
                new ConfigDescription("Settings of the WizshBone Twitch Integration", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configQuickTestRedeemKey = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionKeybinds, "Quick test redeem key", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Fires the redeem set via WBTISetQuickRedeem, for quick in-game testing", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));


            configShowSafeZoneDebug = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionDebug, "Show safezone bounds", false,
                new ConfigDescription("Shows the bounds of active safezones (ships, wards, traders) in-game as a wireframe outline", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configShowSafeZoneDebug.SettingChanged += (obj, attr) =>
            {
                TwitchSafeZone.RefreshDebugVisuals(configShowSafeZoneDebug.Value);
            };


            configShipIdlePhysicsThrottle = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionPerformance, "Idle boat physics throttle", 4,
                new ConfigDescription("Redeem-spawned boats with nobody at the helm only run their full buoyancy physics every Nth physics tick instead of every tick (e.g. 4 = ~12.5Hz instead of 50Hz). Reduces the performance cost of large boat piles (e.g. Boatpocalypse). Set to 1 to disable throttling. Boats being actively steered always run at full rate regardless of this setting.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
