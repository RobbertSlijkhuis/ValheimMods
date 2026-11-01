namespace ModularMagic_FireStaffs.Models
{
    /// <summary>
    /// The snapshots of the imbuable staffs. They are created in AddFireStaffs, when the config is ready,
    /// so a staff that has no snapshot yet (or where creating the staffs failed) is not handled as an imbuable Fire staff.
    /// </summary>
    internal class ItemDataSnapshots
    {
        public ItemDataSnapShot staffFire1;
        public ItemDataSnapShot staffFire2;
        public ItemDataSnapShot staffFire3;
    }
}
