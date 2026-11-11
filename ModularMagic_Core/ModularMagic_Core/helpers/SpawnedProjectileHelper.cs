using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The projectiles that a SpawnAbility spawns for a staff (the meteors, bolts, boulders and shards of the Rain rune). The game
    /// spawns them without hit data, so they would deal the damage of their prefab. A staff mod registers the prefab name of such a
    /// projectile with a callback, and Core calls it for every projectile of that prefab right after the game has set it up. The
    /// callback sets the damage on that one projectile (not on the prefab), so it is always the damage of the staff that cast the rain.
    /// </summary>
    public static class SpawnedProjectileHelper
    {
        private static readonly Dictionary<string, Action<Projectile, ItemData>> registered = new Dictionary<string, Action<Projectile, ItemData>>();

        /// <summary>
        /// Registers a projectile. Call it once at startup (in Awake, after the prefabs are loaded).
        /// </summary>
        /// <param name="prefabName">The name of the projectile prefab the SpawnAbility spawns.</param>
        /// <param name="apply">Called with the spawned projectile and the staff that cast the rain (can be null).</param>
        public static void Register(string prefabName, Action<Projectile, ItemData> apply)
        {
            if (string.IsNullOrEmpty(prefabName) || apply == null)
            {
                Jotunn.Logger.LogWarning("[SpawnedProjectile] Cannot register a projectile without a prefab name and a callback");
                return;
            }

            if (registered.ContainsKey(prefabName))
            {
                Jotunn.Logger.LogWarning($"[SpawnedProjectile] '{prefabName}' is already registered, the second registration is ignored");
                return;
            }

            registered[prefabName] = apply;
        }

        internal static void OnProjectileSpawned(Projectile projectile, ItemData staff)
        {
            try
            {
                if (projectile == null)
                    return;

                if (registered.TryGetValue(projectile.name.Replace("(Clone)", ""), out Action<Projectile, ItemData> apply))
                    apply(projectile, staff);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set up the spawned projectile: " + e);
            }
        }
    }
}
