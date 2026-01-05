using ModularMagic_Core.Locale;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneMiscData
    {
        public RuneMiscData(List<RuneEntry> list)
        {
            RuneEntryOptions eitrCostOptions = new RuneEntryOptions();
            eitrCostOptions.allowedWeapons = RuneData.EarthFireIceLightning;
            eitrCostOptions.description = "Reduce Eitr cost of your weapon's main attack!";
            eitrCostOptions.descriptionKey = LocaleKey.ItemEitrCostDesc;
            eitrCostOptions.name = "Rune: Eitr cost";
            eitrCostOptions.nameKey = LocaleKey.ItemEitrCost;
            eitrCostOptions.prefab = ModularMagic_Core.prefabs.RuneEitrCostWood;
            eitrCostOptions.recipe = $"RoundLog:3, {ModularMagic_Core.prefabs.EitrCrude.name}:9";
            eitrCostOptions.type = ImbuementType.EitrCost;
            eitrCostOptions.value = "0.25";
            list.Add(new RuneEntry(eitrCostOptions));

            eitrCostOptions.tier = 2;
            eitrCostOptions.level = 2;
            eitrCostOptions.prefab = ModularMagic_Core.prefabs.RuneEitrCostStone;
            eitrCostOptions.recipe = $"Stone:3, {ModularMagic_Core.prefabs.EitrFine.name}:9";
            list.Add(new RuneEntry(eitrCostOptions));

            eitrCostOptions.tier = 3;
            eitrCostOptions.level = 3;
            eitrCostOptions.prefab = ModularMagic_Core.prefabs.RuneEitrCostMarble;
            eitrCostOptions.recipe = "BlackMarble:3, Eitr:9";
            list.Add(new RuneEntry(eitrCostOptions));

            eitrCostOptions.tier = 4;
            eitrCostOptions.level = 4;
            eitrCostOptions.prefab = ModularMagic_Core.prefabs.RuneEitrCostGrausten;
            eitrCostOptions.recipe = "Grausten:3, Eitr:9";
            list.Add(new RuneEntry(eitrCostOptions));
        }
    }
}
