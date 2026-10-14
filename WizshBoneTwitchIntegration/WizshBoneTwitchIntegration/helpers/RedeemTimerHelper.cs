using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Owns the timer-label side of the "Show safezone bounds" debug toggle: every IRedeemTimer
    // (TwitchPersistentDestruction, TwitchTerrainReset) with a running countdown registers itself
    // here, and gets a RedeemTimerLabel added/removed as the toggle changes. Purely client-local.
    internal static class RedeemTimerHelper
    {
        private static readonly HashSet<IRedeemTimer> s_tracked = new HashSet<IRedeemTimer>();

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
        public static void Register(IRedeemTimer timer)
        {
            s_tracked.Add(timer);

            if (Show)
                Attach(timer);
        }

        public static void Unregister(IRedeemTimer timer)
        {
            s_tracked.Remove(timer);
        }

        // Called from PluginConfig.configShowSafeZoneDebug.SettingChanged so toggling the option
        // live shows/hides the timers on everything currently counting down.
        public static void SetVisible(bool show)
        {
            foreach (IRedeemTimer timer in s_tracked)
            {
                // Unity's overloaded == only applies through UnityEngine.Object, hence the cast.
                if ((Object)timer == null)
                    continue;

                RedeemTimerLabel label = timer.gameObject.GetComponent<RedeemTimerLabel>();

                if (show && label == null)
                    Attach(timer);
                else if (!show && label != null)
                    Object.Destroy(label);
            }
        }

        private static void Attach(IRedeemTimer timer)
        {
            if (timer.gameObject.GetComponent<RedeemTimerLabel>() != null)
                return;

            timer.gameObject.AddComponent<RedeemTimerLabel>().Initialize(timer);
        }
    }
}
