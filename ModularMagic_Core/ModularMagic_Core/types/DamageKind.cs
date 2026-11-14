namespace ModularMagic_Core.Types
{
    /// <summary>
    /// A damage type that a damage conversion reads from or writes to, see DamageConversionRule.
    /// </summary>
    public enum DamageKind
    {
        Blunt,
        Slash,
        Pierce,
        Chop,
        Pickaxe,
        Fire,
        Frost,
        Lightning,
        Poison,
        Spirit,

        /// <summary>
        /// Only as the source of a rule, not as its target. It is all of the blunt, pierce and slash damage together, and converting it takes
        /// away all three. A staff has just one of them (a damage type rune moves the main damage from one to another), so for a staff it
        /// is the one that is set, whichever it is. A conversion with a MainPhysical rule ignores its rules from blunt, pierce or slash.
        /// </summary>
        MainPhysical,
    }
}
