using PlayFab.GroupsModels;

namespace WizshBoneTwitchIntegration.Models
{
    internal class DamageData : CloneableData
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

        public bool basedOnMaxHealthAndArmor = false;
        public float armorPercentage = 0.8f;
        public float maxHealthPercentage = 0.45f;

        public DamageData() { }
    }
}
