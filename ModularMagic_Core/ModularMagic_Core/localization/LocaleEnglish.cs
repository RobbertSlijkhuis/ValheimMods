using Jotunn.Managers;
using ModularMagic_Core.Locale;
using System.Collections.Generic;

namespace ModularMagic_Core.localization
{
    internal class LocaleEnglish
    {
        public static void Init()
        {
            Jotunn.Logger.LogWarning("Adding English locale...");
            ModularMagic_Core.Localization = LocalizationManager.Instance.GetLocalization();
            ModularMagic_Core.Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {$"{LocaleKey.ItemDamageTypeBlunt}_mmes", "Rune: Damage Blunt"},
                {$"{LocaleKey.ItemDamageTypeBluntDesc}_mmes", "Change physical damage on the weapon to Blunt!"},
                {$"{LocaleKey.ItemDamageTypePierce}_mmes", "Rune: Damage Pierce"},
                {$"{LocaleKey.ItemDamageTypePierceDesc}_mmes", "Change physical damage on the weapon to Pierce!"},
                {$"{LocaleKey.ItemDamageTypeSlash}_mmes", "Rune: Damage Slash"},
                {$"{LocaleKey.ItemDamageTypeSlashDesc}_mmes", "Change physical damage on the weapon to Slash!"},

                {$"{LocaleKey.ItemEitrCost}_mmes", "Rune: Eitr Cost"},
                {$"{LocaleKey.ItemEitrCostDesc}_mmes", "Reduce the eitr cost of the weapon's main attack"},

                {$"{LocaleKey.ItemRuneProjectileAccuracy}_mmes", "Rune: Accuracy"},
                {$"{LocaleKey.ItemRuneProjectileAccuracyDesc}_mmes", "Improve the accuracy of the weapon's main attack"},
                {$"{LocaleKey.ItemRuneProjectileBurst}_mmes", "Rune: Attack Speed"},
                {$"{LocaleKey.ItemRuneProjectileBurstDesc}_mmes", "Improve the attack speed of the weapon's main attack"},
                {$"{LocaleKey.ItemRuneProjectileSpeed}_mmes", "Rune: Projectile Speed"},
                {$"{LocaleKey.ItemRuneProjectileSpeedDesc}_mmes", "Improve the projectile speed of the weapon's main attack"},

                {$"{LocaleKey.ItemRuneNova}_mmes", "Rune: Nova"},
                {$"{LocaleKey.ItemRuneNovaDesc}_mmes", "Cast an area of effect spell around you!"},
                {$"{LocaleKey.ItemRuneRain}_mmes", "Rune: Giant f*cking Boulder"},
                {$"{LocaleKey.ItemRuneRainDesc}_mmes", "Summon a giant boulder from the sky to crush your enemies!"},

                {$"{LocaleKey.None}_mmes", "Rune: None"},
                {$"{LocaleKey.NoneDesc}_mmes", "Placholder description"},
            });
        }
    }
}
