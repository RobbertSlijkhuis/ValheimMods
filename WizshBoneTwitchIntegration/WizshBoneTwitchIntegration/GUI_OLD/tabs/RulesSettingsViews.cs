using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Narrowed BoundField&lt;T&gt; views over <see cref="ProfileSettingsHelper.Current"/>, one per
    /// PluginConfig.cs section that moved to per-profile storage - rendered by RulesTab through
    /// ObjectEditor, same as <c>models/views/RainView.cs</c> does for redeem data. Every setter
    /// re-applies live side effects and persists to the active profile's profile.yaml immediately,
    /// so there's no separate Save button (matches how ConfigEntry.Value used to autosave).
    /// </summary>
    internal static class RulesSettingsViewHelper
    {
        public static void Persist()
        {
            ProfileSettingsHelper.ApplyToLiveComponents();
            ProfileSettingsHelper.Save();
        }
    }

    internal sealed class ChattingSettingsView
    {
        [EditorLabel("Enable in-game chatting feature")]
        [EditorTooltip("Wether viewer chat messages are shown above creatures in-game")]
        public BoundField<bool> chattingEnabled = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.chattingEnabled,
            v => { ProfileSettingsHelper.Current.chattingEnabled = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Chatting black list")]
        [EditorTooltip("Blacklist for the in-game chatting feature to prevent bots or viewers from being chosen")]
        public BoundField<string> chattingBlackList = new BoundField<string>(
            () => ProfileSettingsHelper.Current.chattingBlackList,
            v => { ProfileSettingsHelper.Current.chattingBlackList = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Chatting claim duration")]
        [EditorTooltip("The duration of the creature claim (0 means permanent)")]
        public BoundField<int> chattingClaimDuration = new BoundField<int>(
            () => ProfileSettingsHelper.Current.chattingClaimDuration,
            v => { ProfileSettingsHelper.Current.chattingClaimDuration = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Chatting culling range")]
        [EditorTooltip("The range where creature messages are still visible")]
        public BoundField<float> chattingCullingRange = new BoundField<float>(
            () => ProfileSettingsHelper.Current.chattingCullingRange,
            v => { ProfileSettingsHelper.Current.chattingCullingRange = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Chatting scan interval")]
        [EditorTooltip("The interval that the chatting system will scan for creatures")]
        public BoundField<float> chattingInterval = new BoundField<float>(
            () => ProfileSettingsHelper.Current.chattingInterval,
            v => { ProfileSettingsHelper.Current.chattingInterval = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Chatting scan radius")]
        [EditorTooltip("The radius that the chatting system will scan for creatures")]
        public BoundField<float> chattingRadius = new BoundField<float>(
            () => ProfileSettingsHelper.Current.chattingRadius,
            v => { ProfileSettingsHelper.Current.chattingRadius = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Chatting max talkers")]
        [EditorTooltip("Maximum amount of claimed creatures that can talk (show a speech bubble) at the same time, limited to the ones nearest to the player. Set to 0 to disable the limit.")]
        public BoundField<int> chattingMaxTalkers = new BoundField<int>(
            () => ProfileSettingsHelper.Current.chattingMaxTalkers,
            v => { ProfileSettingsHelper.Current.chattingMaxTalkers = v; RulesSettingsViewHelper.Persist(); });
    }

    internal sealed class CreaturesSettingsView
    {
        [EditorLabel("Creatures same faction")]
        [EditorTooltip("Wether spawned creatures will only target players")]
        public BoundField<bool> creaturesSameFaction = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.creaturesSameFaction,
            v => { ProfileSettingsHelper.Current.creaturesSameFaction = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Max creatures amount")]
        [EditorTooltip("The maximum amount of creatures allowed in an area")]
        public BoundField<float> creaturesMaxAmount = new BoundField<float>(
            () => ProfileSettingsHelper.Current.creaturesMaxAmount,
            v => { ProfileSettingsHelper.Current.creaturesMaxAmount = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Max creatures radius")]
        [EditorTooltip("The radius to check for maximum amount of creatures allowed in an area")]
        public BoundField<float> creaturesMaxRadius = new BoundField<float>(
            () => ProfileSettingsHelper.Current.creaturesMaxRadius,
            v => { ProfileSettingsHelper.Current.creaturesMaxRadius = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Creature scaling")]
        [EditorTooltip("Wether spawned creatures will scale to player progression")]
        public BoundField<bool> creaturesScaling = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.creaturesScaling,
            v => { ProfileSettingsHelper.Current.creaturesScaling = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Creature damage scaling per biome tier")]
        [EditorTooltip("Percentage of damage scaled up/down per tier difference (e.g. 0.15 = 15% per tier). Positive delta scales up, negative scales down.")]
        public BoundField<float> creaturesDamageScale = new BoundField<float>(
            () => ProfileSettingsHelper.Current.creaturesDamageScale,
            v => { ProfileSettingsHelper.Current.creaturesDamageScale = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Creature health scaling per biome tier")]
        [EditorTooltip("Percentage of health scaled up/down per tier difference (e.g. 0.15 = 15% per tier). Positive delta scales up, negative scales down.")]
        public BoundField<float> creaturesHealthScale = new BoundField<float>(
            () => ProfileSettingsHelper.Current.creaturesHealthScale,
            v => { ProfileSettingsHelper.Current.creaturesHealthScale = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Friendly follow radius")]
        [EditorTooltip("The radius in which friendlies will toggle their follow state")]
        public BoundField<float> creaturesFollowRadius = new BoundField<float>(
            () => ProfileSettingsHelper.Current.creaturesFollowRadius,
            v => { ProfileSettingsHelper.Current.creaturesFollowRadius = v; RulesSettingsViewHelper.Persist(); });
    }

    internal sealed class IndestructibleSettingsView
    {
        [EditorLabel("Indestructible boats")]
        [EditorTooltip("Wether all boats are completely indestructible")]
        public BoundField<bool> indestructibleBoats = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.indestructibleBoats,
            v => { ProfileSettingsHelper.Current.indestructibleBoats = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Indestructible chests")]
        [EditorTooltip("Wether all chests are completely indestructible")]
        public BoundField<bool> indestructibleChests = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.indestructibleChests,
            v => { ProfileSettingsHelper.Current.indestructibleChests = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Indestructible portals")]
        [EditorTooltip("Wether all portals are completely indestructible")]
        public BoundField<bool> indestructiblePortals = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.indestructiblePortals,
            v => { ProfileSettingsHelper.Current.indestructiblePortals = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Indestructible vegetables")]
        [EditorTooltip("Wether all vegetables are completely indestructible")]
        public BoundField<bool> indestructibleVegetables = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.indestructibleVegetables,
            v => { ProfileSettingsHelper.Current.indestructibleVegetables = v; RulesSettingsViewHelper.Persist(); });
    }

    internal sealed class RedeemsSettingsView
    {
        [EditorLabel("Enable redeems on login")]
        [EditorTooltip("Wether the redeems are automaticly enabled when logged in")]
        public BoundField<bool> enableRedeemsOnLogin = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.enableRedeemsOnLogin,
            v => { ProfileSettingsHelper.Current.enableRedeemsOnLogin = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Auto resolve redeems")]
        [EditorTooltip("Wether the redeems are automaticly resolved")]
        public BoundField<bool> autoResolveRedeems = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.autoResolveRedeems,
            v => { ProfileSettingsHelper.Current.autoResolveRedeems = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Allow redeems on boats")]
        [EditorTooltip("Wether the redeems are allowed on boats or should act like wards")]
        // Underlying field is now ProfileSettingsData.safezoneBoats (renamed, inverted meaning -
        // true means the safezone is active). Inverted here too so this row keeps showing/storing
        // exactly what it always did ("allowed" = true = no safezone).
        public BoundField<bool> allowRedeemsOnBoats = new BoundField<bool>(
            () => !ProfileSettingsHelper.Current.safezoneBoats,
            v => { ProfileSettingsHelper.Current.safezoneBoats = !v; RulesSettingsViewHelper.Persist(); });
    }

    internal sealed class WardSettingsView
    {
        [EditorLabel("Twitchy Ward recipe")]
        [EditorTooltip("The recipe to build the Twitchy Ward")]
        public BoundField<string> wardRecipe = new BoundField<string>(
            () => ProfileSettingsHelper.Current.wardRecipe,
            v => { ProfileSettingsHelper.Current.wardRecipe = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Ward burn spawned creatures")]
        [EditorTooltip("Wether the Twitchy Ward will burn creatures (only spawned by the mod)")]
        public BoundField<bool> wardBurnCreatures = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.wardBurnCreatures,
            v => { ProfileSettingsHelper.Current.wardBurnCreatures = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Ward push out spawned creatures")]
        [EditorTooltip("Wether the Twitchy Ward will push out creatures (only spawned by the mod)")]
        public BoundField<bool> wardPushCreatures = new BoundField<bool>(
            () => ProfileSettingsHelper.Current.wardPushCreatures,
            v => { ProfileSettingsHelper.Current.wardPushCreatures = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("Ward push force")]
        [EditorTooltip("How much force the Twitchy Ward will push out creatures (only spawned by the mod)")]
        public BoundField<float> wardPushForce = new BoundField<float>(
            () => ProfileSettingsHelper.Current.wardPushForce,
            v => { ProfileSettingsHelper.Current.wardPushForce = v; RulesSettingsViewHelper.Persist(); });
    }

    internal sealed class HudSettingsView
    {
        [EditorLabel("HUD position")]
        [EditorTooltip("The predefined position of the status HUD panel. Set to Custom to use the X and Y offset below.")]
        public BoundField<HudPosition> hudPosition = new BoundField<HudPosition>(
            () => ProfileSettingsHelper.Current.hudPosition,
            v => { ProfileSettingsHelper.Current.hudPosition = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("HUD custom offset X")]
        [EditorTooltip("Horizontal offset from the screen center when HUD position is set to Custom.")]
        public BoundField<float> hudOffsetX = new BoundField<float>(
            () => ProfileSettingsHelper.Current.hudOffsetX,
            v => { ProfileSettingsHelper.Current.hudOffsetX = v; RulesSettingsViewHelper.Persist(); });

        [EditorLabel("HUD custom offset Y")]
        [EditorTooltip("Vertical offset from the screen center when HUD position is set to Custom.")]
        public BoundField<float> hudOffsetY = new BoundField<float>(
            () => ProfileSettingsHelper.Current.hudOffsetY,
            v => { ProfileSettingsHelper.Current.hudOffsetY = v; RulesSettingsViewHelper.Persist(); });
    }
}
