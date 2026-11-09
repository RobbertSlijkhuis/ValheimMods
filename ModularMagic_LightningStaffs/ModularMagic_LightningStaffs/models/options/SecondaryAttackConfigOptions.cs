#nullable enable
using UnityEngine;

namespace ModularMagic_LightningStaffs.Models
{
    internal class SecondaryAttackConfigOptions
    {
        // The wrapper prefab that holds the attack (its attack is copied to the staff on equip)
        public GameObject prefab;
        // The prefab the attack spawns and that carries the damage: the area hit (nova) or the projectile (lightning bolts)
        public GameObject secondPrefab;
        // See SecondaryAttackType, this decides how the values are applied to secondPrefab
        public string type;
        public string sectionName;
        public string cooldownStatusEffectName;

        public float aoe = 6f;
        public float cooldown = 30f;

        // The eitr cost of the attack as a multiplier of the eitr cost of the staff's attack
        public float eitrMultiplier = 3.4f;

        // The damage of the attack as a multiplier of the damage of the staff. The area hit (nova) and the projectiles
        // (lightning bolts) have no damage of their own, apart from the chop and pickaxe damage of the projectiles
        public float damageMultiplier = 1f;

        // The attack force of the attack as a multiplier of the attack force of the staff
        public float forceMultiplier = 1f;

        // Only for attacks that deal this type of damage
        public float? damageChop;
        public float? damagePickaxe;

        // Only for attacks that spawn several projectiles with a SpawnAbility, this is the prefab that has it
        public GameObject? spawnAbilityPrefab;
        public int? minToSpawn;
        public int? maxToSpawn;
        public float? spawnRadius;
        // The spread of the spawned projectiles in degrees
        public float? spawnAccuracy;
        // The time in seconds between two spawned projectiles
        public float? spawnDelay;

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
