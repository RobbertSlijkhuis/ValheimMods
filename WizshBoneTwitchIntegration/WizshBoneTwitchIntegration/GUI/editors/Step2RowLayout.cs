using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Lays out one bordered card per field, top-down, inside a redeem wizard step-2 per-type
    /// form - the same visual shape <see cref="Tabs.SettingsTab"/> uses (<see cref="GuiHelper.CreateCard"/>,
    /// title at <see cref="GuiHelper.CardTitleY"/>, field+reset below, description growing downward
    /// from <see cref="GuiHelper.CardDescriptionTopY"/>) - except each card's height is measured
    /// from its own description length rather than Settings' fixed 120px row height, since step-2
    /// descriptions vary from one clause to several sentences. Construct one fresh instance per
    /// form "page" each time that page is (re)built - this class holds no state beyond the current
    /// Y cursor and pairing scratch state, so there's nothing to reset between builds.
    /// </summary>
    internal class Step2RowLayout
    {
        private const float CardInset = 24f;
        private const float FieldOffsetY = -58f;
        private const float BottomPadding = 14f;
        private const float PairGap = 0f;
        private static readonly float MinCardHeight = -FieldOffsetY + GuiFieldBuilder.FieldHeight / 2f + BottomPadding;

        private readonly GameObject m_parent;
        private readonly float m_width;
        private float m_y;

        // Current cell a *Row method builds its card into - (0, m_width) for a normal full-width
        // row, narrowed to a half-width slot while inside PairRow.
        private float m_cellX;
        private float m_cellWidth;
        private bool m_pairing;
        private readonly List<(GameObject Card, float Height)> m_pairEntries = new List<(GameObject, float)>();

        public Step2RowLayout(GameObject parent, float topY, float width = RedeemWizard.Step2FieldWidth - ScrollableList.ScrollbarWidth)
        {
            m_parent = parent;
            m_width = width;
            m_y = topY;
            m_cellX = 0f;
            m_cellWidth = width;
        }

        /// <summary>Y the next row would start at - lets a caller size a scroll container after laying out all its rows.</summary>
        public float CurrentY => m_y;

        private static void ResizeCard(GameObject card, float height)
        {
            RectTransform rt = (RectTransform)card.transform;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Round(height));
        }

        /// <summary>Creates this row's card (at the current cell) and its title - every <c>*Row</c> method starts here.</summary>
        private GameObject BeginFieldCard(string label, out float fieldY)
        {
            GameObject card = GuiHelper.CreateCard(m_parent, new Vector2(m_cellX, m_y), m_cellWidth, MinCardHeight);
            GuiHelper.CreateCardTitle(card, label, m_cellWidth - CardInset);
            fieldY = FieldOffsetY;
            return card;
        }

        /// <summary>
        /// Draws the description (if any), measures the card's real required height, and either
        /// resizes+advances immediately (normal single row) or - while inside <see cref="PairRow"/> -
        /// records the card for the pair to resize together once both sides are known.
        /// </summary>
        private void EndFieldCard(GameObject card, string description)
        {
            float descHeight = 0f;
            if (!string.IsNullOrEmpty(description))
            {
                Text text = GuiHelper.CreateCardDescription(card, description, m_cellWidth - CardInset);
                descHeight = text.preferredHeight;
            }

            float cardHeight = Mathf.Max(MinCardHeight, -GuiHelper.CardDescriptionTopY + descHeight + BottomPadding);

            if (m_pairing)
            {
                m_pairEntries.Add((card, cardHeight));
                return;
            }

            ResizeCard(card, cardHeight);
            m_y -= cardHeight + RedeemWizard.RowGap;
        }

        /// <summary>
        /// Splits the current row into two half-width cards and runs <paramref name="left"/>/
        /// <paramref name="right"/> into them (each just calls one of this class's own <c>*Row</c>
        /// methods) - both cards end up the shared max height so the row looks level, then the
        /// cursor advances once. Mixed field types (e.g. a float paired with a bool) work fine since
        /// pairing only cares about card geometry, not the field's own type. For a pair where one
        /// side is conditionally hidden (e.g. SpawnCreature's Commandable only when Friendly is on),
        /// the caller should call the plain single-field <c>*Row</c> method instead of <see cref="PairRow"/>
        /// when the condition is false, rather than leaving an empty half - see SpawnCreatureForm.
        /// </summary>
        public void PairRow(Action left, Action right)
        {
            float halfWidth = (m_width - PairGap) / 2f;
            float leftCenterX = -halfWidth / 2f - PairGap / 2f;
            float rightCenterX = halfWidth / 2f + PairGap / 2f;

            m_pairing = true;
            m_pairEntries.Clear();

            m_cellX = leftCenterX;
            m_cellWidth = halfWidth;
            left();

            m_cellX = rightCenterX;
            m_cellWidth = halfWidth;
            right();

            m_pairing = false;
            m_cellX = 0f;
            m_cellWidth = m_width;

            if (m_pairEntries.Count == 0)
                return;

            float maxHeight = 0f;
            foreach ((GameObject _, float height) in m_pairEntries)
                maxHeight = Mathf.Max(maxHeight, height);
            foreach ((GameObject entryCard, float _) in m_pairEntries)
                ResizeCard(entryCard, maxHeight);

            m_y -= maxHeight + RedeemWizard.RowGap;
        }

        public InputField TextRow(string label, string description, string value, string placeholder, Action<string> onChanged, string defaultValue = "")
        {
            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            InputField field = GuiFieldBuilder.CreateInputField(card, new Vector2(fieldCenterX, fieldY), fieldWidth, value ?? "", placeholder ?? "");
            field.onValueChanged.AddListener(v => onChanged?.Invoke(v));
            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () => field.text = defaultValue ?? "");

            EndFieldCard(card, description);
            return field;
        }

        public InputField IntRow(string label, string description, int value, Action<int> onChanged, int defaultValue = 0, int maxLength = 9, int min = GuiFieldBuilder.DefaultMin, int max = GuiFieldBuilder.DefaultMax)
        {
            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            InputField field = GuiFieldBuilder.CreateIntField(card, new Vector2(fieldCenterX, fieldY), fieldWidth, value, onChanged, emptyAsZero: true, maxLength: maxLength, min: min, max: max);
            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () => field.text = defaultValue.ToString());

            EndFieldCard(card, description);
            return field;
        }

        public InputField FloatRow(string label, string description, float value, Action<float> onChanged, float defaultValue = 0f, float min = GuiFieldBuilder.DefaultMin, float max = GuiFieldBuilder.DefaultMax)
        {
            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            InputField field = GuiFieldBuilder.CreateFloatField(card, new Vector2(fieldCenterX, fieldY), fieldWidth, value, onChanged, min: min, max: max);
            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () => field.text = defaultValue.ToString("G"));

            EndFieldCard(card, description);
            return field;
        }

        /// <summary>
        /// Three float fields (X, Y, Z - left to right) sharing one card's field slot, with one Reset
        /// for all three (back to 0). <paramref name="onChanged"/> receives the full vector whenever
        /// any axis changes. The card title/description should name the axis order, as the fields
        /// carry no labels of their own.
        /// </summary>
        public InputField[] Vector3Row(string label, string description, Vector3 value, Action<Vector3> onChanged)
        {
            const float axisGap = 6f;

            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            float axisWidth = (fieldWidth - 2f * axisGap) / 3f;
            float firstCenterX = fieldCenterX - fieldWidth / 2f + axisWidth / 2f;

            Vector3 current = value;
            InputField[] fields = new InputField[3];

            for (int i = 0; i < 3; i++)
            {
                int axis = i;
                fields[i] = GuiFieldBuilder.CreateFloatField(card, new Vector2(firstCenterX + i * (axisWidth + axisGap), fieldY), axisWidth, value[axis], v =>
                {
                    current[axis] = v;
                    onChanged?.Invoke(current);
                });
            }

            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () =>
            {
                foreach (InputField field in fields)
                    field.text = "0";
            });

            EndFieldCard(card, description);
            return fields;
        }

        public Toggle ToggleRow(string label, string description, bool value, Action<bool> onChanged, bool defaultValue = false)
        {
            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            Toggle toggle = GuiFieldBuilder.CreateBoolField(card, new Vector2(fieldCenterX, fieldY), fieldWidth, value, onChanged);
            // Toggle.isOn is a no-op when already equal to the target value, so this only fires the
            // listener above (and persists) when a reset actually changes anything.
            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () => toggle.isOn = defaultValue);

            EndFieldCard(card, description);
            return toggle;
        }

        public GameObject ColorRow(string label, string description, string hexValue, Action<string> onChanged, string defaultValue = "#ffffff")
        {
            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            GameObject swatch = GuiFieldBuilder.CreateColorField(card, new Vector2(fieldCenterX, fieldY), fieldWidth, hexValue, label, onChanged);
            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () =>
            {
                // CreateColorField's swatch has no exposed "set color" API (only the picker click
                // callback repaints it) - reach the same overlay Image the picker paints on directly.
                Image overlay = swatch.transform.Find("ColorOverlay").GetComponent<Image>();
                if (ColorUtility.TryParseHtmlString(defaultValue, out Color c))
                    overlay.color = c;
                onChanged?.Invoke(defaultValue);
            });

            EndFieldCard(card, description);
            return swatch;
        }

        public SearchableDropdown DropdownRow(string label, string description, List<DropdownOption> options, string value, Action<string> onChanged, string defaultValue = null, bool showSearch = true)
        {
            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            SearchableDropdown dropdown = new SearchableDropdown();
            dropdown.Build(card, new Vector2(fieldCenterX, fieldY), fieldWidth, GuiFieldBuilder.FieldHeight, options, value, showSearch);
            dropdown.OnValueChanged += v => onChanged?.Invoke(v);
            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () =>
            {
                // Value's setter only repaints the toggle label, it doesn't raise OnValueChanged
                // (see SearchableDropdown.cs) - invoke the callback explicitly so a reset actually
                // writes back into the model, not just the displayed text.
                dropdown.Value = defaultValue;
                onChanged?.Invoke(defaultValue);
            });

            EndFieldCard(card, description);
            return dropdown;
        }

        public SearchableChecklist ChecklistRow(string label, string description, List<DropdownOption> options, List<string> values, Action<List<string>> onChanged, List<string> defaultValues = null)
        {
            GameObject card = BeginFieldCard(label, out float fieldY);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(m_cellWidth);
            SearchableChecklist checklist = new SearchableChecklist();
            checklist.Build(card, new Vector2(fieldCenterX, fieldY), fieldWidth, GuiFieldBuilder.FieldHeight, options, values);
            checklist.OnSelectionChanged += v => onChanged?.Invoke(v);
            GuiHelper.CreateResetButton(card, resetCenterX, fieldY, () =>
            {
                List<string> resetValues = defaultValues != null ? new List<string>(defaultValues) : new List<string>();
                // SetOptions doesn't raise OnSelectionChanged either (mirrors SearchableDropdown) -
                // invoke the callback explicitly so the reset writes back into the model.
                checklist.SetOptions(options, resetValues);
                onChanged?.Invoke(resetValues);
            });

            EndFieldCard(card, description);
            return checklist;
        }
    }
}
