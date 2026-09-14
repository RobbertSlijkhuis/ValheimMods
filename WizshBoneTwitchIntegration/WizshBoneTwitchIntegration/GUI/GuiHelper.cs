using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Shared UI utility methods for the new shell - a trimmed copy of GUI_OLD/tabs/TabUIHelper.cs
    /// keeping only what this round needs, plus <see cref="CreateRegion"/>/<see cref="AddBackground"/>
    /// for laying out the sidebar/topbar/content regions inside one wood panel.
    /// </summary>
    internal static class GuiHelper
    {
        public const int TitleFontSize = 16;

        /// <summary>
        /// Creates a left-aligned orange section title label.
        /// </summary>
        public static Text CreateTitle(string text, GameObject parent, Vector2 position, float width = 300f)
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
        /// Creates an invisible region using standard Unity anchor+offset stretch rules (e.g. a
        /// fixed-width left column, a fixed-height top strip, or a remaining fill area) - more
        /// general than <see cref="UIContainer.Create"/>'s fixed full-stretch. Used to lay out the
        /// shell's sidebar/topbar/content regions inside a single wood panel.
        /// </summary>
        public static GameObject CreateRegion(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject region = new GameObject(name);
            region.transform.SetParent(parent.transform, false);

            RectTransform rt = region.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            return region;
        }

        /// <summary>
        /// Adds a flat-colored background <see cref="Image"/> filling a region. The shell keeps
        /// Jötunn's built-in wood panel sprite (no custom texture import, per the round 1 decision)
        /// rather than a distinct sidebar/topbar texture - a simple dark overlay is enough to
        /// separate them visually.
        /// </summary>
        public static Image AddBackground(GameObject region, Color color)
        {
            Image image = region.AddComponent<Image>();
            image.color = color;
            return image;
        }
    }
}
