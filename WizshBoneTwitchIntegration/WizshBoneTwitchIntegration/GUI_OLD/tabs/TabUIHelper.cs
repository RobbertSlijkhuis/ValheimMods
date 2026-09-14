using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Shared UI utility methods used across all settings tabs.
    /// </summary>
    internal static class TabUIHelper
    {
        public const int TitleFontSize = 16;

        /// <summary>
        /// Creates a left-aligned orange tab title label.
        /// </summary>
        public static Text CreateTabTitle(string text, GameObject parent, Vector2 position, float width = 300f)
        {
            Text label = GUIManager.Instance.CreateText(
                text:                text,
                parent:              parent.transform,
                anchorMin:           new Vector2(0.5f, 1f),
                anchorMax:           new Vector2(0.5f, 1f),
                position:            position,
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            TitleFontSize,
                color:               GUIManager.Instance.ValheimOrange,
                outline:             true,
                outlineColor:        Color.black,
                width:               width,
                height:              25f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            return label;
        }

        /// <summary>
        /// Destroys all children of a container <see cref="GameObject"/>.
        /// </summary>
        public static void ClearContainer(GameObject container)
        {
            foreach (Transform child in container.transform)
                GameObject.Destroy(child.gameObject);
        }

        /// <summary>
        /// Adds a thin colored outline around a button's background image, matching its label
        /// color - GUIManager.Instance.CreateButton's root GameObject carries both the Button and
        /// its Image directly, so Outline (a Shadow variant that draws 4 diagonally-offset copies)
        /// applied there reads as a border around the button.
        /// </summary>
        public static void AddBorder(GameObject buttonObj, Color color)
        {
            Outline outline = buttonObj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(2f, 2f);
        }

        /// <summary>
        /// Creates a top-center anchored invisible container used to hold editor fields
        /// inside a scroll view content object.
        /// </summary>
        public static GameObject CreateStaticContainer(string name, GameObject parent, float yPos)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 1f);
            rt.anchorMax        = new Vector2(0.5f, 1f);
            rt.pivot            = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, yPos);
            rt.sizeDelta        = new Vector2(1050f, 400f);

            return container;
        }

        /// <summary>
        /// Resizes a scroll view content container to fit its editor fields.
        /// </summary>
        /// <param name="scrollContent">The content <see cref="GameObject"/> returned by <see cref="ScrollableView"/>.</param>
        /// <param name="editorContainerYPos">The Y position the editor container was placed at.</param>
        /// <param name="editorHeight">The current height consumed by editor fields.</param>
        public static void UpdateScrollContentHeight(GameObject scrollContent, float editorContainerYPos, float editorHeight)
        {
            float totalHeight = Mathf.Abs(editorContainerYPos) + editorHeight + 40f;
            ScrollableView.SetContentHeight(scrollContent, totalHeight);
        }
    }
}