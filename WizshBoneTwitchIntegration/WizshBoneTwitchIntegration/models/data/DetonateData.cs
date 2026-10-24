using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class DetonateData : CloneableData
    {
        public string announceMessage;

        public float radius = 20f;

        public string type = DetonateType.Fish;

        public List<string> values = new List<string>() { "Fish" };

        public DamageData damageData = new DamageData() { basedOnMaxHealthAndArmor = true, blunt = 70f, chop = 160f, fire = 30f, pickaxe = 160f };

        public bool damageTerrain = true;

        public DetonateData() { }
    }
}
