using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Models
{
    /// <summary>
    /// The "rules" settings that are scoped per-profile (as opposed to General/Debug/Performance,
    /// which stay global BepInEx config) - persisted in profiles/&lt;name&gt;/profile.yaml's
    /// "settings:" key (see ModData.settings) and edited through the Rules tab. Defaults
    /// mirror what these settings used to default to as BepInEx
    /// ConfigEntry&lt;T&gt; values in PluginConfig.cs.
    /// </summary>
    internal class ProfileSettingsData
    {
        // Chatting
        public bool chattingEnabled = true;
        public string chattingBlackList = "Nightbot, StreamElements";
        public int chattingClaimDuration = 300;
        public float chattingCullingRange = 50f;
        public float chattingInterval = 30f;
        public float chattingRadius = 60f;
        public int chattingMaxTalkers = 5;
        public bool chattingClaimFreeForAll = false;

        // Creatures
        public bool creaturesSameFaction = false;
        public float creaturesMaxAmount = 100f;
        public float creaturesMaxRadius = 100f;
        public bool creaturesScaling = true;
        public float creaturesDamageScale = 0.15f;
        public float creaturesHealthScale = 0.15f;
        public float creaturesFollowRadius = 30f;

        // Indestructible
        public bool indestructibleBoats = false;
        public bool indestructibleChests = false;
        public bool indestructiblePortals = false;
        public bool indestructibleVegetables = false;

        // Redeems
        public bool enableRedeemsOnLogin = true;
        public bool autoResolveRedeems = true;
        public string redeemTitlePrefix = "WBTI";
        public int redeemsDiscountPercent = 0;

        // Rejections: auto reject (cancel + refund) redeems that arrive while the streamer is in
        // one of these states.
        public bool rejectWhenPaused = true;
        public bool rejectWhenSleeping = true;
        public bool rejectWhenTeleporting = true;

        // Twitch Ward
        public bool wardEnabled = true;
        public string wardRecipe = "FineWood:5, GreydwarfEye:5, SurtlingCore:1";
        public bool wardBurnCreatures = false;
        public bool wardPushCreatures = true;
        public float wardPushForce = 2000f;
        public int wardMaxCount = 0; // 0 = unlimited

        // Safezones
        public bool safezoneTraders = true;
        public bool safezoneBoats = false;

        // HUD
        public HudPosition hudPosition = HudPosition.UnderMinimap;
        public float hudOffsetX;
        public float hudOffsetY;
        public bool showDeathCounter = true;
        public bool showTopDeathCausers = false;
        public bool showTotalDeathCount = true; // false = only the current world's deaths
        public bool showSafeZoneMessage = true;

        /// <summary>
        /// Copies the personal preferences - the settings a read-only (synced or locked built-in)
        /// profile still lets its user change: everything under Chatting and HUD, plus enable
        /// redeems on login, auto resolve and the redeem discount. Everything else is the
        /// profile's fixed content. Keep in sync with the Settings/Home tab sections that stay
        /// editable on a read-only profile.
        /// </summary>
        public void CopyPersonalSettingsFrom(ProfileSettingsData source)
        {
            chattingEnabled = source.chattingEnabled;
            chattingBlackList = source.chattingBlackList;
            chattingClaimDuration = source.chattingClaimDuration;
            chattingCullingRange = source.chattingCullingRange;
            chattingInterval = source.chattingInterval;
            chattingRadius = source.chattingRadius;
            chattingMaxTalkers = source.chattingMaxTalkers;
            chattingClaimFreeForAll = source.chattingClaimFreeForAll;

            enableRedeemsOnLogin = source.enableRedeemsOnLogin;
            autoResolveRedeems = source.autoResolveRedeems;
            redeemsDiscountPercent = source.redeemsDiscountPercent;

            hudPosition = source.hudPosition;
            hudOffsetX = source.hudOffsetX;
            hudOffsetY = source.hudOffsetY;
            showDeathCounter = source.showDeathCounter;
            showTopDeathCausers = source.showTopDeathCausers;
            showTotalDeathCount = source.showTotalDeathCount;
            showSafeZoneMessage = source.showSafeZoneMessage;
        }
    }
}
