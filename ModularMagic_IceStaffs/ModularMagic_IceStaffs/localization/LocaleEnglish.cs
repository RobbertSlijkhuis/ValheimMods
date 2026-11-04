using ModularMagic_Core.Types;
using System.Collections.Generic;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_IceStaffs.Locale
{
    /// <summary>
    /// The names and descriptions of the runes on the Ice staffs. Core looks them up with the key of the rune and the
    /// weapon type, a rune without a text here shows its default name and description.
    /// </summary>
    internal class LocaleEnglish
    {
        public static void Init()
        {
            Dictionary<string, string> texts = new Dictionary<string, string>();

            AddRune(texts, RuneId.DamageBlunt, "Rune: Damage Blunt", "Change the physical damage of the weapon to Blunt! The frost damage stays");
            AddRune(texts, RuneId.DamageSlash, "Rune: Damage Slash", "Change the physical damage of the weapon to Slash! The frost damage stays");
            AddRune(texts, RuneId.EitrCost, "Rune: Eitr Cost", "Reduce the eitr cost of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileAccuracy, "Rune: Accuracy", "Improve the accuracy of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileBurst, "Rune: Attack Speed", "Improve the attack speed of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileSpeed, "Rune: Projectile Speed", "Improve the projectile speed of the weapon's main attack");
            AddRune(texts, RuneId.Nova, "Rune: Frost Nova", "Release a nova of frost around you!");
            AddRune(texts, RuneId.Cone, "Rune: Frost Cone", "Change your main attack into a cone of frost!");
            AddRune(texts, RuneId.Rain, "Rune: Ice Shards", "Call down a rain of ice shards on your enemies!");

            ModularMagic_IceStaffs.Localization.AddTranslation("English", texts);
        }

        private static void AddRune(Dictionary<string, string> texts, string runeId, string name, string description)
        {
            texts[CoreImbuementHelper.GetNameKey(runeId, WeaponType.MMIS)] = name;
            texts[CoreImbuementHelper.GetDescriptionKey(runeId, WeaponType.MMIS)] = description;
        }
    }
}
