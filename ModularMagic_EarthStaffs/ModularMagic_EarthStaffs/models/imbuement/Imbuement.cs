using ModularMagic_EarthStaffs.Types;

namespace ModularMagic_EarthStaffs.Models
{
    internal class Imbuement
    {
        public string description = "";
        public int level = 1;
        public string name = "Empty";
        public string prefab = "";
        public bool saved = false;
        public int tier = 1;
        public string type = ImbuementType.None;
        public string value = "";
        public string weaponType = WeaponType.MMES;

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
