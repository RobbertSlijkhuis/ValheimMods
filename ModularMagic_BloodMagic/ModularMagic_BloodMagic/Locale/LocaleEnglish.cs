using ModularMagic_Core.Types;
using System.Collections.Generic;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_BloodMagic.Locale
{
    /// <summary>
    /// The names and descriptions of the runes on the scythes. Core looks them up with the key of the rune and the
    /// weapon type, a rune without a text here shows its default name and description.
    /// </summary>
    internal class LocaleEnglish
    {
        public static void Init()
        {
            Dictionary<string, string> texts = new Dictionary<string, string>();

            AddRune(texts, RuneId.EitrCost, "Rune: Eitr Cost", "Reduce the eitr of every hit that charges the scythe");
            AddRune(texts, RuneId.AttackSpeed, "Rune: Attack Speed", "Swing the scythe faster");

            ModularMagic_BloodMagic.Localization.AddTranslation("English", texts);
        }

        private static void AddRune(Dictionary<string, string> texts, string runeId, string name, string description)
        {
            texts[CoreImbuementHelper.GetNameKey(runeId, WeaponType.MMBM)] = name;
            texts[CoreImbuementHelper.GetDescriptionKey(runeId, WeaponType.MMBM)] = description;
        }
    }
}
