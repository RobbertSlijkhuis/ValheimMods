using PlayFab.GroupsModels;

namespace WizshBoneTwitchIntegration.Models
{
    internal class DamageData
    {
        public float? blunt;
        public float? chop;
        public float? damage;
        public float? fire;
        public float? frost;
        public float? lightning;
        public bool maxHealthArmorBased = false;
        public float? pickaxe;
        public float? pierce;
        public float? poison;
        public float? slash;
        public float? spirit;

        public DamageData() { }

        public HitData.DamageTypes ConvertToDamageTypes()
        {
            HitData.DamageTypes damages = new HitData.DamageTypes();

            if (blunt != null) damages.m_blunt = (float)blunt;
            if (chop != null) damages.m_chop = (float)chop;
            if (damage != null) damages.m_damage = (float)damage;
            if (fire != null) damages.m_fire = (float)fire;
            if (frost != null)  damages.m_frost = (float)frost;
            if (lightning != null)  damages.m_lightning = (float)lightning;
            if (pickaxe != null)  damages.m_pickaxe = (float)pickaxe;
            if (pierce != null)  damages.m_pierce = (float)pierce;
            if (poison != null)  damages.m_poison = (float)poison;
            if (slash != null)  damages.m_slash = (float)slash;
            if (spirit != null)  damages.m_slash = (float)spirit;

            return damages;
        }

        public void SetFromDamageTypes(HitData.DamageTypes damages)
        {
            blunt = damages.m_blunt;
            chop = damages.m_chop;
            damage = damages.m_damage;
            fire = damages.m_fire;
            frost = damages.m_frost;
            lightning = damages.m_lightning;
            pickaxe = damages.m_pickaxe;
            pierce = damages.m_pierce;
            poison = damages.m_poison;
            slash = damages.m_slash;
            spirit = damages.m_spirit;
        }
    }
}
