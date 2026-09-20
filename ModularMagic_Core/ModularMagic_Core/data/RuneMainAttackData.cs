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
            coneOptions.description = "Change the main ability of your weapon! This ability is different depending on the type of the weapon!";
            coneOptions.id = RuneId.Cone;
            coneOptions.name = "Rune: Cone";
            coneOptions.prefab = ModularMagic_Core.prefabs.RuneCone;
            coneOptions.recipe = $"RoundLog:3, TrophyGreydwarfShaman:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            coneOptions.recipeUpgrade = coneOptions.recipe;
            coneOptions.tiers = new int[] { 1, 2, 3, 4 };
            coneOptions.type = ImbuementType.MainAttack;
            coneOptions.value = "Cone";
            list.Add(new RuneEntry(coneOptions));
        }
    }
}
