using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    /// <summary>
    /// The runes whose effect is done by the weapon mod itself: Core only stores them, the mod reads their level (ImbuementHelper.Read)
    /// and the value per level from its ImbuementConfig.
    /// </summary>
    internal class RuneWeaponData
    {
        public RuneWeaponData(List<RuneEntry> list)
        {
            RuneEntryOptions rangeOptions = new RuneEntryOptions();
            rangeOptions.description = "Increase the reach of your weapon! When the reach is larger depends on the type of the weapon!";
            rangeOptions.id = RuneId.Range;
            rangeOptions.name = "Rune: Range";
            rangeOptions.prefab = ModularMagic_Core.prefabs.RuneRange;
            rangeOptions.recipe = $"RoundLog:3, AtgeirBronze:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            rangeOptions.recipeUpgrade = rangeOptions.recipe;
            rangeOptions.tiers = new int[] { 1, 2, 3, 4 };
            rangeOptions.type = ImbuementType.AttackRange;
            rangeOptions.value = "1";
            RuneData.Add(list, rangeOptions);

            RuneEntryOptions legionOptions = new RuneEntryOptions();
            legionOptions.description = "Summon more creatures at the same time!";
            legionOptions.id = RuneId.Legion;
            legionOptions.name = "Rune: Legion";
            legionOptions.prefab = ModularMagic_Core.prefabs.RuneLegion;
            legionOptions.recipe = $"RoundLog:3, TrophySkeleton:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            legionOptions.recipeUpgrade = legionOptions.recipe;
            legionOptions.tiers = new int[] { 1, 2, 3, 4 };
            legionOptions.type = ImbuementType.SummonLimit;
            legionOptions.value = "1";
            RuneData.Add(list, legionOptions);

            // The effect depends on the staff mod (the Ice staffs freeze the water their frost shots hit), any level turns it on
            RuneEntryOptions utilityOptions = new RuneEntryOptions();
            utilityOptions.description = "A utility for your staff! What it does depends on the type of the staff!";
            utilityOptions.id = RuneId.Utility;
            utilityOptions.name = "Rune: Utility";
            utilityOptions.prefab = ModularMagic_Core.prefabs.RuneUtility;
            utilityOptions.recipe = $"RoundLog:3, {ModularMagic_Core.prefabs.EitrCrude.name}:6";
            utilityOptions.recipeUpgrade = utilityOptions.recipe;
            utilityOptions.tiers = new int[] { 1, 2, 3, 4 };
            utilityOptions.type = ImbuementType.Utility;
            utilityOptions.value = "1";
            RuneData.Add(list, utilityOptions);
        }
    }
}
