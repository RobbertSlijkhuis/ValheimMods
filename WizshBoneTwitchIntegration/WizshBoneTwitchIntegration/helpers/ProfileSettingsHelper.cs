using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Per-profile equivalent of <see cref="RedeemHelper"/> for the "rules" settings
    /// (Chatting/Creatures/Indestructible/Redeems/Twitchy Ward/HUD) that used to be global
    /// BepInEx <c>ConfigEntry&lt;T&gt;</c> values in <c>PluginConfig.cs</c>. General/Debug/
    /// Performance stay global and are untouched by this class.
    /// </summary>
    internal static class ProfileSettingsHelper
    {
        public static ProfileSettingsData Current = new ProfileSettingsData();

        /// <summary>
        /// Re-reads the active profile's settings.yaml and re-applies every live side effect
        /// that used to live in PluginConfig.cs's SettingChanged handlers. Called at startup,
        /// on every profile switch/rename/import/sync, and after every Rules tab edit - so all
        /// call sites are null-guarded since the relevant live components may not exist yet
        /// (e.g. at boot, before a world is loaded).
        /// </summary>
        public static bool Reload()
        {
            ProfileSettingsData data = ExtraConfigHelper.ReadSettingsConfig();

            if (data == null)
            {
                Jotunn.Logger.LogError("Could not find profile settings configuration for reload!");
                return false;
            }

            Current = data;
            ApplyToLiveComponents();
            return true;
        }

        public static void Save()
        {
            ExtraConfigHelper.WriteSettingsConfig(ProfileManager.GetActiveSettingsPath(), Current);
        }

        /// <summary>
        /// Pushes the current in-memory settings into whichever live components already exist,
        /// mirroring what PluginConfig.cs's SettingChanged lambdas used to do per-field. Safe to
        /// call before a world is loaded (Game.instance null) or before the Twitchy Ward prefab
        /// is registered - each push is skipped if its target isn't present yet.
        /// </summary>
        public static void ApplyToLiveComponents()
        {
            TwitchChatting chatting = Game.instance?.gameObject.GetComponent<TwitchChatting>();
            if (chatting != null)
            {
                chatting.m_enabled = Current.chattingEnabled;
                chatting.DeserializeUserBlackList(Current.chattingBlackList);
                chatting.m_scanRadius = Current.chattingRadius;
                chatting.m_scanInterval = Current.chattingInterval;
                chatting.InvokeRepeatingScan();
            }

            TwitchCustomRewards customRewards = Game.instance?.gameObject.GetComponent<TwitchCustomRewards>();
            if (customRewards != null)
                customRewards.m_playerIsInSafeZone = false;

            TwitchAuth auth = Game.instance?.gameObject.GetComponent<TwitchAuth>();
            if (auth?.wizshBoneHUD != null)
                auth.wizshBoneHUD.RepositionHUD();

            Jotunn.Managers.PrefabManager prefabManager = Jotunn.Managers.PrefabManager.Instance;
            UnityEngine.GameObject guardStone = prefabManager?.GetPrefab("WBTI_guard_stone");
            Piece piece = guardStone?.GetComponent<Piece>();
            if (piece != null)
                piece.m_resources = RecipeHelper.GetAsPieceRequirementArray(Current.wardRecipe, null, null);
        }
    }
}
