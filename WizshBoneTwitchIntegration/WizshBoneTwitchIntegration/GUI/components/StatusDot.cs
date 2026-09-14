using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Small procedurally-generated colored circle indicator (e.g. Twitch login status) - no
    /// texture import needed. Extracted from the private CreateCircle/CreateCircleSprite methods
    /// on GUI_OLD/panels/WizshBoneHUD.cs and made reusable, since they weren't exposed there.
    /// </summary>
    internal static class StatusDot
    {
        private const int Resolution = 64;
        private static Sprite s_circleSprite;

        /// <summary>
        /// Creates a colored circle <see cref="Image"/> parented to <paramref name="parent"/>.
        /// </summary>
        public static Image Create(GameObject parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, float diameter, Color color)
        {
            GameObject dotObj = new GameObject("StatusDot");
            dotObj.transform.SetParent(parent.transform, false);

            RectTransform rt = dotObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = new Vector2(diameter, diameter);
            rt.anchoredPosition = position;

            Image image = dotObj.AddComponent<Image>();
            image.sprite = GetCircleSprite();
            image.color = color;

            return image;
        }

        private static Sprite GetCircleSprite()
        {
            if (s_circleSprite != null)
                return s_circleSprite;

            Texture2D texture = new Texture2D(Resolution, Resolution, TextureFormat.ARGB32, false);
            Color[] pixels = new Color[Resolution * Resolution];

            Vector2 center = new Vector2(Resolution / 2f, Resolution / 2f);
            float radius = Resolution / 2f;

            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * Resolution + x] = distance <= radius ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            s_circleSprite = Sprite.Create(texture, new Rect(0, 0, Resolution, Resolution), new Vector2(0.5f, 0.5f));
            return s_circleSprite;
        }
    }
}
