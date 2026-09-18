using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Help tab content (formerly "Debug") - a few fixed orientation lines (what "Redeems" means
    /// is now covered by RedeemsTab's own under-title description instead of repeated here; see
    /// how hovering a "?" icon works, demonstrated live) above the same card grid matching
    /// <see cref="HomeTab"/>'s card style (RedesignUI.dc.html only mocks up the first card; the
    /// "Show safezone bounds" toggle is carried over from GUI_OLD/tabs/DebugTab.cs, which the
    /// mockup didn't include). Purely client-local: nothing here touches ZDOs or the network.
    /// </summary>
    internal class HelpTab : IShellTabView
    {
        private GameObject m_root;
        private Toggle m_safeZoneDebugToggle;
        private Text m_safeZoneDebugStatusText;

        private static readonly Color EnabledColor = new Color(0.95f, 0.65f, 0.2f, 1f);

        // ── layout constants (same 3-col card grid math as HomeTab.cs) ─────────────────────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentMargin = 30f;
        private const float LeftEdgeX  = -(ContentWidth / 2f) + ContentMargin;
        private const float RightEdgeX = (ContentWidth / 2f) - ContentMargin;

        private const float TitleY = -30f;
        private const float TitleWidth = 300f;

        // Fixed (non-scrolling) orientation lines between the title and the card grid.
        private const float DescLine1Y = -55f;
        private const float DescLine2Y = -78f;
        private const float DescLine3Y = -101f;
        private const float DescIconSize = 18f;
        private const float DescIconGap = 10f;
        private const float DescLine3Width = ContentWidth - 2f * ContentMargin - DescIconSize - DescIconGap;

        // Pushed down from the old -57 to leave room for the 3 description lines above.
        private const float GridTopY = -140f;
        private const float CardGap = 14f;
        private const float RowGap = 14f;
        private const float CardWidth = (ContentWidth - 2f * ContentMargin - 2f * CardGap) / 3f;
        private const float CardHeight = 160f;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "HelpTab");

            GuiHelper.CreateTitle("Help", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);

            BuildDescriptionLines();

            BuildSafezoneUnstuckCard(CardTopCenter(0, 0));
            BuildSafezoneBoundsCard(CardTopCenter(0, 1));

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

            GuiHelper.CreateTabDescription(
                "Hovering a \"?\" icon like this one shows an explanatory tooltip:",
                m_root, new Vector2(LeftEdgeX + DescLine3Width / 2f, DescLine3Y), width: DescLine3Width);

            GuiHelper.CreateHelpIcon(m_root, new Vector2(RightEdgeX - DescIconSize / 2f, DescLine3Y),
                title: "Example tooltip",
                description: "This is exactly what hovering a \"?\" icon shows: a bold title line, then a short explanation underneath.");
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
            ToastNotifications.Show("Safezone state reset.");
        }
    }
}
