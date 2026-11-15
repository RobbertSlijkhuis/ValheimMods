using UnityEngine;

namespace ModularMagic_BloodMagic.Models
{
    class CustomPrefabs
    {
        public GameObject Scythe1;
        public GameObject Scythe2;
        public GameObject Scythe3;
        public GameObject SpawnAbilityScythe1;
        public GameObject SpawnAbilityScythe2;
        public GameObject SpawnAbilityScythe3;
        public GameObject SporeEffect;
        public GameObject SpikeEffect;

        // The secondary attacks of the runes: the wrapper item whose attack Core copies onto the scythe, and the local object the attack
        // spawns when it fires (it has no ZNetView, so only the caster has it). Null when they are not in the asset bundle
        public GameObject SecondaryAttackNova;
        public GameObject AttackNova;
        public GameObject SecondaryAttackBloodTap;
        public GameObject AttackBloodTap;

        // The start effects of these secondary attacks (m_startEffect of the wrapper's attack)
        public GameObject NovaEffect;
        public GameObject BloodTapEffect;
    }
}
