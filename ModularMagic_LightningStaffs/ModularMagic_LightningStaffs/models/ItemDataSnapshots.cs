namespace ModularMagic_LightningStaffs.Models
{
    /// <summary>
    /// The snapshots of the imbuable staffs. They are created in AddLightningStaffs, when the config is ready,
    /// so a staff that has no snapshot yet (or where creating the staffs failed) is not handled as an imbuable Lightning staff.
    /// </summary>
    internal class ItemDataSnapshots
    {
        public ItemDataSnapShot staffLightning1;
        public ItemDataSnapShot staffLightning2;
        public ItemDataSnapShot staffLightning3;
    }
}
