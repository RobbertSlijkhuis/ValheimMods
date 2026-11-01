#nullable enable
using UnityEngine;

namespace ModularMagic_FireStaffs.Models
{
    internal class SecondaryAttackConfigOptions
    {
        // The wrapper prefab that holds the attack (its attack is copied to the staff on equip)
        public GameObject prefab;
        // The prefab the attack spawns and that carries the damage: the AOE (nova) or the projectile (meteors)
        public GameObject secondPrefab;
        // See SecondaryAttackType, this decides how the values are applied to secondPrefab
        public string type;
        public string sectionName;
        public string cooldownStatusEffectName;

        public int useEitr = 100;
        public float aoe = 6f;
        public float damageFire = 0f;
        public float attackForce = 0f;
        public float cooldown = 30f;

        // Only for attacks that deal this type of damage
        public float? damageBlunt;
        public float? damageChop;
        public float? damagePickaxe;

        // Only for attacks that fire a projectile from the staff (values of the wrapper attack)
        public float? launchAngle;
        public float? projectileVelocity;
        public float? projectileAccuracy;

        // Only for attacks that spawn several projectiles with a SpawnAbility, this is the prefab that has it
        public GameObject? spawnAbilityPrefab;
        public int? minToSpawn;
        public int? maxToSpawn;
        public float? spawnRadius;

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
