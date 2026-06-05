using Jotunn.Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class ObjectListEditor
    {
        private const float AddButtonHeight = 36f;
        private const float AddButtonGap    = 10f;
        private const float ScrollMaxHeight = 300f;
        private const float EntryHeaderHeight = 36f;
        private const float EntryFieldSpacing  = 8f;
        private const float EntryBlockSpacing  = 12f;

        private readonly IList  m_list;
        private readonly Type   m_elementType;
        private readonly float  m_startX;
        private readonly float  m_fieldWidth;

        private GameObject      m_scrollContent;

        public ObjectListEditor(IList list, Type elementType, float startX, float fieldWidth)
        {
            m_list        = list;
            m_elementType = elementType;
            m_startX      = startX;
            m_fieldWidth  = fieldWidth;
        }

        /// <summary>
        /// Builds the list editor at <paramref name="startY"/> inside <paramref name="parent"/>.
        /// Returns the total height consumed.
        /// </summary>
        public float Build(GameObject parent, float startY)
        {
            float yOffset = startY;

            // "Add" button
            float addBtnCenterX = m_startX + FieldUIBuilder.LabelWidth / 2f + FieldUIBuilder.LabelFieldGap + m_fieldWidth / 2f;
            GameObject addBtn = GUIManager.Instance.CreateButton(
                text: "+ Add",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(addBtnCenterX, yOffset - AddButtonHeight / 2f),
                width: m_fieldWidth,
                height: AddButtonHeight
            );
            addBtn.SetActive(true);
            addBtn.GetComponent<Button>().onClick.AddListener(OnAddEntry);

            yOffset -= AddButtonHeight + AddButtonGap;

            // ScrollRect
            m_scrollContent = CreateScrollableList(parent, new Vector2(addBtnCenterX, yOffset), 600);
            RefreshList();

            return AddButtonHeight + AddButtonGap + ScrollMaxHeight;
        }

        private void OnAddEntry()
        {
            m_list.Add(Activator.CreateInstance(m_elementType));
            RefreshList();
        }

        private void OnRemoveEntry(int index)
        {
            if (index >= 0 && index < m_list.Count)
            {
                m_list.RemoveAt(index);
                RefreshList();
            }
        }

        private void RefreshList()
        {
            foreach (Transform child in m_scrollContent.transform)
                GameObject.Destroy(child.gameObject);

            float contentWidth  = m_fieldWidth - 16f; // subtract scrollbar width
            float yOffset       = 0f;

            for (int i = 0; i < m_list.Count; i++)
            {
                object entry       = m_list[i];
                int    capturedIdx = i;

                // Entry header: "Entry N" label + "X" remove button
                float headerY = -(yOffset + EntryHeaderHeight / 2f);

                float labelCenterX = -contentWidth / 2f + FieldUIBuilder.LabelWidth / 2f;
                GUIManager.Instance.CreateText(
                    text: "Entry " + (i + 1),
                    parent: m_scrollContent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(labelCenterX, headerY),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: FieldUIBuilder.LabelWidth,
                    height: EntryHeaderHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

                float removeBtnCenterX = contentWidth / 2f - 30f;
                GameObject removeBtn = GUIManager.Instance.CreateButton(
                    text: "X",
                    parent: m_scrollContent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(removeBtnCenterX, headerY),
                    width: 50f,
                    height: EntryHeaderHeight
                );
                removeBtn.SetActive(true);
                removeBtn.GetComponentInChildren<Text>().color = Color.red;
                removeBtn.GetComponent<Button>().onClick.AddListener(() => OnRemoveEntry(capturedIdx));

                yOffset += EntryHeaderHeight + EntryFieldSpacing;

                // Entry fields rendered into the scroll content at the current yOffset
                var fieldEditor = new ObjectEditor(m_startX, 0f, m_fieldWidth);
                float fieldsHeight = fieldEditor.BuildFields(m_scrollContent, entry, -(yOffset));
                yOffset += fieldsHeight + EntryBlockSpacing;
            }

            // Resize content so ScrollRect knows the total height
            RectTransform contentRt = m_scrollContent.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Max(0f, yOffset));
        }

        private GameObject CreateScrollableList(GameObject parent, Vector2 position, float fieldWidth)
        {
            GameObject scrollRoot = new GameObject("ObjectListScrollView");
            scrollRoot.transform.SetParent(parent.transform, false);

            RectTransform scrollRt       = scrollRoot.AddComponent<RectTransform>();
            scrollRt.anchorMin           = new Vector2(0.5f, 1f);
            scrollRt.anchorMax           = new Vector2(0.5f, 1f);
            scrollRt.pivot               = new Vector2(0.5f, 1f);
            scrollRt.sizeDelta           = new Vector2(fieldWidth, ScrollMaxHeight);
            scrollRt.anchoredPosition    = position;

            scrollRoot.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            ScrollRect scrollRect            = scrollRoot.AddComponent<ScrollRect>();
            scrollRect.horizontal            = false;
            scrollRect.vertical              = true;
            scrollRect.scrollSensitivity     = 30f;
            scrollRect.movementType          = ScrollRect.MovementType.Clamped;

            GameObject viewport              = new GameObject("Viewport");
            viewport.transform.SetParent(scrollRoot.transform, false);
            RectTransform viewportRt         = viewport.AddComponent<RectTransform>();
            viewportRt.anchorMin             = Vector2.zero;
            viewportRt.anchorMax             = Vector2.one;
            viewportRt.offsetMin             = Vector2.zero;
            viewportRt.offsetMax             = new Vector2(-16f, 0f);
            viewport.AddComponent<Image>().color           = new Color(0f, 0f, 0f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic  = false;
            scrollRect.viewport              = viewportRt;

            GameObject content               = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRt          = content.AddComponent<RectTransform>();
            contentRt.anchorMin              = new Vector2(0f, 1f);
            contentRt.anchorMax              = new Vector2(1f, 1f);
            contentRt.pivot                  = new Vector2(0.5f, 1f);
            contentRt.sizeDelta              = Vector2.zero;
            contentRt.anchoredPosition       = Vector2.zero;
            scrollRect.content               = contentRt;

            // Scrollbar
            GameObject scrollbarObj          = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(scrollRoot.transform, false);
            RectTransform scrollbarRt        = scrollbarObj.AddComponent<RectTransform>();
            scrollbarRt.anchorMin            = new Vector2(1f, 0f);
            scrollbarRt.anchorMax            = new Vector2(1f, 1f);
            scrollbarRt.pivot                = new Vector2(1f, 0.5f);
            scrollbarRt.sizeDelta            = new Vector2(16f, 0f);
            scrollbarRt.anchoredPosition     = Vector2.zero;
            scrollbarObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Scrollbar scrollbar              = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction              = Scrollbar.Direction.BottomToTop;

            GameObject handleObj             = new GameObject("Handle");
            handleObj.transform.SetParent(scrollbarObj.transform, false);
            RectTransform handleRt           = handleObj.AddComponent<RectTransform>();
            handleRt.anchorMin               = Vector2.zero;
            handleRt.anchorMax               = Vector2.one;
            handleRt.sizeDelta               = Vector2.zero;
            Image handleImage                = handleObj.AddComponent<Image>();
            handleImage.color                = new Color(0.6f, 0.6f, 0.6f, 1f);
            scrollbar.handleRect             = handleRt;
            scrollbar.targetGraphic          = handleImage;

            scrollRect.verticalScrollbar           = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            return content;
        }
    }
}
