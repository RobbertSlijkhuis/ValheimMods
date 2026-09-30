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

            switch (ProfileSettingsHelper.Current.hudPosition)
            {
                case HudPosition.BottomLeft:
                    anchor = new Vector2(0f, 0f);
                    position = new Vector2(110f, 30f);
                    break;
                case HudPosition.AboveMinimap:
                    anchor = new Vector2(1f, 1f);
                    position = new Vector2(-140f, -20f);
                    break;
                case HudPosition.UnderMinimap:
                    anchor = new Vector2(1f, 1f);
                    position = new Vector2(-140f, -260f);
                    break;
                case HudPosition.Custom:
                    anchor = new Vector2(0.5f, 0.5f);
                    position = new Vector2(ProfileSettingsHelper.Current.hudOffsetX, ProfileSettingsHelper.Current.hudOffsetY);
                    break;
                default: // BottomRight
                    anchor = new Vector2(1f, 0f);
                    position = new Vector2(-110f, 30f);
                    break;
            }

            hudPanelRect.anchorMin = anchor;
            hudPanelRect.anchorMax = anchor;
            hudPanelRect.anchoredPosition = position;
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
