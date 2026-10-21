namespace ModularMagic_Armors.Types
{
    /**
     * Public and with explicit values because Unity serializes it on the attach components in the armor prefabs. Never renumber.
     */
    public enum ArmorSetId
    {
        None = 0,
        Shaman = 1,
        Wraith = 2,
        FrostWolf = 3,
        DarkWizard = 4,
    }
}
