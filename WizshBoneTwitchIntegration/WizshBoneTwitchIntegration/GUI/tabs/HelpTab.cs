using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Help tab content (formerly "Debug") - a couple of fixed orientation lines (what "Redeems"
    /// means is now covered by RedeemsTab's own under-title description instead of repeated here)
    /// above the same card grid matching <see cref="HomeTab"/>'s card style (RedesignUI.dc.html
    /// only mocks up the first card; the "Show safezone bounds" toggle is carried over from
    /// GUI_OLD/tabs/DebugTab.cs, which the mockup didn't include), followed by a clickable topic
    /// list explaining Profiles/Redeems/Viewers/Settings/Safezones. Purely client-local: nothing
    /// here touches ZDOs or the network.
    /// </summary>
    internal class HelpTab : IShellTabView
    {
        private GameObject m_root;
        private Toggle m_safeZoneDebugToggle;
        private Text m_safeZoneDebugStatusText;

        private readonly Dictionary<string, (GameObject Btn, Image Bg, Text Label)> m_topicButtons = new Dictionary<string, (GameObject, Image, Text)>();
        private Color m_topicButtonDefaultColor;
        private Text m_topicPreviewTitle;
        private Text m_topicDescriptionText;
        private string m_selectedTopic;

        private static readonly string[] TopicOrder = { "Profiles", "Redeems", "Viewers", "Settings", "Safezones" };

        private static readonly Dictionary<string, string> TopicDescriptions = new Dictionary<string, string>
        {
            ["Profiles"] =
                "Each connected Twitch account keeps its own Profile - its own Redeems, Settings, and Creature groups. " +
                "Switch profiles from the Profiles tab to swap your whole setup at once.",
            ["Redeems"] =
                "Redeems are your channel point rewards. Each one triggers an in-game effect - spawning a creature, " +
                "changing terrain, dealing damage, and more - when a viewer redeems it on Twitch. Twitch allows a " +
                "maximum of 50 enabled at once, so keep an eye on how many you have active.",
            ["Viewers"] =
                "Viewers are the Twitch chatters and redeemers you choose to register. Registering a viewer lets you " +
                "give them cosmetics, like a name color, with more perks and effects planned for later.",
            ["Settings"] =
                "Settings control this profile's overall behavior - things like auto-resolving redeems and whether " +
                "chatting keeps working while you're not logged in. Settings save automatically, so there's no Save " +
                "button anywhere in this mod.",
            ["Safezones"] =
                "Safezones - ships, wards, and traders - block certain negative redeem effects while a player is " +
                "inside them. If one ever seems stuck, use the \"Unstuck safezones\" button below; \"Show safezone " +
                "bounds\" can visualize their bounds in-game as a wireframe outline.",
        };

        private ToastType m_nextToastType = ToastType.Success;
        private int m_toastCallCount;

        private const string ToastLoremIpsum =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut " +
            "labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco " +
            "laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in " +
            "voluptate velit esse cillum dolore eu fugiat nulla pariatur.";

        private static readonly Color EnabledColor = new Color(0.95f, 0.65f, 0.2f, 1f);

        // ── layout constants (same 3-col card grid math as HomeTab.cs) ─────────────────────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentMargin = 30f;
        private const float LeftEdgeX  = -(ContentWidth / 2f) + ContentMargin;

        private const float TitleY = -30f;
        private const float TitleWidth = 300f;

        // Fixed (non-scrolling) orientation lines between the title and the card grid.
        private const float DescLine1Y = -55f;
        private const float DescLine2Y = -78f;

        // Pushed down from the old -57 to leave room for the 2 description lines above.
        private const float GridTopY = -117f;
        private const float CardGap = 14f;
        private const float RowGap = 14f;
        private const float CardWidth = (ContentWidth - 2f * ContentMargin - 2f * CardGap) / 3f;
        private const float CardHeight = 160f;

        // Topic list + side preview, sat below the card grid (GridTopY - CardHeight is the grid's
        // bottom edge). Vertical list on the left, selected topic's title+description in a preview
        // column to the right - mirrors RedeemWizard.cs's step-1 effect-type selector.
        private const float TopicsHeaderY = GridTopY - CardHeight - 30f;
        private const float TopicsBodyTopY = TopicsHeaderY - 30f;

        private const float TopicListWidth = 200f;
        private const float TopicBtnHeight = 34f;
        private const float TopicBtnGap = 4f;

        private const float TopicsGapX = 20f;
        private const float TopicPreviewWidth = (ContentWidth - 2f * ContentMargin) - TopicListWidth - TopicsGapX;
        private const float TopicPreviewX = LeftEdgeX + TopicListWidth + TopicsGapX + TopicPreviewWidth / 2f;

        private const float TopicPreviewTitleY = TopicsBodyTopY;
        private const float TopicPreviewDescriptionY = TopicPreviewTitleY - 30f;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "HelpTab");

            GuiHelper.CreateTitle("Help", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);

            BuildDescriptionLines();

            BuildSafezoneUnstuckCard(CardTopCenter(0, 0));
            BuildSafezoneBoundsCard(CardTopCenter(0, 1));
            BuildToastPreviewCard(CardTopCenter(0, 2));

            BuildTopicsSection();

            return m_root;
        }

        public void Refresh()
        {
            // Re-sync in case the value changed elsewhere (e.g. an external config-manager mod)
            // while this tab wasn't visible - matches GUI_OLD/tabs/DebugTab.cs's Refresh().
            if (m_safeZoneDebugToggle != null)
                m_safeZoneDebugToggle.isOn = PluginConfig.configShowSafeZoneDebug.Value;
        }

        private void BuildDescriptionLines()
        {
            GuiHelper.CreateTabDescription(
                "Each connected Twitch account keeps its own Profile - its own Redeems, Settings, and Creature groups.",
                m_root, new Vector2(LeftEdgeX + (ContentWidth - 2f * ContentMargin) / 2f, DescLine1Y), width: ContentWidth - 2f * ContentMargin);

            GuiHelper.CreateTabDescription(
                "Settings save automatically - there's no Save button anywhere in this mod.",
                m_root, new Vector2(LeftEdgeX + (ContentWidth - 2f * ContentMargin) / 2f, DescLine2Y), width: ContentWidth - 2f * ContentMargin);
        }

        private static Vector2 CardTopCenter(int row, int col)
        {
            float x = LeftEdgeX + col * (CardWidth + CardGap) + CardWidth / 2f;
            float y = GridTopY - row * (CardHeight + RowGap);
            return new Vector2(x, y);
        }

        private void BuildSafezoneUnstuckCard(Vector2 topCenter)
        {
            GameObject card = GuiHelper.CreateCard(m_root, topCenter, CardWidth, CardHeight);
            GuiHelper.CreateCardTitle(card, "Safezone Unstuck", CardWidth - 24f);
            GuiHelper.CreateCardButton(card, "Unstuck safezones", CardWidth, OnUnstuckSafezones);
            GuiHelper.CreateCardDescription(card, "Use if Safezone appears to be stuck", CardWidth - 24f, height: 38f);
        }

        private void BuildSafezoneBoundsCard(Vector2 topCenter)
        {
            GameObject card = GuiHelper.CreateCard(m_root, topCenter, CardWidth, CardHeight);
            GuiHelper.CreateCardTitle(card, "Show safezone bounds", CardWidth - 24f);

            bool currentValue = PluginConfig.configShowSafeZoneDebug.Value;
            var row = GuiHelper.CreateToggleStatusRow(card, CardWidth, currentValue, OnSafeZoneDebugToggled, EnabledColor);
            m_safeZoneDebugToggle = row.Toggle;
            m_safeZoneDebugStatusText = row.Status;

            GuiHelper.CreateCardDescription(card, "Shows the bounds of active safezones (ships, wards, traders) in-game as a wireframe outline.", CardWidth - 24f, height: 38f);
        }

        private void BuildToastPreviewCard(Vector2 topCenter)
        {
            GameObject card = GuiHelper.CreateCard(m_root, topCenter, CardWidth, CardHeight);
            GuiHelper.CreateCardTitle(card, "Toast styles", CardWidth - 24f);
            GuiHelper.CreateCardButton(card, "Show next toast", CardWidth, OnShowNextToast);
            GuiHelper.CreateCardDescription(card, "Cycles through the success, warning and error toast styles.", CardWidth - 24f, height: 38f);
        }

        private void BuildTopicsSection()
        {
            GuiHelper.CreateTabDescription(
                "Learn more about:", m_root,
                new Vector2(0f, TopicsHeaderY), width: ContentWidth - 2f * ContentMargin);

            float yOffset = TopicsBodyTopY - TopicBtnHeight / 2f;
            foreach (string topic in TopicOrder)
            {
                GameObject btnObj = GuiHelper.CreateButton(
                    text: topic,
                    parent: m_root.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(LeftEdgeX + TopicListWidth / 2f, yOffset),
                    width: TopicListWidth,
                    height: TopicBtnHeight
                );
                btnObj.SetActive(true);

                Image bg = btnObj.GetComponent<Image>();
                if (m_topicButtons.Count == 0)
                    m_topicButtonDefaultColor = bg.color;
                Text label = btnObj.GetComponentInChildren<Text>();
                label.color = GUIManager.Instance.ValheimOrange;
                label.alignment = TextAnchor.MiddleLeft;
                RectTransform labelRt = label.rectTransform;
                labelRt.offsetMin = new Vector2(labelRt.offsetMin.x + ListRow.LeftPadding, labelRt.offsetMin.y);

                btnObj.GetComponent<Button>().onClick.AddListener(() => SelectTopic(topic));

                m_topicButtons[topic] = (btnObj, bg, label);

                yOffset -= TopicBtnHeight + TopicBtnGap;
            }

            m_topicPreviewTitle = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(TopicPreviewX, TopicPreviewTitleY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: TopicPreviewWidth,
                height: 24f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_topicPreviewTitle.alignment = TextAnchor.MiddleLeft;
            GuiHelper.PivotToTop(m_topicPreviewTitle.rectTransform, TopicPreviewTitleY);

            m_topicDescriptionText = GuiHelper.CreateTabDescription(
                "", m_root, new Vector2(TopicPreviewX, TopicPreviewDescriptionY), width: TopicPreviewWidth);
            GuiHelper.MakeDescriptionExpandDownward(m_topicDescriptionText, TopicPreviewDescriptionY);

            SelectTopic(TopicOrder[0]);
        }

        private void SelectTopic(string topic)
        {
            m_selectedTopic = topic;
            RefreshTopicSelection();
        }

        private void RefreshTopicSelection()
        {
            foreach (var kvp in m_topicButtons)
            {
                bool selected = kvp.Key == m_selectedTopic;
                kvp.Value.Bg.color = selected ? ShellSidebar.TabActiveColor : m_topicButtonDefaultColor;
            }

            m_topicPreviewTitle.text = m_selectedTopic;
            m_topicDescriptionText.text = TopicDescriptions[m_selectedTopic];
        }

        private void OnShowNextToast()
        {
            ToastType type = m_nextToastType;
            m_nextToastType = (ToastType)(((int)m_nextToastType + 1) % 3);

            bool useLongText = m_toastCallCount % 2 == 1;
            m_toastCallCount++;

            string message = useLongText
                ? $"This is a {type.ToString().ToLowerInvariant()} toast. {ToastLoremIpsum}"
                : $"This is a {type.ToString().ToLowerInvariant()} toast.";

            ToastNotifications.Show(message, type);
        }

        private void OnSafeZoneDebugToggled(bool value)
        {
            // Setting .Value fires configShowSafeZoneDebug.SettingChanged, which already calls
            // TwitchSafeZone.RefreshDebugVisuals - no extra plumbing needed here.
            PluginConfig.configShowSafeZoneDebug.Value = value;

            m_safeZoneDebugStatusText.text = value ? "Enabled" : "Disabled";
            m_safeZoneDebugStatusText.color = value ? EnabledColor : GUIManager.Instance.ValheimBeige;
        }

        private void OnUnstuckSafezones()
        {
            // Clears this client's own stuck safe-zone bookkeeping (entry counter / "in safe
            // zone" flag / HUD) - never touches placed Wards, ships, or trader zones.
            TwitchSafeZone.ForceExitAllZones();
            ToastNotifications.Show("Safezone state reset.", ToastType.Success);
        }
    }
}
