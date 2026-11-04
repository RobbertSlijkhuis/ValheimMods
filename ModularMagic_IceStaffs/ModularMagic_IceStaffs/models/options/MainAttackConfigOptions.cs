using UnityEngine;

namespace ModularMagic_IceStaffs.Models
{
    internal class MainAttackConfigOptions
    {
        // The wrapper prefab that holds the attack (its attack replaces the main attack of the staff on equip)
        public GameObject prefab;
        // The AOE the attack spawns and that carries the damage
        public GameObject aoePrefab;
        public string sectionName;

        // The eitr cost of one burst. A staff has its own cost per shot, the cone fires many bursts
        public int useEitr = 5;
        public float burstInterval = 0.4f;
        public float damageFrost = 0f;
        public float attackForce = 0f;

        public MainAttackConfigOptions(GameObject prefab, GameObject aoePrefab, string name)
        {
            this.prefab = prefab;
            this.aoePrefab = aoePrefab;
            this.sectionName = $"{name.Replace("'", "")}";
        }
    }
}
