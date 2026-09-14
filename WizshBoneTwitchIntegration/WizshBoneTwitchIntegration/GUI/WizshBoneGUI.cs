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
    /// </summary>
    internal class WizshBoneGUI : MonoBehaviour
    {
        private readonly WizshBoneShellGUI m_shell = new WizshBoneShellGUI();

        private TwitchAuth m_auth;
        private TwitchCustomRewards m_customRewards;

        public bool IsVisible => m_shell.IsVisible;

        private void Awake()
        {
            m_auth = GetComponent<TwitchAuth>();
            m_customRewards = GetComponent<TwitchCustomRewards>();
        }

        private void Update()
        {
            if (m_shell.IsVisible)
                m_shell.RefreshTopBar();
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
    }
}
