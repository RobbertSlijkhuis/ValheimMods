using System.IO;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Per-profile equivalent of <see cref="RedeemHelper"/> for the "rules" settings
    /// (Chatting/Creatures/Indestructible/Redeems/Twitchy Ward/HUD) that used to be global
    /// BepInEx <c>ConfigEntry&lt;T&gt;</c> values in <c>PluginConfig.cs</c>. General/Debug/
    /// Performance stay global and are untouched by this class. Settings live embedded in the
    /// active profile's profile.yaml (<see cref="ModData.settings"/>), alongside redeems and
    /// creatureGroups.
    /// </summary>
    internal static class ProfileSettingsHelper
    {
        public static ProfileSettingsData Current = new ProfileSettingsData();

        /// <summary>
        /// Re-reads the active profile's profile.yaml and re-applies every live side effect
        /// that used to live in PluginConfig.cs's SettingChanged handlers. Called at startup,
        /// on every profile switch/rename/import/sync, and after every Rules tab edit - so all
        /// call sites are null-guarded since the relevant live components may not exist yet
        /// (e.g. at boot, before a world is loaded).
        /// </summary>
        public static bool Reload()
        {
            ModData data = ExtraConfigHelper.ReadRedeemsConfig();

            if (data == null)
            {
                Jotunn.Logger.LogError("Could not find profile redeems configuration for reload!");
                return false;
            }

            if (data.settings != null)
            {
                Current = data.settings;
            }
            else if (File.Exists(ProfileManager.GetActiveSettingsPath()))
            {
                // Pre-merge profile: settings still live in a standalone settings.yaml (this
                // profile's profile.yaml predates settings being embedded in it). Adopt the
                // legacy file's values once, then immediately fold them into profile.yaml via
                // Save() so every reload after this one takes the fast path above. The old
                // settings.yaml is left in place afterward - harmless, just unused.
                Jotunn.Logger.LogWarning($"[WBTI] Migrating legacy settings.yaml for profile '{ProfileManager.ActiveProfile}' into profile.yaml.");
                Current = ExtraConfigHelper.ReadSettingsConfig() ?? new ProfileSettingsData();
                Save();
            }
            else
            {
                Current = new ProfileSettingsData();
            }

            if (!string.IsNullOrEmpty(Current.redeemTitlePrefix) && Current.redeemTitlePrefix.Length > 6)
                Current.redeemTitlePrefix = Current.redeemTitlePrefix.Substring(0, 6);

            ApplyToLiveComponents();
            return true;
        }

        /// <summary>
        /// Persists Current into the active profile's profile.yaml (read-modify-write, same
        /// backup-then-restore-on-failure shape as RedeemManager.Save/CreatureGroupsTab.Save) -
        /// fires on every single Rules tab field edit, so failures must never corrupt the file.
        /// </summary>
        public static void Save()
        {
            string path = ProfileManager.GetActiveRedeemPath();
            string backupPath = path + ".bak";

            try
            {
                if (File.Exists(path))
                    File.Copy(path, backupPath, overwrite: true);

                ModData data = ExtraConfigHelper.ReadRedeemsConfig(path) ?? new ModData();

                ExtraConfigHelper.WriteRedeemsConfig(path, Current, data.creatureGroups, data.redeems);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError($"Failed to save profile settings, restoring backup: {e}");

                if (File.Exists(backupPath))
                    File.Copy(backupPath, path, overwrite: true);
            }
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

            WizshBoneGUI gui = Game.instance?.gameObject.GetComponent<WizshBoneGUI>();
            gui?.RepositionHUD();

            Jotunn.Managers.PrefabManager prefabManager = Jotunn.Managers.PrefabManager.Instance;
            UnityEngine.GameObject guardStone = prefabManager?.GetPrefab("WBTI_guard_stone");
            Piece piece = guardStone?.GetComponent<Piece>();
            if (piece != null)
                piece.m_resources = RecipeHelper.GetAsPieceRequirementArray(Current.wardRecipe, null, null);
        }
    }
}
