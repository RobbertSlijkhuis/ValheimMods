using ModularMagic_Core.Types;
using System.Collections.Generic;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_LightningStaffs.Locale
{
    /// <summary>
    /// The names and descriptions of the runes on the Lightning staffs. Core looks them up with the key of the rune and the
    /// weapon type, a rune without a text here shows its default name and description.
    /// </summary>
    internal class LocaleEnglish
    {
        public static void Init()
        {
            Dictionary<string, string> texts = new Dictionary<string, string>();

            AddRune(texts, RuneId.EitrCost, "Rune: Eitr Cost", "Reduce the eitr cost of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileAccuracy, "Rune: Accuracy", "Improve the accuracy of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileBurst, "Rune: Attack Speed", "Improve the attack speed of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileSpeed,"Rune: Projectile Speed", "Improve the projectile speed of the weapon's main attack");
            AddRune(texts, RuneId.Nova, "Rune: Lightning Nova", "Release a nova of lightning around you!");
            AddRune(texts, RuneId.Cone, "Rune: Lightning Cone", "Change your main attack into a cone of lightning!");
            AddRune(texts, RuneId.Rain,"Rune: Lightning Bolts", "Call down a rain of lightning bolts on your enemies!");

            ModularMagic_LightningStaffs.Localization.AddTranslation("English", texts);
        }

        private static void AddRune(Dictionary<string, string> texts, string runeId, string name, string description)
        {
            texts[CoreImbuementHelper.GetNameKey(runeId, WeaponType.MMLS)] = name;
            texts[CoreImbuementHelper.GetDescriptionKey(runeId, WeaponType.MMLS)] = description;
        }
    }
}
