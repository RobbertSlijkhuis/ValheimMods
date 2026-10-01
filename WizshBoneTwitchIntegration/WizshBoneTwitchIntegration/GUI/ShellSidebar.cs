using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Left sidebar: the WizshBone logo image, two labeled nav groups (matching RedesignUI.dc.html) -
    /// "General" (Home/Profiles/Viewers/Help) and a dynamic "Profile: {activeProfileName}"
    /// group (Redeems/Settings/Creature groups) - and a Close button pinned to the bottom.
    /// GUI_OLD/WizshBoneSettingsGUI.cs uses a horizontal tab-button row with no grouping instead
    /// of a vertical sidebar, so this layout is new; the active/inactive button-color idiom is
    /// ported from its SetTabButtonColor.
    ///
    /// <see cref="ShellTabExtensions.All"/> stays the canonical tab set/order for tab-root
    /// construction in <see cref="WizshBoneShellGUI"/> - only this sidebar's *rendering*
    /// grouping/order differs from it.
    /// </summary>
    internal class ShellSidebar
    {
        public const float Width = 220f;

        internal static readonly Color TabActiveColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        private class TabButtonVisual
        {
            public Button Button;
            public Image Background;
            public Text Label;
        }

        private static readonly ShellTab[] GeneralGroupTabs =
        {
            ShellTab.Home, ShellTab.Profiles, ShellTab.Viewers, ShellTab.Help
        };

        // CreatureGroups is deliberately left out for now - the concept is being rethought from
        // scratch, so its sidebar button is hidden until that redesign happens. ShellTab.CreatureGroups,
        // ShellTabExtensions.All, and CreatureGroupsTab itself stay untouched - the tab still
        // builds, it's just unreachable from here in the meantime.
        private static readonly ShellTab[] ProfileGroupTabs =
        {
            ShellTab.Redeems, ShellTab.Settings
        };

        private const float TitleY           = -30f;
        private const float TitleLogoWidth   = 200f;
        private const float TitleLogoHeight  = 50f;

        private const string TitleLogoResource = "WizshBoneTwitchIntegration.resources.WizshBone_Simple_512.png";

        // DividerRightInset pulls the title divider bar in from the sidebar's right edge.
        private const float DividerThickness = 2f;
        private const float DividerRightInset = 2f;

        // Independent of GuiHelper.PanelBorderThickness (the outer wood panel frame's own thickness).
        private const float RightBorderThickness = 2f;
        private const float RightBorderTopInset = 3f;
        private const float RightBorderBottomInset = 5f;

        // Pinned to ShellTopBar.Height (rather than a separate hand-picked Y) so this divider and
        // the top bar's own bottom-edge divider always land in the exact same pixel band instead
        // of drifting out of alignment if either constant changes later.
        private const float TitleDividerY    = -(ShellTopBar.Height - DividerThickness);
        // Chosen so the "General" header lines up with a tab's own content title (e.g. HomeTab's
        // "OVERVIEW") - both sit ShellTopBar.Height (60) + 30px below the panel's top edge, since
        // every content title uses that same TitleY=-30 convention (see GuiHelper.CreateTitle's
        // call sites). GroupHeaderGap below is then back-solved separately so the first group's
        // first button (Home) still lands flush with HomeTab's card row (60 + HomeTab.GridTopY's
        // 57px = 117px down) despite the header itself moving - see CreateGroup's Y math.
        private const float FirstGroupY      = -90f;
        private const float GroupHeaderHeight = 22f;
        private const float GroupHeaderGap    = 27f;
        private const float ButtonHeight      = 44f;
        private const float ButtonSpacing     = 6f;
        private const float GroupGap          = 16f;
        private const float CloseButtonY      = 40f;

        private readonly Dictionary<ShellTab, TabButtonVisual> m_tabButtons = new Dictionary<ShellTab, TabButtonVisual>();
        private Color m_tabDefaultColor;
        private Text m_profileGroupHeaderText;

        /// <summary>
        /// Builds the sidebar region as a child of <paramref name="panel"/>.
        /// </summary>
        /// <param name="onSelectTab">Invoked with the clicked tab.</param>
        /// <param name="onClose">Invoked when the Close button is clicked.</param>
        /// <param name="onNews">Invoked when the News button (pinned above Close) is clicked.</param>
        public GameObject Create(GameObject panel, Action<ShellTab> onSelectTab, Action onClose, Action onNews)
        {
            // ShellSidebar is a readonly field of WizshBoneShellGUI, reused (not reconstructed)
            // if the shell panel is ever torn down and rebuilt - reset every piece of mutable
            // per-build state so a second Create() call is as clean as the first.
            m_tabButtons.Clear();
            m_tabDefaultColor = default;
            m_profileGroupHeaderText = null;

            GameObject root = GuiHelper.CreateRegion(
                panel, "Sidebar",
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(0f, 1f),
                offsetMin: Vector2.zero,
                offsetMax: new Vector2(Width, 0f));
            GuiHelper.AddBackground(root, new Color(0f, 0f, 0f, 0.6f));

            GameObject rightBorder = GuiHelper.CreateRegion(root, "RightBorder",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-RightBorderThickness, RightBorderBottomInset), new Vector2(0f, -RightBorderTopInset));
            GuiHelper.AddBackground(rightBorder, GuiHelper.PanelBorderColor);

            GameObject titleLogo = new GameObject("TitleLogo");
            titleLogo.transform.SetParent(root.transform, false);

            RectTransform titleLogoRt = titleLogo.AddComponent<RectTransform>();
            titleLogoRt.anchorMin = new Vector2(0.5f, 1f);
            titleLogoRt.anchorMax = new Vector2(0.5f, 1f);
            titleLogoRt.pivot = new Vector2(0.5f, 0.5f);
            titleLogoRt.sizeDelta = new Vector2(TitleLogoWidth, TitleLogoHeight);
            titleLogoRt.anchoredPosition = new Vector2(0f, TitleY);

            Image titleLogoImage = titleLogo.AddComponent<Image>();
            titleLogoImage.sprite = GuiHelper.LoadEmbeddedSprite(TitleLogoResource);
            titleLogoImage.preserveAspect = true;

            GameObject titleDivider = GuiHelper.CreateRegion(root, "TitleDivider",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, TitleDividerY - DividerThickness), new Vector2(-DividerRightInset, TitleDividerY));
            GuiHelper.AddBackground(titleDivider, GuiHelper.DividerColorStrong);

            float y = FirstGroupY;

            y = CreateGroup(root, "General", GeneralGroupTabs, y, onSelectTab, isProfileGroup: false);
            y = CreateGroup(root, $"Profile: {ProfileManager.ActiveProfile}", ProfileGroupTabs, y, onSelectTab, isProfileGroup: true);

            // An action button rather than a tab: opens the News dialog, never highlighted as active.
            // Pinned to the bottom, directly above Close.
            CreateStockButton(root, "News", CloseButtonY + ButtonHeight + ButtonSpacing, anchorBottom: true)
                .Button.onClick.AddListener(() => onNews());

            GameObject closeBtnObj = GuiHelper.CreateButton(
                text:      "Close",
                parent:    root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position:  new Vector2(0f, CloseButtonY),
                width:     Width - 30f,
                height:    44f
            );
            closeBtnObj.SetActive(true);
            closeBtnObj.GetComponentInChildren<Text>().color = Color.red;
            GuiHelper.AddBorder(closeBtnObj, Color.red);
            closeBtnObj.GetComponent<Button>().onClick.AddListener(() => onClose());

            return root;
        }

        /// <summary>
        /// Highlights <paramref name="activeTab"/>'s button and resets every other button to its
        /// default color.
        /// </summary>
        public void SetActiveTab(ShellTab activeTab)
        {
            foreach (KeyValuePair<ShellTab, TabButtonVisual> kvp in m_tabButtons)
            {
                bool isActive = kvp.Key == activeTab;
                kvp.Value.Background.color = isActive ? TabActiveColor : m_tabDefaultColor;
            }
        }

        /// <summary>
        /// Re-reads <see cref="ProfileManager.ActiveProfile"/> into the "Profile: {name}" group
        /// header. Called every frame by <see cref="WizshBoneShellGUI"/> while the shell is
        /// visible, mirroring how <see cref="ShellTopBar.Refresh"/> already polls
        /// <see cref="TwitchIntegration.TwitchAuth"/> state instead of needing a callback wired
        /// through every profile-switch call site.
        /// </summary>
        public void RefreshProfileGroupLabel()
        {
            if (m_profileGroupHeaderText != null)
                GuiHelper.SetTruncatedText(m_profileGroupHeaderText, $"Profile: {ProfileManager.ActiveProfile}", Width - 30f);
        }

        private float CreateGroup(GameObject root, string groupLabel, ShellTab[] tabs, float y, Action<ShellTab> onSelectTab, bool isProfileGroup)
        {
            Text header = GUIManager.Instance.CreateText(
                text:                isProfileGroup ? "" : groupLabel,
                parent:              root.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, y),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            13,
                color:               GUIManager.Instance.ValheimBeige,
                outline:             true,
                outlineColor:        Color.black,
                width:               Width - 30f,
                height:              GroupHeaderHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            header.alignment = TextAnchor.MiddleLeft;

            if (isProfileGroup)
            {
                m_profileGroupHeaderText = header;
                GuiHelper.SetTruncatedText(header, groupLabel, Width - 30f);
            }

            y -= GroupHeaderHeight + GroupHeaderGap;

            foreach (ShellTab tab in tabs)
            {
                ShellTab capturedTab = tab;

                TabButtonVisual visual = CreateStockTabButton(root, capturedTab, y, onSelectTab);
                m_tabButtons[tab] = visual;

                y -= ButtonHeight + ButtonSpacing;
            }

            return y - GroupGap;
        }

        /// <summary>
        /// Jötunn's own sprite-backed button, active/inactive distinguished by tinting that one
        /// Image.
        /// </summary>
        private TabButtonVisual CreateStockTabButton(GameObject root, ShellTab tab, float y, Action<ShellTab> onSelectTab)
        {
            TabButtonVisual visual = CreateStockButton(root, tab.Label(), y);
            visual.Button.onClick.AddListener(() => onSelectTab(tab));
            return visual;
        }

        /// <summary>
        /// Same stock button as a tab's, with no click handler wired - also used directly for
        /// action buttons (e.g. News) that aren't tabs and so never get registered in
        /// <see cref="m_tabButtons"/> or highlighted as active. <paramref name="y"/> is measured
        /// down from the sidebar's top edge, or up from its bottom edge when
        /// <paramref name="anchorBottom"/> is set.
        /// </summary>
        private TabButtonVisual CreateStockButton(GameObject root, string labelText, float y, bool anchorBottom = false)
        {
            float anchorY = anchorBottom ? 0f : 1f;

            GameObject btnObj = GuiHelper.CreateButton(
                text:      labelText,
                parent:    root.transform,
                anchorMin: new Vector2(0.5f, anchorY),
                anchorMax: new Vector2(0.5f, anchorY),
                position:  new Vector2(0f, y),
                width:     Width - 30f,
                height:    ButtonHeight
            );
            btnObj.SetActive(true);

            Button button = btnObj.GetComponent<Button>();
            Image background = btnObj.GetComponent<Image>();
            if (m_tabButtons.Count == 0)
                m_tabDefaultColor = background.color;

            Text label = btnObj.GetComponentInChildren<Text>();
            label.color = GUIManager.Instance.ValheimOrange;

            return new TabButtonVisual { Button = button, Background = background, Label = label };
        }
    }
}
