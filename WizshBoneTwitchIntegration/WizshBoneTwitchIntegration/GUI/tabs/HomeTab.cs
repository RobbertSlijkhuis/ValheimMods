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

        private static readonly Color EnabledColor = new Color(0.95f, 0.65f, 0.2f, 1f);

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
        private const float CardHeight = 160f;
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
            GameObject card = GuiHelper.CreateCard(m_overviewRoot, topCenter, CardWidth, CardHeight);

            GuiHelper.CreateCardTitle(card, "Active profile", CardWidth - 24f);
            GuiHelper.CreateCardText(card, ProfileManager.ActiveProfile, -54f, 22, GUIManager.Instance.ValheimOrange, CardWidth - 24f, height: 30f);
            GuiHelper.CreateCardDescription(card, $"{totalRedeems} redeems configured", CardWidth - 24f);
        }

        private void BuildRedemptionLogCard(Vector2 topCenter, TwitchCustomRewards customRewards)
        {
            GameObject card = GuiHelper.CreateCard(m_overviewRoot, topCenter, CardWidth, CardHeight);

            GuiHelper.CreateCardTitle(card, "Redemption log", CardWidth - 24f);
            GuiHelper.CreateCardButton(card, "View history", CardWidth, ShowHistory);
            GuiHelper.CreateCardDescription(card, $"{customRewards.m_redeemHistory.Count} redeems in history", CardWidth - 24f);
        }

        /// <summary>
        /// Shared shape for the four toggle-driven cards (Redeem status, Auto resolve, Enable on
        /// login, and the toggle/status portion of Chatting feature) - title, a toggle with a big
        /// colored status word next to it, then a description line.
        /// </summary>
        private void BuildToggleCard(Vector2 topCenter, string title, Func<bool> getValue, Action<bool> onChanged, string description)
        {
            GameObject card = GuiHelper.CreateCard(m_overviewRoot, topCenter, CardWidth, CardHeight);
            BuildToggleRow(card, title, getValue, onChanged);
            GuiHelper.CreateCardDescription(card, description, CardWidth - 24f, height: 38f);
        }

        /// <summary>
        /// Builds just the title + toggle + status-word row shared by every toggle card, returning
        /// nothing further to add since callers append their own content beneath it.
        /// </summary>
        private void BuildToggleRow(GameObject card, string title, Func<bool> getValue, Action<bool> onChanged)
        {
            GuiHelper.CreateCardTitle(card, title, CardWidth - 24f);
            GuiHelper.CreateToggleStatusRow(card, CardWidth, getValue(), onChanged, EnabledColor);
        }

        private void BuildChattingCard(Vector2 topCenter, TwitchAuth auth, TwitchChatting chatting)
        {
            GameObject card = GuiHelper.CreateCard(m_overviewRoot, topCenter, CardWidth, CardHeight);
            BuildToggleRow(card, "Chatting feature", () => chatting.m_enabled, _ => { auth.ToggleChatting(); Refresh(); });

            float inputWidth = (CardWidth - 24f - 12f) / 2f;
            float leftX = -CardWidth / 2f + 12f + inputWidth / 2f;
            float rightX = leftX + inputWidth + 12f;

            // Same fixed top edge every card's description uses (GuiHelper.CardDescriptionTopY) -
            // Chatting has no description, so its input-fields row occupies that slot instead.
            // Jotunn's CreateInputField leaves Unity's default center pivot in place, so without
            // GuiHelper.PivotToTop the fields would sit vertically centered on fieldTopY instead of
            // flush under the toggle row above them.
            const float fieldTopY = GuiHelper.CardDescriptionTopY;

            InputField radiusField = GuiFieldBuilder.CreateFloatField(card, new Vector2(leftX, fieldTopY), inputWidth,
                ProfileSettingsHelper.Current.chattingRadius,
                v => { ProfileSettingsHelper.Current.chattingRadius = v; ProfileSettingsPersistHelper.Persist(); });
            GuiHelper.PivotToTop((RectTransform)radiusField.transform, fieldTopY);

            InputField intervalField = GuiFieldBuilder.CreateFloatField(card, new Vector2(rightX, fieldTopY), inputWidth,
                ProfileSettingsHelper.Current.chattingInterval,
                v => { ProfileSettingsHelper.Current.chattingInterval = v; ProfileSettingsPersistHelper.Persist(); });
            GuiHelper.PivotToTop((RectTransform)intervalField.transform, fieldTopY);

            const float captionGap = 6f;
            float captionTopY = fieldTopY - GuiFieldBuilder.FieldHeight - captionGap;
            CreateCaptionUnder(card, "radius (m)", leftX, inputWidth, captionTopY);
            CreateCaptionUnder(card, "interval (s)", rightX, inputWidth, captionTopY);
        }

        private static void CreateCaptionUnder(GameObject card, string text, float x, float width, float topY)
        {
            Text caption = GUIManager.Instance.CreateText(
                text: text,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(x, topY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 11,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: 18f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            caption.alignment = TextAnchor.MiddleCenter;
            GuiHelper.PivotToTop(caption.rectTransform, topY);
        }
    }
}
