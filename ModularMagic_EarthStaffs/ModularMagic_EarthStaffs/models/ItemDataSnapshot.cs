namespace ModularMagic_EarthStaffs.Models
{
    /// <summary>
    /// The snapshots of the staffs. They are created in AddEarthStaffs, when the config is ready,
    /// so a staff that has no snapshot yet (or where creating the staffs failed) is not handled as an imbuable Earth staff.
    /// </summary>
    internal class ItemDataSnapshots
    {
        public ItemDataSnapShot staffEarth0;
        public ItemDataSnapShot staffEarth1;
        public ItemDataSnapShot staffEarth2;
        public ItemDataSnapShot staffEarth3;
    }
}
