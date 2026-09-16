using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Debug tab content - a card grid matching <see cref="HomeTab"/>'s card style (RedesignUI.dc.html
    /// only mocks up the first card; the "Show safezone bounds" toggle is carried over from
    /// GUI_OLD/tabs/DebugTab.cs, which the mockup didn't include). Purely client-local: nothing
    /// here touches ZDOs or the network.
    /// </summary>
    internal class DebugTab : IShellTabView
    {
        private GameObject m_root;
        private Toggle m_safeZoneDebugToggle;
        private Text m_safeZoneDebugStatusText;

        private static readonly Color MutedLabelColor = new Color(0.66f, 0.62f, 0.53f, 1f);
        private static readonly Color DescriptionColor = new Color(0.54f, 0.5f, 0.44f, 1f);
        private static readonly Color CardBackground = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color EnabledColor = new Color(0.95f, 0.65f, 0.2f, 1f);
        private static readonly Color DisabledColor = new Color(0.6f, 0.56f, 0.49f, 1f);

        // ── layout constants (same 3-col card grid math as HomeTab.cs; only 2 of the 3 columns
        // are used so far, leaving room for a future card without re-deriving the grid) ─────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentMargin = 30f;
        private const float LeftEdgeX = -(ContentWidth / 2f) + ContentMargin;

        private const float TitleY = -30f;
        private const float TitleWidth = 300f;

        // See HomeTab.cs's GridTopY comment - top-pivoted cards need -57, not -75, to visually
        // line up with the center-pivoted toolbar rows on Profiles/Redeems/Viewers.
        private const float GridTopY = -57f;
        private const float CardGap = 14f;
        private const float CardWidth = (ContentWidth - 2f * ContentMargin - 2f * CardGap) / 3f;
        private const float CardHeight = 150f;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "DebugTab");

            GuiHelper.CreateTitle("Debug", m_root, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);

            BuildSafezoneUnstuckCard(CardTopCenter(0));
            BuildSafezoneBoundsCard(CardTopCenter(1));

            return m_root;
        }

        public void Refresh()
        {
            // Re-sync in case the value changed elsewhere (e.g. an external config-manager mod)
            // while this tab wasn't visible - matches GUI_OLD/tabs/DebugTab.cs's Refresh().
            if (m_safeZoneDebugToggle != null)
                m_safeZoneDebugToggle.isOn = PluginConfig.configShowSafeZoneDebug.Value;
        }

        private static Vector2 CardTopCenter(int col)
        {
            float x = LeftEdgeX + col * (CardWidth + CardGap) + CardWidth / 2f;
            return new Vector2(x, GridTopY);
        }

        private void BuildSafezoneUnstuckCard(Vector2 topCenter)
        {
            GameObject card = CreateCard(topCenter);
            CreateCardText(card, "Safezone Unstuck", -14f, 16, MutedLabelColor);

            GameObject btnObj = GUIManager.Instance.CreateButton(
                text: "Unstuck safezones",
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -55f),
                width: CardWidth - 40f,
                height: 40f
            );
            btnObj.SetActive(true);
            btnObj.GetComponent<Button>().onClick.AddListener(OnUnstuckSafezones);

            CreateCardText(card, "Use if Safezone appears to be stuck", -100f, 13, DescriptionColor, height: 38f);
        }

        private void BuildSafezoneBoundsCard(Vector2 topCenter)
        {
            GameObject card = CreateCard(topCenter);
            CreateCardText(card, "Show safezone bounds", -14f, 16, MutedLabelColor);

            bool currentValue = PluginConfig.configShowSafeZoneDebug.Value;
            m_safeZoneDebugToggle = GuiFieldBuilder.CreateBoolField(card, new Vector2(-CardWidth / 2f + 26f, -46f), 40f, currentValue, OnSafeZoneDebugToggled);

            // Toggle sits ~56px in from the card's left edge (see leftAlignedX in
            // GuiFieldBuilder.CreateBoolField); the status word picks up right after it - same
            // toggle+status-word shape as HomeTab.cs's BuildToggleRow.
            float statusWidth = CardWidth - 68f;
            float statusCenterX = -CardWidth / 2f + 56f + statusWidth / 2f;
            m_safeZoneDebugStatusText = GUIManager.Instance.CreateText(
                text: currentValue ? "Enabled" : "Disabled",
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(statusCenterX, -50f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: currentValue ? EnabledColor : DisabledColor,
                outline: true,
                outlineColor: Color.black,
                width: statusWidth,
                height: 24f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_safeZoneDebugStatusText.alignment = TextAnchor.MiddleLeft;

            CreateCardText(card, "Shows the bounds of active safezones (ships, wards, traders) in-game as a wireframe outline.", -80f, 13, DescriptionColor, height: 38f);
        }

        private void OnSafeZoneDebugToggled(bool value)
        {
            // Setting .Value fires configShowSafeZoneDebug.SettingChanged, which already calls
            // TwitchSafeZone.RefreshDebugVisuals - no extra plumbing needed here.
            PluginConfig.configShowSafeZoneDebug.Value = value;

            m_safeZoneDebugStatusText.text = value ? "Enabled" : "Disabled";
            m_safeZoneDebugStatusText.color = value ? EnabledColor : DisabledColor;
        }

        private void OnUnstuckSafezones()
        {
            // Clears this client's own stuck safe-zone bookkeeping (entry counter / "in safe
            // zone" flag / HUD) - never touches placed Wards, ships, or trader zones.
            TwitchSafeZone.ForceExitAllZones();
            ToastNotifications.Show("Safezone state reset.");
        }

        // ── low-level card/text helpers (same shape as HomeTab.cs's - kept local rather than
        // shared infra, since each tab's card layout differs enough that HomeTab/SettingsTab
        // already keep their own) ───────────────────────────────────────────────────────────

        private GameObject CreateCard(Vector2 topCenter)
        {
            GameObject card = new GameObject("Card");
            card.transform.SetParent(m_root.transform, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(CardWidth, CardHeight);
            rt.anchoredPosition = topCenter;

            Image bg = card.AddComponent<Image>();
            bg.color = CardBackground;

            return card;
        }

        private static Text CreateCardText(GameObject card, string text, float y, int fontSize, Color color, float height = 24f)
        {
            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, y),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: fontSize,
                color: color,
                outline: true,
                outlineColor: Color.black,
                width: CardWidth - 24f,
                height: height,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            return label;
        }
    }
}
