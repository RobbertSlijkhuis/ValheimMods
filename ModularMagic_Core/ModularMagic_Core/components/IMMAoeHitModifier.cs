namespace ModularMagic_Core.Components
{
    /// <summary>
    /// A component on the same prefab as an MMAoe that changes the hit data of the attack before the area is hit, for example to
    /// deal the damage as other damage types. The hit data holds the damage of the staff, already scaled by the multipliers of the attack.
    /// </summary>
    public interface IMMAoeHitModifier
    {
        void Modify(HitData hitData);
    }
}
