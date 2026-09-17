using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Left sidebar: "WizshBone" title, two labeled nav groups (matching RedesignUI.dc.html) -
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

        private static readonly Color TabActiveColor = new Color(0.9f, 0.9f, 0.9f, 1f);

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
        private const float FirstGroupY      = -80f;
        private const float GroupHeaderHeight = 22f;
        private const float GroupHeaderGap    = 12f;
        private const float ButtonHeight      = 44f;
        private const float ButtonSpacing     = 6f;
        private const float GroupGap          = 16f;

        private readonly Dictionary<ShellTab, Button> m_tabButtons = new Dictionary<ShellTab, Button>();
        private Color m_tabDefaultColor;
        private Text m_profileGroupHeaderText;

        /// <summary>
        /// Builds the sidebar region as a child of <paramref name="panel"/>.
        /// </summary>
        /// <param name="onSelectTab">Invoked with the clicked tab.</param>
        /// <param name="onClose">Invoked when the Close button is clicked.</param>
        public GameObject Create(GameObject panel, Action<ShellTab> onSelectTab, Action onClose)
        {
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

            Text title = GUIManager.Instance.CreateText(
                text:                "WizshBone",
                parent:              root.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, TitleY),
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            20,
                color:               GUIManager.Instance.ValheimOrange,
                outline:             true,
                outlineColor:        Color.black,
                width:               Width - 20f,
                height:              30f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            title.alignment = TextAnchor.MiddleCenter;

            GameObject titleDivider = GuiHelper.CreateRegion(root, "TitleDivider",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, TitleDividerY - DividerThickness), new Vector2(-DividerRightInset, TitleDividerY));
            GuiHelper.AddBackground(titleDivider, GuiHelper.DividerColorStrong);

            float y = FirstGroupY;

            y = CreateGroup(root, "General", GeneralGroupTabs, y, onSelectTab, isProfileGroup: false);
            y = CreateGroup(root, $"Profile: {ProfileManager.ActiveProfile}", ProfileGroupTabs, y, onSelectTab, isProfileGroup: true);

            GameObject closeBtnObj = GUIManager.Instance.CreateButton(
                text:      "Close",
                parent:    root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position:  new Vector2(0f, 40f),
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
            foreach (KeyValuePair<ShellTab, Button> kvp in m_tabButtons)
            {
                Image image = kvp.Value.GetComponent<Image>();
                if (image != null)
                    image.color = kvp.Key == activeTab ? TabActiveColor : m_tabDefaultColor;
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
                m_profileGroupHeaderText.text = $"Profile: {ProfileManager.ActiveProfile}";
        }

        private float CreateGroup(GameObject root, string groupLabel, ShellTab[] tabs, float y, Action<ShellTab> onSelectTab, bool isProfileGroup)
        {
            Text header = GUIManager.Instance.CreateText(
                text:                groupLabel,
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
                m_profileGroupHeaderText = header;

            y -= GroupHeaderHeight + GroupHeaderGap;

            foreach (ShellTab tab in tabs)
            {
                ShellTab capturedTab = tab;

                GameObject btnObj = GUIManager.Instance.CreateButton(
                    text:      tab.Label(),
                    parent:    root.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position:  new Vector2(0f, y),
                    width:     Width - 30f,
                    height:    ButtonHeight
                );
                btnObj.SetActive(true);

                Button button = btnObj.GetComponent<Button>();
                if (m_tabButtons.Count == 0)
                    m_tabDefaultColor = btnObj.GetComponent<Image>().color;

                button.onClick.AddListener(() => onSelectTab(capturedTab));
                m_tabButtons[tab] = button;

                y -= ButtonHeight + ButtonSpacing;
            }

            return y - GroupGap;
        }
    }
}
