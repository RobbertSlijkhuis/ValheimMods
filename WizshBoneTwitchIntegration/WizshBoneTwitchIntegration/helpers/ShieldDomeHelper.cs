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

        private static TwitchShieldDomeEffect GetOrCreateEffect()
        {
            try
            {
                Camera cam = Camera.main;
                if (cam == null)
                {
                    Jotunn.Logger.LogWarning("ShieldDomeHelper: Camera.main is null, cannot show/hide shield dome");
                    return null;
                }

                TwitchShieldDomeEffect effect = cam.gameObject.GetComponent<TwitchShieldDomeEffect>();
                if (effect == null)
                    effect = cam.gameObject.AddComponent<TwitchShieldDomeEffect>();

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
