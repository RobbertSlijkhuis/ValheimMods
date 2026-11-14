using ModularMagic_Core.Types;
using System;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// Turns damage of one type into damage of another type, see DamageHelper.Convert.
    /// </summary>
    public class DamageConversionRule
    {
        // The type that is converted. MainPhysical is all of the blunt, pierce and slash damage
        public readonly DamageKind from;

        // The type that gets the converted damage (not MainPhysical). It is added to what that type already has
        public readonly DamageKind to;

        // The part of the damage that is converted: 1 is all of it, 0.5 is half of it
        public readonly float ratio;

        // The converted damage is also left on the source type, so it is dealt as both types
        public readonly bool copy;

        public DamageConversionRule(DamageKind from, DamageKind to, float ratio = 1f, bool copy = false)
        {
            if (to == DamageKind.MainPhysical)
                throw new ArgumentException("A damage conversion rule can not convert into MainPhysical, pick blunt, slash or pierce");

            if (ratio < 0f)
                throw new ArgumentException($"A damage conversion rule can not have a negative ratio ({ratio})");

            this.from = from;
            this.to = to;
            this.ratio = ratio;
            this.copy = copy;
        }
    }
}
