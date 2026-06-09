using Jotunn.Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class ObjectListEditor
    {
        internal const float TotalHeight       = ScrollMaxHeight;
        private const float ScrollMaxHeight    = 300f;
        private const float EntryHeaderHeight     = 36f;
        private const float EntryFieldSpacing     = 24f;
        private const float EntryBlockSpacing     = 16f;
        private const float ContentPadding        = 12f;

        private readonly IList  m_list;
        private readonly Type   m_elementType;
        private readonly string m_label;
        private readonly float  m_startX;
        private readonly float  m_fieldWidth;

        private GameObject m_scrollContent;
        private float      m_scrollWidth;

        public ObjectListEditor(IList list, Type elementType, string label, float startX, float fieldWidth)
        {
            m_list        = list;
            m_elementType = elementType;
            m_label       = label;
            m_startX      = startX;
            m_fieldWidth  = fieldWidth;
        }

        public void Build(GameObject parent, Vector2 rowPosition)
        {
            m_scrollWidth = FieldUIBuilder.LabelWidth + FieldUIBuilder.LabelFieldGap
                          + m_fieldWidth + FieldUIBuilder.TooltipGap + FieldUIBuilder.TooltipWidth
                          + 2f * ContentPadding + 16f;

            // Label — same style as all other field labels, position unchanged
            Text labelComp = GUIManager.Instance.CreateText(
                text: m_label,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: rowPosition,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.LabelFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: FieldUIBuilder.LabelWidth,
                height: FieldUIBuilder.FieldHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            labelComp.alignment = TextAnchor.MiddleLeft;

            // Add button — inline with the label, in the field column position
            float fieldLeftCenterX = rowPosition.x + FieldUIBuilder.LabelWidth / 2f + FieldUIBuilder.LabelFieldGap + m_fieldWidth / 4f;
            GameObject addBtn = GUIManager.Instance.CreateButton(
                text: "+ Add",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(fieldLeftCenterX, rowPosition.y),
                width: m_fieldWidth / 2f,
                height: FieldUIBuilder.FieldHeight
            );

            // Scroll view — full width, left edge aligned with label, directly below the label row
            float scrollLeft    = rowPosition.x - FieldUIBuilder.LabelWidth / 2f - ContentPadding;
            float scrollCenterX = scrollLeft + m_scrollWidth / 2f;
            float scrollY       = rowPosition.y - FieldUIBuilder.FieldHeight;
            m_scrollContent = ListEditor.CreateScrollableList(
                parent,
                new Vector2(scrollCenterX, scrollY),
                m_scrollWidth,
                ScrollMaxHeight,
                "ObjectListScrollView"
            );
            RefreshList();
            addBtn.SetActive(true);
            addBtn.GetComponent<Button>().onClick.AddListener(OnAddEntry);
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

            // Content stretches full viewport width; its center is at x=0 relative to anchor.
            // Offset startX so label aligns to the left edge with a small padding.
            float viewportWidth  = m_scrollWidth - 16f; // subtract scrollbar
            float innerStartX    = -(viewportWidth / 2f) + ContentPadding + FieldUIBuilder.LabelWidth / 2f;
            float yOffset        = 0f;

            for (int i = m_list.Count - 1; i >= 0; i--)
            {
                object entry       = m_list[i];
                int    capturedIdx = i;

                // Entry header: "Entry N" (left) + red X button (right)
                float headerCenterY = -(yOffset + EntryHeaderHeight / 2f);

                GUIManager.Instance.CreateText(
                    text: "Entry " + (i + 1),
                    parent: m_scrollContent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(innerStartX, headerCenterY),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: FieldUIBuilder.LabelWidth,
                    height: EntryHeaderHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

                float removeBtnCenterX = viewportWidth / 2f - ContentPadding - 25f;
                GameObject removeBtn = GUIManager.Instance.CreateButton(
                    text: "X",
                    parent: m_scrollContent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(removeBtnCenterX, headerCenterY),
                    width: 50f,
                    height: EntryHeaderHeight
                );
                removeBtn.SetActive(true);
                removeBtn.GetComponentInChildren<Text>().color = Color.red;
                removeBtn.GetComponent<Button>().onClick.AddListener(() => OnRemoveEntry(capturedIdx));

                yOffset += EntryHeaderHeight + EntryFieldSpacing;

                // Entry fields — no tooltips (would be clipped by the scroll mask)
                var fieldEditor    = new ObjectEditor(innerStartX, 0f, m_fieldWidth);
                float fieldsHeight = fieldEditor.BuildFields(m_scrollContent, entry, -yOffset);
                yOffset += fieldsHeight + EntryBlockSpacing;
            }

            RectTransform contentRt = m_scrollContent.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Max(0f, yOffset));
        }
    }
}
