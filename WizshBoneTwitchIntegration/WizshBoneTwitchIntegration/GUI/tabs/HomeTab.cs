using System;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Home tab content - built directly against RedesignUI.dc.html's structure: a 3x2 grid of
    /// stat/quick-setting cards (Active profile, Redeem status, Chatting feature, Auto resolve,
    /// Enable on login, Redemption log) plus a "Create a new redeem" button. No login/logout
    /// controls here - unlike GUI_OLD/tabs/HomeTab.cs, that's already handled by
    /// <see cref="ShellTopBar"/> (round 2), matching the mockup's Home tab having none either.
    ///
    /// Looks up TwitchAuth/TwitchCustomRewards/TwitchChatting itself via Game.instance.gameObject
    /// rather than having them threaded through <see cref="Create"/> - same "own its dependencies"
    /// approach <see cref="WizshBoneGUI"/>'s own doc comment already calls out for this round.
    /// </summary>
    internal class HomeTab : IShellTabView
    {
        private GameObject m_root;

        /// <summary>
        /// The overview grid (cards + "Create a new redeem") - a sibling of
        /// <see cref="m_historySection"/>'s own root under <see cref="m_root"/>, swapped out for it
        /// via <see cref="ShowHistory"/>/<see cref="ShowOverview"/> the same way
        /// <see cref="Tabs.RedeemsTab"/> swaps its list root for <see cref="RedeemWizard"/>'s.
        /// </summary>
        private GameObject m_overviewRoot;

        private readonly RedeemHistorySection m_historySection = new RedeemHistorySection();

        private readonly ConfirmDialog m_confirmDialog = new ConfirmDialog();

        /// <summary>
        /// Wired up by <see cref="WizshBoneShellGUI"/> right after construction so "Create a new
        /// redeem" can switch tabs without this class needing a reference to the shell itself.
        /// </summary>
        public Action<ShellTab> OnNavigateToTab;

        private static readonly Color MutedLabelColor = new Color(0.66f, 0.62f, 0.53f, 1f);
        private static readonly Color DescriptionColor = new Color(0.54f, 0.5f, 0.44f, 1f);
        private static readonly Color CardBackground = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color EnabledColor = new Color(0.95f, 0.65f, 0.2f, 1f);
        private static readonly Color DisabledColor = new Color(0.6f, 0.56f, 0.49f, 1f);

        // ── layout constants (same derived-from-shell-size pattern as ProfilesTab.cs) ──────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentMargin = 30f;
        private const float LeftEdgeX = -(ContentWidth / 2f) + ContentMargin;

        private const float TitleY = -30f;
        private const float TitleWidth = 300f;

        // CreateCard() top-pivots its cards, so this is their exact top edge - matched to -57 (not
        // -75) so it lines up with Profiles/Redeems/Viewers' toolbar row, whose controls are
        // center-pivoted (Unity's default) at Y=-75, putting their own visual top edge at
        // -75 + FieldHeight/2 = -57.
        private const float GridTopY = -57f;
        private const float CardGap = 14f;
        private const float RowGap = 14f;
        private const float CardWidth = (ContentWidth - 2f * ContentMargin - 2f * CardGap) / 3f;
        private const float CardHeight = 150f;
        private const float ButtonRowY = GridTopY - 2f * CardHeight - RowGap - 30f;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "HomeTab");
            m_overviewRoot = UIContainer.Create(m_root, "Overview", startActive: true);
            m_confirmDialog.Init();

            m_historySection.Create(m_root);
            m_historySection.OnBack = ShowOverview;

            Rebuild();
            return m_root;
        }

        public void Refresh()
        {
            if (m_historySection.IsVisible)
            {
                m_historySection.Hide();
                m_overviewRoot.SetActive(true);
            }

            Rebuild();
        }

        /// <summary>
        /// Hides the overview grid and shows <see cref="m_historySection"/> in its place. Called
        /// both by this tab's own "View history" button and, via
        /// <see cref="WizshBoneShellGUI"/>'s "navigate to Home, then show history" callback, from
        /// other tabs' "Open history" confirm-dialog cancel options.
        /// </summary>
        public void ShowHistory()
        {
            m_overviewRoot.SetActive(false);
            m_historySection.Show();
        }

        /// <summary>
        /// <see cref="RedeemHistorySection.OnBack"/>'s handler - swaps back to the overview grid
        /// and rebuilds its cards, since redeem/history state may have changed while the section
        /// was showing (mirrors <see cref="Tabs.RedeemsTab.OnWizardFinished"/> refreshing its list).
        /// </summary>
        private void ShowOverview()
        {
            m_overviewRoot.SetActive(true);
            Rebuild();
        }

        private void Rebuild()
        {
            GuiHelper.ClearContainer(m_overviewRoot);

            GuiHelper.CreateTitle("Overview", m_overviewRoot, new Vector2(LeftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);

            TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();

            int totalRedeems = RedeemHelper.redeems.Count;
            int enabledRedeems = RedeemHelper.redeems.Count(r => r.enabled);

            BuildActiveProfileCard(CardTopCenter(0, 0), totalRedeems);

            BuildToggleCard(
                CardTopCenter(0, 1), "Redeem status",
                () => customRewards.m_enabled,
                _ => ToggleRedeems(auth, customRewards),
                $"{enabledRedeems} / {totalRedeems} enabled");

            BuildChattingCard(CardTopCenter(0, 2), auth, chatting);

            BuildToggleCard(
                CardTopCenter(1, 0), "Auto resolve redeem",
                () => ProfileSettingsHelper.Current.autoResolveRedeems,
                v => { ProfileSettingsHelper.Current.autoResolveRedeems = v; ProfileSettingsPersistHelper.Persist(); },
                "Whether the redeems are resolved automaticly");

            BuildToggleCard(
                CardTopCenter(1, 1), "Enable redeems on login",
                () => ProfileSettingsHelper.Current.enableRedeemsOnLogin,
                v => { ProfileSettingsHelper.Current.enableRedeemsOnLogin = v; ProfileSettingsPersistHelper.Persist(); },
                "Automatically turns redeems on when you connect");

            BuildRedemptionLogCard(CardTopCenter(1, 2), customRewards);

            GameObject createBtnObj = GUIManager.Instance.CreateButton(
                text: "Create a new redeem",
                parent: m_overviewRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(LeftEdgeX + 100f, ButtonRowY),
                width: 200f,
                height: 40f
            );
            createBtnObj.SetActive(true);
            createBtnObj.GetComponent<Button>().onClick.AddListener(() => OnNavigateToTab?.Invoke(ShellTab.Redeems));
        }

        private static Vector2 CardTopCenter(int row, int col)
        {
            float x = LeftEdgeX + col * (CardWidth + CardGap) + CardWidth / 2f;
            float y = GridTopY - row * (CardHeight + RowGap);
            return new Vector2(x, y);
        }

        private void ToggleRedeems(TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            if (customRewards.HasUnresolvedRedeems())
            {
                m_confirmDialog.Show(
                    title: "Disable Redeems",
                    description: "Auto-resolve is off. Pending redeems won't be refunded automatically. Are you sure?",
                    onConfirm: () => { auth.ToggleRedeems(); Refresh(); },
                    confirmText: "Disable",
                    cancelText: "Open history",
                    onCancel: ShowHistory);
                return;
            }

            auth.ToggleRedeems();
            Refresh();
        }

        // ── card builders ────────────────────────────────────────────────────────────────────

        private void BuildActiveProfileCard(Vector2 topCenter, int totalRedeems)
        {
            GameObject card = CreateCard(topCenter);

            CreateCardText(card, "Active profile", -14f, 16, MutedLabelColor);
            CreateCardText(card, ProfileManager.ActiveProfile, -44f, 22, GUIManager.Instance.ValheimOrange, height: 30f);
            CreateCardText(card, $"{totalRedeems} redeems configured", -74f, 15, DescriptionColor);
        }

        private void BuildRedemptionLogCard(Vector2 topCenter, TwitchCustomRewards customRewards)
        {
            GameObject card = CreateCard(topCenter);

            CreateCardText(card, "Redemption log", -14f, 16, MutedLabelColor);

            const float historyBtnWidth = 140f;
            float historyBtnX = -(CardWidth - 24f) / 2f + historyBtnWidth / 2f;

            GameObject historyBtnObj = GUIManager.Instance.CreateButton(
                text: "View history",
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(historyBtnX, -44f),
                width: historyBtnWidth,
                height: 28f
            );
            historyBtnObj.SetActive(true);
            historyBtnObj.GetComponent<Button>().onClick.AddListener(ShowHistory);

            CreateCardText(card, $"{customRewards.m_redeemHistory.Count} redeems in history", -82f, 15, DescriptionColor);
        }

        /// <summary>
        /// Shared shape for the four toggle-driven cards (Redeem status, Auto resolve, Enable on
        /// login, and the toggle/status portion of Chatting feature) - title, a toggle with a big
        /// colored status word next to it, then a description line.
        /// </summary>
        private void BuildToggleCard(Vector2 topCenter, string title, Func<bool> getValue, Action<bool> onChanged, string description)
        {
            GameObject card = CreateCard(topCenter);
            BuildToggleRow(card, title, getValue, onChanged);
            // Descriptions can wrap onto a second line (e.g. "Whether the redeems are resolved
            // automaticly" is wider than the card) - tall enough for 2 lines at fontSize 13, same
            // fix as the profile-name box: too-short a height drops the whole overflowing line
            // instead of clipping it.
            CreateCardText(card, description, -74f, 13, DescriptionColor, height: 38f);
        }

        /// <summary>
        /// Builds just the title + toggle + status-word row shared by every toggle card, returning
        /// nothing further to add since callers append their own content beneath it.
        /// </summary>
        private void BuildToggleRow(GameObject card, string title, Func<bool> getValue, Action<bool> onChanged)
        {
            CreateCardText(card, title, -14f, 16, MutedLabelColor);

            bool currentValue = getValue();
            GuiFieldBuilder.CreateBoolField(card, new Vector2(-CardWidth / 2f + 26f, -46f), 40f, currentValue, onChanged);

            // Toggle sits ~56px in from the card's left edge (see leftAlignedX in
            // GuiFieldBuilder.CreateBoolField); the status word picks up right after it, sharing
            // the same top-center (0.5, 1) anchor convention every other element on this card uses.
            float statusWidth = CardWidth - 68f;
            float statusCenterX = -CardWidth / 2f + 56f + statusWidth / 2f;
            Text statusText = GUIManager.Instance.CreateText(
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
            statusText.alignment = TextAnchor.MiddleLeft;
        }

        private void BuildChattingCard(Vector2 topCenter, TwitchAuth auth, TwitchChatting chatting)
        {
            GameObject card = CreateCard(topCenter);
            BuildToggleRow(card, "Chatting feature", () => chatting.m_enabled, _ => { auth.ToggleChatting(); Refresh(); });

            float inputWidth = (CardWidth - 24f - 12f) / 2f;
            float leftX = -CardWidth / 2f + 12f + inputWidth / 2f;
            float rightX = leftX + inputWidth + 12f;

            GuiFieldBuilder.CreateFloatField(card, new Vector2(leftX, -80f), inputWidth,
                ProfileSettingsHelper.Current.chattingRadius,
                v => { ProfileSettingsHelper.Current.chattingRadius = v; ProfileSettingsPersistHelper.Persist(); });

            GuiFieldBuilder.CreateFloatField(card, new Vector2(rightX, -80f), inputWidth,
                ProfileSettingsHelper.Current.chattingInterval,
                v => { ProfileSettingsHelper.Current.chattingInterval = v; ProfileSettingsPersistHelper.Persist(); });

            CreateCaptionUnder(card, "radius (m)", leftX, inputWidth);
            CreateCaptionUnder(card, "interval (s)", rightX, inputWidth);
        }

        private static void CreateCaptionUnder(GameObject card, string text, float x, float width)
        {
            Text caption = GUIManager.Instance.CreateText(
                text: text,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(x, -105f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 11,
                color: DescriptionColor,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: 18f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            caption.alignment = TextAnchor.MiddleCenter;
        }

        // ── low-level card/text helpers ─────────────────────────────────────────────────────

        private GameObject CreateCard(Vector2 topCenter)
        {
            GameObject card = new GameObject("Card");
            card.transform.SetParent(m_overviewRoot.transform, false);

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

        /// <summary>
        /// <paramref name="height"/> must comfortably exceed <paramref name="fontSize"/>'s natural
        /// line height - Unity's default vertical overflow (Truncate) doesn't partially clip a
        /// single line that doesn't fully fit its box, it drops the whole line, so a too-short box
        /// renders nothing at all rather than a clipped one. 24px default only suits fontSize up to
        /// ~17; larger text (e.g. the 22pt profile name) must pass a taller height explicitly.
        /// </summary>
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
