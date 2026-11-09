using ModularMagic_Core.Types;
using System.Collections.Generic;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_FireStaffs.Locale
{
    /// <summary>
    /// The names and descriptions of the runes on the Fire staffs. Core looks them up with the key of the rune and the
    /// weapon type, a rune without a text here shows its default name and description.
    /// </summary>
    internal class LocaleEnglish
    {
        public static void Init()
        {
            Dictionary<string, string> texts = new Dictionary<string, string>();

            AddRune(texts, RuneId.EitrCost, "Rune: Eitr Cost", "Reduce the eitr cost of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileAccuracy, "Rune: Accuracy", "Improve the accuracy of the weapon's main attack");
            AddRune(texts, RuneId.AttackSpeed, "Rune: Attack Speed", "Improve the attack speed of the weapon's main attack");
            AddRune(texts, RuneId.Cone, "Rune: Fire Cone", "Change your main attack into a cone of fire!");
            AddRune(texts, RuneId.ProjectileSpeed, "Rune: Projectile Speed", "Improve the projectile speed of the weapon's main attack");
            AddRune(texts, RuneId.Nova, "Rune: Fire Nova", "Release a nova of fire around you!");
            AddRune(texts, RuneId.Rain, "Rune: Meteors", "Call down a rain of meteors on your enemies!");

            ModularMagic_FireStaffs.Localization.AddTranslation("English", texts);
        }

        private static void AddRune(Dictionary<string, string> texts, string runeId, string name, string description)
        {
            texts[CoreImbuementHelper.GetNameKey(runeId, WeaponType.MMFS)] = name;
            texts[CoreImbuementHelper.GetDescriptionKey(runeId, WeaponType.MMFS)] = description;
        }
    }
}
