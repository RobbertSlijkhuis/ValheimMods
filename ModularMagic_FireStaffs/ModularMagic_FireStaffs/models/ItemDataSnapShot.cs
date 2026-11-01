using ModularMagic_FireStaffs.Configs;
using static ItemDrop;

namespace ModularMagic_FireStaffs.Models
{
    /// <summary>
    /// The base stats of a staff without imbuements, which the imbuements are applied on top of.
    /// The numbers are read from the config every time and are not copied at startup, so the values a server
    /// synced after the game started are used as well.
    /// </summary>
    internal class ItemDataSnapShot
    {
        private readonly StaffConfig config;

        // The item data of the staff prefab, its attack is copied for every equipped staff
        public readonly ItemData mainAttack;

        public float attackEitr => config.useEitr.Value;
        public float projectileAccuracy => config.projectileAccuracy.Value;
        public float projectileVelocity => config.projectileVelocity.Value;

        // The config has to be bound before a snapshot is created
        public ItemDataSnapShot(ItemData itemData, StaffConfig config)
        {
            this.config = config;
            mainAttack = itemData;
        }
    }
}
