using ModularMagic_Core.Locale;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneMainAttackData
    {
        public RuneMainAttackData(List<RuneEntry> list)
        {
            RuneEntryOptions coneOptions = new RuneEntryOptions();
            coneOptions.allowedWeapons = RuneData.EarthFireIceLightning;
            coneOptions.description = "Change the main ability of your weapon! This ability is different depending on the type of the weapon!";
            coneOptions.descriptionKey = LocaleKey.NoneDesc;
            coneOptions.name = "Rune: Cone";
            coneOptions.nameKey = LocaleKey.None;
            coneOptions.prefab = ModularMagic_Core.prefabs.RuneConeWood;
            coneOptions.recipe = "Wood:3, Coal:3";
            coneOptions.tier = 1;
            coneOptions.type = ImbuementType.MainAttack;
            coneOptions.value = "Cone";
            list.Add(new RuneEntry(coneOptions));

            coneOptions.tier = 2;
            coneOptions.level = 2;
            coneOptions.prefab = ModularMagic_Core.prefabs.RuneConeStone;
            coneOptions.recipe = "Stone:3, Coal:3";
            list.Add(new RuneEntry(coneOptions));

            coneOptions.tier = 3;
            coneOptions.level = 3;
            coneOptions.prefab = ModularMagic_Core.prefabs.RuneConeMarble;
            coneOptions.recipe = "BlackMarble:3, Coal:3";
            list.Add(new RuneEntry(coneOptions));

            coneOptions.tier = 4;
            coneOptions.level = 4;
            coneOptions.prefab = ModularMagic_Core.prefabs.RuneConeGrausten;
            coneOptions.recipe = "Grausten:3, Coal:3";
            list.Add(new RuneEntry(coneOptions));
        }
    }
}
