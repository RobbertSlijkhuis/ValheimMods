using UnityEngine;

namespace ModularMagic_EarthStaffs.Models
{
    /// <summary>
    /// The options of the config of an Earth staff. Every Earth staff has the stats below (the other staff mods bind only the stats they
    /// give a value), so they are set here and an Earth staff only sets what is different.
    /// </summary>
    internal class EarthStaffConfigOptions : StaffConfigOptions
    {
        public EarthStaffConfigOptions(GameObject prefab, string name, string recipe, string upgradeRecipe) : base(prefab, name, recipe, upgradeRecipe)
        {
            damageBlunt = 0f;
            damageBluntPerLevel = 0f;
            damageSpirit = 0f;
            damageSpiritPerLevel = 0f;
            useEitr = 10;
            projectileVelocity = 15f;
            projectileAccuracy = 1f;
            weight = 0.3f;
            maxDurability = 200f;
            movementSpeed = -0.05f;
            blockArmor = 0;
            deflectionForce = 20;
            attackForce = 20;
        }
    }
}
