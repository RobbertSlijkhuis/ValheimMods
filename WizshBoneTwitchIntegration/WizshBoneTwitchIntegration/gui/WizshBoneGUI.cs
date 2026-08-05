using Jotunn.Managers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Top-level gate for the WizshBone GUI. Owns input-blocking and lazily constructs/forwards to
    /// <see cref="WizshBoneSettingsGUI"/> (Home/Profiles/Redeems/Settings/Creature groups/Viewers tabs)
    /// and <see cref="WizshBoneRedeemHistoryGUI"/>. F3 (see WizshBoneTwitchIntegration.HandleWizshBoneWindowInput)
    /// calls <see cref="ShowGUI"/>/<see cref="CloseGUI"/> directly - there is no separate outer panel
    /// of its own anymore, the login/redeems/chatting content that used to live here is now the
    /// Settings panel's "Home" tab (see HomeTab.cs).
    /// </summary>
    internal class WizshBoneGUI
    {
        private TwitchAuth auth;
        private TwitchCustomRewards customRewards;

        private WizshBoneSettingsGUI m_settingsGUI;
        private WizshBoneRedeemHistoryGUI m_redeemHistoryGUI;

        private bool m_blockingInput = false;
        private bool m_standaloneBlockingInput = false;

        public void ShowGUI()
        {
            if (GUIManager.Instance == null)
            {
                Jotunn.Logger.LogError("GUIManager instance is null");
                return;
            }

            if (!GUIManager.CustomGUIFront)
            {
                Jotunn.Logger.LogError("GUIManager CustomGUI is null");
                return;
            }

            auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (m_settingsGUI == null)
                m_settingsGUI = new WizshBoneSettingsGUI(auth);

            if (m_redeemHistoryGUI == null)
                m_redeemHistoryGUI = new WizshBoneRedeemHistoryGUI(customRewards);

            if (!m_blockingInput)
            {
                m_blockingInput = true;
                InputBlockGate.Push();
            }

            m_settingsGUI.ShowSettings();
        }

        public void CloseGUI()
        {
            m_settingsGUI?.CloseSettings();
            m_redeemHistoryGUI?.CloseGUI();

            if (m_blockingInput)
            {
                m_blockingInput = false;
                InputBlockGate.Pop();
            }
        }

        public void UpdateGUI()
        {
            m_settingsGUI?.RefreshHomeTab();
        }

        public void OpenRedeemHistory()
        {
            m_redeemHistoryGUI.ShowGUI();
        }

        /// <summary>
        /// Opens the redeem history on its own, without the Settings panel.
        /// Closing the history panel fully closes this and unblocks input.
        /// </summary>
        public void OpenRedeemHistoryStandalone()
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
            {
                Jotunn.Logger.LogError("GUIManager CustomGUI is null");
                return;
            }

            auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (m_redeemHistoryGUI == null)
                m_redeemHistoryGUI = new WizshBoneRedeemHistoryGUI(customRewards);

            if (!m_standaloneBlockingInput)
            {
                m_standaloneBlockingInput = true;
                InputBlockGate.Push();
            }

            m_redeemHistoryGUI.ShowGUI(onClose: () =>
            {
                if (!m_standaloneBlockingInput)
                    return;

                m_standaloneBlockingInput = false;
                InputBlockGate.Pop();
            });
        }

        public bool IsAnyGUIVisible =>
            (m_settingsGUI != null && m_settingsGUI.IsVisible) ||
            (m_redeemHistoryGUI != null && m_redeemHistoryGUI.IsVisible);
    }
}
