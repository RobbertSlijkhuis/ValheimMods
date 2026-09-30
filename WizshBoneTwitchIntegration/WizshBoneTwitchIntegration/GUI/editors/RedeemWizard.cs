using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The redeem create/edit wizard - built against RedesignUI.dc.html's 3-step flow (choose
    /// effect -> configure parameters -> Twitch reward setup). Create runs all 3 steps; Edit
    /// starts at step 2 since the effect type can't change once a redeem exists (mirrors the
    /// mockup's <c>wizardSteps</c>/<c>isEdit</c> logic and GUI_OLD/tabs/RedeemsTab.cs's
    /// <c>ShowEditView</c> locking its type dropdown).
    ///
    /// Step 2 (per-type parameters) is a placeholder for every type this round - real per-type
    /// fields are future rounds' scope, one type at a time. Step 1 and step 3 are fully wired to
    /// the real backend: <see cref="RedeemManager"/> already validates/persists/pushes to Twitch,
    /// this class just calls into it exactly like GUI_OLD's create view did.
    ///
    /// Owned by <see cref="Tabs.RedeemsTab"/> the same way <see cref="ConfirmDialog"/>/
    /// <see cref="InputDialog"/> are owned by other tabs, except this is an inline content swap
    /// within the tab (list root vs. wizard root), not a floating modal.
    /// </summary>
    internal class RedeemWizard
    {
        /// <summary>
        /// Invoked whenever the wizard closes - with a toast message on a successful save, or
        /// <c>null</c> on Cancel. The owning tab switches back to the
        /// list view either way and shows the toast if one was given.
        /// </summary>
        public Action<string> OnFinished;

        private GameObject m_root;

        private GameObject m_step1Root;
        private GameObject m_step2Root;
        private GameObject m_step3Root;

        private readonly List<Image> m_stepBars = new List<Image>();
        private Text m_stepTitleText;
        private Text m_stepDescriptionText;

        // Step 1
        private readonly SelectorList m_effectList = new SelectorList();
        private readonly SelectorPreviewPanel m_previewPanel = new SelectorPreviewPanel();

        // Step 2
        private Text m_step2PlaceholderText;
        private GameObject m_step2PlaceholderRoot;
        private readonly Dictionary<string, IRedeemStep2Form> m_step2Forms = new Dictionary<string, IRedeemStep2Form>();
        private readonly Dictionary<string, GameObject> m_step2FormRoots = new Dictionary<string, GameObject>();

        // Step 3
        private Image m_bgColorOverlay;
        private InputField m_titleInput;
        private InputField m_prefixInput;
        private InputField m_descriptionInput;
        private InputField m_costInput;
        private InputField m_cooldownInput;
        private SearchableDropdown m_cooldownUnitDropdown;
        private InputField m_maxPerStreamInput;
        private InputField m_maxPerUserPerStreamInput;
        private Toggle m_userInputToggle;
        private Toggle m_ignoreSafezoneToggle;
        private SearchableDropdown m_addConditionDropdown;
        private SearchableDropdown m_removeConditionDropdown;

        // Title row geometry LayoutTitleRow re-derives the prefix/title input sizes from
        private float m_titleRowLeftX;
        private float m_titleRowWidth;

        // True while PopulateStep3Fields/OnCooldownChanged write the cooldown controls
        // themselves, so those writes don't re-enter OnCooldownChanged (and, on populate, don't
        // round a saved cooldown down to a whole number of the shown unit).
        private bool m_syncingCooldown;

        // Nav
        private Button m_backBtn;
        private Button m_cancelBtn;
        private Text m_cancelBtnText;
        private Button m_nextBtn;
        private Text m_nextBtnText;

        private RedeemData m_editingOriginal;
        private RedeemData m_working;
        private bool m_isEdit;

        // View-only mode (a redeem in a synced profile): every step's fields are shown but can't
        // be changed, and nothing can be saved. Done with one CanvasGroup per step root whose
        // `interactable` is switched off - that disables every Selectable underneath (inputs,
        // toggles, buttons, dropdowns, color swatches), including ones step-2 forms build later,
        // while the nav row (a sibling of the step roots) keeps working.
        private bool m_readOnly;
        private CanvasGroup[] m_stepGroups;
        private int m_step;
        private int m_floorStep;

        // ── layout constants (same derived-from-shell-size pattern as ProfilesTab.cs) ──────────
        private const float ContentWidth  = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentHeight = WizshBoneShellGUI.PanelHeight - ShellTopBar.Height;   // 660
        private const float ContentMargin = 30f;

        // The wizard's content sits ContentMargin from the section root's edges, like the other
        // tabs. StepRowWidth is that full content width - the step indicator's card, and
        // everything below it (step 1's list/preview, step 3's cards), spans it.
        private const float StepRowWidth = ContentWidth - 2f * ContentMargin;
        private const float StepRowLeftX = -StepRowWidth / 2f;

        // The step indicator (bars + title + description) sits inside its own card, inset by
        // StepIndicatorPadding on every side, so the bars are narrower than StepRowWidth.
        private const float StepIndicatorPadding = 14f;
        private const int   StepBarCount = 3;
        private const float StepBarGap   = 8f;
        private const float StepBarHeight = 5f;
        private const float StepBarY     = -(ContentMargin + StepIndicatorPadding + StepBarHeight / 2f);
        private const float StepBarWidth = (StepRowWidth - 2f * StepIndicatorPadding - (StepBarCount - 1) * StepBarGap) / StepBarCount;
        private const float StepBarsWidth = StepBarCount * StepBarWidth + (StepBarCount - 1) * StepBarGap;
        private const float StepBarsLeftX = -StepBarsWidth / 2f;

        // Top edge (not center - see the PivotToTop call in BuildStepIndicator) of the step title
        // text, offset below the step bars' own bottom edge by StepTitleGap. Previously this was a
        // center-pivoted -55, whose box top (-42.5) landed ABOVE the bars' own bottom edge (-47.5)
        // - the title text sat flush against, almost overlapping, the bar above it.
        private const float StepTitleGap = 12f;
        private const float StepTitleY   = StepBarY - StepBarHeight / 2f - StepTitleGap;
        private const float StepTitleHeight = 25f;

        // One-line description directly under the step title (top-anchored like the title).
        private const float StepDescriptionGap    = 4f;
        private const float StepDescriptionHeight = 18f;
        private const float StepDescriptionY      = StepTitleY - StepTitleHeight - StepDescriptionGap;

        // The indicator card runs from ContentMargin down to StepIndicatorPadding below the
        // description; the step bodies start a RowGap under it.
        private const float StepIndicatorBottomY = StepDescriptionY - StepDescriptionHeight - StepIndicatorPadding;
        private const float StepIndicatorHeight  = -StepIndicatorBottomY - ContentMargin;
        // internal, not private: step-2 per-type forms start their own content at this same Y.
        internal const float BodyTopY             = StepIndicatorBottomY - RowGap;

        // internal, not private: reused by Step2RowLayout so every step-2 per-type form lines up
        // with step 3's own field width/spacing instead of inventing its own constants.
        internal const float FieldWidth  = 460f;
        private const float RowHeight   = 36f;
        internal const float LabelHeight = 20f;
        // Breathing room between a step-3 field label and the input under it.
        private const float LabelInputGap = 3f;
        internal const float RowGap      = 14f;

        // internal, not private: step 2's own forms use this instead of the narrower FieldWidth
        // above - matches StepRowWidth, the same width the step-indicator/"CONFIGURE PARAMETERS"
        // card already spans, so step 2's fields use the same full width instead of a narrower
        // centered column. Step 3 still uses FieldWidth/Step3Width for its own layout, unchanged.
        internal const float Step2FieldWidth = StepRowWidth;

        // Step 3 spans nearly the full wizard card, not the narrow FieldWidth column steps 1/2 use
        // - RedesignUI.dc.html's step-3 content sits in a `flex:1` column (own 22px padding) inside
        // the wizard's own section, not a fixed-width centered form. It spans the same
        // ContentWidth - 2*ContentMargin as the step-bar row, so the two line up exactly.
        private const float Step3Width   = ContentWidth - 2f * ContentMargin;

        // Step 3's reward-preview card (BuildStep3) - swatch + title/description + points, all in
        // one card matching RedesignUI.dc.html's step-3 layout.
        private const float BoxPadding          = 14f;
        private const float BoxRowGap           = 6f;
        private const float CooldownNumberWidth = 100f;

        // Same 10f-per-side text inset Jotunn's CreateInputField gives every input (text width is
        // width - 20f), so the prefix badge's text sits as far from its border as the title's does.
        private const float PrefixTextPadding = 20f;
        private const float PrefixGap         = 4f;

        // Step 3's limits, conditions and toggles cards (BuildStep3) - same padding as the
        // preview card, with a fixed gap between the two halves of a row.
        // Longest typed count (points, cooldown, limits) - keeps int.Parse from overflowing.
        private const int   CountMaxLength     = 9;
        private const float CardColumnGap      = 20f;

        // Temporarily hides the "Limit per stream" / "Limit per user per stream" card. The card is
        // still built (its fields keep their values and saved limits are left untouched); flip to
        // true to bring it back.
        private const bool  LimitsCardEnabled  = false;

        // The reward card's right-hand column: Points on top, and beneath it the cooldown - a
        // fixed-width number beside a fixed-width unit dropdown. The column is exactly as wide as
        // that pair, so Points spans the same edges.
        private const float CooldownUnitWidth  = 130f;
        private const float PointsColumnWidth  = CooldownNumberWidth + BoxRowGap + CooldownUnitWidth;

        // The conditions card's one-line description (BuildStep3) and the gap under it.
        private const float ConditionDescriptionHeight = 20f;
        private const float ConditionDescriptionGap    = 10f;

        // A toggle's label is centered this far below the toggle's `position.y`. That matches where
        // GuiFieldBuilder.CreateBoolField actually draws the toggle circle's center (~1px below
        // `position.y`, see the toggles card in BuildStep3), so the label is vertically centered on
        // the toggle graphic. (Was 4f, copied from Home's toggle/status-word pair, which left the
        // label ~3px too low here.)
        private const float ToggleLabelYOffset = 1f;

        // The type list runs from BodyTopY down to a RowGap above the Back/Next buttons' top edge,
        // so step 1's body fills the same vertical space step 3's cards do.
        internal const float TypeListWidth  = 220f;
        private const float TypeListHeight = ContentHeight - (BtnY + BtnHeight / 2f) - RowGap + BodyTopY;
        private const float TypeBtnHeight  = 34f;

        // internal, not private: the same vertical span from BodyTopY down to a RowGap above the
        // Back/Next buttons that TypeListHeight already computes for step 1's list - step 2's own
        // per-type forms (and GeneralDamageTabSwitcher's two tab panes) reuse this exact value for
        // their own ScrollableList containers so nothing needs to duplicate the formula.
        internal const float Step2ContentHeight = TypeListHeight;

        // The preview column spans the rest of the same row the type list and step-indicator bars
        // already anchor to (StepRowLeftX/StepRowWidth), not the narrower FieldWidth (that's step
        // 2's per-type *form field* width, a different, intentionally-narrow context) - previously
        // this column was wrongly sized off FieldWidth, leaving most of the row's width unused.
        private const float PreviewGapX   = RowGap;   // same as the vertical gaps between cards
        private const float PreviewWidth  = StepRowWidth - TypeListWidth - PreviewGapX;
        private const float PreviewX      = StepRowLeftX + TypeListWidth + PreviewGapX + PreviewWidth / 2f;

        // Reserved space for a future per-type image/video thumbnail (no actual media yet - see
        // PreviewImagePlaceholder in BuildStep1), sized/positioned per RedesignUI.dc.html's own
        // step-1 preview box. When enabled, the selected effect's title + description sit in a
        // fixed-height card (BoxPadding around the text) at the bottom of the column, ending level
        // with the type list, and the image placeholder takes all the height above it.
        //
        // Enabled as a flat placeholder with a "coming in a future update" message - no media
        // exists yet. Disabling it again is safe: the text card then takes over the full column
        // (PreviewCardTopY/PreviewCardHeightActual) instead of just the bottom slice, so the
        // preview isn't left with dead space above it.
        private const bool EffectPreviewImageEnabled = true;
        private const float PreviewCardHeight  = 156f;
        private const float PreviewImageGap    = 14f; // matches RowGap's existing section-gap value
        private const float PreviewImageHeight = TypeListHeight - PreviewCardHeight - PreviewImageGap;
        private const float PreviewLabelTopY   = BodyTopY - PreviewImageHeight - PreviewImageGap;

        private const float PreviewCardTopY = EffectPreviewImageEnabled ? PreviewLabelTopY : BodyTopY;
        private const float PreviewCardHeightActual = EffectPreviewImageEnabled ? PreviewCardHeight : TypeListHeight;

        // BtnY is the buttons' center measured up from the root's bottom edge: the section's bottom
        // margin plus half the button height, so the buttons' lower edge sits ContentMargin above
        // the root's - see RedeemHistorySection.ActionRowY.
        private const float BtnWidth  = 160f;
        private const float BtnHeight = 44f;
        private const float BtnY      = ContentMargin + BtnHeight / 2f;
        // Back and Next sit at the content row's left/right edges (level with the step indicator
        // and cards above); Cancel is always centered between them.
        private const float BackBtnX   = StepRowLeftX + BtnWidth / 2f;
        private const float CancelBtnX = 0f;
        private const float NextBtnX   = -BackBtnX;

        /// <summary>
        /// Builds the wizard's root as a child of <paramref name="parent"/>, hidden until
        /// <see cref="OpenCreate"/>/<see cref="OpenEdit"/>. Called once by
        /// <see cref="Tabs.RedeemsTab.Create"/>.
        /// </summary>
        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "RedeemWizard");

            BuildStepIndicator();
            BuildStep1();
            BuildStep2();
            BuildStep3();
            BuildNavRow();

            m_stepGroups = new[]
            {
                m_step1Root.AddComponent<CanvasGroup>(),
                m_step2Root.AddComponent<CanvasGroup>(),
                m_step3Root.AddComponent<CanvasGroup>(),
            };

            m_root.SetActive(false);
            return m_root;
        }

        public void OpenCreate()
        {
            SetReadOnly(false);
            m_editingOriginal = null;
            m_working = new RedeemData();
            m_isEdit = false;
            m_floorStep = 1;

            RefreshStep1Selection();
            SetActiveStep(1);
            m_root.SetActive(true);
        }

        public void OpenEdit(RedeemData redeem)
        {
            OpenExisting(redeem, readOnly: false);
        }

        /// <summary>
        /// Opens <paramref name="redeem"/> like <see cref="OpenEdit"/>, but view-only: every field
        /// is visible and can be browsed (Back/Next, scrolling), yet nothing can be changed or
        /// saved. Used for redeems in a synced profile.
        /// </summary>
        public void OpenView(RedeemData redeem)
        {
            OpenExisting(redeem, readOnly: true);
        }

        public bool IsOpen => m_root != null && m_root.activeSelf;

        /// <summary>
        /// Closes the wizard without saving (same as Cancel) if it is open - for when the data it
        /// was working on is replaced underneath it, e.g. by a profile sync.
        /// </summary>
        public void ForceClose()
        {
            if (IsOpen)
                Close(null);
        }

        private void SetReadOnly(bool readOnly)
        {
            m_readOnly = readOnly;
            foreach (CanvasGroup group in m_stepGroups)
                group.interactable = !readOnly;
        }

        private void OpenExisting(RedeemData redeem, bool readOnly)
        {
            SetReadOnly(readOnly);
            m_editingOriginal = redeem;
            m_working = redeem.DeepClone<RedeemData>();
            m_isEdit = true;
            m_floorStep = 2;

            // Normalize now, not just on display: a redeem created via GUI_OLD (or before this
            // wizard stopped baking the prefix in) still has it baked into m_working.title at this
            // point. Without this, saving without ever touching the title field would silently
            // keep the old baked-in value instead of moving it to the prefix-free storage this
            // wizard uses everywhere else.
            m_working.title = StripKnownPrefix(m_working.title);

            RefreshStep1Selection();
            SetActiveStep(2);
            m_root.SetActive(true);
        }

        private void Close(string toastMessage)
        {
            // A color swatch on step 2 (Flashbang's flash color, a spawned creature's color, ...)
            // may have a picker open - Jötunn's ColorPicker is a scene-wide singleton, not a child
            // of this wizard, so hiding m_root doesn't close it and it's otherwise left open,
            // floating over whatever GUI shows next.
            GuiHelper.CloseOpenColorPicker();
            m_root.SetActive(false);
            OnFinished?.Invoke(toastMessage);
        }

        // ── step indicator / title ──────────────────────────────────────────

        private void BuildStepIndicator()
        {
            GuiHelper.CreateCard(m_root, new Vector2(0f, -ContentMargin), StepRowWidth, StepIndicatorHeight);

            float startX = StepBarsLeftX + StepBarWidth / 2f;

            for (int i = 0; i < StepBarCount; i++)
            {
                GameObject bar = new GameObject($"StepBar{i + 1}");
                bar.transform.SetParent(m_root.transform, false);

                RectTransform rt = bar.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(StepBarWidth, StepBarHeight);
                rt.anchoredPosition = new Vector2(startX + i * (StepBarWidth + StepBarGap), StepBarY);

                Image img = bar.AddComponent<Image>();
                m_stepBars.Add(img);
            }

            m_stepTitleText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, StepTitleY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiHelper.TitleFontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: ContentWidth - 2f * ContentMargin,
                height: StepTitleHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_stepTitleText.alignment = TextAnchor.MiddleCenter;
            GuiHelper.PivotToTop(m_stepTitleText.rectTransform, StepTitleY);

            m_stepDescriptionText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, StepDescriptionY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: ContentWidth - 2f * ContentMargin,
                height: StepDescriptionHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_stepDescriptionText.alignment = TextAnchor.MiddleCenter;
            GuiHelper.PivotToTop(m_stepDescriptionText.rectTransform, StepDescriptionY);
        }

        private void RefreshStepIndicator()
        {
            int[] steps = m_isEdit ? new[] { 2, 3 } : new[] { 1, 2, 3 };
            for (int i = 0; i < m_stepBars.Count; i++)
            {
                int stepNumber = i < steps.Length ? steps[i] : -1;
                bool visible = stepNumber >= 0;
                m_stepBars[i].gameObject.SetActive(visible);
                if (visible)
                    m_stepBars[i].color = stepNumber <= m_step ? GUIManager.Instance.ValheimOrange : new Color(1f, 1f, 1f, 0.15f);
            }

            bool hasType = !string.IsNullOrEmpty(m_working?.type) && m_working.type != RedeemType.Undefined;
            string step2Title = hasType ? RedeemEffectCatalog.LabelFor(m_working.type).ToUpperInvariant() : "CONFIGURE PARAMETERS";
            string[] titles = { "", "CHOOSE AN EFFECT", step2Title, "SETUP CHANNEL POINT REWARD" };
            string[] descriptions =
            {
                "",
                "Pick what happens in the game when a viewer redeems this reward.",
                "Adjust how the chosen effect behaves.",
                "Set how the reward looks on Twitch and how often it can be redeemed."
            };
            bool validStep = m_step >= 1 && m_step <= 3;
            m_stepTitleText.text = validStep ? titles[m_step] : "";
            m_stepDescriptionText.text = validStep ? (m_readOnly ? "View only (synced profile) - " : "") + descriptions[m_step] : "";
        }

        // ── step 1: choose effect ────────────────────────────────────────────

        private void BuildStep1()
        {
            m_step1Root = UIContainer.Create(m_root, "Step1");

            GameObject listContent = ScrollableList.CreateFixed(
                m_step1Root, "EffectTypeList",
                anchoredPosition: new Vector2(StepRowLeftX + TypeListWidth / 2f, BodyTopY),
                width: TypeListWidth, height: TypeListHeight,
                backgroundColor: GuiHelper.CardBackgroundColor);

            var items = new List<(string Key, string Label)>();
            foreach (RedeemEffectInfo info in RedeemEffectCatalog.All)
                items.Add((info.Type, info.Label));

            float listHeight = m_effectList.Build(
                listContent.transform, items, TypeListWidth - ScrollableList.ScrollbarWidth - 8f, TypeBtnHeight, 4f,
                onClick: type =>
                {
                    // Door/Windmill/Smite/Rain/Meteor/Trap/Root/LogRain all share one
                    // SpawnAbilityData, so switching between them would otherwise carry the previous
                    // type's spawns/isBiomeList/etc. into the new form (e.g. a LogRain log showing
                    // up in the Door prefab list). Start the new type from a clean slate instead.
                    bool typeChanged = m_working.type != type;
                    if (typeChanged
                        && RedeemType.SpawnAbilityFamily.Contains(m_working.type)
                        && RedeemType.SpawnAbilityFamily.Contains(type))
                        m_working.spawnAbilityData = new SpawnAbilityData();

                    m_working.type = type;
                    if (typeChanged && m_step2Forms.TryGetValue(type, out IRedeemStep2Form selectedForm)
                        && selectedForm is IAppliesDefaultsOnSelect withDefaults)
                        withDefaults.ApplyDefaults(m_working);
                    RefreshStep1Selection();
                    RefreshNavButtons();
                });
            ScrollableList.SetContentHeight(listContent, listHeight);

            if (EffectPreviewImageEnabled)
            {
                // Reserved space for a future per-type image/video thumbnail - no media exists yet,
                // so this is just a flat placeholder in the scrollbar track's own dark shade.
                GameObject previewImagePlaceholder = new GameObject("PreviewImagePlaceholder");
                previewImagePlaceholder.transform.SetParent(m_step1Root.transform, false);

                RectTransform previewImageRt = previewImagePlaceholder.AddComponent<RectTransform>();
                previewImageRt.anchorMin = new Vector2(0.5f, 1f);
                previewImageRt.anchorMax = new Vector2(0.5f, 1f);
                previewImageRt.pivot = new Vector2(0.5f, 1f);
                previewImageRt.sizeDelta = new Vector2(PreviewWidth, PreviewImageHeight);
                previewImageRt.anchoredPosition = new Vector2(PreviewX, BodyTopY);

                Image previewImageBg = previewImagePlaceholder.AddComponent<Image>();
                // Fully opaque here, unlike ScrollableList.TrackColor's own alpha - that field stays
                // translucent for the real scrollbar track, this placeholder just reuses its RGB.
                Color trackColor = ScrollableList.TrackColor;
                previewImageBg.color = new Color(trackColor.r, trackColor.g, trackColor.b, 1f);

                Text previewImageText = GUIManager.Instance.CreateText(
                    text: "Preview video's coming in future update!",
                    parent: previewImagePlaceholder.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: Vector2.zero,
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: PreviewWidth - 2f * BoxPadding,
                    height: 60f,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                previewImageText.alignment = TextAnchor.MiddleCenter;
                previewImageText.horizontalOverflow = HorizontalWrapMode.Wrap;
                previewImageText.raycastTarget = false;
            }

            m_previewPanel.Build(m_step1Root, new Vector2(PreviewX, PreviewCardTopY), PreviewWidth, PreviewCardHeightActual, BoxPadding);
        }

        private void RefreshStep1Selection()
        {
            bool hasType = !string.IsNullOrEmpty(m_working?.type) && m_working.type != RedeemType.Undefined;

            m_effectList.Select(hasType ? m_working.type : null);

            m_previewPanel.UpdateContent(
                hasType ? RedeemEffectCatalog.LabelFor(m_working.type) : "Choose an effect from the list",
                hasType ? RedeemEffectCatalog.DescriptionFor(m_working.type) : "Select an effect on the left to see what it does in-game.");
        }

        // ── step 2: configure parameters ────────────────────────────────────
        //
        // One sub-root + one IRedeemStep2Form per real type, all built once up front here
        // (mirroring how m_step1Root/m_step2Root/m_step3Root are themselves all built once in
        // Create() and toggled via SetActive) and dispatched to by RefreshStep2Content based on
        // m_working.type. The bare SpawnAbility type (removed from RedeemEffectCatalog entirely)
        // has no registered form, so it keeps falling through to the original inert placeholder text.

        private void BuildStep2()
        {
            m_step2Root = UIContainer.Create(m_root, "Step2");

            m_step2PlaceholderRoot = UIContainer.Create(m_step2Root, "Placeholder");
            m_step2PlaceholderText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_step2PlaceholderRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, BodyTopY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 15,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: Step2FieldWidth,
                height: 60f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_step2PlaceholderText.alignment = TextAnchor.UpperCenter;
            m_step2PlaceholderText.horizontalOverflow = HorizontalWrapMode.Wrap;
            // Same center-pivot fix SelectorPreviewPanel applies to its own title/description in
            // BuildStep1 - without it, this box's top half sits above BodyTopY instead of starting
            // there, putting it noticeably higher/tighter under the step bar than step 1's
            // (genuinely top-pivoted) content does.
            GuiHelper.PivotToTop(m_step2PlaceholderText.rectTransform, BodyTopY);

            RegisterStep2Form(RedeemType.Flashbang, new FlashbangForm());
            RegisterStep2Form(RedeemType.Mist, new MistForm());
            RegisterStep2Form(RedeemType.TimeStop, new TimeStopForm());
            RegisterStep2Form(RedeemType.Weather, new WeatherForm());
            RegisterStep2Form(RedeemType.TerrainEdit, new TerrainEditForm());
            RegisterStep2Form(RedeemType.Detonate, new DetonateForm());
            RegisterStep2Form(RedeemType.Door, new DoorForm());
            RegisterStep2Form(RedeemType.Windmill, new WindmillForm());
            RegisterStep2Form(RedeemType.Smite, new SmiteForm());
            RegisterStep2Form(RedeemType.Rain, new RainForm());
            RegisterStep2Form(RedeemType.Meteor, new MeteorForm());
            RegisterStep2Form(RedeemType.Trap, new TrapForm());
            RegisterStep2Form(RedeemType.Root, new RootForm());
            RegisterStep2Form(RedeemType.LogRain, new LogRainForm());
            RegisterStep2Form(RedeemType.BoatRain, new BoatRainForm());
            RegisterStep2Form(RedeemType.SpawnCreature, new SpawnCreatureForm());
            RegisterStep2Form(RedeemType.StatusEffect, new StatusEffectForm());
            RegisterStep2Form(RedeemType.SurpriseChest, new SurpriseChestForm());
        }

        private void RegisterStep2Form(string type, IRedeemStep2Form form)
        {
            GameObject formRoot = UIContainer.Create(m_step2Root, type + "Form");
            form.Build(formRoot);
            m_step2Forms[type] = form;
            m_step2FormRoots[type] = formRoot;
        }

        private void RefreshStep2Content()
        {
            string type = m_working.type;
            bool hasForm = m_step2Forms.TryGetValue(type, out IRedeemStep2Form form);

            m_step2PlaceholderRoot.SetActive(!hasForm);
            if (!hasForm)
            {
                string label = RedeemEffectCatalog.LabelFor(type);
                m_step2PlaceholderText.text = $"{label} has no extra parameters yet - continue to set up the reward.\n(Per-effect configuration is coming in a future update.)";
            }

            foreach (var kvp in m_step2FormRoots)
                kvp.Value.SetActive(hasForm && kvp.Key == type);

            if (hasForm)
                form.Populate(m_working);
        }

        // ── step 3: Twitch reward setup ──────────────────────────────────────

        private void BuildStep3()
        {
            m_step3Root = UIContainer.Create(m_root, "Step3");

            float y = BodyTopY;

            // Reward preview card - color swatch + title/description column + points/cooldown
            // column, all in one dark card, matching RedesignUI.dc.html's step-3 "live preview
            // tile". Two label-over-input rows: "Title" / "Points" on top, "Description" /
            // "Cooldown" beneath, with the swatch spanning both rows. Same label style as the
            // cards below; the first input row starts under the label row at contentTopY.
            float labelRowY = -BoxPadding;
            float contentTopY = labelRowY - LabelHeight - LabelInputGap;
            float secondLabelRowY = contentTopY - RowHeight - BoxRowGap;
            float secondContentTopY = secondLabelRowY - LabelHeight - LabelInputGap;
            float boxContentHeight = 2f * RowHeight + BoxRowGap + LabelHeight + LabelInputGap;
            float boxHeight = 2f * BoxPadding + LabelHeight + LabelInputGap + boxContentHeight;
            GameObject rewardBox = GuiHelper.CreateCard(m_step3Root, new Vector2(0f, y), Step3Width, boxHeight);

            // Swatch - PopulateStep3Fields keeps its overlay in sync with m_working.backgroundColor
            // on every step-3 entry (the picker's own onChanged callback keeps m_working in sync
            // the other way).
            // Square sized to span both input rows (title input down to the description input,
            // including the second row's label), so its top/bottom edges line up with them.
            float swatchSize = boxContentHeight;
            float swatchX = -Step3Width / 2f + BoxPadding + swatchSize / 2f;
            float swatchY = contentTopY - swatchSize / 2f;
            GameObject bgSwatch = GuiFieldBuilder.CreateColorField(rewardBox, new Vector2(swatchX, swatchY), swatchSize, "#a970ff", "Redeem background color", v => m_working.backgroundColor = v, swatchSize);
            m_bgColorOverlay = bgSwatch.transform.Find("ColorOverlay").GetComponent<Image>();

            // Right column - flush with the box's right edge: points on the title row, cooldown
            // (number + unit dropdown) on the description row.
            float pointsX = Step3Width / 2f - BoxPadding - PointsColumnWidth / 2f;
            float pointsInputY = contentTopY - RowHeight / 2f;
            m_costInput = GuiFieldBuilder.CreateIntField(rewardBox, new Vector2(pointsX, pointsInputY), PointsColumnWidth, 0, v => m_working.points = Mathf.Max(0, v), emptyAsZero: true, maxLength: CountMaxLength);

            // Middle column: title row (prefix badge + input), description row beneath it.
            float middleLeftX = swatchX + swatchSize / 2f + BoxPadding;
            float middleRightX = pointsX - PointsColumnWidth / 2f - BoxPadding;
            float middleWidth = middleRightX - middleLeftX;
            float middleCenterX = (middleLeftX + middleRightX) / 2f;

            CreateRowLabel(rewardBox, "Color", labelRowY, swatchSize, swatchX);
            CreateRowLabel(rewardBox, "Title", labelRowY, middleWidth, middleCenterX);
            CreateRowLabel(rewardBox, "Points", labelRowY, PointsColumnWidth, pointsX);
            CreateRowLabel(rewardBox, "Description", secondLabelRowY, middleWidth, middleCenterX);
            CreateRowLabel(rewardBox, "Cooldown", secondLabelRowY, PointsColumnWidth, pointsX);

            float titleRowY = contentTopY - RowHeight / 2f;
            m_titleRowLeftX = middleLeftX;
            m_titleRowWidth = middleWidth;

            // Prefix: a read-only input sitting directly in front of the title input. Both are
            // created at placeholder sizes here and sized for real by LayoutTitleRow, which also
            // re-runs whenever step 3 is entered (the prefix can differ per profile).
            // readOnly (not interactable = false) so it keeps the normal, undimmed input colors;
            // its graphics stop being raycast targets so it can't be clicked or focused either.
            m_prefixInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(middleLeftX, titleRowY), 100f);
            m_prefixInput.readOnly = true;
            foreach (Graphic graphic in m_prefixInput.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
            m_prefixInput.textComponent.alignment = TextAnchor.MiddleCenter;

            m_titleInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(middleCenterX, titleRowY), middleWidth, placeholderText: "Reward title");
            m_titleInput.onValueChanged.AddListener(OnTitleInputChanged);
            LayoutTitleRow();

            float descriptionRowY = secondContentTopY - RowHeight / 2f;
            m_descriptionInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(middleCenterX, descriptionRowY), middleWidth, placeholderText: "Description shown to viewers when they open the reward");
            m_descriptionInput.onValueChanged.AddListener(v => m_working.description = v);

            // Cooldown: number on the left, unit dropdown on the right, sharing the points column.
            float cooldownNumberX = pointsX - PointsColumnWidth / 2f + CooldownNumberWidth / 2f;
            float cooldownUnitX = pointsX + PointsColumnWidth / 2f - CooldownUnitWidth / 2f;

            m_cooldownInput = GuiFieldBuilder.CreateIntField(rewardBox, new Vector2(cooldownNumberX, secondContentTopY), CooldownNumberWidth, 0, _ => OnCooldownChanged(), emptyAsZero: true, maxLength: CountMaxLength);
            GuiHelper.PivotToTop((RectTransform)m_cooldownInput.transform, secondContentTopY);

            m_cooldownUnitDropdown = new SearchableDropdown();
            GameObject cooldownUnitToggle = m_cooldownUnitDropdown.Build(rewardBox, new Vector2(cooldownUnitX, secondContentTopY), CooldownUnitWidth, GuiFieldBuilder.FieldHeight, GetCooldownUnitOptions(), CooldownHelper.Seconds, showSearch: false);
            GuiHelper.PivotToTop((RectTransform)cooldownUnitToggle.transform, secondContentTopY);
            m_cooldownUnitDropdown.OnValueChanged += _ => OnCooldownChanged();

            y -= boxHeight + RowGap;

            // The limits, conditions and toggles cards below share the same two-column grid inside their own BoxPadding inset.
            float halfWidth = (Step3Width - 2f * BoxPadding - CardColumnGap) / 2f;
            float leftHalfX = -Step3Width / 2f + BoxPadding + halfWidth / 2f;
            float rightHalfX = Step3Width / 2f - BoxPadding - halfWidth / 2f;

            // Limits card: limit per stream and limit per user per stream, one per column. 0/empty
            // means "off" for both (SetRewards only enables the Twitch-side flag for values above 0).
            float limitsCardHeight = 2f * BoxPadding + LabelHeight + LabelInputGap + RowHeight;
            GameObject limitsCard = GuiHelper.CreateCard(m_step3Root, new Vector2(0f, y), Step3Width, limitsCardHeight);

            float limitsRowY = -BoxPadding;
            CreateRowLabel(limitsCard, "Limit per stream", limitsRowY, halfWidth, leftHalfX);
            CreateRowLabel(limitsCard, "Limit per user per stream", limitsRowY, halfWidth, rightHalfX);
            float limitsInputY = limitsRowY - LabelHeight - LabelInputGap;
            m_maxPerStreamInput = GuiFieldBuilder.CreateIntField(limitsCard, new Vector2(leftHalfX, limitsInputY), halfWidth, 0, v => m_working.maxPerStream = Mathf.Max(0, v), emptyAsZero: true, maxLength: CountMaxLength);
            GuiHelper.PivotToTop((RectTransform)m_maxPerStreamInput.transform, limitsInputY);
            m_maxPerUserPerStreamInput = GuiFieldBuilder.CreateIntField(limitsCard, new Vector2(rightHalfX, limitsInputY), halfWidth, 0, v => m_working.maxPerUserPerStream = Mathf.Max(0, v), emptyAsZero: true, maxLength: CountMaxLength);
            GuiHelper.PivotToTop((RectTransform)m_maxPerUserPerStreamInput.transform, limitsInputY);

            // Hidden for now (see LimitsCardEnabled) - still built so its fields and
            // PopulateStep3Fields keep working, but the cards below close up the gap.
            limitsCard.SetActive(LimitsCardEnabled);
            if (LimitsCardEnabled)
                y -= limitsCardHeight + RowGap;

            // Conditions card: a one-line description on top, then the add/remove condition
            // dropdowns. Single line only - the card's height is fixed, and a description that
            // wrapped would be dropped entirely by Unity's vertical-truncate on a too-short box.
            float conditionCardHeight = 2f * BoxPadding + ConditionDescriptionHeight + ConditionDescriptionGap + LabelHeight + LabelInputGap + RowHeight;
            GameObject conditionCard = GuiHelper.CreateCard(m_step3Root, new Vector2(0f, y), Step3Width, conditionCardHeight);

            GuiHelper.CreateCardDescription(
                conditionCard,
                "Offer this redeem only once the add key is set, and hide it once the remove key is set. Both are optional.",
                -BoxPadding, Step3Width - 2f * BoxPadding, ConditionDescriptionHeight);

            float conditionRowY = -(BoxPadding + ConditionDescriptionHeight + ConditionDescriptionGap);

            // Add / remove condition - dropdowns over Valheim's global keys, matching GUI_OLD's
            // FieldUIBuilder.BuildStringDropdownField wiring: OnValueChanged writes straight into
            // m_working, and PopulateStep3Fields re-syncs the displayed value/options every time
            // step 3 is (re-)entered.
            CreateRowLabel(conditionCard, "Add condition", conditionRowY, halfWidth, leftHalfX);
            CreateRowLabel(conditionCard, "Remove condition", conditionRowY, halfWidth, rightHalfX);
            m_addConditionDropdown = new SearchableDropdown();
            float conditionInputY = conditionRowY - LabelHeight - LabelInputGap;
            GameObject addConditionToggle = m_addConditionDropdown.Build(conditionCard, new Vector2(leftHalfX, conditionInputY), halfWidth, GuiFieldBuilder.FieldHeight, GetAvailableGlobalKeyOptions(), "");
            GuiHelper.PivotToTop((RectTransform)addConditionToggle.transform, conditionInputY);
            m_addConditionDropdown.OnValueChanged += v => m_working.globalKeyAdd = v;
            m_removeConditionDropdown = new SearchableDropdown();
            GameObject removeConditionToggle = m_removeConditionDropdown.Build(conditionCard, new Vector2(rightHalfX, conditionInputY), halfWidth, GuiFieldBuilder.FieldHeight, GetAvailableGlobalKeyOptions(), "");
            GuiHelper.PivotToTop((RectTransform)removeConditionToggle.transform, conditionInputY);
            m_removeConditionDropdown.OnValueChanged += v => m_working.globalKeyRemove = v;

            y -= conditionCardHeight + RowGap;

            // Toggles card: the two toggles, one per column (no label above them, so the row
            // starts at the card's top padding).
            float toggleCardHeight = 2f * BoxPadding + RowHeight;
            GameObject toggleCard = GuiHelper.CreateCard(m_step3Root, new Vector2(0f, y), Step3Width, toggleCardHeight);

            // GuiFieldBuilder.CreateBoolField's `position.y` is NOT the toggle graphic's center: the
            // circle's center ends up ~1px below it (root at y - FieldHeight/4, circle 8px above
            // the root's center). So pass the row's center + 1 to put the circle - and the label
            // beside it - in the vertical middle of the row (BoxPadding above and below).
            float toggleRowY = -(BoxPadding + RowHeight / 2f) + 1f;
            m_userInputToggle = GuiFieldBuilder.CreateBoolField(toggleCard, new Vector2(leftHalfX, toggleRowY), halfWidth, false, v => m_working.userInput = v);
            CreateInlineToggleLabel(toggleCard, "Requires viewer input", leftHalfX, halfWidth, toggleRowY);
            m_ignoreSafezoneToggle = GuiFieldBuilder.CreateBoolField(toggleCard, new Vector2(rightHalfX, toggleRowY), halfWidth, false, v => m_working.ignoreWard = v);
            CreateInlineToggleLabel(toggleCard, "Ignore safezone", rightHalfX, halfWidth, toggleRowY);
        }

        /// <summary>
        /// Sizes the prefix badge to the current profile's actual prefix (measured with the
        /// input's own font, plus <see cref="PrefixTextPadding"/>) and gives the title input the
        /// rest of the middle column. Re-run on every step-3 entry because the prefix can differ
        /// per profile; an empty prefix hides the badge and its gap entirely.
        /// </summary>
        private void LayoutTitleRow()
        {
            string prefix = ProfileSettingsHelper.Current.redeemTitlePrefix ?? "";
            bool hasPrefix = prefix.Length > 0;
            m_prefixInput.gameObject.SetActive(hasPrefix);

            float prefixWidth = 0f;
            float gap = 0f;
            if (hasPrefix)
            {
                Text prefixText = m_prefixInput.textComponent;
                float textWidth = prefixText.cachedTextGenerator.GetPreferredWidth(prefix, prefixText.GetGenerationSettings(Vector2.zero)) / prefixText.pixelsPerUnit;
                prefixWidth = Mathf.Round(Mathf.Ceil(textWidth) + PrefixTextPadding);
                gap = PrefixGap;

                RectTransform prefixRt = (RectTransform)m_prefixInput.transform;
                prefixRt.sizeDelta = new Vector2(prefixWidth, prefixRt.sizeDelta.y);
                prefixRt.anchoredPosition = new Vector2(Mathf.Round(m_titleRowLeftX + prefixWidth / 2f), prefixRt.anchoredPosition.y);
            }

            float titleWidth = Mathf.Round(m_titleRowWidth - prefixWidth - gap);
            RectTransform titleRt = (RectTransform)m_titleInput.transform;
            titleRt.sizeDelta = new Vector2(titleWidth, titleRt.sizeDelta.y);
            titleRt.anchoredPosition = new Vector2(Mathf.Round(m_titleRowLeftX + prefixWidth + gap + titleWidth / 2f), titleRt.anchoredPosition.y);
        }

        private static int ParseCount(string text)
        {
            return int.TryParse(text, out int value) ? Mathf.Max(0, value) : 0;
        }

        private static List<DropdownOption> GetCooldownUnitOptions()
        {
            return CooldownHelper.Units.Select(u => new DropdownOption(u, CooldownHelper.Label(u))).ToList();
        }

        /// <summary>
        /// Recomputes <see cref="RedeemData.cooldown"/> (seconds) from the shown number and unit.
        /// If that exceeds the cap the field snaps back to the largest allowed number for the
        /// unit, so the user sees the clamp.
        /// </summary>
        private void OnCooldownChanged()
        {
            if (m_syncingCooldown)
                return;

            string unit = m_cooldownUnitDropdown.Value;
            int value = ParseCount(m_cooldownInput.text);
            int seconds = CooldownHelper.ToSeconds(value, unit);

            m_working.cooldown = seconds;
            m_working.cooldownUnit = unit;

            int shown = CooldownHelper.FromSeconds(seconds, unit);
            if (shown != value)
            {
                m_syncingCooldown = true;
                m_cooldownInput.text = shown.ToString();
                m_syncingCooldown = false;
            }
        }

        /// <summary>
        /// Value = the global key string ZoneSystem stores (what gets saved), Label = a readable
        /// name matching vanilla boss naming. A small fixed list, not a runtime scan, so no
        /// caching is needed. Ported from GUI_OLD/editors/FieldUIBuilder.cs's method of the same
        /// name - includes a leading "" -> "None" entry, deliberately excludes
        /// <see cref="GlobalKeyType.DefeatedNothing"/>/<see cref="GlobalKeyType.DefeatedNoBoss"/>
        /// (sentinels never read by ZoneSystem, not real global keys).
        /// </summary>
        private static List<DropdownOption> GetAvailableGlobalKeyOptions()
        {
            return new List<DropdownOption>
            {
                new DropdownOption("", "None"),
                new DropdownOption(GlobalKeyType.DefeatedEikthyr, "Defeated Eikthyr"),
                new DropdownOption(GlobalKeyType.DefeatedElder, "Defeated the Elder"),
                new DropdownOption(GlobalKeyType.DefeatedBonemass, "Defeated Bonemass"),
                new DropdownOption(GlobalKeyType.DefeatedModer, "Defeated Moder"),
                new DropdownOption(GlobalKeyType.DefeatedYagluth, "Defeated Yagluth"),
                new DropdownOption(GlobalKeyType.DefeatedQueen, "Defeated the Queen"),
                new DropdownOption(GlobalKeyType.DefeatedFader, "Defeated Fader"),
                new DropdownOption(GlobalKeyType.KilledBat, "Killed a Bat"),
                new DropdownOption(GlobalKeyType.KilledTroll, "Killed a Troll"),
                new DropdownOption(GlobalKeyType.killedSurtling, "Killed a Surtling"),
            };
        }

        /// <summary>
        /// Returns <paramref name="options"/> with <paramref name="currentValue"/> added in
        /// (sorted) if it's missing - so a saved value set outside this curated list (e.g. via
        /// GUI_OLD or a hand-edited profile.yaml) still displays correctly instead of silently
        /// reverting to "None". Ported from GUI_OLD/editors/FieldUIBuilder.cs's method of the same
        /// name.
        /// </summary>
        private static List<DropdownOption> EnsureIncludesCurrentValue(List<DropdownOption> options, string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue) || options.Any(o => o.Value == currentValue))
                return options;

            var withCurrent = new List<DropdownOption>(options) { new DropdownOption(currentValue, currentValue) };
            withCurrent.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            return withCurrent;
        }

        private void OnTitleInputChanged(string suffix)
        {
            // RedeemData.title is stored prefix-free - the prefix badge next to this field is
            // purely visual. RedeemManager.GetFullTitle() combines them wherever the Twitch-facing
            // string is actually needed (pushing to Twitch, matching redemption events, list
            // display).
            m_working.title = StripKnownPrefix(suffix);
        }

        /// <summary>
        /// Strips the current profile's redeem title prefix from <paramref name="title"/> if it's
        /// already there, otherwise returns it unchanged. Used both live (<see cref="OnTitleInputChanged"/>)
        /// and once up front when opening an existing redeem for edit (<see cref="OpenEdit"/>) -
        /// the latter matters because a redeem created via GUI_OLD (which always bakes the prefix
        /// into the stored title) or one saved before this wizard stopped doing the same still has
        /// it baked into <see cref="m_working"/>'s title until normalized.
        /// </summary>
        private static string StripKnownPrefix(string title)
        {
            title = title ?? "";
            string prefix = ProfileSettingsHelper.Current.redeemTitlePrefix ?? "";
            string prefixWithSpace = string.IsNullOrEmpty(prefix) ? "" : prefix + " ";
            return !string.IsNullOrEmpty(prefixWithSpace) && title.StartsWith(prefixWithSpace)
                ? title.Substring(prefixWithSpace.Length)
                : title;
        }

        /// <summary>
        /// Re-reads every step-3 control from <see cref="m_working"/> - called each time the
        /// wizard navigates into step 3 (Next from step 2, or edit re-entering it after Back).
        /// Safe to call repeatedly: every field already lives-writes back into
        /// <see cref="m_working"/> as it's edited, so re-populating from it is idempotent.
        /// </summary>
        private void PopulateStep3Fields()
        {
            if (ColorUtility.TryParseHtmlString(m_working.backgroundColor, out Color bgColor))
                m_bgColorOverlay.color = bgColor;

            m_prefixInput.text = ProfileSettingsHelper.Current.redeemTitlePrefix ?? "";
            LayoutTitleRow();

            // m_working.title is normalized to prefix-free by OpenEdit/OnTitleInputChanged, but
            // strip again defensively (StripKnownPrefix is a no-op if there's nothing to strip).
            m_titleInput.text = StripKnownPrefix(m_working.title);
            m_descriptionInput.text = m_working.description ?? "";
            m_costInput.text = m_working.points.ToString();

            m_syncingCooldown = true;
            m_cooldownUnitDropdown.SetOptions(GetCooldownUnitOptions(), m_working.cooldownUnit ?? CooldownHelper.Seconds);
            m_cooldownInput.text = m_working.cooldown > 0
                ? CooldownHelper.FromSeconds(m_working.cooldown, m_cooldownUnitDropdown.Value).ToString()
                : "0";
            m_syncingCooldown = false;
            m_maxPerStreamInput.text = Mathf.Max(0, m_working.maxPerStream).ToString();
            m_maxPerUserPerStreamInput.text = Mathf.Max(0, m_working.maxPerUserPerStream).ToString();

            m_userInputToggle.isOn = m_working.userInput;
            m_ignoreSafezoneToggle.isOn = m_working.ignoreWard;
            m_addConditionDropdown.SetOptions(EnsureIncludesCurrentValue(GetAvailableGlobalKeyOptions(), m_working.globalKeyAdd), m_working.globalKeyAdd ?? "");
            m_removeConditionDropdown.SetOptions(EnsureIncludesCurrentValue(GetAvailableGlobalKeyOptions(), m_working.globalKeyRemove), m_working.globalKeyRemove ?? "");
        }

        // ── nav row ───────────────────────────────────────────────────────

        private void BuildNavRow()
        {
            GameObject backBtnObj = GuiHelper.CreateButton(
                text: "Back",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(BackBtnX, BtnY),
                width: BtnWidth,
                height: BtnHeight
            );
            backBtnObj.SetActive(true);
            m_backBtn = backBtnObj.GetComponent<Button>();
            m_backBtn.onClick.AddListener(OnBack);

            GameObject cancelBtnObj = GuiHelper.CreateButton(
                text: "Cancel",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(CancelBtnX, BtnY),
                width: BtnWidth,
                height: BtnHeight
            );
            cancelBtnObj.SetActive(true);
            m_cancelBtn = cancelBtnObj.GetComponent<Button>();
            m_cancelBtnText = cancelBtnObj.GetComponentInChildren<Text>();
            m_cancelBtn.onClick.AddListener(() => Close(null));

            GameObject nextBtnObj = GuiHelper.CreateButton(
                text: "Next",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(NextBtnX, BtnY),
                width: BtnWidth,
                height: BtnHeight
            );
            nextBtnObj.SetActive(true);
            m_nextBtn = nextBtnObj.GetComponent<Button>();
            m_nextBtnText = nextBtnObj.GetComponentInChildren<Text>();
            m_nextBtn.onClick.AddListener(OnNext);
        }

        private void OnBack()
        {
            // Back is disabled at the floor step (RefreshNavButtons); Cancel is the way out.
            if (m_step <= m_floorStep)
                return;

            SetActiveStep(m_step - 1);
        }

        private void OnNext()
        {
            bool hasType = !string.IsNullOrEmpty(m_working.type) && m_working.type != RedeemType.Undefined;
            if (m_step == 1 && !hasType)
                return;

            if (m_step == 3)
            {
                Finish();
                return;
            }

            SetActiveStep(m_step + 1);
        }

        private void Finish()
        {
            // Defensive - the Save button is hidden in view-only mode, and the step roots are
            // non-interactable, so nothing should reach here.
            if (m_readOnly)
                return;

            if (string.IsNullOrEmpty(m_titleInput.text.Trim()))
            {
                ToastNotifications.Show("Title is required.", ToastType.Warning);
                return;
            }

            // Some types force certain sub-data values regardless of what step 2 showed (e.g.
            // Windmill/Smite/Trap's locked prefab, TerrainEdit's always-on raise) - apply them now
            // so the saved data (and anything that inspects it, like the redeem list or a
            // hand-exported profile.yaml) reflects the true values immediately. Redundant with, but
            // not a replacement for, SpawnAbilityHelper.Normalizers' own redemption-time
            // re-enforcement of the same values for hand-edited YAML.
            if (m_step2Forms.TryGetValue(m_working.type, out IRedeemStep2Form activeForm) && activeForm is IForcesValuesOnSave forcing)
                forcing.ApplyForcedValues(m_working);

            bool success;
            string error;
            string successMessage;

            string fullTitle = RedeemManager.GetFullTitle(m_working);

            if (m_editingOriginal != null)
            {
                success = RedeemManager.UpdateRedeem(m_editingOriginal, m_working, out error);
                successMessage = $"Redeem '{fullTitle}' updated.";
            }
            else
            {
                success = RedeemManager.AddRedeem(m_working, out error);
                successMessage = $"Redeem '{fullTitle}' created.";
            }

            if (!success)
            {
                ToastNotifications.Show(error, ToastType.Error);
                return;
            }

            Close(successMessage);
        }

        private void SetActiveStep(int step)
        {
            // A step-2 color swatch (Flashbang's flash color, a spawned creature's color, ...) may
            // have a picker open - it's a scene-wide singleton (see GuiHelper.CloseOpenColorPicker),
            // so navigating away from step 2 (Back/Next, or re-selecting a type in step 1) doesn't
            // close it on its own. Only matters when leaving step 2, but harmless (a no-op) to call
            // on every transition rather than special-casing "was step 2" here.
            if (m_step == 2 && step != 2)
                GuiHelper.CloseOpenColorPicker();

            m_step = step;

            m_step1Root.SetActive(step == 1);
            m_step2Root.SetActive(step == 2);
            m_step3Root.SetActive(step == 3);

            if (step == 2)
                RefreshStep2Content();
            else if (step == 3)
                PopulateStep3Fields();

            if (m_readOnly)
                KeepScrollbarsInteractive(step == 2 ? m_step2Root : step == 3 ? m_step3Root : m_step1Root);

            RefreshStepIndicator();
            RefreshNavButtons();
        }

        /// <summary>
        /// The step roots' CanvasGroups disable every Selectable below them, scrollbars included -
        /// which would stop a long list being dragged in view-only mode (the mouse wheel would still
        /// scroll). A child CanvasGroup that ignores its parents' groups keeps just the scrollbars
        /// live; safe to call repeatedly (step-2 forms can build scrollbars lazily).
        /// </summary>
        private static void KeepScrollbarsInteractive(GameObject stepRoot)
        {
            foreach (Scrollbar scrollbar in stepRoot.GetComponentsInChildren<Scrollbar>(true))
            {
                CanvasGroup group = scrollbar.GetComponent<CanvasGroup>();
                if (group == null)
                    group = scrollbar.gameObject.AddComponent<CanvasGroup>();
                group.ignoreParentGroups = true;
            }
        }

        private void RefreshNavButtons()
        {
            m_cancelBtnText.text = m_readOnly ? "Close" : "Cancel";
            // View-only has nothing to save, so the last step's Save button goes away entirely.
            m_nextBtn.gameObject.SetActive(!(m_readOnly && m_step == 3));

            m_backBtn.interactable = m_step > m_floorStep;

            bool hasType = !string.IsNullOrEmpty(m_working?.type) && m_working.type != RedeemType.Undefined;
            m_nextBtn.interactable = !(m_step == 1 && !hasType);
            m_nextBtnText.text = m_step == 3 ? (m_isEdit ? "Save changes" : "Save redeem") : "Next";
        }

        // ── small layout helpers ─────────────────────────────────────────────

        private void CreateRowLabel(GameObject parent, string text, float y, float width = FieldWidth, float x = 0f)
        {
            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                // Rounded to whole pixels - see the same note in CreateInlineToggleLabel.
                position: new Vector2(Mathf.Round(x), Mathf.Round(y)),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: Mathf.Round(width),
                height: LabelHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            // CreateText leaves Unity's default center pivot in place, so without this, the label's
            // box (center-pivoted on y) extends both above AND below y - its bottom half then
            // overlapped whatever field sits at y - LabelHeight below it (also center-pivoted, so
            // its own top half reaches back up past that point). PivotToTop makes y the label's top
            // edge instead, so "field at y - LabelHeight" lands exactly at the label's bottom edge.
            GuiHelper.PivotToTop(label.rectTransform, y);
        }

        private void CreateInlineToggleLabel(GameObject parent, string text, float centerX, float slotWidth, float y)
        {
            // Mirrors GuiFieldBuilder.CreateBoolField's own left-alignment math (toggle center =
            // position.x - width/2 + FieldHeight/2 + 5, toggle drawn at FieldHeight square) so the
            // label starts right after the toggle's actual right edge, not a guessed offset.
            float toggleRightEdge = centerX - slotWidth / 2f + GuiFieldBuilder.FieldHeight + 5f;
            float labelLeftEdge = toggleRightEdge + 6f;
            float slotRightEdge = centerX + slotWidth / 2f;
            float labelWidth = Mathf.Max(0f, slotRightEdge - labelLeftEdge);
            float labelX = labelLeftEdge + labelWidth / 2f;

            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                // Rounded to whole pixels - labelWidth/labelX above can land on a half-pixel (e.g.
                // an odd labelWidth halved for centering), and legacy uGUI Text doesn't pixel-snap
                // (see GuiHelper's CreateCard/CreateCardText for the same fix).
                position: new Vector2(Mathf.Round(labelX), Mathf.Round(y - ToggleLabelYOffset)),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: Mathf.Round(labelWidth),
                height: RowHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
        }
    }
}
