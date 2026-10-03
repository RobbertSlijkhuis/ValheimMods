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
        private Text m_redeemsValueText;

        // Gap (in our canvas units) between the minimap's edge and the panel.
        private const float MinimapGap = 10f;
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

            GUIManager.Instance.CreateText(
                text: "Status",
                parent: hudPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-60f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 50f,
                height: 20f,
                addContentSizeFitter: false
            );

            m_loginStatusCircle = CreateCircle("LoginStatusCircle", new Vector2(-30f, 0f));

            GUIManager.Instance.CreateText(
                text: "Redeems:",
                parent: hudPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(25f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 80f,
                height: 20f,
                addContentSizeFitter: false
            );

            m_redeemsValueText = GUIManager.Instance.CreateText(
                text: GetRedeemsValueText(),
                parent: hudPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(70f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: customRewards.m_enabled ? ColorLoggedIn : ColorLoggedOut,
                outline: true,
                outlineColor: Color.black,
                width: 30f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

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

            m_redeemsValueText.text = GetRedeemsValueText();
            m_redeemsValueText.color = customRewards.m_enabled ? ColorLoggedIn : ColorLoggedOut;
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
                    if (!TryGetMinimapPlacement(above: true, scale, out anchor, out position))
                    {
                        anchor = new Vector2(1f, 1f);
                        position = new Vector2(-140f, -20f) * scale;
                    }
                    break;
                case HudPosition.UnderMinimap:
                    if (!TryGetMinimapPlacement(above: false, scale, out anchor, out position))
                    {
                        anchor = new Vector2(1f, 1f);
                        position = new Vector2(-140f, -260f) * scale;
                    }
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

        private Image CreateCircle(string name, Vector2 position)
        {
            GameObject circleObj = new GameObject(name);
            circleObj.transform.SetParent(hudPanel.transform, false);

            RectTransform rt = circleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(16f, 16f);
            rt.anchoredPosition = position;

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

        private string GetRedeemsValueText()
        {
            return customRewards.m_enabled ? "On" : "Off";
        }
    }
}
