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
            eitrCostOptions.description = "Reduce Eitr cost of your weapon's main attack!";
            eitrCostOptions.id = RuneId.EitrCost;
            eitrCostOptions.name = "Rune: Eitr cost";
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
