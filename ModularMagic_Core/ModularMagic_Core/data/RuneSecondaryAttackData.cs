using ModularMagic_Core.Locale;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneSecondaryAttackData
    {
        public RuneSecondaryAttackData(List<RuneEntry> list)
        {
            RuneEntryOptions novaOptions = new RuneEntryOptions();
            novaOptions.allowedWeapons = RuneData.EarthFireIceLightning;
            novaOptions.description = "Apply this rune to your weapon to add a secondary ability! This ability is different depending on the type of the weapon! ";
            novaOptions.descriptionKey = LocaleKey.ItemRuneNovaDesc;
            novaOptions.name = "Rune: Nova";
            novaOptions.nameKey = LocaleKey.ItemRuneNova;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaStone;
            novaOptions.recipe = "Stone:3, Coal:3";
            novaOptions.tier = 2;
            novaOptions.type = ImbuementType.SecondaryAttack;
            novaOptions.value = "Nova";
            list.Add(new RuneEntry(novaOptions));

            novaOptions.tier = 3;
            novaOptions.level = 2;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaMarble;
            novaOptions.recipe = "BlackMarble:3, Coal:3";
            list.Add(new RuneEntry(novaOptions));

            novaOptions.tier = 4;
            novaOptions.level = 3;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaGrausten;
            novaOptions.recipe = "Grausten:3, Coal:3";
            list.Add(new RuneEntry(novaOptions));

            RuneEntryOptions rainOptions = new RuneEntryOptions();
            rainOptions.allowedWeapons = RuneData.EarthFireIce;
            rainOptions.description = "Apply this rune to your weapon to add a secondary ability! This ability is different depending on the type of the weapon! ";
            rainOptions.descriptionKey = LocaleKey.ItemRuneRainDesc;
            rainOptions.name = "Rune: Rain";
            rainOptions.nameKey = LocaleKey.ItemRuneRain;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainStone;
            rainOptions.recipe = "Stone:3, Blueberries:3";
            rainOptions.tier = 2;
            rainOptions.type = ImbuementType.SecondaryAttack;
            rainOptions.value = "Rain";
            list.Add(new RuneEntry(rainOptions));

            rainOptions.tier = 3;
            rainOptions.level = 2;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainMarble;
            rainOptions.recipe = "BlackMarble:3, Blueberries:3";
            list.Add(new RuneEntry(rainOptions));

            rainOptions.tier = 4;
            rainOptions.level = 3;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainGrausten;
            rainOptions.recipe = "Grausten:3, Blueberries:3";
            list.Add(new RuneEntry(rainOptions));
        }
    }
}
