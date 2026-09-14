using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Left sidebar: "WizshBone" title, one button per <see cref="ShellTab"/>, and a Close button
    /// pinned to the bottom. GUI_OLD/WizshBoneSettingsGUI.cs uses a horizontal tab-button row
    /// instead of a vertical sidebar, so this layout is new; the active/inactive button-color
    /// idiom is ported from its SetTabButtonColor.
    /// </summary>
    internal class ShellSidebar
    {
        public const float Width = 220f;

        private static readonly Color TabActiveColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private readonly Dictionary<ShellTab, Button> m_tabButtons = new Dictionary<ShellTab, Button>();
        private Color m_tabDefaultColor;

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
            GuiHelper.AddBackground(root, new Color(0f, 0f, 0f, 0.35f));

            Text title = GUIManager.Instance.CreateText(
                text:                "WizshBone",
                parent:              root.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            new Vector2(0f, -30f),
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

            float y = -80f;
            foreach (ShellTab tab in ShellTabExtensions.All)
            {
                ShellTab capturedTab = tab;

                GameObject btnObj = GUIManager.Instance.CreateButton(
                    text:      tab.Label(),
                    parent:    root.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position:  new Vector2(0f, y),
                    width:     Width - 30f,
                    height:    44f
                );
                btnObj.SetActive(true);

                Button button = btnObj.GetComponent<Button>();
                if (m_tabButtons.Count == 0)
                    m_tabDefaultColor = btnObj.GetComponent<Image>().color;

                button.onClick.AddListener(() => onSelectTab(capturedTab));
                m_tabButtons[tab] = button;

                y -= 50f;
            }

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
    }
}
