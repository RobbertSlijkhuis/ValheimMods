using UnityEngine;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The options of the config of the cone main attack of a staff mod, see MainAttackConfig.
    /// </summary>
    public class MainAttackConfigOptions
    {
        // The wrapper prefab that holds the attack (its attack replaces the main attack of the staff on equip)
        public GameObject prefab;
        // The AOE the attack spawns and that carries the damage
        public GameObject aoePrefab;
        public string sectionName;

        // The eitr cost of one burst as a multiplier of the eitr cost of the staff's attack. The cone fires many bursts, so this is a small part of it
        public float eitrMultiplier = 0.1f;
        public float burstInterval = 0.2f;
        // The damage of one burst as a multiplier of the damage of the staff
        public float damageMultiplier = 0.1f;
        // Describes what the damage multiplier applies to when it is not simply the damage of the staff (for example the cone of Earth is poison and spirit damage)
        public string damageDescription = "The damage of every burst of the cone as a multiplier of the damage of the staff (including quality). 1 is the damage of the staff, 0.1 is a tenth";

        public MainAttackConfigOptions(GameObject prefab, GameObject aoePrefab, string name)
        {
            this.prefab = prefab;
            this.aoePrefab = aoePrefab;
            this.sectionName = $"{name.Replace("'", "")}";
        }
    }
}
