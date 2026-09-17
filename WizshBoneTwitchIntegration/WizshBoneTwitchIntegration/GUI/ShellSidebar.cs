using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Configs;
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

        // RedesignUI.dc.html's sidebar tab-button colors, used only when configAlternativeSidebar is on.
        private static readonly Color AltRowActiveBgColor = new Color(0f, 0f, 0f, 80f / 255f);    // #00000050
        private static readonly Color AltRowInactiveBgColor = new Color(0f, 0f, 0f, 0f);           // transparent
        private static readonly Color AltBorderAccentColor = new Color(240f / 255f, 168f / 255f, 80f / 255f);  // #f0a850
        private static readonly Color AltTextActiveColor = new Color(232f / 255f, 176f / 255f, 96f / 255f);    // #e8b060
        private static readonly Color AltTextInactiveColor = new Color(200f / 255f, 191f / 255f, 168f / 255f); // #c8bfa8
        private const float AltBorderAccentWidth = 3f;

        /// <summary>
        /// Per-tab visuals a style's construction path populated - <see cref="BorderAccent"/>/
        /// <see cref="Label"/> stay null under the stock style, since <see cref="SetActiveTab"/>
        /// only ever reaches for them when <see cref="m_useAlternativeStyle"/> is true, which is
        /// only true when <see cref="CreateAltTabRow"/> (which always sets them) built every entry.
        /// </summary>
        private class TabButtonVisual
        {
            public Button Button;
            public Image Background;
            public Image BorderAccent;
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

        private readonly Dictionary<ShellTab, TabButtonVisual> m_tabButtons = new Dictionary<ShellTab, TabButtonVisual>();
        private Color m_tabDefaultColor;
        private Text m_profileGroupHeaderText;
        private bool m_useAlternativeStyle;

        /// <summary>
        /// Builds the sidebar region as a child of <paramref name="panel"/>.
        /// </summary>
        /// <param name="onSelectTab">Invoked with the clicked tab.</param>
        /// <param name="onClose">Invoked when the Close button is clicked.</param>
        public GameObject Create(GameObject panel, Action<ShellTab> onSelectTab, Action onClose)
        {
            // ShellSidebar is a readonly field of WizshBoneShellGUI, reused (not reconstructed)
            // across a style-change rebuild (see WizshBoneShellGUI.DestroyPanel) - reset every
            // piece of mutable per-build state so a second Create() call is as clean as the first.
            m_useAlternativeStyle = PluginConfig.configAlternativeSidebar.Value;
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
            foreach (KeyValuePair<ShellTab, TabButtonVisual> kvp in m_tabButtons)
            {
                bool isActive = kvp.Key == activeTab;
                TabButtonVisual visual = kvp.Value;

                if (m_useAlternativeStyle)
                {
                    visual.Background.color = isActive ? AltRowActiveBgColor : AltRowInactiveBgColor;
                    visual.BorderAccent.color = isActive ? AltBorderAccentColor : AltRowInactiveBgColor;
                    visual.Label.color = isActive ? AltTextActiveColor : AltTextInactiveColor;
                }
                else
                {
                    visual.Background.color = isActive ? TabActiveColor : m_tabDefaultColor;
                }
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

                TabButtonVisual visual = m_useAlternativeStyle
                    ? CreateAltTabRow(root, capturedTab, y, onSelectTab)
                    : CreateStockTabButton(root, capturedTab, y, onSelectTab);
                m_tabButtons[tab] = visual;

                y -= ButtonHeight + ButtonSpacing;
            }

            return y - GroupGap;
        }

        /// <summary>
        /// Today's stock look: Jötunn's own sprite-backed button, active/inactive distinguished by
        /// tinting that one Image - unchanged from before the Alternative UI style existed.
        /// </summary>
        private TabButtonVisual CreateStockTabButton(GameObject root, ShellTab tab, float y, Action<ShellTab> onSelectTab)
        {
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
            Image background = btnObj.GetComponent<Image>();
            if (m_tabButtons.Count == 0)
                m_tabDefaultColor = background.color;

            button.onClick.AddListener(() => onSelectTab(tab));

            return new TabButtonVisual { Button = button, Background = background };
        }

        /// <summary>
        /// RedesignUI.dc.html's tab-row look: a transparent row that tints to <see cref="AltRowActiveBgColor"/>
        /// with a left <see cref="AltBorderAccentColor"/> border accent when active - hand-built from
        /// plain Images the same way <see cref="GuiFieldBuilder.CreateRectBoolField"/> is, since
        /// Jötunn's button prefab brings its own sprite background that can't be recolored into
        /// this shape.
        /// </summary>
        private TabButtonVisual CreateAltTabRow(GameObject root, ShellTab tab, float y, Action<ShellTab> onSelectTab)
        {
            GameObject rowObj = new GameObject("TabRow", typeof(RectTransform));
            rowObj.transform.SetParent(root.transform, false);

            RectTransform rowRt = (RectTransform)rowObj.transform;
            rowRt.anchorMin = new Vector2(0.5f, 1f);
            rowRt.anchorMax = new Vector2(0.5f, 1f);
            rowRt.pivot = new Vector2(0.5f, 0.5f);
            rowRt.sizeDelta = new Vector2(Width - 30f, ButtonHeight);
            rowRt.anchoredPosition = new Vector2(0f, Mathf.Round(y));

            Image background = rowObj.AddComponent<Image>();
            background.color = AltRowInactiveBgColor;

            Button button = rowObj.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => onSelectTab(tab));

            GameObject borderObj = new GameObject("BorderAccent", typeof(RectTransform));
            borderObj.transform.SetParent(rowObj.transform, false);
            RectTransform borderRt = (RectTransform)borderObj.transform;
            borderRt.anchorMin = new Vector2(0f, 0f);
            borderRt.anchorMax = new Vector2(0f, 1f);
            borderRt.pivot = new Vector2(0f, 0.5f);
            borderRt.sizeDelta = new Vector2(AltBorderAccentWidth, 0f);
            borderRt.anchoredPosition = Vector2.zero;
            Image borderAccent = borderObj.AddComponent<Image>();
            borderAccent.raycastTarget = false;
            borderAccent.color = AltRowInactiveBgColor;

            // CreateText leaves the RectTransform's pivot at Unity's default (0.5, 0.5), so
            // position.x is always the box's CENTER, never its left edge - matching every other
            // left-point-anchored text in this codebase (e.g. ShellTopBar.cs's m_statusLabel),
            // position.x is the desired left edge plus half the box's own width.
            float labelLeftEdge = AltBorderAccentWidth + 12f;
            float labelWidth = Width - 30f - AltBorderAccentWidth - 20f;

            Text label = GUIManager.Instance.CreateText(
                text:                 tab.Label(),
                parent:               rowObj.transform,
                anchorMin:            new Vector2(0f, 0.5f),
                anchorMax:            new Vector2(0f, 0.5f),
                position:             new Vector2(labelLeftEdge + labelWidth / 2f, 0f),
                font:                 GUIManager.Instance.AveriaSerifBold,
                fontSize:             14,
                color:                AltTextInactiveColor,
                outline:              true,
                outlineColor:         Color.black,
                width:                labelWidth,
                height:               ButtonHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            label.raycastTarget = false;

            return new TabButtonVisual { Button = button, Background = background, BorderAccent = borderAccent, Label = label };
        }
    }
}
