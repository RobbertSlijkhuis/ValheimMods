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
            eitrCostOptions.id = "EitrCost";
            eitrCostOptions.name = "Rune: Eitr cost";
            eitrCostOptions.nameKey = LocaleKey.ItemEitrCost;
            eitrCostOptions.prefab = ModularMagic_Core.prefabs.RuneEitrCost;
            eitrCostOptions.recipe = $"RoundLog:3, {ModularMagic_Core.prefabs.EitrCrude.name}:9";
            eitrCostOptions.recipeUpgrade = eitrCostOptions.recipe;
            eitrCostOptions.tiers = new int[] { 1, 2, 3, 4 };
            eitrCostOptions.type = ImbuementType.EitrCost;
            eitrCostOptions.value = "0.25";
            list.Add(new RuneEntry(eitrCostOptions));
        }
    }
}
