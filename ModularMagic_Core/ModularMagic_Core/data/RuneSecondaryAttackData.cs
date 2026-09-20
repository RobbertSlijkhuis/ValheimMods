using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneSecondaryAttackData
    {
        public RuneSecondaryAttackData(List<RuneEntry> list)
        {
            // The secondary attack runes have three levels, they start at slot tier 2
            RuneEntryOptions creaturesOptions = new RuneEntryOptions();
            creaturesOptions.description = "Add a secondary ability to your weapon! This ability is different depending on the type of the weapon!";
            creaturesOptions.id = RuneId.Creatures;
            creaturesOptions.name = "Rune: Summon";
            creaturesOptions.prefab = ModularMagic_Core.prefabs.RuneCreatures;
            creaturesOptions.recipe = $"Stone:3, TrophySGolem:1, {ModularMagic_Core.prefabs.EitrFine.name}:6";
            creaturesOptions.recipeUpgrade = creaturesOptions.recipe;
            creaturesOptions.tiers = new int[] { 2, 3, 4 };
            creaturesOptions.type = ImbuementType.SecondaryAttack;
            creaturesOptions.value = "Summon";
            list.Add(new RuneEntry(creaturesOptions));

            RuneEntryOptions novaOptions = new RuneEntryOptions();
            novaOptions.description = "Add a secondary ability to your weapon! This ability is different depending on the type of the weapon!";
            novaOptions.id = RuneId.Nova;
            novaOptions.name = "Rune: Nova";
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNova;
            novaOptions.recipe = $"Stone:3, AtgeirIron:1, {ModularMagic_Core.prefabs.EitrFine.name}:6";
            novaOptions.recipeUpgrade = novaOptions.recipe;
            novaOptions.tiers = new int[] { 2, 3, 4 };
            novaOptions.type = ImbuementType.SecondaryAttack;
            novaOptions.value = "Nova";
            list.Add(new RuneEntry(novaOptions));

            RuneEntryOptions rainOptions = new RuneEntryOptions();
            rainOptions.description = "Add a secondary ability to your weapon! This ability is different depending on the type of the weapon!";
            rainOptions.id = RuneId.Rain;
            rainOptions.name = "Rune: Rain";
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRain;
            rainOptions.recipe = $"Stone:3, ArrowObsidian:200, {ModularMagic_Core.prefabs.EitrFine.name}:6";
            rainOptions.recipeUpgrade = rainOptions.recipe;
            rainOptions.tiers = new int[] { 2, 3, 4 };
            rainOptions.type = ImbuementType.SecondaryAttack;
            rainOptions.value = "Rain";
            list.Add(new RuneEntry(rainOptions));
        }
    }
}
