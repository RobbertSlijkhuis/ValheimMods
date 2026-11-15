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

            // Swings the weapon with the attack animations of a two handed sword, see StaffDefinition.AddMainAttackAnimation
            RuneEntryOptions greatswordOptions = new RuneEntryOptions();
            greatswordOptions.description = "Swing your weapon like a greatsword!";
            greatswordOptions.id = RuneId.Greatsword;
            greatswordOptions.name = "Rune: Greatsword";
            greatswordOptions.prefab = ModularMagic_Core.prefabs.RuneGreatsword;
            greatswordOptions.recipe = $"RoundLog:3, SwordIron:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            greatswordOptions.recipeUpgrade = greatswordOptions.recipe;
            greatswordOptions.tiers = new int[] { 1, 2, 3, 4 };
            greatswordOptions.type = ImbuementType.MainAttack;
            greatswordOptions.value = "Greatsword";
            RuneData.Add(list, greatswordOptions);
        }
    }
}
