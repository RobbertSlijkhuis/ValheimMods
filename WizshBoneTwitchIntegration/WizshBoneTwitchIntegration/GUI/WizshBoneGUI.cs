using Jotunn.Managers;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Attach point for the new UI shell (round 2 of the UI redesign) - added to
    /// <c>Game.instance.gameObject</c> alongside TwitchAuth/TwitchCustomRewards/etc. (see
    /// harmony/LoginPatchesWBTI.cs), toggled by F3 (see WizshBoneTwitchIntegration.cs).
    ///
    /// Deliberately does not add a callback field on <see cref="TwitchAuth"/> - looks up its own
    /// dependencies via GetComponent and polls auth state each visible frame instead.
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
        private readonly StatusMessageHUD m_statusMessageHUD = new StatusMessageHUD();
        private readonly DeathCounterHUD m_deathCounterHUD = new DeathCounterHUD();

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
                m_statusMessageHUD.Init();
                m_deathCounterHUD.Init();
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
            m_statusMessageHUD.SyncVisibility();
            m_deathCounterHUD.Refresh();

            InputBlockGate.Verify();

            if (m_shell.IsVisible)
            {
                m_shell.RefreshTopBar();
                m_shell.RefreshSidebar();
                m_shell.RefreshLiveTab();
            }
        }

        public void ShowShell()
        {
            try
            {
                m_shell.Show(m_auth, m_customRewards);
                m_shell.ShowNewsIfUnseen();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] WizshBoneGUI.ShowShell failed: {e}");
            }
        }

        public void CloseShell()
        {
            // F3 (the only caller) is ignored while any dialog is up, so the shell never closes
            // underneath it (e.g. the News dialog's own Close button records the news as seen).
            if (m_shell.IsModalOpen)
                return;

            m_shell.Close();
        }

        private void OnDestroy()
        {
            // A scene change (logout, disconnect, kick) destroys this component while the shell or a
            // dialog may still hold InputBlockGate pushes - release them so the static count doesn't
            // carry over into the next session (see InputBlockGate.Reset).
            GUIManager.OnCustomGUIAvailable -= OnGUIAvailable;
            InputBlockGate.Reset();

            // The next session starts with the status panel hidden again until its player spawns.
            HudHelper.ResetPlayerSpawned();
        }

        /// <summary>
        /// Shows the unresolved-redeems logout/quit confirm dialog - called from
        /// harmony/LoginPatchesWBTI.cs's Menu.OnLogoutYes/OnQuitYes patches, which fire regardless
        /// of whether the shell/HUD has ever needed attention before, so this must be reachable
        /// independent of <see cref="ShowShell"/>/<see cref="IsVisible"/>.
        /// </summary>
        public bool ShowExitConfirm(string title, string description, Action onConfirm, Action onCancel = null, string confirmText = "Confirm", string cancelText = "Cancel")
        {
            return m_exitConfirmDialog.Show(title, description, onConfirm, onCancel, confirmText, cancelText);
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
        /// Shows/hides a persistent on-screen status message (the safe zone one, the redeems-over-limit one, ...).
        /// Callers go through <see cref="StatusMessages"/>, which holds each message's text, color and priority.
        /// </summary>
        public void ShowStatusMessage(string id, string text, Color color, int priority, Func<bool> visibleWhen = null)
            => m_statusMessageHUD.Show(id, text, color, priority, visibleWhen);

        public void HideStatusMessage(string id) => m_statusMessageHUD.Hide(id);

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
