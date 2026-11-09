#nullable enable
using UnityEngine;

namespace ModularMagic_EarthStaffs.Models
{
    internal class SecondaryAttackConfigOptions
    {
        public GameObject prefab;
        public GameObject secondPrefab;
        public string type;
        public string sectionName;
        public string cooldownStatusEffectName;

        public int useEitr = 100;
        public float cooldown = 30f;

        // Only for attacks whose eitr cost is a multiplier of the eitr cost of the staff's attack, instead of the fixed useEitr
        public float? eitrMultiplier;
        // Only for attacks that use the damage and the attack force of the staff (the nova and the boulders): the multipliers of those.
        // These attacks have no damage or attack force of their own, apart from the chop and pickaxe damage of the boulders
        public float? damageMultiplier;
        public float? forceMultiplier;
        public float? health;
        public int? minToSpawn;
        public int? maxToSpawn;
        public int? maxSpawns;
        public float? spawnRadius;
        public float? aoe;
        public float? damageBlunt;
        public float? damageChop;
        public float? damageGeneral;
        public float? damagePickaxe;
        public float? damagePierce;
        public float? damagePoison;
        public float? damageSpirit;
        public float? attackForce;
        public StatusEffect? attackStatusEffect;
        public float? launchAngle;
        public float? projectileVelocity;
        public float? projectileAccuracy;

        public SecondaryAttackConfigOptions(GameObject prefab, GameObject secondPrefab, string type, string name)
        {
            this.prefab = prefab;
            this.secondPrefab = secondPrefab;
            this.type = type;
            this.sectionName = $"{name.Replace("'", "")}";
            this.cooldownStatusEffectName = $"{prefab.name}CooldownStatusEffect";
        }
    }
}
