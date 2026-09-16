using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Bottom-right stack of auto-dismissing feedback messages, matching RedesignUI.dc.html's
    /// toast pattern. Nothing like this exists in GUI_OLD (its tabs just overwrite a static
    /// feedback <see cref="Text"/>), so this is new infrastructure, not a port - built as shared
    /// GUI/ infra since every planned tab's mockup leans on the same pattern, not just Profiles.
    /// </summary>
    internal static class ToastNotifications
    {
        private const float ToastWidth    = 320f;
        private const float ToastHeight   = 50f;
        private const float ToastSpacing  = 8f;
        private const float BottomMargin  = 20f;
        private const float RightMargin   = 20f;
        private const float DisplaySeconds = 3.5f;

        private static GameObject s_container;

        // Newest toast is inserted at index 0 and positioned closest to the bottom margin -
        // older toasts get pushed upward as new ones arrive, matching typical toast-stack UX.
        private static readonly List<GameObject> s_activeToasts = new List<GameObject>();

        /// <summary>
        /// Creates the toast stack's container once, anchored to the bottom-right of
        /// <paramref name="panel"/>. Call once when the shell panel is built, alongside
        /// sidebar/top bar creation.
        /// </summary>
        public static void Init(GameObject panel)
        {
            if (s_container != null)
                return;

            s_container = new GameObject("ToastContainer");
            s_container.transform.SetParent(panel.transform, false);

            RectTransform rt = s_container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(1f, 0f);
            rt.sizeDelta = new Vector2(ToastWidth, 0f);
            rt.anchoredPosition = new Vector2(-RightMargin, BottomMargin);

            // Always draw above the rest of the shell's content.
            s_container.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Queues a toast with the given message. Safe to call from any tab once
        /// <see cref="Init"/> has run.
        /// </summary>
        public static void Show(string message)
        {
            if (s_container == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] ToastNotifications.Show called before Init().");
                return;
            }

            s_container.transform.SetAsLastSibling();

            GameObject toastObj = new GameObject("Toast");
            toastObj.transform.SetParent(s_container.transform, false);

            RectTransform toastRt = toastObj.AddComponent<RectTransform>();
            toastRt.anchorMin = new Vector2(1f, 0f);
            toastRt.anchorMax = new Vector2(1f, 0f);
            toastRt.pivot     = new Vector2(1f, 0f);
            toastRt.sizeDelta = new Vector2(ToastWidth, ToastHeight);

            Image background = toastObj.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.85f);
            GuiHelper.AddBorder(toastObj, GUIManager.Instance.ValheimOrange);

            Text text = GUIManager.Instance.CreateText(
                text:                message,
                parent:              toastObj.transform,
                anchorMin:           Vector2.zero,
                anchorMax:           Vector2.one,
                position:            Vector2.zero,
                font:                GUIManager.Instance.AveriaSerifBold,
                fontSize:            13,
                color:               GUIManager.Instance.ValheimBeige,
                outline:             true,
                outlineColor:        Color.black,
                width:               ToastWidth - 20f,
                height:              ToastHeight - 10f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;

            s_activeToasts.Insert(0, toastObj);
            RepositionAll();

            ToastEntryTimer timer = toastObj.AddComponent<ToastEntryTimer>();
            timer.BeginExpiry(DisplaySeconds, () => Remove(toastObj));
        }

        private static void Remove(GameObject toastObj)
        {
            if (toastObj == null)
                return;

            s_activeToasts.Remove(toastObj);
            GameObject.Destroy(toastObj);
            RepositionAll();
        }

        private static void RepositionAll()
        {
            for (int i = 0; i < s_activeToasts.Count; i++)
            {
                RectTransform rt = s_activeToasts[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, i * (ToastHeight + ToastSpacing));
            }
        }
    }

    /// <summary>
    /// Lives on one toast entry; destroys/removes it after a fixed delay. A plain
    /// <see cref="MonoBehaviour"/> timer, mirroring how <see cref="SearchableDropdownHandle"/>
    /// and other GUI/ components already use small dedicated components for lifecycle hooks.
    ///
    /// Deliberately an unscaled-time deadline checked from <see cref="Update"/> rather than a
    /// <c>StartCoroutine</c>/<c>WaitForSeconds</c> pair - a coroutine is killed outright the
    /// moment this object (ToastContainer is a direct child of the shell's main panel - see
    /// <see cref="WizshBoneShellGUI.BuildGUI"/>) or any ancestor is deactivated, e.g. closing the
    /// shell (F3) while a toast is still showing, and Unity does NOT resume a killed coroutine
    /// when the object is reactivated - the toast's dismiss callback would then never fire and
    /// it would sit on screen forever the next time the shell opens. Update() has no such
    /// problem: it simply stops ticking while inactive and, once reactivated, immediately fires
    /// the (already-overdue) expiry instead of losing it.
    /// </summary>
    internal class ToastEntryTimer : MonoBehaviour
    {
        private float  m_expireAtUnscaledTime;
        private Action m_onExpired;
        private bool   m_pending;

        internal void BeginExpiry(float delaySeconds, Action onExpired)
        {
            m_onExpired = onExpired;
            m_expireAtUnscaledTime = Time.unscaledTime + delaySeconds;
            m_pending = true;
        }

        private void Update()
        {
            if (!m_pending || Time.unscaledTime < m_expireAtUnscaledTime)
                return;

            m_pending = false;
            m_onExpired?.Invoke();
        }
    }
}
