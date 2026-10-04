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

        // Twitchy Ward
        public string wardRecipe = "FineWood:5, GreydwarfEye:5, SurtlingCore:1";
        public bool wardBurnCreatures = false;
        public bool wardPushCreatures = true;
        public float wardPushForce = 2000f;

        // Safezones
        public bool safezoneTraders = true;
        public bool safezoneBoats = false;

        // HUD
        public HudPosition hudPosition = HudPosition.UnderMinimap;
        public float hudOffsetX;
        public float hudOffsetY;
    }
}
