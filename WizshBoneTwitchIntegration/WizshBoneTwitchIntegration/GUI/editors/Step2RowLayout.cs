using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Stacks label+field+description rows top-down inside a redeem wizard step-2 per-type form,
    /// tracking its own Y cursor so per-type form classes don't hand-compute row offsets the way
    /// <see cref="RedeemWizard"/>'s step 3 still does. Construct one fresh instance per form
    /// "page" (a type's General tab, its Damage tab, an entry-list card, ...) each time that page
    /// is (re)built - this class holds no state beyond the current Y cursor, so there's nothing to
    /// reset between builds.
    ///
    /// Every row shows an always-visible description under its field (no hover-only tooltip/"?"
    /// icon - removed entirely from this page) and a "Reset" button that writes
    /// <c>defaultValue</c> back into the field, reusing <see cref="GuiHelper"/>'s reset-button
    /// plumbing (originally built for SettingsTab). Because descriptions vary from none to full
    /// sentences that wrap 2-3 lines at <see cref="RedeemWizard.FieldWidth"/>, each row's height is
    /// measured rather than fixed - see <see cref="CreateDescription"/>.
    /// </summary>
    internal class Step2RowLayout
    {
        private readonly GameObject m_parent;
        private readonly float m_width;
        private float m_y;

        public Step2RowLayout(GameObject parent, float topY, float width = RedeemWizard.Step2FieldWidth - ScrollableList.ScrollbarWidth)
        {
            m_parent = parent;
            m_width = width;
            m_y = topY;
        }

        /// <summary>Y the next row would start at - lets a caller size a scroll container after laying out all its rows.</summary>
        public float CurrentY => m_y;

        private void CreateLabel(string label, float y)
        {
            Text text = GUIManager.Instance.CreateText(
                text: label,
                parent: m_parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, y),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: m_width,
                height: RedeemWizard.LabelHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            text.alignment = TextAnchor.MiddleLeft;
            GuiHelper.PivotToTop(text.rectTransform, y);
        }

        /// <summary>
        /// Draws an always-visible description below the field (skipped entirely when
        /// <paramref name="description"/> is null/empty) and returns the height it actually needs
        /// once wrapped at this row's width - measured via <see cref="Text.preferredHeight"/>
        /// (which computes the wrapped-text-generator height against the Text's current rect width
        /// immediately, no layout-pass/frame delay needed) so the row cursor can advance past
        /// however many lines a long description wraps to, instead of assuming a fixed height.
        /// </summary>
        private float CreateDescription(string description, float topY)
        {
            if (string.IsNullOrEmpty(description))
                return 0f;

            Text text = GuiHelper.CreateCardDescription(m_parent, description, topY, m_width, height: 18f);
            return text.preferredHeight;
        }

        /// <summary>Advances the row cursor past a field row of <paramref name="fieldBottomY"/> plus however tall its description measured.</summary>
        private void FinishRow(float fieldBottomY, float descriptionHeight)
        {
            float afterDescriptionY = descriptionHeight > 0f ? fieldBottomY - descriptionHeight : fieldBottomY;
            m_y = afterDescriptionY - RedeemWizard.RowGap;
        }

        /// <summary>Advances the row cursor and draws the row's label + description without building a field - for a custom widget the caller builds itself (e.g. a multi-part row). No reset button, since there's no single field value here to reset.</summary>
        public float LabelOnlyRow(string label, string description)
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);
            float descHeight = CreateDescription(description, fieldY);
            FinishRow(fieldY, descHeight);
            return fieldY;
        }

        public InputField TextRow(string label, string description, string value, string placeholder, Action<string> onChanged, string defaultValue = "")
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_width, inset: 0f);
            InputField field = GuiFieldBuilder.CreateInputField(m_parent, new Vector2(fieldCenterX, fieldY), fieldWidth, value ?? "", placeholder ?? "");
            field.onValueChanged.AddListener(v => onChanged?.Invoke(v));
            GuiHelper.PivotToTop((RectTransform)field.transform, fieldY);
            GameObject resetBtn = GuiHelper.CreateResetButton(m_parent, resetCenterX, fieldY, () => field.text = defaultValue ?? "");
            GuiHelper.PivotToTop((RectTransform)resetBtn.transform, fieldY);

            float descHeight = CreateDescription(description, fieldY - GuiFieldBuilder.FieldHeight);
            FinishRow(fieldY - GuiFieldBuilder.FieldHeight, descHeight);
            return field;
        }

        public InputField IntRow(string label, string description, int value, Action<int> onChanged, int defaultValue = 0, int maxLength = 9)
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_width, inset: 0f);
            InputField field = GuiFieldBuilder.CreateIntField(m_parent, new Vector2(fieldCenterX, fieldY), fieldWidth, value, onChanged, emptyAsZero: true, maxLength: maxLength);
            GuiHelper.PivotToTop((RectTransform)field.transform, fieldY);
            GameObject resetBtn = GuiHelper.CreateResetButton(m_parent, resetCenterX, fieldY, () => field.text = defaultValue.ToString());
            GuiHelper.PivotToTop((RectTransform)resetBtn.transform, fieldY);

            float descHeight = CreateDescription(description, fieldY - GuiFieldBuilder.FieldHeight);
            FinishRow(fieldY - GuiFieldBuilder.FieldHeight, descHeight);
            return field;
        }

        public InputField FloatRow(string label, string description, float value, Action<float> onChanged, float defaultValue = 0f)
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_width, inset: 0f);
            InputField field = GuiFieldBuilder.CreateFloatField(m_parent, new Vector2(fieldCenterX, fieldY), fieldWidth, value, onChanged);
            GuiHelper.PivotToTop((RectTransform)field.transform, fieldY);
            GameObject resetBtn = GuiHelper.CreateResetButton(m_parent, resetCenterX, fieldY, () => field.text = defaultValue.ToString("G"));
            GuiHelper.PivotToTop((RectTransform)resetBtn.transform, fieldY);

            float descHeight = CreateDescription(description, fieldY - GuiFieldBuilder.FieldHeight);
            FinishRow(fieldY - GuiFieldBuilder.FieldHeight, descHeight);
            return field;
        }

        public Toggle ToggleRow(string label, string description, bool value, Action<bool> onChanged, bool defaultValue = false)
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_width, inset: 0f);
            Toggle toggle = GuiFieldBuilder.CreateBoolField(m_parent, new Vector2(fieldCenterX, fieldY), fieldWidth, value, onChanged);
            GuiHelper.PivotToTop((RectTransform)toggle.transform, fieldY);
            // Toggle.isOn is a no-op when already equal to the target value, so this only fires the
            // listener above (and persists) when a reset actually changes anything.
            GameObject resetBtn = GuiHelper.CreateResetButton(m_parent, resetCenterX, fieldY, () => toggle.isOn = defaultValue);
            GuiHelper.PivotToTop((RectTransform)resetBtn.transform, fieldY);

            float descHeight = CreateDescription(description, fieldY - GuiFieldBuilder.FieldHeight);
            FinishRow(fieldY - GuiFieldBuilder.FieldHeight, descHeight);
            return toggle;
        }

        public GameObject ColorRow(string label, string description, string hexValue, Action<string> onChanged, string defaultValue = "#ffffff")
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_width, inset: 0f);
            GameObject swatch = GuiFieldBuilder.CreateColorField(m_parent, new Vector2(fieldCenterX, fieldY), fieldWidth, hexValue, label, onChanged);
            GuiHelper.PivotToTop((RectTransform)swatch.transform, fieldY);
            GameObject resetBtn = GuiHelper.CreateResetButton(m_parent, resetCenterX, fieldY, () =>
            {
                // CreateColorField's swatch has no exposed "set color" API (only the picker click
                // callback repaints it) - reach the same overlay Image the picker paints on directly.
                Image overlay = swatch.transform.Find("ColorOverlay").GetComponent<Image>();
                if (ColorUtility.TryParseHtmlString(defaultValue, out Color c))
                    overlay.color = c;
                onChanged?.Invoke(defaultValue);
            });
            GuiHelper.PivotToTop((RectTransform)resetBtn.transform, fieldY);

            float descHeight = CreateDescription(description, fieldY - GuiFieldBuilder.FieldHeight);
            FinishRow(fieldY - GuiFieldBuilder.FieldHeight, descHeight);
            return swatch;
        }

        public SearchableDropdown DropdownRow(string label, string description, List<DropdownOption> options, string value, Action<string> onChanged, string defaultValue = null, bool showSearch = true)
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_width, inset: 0f);
            SearchableDropdown dropdown = new SearchableDropdown();
            GameObject dropdownObj = dropdown.Build(m_parent, new Vector2(fieldCenterX, fieldY), fieldWidth, GuiFieldBuilder.FieldHeight, options, value, showSearch);
            GuiHelper.PivotToTop((RectTransform)dropdownObj.transform, fieldY);
            dropdown.OnValueChanged += v => onChanged?.Invoke(v);
            GameObject resetBtn = GuiHelper.CreateResetButton(m_parent, resetCenterX, fieldY, () =>
            {
                // Value's setter only repaints the toggle label, it doesn't raise OnValueChanged
                // (see SearchableDropdown.cs) - invoke the callback explicitly so a reset actually
                // writes back into the model, not just the displayed text.
                dropdown.Value = defaultValue;
                onChanged?.Invoke(defaultValue);
            });
            GuiHelper.PivotToTop((RectTransform)resetBtn.transform, fieldY);

            float descHeight = CreateDescription(description, fieldY - GuiFieldBuilder.FieldHeight);
            FinishRow(fieldY - GuiFieldBuilder.FieldHeight, descHeight);
            return dropdown;
        }

        public SearchableChecklist ChecklistRow(string label, string description, List<DropdownOption> options, List<string> values, Action<List<string>> onChanged, List<string> defaultValues = null)
        {
            float labelY = m_y;
            float fieldY = labelY - RedeemWizard.LabelHeight;
            CreateLabel(label, labelY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_width, inset: 0f);
            SearchableChecklist checklist = new SearchableChecklist();
            GameObject checklistObj = checklist.Build(m_parent, new Vector2(fieldCenterX, fieldY), fieldWidth, GuiFieldBuilder.FieldHeight, options, values);
            GuiHelper.PivotToTop((RectTransform)checklistObj.transform, fieldY);
            checklist.OnSelectionChanged += v => onChanged?.Invoke(v);
            GameObject resetBtn = GuiHelper.CreateResetButton(m_parent, resetCenterX, fieldY, () =>
            {
                List<string> resetValues = defaultValues != null ? new List<string>(defaultValues) : new List<string>();
                // SetOptions doesn't raise OnSelectionChanged either (mirrors SearchableDropdown) -
                // invoke the callback explicitly so the reset writes back into the model.
                checklist.SetOptions(options, resetValues);
                onChanged?.Invoke(resetValues);
            });
            GuiHelper.PivotToTop((RectTransform)resetBtn.transform, fieldY);

            float descHeight = CreateDescription(description, fieldY - GuiFieldBuilder.FieldHeight);
            FinishRow(fieldY - GuiFieldBuilder.FieldHeight, descHeight);
            return checklist;
        }
    }
}
