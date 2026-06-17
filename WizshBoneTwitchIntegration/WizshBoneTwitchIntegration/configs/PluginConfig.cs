using BepInEx.Configuration;
using Jotunn.Managers;
using UnityEngine;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Configs
{
    internal static class PluginConfig
    {
        public static string sectionChatting = "Chatting";
        public static string sectionCreatures = "Creatures";
        public static string sectionIndestructible = "Indestructible";
        public static string sectionGeneral = "General";
        public static string sectionRedeems = "Redeems";
        public static string sectionWard = "Twitchy Ward";
        public static string sectionHUD = "HUD";

        public static ConfigEntry<KeyboardShortcut> configWizshBoneWindow;

        public static ConfigEntry<string> configChattingBlackList;
        public static ConfigEntry<bool> configChattingEnabled;
        public static ConfigEntry<int> configChattingClaimDuration;
        public static ConfigEntry<float> configChattingCullingRange;
        public static ConfigEntry<float> configChattingInterval;
        public static ConfigEntry<float> configChattingRadius;      

        public static ConfigEntry<bool> configCreaturesSameFaction;
        public static ConfigEntry<float> configCreaturesMaxAmount;
        public static ConfigEntry<float> configCreaturesMaxRadius;
        public static ConfigEntry<bool> configCreaturesScaling;
        public static ConfigEntry<float> configCreaturesDeltaScale;
        public static ConfigEntry<float> configCreaturesdamageScale;
        public static ConfigEntry<float> configCreaturesHealthScale;
        public static ConfigEntry<float> configCreaturesFollowRadius;

        public static ConfigEntry<bool> configIndestructibleBoats;
        public static ConfigEntry<bool> configIndestructibleChests;
        public static ConfigEntry<bool> configIndestructiblePortals;
        public static ConfigEntry<bool> configIndestructibleVegetables;

        public static ConfigEntry<bool> configEnableRedeemsOnLogin;
        public static ConfigEntry<bool> configAutoResolveRedeems;
        public static ConfigEntry<bool> configAllowRedeemsOnBoats;

        public static ConfigEntry<float> configRaiseRadius;
        public static ConfigEntry<float> configRaisePower;
        public static ConfigEntry<float> configRaiseDelta;

        public static ConfigEntry<string> configWardRecipe;
        public static ConfigEntry<bool> configWardBurnCreatures;
        public static ConfigEntry<bool> configWardPushCreatures;
        public static ConfigEntry<float> configWardPushForce;

        // HUD
        public static ConfigEntry<HudPosition> configHudPosition;
        public static ConfigEntry<float> configHudOffsetX;
        public static ConfigEntry<float> configHudOffsetY;

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


            configChattingBlackList = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting black list", "Nightbot, StreamElements",
                new ConfigDescription("Blacklist for the in-game chatting feature to prevent bots or viewers from being chosen", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingBlackList.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.DeserializeUserBlackList(configChattingBlackList.Value);
            };

            configChattingClaimDuration = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting claim duration", 300,
                new ConfigDescription("The duration of the creature claim (0 means permanent)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configChattingCullingRange = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting culling range", 50f,
                new ConfigDescription("The range where creature messages are still visible", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configChattingEnabled = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Enable in-game chatting feature", true,
                new ConfigDescription("Wether viewer chat messages are shown above creatures in-game", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingEnabled.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.m_enabled = configChattingEnabled.Value;
            };

            configChattingInterval = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting scan interval", 60f,
                new ConfigDescription("The interval that the chatting system will scan for creatures", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingInterval.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.m_scanInterval = configChattingInterval.Value;
                chatting.InvokeRepeatingScan();
            };

            configChattingRadius = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionChatting, "Chatting scan radius", 30f,
                new ConfigDescription("The radius that the chatting system will scan for creatures", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configChattingRadius.SettingChanged += (obj, attr) =>
            {
                TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                chatting.m_scanRadius = configChattingRadius.Value;
            };


            configCreaturesSameFaction = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionCreatures, "Creatures same faction", true,
                new ConfigDescription("Wether spawned creatures will only target players", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configCreaturesMaxAmount = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionCreatures, "Max creatures amount", 100f,
                new ConfigDescription("The maximum amount of creatures allowed in an area", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configCreaturesMaxRadius = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionCreatures, "Max creatures radius", 100f,
                new ConfigDescription("The radius to check for maximum amount of creatures allowed in an area", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configCreaturesScaling = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionCreatures, "Creature scaling", true,
                new ConfigDescription("Wether spawned creatures will scale to player progression", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configCreaturesdamageScale = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionCreatures, "Creature damage scaling per biome tier", 0.15f,
                new ConfigDescription("Percentage of damage scaled up/down per tier difference (e.g. 0.15 = 15% per tier). Positive delta scales up, negative scales down.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configCreaturesHealthScale = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionCreatures, "Creature health scaling per biome tier", 0.15f,
                new ConfigDescription("Percentage of health scaled up/down per tier difference (e.g. 0.15 = 15% per tier). Positive delta scales up, negative scales down.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configCreaturesFollowRadius = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionCreatures, "Friendly follow radius", 30f,
                new ConfigDescription("The radius in which friendlies will toggle their follow state", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));


            configIndestructibleBoats = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionIndestructible, "Indestructible boats", false,
                new ConfigDescription("Wether all boats are completely indestructible", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configIndestructibleChests = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionIndestructible, "Indestructible chests", false,
                new ConfigDescription("Wether all chests are completely indestructible", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configIndestructiblePortals = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionIndestructible, "Indestructible portals", false,
                new ConfigDescription("Wether all portals are completely indestructible", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configIndestructibleVegetables = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionIndestructible, "Indestructible vegetables", false,
                new ConfigDescription("Wether all vegetables are completely indestructible", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));


            configEnableRedeemsOnLogin = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionRedeems, "Enable redeems on login", true,
                new ConfigDescription("Wether the redeems are automaticly enabled when logged in", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configAutoResolveRedeems = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionRedeems, "Auto resolve redeems", true,
                new ConfigDescription("Wether the redeems are automaticly resolved", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configAllowRedeemsOnBoats = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionRedeems, "Allow redeems on boats", false,
                new ConfigDescription("Wether the redeems are allowed on boats or should act like wards", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configAllowRedeemsOnBoats.SettingChanged += (obj, attr) =>
            {
                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
                customRewards.m_playerIsInSafeZone = false;
            };


            configWardRecipe = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionWard, "Twitchy Ward recipe", "FineWood:5, GreydwarfEye:5, SurtlingCore:1",
                new ConfigDescription("The recipe to build the Twitchy Ward", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configWardRecipe.SettingChanged += (obj, attr) =>
            {
                GameObject guardStone = PrefabManager.Instance.GetPrefab("WBTI_guard_stone");
                Piece piece = guardStone.GetComponent<Piece>();
                piece.m_resources = RecipeHelper.GetAsPieceRequirementArray(configWardRecipe.Value, null, null);
            };

            configWardBurnCreatures = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionWard, "Ward burn spawned creatures", false,
                new ConfigDescription("Wether the Twitchy Ward will burn creatures (only spawned by the mod)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));

            configWardPushCreatures = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionWard, "Ward push out spawned creatures", true,
                new ConfigDescription("Wether the Twitchy Ward will push out creatures (only spawned by the mod)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configWardPushForce = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionWard, "Ward push force", 2000f,
                new ConfigDescription("How much force the Twitchy Ward will push out creatures (only spawned by the mod)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));


            configHudPosition = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionHUD, "HUD position", HudPosition.BottomRight,
                new ConfigDescription("The predefined position of the status HUD panel. Set to Custom to use the X and Y offset below.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configHudPosition.SettingChanged += (obj, attr) =>
            {
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                auth.wizshBoneHUD.RepositionHUD();
            };

            configHudOffsetX = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionHUD, "HUD custom offset X", 0f,
                new ConfigDescription("Horizontal offset from the screen center when HUD position is set to Custom.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configHudOffsetX.SettingChanged += (obj, attr) =>
            {
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                auth.wizshBoneHUD.RepositionHUD();
            };

            configHudOffsetY = WizshBoneTwitchIntegration.Instance.Config.Bind(sectionHUD, "HUD custom offset Y", 0f,
                new ConfigDescription("Vertical offset from the screen center when HUD position is set to Custom.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = HandleOrder() }));
            configHudOffsetY.SettingChanged += (obj, attr) =>
            {
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                auth.wizshBoneHUD.RepositionHUD();
            };
        }

        private static int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
