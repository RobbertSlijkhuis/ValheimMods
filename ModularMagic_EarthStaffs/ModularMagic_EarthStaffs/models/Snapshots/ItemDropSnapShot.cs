using ModularMagic_EarthStaffs.Configs;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Models
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

        public float? attackEitr => config.useEitr.Value;
        public float? damageBlunt => config.damageBlunt == null ? 0f : config.damageBlunt.Value;
        public float? damageBluntPerlevel => config.damageBluntPerLevel == null ? 0f : config.damageBluntPerLevel.Value;
        public float? projectileAccuracy => config.projectileAccuracy == null ? 0f : config.projectileAccuracy.Value;
        public float? projectileBurst => config.projectileBurst == null ? 0f : config.projectileBurst.Value;
        public float? projectileVelocity => config.projectileVelocity == null ? 0f : config.projectileVelocity.Value;

        // The config has to be bound before a snapshot is created
        public ItemDataSnapShot(ItemData itemData, StaffConfig config)
        {
            this.config = config;
            mainAttack = itemData;
        }
    }
}
