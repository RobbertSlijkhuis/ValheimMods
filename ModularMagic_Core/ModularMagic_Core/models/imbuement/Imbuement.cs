using ModularMagic_Core.Components;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Types;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// One slot of an imbuable weapon. Only the rune id and level are persisted, everything else about the
    /// rune (type, value, name, description) is looked up from the rune definition so it can never go stale.
    /// </summary>
    public class Imbuement
    {
        public int level = 0;
        public string runeId = "";
        // Working copy flag of the rune table, this is never persisted
        public bool saved = false;
        public int tier = 1;
        public string weaponType = WeaponType.None;

        public Imbuement() { }

        public Imbuement(int tier, string weaponType)
        {
            this.tier = tier;
            this.weaponType = weaponType ?? WeaponType.None;
        }

        public ImbuementRune rune
        {
            get { return ImbuementHelper.FindRune(runeId); }
        }

        public string type
        {
            get { ImbuementRune r = rune; return r != null ? r.m_type : ImbuementType.None; }
        }

        public string value
        {
            get { ImbuementRune r = rune; return r != null ? r.m_value : ""; }
        }

        public string name
        {
            get { ImbuementRune r = rune; return r != null ? r.GetName(weaponType) : "Empty"; }
        }

        public string description
        {
            get { ImbuementRune r = rune; return r != null ? r.GetDescription(weaponType) : ""; }
        }

        // Name of the rune prefab, empty when the slot is empty
        public string prefab
        {
            get { ImbuementRune r = rune; return r != null ? r.gameObject.name : ""; }
        }

        public bool HasSameRune(Imbuement other)
        {
            return runeId == other.runeId && level == other.level;
        }

        // The level is the quality of the rune item
        public void SetRune(ImbuementRune imbuementRune, int level)
        {
            runeId = imbuementRune.m_id;
            this.level = level;
            saved = false;
        }

        // A copy of the rune and level, without the state of the working copy
        public Imbuement CopyRune()
        {
            Imbuement copy = new Imbuement(tier, weaponType);
            copy.runeId = runeId;
            copy.level = level;
            copy.saved = saved;
            return copy;
        }

        public void Clear()
        {
            runeId = "";
            level = 0;
            saved = false;
        }
    }
}
