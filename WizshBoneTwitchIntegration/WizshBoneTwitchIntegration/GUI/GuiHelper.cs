using System;
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
        public const float PanelBorderThickness = 0f;
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
        /// A short, muted line of extra orientation info shown directly under a tab's title (e.g.
        /// Redeems'/Profiles'/Viewers'/Settings' under-title blurbs, HelpTab's description lines) -
        /// always visible, not a tooltip.
        /// </summary>
        public static Text CreateTabDescription(string text, GameObject parent, Vector2 position, float width)
        {
            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            return label;
        }

        /// <summary>
        /// A small "?" marker with a <see cref="TooltipTrigger"/> attached, showing
        /// <paramref name="title"/> (bold, orange) followed by <paramref name="description"/> on
        /// hover - the mod's one reusable inline-help affordance. First used by HelpTab's own
        /// description lines to demonstrate the exact thing they explain.
        /// </summary>
        public static GameObject CreateHelpIcon(GameObject parent, Vector2 position, string title, string description, float size = 18f)
        {
            GameObject iconObj = GUIManager.Instance.CreateText(
                text: "?",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: size,
                height: size,
                addContentSizeFitter: false
            ).gameObject;

            Text text = iconObj.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = true; // TooltipTrigger's pointer-enter/exit handlers need a raycastable Graphic here

            iconObj.AddComponent<TooltipTrigger>().Init(title, description);

            return iconObj;
        }

        /// <summary>
        /// Re-pivots an already-created, center-pivoted <see cref="RectTransform"/> (both Jotunn's
        /// CreateText and CreateInputField leave Unity's default center pivot in place) so
        /// <paramref name="topY"/> becomes its top edge instead of its center - callers should pass
        /// the same Y they originally gave the center-pivoted creation call's <c>position</c>, which
        /// (since it was previously a center, not a top) lands the new top edge comfortably below
        /// whatever sits directly above it, rather than reproducing that old, too-tight center-derived
        /// top. Without this, an element sits vertically centered on the Y it was given instead of
        /// flush against it, drifting out of alignment with sibling elements that do use this fix
        /// (e.g. <see cref="MakeDescriptionExpandDownward"/>) or are otherwise top-anchored.
        /// </summary>
        public static void PivotToTop(RectTransform rt, float topY)
        {
            rt.pivot = new Vector2(rt.pivot.x, 1f);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, topY);
        }

        /// <summary>
        /// <see cref="PivotToTop"/>, plus lets <paramref name="text"/> grow downward from that fixed
        /// top edge as content wraps onto more lines, instead of a line being dropped when it
        /// overflows a fixed-height box.
        /// </summary>
        public static void MakeDescriptionExpandDownward(Text text, float topY)
        {
            PivotToTop(text.rectTransform, topY);

            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // ── shared card shape ───────────────────────────────────────────────────────────────
        //
        // The one "card" component HomeTab.cs and HelpTab.cs both build from - title always at
        // CardTitleY and description always at CardDescriptionTopY, so those two can never drift
        // apart between tabs the way they already have twice (the toggle/status Y mismatch on
        // Help's Panel image type card, and descriptions never getting the top-pivot fix Home's
        // did). Only what a card puts between title and description - a toggle+status row, a
        // button, input fields, a value line - varies per call site.

        public const float CardTitleY = -24f;
        public const float CardDescriptionTopY = -84f;
        public static readonly Color CardBackgroundColor = new Color(0f, 0f, 0f, 0.6f);

        /// <summary>
        /// The card shell every HomeTab/HelpTab card starts from - a top-pivoted, flat-background
        /// panel at a fixed grid slot.
        /// </summary>
        public static GameObject CreateCard(GameObject parent, Vector2 topCenter, float width, float height)
        {
            GameObject card = new GameObject("Card");
            card.transform.SetParent(parent.transform, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = topCenter;

            Image bg = card.AddComponent<Image>();
            bg.color = CardBackgroundColor;

            return card;
        }

        /// <summary>
        /// A card's title, always at the fixed <see cref="CardTitleY"/> top edge - never a
        /// per-call-site Y, so it can't drift the way the card's other elements already have.
        /// </summary>
        public static Text CreateCardTitle(GameObject card, string text, float width)
        {
            return CreateCardText(card, text, CardTitleY, 16, GUIManager.Instance.ValheimBeige, width);
        }

        /// <summary>
        /// Generic card text for whatever a card puts between its title and description (a value
        /// line, a status word, a field label) - the one part of the card shape that's allowed to
        /// vary. <paramref name="height"/> must comfortably exceed <paramref name="fontSize"/>'s
        /// natural line height - Unity's default vertical overflow (Truncate) doesn't partially
        /// clip a single line that doesn't fully fit its box, it drops the whole line, so a
        /// too-short box renders nothing at all rather than a clipped one.
        /// </summary>
        public static Text CreateCardText(GameObject card, string text, float y, int fontSize, Color color, float width, float height = 24f, float x = 0f)
        {
            Text label = GUIManager.Instance.CreateText(
                text: text,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(x, y),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: fontSize,
                color: color,
                outline: true,
                outlineColor: Color.black,
                width: width,
                height: height,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            return label;
        }

        /// <summary>
        /// A card's description, always top-pivoted at the fixed <see cref="CardDescriptionTopY"/>
        /// and free to grow downward if it wraps (<see cref="MakeDescriptionExpandDownward"/>) - the
        /// shape every card description should use so a call site can't forget the top-pivot fix
        /// the way every one of them originally did.
        /// </summary>
        public static Text CreateCardDescription(GameObject card, string text, float width, float height = 24f, int fontSize = 13)
        {
            return CreateCardDescription(card, text, CardDescriptionTopY, width, height, fontSize);
        }

        /// <summary>
        /// A description at a non-standard top edge, for the rare card whose "middle" content is
        /// taller than <see cref="CardDescriptionTopY"/> leaves room for (Help's soon-to-be-removed
        /// layout-tuning cards) - every other card should use the fixed-position overload above.
        /// </summary>
        public static Text CreateCardDescription(GameObject card, string text, float topY, float width, float height, int fontSize = 13)
        {
            Text label = CreateCardText(card, text, topY, fontSize, GUIManager.Instance.ValheimBeige, width, height);
            MakeDescriptionExpandDownward(label, topY);
            return label;
        }

        /// <summary>
        /// The shared toggle + big colored status-word row every "boolean" card uses (Home's Redeem
        /// status/Auto resolve/Enable on login/Chatting feature, Help's Show safezone bounds) -
        /// toggle and status always at the same Y so this shape can't silently drift between tabs
        /// the way it already did once (Help's Panel image type card was 4px off both this and its
        /// own sibling card before that got fixed). Returns the toggle and status Text so a caller
        /// that needs to mutate the status word later (Help's live safezone-debug toggle) can hold
        /// onto them.
        /// </summary>
        public static (Toggle Toggle, Text Status) CreateToggleStatusRow(GameObject card, float cardWidth, bool currentValue, Action<bool> onChanged, Color enabledColor)
        {
            const float toggleY = -56f;
            const float statusY = -60f;
            const float statusGap = 12f;

            Toggle toggle = GuiFieldBuilder.CreateBoolField(card, new Vector2(0f, toggleY), cardWidth - 24f, currentValue, onChanged);

            // The status word sits a gap right of the toggle, sharing the same top-center (0.5, 1)
            // anchor convention every other element on the card uses.
            float titleLeftEdge = -cardWidth / 2f + 12f;
            float toggleRightEdge = titleLeftEdge + GuiFieldBuilder.FieldHeight;
            float statusLeftEdge = toggleRightEdge + statusGap;
            float statusRightEdge = cardWidth / 2f - 12f;
            float statusWidth = statusRightEdge - statusLeftEdge;
            float statusCenterX = statusLeftEdge + statusWidth / 2f;

            Text status = CreateCardText(card, currentValue ? "Enabled" : "Disabled", statusY, 16,
                currentValue ? enabledColor : GUIManager.Instance.ValheimBeige, statusWidth, height: 24f, x: statusCenterX);

            return (toggle, status);
        }

        /// <summary>
        /// The shared single-button row every "one button, then a description" card uses (Home's
        /// Redemption log, Help's Safezone Unstuck) - left-aligned near the card's left edge, same
        /// width/Y/height every time, so it can't drift the way View history/Unstuck safezones
        /// already had (different width, X-alignment, and Y from each other).
        /// </summary>
        public static GameObject CreateCardButton(GameObject card, string text, float cardWidth, Action onClick, float width = 160f, float height = 40f)
        {
            const float y = -60f;
            float x = -(cardWidth - 24f) / 2f + width / 2f;

            GameObject btnObj = GUIManager.Instance.CreateButton(
                text: text,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(x, y),
                width: width,
                height: height
            );
            btnObj.SetActive(true);
            btnObj.GetComponent<Button>().onClick.AddListener(() => onClick());

            return btnObj;
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
        /// mockup's wide inset matte). Returns a <see cref="PanelBorderHandle"/> so a caller (see
        /// HelpTab's live border-thickness controls) can restyle the already-built bars later
        /// without rebuilding the panel.
        /// </summary>
        public static PanelBorderHandle AddPanelBorder(GameObject panel, float inset, float thickness, Color color)
        {
            RectTransform top = CreateRegion(panel, "BorderTop",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero).GetComponent<RectTransform>();
            AddBackground(top.gameObject, color);

            RectTransform bottom = CreateRegion(panel, "BorderBottom",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                Vector2.zero, Vector2.zero).GetComponent<RectTransform>();
            AddBackground(bottom.gameObject, color);

            RectTransform left = CreateRegion(panel, "BorderLeft",
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                Vector2.zero, Vector2.zero).GetComponent<RectTransform>();
            AddBackground(left.gameObject, color);

            RectTransform right = CreateRegion(panel, "BorderRight",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero).GetComponent<RectTransform>();
            AddBackground(right.gameObject, color);

            PanelBorderHandle handle = new PanelBorderHandle(top, bottom, left, right, inset);
            handle.SetThickness(thickness);
            return handle;
        }

        /// <summary>
        /// Live handle onto the 4 bars <see cref="AddPanelBorder"/> builds, letting a caller
        /// restyle their thickness after the fact (HelpTab's temporary border-thickness control) -
        /// the panel itself is only ever built once, so there's nothing to rebuild against.
        /// </summary>
        public class PanelBorderHandle
        {
            private readonly RectTransform m_top;
            private readonly RectTransform m_bottom;
            private readonly RectTransform m_left;
            private readonly RectTransform m_right;
            private readonly float m_inset;

            public PanelBorderHandle(RectTransform top, RectTransform bottom, RectTransform left, RectTransform right, float inset)
            {
                m_top = top;
                m_bottom = bottom;
                m_left = left;
                m_right = right;
                m_inset = inset;
            }

            public void SetThickness(float thickness)
            {
                thickness = Mathf.Max(0f, thickness);

                m_top.offsetMin = new Vector2(m_inset, -m_inset - thickness);
                m_top.offsetMax = new Vector2(-m_inset, -m_inset);

                m_bottom.offsetMin = new Vector2(m_inset, m_inset);
                m_bottom.offsetMax = new Vector2(-m_inset, m_inset + thickness);

                m_left.offsetMin = new Vector2(m_inset, m_inset);
                m_left.offsetMax = new Vector2(m_inset + thickness, -m_inset);

                m_right.offsetMin = new Vector2(-m_inset - thickness, m_inset);
                m_right.offsetMax = new Vector2(-m_inset, -m_inset);
            }
        }
    }
}
