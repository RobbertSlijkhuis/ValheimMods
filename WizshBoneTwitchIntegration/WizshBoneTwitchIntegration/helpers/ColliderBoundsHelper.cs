using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Owns the "Show safezone bounds" debug toggle for anything that wants its collider outlined
    // (safezones, weather zones, ...). A GameObject with a Box/Capsule/Sphere collider calls
    // Register() when it comes alive and Unregister() when it dies; the ColliderBoundsVisual is
    // then added/removed automatically as the toggle changes. An object can also pass an "is active"
    // predicate (e.g. a safezone that a profile setting has switched off), which Reevaluate() re-checks
    // whenever the toggle or the profile settings change. Purely client-local.
    internal static class ColliderBoundsHelper
    {
        public static readonly Color SafeZoneColor = new Color(0.2f, 1f, 0.3f, 0.9f);
        public static readonly Color WeatherZoneColor = new Color(1f, 0.25f, 0.2f, 0.9f);
        public static readonly Color TimeStopColor = new Color(0.3f, 0.7f, 1f, 0.9f);

        private class Entry
        {
            public Color color;
            public Func<bool> isActive;

            public bool ShouldShow => Show && (isActive == null || isActive());
        }

        private static readonly Dictionary<GameObject, Entry> s_tracked = new Dictionary<GameObject, Entry>();

        // The config entry is bound in plugin Awake; ProfileSettingsHelper can reach Reevaluate()
        // before that on a very early reload, hence the null check.
        private static bool Show => PluginConfig.configShowSafeZoneDebug?.Value == true;

        // Call once the object's collider has its final size - if the outline should currently be
        // shown, it is built immediately from the collider's current dimensions. Later resizes need
        // Refresh(). isActive (optional) gates the outline on top of the toggle.
        public static void Register(GameObject gameObject, Color color, Func<bool> isActive = null)
        {
            Entry entry = new Entry { color = color, isActive = isActive };
            s_tracked[gameObject] = entry;

            if (entry.ShouldShow)
                Attach(gameObject, color);
        }

        public static void Unregister(GameObject gameObject)
        {
            s_tracked.Remove(gameObject);
        }

        // Call after resizing a tracked object's collider so its outline matches again.
        public static void Refresh(GameObject gameObject)
        {
            gameObject.GetComponent<ColliderBoundsVisual>()?.Refresh();
        }

        // Re-checks the toggle and every entry's "is active" predicate, adding/removing outlines to
        // match. Called from PluginConfig.configShowSafeZoneDebug.SettingChanged and from
        // ProfileSettingsHelper.ApplyToLiveComponents, so both toggling the option and editing a
        // profile setting that switches a zone on/off are reflected live on everything tracked.
        public static void Reevaluate()
        {
            foreach (KeyValuePair<GameObject, Entry> pair in s_tracked)
            {
                if (pair.Key == null)
                    continue;

                ColliderBoundsVisual visual = pair.Key.GetComponent<ColliderBoundsVisual>();
                bool shouldShow = pair.Value.ShouldShow;

                if (shouldShow && visual == null)
                    Attach(pair.Key, pair.Value.color);
                else if (!shouldShow && visual != null)
                    UnityEngine.Object.Destroy(visual);
            }
        }

        private static void Attach(GameObject gameObject, Color color)
        {
            if (gameObject.GetComponent<ColliderBoundsVisual>() != null)
                return;

            gameObject.AddComponent<ColliderBoundsVisual>().Initialize(color);
        }
    }
}
