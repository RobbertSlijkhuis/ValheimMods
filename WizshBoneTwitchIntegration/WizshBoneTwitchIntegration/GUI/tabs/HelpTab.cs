using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Help tab content (formerly "Debug") - a single under-title orientation line (same
    /// TitleY/DescriptionY convention as SettingsTab/RedeemsTab), above the same card grid
    /// matching <see cref="HomeTab"/>'s card style (RedesignUI.dc.html only mocks up the first
    /// card; the "Show safezone bounds" toggle is carried over from GUI_OLD/tabs/DebugTab.cs,
    /// which the mockup didn't include), followed by a clickable topic list explaining
    /// Profiles/Redeems/Viewers/Settings/Safezones. Purely client-local: nothing here touches
    /// ZDOs or the network.
    /// </summary>
    internal class HelpTab : IShellTabView
    {
        private GameObject m_root;
        private Toggle m_safeZoneDebugToggle;
        private Text m_safeZoneDebugStatusText;

        private readonly SelectorList m_topicList = new SelectorList();
        private readonly SelectorPreviewPanel m_topicPreview = new SelectorPreviewPanel();

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

        private static readonly Color EnabledColor = new Color(0.95f, 0.65f, 0.2f, 1f);

        // ── layout constants (same 3-col card grid math as HomeTab.cs) ─────────────────────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentMargin = 30f;
        private const float LeftEdgeX  = -(ContentWidth / 2f) + ContentMargin;

        private const float TitleY = -30f;
        private const float TitleWidth = 300f;
        private const float DescriptionY = -50f;

        // Same 22px gap SettingsTab's ScrollTopInset leaves after its own DescriptionY.
        private const float GridTopY = -72f;
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

        // Total stacked height of the topic buttons - the list card and preview card are both
        // built at this height so their top/bottom edges line up.
        private static readonly float TopicsListHeight =
            TopicOrder.Length * TopicBtnHeight + (TopicOrder.Length - 1) * TopicBtnGap;

        private const float TopicPreviewPadding = 14f; // matches RedeemWizard's BoxPadding

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "HelpTab");

            GuiHelper.CreateTitle("Help", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);
            GuiHelper.CreateTabDescription(
                "Here you can find tools that can help with problems and information about the different aspects of the mod",
                m_root, new Vector2(0f, DescriptionY), width: ContentWidth - 2f * ContentMargin);

            BuildSafezoneUnstuckCard(CardTopCenter(0, 0));
            BuildSafezoneBoundsCard(CardTopCenter(0, 1));

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

        private void BuildTopicsSection()
        {
            GuiHelper.CreateTabDescription(
                "Learn more about:", m_root,
                new Vector2(0f, TopicsHeaderY), width: ContentWidth - 2f * ContentMargin);

            GameObject listCard = GuiHelper.CreateCard(
                m_root, new Vector2(LeftEdgeX + TopicListWidth / 2f, TopicsBodyTopY), TopicListWidth, TopicsListHeight);

            var items = new List<(string Key, string Label)>();
            foreach (string topic in TopicOrder)
                items.Add((topic, topic));
            m_topicList.Build(listCard.transform, items, TopicListWidth, TopicBtnHeight, TopicBtnGap, SelectTopic);

            m_topicPreview.Build(
                m_root, new Vector2(TopicPreviewX, TopicsBodyTopY), TopicPreviewWidth, TopicsListHeight,
                TopicPreviewPadding);

            SelectTopic(TopicOrder[0]);
        }

        private void SelectTopic(string topic)
        {
            m_topicList.Select(topic);
            m_topicPreview.UpdateContent(topic, TopicDescriptions[topic]);
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
