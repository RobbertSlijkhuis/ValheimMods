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
    /// Home tab content - built directly against RedesignUI.dc.html's structure: originally a 3x2
    /// grid of stat/quick-setting cards (Active profile, Redeem status, Chatting feature, Auto
    /// resolve, Enable on login, Redemption log), now with a 7th "Redeem discount" card on its own
    /// row underneath, plus a "Create a new redeem" button. No login/logout controls here - unlike
    /// GUI_OLD/tabs/HomeTab.cs, that's already handled by <see cref="ShellTopBar"/> (round 2),
    /// matching the mockup's Home tab having none either.
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
        /// Wired up by <see cref="WizshBoneShellGUI"/> right after construction (same "wire a
        /// callback after construction" pattern as <see cref="RedeemsTab.OnCloseRequested"/>) so
        /// "Create a new redeem" can switch to the Redeems tab and open its create wizard without
        /// this class needing a reference to the shell or to <see cref="Tabs.RedeemsTab"/> itself.
        /// </summary>
        public Action OnCreateRedeemRequested;

        /// <summary>
        /// Wired up by <see cref="WizshBoneShellGUI"/> (same pattern as
        /// <see cref="OnCreateRedeemRequested"/>), fired after the history section's Back has
        /// restored the overview grid, so the shell can return to whichever tab the history was
        /// opened from instead of always staying on Home.
        /// </summary>
        public Action OnHistoryBack;

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

        // The discount card sits alone on a 3rd row (below the original 3x2 grid), so the button
        // row's Y has to account for that extra row + gap on top of what the 2-row layout used.
        private const float ButtonRowY = GridTopY - 2f * (CardHeight + RowGap) - CardHeight - 30f;

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
            OnHistoryBack?.Invoke();
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

            // A synced profile's redeems are read-only - only its owner can change and re-sync
            // them. Every Home setting (chatting, auto-resolve, enable-on-login, discount) stays
            // editable; on Home only "Create a new redeem" is locked.
            bool isSynced = ProfileManager.IsSyncedProfile(ProfileManager.ActiveProfile);

            BuildActiveProfileCard(CardTopCenter(0, 0), totalRedeems, isSynced);

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

            BuildDiscountCard(CardTopCenter(2, 0), auth, customRewards);

            GameObject createBtnObj = GuiHelper.CreateButton(
                text: "Create a new redeem",
                parent: m_overviewRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(LeftEdgeX + 100f, ButtonRowY),
                width: 200f,
                height: 40f
            );
            createBtnObj.SetActive(true);
            Button createBtn = createBtnObj.GetComponent<Button>();
            createBtn.onClick.AddListener(() => OnCreateRedeemRequested?.Invoke());
            createBtn.interactable = !isSynced;
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

        private void BuildActiveProfileCard(Vector2 topCenter, int totalRedeems, bool isSynced)
        {
            GameObject card = GuiHelper.CreateCard(m_overviewRoot, topCenter, CardWidth, CardHeight);

            GuiHelper.CreateCardTitle(card, "Active profile", CardWidth - 24f);
            GuiHelper.CreateCardText(card, ProfileManager.ActiveProfile, -54f, 22, GUIManager.Instance.ValheimOrange, CardWidth - 24f, height: 30f);
            GuiHelper.CreateCardDescription(card, $"{totalRedeems} redeems configured" + (isSynced ? " - synced, read-only" : ""), CardWidth - 24f);
        }

        /// <summary>
        /// Percent (0-100) knocked off every redeem's Twitch point cost. Editing the field only
        /// updates ProfileSettingsData.redeemsDiscountPercent (persisted immediately, same as every
        /// other field on this tab) - it does *not* re-push costs to Twitch on its own, since
        /// existing rewards there aren't touched until <see cref="TwitchCustomRewards.SetRewards"/>
        /// runs again. "Apply" is the explicit trigger for that, using the same logged-in +
        /// redeems-enabled guard as ProfilesTab's Reload action, but calling SetRewards directly
        /// rather than reloading from disk, since only the discount - not the redeem list itself -
        /// changed.
        /// </summary>
        private void BuildDiscountCard(Vector2 topCenter, TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            GameObject card = GuiHelper.CreateCard(m_overviewRoot, topCenter, CardWidth, CardHeight);
            GuiHelper.CreateCardTitle(card, "Redeem discount", CardWidth - 24f);

            const float rowY = -56f;
            const float fieldWidth = 70f;
            const float percentWidth = 20f;
            const float gap = 10f;
            const float buttonWidth = 100f;

            // Left-aligned row, matching the title's own left edge (-CardWidth/2 + 12) rather than
            // the field sitting centered in the card the way the first version of this card did.
            float rowLeftEdge = -CardWidth / 2f + 12f;

            float fieldX = rowLeftEdge + fieldWidth / 2f;
            GuiFieldBuilder.CreateIntField(card, new Vector2(fieldX, rowY), fieldWidth,
                ProfileSettingsHelper.Current.redeemsDiscountPercent,
                v =>
                {
                    ProfileSettingsHelper.Current.redeemsDiscountPercent = Mathf.Clamp(v, 0, 100);
                    ProfileSettingsPersistHelper.Persist();
                },
                emptyAsZero: true, maxLength: 3);

            float percentX = rowLeftEdge + fieldWidth + gap + percentWidth / 2f;
            GuiHelper.CreateCardText(card, "%", rowY, 16, GUIManager.Instance.ValheimBeige,
                width: percentWidth, height: GuiFieldBuilder.FieldHeight, x: percentX);

            float buttonLeftEdge = rowLeftEdge + fieldWidth + gap + percentWidth + gap;
            float buttonX = buttonLeftEdge + buttonWidth / 2f;
            // Same height as Redemption log's "View history" (GuiHelper.CreateCardButton's default),
            // so every card's button reads the same size regardless of what else sits in its row.
            const float buttonHeight = 40f;
            GameObject applyBtnObj = GuiHelper.CreateButton(
                text: "Apply",
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                // Rounded to whole pixels - buttonX derives from CardWidth-based divisions that
                // don't come out even, and legacy uGUI Text doesn't pixel-snap (see GuiHelper's
                // CreateCard/CreateCardButton for the same fix); unlike CreateCardText/CreateInputField,
                // the lower-level CreateButton this calls doesn't round internally.
                position: new Vector2(Mathf.Round(buttonX), Mathf.Round(rowY)),
                width: Mathf.Round(buttonWidth),
                height: Mathf.Round(buttonHeight)
            );
            applyBtnObj.SetActive(true);
            applyBtnObj.GetComponent<Button>().onClick.AddListener(() => ApplyDiscount(auth, customRewards));

            GuiHelper.CreateCardDescription(card, "Percent off every redeem's Twitch point cost", CardWidth - 24f, height: 38f);
        }

        private void ApplyDiscount(TwitchAuth auth, TwitchCustomRewards customRewards)
        {
            if (auth == null || customRewards == null || !auth.m_loggedIn || !customRewards.m_enabled)
            {
                ToastNotifications.Show("Log in and enable redeems before applying the discount.", ToastType.Warning);
                return;
            }

            customRewards.SetRewards();
            ToastNotifications.Show("Discount applied to Twitch redeem costs.", ToastType.Success);
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
            // Chatting has no description, so its caption+field rows occupy that slot instead.
            // Captions sit above their field (a label over an input, not underneath it like a
            // caption on a photo) so each column reads top-down: "radius (m)", then the box to type
            // it into.
            const float captionTopY = GuiHelper.CardDescriptionTopY;
            const float captionHeight = 18f;
            const float captionGap = 4f;

            // Jotunn's CreateInputField leaves Unity's default center pivot in place, so without
            // GuiHelper.PivotToTop the fields would sit vertically centered on fieldTopY instead of
            // flush under their caption.
            const float fieldTopY = captionTopY - captionHeight - captionGap;

            CreateFieldCaption(card, "radius (m)", leftX, inputWidth, captionTopY);
            CreateFieldCaption(card, "interval (s)", rightX, inputWidth, captionTopY);

            InputField radiusField = GuiFieldBuilder.CreateFloatField(card, new Vector2(leftX, fieldTopY), inputWidth,
                ProfileSettingsHelper.Current.chattingRadius,
                v => { ProfileSettingsHelper.Current.chattingRadius = v; ProfileSettingsPersistHelper.Persist(); });
            GuiHelper.PivotToTop((RectTransform)radiusField.transform, fieldTopY);

            InputField intervalField = GuiFieldBuilder.CreateFloatField(card, new Vector2(rightX, fieldTopY), inputWidth,
                ProfileSettingsHelper.Current.chattingInterval,
                v => { ProfileSettingsHelper.Current.chattingInterval = v; ProfileSettingsPersistHelper.Persist(); });
            GuiHelper.PivotToTop((RectTransform)intervalField.transform, fieldTopY);
        }

        private static void CreateFieldCaption(GameObject card, string text, float x, float width, float topY)
        {
            Text caption = GUIManager.Instance.CreateText(
                text: text,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                // Rounded to whole pixels - x/width come from CardWidth-derived divisions that
                // don't come out even, and legacy uGUI Text doesn't pixel-snap (see GuiHelper's
                // CreateCard/CreateCardText for the same fix).
                position: new Vector2(Mathf.Round(x), topY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 11,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: Mathf.Round(width),
                height: 18f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            // Left- and top-aligned like a card description (its box also starts at the card's
            // 12px inset and at CardDescriptionTopY), so the caption starts where the description
            // text does and lines up with its field's left edge instead of floating centered.
            caption.alignment = TextAnchor.UpperLeft;
            GuiHelper.PivotToTop(caption.rectTransform, topY);
        }
    }
}
