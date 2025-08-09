using System.Collections.Generic;

namespace ModularMagic_Core.Models
{
    internal class Imbuement
    {
        public bool enabled;
        public string description;
        public bool isImbued;
        public int materialRequired;
        public string name;
        public ImbuementPath path;
        public List<Imbuement> requiredFor = new List<Imbuement>();
        public List<Imbuement> requires = new List<Imbuement>();
        public int skillRequired;
        public string type;
        public string value;

        public Imbuement(string name, string type, string description, string value, int skillRequired, int materialRequired, ImbuementPath path, bool enabled = false)
        {
            this.enabled = enabled;
            this.description = description;
            this.materialRequired = materialRequired;
            this.name = name;
            this.path = path;
            this.skillRequired = skillRequired;
            this.type = type;
            this.value = value;
        }
    }
}
