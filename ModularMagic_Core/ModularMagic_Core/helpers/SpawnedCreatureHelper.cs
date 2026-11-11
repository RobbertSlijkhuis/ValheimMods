using System;
using System.Collections.Generic;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The creatures that a SpawnAbility spawns for a staff (the summon of the Creatures rune). A staff mod registers the prefab name of
    /// such a creature with a callback, and Core calls it for every creature of that prefab right after the game has spawned it. The
    /// callback gets the staff that cast the summon, so the creature can get its strength from that staff.
    /// </summary>
    public static class SpawnedCreatureHelper
    {
        private static readonly Dictionary<string, Action<Character, ItemData>> registered = new Dictionary<string, Action<Character, ItemData>>();

        /// <summary>
        /// Registers a creature. Call it once at startup (in Awake, after the prefabs are loaded).
        /// </summary>
        /// <param name="prefabName">The name of the creature prefab the SpawnAbility spawns.</param>
        /// <param name="apply">Called with the spawned creature and the staff that cast the summon (can be null).</param>
        public static void Register(string prefabName, Action<Character, ItemData> apply)
        {
            if (string.IsNullOrEmpty(prefabName) || apply == null)
            {
                Jotunn.Logger.LogWarning("[SpawnedCreature] Cannot register a creature without a prefab name and a callback");
                return;
            }

            if (registered.ContainsKey(prefabName))
            {
                Jotunn.Logger.LogWarning($"[SpawnedCreature] '{prefabName}' is already registered, the second registration is ignored");
                return;
            }

            registered[prefabName] = apply;
        }

        internal static void OnCreatureSpawned(Character creature, ItemData staff)
        {
            try
            {
                if (creature == null)
                    return;

                if (registered.TryGetValue(creature.name.Replace("(Clone)", ""), out Action<Character, ItemData> apply))
                    apply(creature, staff);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set up the spawned creature: " + e);
            }
        }
    }
}
