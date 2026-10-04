using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Attach to any UI <see cref="GameObject"/> to show a small floating tooltip while it is
    /// hovered, with the tooltip's bottom-left corner tracking the mouse cursor.
    ///
    /// Copy-adapted from GUI_OLD/components/TooltipTrigger.cs (new namespace, uses this folder's
    /// own <see cref="GuiFieldBuilder.FieldFontSize"/>) per the new UI's isolation-from-GUI_OLD
    /// constraint - unwired for now, added as shared infra ahead of the Redeems tab round, which
    /// will attach it to the test-redeem button the same way GUI_OLD does.
    /// </summary>
    internal class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float TextWidth     = 260f; // max text width - shorter text gets a narrower box
        private const float Padding       = 8f;
        private const float MinTextHeight = 20f;

        private string m_text;
        private Func<string> m_textProvider;
        private GameObject m_tooltipObj;
        private RectTransform m_tooltipRt;
        private Canvas m_rootCanvas;
        private RectTransform m_rootCanvasRt;

        public void Init(string text)
        {
            m_text = text;
            m_textProvider = null;
        }

        /// <summary>
        /// Overload for a tooltip whose text depends on live state (e.g. the HUD's connection
        /// dot) - the provider is evaluated each time the tooltip opens.
        /// </summary>
        public void Init(Func<string> textProvider)
        {
            m_textProvider = textProvider;
        }

        /// <summary>
        /// Overload for a tooltip that leads with a bold, colored title line above a description
        /// paragraph - used by <see cref="GuiHelper.CreateHelpIcon"/>'s "?" markers. Formats both
        /// into the same single <see cref="m_text"/> string <see cref="OnPointerEnter"/>'s existing
        /// preferredHeight-based sizing already measures, rather than adding a second Text element
        /// with its own layout math.
        /// </summary>
        public void Init(string title, string description)
        {
            string titleHex = ColorUtility.ToHtmlStringRGB(GUIManager.Instance.ValheimOrange);
            m_text = $"<b><color=#{titleHex}>{title}</color></b>\n{description}";
            m_textProvider = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Canvas rootCanvas = GetRootCanvas(gameObject);
            if (rootCanvas == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] TooltipTrigger: no root Canvas found, skipping tooltip.");
                return;
            }

            m_rootCanvas   = rootCanvas;
            m_rootCanvasRt = (RectTransform)rootCanvas.transform;

            // Reparented to the root canvas so it draws above everything and isn't clipped by
            // any ancestor ScrollRect/Mask the hovered element is nested inside.
            m_tooltipObj = new GameObject("Tooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            m_tooltipObj.transform.SetParent(rootCanvas.transform, false);

            RectTransform tooltipRt = m_tooltipObj.GetComponent<RectTransform>();
            m_tooltipRt = tooltipRt;
            tooltipRt.pivot    = new Vector2(0f, 0f);
            tooltipRt.anchorMin = new Vector2(0f, 0f);
            tooltipRt.anchorMax = new Vector2(0f, 0f);

            // Never intercept the pointer - the tooltip floats over the hovered element itself,
            // and if it raycast-blocked, the pointer would "leave" the hovered element as soon as
            // the tooltip appeared under it, destroying the tooltip, which let the pointer re-enter
            // the element and recreate it - a flicker loop.
            Image background = m_tooltipObj.GetComponent<Image>();
            background.sprite        = GUIManager.Instance.GetSprite("button_small");
            background.type          = Image.Type.Sliced;
            background.color         = new Color(0.05f, 0.05f, 0.05f, 0.92f);
            background.raycastTarget = false;

            Text tooltipText = GUIManager.Instance.CreateText(
                text: m_textProvider != null ? m_textProvider() : m_text,
                parent: m_tooltipObj.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: Vector2.zero,
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: GuiFieldBuilder.FieldFontSize,
                color: new Color(0.78f, 0.78f, 0.78f, 1f),
                outline: false,
                outlineColor: Color.black,
                width: TextWidth,
                height: MinTextHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            tooltipText.alignment          = TextAnchor.MiddleLeft;
            tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            tooltipText.verticalOverflow   = VerticalWrapMode.Overflow;
            tooltipText.raycastTarget      = false;
            tooltipText.supportRichText    = true;

            // Width fits the longest line, capped at TextWidth (beyond which the text wraps). Measured
            // unwrapped first; the small slack keeps float rounding from wrapping a line that fits.
            tooltipText.horizontalOverflow = HorizontalWrapMode.Overflow;
            float textWidth = Mathf.Min(TextWidth, Mathf.Ceil(tooltipText.preferredWidth) + 2f);
            tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            tooltipText.rectTransform.sizeDelta = new Vector2(textWidth, MinTextHeight);

            float textHeight = Mathf.Max(MinTextHeight, tooltipText.preferredHeight);
            tooltipText.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
            tooltipRt.sizeDelta = new Vector2(textWidth + Padding * 2f, textHeight + Padding * 2f);

            PositionAtMouse();
        }

        public void OnPointerExit(PointerEventData eventData) => DestroyTooltip();

        private void OnDisable() => DestroyTooltip();

        private void OnDestroy() => DestroyTooltip();

        private void Update()
        {
            if (m_tooltipObj != null)
                PositionAtMouse();
        }

        private void PositionAtMouse()
        {
            Camera cam = m_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_rootCanvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rootCanvasRt, Input.mousePosition, cam, out Vector2 local))
                return;

            // The tooltip is anchored to the canvas's bottom-left (see OnPointerEnter), so its
            // anchoredPosition is the cursor's offset from the canvas rect's min corner.
            Rect bounds = m_rootCanvasRt.rect;
            Vector2 size = m_tooltipRt.sizeDelta;
            Vector2 pos = local - bounds.min;

            // Default is up-and-right of the cursor; flip to the other side when that would run off
            // the screen, then clamp for the case where neither side fits.
            if (pos.x + size.x > bounds.width)
                pos.x -= size.x;
            if (pos.y + size.y > bounds.height)
                pos.y -= size.y;

            pos.x = Mathf.Clamp(pos.x, 0f, Mathf.Max(0f, bounds.width - size.x));
            pos.y = Mathf.Clamp(pos.y, 0f, Mathf.Max(0f, bounds.height - size.y));

            m_tooltipRt.anchoredPosition = pos;
        }

        private void DestroyTooltip()
        {
            if (m_tooltipObj != null)
                GameObject.Destroy(m_tooltipObj);

            m_tooltipObj   = null;
            m_tooltipRt    = null;
            m_rootCanvas   = null;
            m_rootCanvasRt = null;
        }

        private static Canvas GetRootCanvas(GameObject obj)
        {
            Canvas[] canvases = obj.GetComponentsInParent<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].isRootCanvas)
                    return canvases[i];
            }
            return canvases.Length > 0 ? canvases[canvases.Length - 1] : null;
        }
    }
}
