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
        public float? health;
        public int? minToSpawn;
        public int? maxToSpawn;
        public int? maxSpawns;
        public float? spawnRadius;
        public float? aoe;
        public float? damageBlunt;
        public float? damageChop;
        public float? damagePickaxe;
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
