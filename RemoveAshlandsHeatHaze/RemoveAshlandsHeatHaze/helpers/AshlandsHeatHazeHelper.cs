using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RemoveAshlandsHeatHaze.Helpers
{
    // Suppresses Ashlands weather's screen-wide heat-distortion VFX (vfx_Ashlands_HeatDistortion).
    // It's not spawned/despawned - it's a persistent scene object that vanilla just toggles via
    // SetActive/particle-emission, so we find it once by name and force-disable it whenever it's
    // active. Never Destroy() it: it's unconfirmed whether it lives in EnvSetup.m_envObject (safe
    // to destroy) or m_psystems (destroying it throws on future weather transitions, since
    // EnvMan.SetParticleArrayEnabled has no null/destroyed guard).
    internal static class AshlandsHeatHazeHelper
    {
        private const string NameFilter = "HeatDistortion";

        private static readonly List<GameObject> _targets = new List<GameObject>();
        private static bool _found;
        private static float _nextSearchTime;

        public static void SuppressIfActive(string envName)
        {
            if (!_found)
            {
                bool inAshlands = Player.m_localPlayer != null
                    && Player.m_localPlayer.GetCurrentBiome() == Heightmap.Biome.AshLands;

                if (inAshlands && Time.unscaledTime >= _nextSearchTime)
                {
                    FindTargets();
                    _nextSearchTime = Time.unscaledTime + 3f;
                }

                if (!_found)
                    return;
            }

            foreach (GameObject target in _targets)
            {
                if (target != null && target.activeSelf)
                {
                    Jotunn.Logger.LogWarning($"[RemoveAshlandsHeatHaze] Suppressing '{target.name}' (weather: {envName}).");
                    target.SetActive(false);
                }
            }
        }

        private static void FindTargets()
        {
            Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();

            foreach (Transform t in all)
            {
                // Resources.FindObjectsOfTypeAll also returns loaded prefab/asset objects that
                // were never placed in a scene - skip those, we only want the live instance.
                if (!t.gameObject.scene.IsValid())
                    continue;

                if (t.name.IndexOf(NameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (!_targets.Contains(t.gameObject))
                    _targets.Add(t.gameObject);
            }

            if (_targets.Count > 0)
            {
                _found = true;
                Jotunn.Logger.LogWarning("[RemoveAshlandsHeatHaze] Found heat-haze object(s): " +
                    string.Join(", ", _targets.Select(g => g.name)));
            }
        }
    }
}
