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
        /// Shared border/divider styling (ported from RedesignUI.dc.html's `border:3px solid
        /// #120d08` panel frame and its sidebar-right/title-underline/topbar-bottom lines) - kept
        /// here since <see cref="WizshBoneShellGUI"/>, <see cref="ShellSidebar"/>, and
        /// <see cref="ShellTopBar"/> all need the same values.
        /// </summary>
        public const float PanelBorderThickness = 4f;
        public static readonly Color PanelBorderColor = new Color(0.071f, 0.051f, 0.031f, 1f); // #120d08
        public static readonly Color DividerColorStrong = new Color(1f, 1f, 1f, 0.13f);         // ~#ffffff20, title underline
        public static readonly Color DividerColorSubtle = new Color(1f, 1f, 1f, 0.09f);         // ~#ffffff18, top-bar bottom

        /// <summary>
        /// Creates a left-aligned orange section title label.
        /// </summary>
        public static Text CreateTitle(string text, GameObject parent, Vector2 position, float width = 300f)
        {
            Text label = GUIManager.Instance.CreateText(
                text:                text.ToUpperInvariant(),
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

        /// <summary>
        /// Adds a thin colored outline around a button's background image, matching its label
        /// color - copy-adapted from GUI_OLD/tabs/TabUIHelper.cs's AddBorder (same technique:
        /// GUIManager.Instance.CreateButton's root GameObject carries both the Button and its
        /// Image directly, so an Outline applied there reads as a border around the button).
        /// </summary>
        public static void AddBorder(GameObject buttonObj, Color color)
        {
            Outline outline = buttonObj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(2f, 2f);
        }

        /// <summary>
        /// Adds a thin border frame near a panel's outer edge - four flat-colored bars built the
        /// same way as the sidebar/topbar/content regions (<see cref="CreateRegion"/>/
        /// <see cref="AddBackground"/>), added as the panel's last children so they draw on top of
        /// everything else already filling it edge-to-edge (ported from RedesignUI.dc.html's
        /// `border:3px solid #120d08` panel frame, kept subtle/close to the edge rather than the
        /// mockup's wide inset matte).
        /// </summary>
        public static void AddPanelBorder(GameObject panel, float inset, float thickness, Color color)
        {
            AddBackground(CreateRegion(panel, "BorderTop",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(inset, -inset - thickness), new Vector2(-inset, -inset)), color);

            AddBackground(CreateRegion(panel, "BorderBottom",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(inset, inset), new Vector2(-inset, inset + thickness)), color);

            AddBackground(CreateRegion(panel, "BorderLeft",
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(inset, inset), new Vector2(inset + thickness, -inset)), color);

            AddBackground(CreateRegion(panel, "BorderRight",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-inset - thickness, inset), new Vector2(-inset, -inset)), color);
        }
    }
}
