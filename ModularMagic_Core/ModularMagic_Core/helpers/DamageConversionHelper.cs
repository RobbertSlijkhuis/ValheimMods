using ModularMagic_Core.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The damage conversion of the attack prefabs of the staffs (the area hits of MMAoe), see DamageHelper.Convert.
    /// The rules are kept here by the name of the prefab and not as a field of a component, because Unity does not copy a field of a
    /// custom type to the instance it spawns (the rules came out empty).
    /// </summary>
    public static class DamageConversionHelper
    {
        private class Conversion
        {
            public DamageConversionRule[] rules;
            public bool keepOtherDamage;
        }

        private static readonly Dictionary<string, Conversion> conversions = new Dictionary<string, Conversion>();

        /// <summary>
        /// Makes the attack deal its damage as other damage types. Call it once at startup for the prefab of the attack (the one with the MMAoe).
        /// </summary>
        /// <param name="keepOtherDamage">
        /// True: the damage that no rule converts stays as it is. False: the attack only deals what the rules convert into and the rest of
        /// the damage of the staff is dropped.
        /// </param>
        /// <param name="rules">What to convert into what.</param>
        public static void SetDamageConversion(GameObject prefab, bool keepOtherDamage, params DamageConversionRule[] rules)
        {
            // A wrong rule is reported here, when the mod loads, and not by the first hit of the attack
            if (rules == null || rules.Length == 0)
                throw new ArgumentException($"The damage conversion of {prefab.name} has no rules");

            // The rules themselves are checked when they are created, see DamageConversionRule
            foreach (DamageConversionRule rule in rules)
            {
                if (rule == null)
                    throw new ArgumentException($"The damage conversion of {prefab.name} has a rule that is null");
            }

            if (conversions.ContainsKey(prefab.name))
                throw new ArgumentException($"There already is a damage conversion for a prefab named {prefab.name}");

            conversions[prefab.name] = new Conversion { rules = rules, keepOtherDamage = keepOtherDamage };
        }

        /// <summary>
        /// The damage of an attack after the conversion of its prefab. The damage is returned as it is when the prefab has no conversion.
        /// </summary>
        /// <param name="spawned">The spawned attack, named after its prefab.</param>
        internal static HitData.DamageTypes ConvertDamage(GameObject spawned, HitData.DamageTypes damage)
        {
            if (!conversions.TryGetValue(spawned.name.Replace("(Clone)", ""), out Conversion conversion))
                return damage;

            return DamageHelper.Convert(damage, conversion.rules, conversion.keepOtherDamage);
        }
    }
}
