using UnityEngine;

namespace ModularMagic_FireStaffs.Models
{
    class CustomPrefabs
    {
        // Staffs
        public GameObject staffFire1Prefab;
        public GameObject staffFire2Prefab;
        public GameObject staffFire3Prefab;

        // The nova rune: the AOE has its own damage, the wrapper holds the secondary attack that is copied to the staff
        public GameObject attackNova;
        public GameObject secondaryAttackNova;

        // The rain rune (meteors): the wrapper holds the attack, the spawn projectile lands, its script spawns the meteors
        public GameObject secondaryAttackRain;
        public GameObject projectileRainSpawn;
        public GameObject scriptMeteors;
        public GameObject projectileMeteor;
    }
}
