using ModularMagic_Core.Configs;
using static ItemDrop;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The base stats of a staff without imbuements, which the imbuements are applied on top of.
    /// The numbers are read from the config every time and are not copied at startup, so the values a server
    /// synced after the game started are used as well. A stat the staff has no config entry for is 0.
    /// </summary>
    public class ItemDataSnapShot
    {
        private readonly StaffConfig config;

        // The item data of the staff prefab, its attack is copied for every equipped staff
        public readonly ItemData mainAttack;

        public float attackEitr => config.useEitr != null ? config.useEitr.Value : 0f;
        public float damageBlunt => config.damageBlunt != null ? config.damageBlunt.Value : 0f;
        public float damageBluntPerLevel => config.damageBluntPerLevel != null ? config.damageBluntPerLevel.Value : 0f;
        public float damagePierce => config.damagePierce != null ? config.damagePierce.Value : 0f;
        public float damagePiercePerLevel => config.damagePiercePerLevel != null ? config.damagePiercePerLevel.Value : 0f;
        public float projectileAccuracy => config.projectileAccuracy != null ? config.projectileAccuracy.Value : 0f;
        public float projectileBurst => config.projectileBurst != null ? config.projectileBurst.Value : 0f;
        public float projectileVelocity => config.projectileVelocity != null ? config.projectileVelocity.Value : 0f;

        // The config has to be bound before a snapshot is created
        public ItemDataSnapShot(ItemData itemData, StaffConfig config)
        {
            this.config = config;
            mainAttack = itemData;
        }
    }
}
