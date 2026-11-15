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

        // The frost shot of the main attack and the ice sheet the Utility rune lets it leave on water
        public GameObject ProjectileIce;
        public GameObject IceSheet;
        public GameObject IceSheetSplash;

        // The nova secondary attack: the wrapper holds the attack, the AOE is what it spawns
        public GameObject SecondaryAttackNova;
        public GameObject AttackNova;

        // The cone rune: the AOE has its own damage, the wrapper holds the main attack that replaces the main attack of the staff
        public GameObject MainAttackCone;
        public GameObject AttackCone;

        // The rain rune (ice shards): the wrapper holds the attack, the spawn projectile lands, its script spawns the shards
        public GameObject SecondaryAttackRain;
        public GameObject ProjectileRainSpawn;
        public GameObject ScriptIceShards;
        public GameObject ProjectileIceShard;
        public GameObject ProjectileIceShardBlocker;
    }
}
