namespace ModularMagic_EarthStaffs.Models
{
    internal class Imbuement
    {

        public string category;
        public int column;
        public string description;
        public bool enabled;
        public int level;
        public int maxLevel;
        public string name;
        public string type;
        public string value;

        public Imbuement(string name, string description, string type, string category, int column, string value, int level, int maxLevel, bool enabled = false)
        {
            this.category = category;
            this.column = column;
            this.description = description;
            this.enabled = enabled;
            this.level = level;
            this.maxLevel = maxLevel;
            this.name = name;
            this.type = type;
            this.value = value;
        }
    }
}
