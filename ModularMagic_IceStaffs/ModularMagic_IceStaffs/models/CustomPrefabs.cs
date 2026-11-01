using UnityEngine;

namespace ModularMagic_IceStaffs.Models
{
    class CustomPrefabs
    {
        // Staffs
        public GameObject StaffIce1Prefab;
        public GameObject StaffIce2Prefab;
        public GameObject StaffIce3Prefab;
        public GameObject StaffIceAOEPrefab;

        // The nova secondary attack: the wrapper holds the attack, the AOE is what it spawns
        public GameObject SecondaryAttackNova;
        public GameObject AttackNova;

        // The rain rune (ice shards): the wrapper holds the attack, the spawn projectile lands, its script spawns the shards
        public GameObject SecondaryAttackRain;
        public GameObject ProjectileRainSpawn;
        public GameObject ScriptIceShards;
        public GameObject ProjectileIceShard;
        public GameObject ProjectileIceShardBlocker;
    }
}
