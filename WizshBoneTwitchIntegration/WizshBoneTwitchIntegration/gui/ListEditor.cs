using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Renders a <see cref="List{T}"/> of <see cref="string"/> with an input field (or dropdown), Add button,
    /// and a scrollable list of items with Remove buttons.
    /// </summary>
    internal class ListEditor
    {
        private const float InputHeight   = 36f;
        private const float ButtonWidth   = 60f;
        private const float ButtonHeight  = 36f;
        private const float ItemHeight    = 40f;
        private const float ItemSpacing   = 5f;
        private const float ListMaxHeight = 150f;
        private const float LeftPadding   = 8f;

        private readonly List<string> m_targetList;
        private readonly List<string> m_dropdownOptions;
        private GameObject m_itemContainer;
        private InputField m_inputField;
        private Dropdown m_dropdown;
        private bool m_useDropdown;
        private float m_fieldWidth;

        public ListEditor(List<string> targetList)
        {
            m_targetList      = targetList;
            m_dropdownOptions = null;
            m_useDropdown     = false;
        }

        public ListEditor(List<string> targetList, List<string> dropdownOptions)
        {
            m_targetList      = targetList;
            m_dropdownOptions = dropdownOptions;
            m_useDropdown     = dropdownOptions != null && dropdownOptions.Count > 0;
        }

        /// <summary>
        /// Builds the list editor UI starting at <paramref name="startPosition"/>.
        /// <paramref name="fieldWidth"/> controls the width of the input and scroll view,
        /// matching the width used by all other field types.
        /// Returns the total height consumed.
        /// </summary>
        public float Build(GameObject parent, Vector2 startPosition, float fieldWidth)
        {
            m_fieldWidth  = fieldWidth;
            float yOffset = startPosition.y;

            float inputWidth   = fieldWidth - ButtonWidth - 5f;
            float inputCenterX = startPosition.x - fieldWidth / 2f + inputWidth / 2f;
            float btnCenterX   = startPosition.x + fieldWidth / 2f - ButtonWidth / 2f;

            if (m_useDropdown)
            {
                GameObject dropdownObj = GUIManager.Instance.CreateDropDown(
                    parent: parent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(inputCenterX, yOffset),
                    fontSize: FieldUIBuilder.FieldFontSize,
                    width: inputWidth,
                    height: InputHeight
                );
                m_dropdown = dropdownObj.GetComponent<Dropdown>();
                FieldUIBuilder.FixDropdownItemHeight(m_dropdown, FieldUIBuilder.InputHeight);
                m_dropdown.ClearOptions();
                m_dropdown.AddOptions(m_dropdownOptions);
                if (m_dropdownOptions.Count > 0)
                {
                    m_dropdown.value = 0;
                    m_dropdown.RefreshShownValue();
                }
            }
            else
            {
                GameObject inputObj = GUIManager.Instance.CreateInputField(
                    parent: parent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(inputCenterX, yOffset),
                    contentType: InputField.ContentType.Standard,
                    placeholderText: "Enter item...",
                    fontSize: FieldUIBuilder.FieldFontSize,
                    width: inputWidth,
                    height: InputHeight
                );
                m_inputField = inputObj.GetComponent<InputField>();
            }

            // Add button - inline with the input field, always visible
            GameObject addBtn = GUIManager.Instance.CreateButton(
                text: "Add",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(btnCenterX, yOffset),
                width: ButtonWidth,
                height: ButtonHeight
            );
            addBtn.SetActive(true);
            addBtn.GetComponent<Button>().onClick.AddListener(OnAddItem);

            yOffset -= InputHeight + 10f;

            // Scroll view - below the input row, full width
            m_itemContainer = CreateScrollableList(parent, new Vector2(startPosition.x, yOffset), fieldWidth, ListMaxHeight, "ListScrollView");
            RefreshItemList();

            return InputHeight + 10f + ListMaxHeight;
        }

        private void OnAddItem()
        {
            string text;

            if (m_useDropdown)
            {
                if (m_dropdown.value < 0 || m_dropdown.value >= m_dropdownOptions.Count)
                    return;
                text = m_dropdownOptions[m_dropdown.value];
            }
            else
            {
                text = m_inputField?.text.Trim();
                if (string.IsNullOrEmpty(text))
                    return;
            }

            // Avoid adding duplicates
            if (m_targetList.Contains(text))
                return;

            m_targetList.Add(text);

            if (!m_useDropdown && m_inputField != null)
                m_inputField.text = "";

            RefreshItemList();
        }

        private void RefreshItemList()
        {
            ClearContainer(m_itemContainer);

            // Content width is the viewport width - scrollbar takes 16px off the right
            float contentWidth  = m_fieldWidth - 16f;
            float itemTextWidth = contentWidth - ButtonWidth - 10f - LeftPadding;
            float textCenterX   = -contentWidth / 2f + LeftPadding + itemTextWidth / 2f;
            float btnCenterX    = contentWidth / 2f - ButtonWidth / 2f;

            float yOffset = -(ItemHeight / 2f);

            for (int i = m_targetList.Count - 1; i >= 0; i--)
            {
                int capturedIndex = i;
                string item = m_targetList[i];

                Text itemText = GUIManager.Instance.CreateText(
                    text: item,
                    parent: m_itemContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(textCenterX, yOffset),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.FieldFontSize,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: itemTextWidth,
                    height: ItemHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                itemText.alignment = TextAnchor.MiddleLeft;

                GameObject removeBtn = GUIManager.Instance.CreateButton(
                    text: "X",
                    parent: m_itemContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(btnCenterX, yOffset),
                    width: ButtonWidth,
                    height: ItemHeight
                );
                removeBtn.SetActive(true);
                removeBtn.GetComponentInChildren<Text>().color = Color.red;
                removeBtn.GetComponent<Button>().onClick.AddListener(() => OnRemoveItem(capturedIndex));

                yOffset -= ItemHeight + ItemSpacing;
            }

            RectTransform contentRt = m_itemContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Max(0f, Mathf.Abs(yOffset)));
        }

        private void OnRemoveItem(int index)
        {
            if (index >= 0 && index < m_targetList.Count)
            {
                m_targetList.RemoveAt(index);
                RefreshItemList();
            }
        }

        internal static GameObject CreateScrollableList(GameObject parent, Vector2 position, float width, float height, string name = "ScrollView")
        {
            float fieldWidth = width;
            float ListMaxHeight = height;
            GameObject scrollRoot = new GameObject(name);
            scrollRoot.transform.SetParent(parent.transform, false);

            RectTransform scrollRt = scrollRoot.AddComponent<RectTransform>();
            scrollRt.anchorMin        = new Vector2(0.5f, 1f);
            scrollRt.anchorMax        = new Vector2(0.5f, 1f);
            scrollRt.pivot            = new Vector2(0.5f, 1f);
            scrollRt.sizeDelta        = new Vector2(fieldWidth, ListMaxHeight);
            scrollRt.anchoredPosition = position;

            scrollRoot.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            ScrollRect scrollRect = scrollRoot.AddComponent<ScrollRect>();
            scrollRect.horizontal        = false;
            scrollRect.vertical          = true;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType      = ScrollRect.MovementType.Clamped;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollRoot.transform, false);
            RectTransform viewportRt = viewport.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = new Vector2(-16f, 0f);

            viewport.AddComponent<Image>().color         = new Color(0f, 0f, 0f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = viewportRt;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin        = new Vector2(0f, 1f);
            contentRt.anchorMax        = new Vector2(1f, 1f);
            contentRt.pivot            = new Vector2(0.5f, 1f);
            contentRt.sizeDelta        = Vector2.zero;
            contentRt.anchoredPosition = Vector2.zero;

            scrollRect.content = contentRt;

            GameObject scrollbarObj = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(scrollRoot.transform, false);
            RectTransform scrollbarRt = scrollbarObj.AddComponent<RectTransform>();
            scrollbarRt.anchorMin        = new Vector2(1f, 0f);
            scrollbarRt.anchorMax        = new Vector2(1f, 1f);
            scrollbarRt.pivot            = new Vector2(1f, 0.5f);
            scrollbarRt.sizeDelta        = new Vector2(16f, 0f);
            scrollbarRt.anchoredPosition = Vector2.zero;

            scrollbarObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(scrollbarObj.transform, false);
            RectTransform handleRt = handleObj.AddComponent<RectTransform>();
            handleRt.anchorMin = Vector2.zero;
            handleRt.anchorMax = Vector2.one;
            handleRt.sizeDelta = Vector2.zero;

            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            scrollbar.handleRect    = handleRt;
            scrollbar.targetGraphic = handleImage;

            scrollRect.verticalScrollbar           = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            return content;
        }

        private void ClearContainer(GameObject container)
        {
            foreach (Transform child in container.transform)
                GameObject.Destroy(child.gameObject);
        }
    }
}