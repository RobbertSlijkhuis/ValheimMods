using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Multi-select sibling of <see cref="SearchableDropdown"/>: same toggle-button + floating
    /// search panel shell, but every row carries a checkbox instead of selecting-and-closing, and
    /// the toggle button shows a "N selected" summary instead of one value's label. Copy-adapted
    /// from <see cref="SearchableDropdown"/> rather than subclassing it (its fields are private,
    /// and this mirrors how <see cref="SearchableDropdown"/> itself was copy-adapted from
    /// GUI_OLD/editors/SearchableDropdown.cs instead of reusing it directly). Reuses
    /// <see cref="SearchableDropdownHandle"/> for the same "close the floating panel if the owning
    /// toggle is disabled/destroyed out from under it" lifecycle guard.
    /// </summary>
    internal class SearchableChecklist
    {
        private const float SearchInputHeight = 32f;
        private const float OptionItemHeight  = 32f;
        private const float OptionItemSpacing = 2f;
        private const float PanelMaxListHeight = 180f;
        private const float PanelPadding      = 4f;
        private const float PanelGap          = 2f;
        private const float ArrowSize         = 18f;
        private const float ArrowMargin       = 32f;
        // 20px matches the stock toggle Background sprite's native size (see
        // GuiFieldBuilder.CreateBoolField's (10,-10) offset comment) - the previous 16f was sized
        // for a flat color swatch, not the real toggle sprite this now draws.
        private const float CheckboxSize      = 20f;
        private const float CheckboxGap       = 8f;

        public event Action<List<string>> OnSelectionChanged;

        private List<DropdownOption> m_options = new List<DropdownOption>();
        private readonly HashSet<string> m_selected = new HashSet<string>();
        private float m_width;
        private bool m_showSearch = true;

        private GameObject m_toggleObj;
        private RectTransform m_toggleRt;
        private Text m_toggleText;
        private Button m_toggleButton;

        private GameObject m_panelObj;
        private GameObject m_blockerObj;
        private InputField m_searchInput;
        private GameObject m_listContent;
        private List<DropdownOption> m_filteredOptions = new List<DropdownOption>();
        private readonly Dictionary<string, Image> m_rowCheckboxes = new Dictionary<string, Image>();
        private bool m_isOpen;

        /// <summary>Defensive copy of the current selection.</summary>
        public List<string> SelectedValues => new List<string>(m_selected);

        public bool Interactable
        {
            get => m_toggleButton != null && m_toggleButton.interactable;
            set { if (m_toggleButton != null) m_toggleButton.interactable = value; }
        }

        /// <summary>Builds the toggle button at <paramref name="position"/>. Returns the toggle GameObject.</summary>
        public GameObject Build(GameObject parent, Vector2 position, float width, float height, List<DropdownOption> options, List<string> selectedValues, bool showSearch = true)
        {
            m_options = options ?? new List<DropdownOption>();
            m_width   = width;
            m_showSearch = showSearch;
            ReplaceSelection(selectedValues);

            m_toggleObj = GuiHelper.CreateButton(
                text: "",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                width: width,
                height: height
            );
            m_toggleObj.SetActive(true);

            m_toggleRt     = m_toggleObj.GetComponent<RectTransform>();
            m_toggleButton = m_toggleObj.GetComponent<Button>();
            m_toggleText   = m_toggleObj.GetComponentInChildren<Text>();

            // Re-skin from CreateButton's embossed "button" look to the flat dropdown/text-field
            // look real Dropdowns use, same as SearchableDropdown.
            Image toggleImg = m_toggleObj.GetComponent<Image>();
            if (toggleImg != null)
            {
                toggleImg.sprite = GUIManager.Instance.GetSprite("text_field");
                toggleImg.type   = Image.Type.Sliced;
                toggleImg.color  = Color.white;
            }

            m_toggleButton.colors = new ColorBlock
            {
                normalColor      = Color.white,
                highlightedColor = new Color(0.882f, 0.882f, 0.882f, 1f),
                pressedColor     = new Color(0.698f, 0.698f, 0.698f, 1f),
                selectedColor    = new Color(0.882f, 0.882f, 0.882f, 1f),
                disabledColor    = new Color(0.521f, 0.521f, 0.521f, 0.502f),
                colorMultiplier  = 1f,
                fadeDuration     = 0.1f
            };

            if (m_toggleText != null)
            {
                m_toggleText.color     = Color.white;
                m_toggleText.alignment = TextAnchor.MiddleLeft;
                m_toggleText.fontSize  = GuiFieldBuilder.FieldFontSize;

                RectTransform textRt = m_toggleText.GetComponent<RectTransform>();
                textRt.offsetMin = new Vector2(10f, textRt.offsetMin.y);
                textRt.offsetMax = new Vector2(-(ArrowMargin + ArrowSize), textRt.offsetMax.y);
            }

            GameObject arrowObj = new GameObject("Arrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            arrowObj.transform.SetParent(m_toggleObj.transform, false);
            RectTransform arrowRt = arrowObj.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(1f, 0.5f);
            arrowRt.anchorMax = new Vector2(1f, 0.5f);
            arrowRt.pivot     = new Vector2(1f, 0.5f);
            arrowRt.sizeDelta = new Vector2(ArrowSize, ArrowSize);
            arrowRt.anchoredPosition = new Vector2(-ArrowMargin, 0f);
            arrowRt.localRotation    = Quaternion.Euler(0f, 0f, 180f);

            Image arrowImg = arrowObj.GetComponent<Image>();
            arrowImg.sprite        = GUIManager.Instance.GetSprite("map_marker");
            arrowImg.color         = Color.white;
            arrowImg.raycastTarget = false;

            UpdateToggleLabel();

            m_toggleButton.onClick.AddListener(() =>
            {
                if (m_isOpen) Close();
                else Open();
            });

            SearchableDropdownHandle handle = m_toggleObj.AddComponent<SearchableDropdownHandle>();
            handle.ToggleButton = m_toggleButton;
            handle.OnOwnerDeactivated = Close;

            return m_toggleObj;
        }

        /// <summary>Replaces the option list and current selection, closing the panel if it's open.</summary>
        public void SetOptions(List<DropdownOption> options, List<string> selectedValues = null)
        {
            m_options = options ?? new List<DropdownOption>();
            ReplaceSelection(selectedValues);
            UpdateToggleLabel();
            Close();
        }

        private void ReplaceSelection(List<string> selectedValues)
        {
            m_selected.Clear();
            if (selectedValues == null)
                return;

            foreach (string v in selectedValues)
            {
                if (m_options.Any(o => o.Value == v))
                    m_selected.Add(v);
            }
        }

        private void UpdateToggleLabel()
        {
            if (m_toggleText == null)
                return;

            string label = m_selected.Count == 0 ? "None selected" : $"{m_selected.Count} selected";
            float maxWidth = ((RectTransform)m_toggleText.transform).rect.width;
            GuiHelper.SetTruncatedText(m_toggleText, label, maxWidth);
        }

        private void Open()
        {
            if (m_isOpen || m_toggleObj == null)
                return;

            Canvas rootCanvas = GetRootCanvas(m_toggleObj);
            if (rootCanvas == null)
                return;

            m_isOpen = true;

            // Full-screen invisible blocker - closes the panel on click-away, same trick Unity's own Dropdown uses.
            m_blockerObj = new GameObject("SearchableChecklistBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            m_blockerObj.transform.SetParent(rootCanvas.transform, false);
            RectTransform blockerRt = m_blockerObj.GetComponent<RectTransform>();
            blockerRt.anchorMin = Vector2.zero;
            blockerRt.anchorMax = Vector2.one;
            blockerRt.offsetMin = Vector2.zero;
            blockerRt.offsetMax = Vector2.zero;
            Image blockerImg = m_blockerObj.GetComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0f);
            Button blockerBtn = m_blockerObj.GetComponent<Button>();
            blockerBtn.transition = Selectable.Transition.None;
            blockerBtn.onClick.AddListener(Close);

            // Panel - reparented to the root canvas so it draws above and isn't clipped by any
            // ancestor ScrollRect/Mask the toggle button is nested inside.
            m_panelObj = new GameObject("SearchableChecklistPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            m_panelObj.transform.SetParent(rootCanvas.transform, false);
            RectTransform panelRt = m_panelObj.GetComponent<RectTransform>();
            panelRt.pivot     = new Vector2(0f, 1f);
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);

            int rowCount   = Mathf.Max(m_options.Count, 1);
            float listHeight = Mathf.Min(PanelMaxListHeight, rowCount * OptionItemHeight + Mathf.Max(0, rowCount - 1) * OptionItemSpacing);
            float searchBlockHeight = m_showSearch ? SearchInputHeight + PanelPadding : 0f;
            float panelHeight = searchBlockHeight + PanelPadding * 2f + listHeight;
            panelRt.sizeDelta = new Vector2(m_width, panelHeight);

            Image panelBg = m_panelObj.GetComponent<Image>();
            panelBg.sprite = GUIManager.Instance.GetSprite("button_small");
            panelBg.type   = Image.Type.Sliced;
            panelBg.color  = Color.white;

            Vector3[] corners = new Vector3[4];
            m_toggleRt.GetWorldCorners(corners);
            panelRt.position = corners[0] + new Vector3(0f, -PanelGap, 0f);

            float innerWidth = m_width - PanelPadding * 2f;

            if (m_showSearch)
            {
                GameObject inputObj = GUIManager.Instance.CreateInputField(
                    parent: m_panelObj.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -(SearchInputHeight / 2f) - PanelPadding),
                    contentType: InputField.ContentType.Standard,
                    placeholderText: "Search...",
                    fontSize: GuiFieldBuilder.FieldFontSize,
                    width: innerWidth,
                    height: SearchInputHeight
                );
                m_searchInput = inputObj.GetComponent<InputField>();
                GuiFieldBuilder.StylePlaceholder(m_searchInput);
                m_searchInput.text = "";
                m_searchInput.onValueChanged.AddListener(OnFilterChanged);
            }

            float listY = -(searchBlockHeight + PanelPadding);
            m_listContent = ScrollableList.CreateFixed(m_panelObj, "SearchableChecklist", new Vector2(0f, listY), innerWidth, listHeight);

            m_filteredOptions = new List<DropdownOption>(m_options);
            RefreshOptionRows();

            m_searchInput?.ActivateInputField();
        }

        private void Close()
        {
            if (m_panelObj != null) GameObject.Destroy(m_panelObj);
            if (m_blockerObj != null) GameObject.Destroy(m_blockerObj);

            m_panelObj    = null;
            m_blockerObj  = null;
            m_searchInput = null;
            m_listContent = null;
            m_rowCheckboxes.Clear();
            m_isOpen      = false;
        }

        private void OnFilterChanged(string text)
        {
            m_filteredOptions = string.IsNullOrEmpty(text)
                ? new List<DropdownOption>(m_options)
                : m_options.Where(o => o.Label.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            RefreshOptionRows();
        }

        private void RefreshOptionRows()
        {
            if (m_listContent == null)
                return;

            foreach (Transform child in m_listContent.transform)
                GameObject.Destroy(child.gameObject);
            m_rowCheckboxes.Clear();

            float rowWidth = Mathf.Max(0f, m_width - PanelPadding * 2f - 16f); // -16 for the scrollbar
            float yOffset  = -(OptionItemHeight / 2f);

            if (m_filteredOptions.Count == 0)
            {
                GUIManager.Instance.CreateText(
                    text: "No matches",
                    parent: m_listContent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, yOffset),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: GuiFieldBuilder.FieldFontSize,
                    color: new Color(0.6f, 0.6f, 0.6f, 1f),
                    outline: false,
                    outlineColor: Color.black,
                    width: rowWidth,
                    height: OptionItemHeight,
                    addContentSizeFitter: false
                );

                SetListContentHeight(OptionItemHeight);
                return;
            }

            foreach (DropdownOption option in m_filteredOptions)
            {
                string capturedValue = option.Value;

                GameObject rowObj = new GameObject("Option", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                rowObj.transform.SetParent(m_listContent.transform, false);

                RectTransform rowRt = rowObj.GetComponent<RectTransform>();
                rowRt.anchorMin = new Vector2(0.5f, 1f);
                rowRt.anchorMax = new Vector2(0.5f, 1f);
                rowRt.pivot     = new Vector2(0.5f, 1f);
                rowRt.sizeDelta = new Vector2(rowWidth, OptionItemHeight);
                rowRt.anchoredPosition = new Vector2(0f, yOffset + OptionItemHeight / 2f);

                Image rowImg = rowObj.GetComponent<Image>();
                rowImg.color = new Color(1f, 1f, 1f, 0.04f);

                Button rowBtn = rowObj.GetComponent<Button>();
                rowBtn.targetGraphic = rowImg;
                rowBtn.transition    = Selectable.Transition.ColorTint;
                ColorBlock cb = rowBtn.colors;
                cb.normalColor      = new Color(1f, 1f, 1f, 0.04f);
                cb.highlightedColor = new Color(1f, 1f, 1f, 0.18f);
                cb.pressedColor     = new Color(1f, 1f, 1f, 0.3f);
                cb.selectedColor    = cb.highlightedColor;
                rowBtn.colors = cb;
                rowBtn.onClick.AddListener(() => ToggleSelection(capturedValue));

                // Checkbox indicator - plain non-interactive Images (background + checkmark), not a
                // real Toggle, since the row's own Button already owns the click; a nested
                // interactive Toggle would fight it for the click event. Uses the same two sprites
                // GUIManager.CreateToggle builds every other toggle in this UI from
                // (ValheimControlResources.standard/checkmark - see GuiHelper.CreateToggle), so this
                // reads as the game's real toggle graphic rather than a flat color swatch; checked
                // state is conveyed by showing/hiding the checkmark child, matching how a real
                // Toggle shows/hides its own Checkmark graphic.
                GameObject checkboxObj = new GameObject("Checkbox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                checkboxObj.transform.SetParent(rowObj.transform, false);
                RectTransform checkboxRt = checkboxObj.GetComponent<RectTransform>();
                checkboxRt.anchorMin = new Vector2(0f, 0.5f);
                checkboxRt.anchorMax = new Vector2(0f, 0.5f);
                checkboxRt.pivot     = new Vector2(0f, 0.5f);
                checkboxRt.sizeDelta = new Vector2(CheckboxSize, CheckboxSize);
                checkboxRt.anchoredPosition = new Vector2(8f, 0f);
                Image checkboxBg = checkboxObj.GetComponent<Image>();
                checkboxBg.raycastTarget = false;
                checkboxBg.sprite = GUIManager.Instance.ValheimControlResources.standard;
                checkboxBg.type = Image.Type.Sliced;

                GameObject checkmarkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                checkmarkObj.transform.SetParent(checkboxObj.transform, false);
                RectTransform checkmarkRt = checkmarkObj.GetComponent<RectTransform>();
                checkmarkRt.anchorMin = Vector2.zero;
                checkmarkRt.anchorMax = Vector2.one;
                checkmarkRt.offsetMin = Vector2.zero;
                checkmarkRt.offsetMax = Vector2.zero;
                Image checkmarkImg = checkmarkObj.GetComponent<Image>();
                checkmarkImg.raycastTarget = false;
                checkmarkImg.sprite = GUIManager.Instance.ValheimControlResources.checkmark;
                // The checkmark sprite is white; a real Toggle gets tinted by Jötunn's toggle style,
                // but this hand-built Image never is, so tint it orange explicitly.
                checkmarkImg.color = GUIManager.Instance.ValheimOrange;
                m_rowCheckboxes[capturedValue] = checkmarkImg;

                float textWidth = rowWidth - CheckboxSize - CheckboxGap - 10f;
                Text rowText = GUIManager.Instance.CreateText(
                    text: "",
                    parent: rowObj.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(CheckboxSize + CheckboxGap, 0f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: GuiFieldBuilder.FieldFontSize,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: textWidth,
                    height: OptionItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                rowText.alignment    = TextAnchor.MiddleLeft;
                rowText.raycastTarget = false;
                GuiHelper.SetTruncatedText(rowText, option.Label, textWidth);

                yOffset -= OptionItemHeight + OptionItemSpacing;
            }

            SetListContentHeight(Mathf.Max(0f, Mathf.Abs(yOffset)));
            RefreshCheckboxStates();
        }

        private void RefreshCheckboxStates()
        {
            // m_rowCheckboxes holds each row's checkmark Image - show/hide it, mirroring how a
            // real Toggle shows/hides its own Checkmark graphic based on isOn.
            foreach (var kvp in m_rowCheckboxes)
                kvp.Value.enabled = m_selected.Contains(kvp.Key);
        }

        private void ToggleSelection(string value)
        {
            if (!m_selected.Add(value))
                m_selected.Remove(value);

            RefreshCheckboxStates();
            UpdateToggleLabel();
            OnSelectionChanged?.Invoke(SelectedValues);
        }

        private void SetListContentHeight(float height)
        {
            RectTransform contentRt = m_listContent.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, height);
        }

        private static Canvas GetRootCanvas(GameObject obj)
        {
            Canvas[] canvases = obj.GetComponentsInParent<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].isRootCanvas)
                    return canvases[i];
            }
            return canvases.Length > 0 ? canvases[canvases.Length - 1] : null;
        }
    }
}
