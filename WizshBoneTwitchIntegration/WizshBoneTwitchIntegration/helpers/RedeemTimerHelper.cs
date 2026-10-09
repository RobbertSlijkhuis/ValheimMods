using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Owns the timer-label side of the "Show safezone bounds" debug toggle: every
    // TwitchPersistentDestruction with a running countdown registers itself here, and gets a
    // RedeemTimerLabel added/removed as the toggle changes. Purely client-local.
    internal static class RedeemTimerHelper
    {
        private static readonly HashSet<TwitchPersistentDestruction> s_tracked = new HashSet<TwitchPersistentDestruction>();

        // Shared screen-space overlay canvas every RedeemTimerLabel lives on. Created on first use and
        // re-created if it was destroyed (e.g. by a scene change) - the labels on it die with it.
        private static Canvas s_canvas;

        private static bool Show => PluginConfig.configShowSafeZoneDebug.Value;

        public static Canvas GetCanvas()
        {
            if (s_canvas == null)
            {
                GameObject root = new GameObject("WBTI_RedeemTimerCanvas", typeof(Canvas), typeof(CanvasScaler));

                Canvas canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;

                // Scales with screen height so the labels read the same at 1080p and 4K. No
                // GraphicRaycaster on purpose - the labels must never block clicks.
                CanvasScaler scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f;

                s_canvas = canvas;
            }

            return s_canvas;
        }

        // Called when the object's countdown starts (fresh redeem or reloaded from its ZDO).
        public static void Register(TwitchPersistentDestruction destruction)
        {
            s_tracked.Add(destruction);

            if (Show)
                Attach(destruction);
        }

        public static void Unregister(TwitchPersistentDestruction destruction)
        {
            s_tracked.Remove(destruction);
        }

        // Called from PluginConfig.configShowSafeZoneDebug.SettingChanged so toggling the option
        // live shows/hides the timers on everything currently counting down.
        public static void SetVisible(bool show)
        {
            foreach (TwitchPersistentDestruction destruction in s_tracked)
            {
                if (destruction == null)
                    continue;

                RedeemTimerLabel label = destruction.GetComponent<RedeemTimerLabel>();

                if (show && label == null)
                    Attach(destruction);
                else if (!show && label != null)
                    Object.Destroy(label);
            }
        }

        private static void Attach(TwitchPersistentDestruction destruction)
        {
            if (destruction.GetComponent<RedeemTimerLabel>() != null)
                return;

            destruction.gameObject.AddComponent<RedeemTimerLabel>().Initialize(destruction);
        }
    }
}
