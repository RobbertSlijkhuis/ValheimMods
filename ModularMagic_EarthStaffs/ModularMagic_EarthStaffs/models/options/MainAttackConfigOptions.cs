using UnityEngine;

namespace ModularMagic_EarthStaffs.Models
{
    internal class MainAttackConfigOptions
    {
        // The wrapper prefab that holds the attack (its attack replaces the main attack of the staff on equip)
        public GameObject prefab;
        // The prefab the attack spawns (see MMAoe)
        public GameObject aoePrefab;
        public string sectionName;

        // The eitr cost of one burst as a multiplier of the eitr cost of the staff's attack. The cone fires many bursts, so this is a part of it
        public float eitrMultiplier = 0.5f;
        public float burstInterval = 0.1f;
        // The damage of one burst as a multiplier of the damage of the staff
        public float damageMultiplier = 0.2f;

        public MainAttackConfigOptions(GameObject prefab, GameObject aoePrefab, string name)
        {
            this.prefab = prefab;
            this.aoePrefab = aoePrefab;
            this.sectionName = $"{name.Replace("'", "")}";
        }
    }
}
