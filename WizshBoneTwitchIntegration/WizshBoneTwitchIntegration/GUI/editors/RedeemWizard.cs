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

        // Step 1
        private Text m_previewSelectedLabel;
        private Text m_previewSelectedDescription;
        private readonly Dictionary<string, (GameObject Btn, Image Bg, Text Label)> m_effectButtons = new Dictionary<string, (GameObject, Image, Text)>();

        // Step 2
        private Text m_step2PlaceholderText;

        // Step 3
        private Image m_bgColorOverlay;
        private InputField m_titleInput;
        private Text m_prefixBadgeText;
        private InputField m_descriptionInput;
        private InputField m_costInput;
        private InputField m_cooldownInput;
        private Toggle m_userInputToggle;
        private Toggle m_ignoreSafezoneToggle;
        private SearchableDropdown m_addConditionDropdown;
        private SearchableDropdown m_removeConditionDropdown;

        // Nav
        private Text m_feedbackText;
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
        private const float ContentMargin = 30f;

        // BuildBackground's card used to inset only 15px (BackgroundInset); StepBarY was tuned to
        // sit snugly against that. Now that it insets ContentMargin (30px) on every side to match
        // the other tabs, the step bar is shifted 15px further down so it still sits inside the
        // (now bigger) card instead of poking above it - see the identical fix in
        // RedeemHistorySection.cs's TitleY.
        private const float StepBarY     = -45f;
        private const float StepBarWidth = 280f;
        private const float StepBarHeight = 5f;
        private const float StepBarGap   = 8f;
        private const int   StepBarCount = 3;

        // Top edge (not center - see the PivotToTop call in BuildStepIndicator) of the step title
        // text, offset below the step bars' own bottom edge by StepTitleGap. Previously this was a
        // center-pivoted -55, whose box top (-42.5) landed ABOVE the bars' own bottom edge (-47.5)
        // - the title text sat flush against, almost overlapping, the bar above it.
        private const float StepTitleGap = 12f;
        private const float StepTitleY   = StepBarY - StepBarHeight / 2f - StepTitleGap;
        private const float BodyTopY     = -95f;

        // The step bars are centered as a row (BuildStepIndicator), independent of ContentMargin -
        // so this, not a ContentMargin-derived LeftEdgeX, is the left edge everything in step 1
        // (the effect list, the preview column) aligns to. Computed from the same bar geometry
        // BuildStepIndicator itself uses, so the two can't drift apart again.
        private const float StepRowWidth = StepBarCount * StepBarWidth + (StepBarCount - 1) * StepBarGap;
        private const float StepRowLeftX = -StepRowWidth / 2f;

        private const float FieldWidth  = 460f;
        private const float RowHeight   = 36f;
        private const float LabelHeight = 20f;
        private const float RowGap      = 14f;

        // Step 3 spans nearly the full wizard card, not the narrow FieldWidth column steps 1/2 use
        // - RedesignUI.dc.html's step-3 content sits in a `flex:1` column (own 22px padding) inside
        // the wizard's own card, not a fixed-width centered form. Step3Padding here mirrors that
        // 22px inset (rounded to match this file's other paddings) from the wizard card's edges
        // (the wizard card itself is ContentWidth - 2*ContentMargin wide).
        private const float Step3Padding = 30f;
        private const float Step3Width   = ContentWidth - 2f * ContentMargin - 2f * Step3Padding;

        // Step 3's reward-preview card (BuildStep3) - swatch + title/description + points, all in
        // one card matching RedesignUI.dc.html's step-3 layout.
        private const float BoxPadding          = 14f;
        private const float BoxRowGap           = 6f;
        private const float BoxSwatchSize       = 64f;
        private const float BoxPointsColumnWidth = 90f;

        private const float TypeListWidth  = 220f;
        private const float TypeListHeight = 380f;
        private const float TypeBtnHeight  = 34f;
        private const float PreviewX       = StepRowLeftX + TypeListWidth + 30f + (FieldWidth - TypeListWidth) / 2f;

        // Also shifted +15 to clear the card's now-bigger bottom inset - see the StepBarY comment
        // above and RedeemHistorySection.cs's matching ActionRowY fix.
        private const float BtnY      = 55f;
        private const float BtnWidth  = 160f;
        private const float BtnHeight = 44f;
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

            BuildBackground();
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

        // ── background ───────────────────────────────────────────────────────

        /// <summary>
        /// The darker inset card behind the whole wizard - missing from the original port, which
        /// built the step bar/steps/nav row directly on the tab's own backdrop with no card of
        /// their own. Built first so every later sibling draws on top of it. Inset by
        /// <see cref="ContentMargin"/> on all four sides so the card sits an even distance from the
        /// section edges. No border - a semi-transparent fill alone is enough to separate it.
        /// </summary>
        private void BuildBackground()
        {
            GameObject background = GuiHelper.CreateRegion(
                m_root, "Background",
                anchorMin: Vector2.zero,
                anchorMax: Vector2.one,
                offsetMin: new Vector2(ContentMargin, ContentMargin),
                offsetMax: new Vector2(-ContentMargin, -ContentMargin));
            GuiHelper.AddBackground(background, new Color(0f, 0f, 0f, 0.6f));
        }

        // ── step indicator / title ──────────────────────────────────────────

        private void BuildStepIndicator()
        {
            float startX = StepRowLeftX + StepBarWidth / 2f;

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
                height: 25f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_stepTitleText.alignment = TextAnchor.MiddleCenter;
            GuiHelper.PivotToTop(m_stepTitleText.rectTransform, StepTitleY);
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
            m_stepTitleText.text = m_step >= 1 && m_step <= 3 ? titles[m_step] : "";
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

                GameObject btnObj = GUIManager.Instance.CreateButton(
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
                Text label = btnObj.GetComponentInChildren<Text>();
                label.alignment = TextAnchor.MiddleLeft;

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

            m_previewSelectedLabel = GUIManager.Instance.CreateText(
                text: "",
                parent: m_step1Root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(PreviewX, BodyTopY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: FieldWidth - TypeListWidth,
                height: 26f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_previewSelectedLabel.alignment = TextAnchor.MiddleLeft;
            // Both this and m_previewSelectedDescription below default to a center pivot, so
            // without re-pivoting, each one's box extends both above AND below its given Y - at
            // BodyTopY and BodyTopY - 40f respectively, that put the label's bottom half and the
            // description's top half on top of each other. PivotToTop makes BodyTopY the label's
            // top edge instead, so it only grows downward from there.
            GuiHelper.PivotToTop(m_previewSelectedLabel.rectTransform, BodyTopY);

            m_previewSelectedDescription = GUIManager.Instance.CreateText(
                text: "",
                parent: m_step1Root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(PreviewX, BodyTopY - 40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: FieldWidth - TypeListWidth,
                height: 100f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_previewSelectedDescription.alignment = TextAnchor.UpperLeft;
            m_previewSelectedDescription.horizontalOverflow = HorizontalWrapMode.Wrap;
            GuiHelper.MakeDescriptionExpandDownward(m_previewSelectedDescription, BodyTopY - 40f);
        }

        private void RefreshStep1Selection()
        {
            bool hasType = !string.IsNullOrEmpty(m_working?.type) && m_working.type != RedeemType.Undefined;

            foreach (var kvp in m_effectButtons)
            {
                bool selected = hasType && kvp.Key == m_working.type;
                kvp.Value.Bg.color = selected ? new Color(0.95f, 0.65f, 0.2f, 0.35f) : new Color(1f, 1f, 1f, 0.08f);
                kvp.Value.Label.color = selected ? GUIManager.Instance.ValheimOrange : GUIManager.Instance.ValheimBeige;
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
            // in one dark card, matching RedesignUI.dc.html's step-3 "live preview tile" (no
            // separate "Background color"/"Title"/"Description" captions - the fields' own
            // styling/placement already reads as a preview of the actual Twitch reward tile).
            float boxContentHeight = 2f * RowHeight + BoxRowGap;
            float boxHeight = 2f * BoxPadding + boxContentHeight;
            GameObject rewardBox = GuiHelper.CreateCard(m_step3Root, new Vector2(0f, y), Step3Width, boxHeight);

            // Swatch - PopulateStep3Fields keeps its overlay in sync with m_working.backgroundColor
            // on every step-3 entry (the picker's own onChanged callback keeps m_working in sync
            // the other way).
            float swatchX = -Step3Width / 2f + BoxPadding + BoxSwatchSize / 2f;
            float swatchY = -BoxPadding - BoxSwatchSize / 2f;
            GameObject bgSwatch = GuiFieldBuilder.CreateColorField(rewardBox, new Vector2(swatchX, swatchY), BoxSwatchSize, "#a970ff", "Redeem background color", v => m_working.backgroundColor = v, BoxSwatchSize);
            m_bgColorOverlay = bgSwatch.transform.Find("ColorOverlay").GetComponent<Image>();

            // Points column - flush with the box's right edge, top-aligned with the title row.
            float pointsX = Step3Width / 2f - BoxPadding - BoxPointsColumnWidth / 2f;
            float pointsInputY = -BoxPadding - RowHeight / 2f;
            m_costInput = GuiFieldBuilder.CreateIntField(rewardBox, new Vector2(pointsX, pointsInputY), BoxPointsColumnWidth - 10f, 0, v => m_working.points = v);
            Text pointsCaption = GUIManager.Instance.CreateText(
                text: "points",
                parent: rewardBox.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(pointsX, pointsInputY - RowHeight / 2f - 10f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 10,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: BoxPointsColumnWidth,
                height: 16f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            pointsCaption.alignment = TextAnchor.MiddleCenter;

            // Middle column: title row (prefix badge + input), description row directly beneath it.
            float middleLeftX = swatchX + BoxSwatchSize / 2f + BoxPadding;
            float middleRightX = pointsX - BoxPointsColumnWidth / 2f - BoxPadding;
            float middleWidth = middleRightX - middleLeftX;
            float middleCenterX = (middleLeftX + middleRightX) / 2f;

            float titleRowY = -BoxPadding - RowHeight / 2f;
            float prefixWidth = 90f;
            m_prefixBadgeText = GUIManager.Instance.CreateText(
                text: "",
                parent: rewardBox.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(middleLeftX + prefixWidth / 2f, titleRowY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: prefixWidth,
                height: RowHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_prefixBadgeText.alignment = TextAnchor.MiddleCenter;

            float titleInputWidth = middleWidth - prefixWidth - 10f;
            float titleInputX = middleLeftX + prefixWidth + 10f + titleInputWidth / 2f;
            m_titleInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(titleInputX, titleRowY), titleInputWidth, placeholderText: "Reward title");
            m_titleInput.onValueChanged.AddListener(OnTitleInputChanged);

            float descriptionRowY = titleRowY - RowHeight - BoxRowGap;
            m_descriptionInput = GuiFieldBuilder.CreateInputField(rewardBox, new Vector2(middleCenterX, descriptionRowY), middleWidth, placeholderText: "Shown to viewers when they open the reward");
            m_descriptionInput.onValueChanged.AddListener(v => m_working.description = v);

            y -= boxHeight + RowGap;

            // Cooldown + toggles, one row: cooldown alone on the left half, both toggles side by
            // side on the right half (matches RedesignUI.dc.html's Row A) - now that step 3 spans
            // the full card width, each toggle+label slot has enough room to not need stacking.
            float halfWidth = (Step3Width - 20f) / 2f;
            float leftHalfX = -Step3Width / 2f + halfWidth / 2f;
            float rightHalfX = Step3Width / 2f - halfWidth / 2f;

            CreateRowLabel(m_step3Root, "Cooldown (sec)", y, halfWidth, leftHalfX);
            m_cooldownInput = GuiFieldBuilder.CreateIntField(m_step3Root, new Vector2(leftHalfX, y - LabelHeight), halfWidth, 0, v => m_working.cooldown = v);
            GuiHelper.PivotToTop((RectTransform)m_cooldownInput.transform, y - LabelHeight);

            float toggleRowY = y - LabelHeight; // align with the cooldown input, not its label
            float quarterWidth = halfWidth / 2f;
            float toggle1X = rightHalfX - quarterWidth / 2f;
            float toggle2X = rightHalfX + quarterWidth / 2f;
            m_userInputToggle = GuiFieldBuilder.CreateBoolField(m_step3Root, new Vector2(toggle1X, toggleRowY), quarterWidth, false, v => m_working.userInput = v);
            CreateInlineToggleLabel(m_step3Root, "Requires viewer input", toggle1X, quarterWidth, toggleRowY);
            m_ignoreSafezoneToggle = GuiFieldBuilder.CreateBoolField(m_step3Root, new Vector2(toggle2X, toggleRowY), quarterWidth, false, v => m_working.ignoreWard = v);
            CreateInlineToggleLabel(m_step3Root, "Ignore safezone", toggle2X, quarterWidth, toggleRowY);

            y -= LabelHeight + RowHeight + RowGap;

            // Add / remove condition - dropdowns over Valheim's global keys, matching GUI_OLD's
            // FieldUIBuilder.BuildStringDropdownField wiring: OnValueChanged writes straight into
            // m_working, and PopulateStep3Fields re-syncs the displayed value/options every time
            // step 3 is (re-)entered.
            CreateRowLabel(m_step3Root, "Add condition", y, halfWidth, leftHalfX);
            CreateRowLabel(m_step3Root, "Remove condition", y, halfWidth, rightHalfX);
            m_addConditionDropdown = new SearchableDropdown();
            GameObject addConditionToggle = m_addConditionDropdown.Build(m_step3Root, new Vector2(leftHalfX, y - LabelHeight), halfWidth, GuiFieldBuilder.FieldHeight, GetAvailableGlobalKeyOptions(), "");
            GuiHelper.PivotToTop((RectTransform)addConditionToggle.transform, y - LabelHeight);
            m_addConditionDropdown.OnValueChanged += v => m_working.globalKeyAdd = v;
            m_removeConditionDropdown = new SearchableDropdown();
            GameObject removeConditionToggle = m_removeConditionDropdown.Build(m_step3Root, new Vector2(rightHalfX, y - LabelHeight), halfWidth, GuiFieldBuilder.FieldHeight, GetAvailableGlobalKeyOptions(), "");
            GuiHelper.PivotToTop((RectTransform)removeConditionToggle.transform, y - LabelHeight);
            m_removeConditionDropdown.OnValueChanged += v => m_working.globalKeyRemove = v;
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

            m_prefixBadgeText.text = ProfileSettingsHelper.Current.redeemTitlePrefix ?? "";

            // m_working.title is normalized to prefix-free by OpenEdit/OnTitleInputChanged, but
            // strip again defensively (StripKnownPrefix is a no-op if there's nothing to strip).
            m_titleInput.text = StripKnownPrefix(m_working.title);
            m_descriptionInput.text = m_working.description ?? "";
            m_costInput.text = m_working.points.ToString();
            m_cooldownInput.text = m_working.cooldown.ToString();
            m_userInputToggle.isOn = m_working.userInput;
            m_ignoreSafezoneToggle.isOn = m_working.ignoreWard;
            m_addConditionDropdown.SetOptions(EnsureIncludesCurrentValue(GetAvailableGlobalKeyOptions(), m_working.globalKeyAdd), m_working.globalKeyAdd ?? "");
            m_removeConditionDropdown.SetOptions(EnsureIncludesCurrentValue(GetAvailableGlobalKeyOptions(), m_working.globalKeyRemove), m_working.globalKeyRemove ?? "");

            m_feedbackText.text = "";
        }

        // ── nav row ───────────────────────────────────────────────────────

        private void BuildNavRow()
        {
            m_feedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0f, BtnY + 40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: ContentWidth - 2f * ContentMargin,
                height: 24f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_feedbackText.alignment = TextAnchor.MiddleCenter;

            GameObject backBtnObj = GUIManager.Instance.CreateButton(
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

            GameObject nextBtnObj = GUIManager.Instance.CreateButton(
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
                m_feedbackText.text = "Title is required.";
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
                m_feedbackText.text = error;
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
                position: new Vector2(Mathf.Round(labelX), Mathf.Round(y - RowHeight / 4f)),
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
