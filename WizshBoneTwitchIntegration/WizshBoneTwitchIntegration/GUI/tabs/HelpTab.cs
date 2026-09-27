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

        private readonly Dictionary<string, Text> m_topicLinks = new Dictionary<string, Text>();
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

        // Topic links row + description area, sat below the card grid (GridTopY - CardHeight is
        // the grid's bottom edge).
        private const float TopicsLabelY = GridTopY - CardHeight - 30f;
        private const float TopicsLinkY = TopicsLabelY - 26f;
        private const float TopicLinkWidth = 150f;
        private const float TopicLinkHeight = 22f;
        private const float TopicLinkGap = 20f;
        private const float TopicsRowWidth = TopicLinkWidth * 5 + TopicLinkGap * 4;
        private const float TopicDescriptionY = TopicsLinkY - TopicLinkHeight - 20f;

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
                new Vector2(0f, TopicsLabelY), width: ContentWidth - 2f * ContentMargin);

            float rowLeftEdge = -TopicsRowWidth / 2f;
            for (int i = 0; i < TopicOrder.Length; i++)
            {
                string topic = TopicOrder[i];
                float x = rowLeftEdge + i * (TopicLinkWidth + TopicLinkGap) + TopicLinkWidth / 2f;

                Text link = GuiHelper.CreateTextLink(m_root, topic, new Vector2(x, TopicsLinkY),
                    TopicLinkWidth, TopicLinkHeight, () => OnTopicSelected(topic));
                m_topicLinks[topic] = link;
            }

            m_topicDescriptionText = GuiHelper.CreateTabDescription(
                "", m_root, new Vector2(0f, TopicDescriptionY), width: ContentWidth - 2f * ContentMargin);
            GuiHelper.MakeDescriptionExpandDownward(m_topicDescriptionText, TopicDescriptionY);

            OnTopicSelected(TopicOrder[0]);
        }

        private void OnTopicSelected(string topic)
        {
            if (m_selectedTopic != null && m_topicLinks.TryGetValue(m_selectedTopic, out Text previous))
                previous.color = GUIManager.Instance.ValheimOrange;

            m_selectedTopic = topic;
            m_topicLinks[topic].color = GUIManager.Instance.ValheimBeige;
            m_topicDescriptionText.text = TopicDescriptions[topic];
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
