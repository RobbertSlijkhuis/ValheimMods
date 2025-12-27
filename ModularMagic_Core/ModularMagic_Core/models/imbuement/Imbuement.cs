using ModularMagic_Core.Types;

namespace ModularMagic_Core.Models
{
    internal class Imbuement
    {
        public string description = "";
        public int level = 1;
        public string name = "Empty";
        public string prefab = "";
        public bool saved = false;
        public int tier;
        public string type = ImbuementType.None;
        public string value = "";
        public string weaponType = WeaponType.None;

        public Imbuement() { }

        public Imbuement(string type, string prefab, string name, string description, string value, int tier, int level, bool saved, string weaponType)
        {
            this.description = description;
            this.level = level;
            this.name = name;
            this.prefab = prefab;
            this.saved = saved;
            this.tier = tier;
            this.type = type;
            this.value = value;
            this.weaponType = weaponType;
        }
    }
}
