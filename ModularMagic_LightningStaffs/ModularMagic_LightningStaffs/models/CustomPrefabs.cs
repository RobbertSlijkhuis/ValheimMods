using UnityEngine;

namespace ModularMagic_LightningStaffs.Models
{
    class CustomPrefabs
    {
        // Staffs
        public GameObject staffLightning1Prefab;
        public GameObject staffLightning2Prefab;
        public GameObject staffLightning3Prefab;

        // The nova rune: the AOE has its own damage, the wrapper holds the secondary attack that is copied to the staff
        public GameObject attackNova;
        public GameObject secondaryAttackNova;

        // The cone rune: the AOE has its own damage, the wrapper holds the main attack that replaces the main attack of the staff
        public GameObject mainAttackCone;
        public GameObject attackCone;

        // The rain rune (lightning bolts): the wrapper holds the attack, the spawn projectile lands, its script spawns the bolts
        public GameObject secondaryAttackRain;
        public GameObject projectileRainSpawn;
        public GameObject scriptBolts;
        public GameObject projectileBolt;
    }
}
