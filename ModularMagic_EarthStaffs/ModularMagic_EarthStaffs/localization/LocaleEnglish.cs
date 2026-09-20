using ModularMagic_Core.Types;
using System.Collections.Generic;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_EarthStaffs.Locale
{
    /// <summary>
    /// The names and descriptions of the runes on the Earth staffs. Core looks them up with the key of the rune and the
    /// weapon type, a rune without a text here shows its default name and description.
    /// </summary>
    internal class LocaleEnglish
    {
        public static void Init()
        {
            Dictionary<string, string> texts = new Dictionary<string, string>();

            AddRune(texts, RuneId.DamagePierce, "Rune: Damage Pierce", "Change physical damage on the weapon to Pierce!");
            AddRune(texts, RuneId.DamageSlash, "Rune: Damage Slash", "Change physical damage on the weapon to Slash!");
            AddRune(texts, RuneId.EitrCost, "Rune: Eitr Cost", "Reduce the eitr cost of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileAccuracy, "Rune: Accuracy", "Improve the accuracy of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileBurst, "Rune: Attack Speed", "Improve the attack speed of the weapon's main attack");
            AddRune(texts, RuneId.ProjectileSpeed, "Rune: Projectile Speed", "Improve the projectile speed of the weapon's main attack");
            AddRune(texts, RuneId.Nova, "Rune: Nova", "Cast an area of effect spell around you!");
            AddRune(texts, RuneId.Rain, "Rune: Giant f*cking Boulder", "Summon a giant boulder from the sky to crush your enemies!");

            ModularMagic_EarthStaffs.Localization.AddTranslation("English", texts);
        }

        private static void AddRune(Dictionary<string, string> texts, string runeId, string name, string description)
        {
            texts[CoreImbuementHelper.GetNameKey(runeId, WeaponType.MMES)] = name;
            texts[CoreImbuementHelper.GetDescriptionKey(runeId, WeaponType.MMES)] = description;
        }
    }
}
