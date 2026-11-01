namespace ModularMagic_IceStaffs.Models
{
    /// <summary>
    /// The snapshots of the imbuable staffs. They are created in _AddIceStaffs, when the config is ready,
    /// so a staff that has no snapshot yet (or where creating the staffs failed) is not handled as an imbuable Ice staff.
    /// </summary>
    internal class ItemDataSnapshots
    {
        public ItemDataSnapShot staffIce1;
        public ItemDataSnapShot staffIce2;
        public ItemDataSnapShot staffIce3;
    }
}
