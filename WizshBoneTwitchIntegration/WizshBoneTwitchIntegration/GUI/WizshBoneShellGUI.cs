using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
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

        private ShellTab m_activeTab = ShellTab.Home;

        /// <summary>
        /// Cached copy of <see cref="BuildGUI"/>'s local "navigate to Home, then show history"
        /// action, so <see cref="ShowHistory"/> can trigger it from outside the shell (see that
        /// method's own doc comment).
        /// </summary>
        private Action m_openHistory;

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
                m_tabViews[m_activeTab].Refresh();
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

            // Jötunn's woodpanel_trophys sprite is 9-sliced by default, which keeps its rustic
            // carved-frame border art at native pixel size/shape regardless of panel size - that's
            // the source of the uneven, not-quite-straight edge, not a stretch/transparency issue.
            // Simple bypasses the border art entirely for a plain stretched texture with a
            // straight rectangular edge (our own GuiHelper.AddPanelBorder line draws the frame now).
            m_panel.GetComponent<Image>().type = Image.Type.Simple;

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

        /// <summary>
        /// Re-reads the active profile name into the sidebar's "Profile: {name}" group header.
        /// Called by <see cref="WizshBoneGUI"/> every frame while the shell is visible, same as
        /// <see cref="RefreshTopBar"/> - see <see cref="ShellSidebar.RefreshProfileGroupLabel"/>.
        /// </summary>
        public void RefreshSidebar()
        {
            if (m_panel != null)
                m_sidebar.RefreshProfileGroupLabel();
        }

        /// <summary>
        /// Shows the shell (building it if this is the first time it's ever been opened this
        /// session) and jumps straight to Home's history section. Used by
        /// harmony/LoginPatchesWBTI.cs's quit/logout confirm dialogs' "Open History" cancel
        /// option, which can fire before the player has ever pressed F3 - unlike
        /// <see cref="ShellTopBar.OnOpenHistoryRequested"/>/<c>RedeemsTab.OnOpenHistoryRequested</c>,
        /// which only fire while the shell is already open.
        /// </summary>
        public void ShowHistory(TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            Show(auth, customRewards);
            m_openHistory?.Invoke();
        }

        private void BuildGUI(TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            m_sidebar.Create(m_panel, SelectTab, Close);
            m_topBar.Create(m_panel, ShellSidebar.Width, auth, customRewards);
            ToastNotifications.Init(m_panel);

            GameObject content = GuiHelper.CreateRegion(
                m_panel, "Content",
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(ShellSidebar.Width, 0f),
                offsetMax: new Vector2(0f, -ShellTopBar.Height));

            foreach (ShellTab tab in ShellTabExtensions.All)
                m_tabRoots[tab] = m_tabViews[tab].Create(content);

            // HomeTab's "Create a new redeem" button needs to switch tabs without holding a
            // reference to this shell class itself - wired here rather than threaded through
            // IShellTabView.Create, since no other tab needs cross-tab navigation (yet).
            Action openHistory = null;
            if (m_tabViews[ShellTab.Home] is HomeTab homeTab)
            {
                homeTab.OnNavigateToTab = SelectTab;

                // History lives as an inline section owned by HomeTab (see
                // RedeemHistorySection/HomeTab.ShowHistory), not a shell-level overlay - so opening
                // it from a different tab (RedeemsTab's/ShellTopBar's "Open history" confirm-dialog
                // cancel options) means switching to Home first, then telling it to show the
                // section - the same "navigate then act" shape OnNavigateToTab above already uses
                // for "Create a new redeem", just in reverse.
                openHistory = () =>
                {
                    SelectTab(ShellTab.Home);
                    homeTab.ShowHistory();
                };

                m_topBar.OnOpenHistoryRequested = openHistory;
                m_openHistory = openHistory;
            }

            // RedeemsTab's Test action needs to close the whole shell panel first (so the player
            // can actually see the effect play out) without holding a reference to this shell
            // class itself - same "wire a callback after construction" pattern as HomeTab above.
            if (m_tabViews[ShellTab.Redeems] is RedeemsTab redeemsTab)
            {
                redeemsTab.OnCloseRequested = Close;
                redeemsTab.OnOpenHistoryRequested = openHistory;
            }

            GuiHelper.AddPanelBorder(m_panel, inset: 0f, thickness: GuiHelper.PanelBorderThickness, color: GuiHelper.PanelBorderColor);

            SelectTab(ShellTab.Home);
        }

        private void SelectTab(ShellTab tab)
        {
            foreach (KeyValuePair<ShellTab, GameObject> kvp in m_tabRoots)
                kvp.Value.SetActive(kvp.Key == tab);

            m_activeTab = tab;
            m_sidebar.SetActiveTab(tab);
            m_tabViews[tab].Refresh();
        }
    }
}
