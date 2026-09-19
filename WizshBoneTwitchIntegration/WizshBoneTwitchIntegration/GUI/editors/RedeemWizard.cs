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
        /// <c>null</c> on Cancel/Back-past-the-floor-step. The owning tab switches back to the
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
        private Text m_previewSelectedLabel;
        private Text m_previewSelectedDescription;
        private readonly Dictionary<string, (GameObject Btn, Image Bg, Text Label)> m_effectButtons = new Dictionary<string, (GameObject, Image, Text)>();
        private Color m_typeButtonDefaultColor;

        // Step 2
        private Text m_step2PlaceholderText;

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
        private Text m_backBtnText;
        private Button m_nextBtn;
        private Text m_nextBtnText;

        private RedeemData m_editingOriginal;
        private RedeemData m_working;
        private bool m_isEdit;
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
        private const float BodyTopY             = StepIndicatorBottomY - RowGap;

        private const float FieldWidth  = 460f;
        private const float RowHeight   = 36f;
        private const float LabelHeight = 20f;
        private const float RowGap      = 14f;

        // Step 3 spans nearly the full wizard card, not the narrow FieldWidth column steps 1/2 use
        // - RedesignUI.dc.html's step-3 content sits in a `flex:1` column (own 22px padding) inside
        // the wizard's own section, not a fixed-width centered form. It spans the same
        // ContentWidth - 2*ContentMargin as the step-bar row, so the two line up exactly.
        private const float Step3Width   = ContentWidth - 2f * ContentMargin;

        // Step 3's reward-preview card (BuildStep3) - swatch + title/description + points, all in
        // one card matching RedesignUI.dc.html's step-3 layout.
        private const float BoxPadding          = 14f;
        private const float BoxRowGap           = 6f;
        private const float BoxPointsColumnWidth = 90f;

        // Same 10f-per-side text inset Jotunn's CreateInputField gives every input (text width is
        // width - 20f), so the prefix badge's text sits as far from its border as the title's does.
        private const float PrefixTextPadding = 20f;
        private const float PrefixGap         = 4f;

        // Step 3's cooldown, toggles and conditions cards (BuildStep3) - same padding as the
        // preview card, with a fixed gap between the two halves of a row and a fixed-width unit
        // dropdown beside the cooldown number.
        // Longest typed count (points, cooldown, limits) - keeps int.Parse from overflowing.
        private const int   CountMaxLength     = 9;
        private const float CardColumnGap      = 20f;
        private const float CooldownUnitWidth  = 130f;

        // The conditions card's one-line description (BuildStep3) and the gap under it.
        private const float ConditionDescriptionHeight = 20f;
        private const float ConditionDescriptionGap    = 10f;

        // A toggle's label is centered this far below the toggle's `position.y` - the same offset
        // GuiHelper.CreateToggleStatusRow uses (toggle at -56, status word at -60) for Home's cards,
        // so the label reads as vertically centered on the toggle graphic, not sitting under it.
        private const float ToggleLabelYOffset = 4f;

        // The type list runs from BodyTopY down to a RowGap above the Back/Next buttons' top edge,
        // so step 1's body fills the same vertical space step 3's cards do.
        private const float TypeListWidth  = 220f;
        private const float TypeListHeight = ContentHeight - (BtnY + BtnHeight / 2f) - RowGap + BodyTopY;
        private const float TypeBtnHeight  = 34f;

        // The preview column spans the rest of the same row the type list and step-indicator bars
        // already anchor to (StepRowLeftX/StepRowWidth), not the narrower FieldWidth (that's step
        // 2's per-type *form field* width, a different, intentionally-narrow context) - previously
        // this column was wrongly sized off FieldWidth, leaving most of the row's width unused.
        private const float PreviewGapX   = RowGap;   // same as the vertical gaps between cards
        private const float PreviewWidth  = StepRowWidth - TypeListWidth - PreviewGapX;
        private const float PreviewX      = StepRowLeftX + TypeListWidth + PreviewGapX + PreviewWidth / 2f;

        // Reserved space for a future per-type image/video thumbnail (no actual media yet - see
        // PreviewImagePlaceholder in BuildStep1), sized/positioned per RedesignUI.dc.html's own
        // step-1 preview box. The selected effect's title + description sit in a fixed-height card
        // (same fill as step 3's cards, BoxPadding around the text) at the bottom of the column,
        // ending level with the type list; the image placeholder takes all the height above it.
        private const float PreviewCardHeight  = 156f;
        private const float PreviewImageGap    = 14f; // matches RowGap's existing section-gap value
        private const float PreviewImageHeight = TypeListHeight - PreviewCardHeight - PreviewImageGap;
        private const float PreviewLabelTopY   = BodyTopY - PreviewImageHeight - PreviewImageGap;

        private const float PreviewTextWidth  = PreviewWidth - 2f * BoxPadding;
        private const float PreviewTextTopY   = PreviewLabelTopY - BoxPadding;

        // BtnY is the buttons' center measured up from the root's bottom edge: the section's bottom
        // margin plus half the button height, so the buttons' lower edge sits ContentMargin above
        // the root's - see RedeemHistorySection.ActionRowY.
        private const float BtnWidth  = 160f;
        private const float BtnHeight = 44f;
        private const float BtnY      = ContentMargin + BtnHeight / 2f;
        private const float BackBtnX  = -150f;
        private const float NextBtnX  =  150f;

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

            m_root.SetActive(false);
            return m_root;
        }

        public void OpenCreate()
        {
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

            string[] titles = { "", "CHOOSE AN EFFECT", "CONFIGURE PARAMETERS", "SETUP CHANNEL POINT REWARD" };
            string[] descriptions =
            {
                "",
                "Pick what happens in the game when a viewer redeems this reward.",
                "Adjust how the chosen effect behaves.",
                "Set how the reward looks on Twitch and how often it can be redeemed."
            };
            bool validStep = m_step >= 1 && m_step <= 3;
            m_stepTitleText.text = validStep ? titles[m_step] : "";
            m_stepDescriptionText.text = validStep ? descriptions[m_step] : "";
        }

        // ── step 1: choose effect ────────────────────────────────────────────

        private void BuildStep1()
        {
            m_step1Root = UIContainer.Create(m_root, "Step1");

            GameObject listContent = ScrollableList.CreateFixed(
                m_step1Root, "EffectTypeList",
                anchoredPosition: new Vector2(StepRowLeftX + TypeListWidth / 2f, BodyTopY),
                width: TypeListWidth, height: TypeListHeight);

            float yOffset = -(TypeBtnHeight / 2f);
            foreach (RedeemEffectInfo info in RedeemEffectCatalog.All)
            {
                string capturedType = info.Type;

                GameObject btnObj = GuiHelper.CreateButton(
                    text: info.Label,
                    parent: listContent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, yOffset),
                    width: TypeListWidth - ScrollableList.ScrollbarWidth - 8f,
                    height: TypeBtnHeight
                );
                btnObj.SetActive(true);

                Image bg = btnObj.GetComponent<Image>();
                if (m_effectButtons.Count == 0)
                    m_typeButtonDefaultColor = bg.color;
                Text label = btnObj.GetComponentInChildren<Text>();
                label.color = GUIManager.Instance.ValheimOrange;
                label.alignment = TextAnchor.MiddleLeft;
                RectTransform labelRt = label.rectTransform;
                labelRt.offsetMin = new Vector2(labelRt.offsetMin.x + ListRow.LeftPadding, labelRt.offsetMin.y);

                btnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    m_working.type = capturedType;
                    RefreshStep1Selection();
                    RefreshNavButtons();
                });

                m_effectButtons[capturedType] = (btnObj, bg, label);

                yOffset -= TypeBtnHeight + 4f;
            }

            ScrollableList.SetContentHeight(listContent, Mathf.Abs(yOffset));

            // Reserved space for a future per-type image/video thumbnail - no media exists yet, so
            // this is just a flat placeholder in the scrollbar track's own dark shade.
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

            // Built before the two texts below (siblings draw in creation order), so the card sits
            // behind them.
            GuiHelper.CreateCard(m_step1Root, new Vector2(PreviewX, PreviewLabelTopY), PreviewWidth, PreviewCardHeight);

            m_previewSelectedLabel = GUIManager.Instance.CreateText(
                text: "",
                parent: m_step1Root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(PreviewX, PreviewTextTopY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: PreviewTextWidth,
                height: 26f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_previewSelectedLabel.alignment = TextAnchor.MiddleLeft;
            // Both this and m_previewSelectedDescription below default to a center pivot, so
            // without re-pivoting, each one's box extends both above AND below its given Y - at
            // PreviewTextTopY and PreviewTextTopY - 40f respectively, that put the label's bottom
            // half and the description's top half on top of each other. PivotToTop makes
            // PreviewTextTopY the label's top edge instead, so it only grows downward from there.
            GuiHelper.PivotToTop(m_previewSelectedLabel.rectTransform, PreviewTextTopY);

            m_previewSelectedDescription = GUIManager.Instance.CreateText(
                text: "",
                parent: m_step1Root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(PreviewX, PreviewTextTopY - 40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: PreviewTextWidth,
                height: 100f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_previewSelectedDescription.alignment = TextAnchor.UpperLeft;
            m_previewSelectedDescription.horizontalOverflow = HorizontalWrapMode.Wrap;
            GuiHelper.MakeDescriptionExpandDownward(m_previewSelectedDescription, PreviewTextTopY - 40f);
        }

        private void RefreshStep1Selection()
        {
            bool hasType = !string.IsNullOrEmpty(m_working?.type) && m_working.type != RedeemType.Undefined;

            foreach (var kvp in m_effectButtons)
            {
                bool selected = hasType && kvp.Key == m_working.type;
                kvp.Value.Bg.color = selected ? ShellSidebar.TabActiveColor : m_typeButtonDefaultColor;
            }

            m_previewSelectedLabel.text = hasType ? RedeemEffectCatalog.LabelFor(m_working.type) : "Choose an effect from the list";
            m_previewSelectedDescription.text = hasType
                ? RedeemEffectCatalog.DescriptionFor(m_working.type)
                : "Select an effect on the left to see what it does in-game.";
        }

        // ── step 2: configure parameters (placeholder this round) ──────────

        private void BuildStep2()
        {
            m_step2Root = UIContainer.Create(m_root, "Step2");

            m_step2PlaceholderText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_step2Root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, BodyTopY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 15,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: FieldWidth,
                height: 60f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_step2PlaceholderText.alignment = TextAnchor.UpperCenter;
            m_step2PlaceholderText.horizontalOverflow = HorizontalWrapMode.Wrap;
            // Same center-pivot fix as m_previewSelectedLabel/Description in BuildStep1 - without
            // it, this box's top half sits above BodyTopY instead of starting there, putting it
            // noticeably higher/tighter under the step bar than step 1's (genuinely top-pivoted)
            // content does.
            GuiHelper.PivotToTop(m_step2PlaceholderText.rectTransform, BodyTopY);
        }

        private void RefreshStep2Placeholder()
        {
            string label = RedeemEffectCatalog.LabelFor(m_working.type);
            m_step2PlaceholderText.text = $"{label} has no extra parameters yet - continue to set up the reward.\n(Per-effect configuration is coming in a future update.)";
        }

        // ── step 3: Twitch reward setup ──────────────────────────────────────

        private void BuildStep3()
        {
            m_step3Root = UIContainer.Create(m_root, "Step3");

            float y = BodyTopY;

            // Reward preview card - color swatch + title/description column + points column, all
            // in one dark card, matching RedesignUI.dc.html's step-3 "live preview tile". One label
            // row on top ("Color" / "Title and description" / "Points"), same label style as the
            // cooldown/rest cards below; the content rows start under it at contentTopY.
            float labelRowY = -BoxPadding;
            float contentTopY = labelRowY - LabelHeight;
            float boxContentHeight = 2f * RowHeight + BoxRowGap;
            float boxHeight = 2f * BoxPadding + LabelHeight + boxContentHeight;
            GameObject rewardBox = GuiHelper.CreateCard(m_step3Root, new Vector2(0f, y), Step3Width, boxHeight);

            // Swatch - PopulateStep3Fields keeps its overlay in sync with m_working.backgroundColor
            // on every step-3 entry (the picker's own onChanged callback keeps m_working in sync
            // the other way).
            // Square sized to span both stacked inputs (title row + gap + description row), so its
            // top/bottom edges line up with them.
            float swatchSize = boxContentHeight;
            float swatchX = -Step3Width / 2f + BoxPadding + swatchSize / 2f;
            float swatchY = contentTopY - swatchSize / 2f;
            GameObject bgSwatch = GuiFieldBuilder.CreateColorField(rewardBox, new Vector2(swatchX, swatchY), swatchSize, "#a970ff", "Redeem background color", v => m_working.backgroundColor = v, swatchSize);
            m_bgColorOverlay = bgSwatch.transform.Find("ColorOverlay").GetComponent<Image>();

            // Points column - flush with the box's right edge, top-aligned with the title row.
            float pointsX = Step3Width / 2f - BoxPadding - BoxPointsColumnWidth / 2f;
            float pointsInputY = contentTopY - RowHeight / 2f;
            m_costInput = GuiFieldBuilder.CreateIntField(rewardBox, new Vector2(pointsX, pointsInputY), BoxPointsColumnWidth, 0, v => m_working.points = Mathf.Max(0, v), emptyAsZero: true, maxLength: CountMaxLength);

            // Middle column: title row (prefix badge + input), description row directly beneath it.
            float middleLeftX = swatchX + swatchSize / 2f + BoxPadding;
            float middleRightX = pointsX - BoxPointsColumnWidth / 2f - BoxPadding;
            float middleWidth = middleRightX - middleLeftX;
            float middleCenterX = (middleLeftX + middleRightX) / 2f;

            CreateRowLabel(rewardBox, "Color", labelRowY, swatchSize, swatchX);
            CreateRowLabel(rewardBox, "Title and description", labelRowY, middleWidth, middleCenterX);
            CreateRowLabel(rewardBox, "Points", labelRowY, BoxPointsColumnWidth, pointsX);

            float titleRowY = contentTopY - RowHeight / 2f;
            m_titleRowLeftX = middleLeftX;
            m_titleRowWidth = middleWidth;

            // Prefix: a disabled-looking input sitting directly in front of the title input. Both
            // are created at placeholder sizes here and sized for real by LayoutTitleRow, which
            // also re-runs whenever step 3 is entered (the prefix can differ per profile).
            m_prefixInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(middleLeftX, titleRowY), 100f);
            m_prefixInput.interactable = false;
            m_prefixInput.textComponent.alignment = TextAnchor.MiddleCenter;

            m_titleInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(middleCenterX, titleRowY), middleWidth, placeholderText: "Reward title");
            m_titleInput.onValueChanged.AddListener(OnTitleInputChanged);
            LayoutTitleRow();

            float descriptionRowY = titleRowY - RowHeight - BoxRowGap;
            m_descriptionInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(middleCenterX, descriptionRowY), middleWidth, placeholderText: "Description shown to viewers when they open the reward");
            m_descriptionInput.onValueChanged.AddListener(v => m_working.description = v);

            y -= boxHeight + RowGap;

            // Both cards below share the same two-column grid inside their own BoxPadding inset.
            float halfWidth = (Step3Width - 2f * BoxPadding - CardColumnGap) / 2f;
            float leftHalfX = -Step3Width / 2f + BoxPadding + halfWidth / 2f;
            float rightHalfX = Step3Width / 2f - BoxPadding - halfWidth / 2f;

            // Cooldown card: one row of three equal columns - cooldown (number + unit dropdown),
            // limit per stream, limit per user per stream. 0/empty means "off" for all three
            // (SetRewards only enables the Twitch-side flag for values above 0).
            float cooldownCardHeight = 2f * BoxPadding + LabelHeight + RowHeight;
            GameObject cooldownCard = GuiHelper.CreateCard(m_step3Root, new Vector2(0f, y), Step3Width, cooldownCardHeight);

            float thirdWidth = (Step3Width - 2f * BoxPadding - 2f * CardColumnGap) / 3f;
            float firstThirdX = -Step3Width / 2f + BoxPadding + thirdWidth / 2f;
            float secondThirdX = firstThirdX + thirdWidth + CardColumnGap;
            float thirdThirdX = secondThirdX + thirdWidth + CardColumnGap;

            float cooldownRowY = -BoxPadding;
            CreateRowLabel(cooldownCard, "Cooldown", cooldownRowY, thirdWidth, firstThirdX);

            float cooldownNumberWidth = thirdWidth - CooldownUnitWidth - BoxRowGap;
            float cooldownNumberX = firstThirdX - thirdWidth / 2f + cooldownNumberWidth / 2f;
            float cooldownUnitX = firstThirdX + thirdWidth / 2f - CooldownUnitWidth / 2f;

            m_cooldownInput = GuiFieldBuilder.CreateIntField(cooldownCard, new Vector2(cooldownNumberX, cooldownRowY - LabelHeight), cooldownNumberWidth, 0, _ => OnCooldownChanged(), emptyAsZero: true, maxLength: CountMaxLength);
            GuiHelper.PivotToTop((RectTransform)m_cooldownInput.transform, cooldownRowY - LabelHeight);

            m_cooldownUnitDropdown = new SearchableDropdown();
            GameObject cooldownUnitToggle = m_cooldownUnitDropdown.Build(cooldownCard, new Vector2(cooldownUnitX, cooldownRowY - LabelHeight), CooldownUnitWidth, GuiFieldBuilder.FieldHeight, GetCooldownUnitOptions(), CooldownHelper.Seconds, showSearch: false);
            GuiHelper.PivotToTop((RectTransform)cooldownUnitToggle.transform, cooldownRowY - LabelHeight);
            m_cooldownUnitDropdown.OnValueChanged += _ => OnCooldownChanged();

            CreateRowLabel(cooldownCard, "Limit per stream", cooldownRowY, thirdWidth, secondThirdX);
            CreateRowLabel(cooldownCard, "Limit per user per stream", cooldownRowY, thirdWidth, thirdThirdX);
            m_maxPerStreamInput = GuiFieldBuilder.CreateIntField(cooldownCard, new Vector2(secondThirdX, cooldownRowY - LabelHeight), thirdWidth, 0, v => m_working.maxPerStream = Mathf.Max(0, v), emptyAsZero: true, maxLength: CountMaxLength);
            GuiHelper.PivotToTop((RectTransform)m_maxPerStreamInput.transform, cooldownRowY - LabelHeight);
            m_maxPerUserPerStreamInput = GuiFieldBuilder.CreateIntField(cooldownCard, new Vector2(thirdThirdX, cooldownRowY - LabelHeight), thirdWidth, 0, v => m_working.maxPerUserPerStream = Mathf.Max(0, v), emptyAsZero: true, maxLength: CountMaxLength);
            GuiHelper.PivotToTop((RectTransform)m_maxPerUserPerStreamInput.transform, cooldownRowY - LabelHeight);

            y -= cooldownCardHeight + RowGap;

            // Conditions card: a one-line description on top, then the add/remove condition
            // dropdowns. Single line only - the card's height is fixed, and a description that
            // wrapped would be dropped entirely by Unity's vertical-truncate on a too-short box.
            float conditionCardHeight = 2f * BoxPadding + ConditionDescriptionHeight + ConditionDescriptionGap + LabelHeight + RowHeight;
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
            GameObject addConditionToggle = m_addConditionDropdown.Build(conditionCard, new Vector2(leftHalfX, conditionRowY - LabelHeight), halfWidth, GuiFieldBuilder.FieldHeight, GetAvailableGlobalKeyOptions(), "");
            GuiHelper.PivotToTop((RectTransform)addConditionToggle.transform, conditionRowY - LabelHeight);
            m_addConditionDropdown.OnValueChanged += v => m_working.globalKeyAdd = v;
            m_removeConditionDropdown = new SearchableDropdown();
            GameObject removeConditionToggle = m_removeConditionDropdown.Build(conditionCard, new Vector2(rightHalfX, conditionRowY - LabelHeight), halfWidth, GuiFieldBuilder.FieldHeight, GetAvailableGlobalKeyOptions(), "");
            GuiHelper.PivotToTop((RectTransform)removeConditionToggle.transform, conditionRowY - LabelHeight);
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
            m_backBtnText = backBtnObj.GetComponentInChildren<Text>();
            m_backBtn.onClick.AddListener(OnBack);

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
            if (m_step <= m_floorStep)
            {
                Close(null);
                return;
            }

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
            if (string.IsNullOrEmpty(m_titleInput.text.Trim()))
            {
                ToastNotifications.Show("Title is required.", ToastType.Warning);
                return;
            }

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
            m_step = step;

            m_step1Root.SetActive(step == 1);
            m_step2Root.SetActive(step == 2);
            m_step3Root.SetActive(step == 3);

            if (step == 2)
                RefreshStep2Placeholder();
            else if (step == 3)
                PopulateStep3Fields();

            RefreshStepIndicator();
            RefreshNavButtons();
        }

        private void RefreshNavButtons()
        {
            m_backBtnText.text = m_step <= m_floorStep ? "Cancel" : "Back";

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
