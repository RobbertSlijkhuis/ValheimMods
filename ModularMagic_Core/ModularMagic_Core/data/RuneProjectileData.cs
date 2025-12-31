using ModularMagic_Core.Locale;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneProjectileData
    {
        public RuneProjectileData(List<RuneEntry> list)
        {
            RuneEntryOptions accuracyOptions = new RuneEntryOptions();
            accuracyOptions.allowedWeapons = RuneData.EarthIce;
            accuracyOptions.description = "Apply this rune to your weapon to increase its accuracy! This rune might not work on certain weapons!";
            accuracyOptions.descriptionKey = LocaleKey.ItemRuneAccuracyDesc;
            accuracyOptions.name = "Rune: Accuracy";
            accuracyOptions.nameKey = LocaleKey.ItemRuneAccuracy;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyWood;
            accuracyOptions.recipe = "FineWood:3, LeatherScraps:3";
            accuracyOptions.tier = 0;
            accuracyOptions.type = ImbuementType.ProjectileAccuracy;
            accuracyOptions.value = "0.125";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.tier = 2;
            accuracyOptions.level = 2;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyStone;
            accuracyOptions.recipe = "Stone:3, LeatherScraps:3";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.tier = 3;
            accuracyOptions.level = 3;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyMarble;
            accuracyOptions.recipe = "BlackMarble:3, LeatherScraps:3";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.tier = 4;
            accuracyOptions.level = 4;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyGrausten;
            accuracyOptions.recipe = "Grausten:3, LeatherScraps:3";
            list.Add(new RuneEntry(accuracyOptions));
        }
    }
}
