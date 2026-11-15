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

        // The ZDO value the game multiplies into every attack of a creature (Character.GetRandomSkillFactor), it is synced to the other clients
        private static readonly int damageFactorKey = ZDOVars.s_randomSkillFactor;

        /// <summary>
        /// Sets the damage of a summoned creature with one factor that the game multiplies into every attack of the creature: an attack that
        /// deals 'baselineDamage' with the factor at 1 deals 'damage' now, and every other attack deals the same fraction of its own damage,
        /// so the attacks keep their strength compared to each other. The factor is stored in the ZDO of the creature, so it is the same on
        /// every client, also when another client becomes the owner of the creature. Call it on the client that spawned the creature.
        /// </summary>
        /// <param name="creature">The spawned creature.</param>
        /// <param name="damage">The damage the baseline attack must deal.</param>
        /// <param name="baselineDamage">The damage the baseline attack deals with the factor at 1 (the attack that is scaled to 'damage').</param>
        /// <param name="multiplyExisting">
        /// True: multiply the factor into the one that is already on the creature, for example the one a SpawnAbility sets from the skill of the
        /// caster (m_copySkill), so both count. False: replace it.
        /// </param>
        public static void SetDamage(Character creature, float damage, float baselineDamage, bool multiplyExisting = false)
        {
            if (creature == null || baselineDamage <= 0f)
                return;

            ZNetView nview = creature.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid())
                return;

            float factor = damage / baselineDamage;

            if (multiplyExisting)
                factor *= nview.GetZDO().GetFloat(damageFactorKey, 1f);

            nview.GetZDO().Set(damageFactorKey, factor);
        }

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
