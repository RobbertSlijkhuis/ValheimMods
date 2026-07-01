using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class ShieldColors
    {
        public static readonly Color TimeStop = new Color(0.4f, 0.85f, 1f);
        public static readonly Color Ward = new Color(0.57f, 0.27f, 1f);
    }

    internal static class ShieldDomeHelper
    {
        public static void ShowDome(object key, Vector3 position, float radius, Color color)
        {
            TwitchShieldDomeEffect effect = GetOrCreateEffect();
            effect?.SetDome(key, position, radius, color);
        }

        public static void HideDome(object key)
        {
            TwitchShieldDomeEffect effect = GetOrCreateEffect();
            effect?.RemoveDome(key);
        }

        public static void BreakDome(object key)
        {
            TwitchShieldDomeEffect effect = GetOrCreateEffect();
            effect?.BreakDome(key);
        }

        // Cached across calls so per-frame callers (e.g. a TimeStop zone following a boat/tame)
        // don't pay for a Camera.main tag lookup + GetComponent every frame. Unity's overloaded
        // == treats a destroyed camera/effect as null, so this naturally re-resolves if the main
        // camera is ever swapped out.
        private static TwitchShieldDomeEffect s_cachedEffect;

        private static TwitchShieldDomeEffect GetOrCreateEffect()
        {
            try
            {
                if (s_cachedEffect != null)
                    return s_cachedEffect;

                Camera cam = Camera.main;
                if (cam == null)
                {
                    Jotunn.Logger.LogWarning("ShieldDomeHelper: Camera.main is null, cannot show/hide shield dome");
                    return null;
                }

                TwitchShieldDomeEffect effect = cam.gameObject.GetComponent<TwitchShieldDomeEffect>();
                if (effect == null)
                    effect = cam.gameObject.AddComponent<TwitchShieldDomeEffect>();

                s_cachedEffect = effect;
                return effect;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("ShieldDomeHelper.GetOrCreateEffect failed: " + e);
                return null;
            }
        }
    }
}
