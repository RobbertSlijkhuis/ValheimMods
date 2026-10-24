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
    /// card; the "Clear Toasts" card is a safety net for stuck toasts, and the "Debug mode" toggle
    /// isn't in the mockup), followed by a clickable topic list explaining
    /// Login/Profiles/Redeems/Viewers/Settings/Safezones/Commands. Purely client-local: nothing here touches
    /// ZDOs or the network.
    /// </summary>
    internal class HelpTab : IShellTabView
    {
        private GameObject m_root;
        private Toggle m_safeZoneDebugToggle;
        private Text m_safeZoneDebugStatusText;

        private readonly SelectorList m_topicList = new SelectorList();
        private readonly SelectorPreviewPanel m_topicPreview = new SelectorPreviewPanel();

        private static readonly string[] TopicOrder = { "Login", "Profiles", "Redeems", "Viewers", "Settings", "Safezones", "Commands" };

        private static readonly Dictionary<string, string> TopicDescriptions = new Dictionary<string, string>
        {
            ["Login"] =
                "You can log in with the \"Twitch Login\" button in the top bar. It opens your main browser, where " +
                "you need to log in to Twitch and authorize the mod. You can find the status on the left!" +
                "\n\nFor the chatting system to work you need to be logged in, so the mod can read the chat messages." +
                "\n\nIf the login goes wrong for some reason, try logging out or restarting the world. This happens occasionally." +
                "\n\nA login session lasts around 4 hours. You will be warned when the session is about to expire.",
            ["Profiles"] =
                "Each profile holds all the configuration for Redeems and Settings, so switching profiles switches " +
                "your entire setup at once. You can import, export, copy and even sync profiles. That lets you share " +
                "a profile, make a copy to adjust it slightly, or make sure you and your friends have the same " +
                "Redeems and Settings." +
                "\n\nBuilt-in profiles (Normal and Chatting only, shown in grey-blue) ship with the mod. They can't be renamed, deleted or synced, but Reset puts one back to its original redeems and settings." +
                "\n\nSynced profiles are read-only for everyone except the owner, so only the owner can push changes. " +
                "This prevents drift: some redeems read values from their configuration, and those must be the same for everyone!" +
                "\n\nA redeem history (the last 1000 redeems) is kept, which allows you to complete redeems or refund points. " +
                "If you like to have control over that, I recommend keeping auto-resolve off.",
            ["Redeems"] =
                "Redeems are your custom channel point rewards. Each one triggers an in-game effect, like spawning " +
                "creatures, raining logs, placing traps and more." +
                "\n\nKeep in mind that Twitch allows a maximum of 50 custom channel point rewards per channel, and that " +
                "includes the ones you already have set up yourself (or through other tools). Make sure you have enough free " +
                "slots for the redeems of your profile: every enabled redeem takes one slot, while disabled redeems are not sent " +
                "to Twitch. The Redeems tab shows how many slots you are using, and the mod will not push your redeems if " +
                "they do not fit next to your other rewards. In that case delete some of your own rewards or disable redeems." +
                "\n\nTo create a new redeem, open the Redeems tab, click \"+ New redeem\" and follow the wizard. " +
                "Step 1: choose an effect. Step 2: configure the chosen effect. Step 3: set up the Twitch channel point reward." +
                "\n\nBEWARE: any changes made to the description, points or cooldowns will remove the channel point reward on Twitch " +
                "and create a new one. Make sure you refund any points you want to before making changes, or they can be lost!",
            ["Viewers"] =
                "Viewers are the Twitch chatters and redeemers you choose to register. Registering a viewer lets you " +
                "give them cosmetics, like a creature color. More perks and effects are planned for a later update." +
                "\n\nRight now you can add viewers manually by name and give them a color. Creatures they claim or spawn will " +
                "then get that color!\n\nFor now this is the only way. In a future update, viewers will be " +
                "able to do most of this themselves, and your only job will be to approve them.",
            ["Settings"] =
                "Settings control this profile's overall behavior. They are grouped into:" +
                "\n- Redeems: enable redeems on login, auto-resolve redeems and the redeem title prefix" +
                "\n- Chatting: showing viewer chat above creatures, claiming creatures with !claim, and how often and how far to scan" +
                "\n- Creatures: same faction, scaling by biome tier, and limits on how many spawn and how far away" +
                "\n- Indestructible: make boats, chests, portals and crops indestructible" +
                "\n- Safezones: the Twitch Ward recipe and behavior, and safezones on traders and boats" +
                "\n\nSettings save automatically; there's no Save button anywhere in this mod.",
            ["Safezones"] =
                "Safezones can be put on boats, wards and traders. While a player is inside one, redeem effects " +
                "are blocked unless \"Ignore safezone\" is enabled on that redeem." +
                "\n\nSpawned creatures can optionally be kept from entering a Twitch Ward's safezone (pushed out) and can optionally " +
                "get burned when they do (both are ward settings you can toggle). Their attacks are stopped, so players inside a safezone are safe from them." +
                "\n\nSome redeems will not spawn anything inside a safezone, and they stop completely as soon as the " +
                "player enters one." +
                "\n\nIf a safezone ever seems stuck, use the \"Unstuck safezones\" button above." +
                "\n\n\"Debug mode\" visualizes their bounds, and the bounds of any active weather zone or timed terrain edit, in-game as a wireframe outline. It also floats a countdown over timed redeem objects (boats, weather zones, ...).",
            ["Commands"] =
                "If a redeem leaves something behind that gets in your way, you can clean it up with the remove commands. " +
                "Open the in-game console (F5) and type one of them. They only remove things spawned by this mod, never " +
                "your own builds or vanilla creatures." +
                "\n\nMost commands remove everything within 30 meters of you. Add a number to change the radius, for example " +
                "\"WBTIRemoveCreatures 60\"." +
                "\n\n- WBTIRemoveCreatures: spawned creatures" +
                "\n- WBTIRemoveBoats: spawned boats" +
                "\n- WBTIRemoveChests: surprise chests" +
                "\n- WBTIRemoveDoors: spawned doors" +
                "\n- WBTIRemoveTraps: spawned traps" +
                "\n- WBTIRemoveWindmills: spawned windmills" +
                "\n- WBTIRemoveLogs: spawned logs" +
                "\n- WBTIRemoveMist: spawned mist (default radius is 100 meters)" +
                "\n- WBTIRemoveWeather: spawned weather zones and restores the normal weather. This one has no radius, " +
                "it removes all of them." +
                "\n- WBTIRemoveTerrainEdit: undoes terrain edits, including permanent ones, and restores the original terrain " +
                "(default radius is 100 meters). Terrain edits made before this command existed can't be undone." +
                "\n\nTip: if you are not sure which one you need, run the matching command while standing next to the problem.",
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

        // Height of both the list card and the preview card, so their top/bottom edges line up.
        // Both stretch from the body's top edge down to the same ContentMargin the tab leaves on
        // its left/right. The preview's description scrolls when it's still longer than that.
        private const float ContentHeight = WizshBoneShellGUI.PanelHeight - ShellTopBar.Height;  // 660
        private const float TopicsListHeight = ContentHeight - ContentMargin + TopicsBodyTopY;

        private const float TopicPreviewPadding = 14f; // matches RedeemWizard's BoxPadding

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "HelpTab");

            GuiHelper.CreateTitle("Help", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);
            GuiHelper.CreateTabDescription(
                "Welcome to the help section! Here you can find tools to deal with potential problems and more information about things in the mod!",
                m_root, new Vector2(0f, DescriptionY), width: ContentWidth - 2f * ContentMargin);

            BuildSafezoneUnstuckCard(CardTopCenter(0, 0));
            BuildSafezoneBoundsCard(CardTopCenter(0, 1));
            BuildClearToastsCard(CardTopCenter(0, 2));

            BuildTopicsSection();

            return m_root;
        }

        public void Refresh()
        {
            // Re-sync in case the value changed elsewhere (e.g. an external config-manager mod)
            // while this tab wasn't visible.
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
            GuiHelper.CreateCardDescription(card, "Use if a safezone appears to be stuck", CardWidth - 24f, height: 38f);
        }

        private void BuildSafezoneBoundsCard(Vector2 topCenter)
        {
            GameObject card = GuiHelper.CreateCard(m_root, topCenter, CardWidth, CardHeight);
            GuiHelper.CreateCardTitle(card, "Debug mode", CardWidth - 24f);

            bool currentValue = PluginConfig.configShowSafeZoneDebug.Value;
            var row = GuiHelper.CreateToggleStatusRow(card, CardWidth, currentValue, OnSafeZoneDebugToggled, EnabledColor);
            m_safeZoneDebugToggle = row.Toggle;
            m_safeZoneDebugStatusText = row.Status;

            GuiHelper.CreateCardDescription(card, "Shows safezone, weather zone and timed terrain edit bounds as a wireframe, plus time left on timed redeems.", CardWidth - 24f, height: 38f);
        }

        private void BuildClearToastsCard(Vector2 topCenter)
        {
            GameObject card = GuiHelper.CreateCard(m_root, topCenter, CardWidth, CardHeight);
            GuiHelper.CreateCardTitle(card, "Clear Toasts", CardWidth - 24f);
            GuiHelper.CreateCardButton(card, "Clear toasts", CardWidth, ToastNotifications.ClearAll);
            GuiHelper.CreateCardDescription(card, "Use if toast notifications get stuck on screen", CardWidth - 24f, height: 38f);
        }

        private void BuildTopicsSection()
        {
            // Same title style as the tab's own "Help" title (orange, upper-case, left-aligned).
            GuiHelper.CreateTitle(
                "Learn more about", m_root,
                new Vector2(LeftEdgeX + TitleWidth / 2f, TopicsHeaderY), width: TitleWidth);

            GameObject listCard = GuiHelper.CreateCard(
                m_root, new Vector2(LeftEdgeX + TopicListWidth / 2f, TopicsBodyTopY), TopicListWidth, TopicsListHeight);

            var items = new List<(string Key, string Label)>();
            foreach (string topic in TopicOrder)
                items.Add((topic, topic));
            m_topicList.Build(listCard.transform, items, TopicListWidth, TopicBtnHeight, TopicBtnGap, SelectTopic);

            m_topicPreview.Build(
                m_root, new Vector2(TopicPreviewX, TopicsBodyTopY), TopicPreviewWidth, TopicsListHeight,
                TopicPreviewPadding, scrollable: true);

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
            // ColliderBoundsHelper.Reevaluate and RedeemTimerHelper.SetVisible - no extra plumbing needed here.
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
