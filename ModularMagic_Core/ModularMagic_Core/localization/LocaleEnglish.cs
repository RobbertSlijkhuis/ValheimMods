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
                {$"{LocaleKey.ItemRuneAccuracy}_mmes", "Rune: Accuracy"},
                {$"{LocaleKey.ItemRuneAccuracyDesc}_mmes", "Improve the accuracy of your main attack"},
                {$"{LocaleKey.ItemRuneBleedingEdge}_mmes", "Rune: Bleeding Edge"},
                {$"{LocaleKey.ItemRuneBleedingEdgeDesc}_mmes", "Sharpen the projectiles of your main attack, changing main damage type to Slash!"},
                {$"{LocaleKey.ItemRuneNova}_mmes", "Rune: Nova"},
                {$"{LocaleKey.ItemRuneNovaDesc}_mmes", "Cast an area of effect spell around you!"},
                {$"{LocaleKey.ItemRuneRain}_mmes", "Rune: Giant f*cking Boulder"},
                {$"{LocaleKey.ItemRuneRainDesc}_mmes", "Summon a giant boulder from the sky to crush your enemies!"},
            });
        }
    }
}
