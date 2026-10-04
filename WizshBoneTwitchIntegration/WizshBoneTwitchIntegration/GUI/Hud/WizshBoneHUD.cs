using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Copy-adapted from GUI_OLD/panels/WizshBoneHUD.cs (new namespace) rather than referenced
    /// directly, per the new UI's isolation constraint - see GUI/dialogs/ConfirmDialog.cs's
    /// identical rationale. The always-visible corner status HUD (login dot + redeems on/off),
    /// shown regardless of whether F3/F4 is ever opened.
    /// </summary>
    internal class WizshBoneHUD
    {
        private GameObject hudPanel;
        private RectTransform hudPanelRect;
        private TwitchAuth auth;
        private TwitchCustomRewards customRewards;

        // Mutable element references
        private Image m_loginStatusCircle;
        private Text m_titleText;
        private Text m_redeemsText;
        private Text m_openKeyText;

        // Gap (in our canvas units) between the minimap's edge and the panel.
        private const float MinimapGap = 10f;

        // Status panel layout (our canvas units) - see LayoutItems. The circle is nudged up a little
        // because text glyphs sit slightly above their box's vertical center.
        private const float ItemSpacing = 10f;
        private const float TextLineHeight = 20f;
        private const float CircleSize = 16f;
        private const float CircleNudgeY = 2f;
        private readonly Vector3[] m_minimapCorners = new Vector3[4];

        private static readonly Color ColorLoggedIn = new Color(0.18f, 0.8f, 0.18f);
        private static readonly Color ColorLoggedOut = new Color(0.8f, 0.18f, 0.18f);

        public void ShowHUD()
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
                return;

            auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            hudPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(1f, 0f),
                anchorMax: new Vector2(1f, 0f),
                position: new Vector2(-110f, 60f),
                width: 200f,
                height: 40f,
                draggable: false
            );

            hudPanelRect = hudPanel.GetComponent<RectTransform>();

            // Every item's position is set by LayoutItems() from the texts' measured widths, so the
            // positions passed to CreateText/CreateCircle are placeholders.
            m_titleText = CreateLabel("WBTI:", GUIManager.Instance.ValheimOrange);

            m_loginStatusCircle = CreateCircle("LoginStatusCircle");
            m_loginStatusCircle.gameObject.AddComponent<TooltipTrigger>().Init(
                () => $"Connected to Twitch: {(auth.m_loggedIn ? "yes" : "no")}");

            // The word itself is the indicator (green = enabled, red = disabled); the hover text
            // spells it out.
            m_redeemsText = CreateLabel("Redeems", customRewards.m_enabled ? ColorLoggedIn : ColorLoggedOut);
            m_redeemsText.raycastTarget = true; // TooltipTrigger's pointer-enter/exit handlers need a raycastable Graphic here
            m_redeemsText.gameObject.AddComponent<TooltipTrigger>().Init(
                () => $"Redeems {(customRewards.m_enabled ? "enabled" : "disabled")} on Twitch");

            m_openKeyText = CreateLabel(GetOpenKeyText(), GUIManager.Instance.ValheimOrange);
            m_openKeyText.raycastTarget = true; // TooltipTrigger's pointer-enter/exit handlers need a raycastable Graphic here
            m_openKeyText.gameObject.AddComponent<TooltipTrigger>().Init("Open the WizshBone UI");

            LayoutItems();
            RepositionHUD();
            hudPanel.SetActive(ShouldBeVisible());
        }

        /// <summary>
        /// Only once the local player has spawned, so the panel stays hidden during the world
        /// loading screen - this panel lives on Jotunn's own canvas, which vanilla's loading screen
        /// doesn't cover. It stays visible through death and respawn (see
        /// <see cref="HudHelper.PlayerHasSpawned"/>). Also follows vanilla's Ctrl+F3 "hide UI"
        /// toggle, which vanilla's Hud never applies to that canvas either.
        /// </summary>
        private static bool ShouldBeVisible()
        {
            return HudHelper.PlayerHasSpawned && !Hud.IsUserHidden();
        }

        public void UpdateHUD()
        {
            if (hudPanel == null)
                return;

            hudPanel.SetActive(ShouldBeVisible());

            // Re-evaluated every tick so window resizes / UI scale changes are followed (position
            // and scale come from the live canvases and minimap).
            RepositionHUD();

            m_loginStatusCircle.color = auth.m_loggedIn ? ColorLoggedIn : ColorLoggedOut;

            m_redeemsText.color = customRewards.m_enabled ? ColorLoggedIn : ColorLoggedOut;

            // A rebound key changes the hint's width, so the whole row is re-laid out.
            string openKeyText = GetOpenKeyText();
            if (openKeyText != m_openKeyText.text)
            {
                m_openKeyText.text = openKeyText;
                LayoutItems();
            }
        }

        /// <summary>
        /// Places the panel's items left to right with <see cref="ItemSpacing"/> between them, each
        /// text sized to its measured width, centered in the panel. The panel itself stays a fixed
        /// width (positioning in <see cref="RepositionHUD"/> relies on that). To add an item, create
        /// it and add it here.
        /// </summary>
        private void LayoutItems()
        {
            RectTransform[] items =
            {
                m_titleText.rectTransform,
                m_loginStatusCircle.rectTransform,
                m_redeemsText.rectTransform,
                m_openKeyText.rectTransform,
            };

            Text[] texts = { m_titleText, m_redeemsText, m_openKeyText };
            foreach (Text text in texts)
            {
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                // Whole-unit widths keep every item edge on a whole unit (see the rounding of
                // "left" below), so nothing renders blurry from sitting between pixels.
                text.rectTransform.sizeDelta = new Vector2(Mathf.Ceil(text.preferredWidth), TextLineHeight);
            }

            float totalWidth = ItemSpacing * (items.Length - 1);
            foreach (RectTransform item in items)
                totalWidth += item.sizeDelta.x;

            // Items are center-anchored in the panel, so x is measured from the panel's center; the
            // row is centered in the (fixed-width) panel.
            float left = Mathf.Round(-totalWidth / 2f);
            foreach (RectTransform item in items)
            {
                float y = item == m_loginStatusCircle.rectTransform ? CircleNudgeY : 0f;
                item.anchoredPosition = new Vector2(left + item.sizeDelta.x / 2f, y);
                left += item.sizeDelta.x + ItemSpacing;
            }
        }

        public void RepositionHUD()
        {
            if (hudPanelRect == null)
                return;

            Vector2 anchor;
            Vector2 position;

            // Our canvas doesn't scale the same as vanilla's, so the panel is scaled to match
            // vanilla UI at the current resolution/UI scale. The fixed offsets below are margins
            // from a screen edge, so they scale with it; Custom's X/Y stay the raw values entered.
            float scale = GetScaleRelativeToVanilla();

            switch (ProfileSettingsHelper.Current.hudPosition)
            {
                case HudPosition.BottomLeft:
                    anchor = new Vector2(0f, 0f);
                    position = new Vector2(110f, 30f) * scale;
                    break;
                case HudPosition.AboveMinimap:
                    PlaceRelativeToMinimap(HudPosition.AboveMinimap, above: true, scale, new Vector2(-140f, -20f), out anchor, out position);
                    break;
                case HudPosition.UnderMinimap:
                    PlaceRelativeToMinimap(HudPosition.UnderMinimap, above: false, scale, new Vector2(-140f, -260f), out anchor, out position);
                    break;
                case HudPosition.Custom:
                    anchor = new Vector2(0.5f, 0.5f);
                    position = new Vector2(ProfileSettingsHelper.Current.hudOffsetX, ProfileSettingsHelper.Current.hudOffsetY);
                    break;
                default: // BottomRight
                    anchor = new Vector2(1f, 0f);
                    position = new Vector2(-110f, 30f) * scale;
                    break;
            }

            hudPanelRect.anchorMin = anchor;
            hudPanelRect.anchorMax = anchor;
            hudPanelRect.anchoredPosition = position;
            hudPanelRect.localScale = new Vector3(scale, scale, 1f);
        }

        // Last placement computed from a live, on-screen minimap, and the mode it was for.
        private HudPosition? m_lastMinimapMode;
        private Vector2 m_lastMinimapAnchor;
        private Vector2 m_lastMinimapPosition;

        /// <summary>
        /// Minimap-relative placement with a memory: a live reading is used (and remembered) when the
        /// minimap is available; when it isn't (e.g. vanilla has moved it off-screen for a moment),
        /// the last good placement for the same mode is kept so the panel doesn't jump, and only if
        /// there has never been one does it use the hardcoded <paramref name="fallbackPosition"/>.
        /// </summary>
        private void PlaceRelativeToMinimap(HudPosition mode, bool above, float scale, Vector2 fallbackPosition, out Vector2 anchor, out Vector2 position)
        {
            if (TryGetMinimapPlacement(above, scale, out anchor, out position))
            {
                m_lastMinimapMode = mode;
                m_lastMinimapAnchor = anchor;
                m_lastMinimapPosition = position;
                return;
            }

            if (m_lastMinimapMode == mode)
            {
                anchor = m_lastMinimapAnchor;
                position = m_lastMinimapPosition;
            }
            else
            {
                anchor = new Vector2(1f, 1f);
                position = fallbackPosition * scale;
            }
        }

        /// <summary>
        /// Places the panel directly above/below vanilla's corner minimap by reading the minimap
        /// image's real on-screen rect, so it lands in the same spot at any resolution, aspect ratio
        /// or UI scale (our canvas may scale differently from vanilla's, so fixed offsets only fit
        /// one screen). Returns false when the minimap isn't available yet - callers fall back to
        /// hardcoded offsets. <paramref name="scale"/> is the panel's scale (see
        /// <see cref="GetScaleRelativeToVanilla"/>), which the gap and clamping account for.
        /// </summary>
        private bool TryGetMinimapPlacement(bool above, float scale, out Vector2 anchor, out Vector2 position)
        {
            anchor = Vector2.zero;
            position = Vector2.zero;

            RectTransform parentRect = hudPanelRect.parent as RectTransform;
            RawImage mapImage = Minimap.instance != null ? Minimap.instance.m_mapImageSmall : null;
            if (parentRect == null || mapImage == null)
                return false;

            Canvas mapCanvas = mapImage.canvas;
            Canvas ownCanvas = parentRect.GetComponentInParent<Canvas>();
            if (mapCanvas == null || ownCanvas == null)
                return false;

            // Corners: 0 = bottom-left, 1 = top-left, 2 = top-right, 3 = bottom-right.
            mapImage.rectTransform.GetWorldCorners(m_minimapCorners);
            Vector3 edgeCenter = above
                ? (m_minimapCorners[1] + m_minimapCorners[2]) / 2f
                : (m_minimapCorners[0] + m_minimapCorners[3]) / 2f;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(mapCanvas), edgeCenter);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, GetCanvasCamera(ownCanvas), out Vector2 local))
                return false;

            Rect bounds = parentRect.rect;

            // Vanilla moves the minimap ~10000 units off-screen at times (seen shortly after spawning).
            // Placing against that rect would just clamp the panel to the screen edge until it moves
            // back, so treat an off-screen minimap as unavailable.
            if (!bounds.Contains(local))
                return false;

            // Scaled size, in the parent's units - the gap and clamping below work with this.
            Vector2 size = hudPanelRect.rect.size * scale;
            Vector2 pivot = hudPanelRect.pivot;

            // Desired panel center, kept fully on screen.
            float centerX = Mathf.Clamp(local.x, bounds.xMin + size.x / 2f, bounds.xMax - size.x / 2f);
            float centerY = above
                ? local.y + MinimapGap * scale + size.y / 2f
                : local.y - MinimapGap * scale - size.y / 2f;
            centerY = Mathf.Clamp(centerY, bounds.yMin + size.y / 2f, bounds.yMax - size.y / 2f);

            // ScreenPointToLocalPoint is relative to the parent's pivot, so anchoring there keeps
            // anchoredPosition in the same space; the panel's own pivot offsets center -> pivot.
            anchor = parentRect.pivot;
            position = new Vector2(
                centerX + (pivot.x - 0.5f) * size.x,
                centerY + (pivot.y - 0.5f) * size.y);
            return true;
        }

        /// <summary>
        /// Vanilla's HUD canvas scale (resolution + the UI scale option) divided by our canvas's
        /// scale - the factor that makes our panel the same size as vanilla UI elements. 1 if
        /// either canvas isn't available yet.
        /// </summary>
        private float GetScaleRelativeToVanilla()
        {
            Canvas vanillaCanvas = Hud.instance != null ? Hud.instance.GetComponentInParent<Canvas>() : null;
            Canvas ownCanvas = hudPanelRect.GetComponentInParent<Canvas>();
            if (vanillaCanvas == null || ownCanvas == null)
                return 1f;

            float vanillaScale = vanillaCanvas.rootCanvas.scaleFactor;
            float ownScale = ownCanvas.rootCanvas.scaleFactor;
            return vanillaScale > 0f && ownScale > 0f ? vanillaScale / ownScale : 1f;
        }

        private static Camera GetCanvasCamera(Canvas canvas)
        {
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        private Text CreateLabel(string text, Color color)
        {
            // Position/size passed here are placeholders - LayoutItems sets both from the measured text.
            return GUIManager.Instance.CreateText(
                text: text,
                parent: hudPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: Vector2.zero,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: color,
                outline: true,
                outlineColor: Color.black,
                width: 50f,
                height: TextLineHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
        }

        private Image CreateCircle(string name)
        {
            GameObject circleObj = new GameObject(name);
            circleObj.transform.SetParent(hudPanel.transform, false);

            RectTransform rt = circleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(CircleSize, CircleSize);

            Image image = circleObj.AddComponent<Image>();
            image.sprite = CreateCircleSprite(64);
            image.color = auth.m_loggedIn ? ColorLoggedIn : ColorLoggedOut;

            return image;
        }

        private static Sprite CreateCircleSprite(int resolution)
        {
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
            Color[] pixels = new Color[resolution * resolution];

            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution / 2f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * resolution + x] = distance <= radius ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// The main UI's open key, read live from config so a rebind (e.g. via the Configuration
        /// Manager) shows up without a restart.
        /// </summary>
        private static string GetOpenKeyText()
        {
            return $"[{PluginConfig.configWizshBoneWindow.Value}]";
        }
    }
}
