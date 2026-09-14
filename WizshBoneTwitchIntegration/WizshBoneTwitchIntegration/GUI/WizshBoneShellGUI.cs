using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using WizshBoneTwitchIntegration.Gui.Tabs;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Root of the new UI shell (round 2 of the UI redesign): one wood panel holding a sidebar,
    /// a top bar, and a main content area that swaps per active tab. Each tab's content is built
    /// by its own <see cref="IShellTabView"/> under GUI/tabs/ (currently just a stub label each -
    /// real per-tab content is future rounds' scope).
    ///
    /// Panel lifecycle (lazy-build-once, SetActive to show/hide, InputBlockGate push/pop) and the
    /// "keep all tab roots alive, SetActive to switch" idea are ported from
    /// GUI_OLD/WizshBoneSettingsGUI.cs, restructured to loop over <see cref="ShellTab"/> instead of
    /// one hand-duplicated block per tab.
    /// </summary>
    internal class WizshBoneShellGUI
    {
        internal const float PanelWidth = 1200f;
        internal const float PanelHeight = 720f;

        private GameObject m_panel;
        private bool m_blockingInput;

        private readonly ShellSidebar m_sidebar = new ShellSidebar();
        private readonly ShellTopBar m_topBar = new ShellTopBar();

        private readonly Dictionary<ShellTab, IShellTabView> m_tabViews = new Dictionary<ShellTab, IShellTabView>
        {
            { ShellTab.Home, new HomeTab() },
            { ShellTab.Profiles, new ProfilesTab() },
            { ShellTab.Redeems, new RedeemsTab() },
            { ShellTab.Settings, new SettingsTab() },
            { ShellTab.CreatureGroups, new CreatureGroupsTab() },
            { ShellTab.Viewers, new ViewersTab() },
            { ShellTab.Debug, new DebugTab() },
        };

        private readonly Dictionary<ShellTab, GameObject> m_tabRoots = new Dictionary<ShellTab, GameObject>();

        public bool IsVisible => m_panel != null && m_panel.activeSelf;

        public void Show(TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
                return;

            if (!m_blockingInput)
            {
                m_blockingInput = true;
                InputBlockGate.Push();
            }

            if (m_panel != null)
            {
                m_panel.transform.SetAsLastSibling();
                m_topBar.Refresh();
                m_panel.SetActive(true);
                return;
            }

            m_panel = GUIManager.Instance.CreateWoodpanel(
                parent:    GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position:  new Vector2(0f, 0f),
                width:     PanelWidth,
                height:    PanelHeight,
                draggable: false
            );
            m_panel.transform.SetAsLastSibling();

            BuildGUI(auth, customRewards);
            m_panel.SetActive(true);
        }

        public void Close()
        {
            if (m_panel == null)
                return;

            m_panel.SetActive(false);

            if (m_blockingInput)
            {
                m_blockingInput = false;
                InputBlockGate.Pop();
            }
        }

        /// <summary>
        /// Re-reads Twitch auth state into the top bar. Called by <see cref="WizshBoneGUI"/> every
        /// frame while the shell is visible.
        /// </summary>
        public void RefreshTopBar()
        {
            if (m_panel != null)
                m_topBar.Refresh();
        }

        private void BuildGUI(TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            m_sidebar.Create(m_panel, SelectTab, Close);
            m_topBar.Create(m_panel, ShellSidebar.Width, auth, customRewards);

            GameObject content = GuiHelper.CreateRegion(
                m_panel, "Content",
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(ShellSidebar.Width, 0f),
                offsetMax: new Vector2(0f, -ShellTopBar.Height));

            foreach (ShellTab tab in ShellTabExtensions.All)
                m_tabRoots[tab] = m_tabViews[tab].Create(content);

            SelectTab(ShellTab.Home);
        }

        private void SelectTab(ShellTab tab)
        {
            foreach (KeyValuePair<ShellTab, GameObject> kvp in m_tabRoots)
                kvp.Value.SetActive(kvp.Key == tab);

            m_sidebar.SetActiveTab(tab);
        }
    }
}
