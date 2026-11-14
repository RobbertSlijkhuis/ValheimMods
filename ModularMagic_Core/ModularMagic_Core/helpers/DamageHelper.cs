using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;
using System.Collections.Generic;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Helpers for the damage of a staff.
    /// </summary>
    public static class DamageHelper
    {
        private static readonly int kindCount = Enum.GetValues(typeof(DamageKind)).Length;

        /// <summary>
        /// The main physical damage: a staff has just one of blunt, pierce and slash (a damage type rune moves the main damage from one to
        /// another), so they are never added up and the one that is set is the main damage. An item with more than one of them gets the largest.
        /// The damage conversion (DamageKind.MainPhysical) is different: it uses all three together, see DamageHelper.Convert.
        /// </summary>
        public static float GetMainPhysical(HitData.DamageTypes damage)
        {
            return UnityEngine.Mathf.Max(damage.m_blunt, UnityEngine.Mathf.Max(damage.m_pierce, damage.m_slash));
        }

        /// <summary>
        /// Converts damage of one type into another type. Every rule reads the damage as it was before any rule ran, so the rules do not
        /// chain and their order does not matter: a blunt to fire rule and a fire to frost rule never turn blunt into frost.
        /// The converted damage is taken from the source type (or left there when the rule copies it) and added to the target type.
        /// When the rules take more than all of a source type, the source is left with nothing, never with a negative amount.
        /// Every rule gets its share of the original amount, so rules that read the same source do not split it: a fire rule and a frost rule
        /// that both convert all of the blunt damage deal that amount as fire and again as frost, so the total damage goes up. Use a ratio
        /// below 1 on each of them (for example 0.5 and 0.5) to split the damage instead.
        /// A rule from MainPhysical uses all of the blunt, pierce and slash damage. When there is such a rule, the rules from blunt, pierce
        /// or slash are ignored.
        /// </summary>
        /// <param name="damage">The damage to convert. It is not changed, the converted damage is returned.</param>
        /// <param name="rules">What to convert into what.</param>
        /// <param name="keepOtherDamage">
        /// True: the damage that no rule converts stays as it is. False: only the targets of the rules are dealt, so the result holds
        /// nothing else (not even the source types, whatever 'copy' says), for an attack that replaces the damage of the staff.
        /// </param>
        public static HitData.DamageTypes Convert(HitData.DamageTypes damage, IList<DamageConversionRule> rules, bool keepOtherDamage = true)
        {
            HitData.DamageTypes result = keepOtherDamage ? damage : default;
            float[] amounts = new float[rules.Count];
            float[] taken = new float[kindCount];
            bool convertsMainPhysical = false;

            for (int i = 0; i < rules.Count; i++)
                convertsMainPhysical |= rules[i].from == DamageKind.MainPhysical;

            for (int i = 0; i < rules.Count; i++)
            {
                DamageConversionRule rule = rules[i];

                // MainPhysical already uses all of the physical damage, so a rule for one of its types is ignored
                if (convertsMainPhysical && IsPhysical(rule.from))
                    continue;

                amounts[i] = Get(damage, rule.from) * rule.ratio;

                if (!rule.copy)
                    taken[(int)rule.from] += rule.ratio;
            }

            if (keepOtherDamage)
            {
                for (int kind = 0; kind < kindCount; kind++)
                {
                    if (taken[kind] > 0f)
                        Scale(ref result, (DamageKind)kind, UnityEngine.Mathf.Max(0f, 1f - taken[kind]));
                }
            }

            for (int i = 0; i < rules.Count; i++)
                Add(ref result, rules[i].to, amounts[i]);

            return result;
        }

        private static float Get(HitData.DamageTypes damage, DamageKind kind)
        {
            switch (kind)
            {
                case DamageKind.Blunt: return damage.m_blunt;
                case DamageKind.Slash: return damage.m_slash;
                case DamageKind.Pierce: return damage.m_pierce;
                case DamageKind.Chop: return damage.m_chop;
                case DamageKind.Pickaxe: return damage.m_pickaxe;
                case DamageKind.Fire: return damage.m_fire;
                case DamageKind.Frost: return damage.m_frost;
                case DamageKind.Lightning: return damage.m_lightning;
                case DamageKind.Poison: return damage.m_poison;
                case DamageKind.Spirit: return damage.m_spirit;
                case DamageKind.MainPhysical: return damage.m_blunt + damage.m_pierce + damage.m_slash;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static bool IsPhysical(DamageKind kind)
        {
            return kind == DamageKind.Blunt || kind == DamageKind.Pierce || kind == DamageKind.Slash;
        }

        private static void Add(ref HitData.DamageTypes damage, DamageKind kind, float amount)
        {
            switch (kind)
            {
                case DamageKind.Blunt: damage.m_blunt += amount; break;
                case DamageKind.Slash: damage.m_slash += amount; break;
                case DamageKind.Pierce: damage.m_pierce += amount; break;
                case DamageKind.Chop: damage.m_chop += amount; break;
                case DamageKind.Pickaxe: damage.m_pickaxe += amount; break;
                case DamageKind.Fire: damage.m_fire += amount; break;
                case DamageKind.Frost: damage.m_frost += amount; break;
                case DamageKind.Lightning: damage.m_lightning += amount; break;
                case DamageKind.Poison: damage.m_poison += amount; break;
                case DamageKind.Spirit: damage.m_spirit += amount; break;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static void Scale(ref HitData.DamageTypes damage, DamageKind kind, float factor)
        {
            if (kind == DamageKind.MainPhysical)
            {
                damage.m_blunt *= factor;
                damage.m_slash *= factor;
                damage.m_pierce *= factor;
                return;
            }

            Add(ref damage, kind, Get(damage, kind) * (factor - 1f));
        }
    }
}
