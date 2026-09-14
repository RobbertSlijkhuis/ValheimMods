using Jotunn.Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Master-detail editor for a <see cref="System.Collections.Generic.List{T}"/> of
    /// <see cref="CloneableData"/>. A sidebar lists every entry (with Add); the detail panel
    /// on the right renders only the currently selected entry's fields, avoiding the long
    /// single-column scroll of stacking every entry's every field at once.
    /// </summary>
    internal class ObjectListEditor
    {
        internal const float TotalHeight = WidgetHeight;

        private const float WidgetHeight       = 640f;
        private const float SidebarWidth       = 150f;
        private const float SidebarGap         = 12f;
        private const float SidebarBtnHeight   = 36f;
        private const float SidebarBtnSpacing  = 4f;
        private const float DetailHeaderHeight = 36f;
        private const float DetailRemoveBtnWidth = 50f;
        private const float ContentPadding     = 12f;

        // Gap between the top row (Add button / entry header) and the scrollable content
        // below it - shared by both columns so they can't drift out of vertical alignment.
        private const float HeaderContentGap   = 2f;

        private readonly IList   m_list;
        private readonly Type    m_elementType;
        private readonly string  m_label;
        private readonly float   m_startX;
        private readonly float   m_fieldWidth;
        private readonly Func<object, object> m_entryViewFactory;
        private readonly Func<object, string> m_entryLabelFactory;

        private GameObject m_sidebarContent;
        private GameObject m_detailHeader;
        private GameObject m_detailScrollContent;
        private float m_detailViewportWidth;
        private int m_selectedIndex = -1;

        /// <param name="entryViewFactory">
        /// Optional. When set, each list entry is wrapped through this factory (e.g. a narrowed
        /// <c>BoundField&lt;T&gt;</c> view class) before its fields are built, so the entry is
        /// edited through a subset of its fields while writes still land on the real entry.
        /// </param>
        /// <param name="entryLabelFactory">
        /// Optional. Produces the sidebar/header label for an entry (e.g. its prefab name).
        /// Falls back to "Entry N" when null or when it returns null/empty.
        /// </param>
        public ObjectListEditor(IList list, Type elementType, string label, float startX, float fieldWidth,
            Func<object, object> entryViewFactory = null, Func<object, string> entryLabelFactory = null)
        {
            m_list              = list;
            m_elementType       = elementType;
            m_label             = label;
            m_startX            = startX;
            m_fieldWidth        = fieldWidth;
            m_entryViewFactory  = entryViewFactory;
            m_entryLabelFactory = entryLabelFactory;

            if (m_list.Count > 0)
                m_selectedIndex = 0;
        }

        public void Build(GameObject parent, Vector2 rowPosition)
        {
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

            float boxLeft = rowPosition.x - FieldUIBuilder.LabelWidth / 2f - ContentPadding;
            float boxTop  = rowPosition.y - FieldUIBuilder.FieldHeight;

            // --- Sidebar: Add button + scrollable entry list ---
            float sidebarCenterX = boxLeft + SidebarWidth / 2f;

            GameObject addBtn = GUIManager.Instance.CreateButton(
                text: "+ Add",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(sidebarCenterX, boxTop),
                width: SidebarWidth,
                height: SidebarBtnHeight
            );
            addBtn.SetActive(true);
            addBtn.GetComponent<Button>().onClick.AddListener(OnAddEntry);

            float sidebarScrollTop    = boxTop - SidebarBtnHeight - HeaderContentGap;
            float sidebarScrollHeight = WidgetHeight - SidebarBtnHeight - HeaderContentGap;

            m_sidebarContent = ListEditor.CreateScrollableList(
                parent,
                new Vector2(sidebarCenterX, sidebarScrollTop),
                SidebarWidth,
                sidebarScrollHeight,
                "ObjectListSidebar"
            );

            // --- Detail panel: fixed header (title + remove) + scrollable fields ---
            m_detailViewportWidth = FieldUIBuilder.LabelWidth + FieldUIBuilder.LabelFieldGap
                                   + m_fieldWidth + FieldUIBuilder.TooltipGap + FieldUIBuilder.TooltipWidth
                                   + 2f * ContentPadding;
            float detailBoxWidth  = m_detailViewportWidth + 16f; // + scrollbar
            float detailCenterX   = boxLeft + SidebarWidth + SidebarGap + detailBoxWidth / 2f;

            m_detailHeader = new GameObject("DetailHeader", typeof(RectTransform));
            m_detailHeader.transform.SetParent(parent.transform, false);
            RectTransform headerRt = m_detailHeader.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0.5f, 1f);
            headerRt.anchorMax = new Vector2(0.5f, 1f);
            headerRt.pivot     = new Vector2(0.5f, 1f);
            headerRt.sizeDelta = new Vector2(detailBoxWidth, DetailHeaderHeight);
            headerRt.anchoredPosition = new Vector2(detailCenterX, boxTop);

            float detailScrollTop    = boxTop - DetailHeaderHeight - HeaderContentGap;
            float detailScrollHeight = WidgetHeight - DetailHeaderHeight - HeaderContentGap;

            m_detailScrollContent = ListEditor.CreateScrollableList(
                parent,
                new Vector2(detailCenterX, detailScrollTop),
                detailBoxWidth,
                detailScrollHeight,
                "ObjectListDetail"
            );

            RefreshSidebar();
            RefreshDetail();
        }

        private void OnAddEntry()
        {
            m_list.Add(Activator.CreateInstance(m_elementType));
            m_selectedIndex = m_list.Count - 1;
            RefreshSidebar();
            RefreshDetail();
        }

        private void OnRemoveEntry(int index)
        {
            if (index < 0 || index >= m_list.Count)
                return;

            m_list.RemoveAt(index);
            m_selectedIndex = m_list.Count == 0 ? -1 : Mathf.Clamp(index, 0, m_list.Count - 1);
            RefreshSidebar();
            RefreshDetail();
        }

        private void OnSelectEntry(int index)
        {
            m_selectedIndex = index;
            RefreshSidebar();
            RefreshDetail();
        }

        private string GetEntryLabel(object entry, int index)
        {
            string label = m_entryLabelFactory?.Invoke(entry);
            return string.IsNullOrEmpty(label) ? "Entry " + (index + 1) : label;
        }

        private void RefreshSidebar()
        {
            foreach (Transform child in m_sidebarContent.transform)
                GameObject.Destroy(child.gameObject);

            float viewportWidth = SidebarWidth - 16f; // subtract scrollbar
            float yOffset       = SidebarBtnHeight / 2f;

            for (int i = 0; i < m_list.Count; i++)
            {
                int capturedIdx = i;

                GameObject entryBtn = GUIManager.Instance.CreateButton(
                    text: GetEntryLabel(m_list[i], i),
                    parent: m_sidebarContent.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -yOffset),
                    width: viewportWidth,
                    height: SidebarBtnHeight
                );
                entryBtn.SetActive(true);
                entryBtn.GetComponentInChildren<Text>().color = i == m_selectedIndex
                    ? GUIManager.Instance.ValheimOrange
                    : GUIManager.Instance.ValheimBeige;
                entryBtn.GetComponent<Button>().onClick.AddListener(() => OnSelectEntry(capturedIdx));

                yOffset += SidebarBtnHeight + SidebarBtnSpacing;
            }

            RectTransform contentRt = m_sidebarContent.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Max(0f, yOffset - SidebarBtnSpacing));
        }

        private void RefreshDetail()
        {
            foreach (Transform child in m_detailHeader.transform)
                GameObject.Destroy(child.gameObject);
            foreach (Transform child in m_detailScrollContent.transform)
                GameObject.Destroy(child.gameObject);

            if (m_selectedIndex < 0 || m_selectedIndex >= m_list.Count)
            {
                GUIManager.Instance.CreateText(
                    text: "No entries yet — click + Add to create one.",
                    parent: m_detailHeader.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: Vector2.zero,
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: m_detailViewportWidth,
                    height: DetailHeaderHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                return;
            }

            int    index = m_selectedIndex;
            object entry = m_list[index];

            float headerTextWidth = m_detailViewportWidth - DetailRemoveBtnWidth - ContentPadding;
            float headerTextX     = -(m_detailViewportWidth / 2f) + ContentPadding + headerTextWidth / 2f;

            GUIManager.Instance.CreateText(
                text: "Entry " + (index + 1) + " - " + GetEntryLabel(entry, index),
                parent: m_detailHeader.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(headerTextX, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FieldUIBuilder.LabelFontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: headerTextWidth,
                height: DetailHeaderHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            float removeBtnX = m_detailViewportWidth / 2f - ContentPadding - DetailRemoveBtnWidth / 2f;
            GameObject removeBtn = GUIManager.Instance.CreateButton(
                text: "X",
                parent: m_detailHeader.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(removeBtnX, 0f),
                width: DetailRemoveBtnWidth,
                height: DetailHeaderHeight
            );
            removeBtn.SetActive(true);
            removeBtn.GetComponentInChildren<Text>().color = Color.red;
            removeBtn.GetComponent<Button>().onClick.AddListener(() => OnRemoveEntry(index));

            object fieldTarget = m_entryViewFactory != null ? m_entryViewFactory(entry) : entry;

            float innerStartX = -(m_detailViewportWidth / 2f) + ContentPadding + FieldUIBuilder.LabelWidth / 2f;
            var   fieldEditor = new ObjectEditor(innerStartX, 0f, m_fieldWidth);
            float fieldsHeight = fieldEditor.BuildFields(m_detailScrollContent, fieldTarget, -FieldUIBuilder.FieldHeight / 2f, onRebuild: () =>
            {
                RefreshSidebar();
                RefreshDetail();
            });

            RectTransform contentRt = m_detailScrollContent.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Max(0f, fieldsHeight));
        }
    }
}
