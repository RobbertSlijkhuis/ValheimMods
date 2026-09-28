using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Jotunn.GUI;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
// System.Drawing types below are always fully qualified (System.Drawing.Color would otherwise
// collide with UnityEngine.Color, used unqualified throughout this file).

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

        private static readonly Dictionary<string, Sprite> s_embeddedSpriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Loads a PNG embedded via <c>&lt;EmbeddedResource&gt;</c> in the .csproj (e.g.
        /// "WizshBoneTwitchIntegration.resources.WizshBone.png") into a <see cref="Sprite"/>,
        /// caching the result so a rebuilt panel (e.g. <see cref="ShellSidebar"/>'s Create) doesn't
        /// re-decode the PNG every time.
        ///
        /// Decodes via <see cref="System.Drawing.Bitmap"/> rather than Unity's
        /// Texture2D.LoadImage/ImageConversion - that route pulls in
        /// UnityEngine.ImageConversionModule.dll, whose netstandard 2.1 dependency conflicts with
        /// this net48 project's netstandard 2.0 facade (CS1705), and pinning Valheim's own
        /// netstandard.dll to fix that in turn breaks on missing System.Memory/ReadOnlySpan
        /// polyfills. System.Drawing avoids that whole dependency chain.
        /// </summary>
        public static Sprite LoadEmbeddedSprite(string resourceName)
        {
            if (s_embeddedSpriteCache.TryGetValue(resourceName, out Sprite cached))
                return cached;

            Sprite sprite = DecodeEmbeddedPng(resourceName);
            s_embeddedSpriteCache[resourceName] = sprite;
            return sprite;
        }

        private static Sprite DecodeEmbeddedPng(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream))
            {
                int width = bitmap.Width;
                int height = bitmap.Height;

                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Color32[] pixels;
                try
                {
                    byte[] buffer = new byte[bmpData.Stride * height];
                    Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);

                    // Bitmap rows run top-down and are BGRA; Unity's pixel array runs bottom-up
                    // (row 0 = image bottom) and RGBA, so both need flipping while copying.
                    pixels = new Color32[width * height];
                    for (int y = 0; y < height; y++)
                    {
                        int srcRow = y * bmpData.Stride;
                        int destRow = (height - 1 - y) * width;
                        for (int x = 0; x < width; x++)
                        {
                            int i = srcRow + x * 4;
                            pixels[destRow + x] = new Color32(buffer[i + 2], buffer[i + 1], buffer[i], buffer[i + 3]);
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }

                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.SetPixels32(pixels);
                texture.Apply();

                return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f));
            }
        }

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

        /// <summary>
        /// A plain, non-clipping, top-anchored container of a fixed width and unbounded height -
        /// for step-2 forms short enough (under 4 card rows - a per-type judgment call, not
        /// measured) to never need scrolling at all, so they can skip <see cref="ScrollableList"/>
        /// entirely and use the full <c>Step2FieldWidth</c> instead of reserving
        /// <see cref="ScrollableList.ScrollbarWidth"/> for a scrollbar that would never appear.
        /// </summary>
        public static GameObject CreateFixedWidthContainer(GameObject parent, string name, Vector2 anchoredPosition, float width)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(width, 0f);
            rt.anchoredPosition = anchoredPosition;

            return container;
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
            // Rounded to whole pixels - callers derive width/topCenter from divisions (e.g.
            // CardWidth = ContentWidth/3) that don't come out even, and legacy uGUI Text doesn't
            // pixel-snap, so a fractional position/size here blurs every glyph inside the card.
            rt.sizeDelta = new Vector2(Mathf.Round(width), Mathf.Round(height));
            rt.anchoredPosition = new Vector2(Mathf.Round(topCenter.x), Mathf.Round(topCenter.y));

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
                // Rounded to whole pixels - see the same note in CreateCard.
                position: new Vector2(Mathf.Round(x), Mathf.Round(y)),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: fontSize,
                color: color,
                outline: true,
                outlineColor: Color.black,
                width: Mathf.Round(width),
                height: Mathf.Round(height),
                addContentSizeFitter: false
            ).GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            return label;
        }

        /// <summary>
        /// Sets <paramref name="text"/>'s content to <paramref name="fullText"/>, truncating with
        /// a trailing "..." if it doesn't fit within <paramref name="maxWidth"/>. Forces single-line
        /// <see cref="HorizontalWrapMode.Overflow"/> rather than the default Wrap - a wrapped
        /// second line just gets silently dropped by Unity's default vertical Truncate overflow
        /// (see <see cref="CreateCardText"/>'s note above) instead of showing the rest of the
        /// text, which is what made long profile names disappear mid-word in the sidebar's
        /// "Profile: {name}" header and the Redeems tab's profile dropdown before this existed.
        /// Wires/unwires a <see cref="TooltipTrigger"/> showing <paramref name="fullText"/> on
        /// <paramref name="text"/> itself whenever truncation actually happened, so hovering a
        /// clipped label reveals the untruncated name - removed again if a later call (e.g. a
        /// rename, or this same header's every-frame refresh) finds the text now fits unclipped.
        /// </summary>
        public static void SetTruncatedText(Text text, string fullText, float maxWidth)
        {
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            fullText = fullText ?? "";
            text.text = fullText;

            bool truncated = fullText.Length != 0 && text.preferredWidth > maxWidth;
            if (truncated)
            {
                int len = fullText.Length - 1;
                for (; len > 0; len--)
                {
                    text.text = fullText.Substring(0, len) + "...";
                    if (text.preferredWidth <= maxWidth)
                        break;
                }
                if (len <= 0)
                    text.text = "...";
            }

            TooltipTrigger tooltip = text.GetComponent<TooltipTrigger>();
            if (truncated)
            {
                text.raycastTarget = true; // TooltipTrigger's pointer-enter/exit handlers need a raycastable Graphic here
                if (tooltip == null)
                    tooltip = text.gameObject.AddComponent<TooltipTrigger>();
                tooltip.Init(fullText);
            }
            else if (tooltip != null)
            {
                UnityEngine.Object.Destroy(tooltip);
            }
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

        // ── reset-to-default plumbing ───────────────────────────────────────────────────────
        //
        // Originally SettingsTab-only; hoisted here once RedeemWizard step 2 needed the exact
        // same "field + reset button" shape, so both share one implementation.

        /// <summary>Sits right of the field, sharing its row rather than a separate one.</summary>
        public const float ResetButtonWidth = 70f;
        public const float ResetButtonGap = 8f;

        /// <summary>
        /// Splits a row's usable width into a left field slot and a right <see cref="ResetButtonWidth"/>
        /// slot - shared by every field row that needs a reset button next to its field, so this
        /// split can't drift between call sites. <paramref name="inset"/> defaults to 24f, matching
        /// every card's title/description inset (SettingsTab's cells); callers with no such
        /// surrounding card inset (RedeemWizard step 2's rows) pass 0f.
        /// </summary>
        public static (float FieldWidth, float FieldCenterX, float ResetCenterX) FieldAndResetLayout(float cellWidth, float inset = 24f)
        {
            float totalWidth = cellWidth - inset;
            float leftEdge = -totalWidth / 2f;
            float rightEdge = totalWidth / 2f;
            float fieldWidth = totalWidth - ResetButtonWidth - ResetButtonGap;
            return (fieldWidth, leftEdge + fieldWidth / 2f, rightEdge - ResetButtonWidth / 2f);
        }

        /// <summary>
        /// Jötunn's <see cref="ColorPicker"/> is a scene-wide singleton (not a child of whatever
        /// swatch/panel opened it), so navigating away from - or closing - whatever GUI opened it
        /// doesn't close it automatically; it's otherwise left open and unusable, floating over
        /// whatever shows next. <see cref="ColorPicker.Cancel"/> reverts to the color the swatch
        /// had before the picker opened and closes it - safe to call any time a color swatch's
        /// owning panel/step/dialog is about to be hidden or navigated away from, even if no
        /// picker is currently open (<see cref="ColorPicker.done"/> guards that no-op case).
        /// Originally ViewerEditDialog-only; hoisted here once RedeemWizard's step 2 needed the
        /// same "close it when leaving" behavior for its own color-field rows.
        /// </summary>
        public static void CloseOpenColorPicker()
        {
            if (!ColorPicker.done)
                ColorPicker.Cancel();
        }

        public static GameObject CreateResetButton(GameObject cell, float centerX, float y, Action onClick)
        {
            GameObject resetBtn = CreateButton(
                text: "Reset",
                parent: cell.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(centerX, y),
                width: ResetButtonWidth,
                height: GuiFieldBuilder.FieldHeight);
            resetBtn.SetActive(true);
            resetBtn.GetComponent<Button>().onClick.AddListener(() => onClick());
            return resetBtn;
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

            // The status word sits a gap right of the toggle, sharing the same top-center (0.5, 1)
            // anchor convention every other element on the card uses.
            float titleLeftEdge = -cardWidth / 2f + 12f;
            float toggleRightEdge = titleLeftEdge + GuiFieldBuilder.FieldHeight;

            Toggle toggle = GuiFieldBuilder.CreateBoolField(card, new Vector2(0f, toggleY), cardWidth - 24f, currentValue, onChanged);

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

            GameObject btnObj = CreateButton(
                text: text,
                parent: card.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                // Rounded to whole pixels - see the same note in CreateCard.
                position: new Vector2(Mathf.Round(x), Mathf.Round(y)),
                width: Mathf.Round(width),
                height: Mathf.Round(height)
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
        public static void AddBorder(GameObject buttonObj, Color color, float width = 2f)
        {
            Outline outline = buttonObj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, width);
        }

        /// <summary>
        /// Wraps <see cref="GUIManager.CreateButton"/> (same signature/defaults) so every button in
        /// this UI automatically gets <see cref="SuppressDuplicateSelectSound"/> applied - a call
        /// site can't forget it the way a "call the real method, then remember an extra line"
        /// convention could. Use this instead of GUIManager.Instance.CreateButton directly.
        /// </summary>
        public static GameObject CreateButton(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, float width = 0f, float height = 0f)
        {
            GameObject buttonObj = GUIManager.Instance.CreateButton(text, parent, anchorMin, anchorMax, position, width, height);
            SuppressDuplicateSelectSound(buttonObj);
            return buttonObj;
        }

        /// <summary>
        /// Silences the duplicate "select" sound Valheim's ButtonSfx component plays on top of the
        /// normal click sound - GUIManager.ApplyButtonStyle (run internally by CreateButton) wires
        /// both m_sfxPrefab (Button.onClick) and m_selectSfxPrefab (ISelectHandler.OnSelect, which
        /// fires the instant a click selects the button), so every button would otherwise play two
        /// sounds per click. Leaves m_sfxPrefab untouched. Only called from <see cref="CreateButton"/>
        /// above.
        /// </summary>
        private static void SuppressDuplicateSelectSound(GameObject buttonObj)
        {
            if (buttonObj.TryGetComponent(out ButtonSfx buttonSfx))
                buttonSfx.m_selectSfxPrefab = null;
        }

        /// <summary>
        /// Wraps <see cref="GUIManager.CreateToggle"/> (same signature) so every toggle in this UI
        /// automatically gets <see cref="AddToggleClickSound"/> applied. Use this instead of
        /// GUIManager.Instance.CreateToggle directly.
        /// </summary>
        public static GameObject CreateToggle(Transform parent, float width, float height)
        {
            GameObject toggleObj = GUIManager.Instance.CreateToggle(parent, width, height);
            AddToggleClickSound(toggleObj);
            return toggleObj;
        }

        /// <summary>
        /// Gives a toggle the same click sound a button gets - GUIManager.ApplyToogleStyle (run
        /// internally by CreateToggle) never attaches a ButtonSfx at all, so toggles are silent by
        /// default in both this UI and GUI_OLD (confirmed by decompiling Jotunn.dll). ButtonSfx's
        /// m_sfxPrefab only ever fires from Button.onClick - a Toggle isn't a Button, so that path
        /// never fires - but m_selectSfxPrefab fires from ISelectHandler.OnSelect, which every
        /// Selectable (including Toggle) raises on click. So the toggle's one sound is wired through
        /// m_selectSfxPrefab, reusing the same "sfx_gui_button" click prefab Jötunn uses for buttons'
        /// own click sound, for a consistent feel. Only called from <see cref="CreateToggle"/> above.
        /// </summary>
        private static void AddToggleClickSound(GameObject toggleObj)
        {
            ButtonSfx buttonSfx = toggleObj.GetComponent<ButtonSfx>() ?? toggleObj.AddComponent<ButtonSfx>();
            buttonSfx.m_selectSfxPrefab = PrefabManager.Cache.GetPrefab<GameObject>("sfx_gui_button");
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
