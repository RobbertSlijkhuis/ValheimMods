using Jotunn.Managers;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Attach point for the new UI shell (round 2 of the UI redesign) - added to
    /// <c>Game.instance.gameObject</c> alongside TwitchAuth/TwitchCustomRewards/etc. (see
    /// harmony/LoginPatchesWBTI.cs), toggled by F3 (see WizshBoneTwitchIntegration.cs).
    ///
    /// Deliberately does not add a callback field on <see cref="TwitchAuth"/> (unlike GUI_OLD's
    /// TwitchAuth.m_settingsGUI) - looks up its own dependencies via GetComponent and polls auth
    /// state each visible frame instead, so components/TwitchAuth.cs stays untouched by this round.
    ///
    /// For the same "GUI pulls from TwitchAuth rather than TwitchAuth pushing into GUI" reason,
    /// this also owns the always-on corner status HUD (<see cref="WizshBoneHUD"/>) and the
    /// unresolved-redeems exit-confirm dialog (<see cref="ConfirmDialog"/>) - both moved here from
    /// TwitchAuth, which only ever held them as convenient attachment points, not because either
    /// is TwitchAuth-specific logic (unlike <see cref="LogoutProgressHUD"/>, which TwitchAuth's
    /// own logout/quit sequence drives directly and so still lives there).
    /// </summary>
    internal class WizshBoneGUI : MonoBehaviour
    {
        private readonly WizshBoneShellGUI m_shell = new WizshBoneShellGUI();
        private readonly WizshBoneHUD m_hud = new WizshBoneHUD();
        private readonly ConfirmDialog m_exitConfirmDialog = new ConfirmDialog();
        private readonly SafeZoneHUD m_safeZoneHUD = new SafeZoneHUD();

        private TwitchAuth m_auth;
        private TwitchCustomRewards m_customRewards;

        public bool IsVisible => m_shell.IsVisible;

        private void Awake()
        {
            m_auth = GetComponent<TwitchAuth>();
            m_customRewards = GetComponent<TwitchCustomRewards>();
            GUIManager.OnCustomGUIAvailable += OnGUIAvailable;
        }

        private void OnGUIAvailable()
        {
            try
            {
                m_hud.ShowHUD();
                m_exitConfirmDialog.Init();
                m_safeZoneHUD.Init();
                GUIManager.OnCustomGUIAvailable -= OnGUIAvailable;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] WizshBoneGUI.OnGUIAvailable failed: {e}");
            }
        }

        private void Update()
        {
            // Unlike the shell (only refreshed while its panel is visible), the corner HUD is
            // always on-screen, so it updates every frame regardless of IsVisible.
            m_hud.UpdateHUD();

            if (m_shell.IsVisible)
            {
                m_shell.RefreshTopBar();
                m_shell.RefreshSidebar();
            }
        }

        public void ShowShell()
        {
            try
            {
                m_shell.Show(m_auth, m_customRewards);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] WizshBoneGUI.ShowShell failed: {e}");
            }
        }

        public void CloseShell()
        {
            m_shell.Close();
        }

        /// <summary>
        /// Shows the unresolved-redeems logout/quit confirm dialog - called from
        /// harmony/LoginPatchesWBTI.cs's Menu.OnLogoutYes/OnQuitYes patches, which fire regardless
        /// of whether the shell/HUD has ever needed attention before, so this must be reachable
        /// independent of <see cref="ShowShell"/>/<see cref="IsVisible"/>.
        /// </summary>
        public void ShowExitConfirm(string title, string description, Action onConfirm, Action onCancel = null, string confirmText = "Confirm", string cancelText = "Cancel")
        {
            m_exitConfirmDialog.Show(title, description, onConfirm, onCancel, confirmText, cancelText);
        }

        /// <summary>
        /// Re-applies the configured HUD position/offset to the live corner status HUD - called by
        /// <see cref="WizshBoneTwitchIntegration.Helpers.ProfileSettingsHelper.ApplyToLiveComponents"/>
        /// whenever the HUD settings fields change.
        /// </summary>
        public void RepositionHUD()
        {
            m_hud.RepositionHUD();
        }

        /// <summary>
        /// Shows/hides the "Streamer is in a safe zone!" label - called by
        /// components/TwitchSafeZone.cs as the local player enters/exits a safe zone.
        /// </summary>
        public void ShowSafeZoneHUD() => m_safeZoneHUD.Show();
        public void HideSafeZoneHUD() => m_safeZoneHUD.Hide();

        /// <summary>
        /// Shows the shell and jumps straight to Home's history section - see
        /// <see cref="WizshBoneShellGUI.ShowHistory"/>'s own doc comment for why this needs its own
        /// entry point rather than reusing <see cref="ShowShell"/>.
        /// </summary>
        public void ShowHistory()
        {
            try
            {
                m_shell.ShowHistory(m_auth, m_customRewards);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] WizshBoneGUI.ShowHistory failed: {e}");
            }
        }
    }
}
