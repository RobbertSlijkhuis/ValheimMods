#nullable enable

using UnityEngine;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The options of the config of a secondary attack of a staff mod, see SecondaryAttackConfig. A value that is null is not bound as a config entry.
    /// </summary>
    public class SecondaryAttackConfigOptions
    {
        // The wrapper prefab that holds the attack (its attack is copied to the staff on equip)
        public GameObject prefab;
        // The prefab the attack spawns and that carries the damage: the area hit (nova), the projectile (rain) or the summoned creature
        public GameObject secondPrefab;
        // See SecondaryAttackType, this decides how the values are applied to secondPrefab
        public string type;
        public string sectionName;
        public string cooldownStatusEffectName;

        public int useEitr = 100;
        public float cooldown = 30f;

        // Only for attacks whose eitr cost is a multiplier of the eitr cost of the staff's attack, instead of the fixed useEitr
        public float? eitrMultiplier;
        // Only for attacks that use the damage and the attack force of the staff (the nova and the rain projectiles): the multipliers of those.
        // These attacks have no damage or attack force of their own, apart from the chop and pickaxe damage of the projectiles
        public float? damageMultiplier;
        public float? forceMultiplier;
        // Only for attacks that summon creatures
        public float? health;
        public int? maxSpawns;
        public float? aoe;
        // Only for attacks that deal this type of damage themselves
        public float? damageBlunt;
        public float? damageChop;
        public float? damageGeneral;
        public float? damagePickaxe;
        public float? damagePierce;
        public float? damagePoison;
        public float? damageSpirit;
        public float? attackForce;
        // Only for attacks that spawn several projectiles or creatures with a SpawnAbility. This is the prefab that has it, when it is not the
        // projectile of the attack of the wrapper prefab
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
