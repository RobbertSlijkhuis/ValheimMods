using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// One selectable entry in a <see cref="SearchableDropdown"/>: <see cref="Value"/> is what
    /// gets stored/returned (e.g. a prefab name), <see cref="Label"/> is what's displayed and
    /// searched (e.g. a localized display name). A plain <c>List&lt;string&gt;</c> option list
    /// is equivalent to using the same string for both.
    /// </summary>
    internal readonly struct DropdownOption
    {
        public readonly string Value;
        public readonly string Label;

        public DropdownOption(string value, string label)
        {
            Value = value;
            Label = label;
        }
    }

    /// <summary>
    /// A dropdown-style field builder: a toggle button that, when clicked, opens a floating
    /// panel containing a search box and a scrollable, live-filtered list of options. Selecting
    /// a row sets <see cref="Value"/>, fires <see cref="OnValueChanged"/>, and closes the panel.
    /// The floating panel is reparented to the root <see cref="Canvas"/> so it renders above
    /// (and is never clipped by) any ScrollRect/Mask the toggle button itself lives inside,
    /// mirroring how Unity's built-in <see cref="Dropdown"/> escapes its own template masking.
    /// </summary>
    internal class SearchableDropdown
    {
        private const float SearchInputHeight = 32f;
        private const float OptionItemHeight  = 32f;
        private const float OptionItemSpacing = 2f;
        private const float PanelMaxListHeight = 180f;
        private const float PanelPadding      = 4f;
        private const float PanelGap          = 2f;
        private const float ArrowSize         = 18f;
        private const float ArrowMargin       = 32f;

        public event Action<string> OnValueChanged;

        private List<DropdownOption> m_options = new List<DropdownOption>();
        private string m_value = "";
        private float m_width;

        private GameObject m_toggleObj;
        private RectTransform m_toggleRt;
        private Text m_toggleText;
        private Button m_toggleButton;

        private GameObject m_panelObj;
        private GameObject m_blockerObj;
        private InputField m_searchInput;
        private GameObject m_listContent;
        private List<DropdownOption> m_filteredOptions = new List<DropdownOption>();
        private bool m_isOpen;

        public string Value
        {
            get => m_value;
            set
            {
                string desired = value ?? "";
                m_value = (m_options.Count == 0 || m_options.Any(o => o.Value == desired)) ? desired : m_options[0].Value;
                UpdateToggleLabel();
            }
        }

        public bool Interactable
        {
            get => m_toggleButton != null && m_toggleButton.interactable;
            set { if (m_toggleButton != null) m_toggleButton.interactable = value; }
        }

        /// <summary>Builds the toggle button at <paramref name="position"/>. Returns the toggle GameObject.</summary>
        public GameObject Build(GameObject parent, Vector2 position, float width, float height, List<string> options, string currentValue)
        {
            return Build(parent, position, width, height, ToOptions(options), currentValue);
        }

        /// <summary>Builds the toggle button at <paramref name="position"/>. Returns the toggle GameObject.</summary>
        public GameObject Build(GameObject parent, Vector2 position, float width, float height, List<DropdownOption> options, string currentValue)
        {
            m_options = options ?? new List<DropdownOption>();
            m_width   = width;
            m_value   = (!string.IsNullOrEmpty(currentValue) && m_options.Any(o => o.Value == currentValue))
                ? currentValue
                : (m_options.Count > 0 ? m_options[0].Value : "");

            m_toggleObj = GUIManager.Instance.CreateButton(
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
            // look real Dropdowns use, and add the same arrow glyph GUIManager.ApplyDropdownStyle uses.
            Image toggleImg = m_toggleObj.GetComponent<Image>();
            if (toggleImg != null)
            {
                toggleImg.sprite = GUIManager.Instance.GetSprite("text_field");
                toggleImg.type   = Image.Type.Sliced;
                toggleImg.color  = Color.white;
            }

            // CreateButton leaves ApplyButtonStyle's dramatic overbright hover tint (ValheimButtonColorBlock)
            // in place, which reads as a "button glow" against the flat field sprite above. InputFields never
            // get their .colors touched by GUIManager, so they keep Unity's own subtle default transition -
            // replicate that here so the dropdown hovers like the input fields around it.
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
                m_toggleText.fontSize  = FieldUIBuilder.FieldFontSize;

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

        /// <summary>Replaces the option list and current value, closing the panel if it's open.</summary>
        public void SetOptions(List<string> options, string selectedValue = null)
        {
            SetOptions(ToOptions(options), selectedValue);
        }

        /// <summary>Replaces the option list and current value, closing the panel if it's open.</summary>
        public void SetOptions(List<DropdownOption> options, string selectedValue = null)
        {
            m_options = options ?? new List<DropdownOption>();
            Value = (selectedValue != null && m_options.Any(o => o.Value == selectedValue))
                ? selectedValue
                : (m_options.Count > 0 ? m_options[0].Value : "");
            Close();
        }

        private static List<DropdownOption> ToOptions(List<string> values)
        {
            return (values ?? new List<string>()).Select(v => new DropdownOption(v, v)).ToList();
        }

        private void UpdateToggleLabel()
        {
            if (m_toggleText == null)
                return;

            DropdownOption match = m_options.FirstOrDefault(o => o.Value == m_value);
            m_toggleText.text = match.Value == m_value ? match.Label : (m_value ?? "");
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
            m_blockerObj = new GameObject("SearchableDropdownBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
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
            m_panelObj = new GameObject("SearchableDropdownPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            m_panelObj.transform.SetParent(rootCanvas.transform, false);
            RectTransform panelRt = m_panelObj.GetComponent<RectTransform>();
            panelRt.pivot     = new Vector2(0f, 1f);
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);

            int rowCount   = Mathf.Max(m_options.Count, 1);
            float listHeight = Mathf.Min(PanelMaxListHeight, rowCount * OptionItemHeight + Mathf.Max(0, rowCount - 1) * OptionItemSpacing);
            float panelHeight = SearchInputHeight + PanelPadding * 3f + listHeight;
            panelRt.sizeDelta = new Vector2(m_width, panelHeight);

            // Same panel sprite GUIManager.ApplyDropdownStyle uses for a Dropdown's template background.
            Image panelBg = m_panelObj.GetComponent<Image>();
            panelBg.sprite = GUIManager.Instance.GetSprite("button_small");
            panelBg.type   = Image.Type.Sliced;
            panelBg.color  = Color.white;

            // Position the panel directly below the toggle, left-aligned, in world space -
            // this works regardless of any scaling/offsets in between since GetWorldCorners
            // already resolves the toggle's true world position.
            Vector3[] corners = new Vector3[4];
            m_toggleRt.GetWorldCorners(corners);
            panelRt.position = corners[0] + new Vector3(0f, -PanelGap, 0f);

            float innerWidth = m_width - PanelPadding * 2f;

            GameObject inputObj = GUIManager.Instance.CreateInputField(
                parent: m_panelObj.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -(SearchInputHeight / 2f) - PanelPadding),
                contentType: InputField.ContentType.Standard,
                placeholderText: "Search...",
                fontSize: FieldUIBuilder.FieldFontSize,
                width: innerWidth,
                height: SearchInputHeight
            );
            m_searchInput = inputObj.GetComponent<InputField>();
            FieldUIBuilder.StylePlaceholder(m_searchInput);
            m_searchInput.text = "";
            m_searchInput.onValueChanged.AddListener(OnFilterChanged);

            float listY = -(SearchInputHeight + PanelPadding * 2f);
            m_listContent = ListEditor.CreateScrollableList(m_panelObj, new Vector2(0f, listY), innerWidth, listHeight, "SearchableDropdownScroll");

            m_filteredOptions = new List<DropdownOption>(m_options);
            RefreshOptionRows();

            m_searchInput.ActivateInputField();
        }

        private void Close()
        {
            if (m_panelObj != null) GameObject.Destroy(m_panelObj);
            if (m_blockerObj != null) GameObject.Destroy(m_blockerObj);

            m_panelObj    = null;
            m_blockerObj  = null;
            m_searchInput = null;
            m_listContent = null;
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
                    fontSize: FieldUIBuilder.FieldFontSize,
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
                rowBtn.onClick.AddListener(() => Select(capturedValue));

                Text rowText = GUIManager.Instance.CreateText(
                    text: option.Label,
                    parent: rowObj.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: Vector2.zero,
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.FieldFontSize,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: rowWidth - 10f,
                    height: OptionItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                rowText.alignment    = TextAnchor.MiddleLeft;
                rowText.raycastTarget = false;

                yOffset -= OptionItemHeight + OptionItemSpacing;
            }

            SetListContentHeight(Mathf.Max(0f, Mathf.Abs(yOffset)));
        }

        private void SetListContentHeight(float height)
        {
            RectTransform contentRt = m_listContent.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, height);
        }

        private void Select(string option)
        {
            Value = option;
            Close();
            OnValueChanged?.Invoke(option);
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

    /// <summary>
    /// Lives on the toggle button GameObject. Closes the owning <see cref="SearchableDropdown"/>'s
    /// floating panel if the toggle is disabled or destroyed out from under it (e.g. a tab
    /// container is cleared/hidden while the panel is open), since the panel is reparented to
    /// the root canvas and would otherwise be orphaned. Also gives
    /// <c>GetComponentsInChildren&lt;SearchableDropdownHandle&gt;</c> callers (e.g. read-only
    /// mode sweeps) a way to reach the underlying <see cref="Button"/>.
    /// </summary>
    internal class SearchableDropdownHandle : MonoBehaviour
    {
        internal Button ToggleButton;
        internal Action OnOwnerDeactivated;

        internal void SetInteractable(bool interactable)
        {
            if (ToggleButton != null)
                ToggleButton.interactable = interactable;
        }

        private void OnDisable() => OnOwnerDeactivated?.Invoke();
        private void OnDestroy() => OnOwnerDeactivated?.Invoke();
    }
}
