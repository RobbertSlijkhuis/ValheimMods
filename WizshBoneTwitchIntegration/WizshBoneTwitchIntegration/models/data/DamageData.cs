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
            if (damage != null) damages.m_chop = (float)chop;
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
    }
}
