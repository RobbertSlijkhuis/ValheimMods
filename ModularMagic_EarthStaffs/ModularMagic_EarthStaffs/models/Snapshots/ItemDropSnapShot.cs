using static ItemDrop;

namespace ModularMagic_EarthStaffs.Models
{
    internal class ItemDataSnapShot
    {
        public float? attackEitr;
        public float? damageBlunt;
        public float? damagePierce;
        public float? damageSlash;
        public float? damageBluntPerlevel;
        public float? damagePiercePerlevel;
        public float? damageSlashPerlevel;
        public ItemData mainAttack;
        public float? projectileAccuracy;
        public float? projectileBurst;
        public float? projectileVelocity;
        public ItemData secondaryAttack;

        public void Init(ItemData itemData)
        {
            attackEitr = itemData.m_shared.m_attack.m_attackEitr;
            damageBlunt = itemData.m_shared.m_damages.m_blunt;
            damagePierce = itemData.m_shared.m_damages.m_pierce;
            damageSlash = itemData.m_shared.m_damages.m_slash;
            damageBluntPerlevel = itemData.m_shared.m_damagesPerLevel.m_blunt;
            damagePiercePerlevel = itemData.m_shared.m_damagesPerLevel.m_pierce;
            damageSlashPerlevel = itemData.m_shared.m_damagesPerLevel.m_slash;
            mainAttack = itemData;
            projectileAccuracy = itemData.m_shared.m_attack.m_projectileAccuracy;
            projectileBurst = itemData.m_shared.m_attack.m_burstInterval;
            projectileVelocity = itemData.m_shared.m_attack.m_projectileVel;
            secondaryAttack = itemData;
        }
    }
}
