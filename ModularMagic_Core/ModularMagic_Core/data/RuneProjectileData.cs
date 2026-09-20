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
            accuracyOptions.description = "Improve the accuracy of your weapon!";
            accuracyOptions.id = RuneId.ProjectileAccuracy;
            accuracyOptions.name = "Rune: Accuracy";
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileAccuracy;
            accuracyOptions.recipe = $"RoundLog:3, BowFineWood:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            accuracyOptions.recipeUpgrade = accuracyOptions.recipe;
            // Level 1 fits every weapon (tier 0), the higher levels need a higher tier
            accuracyOptions.tiers = new int[] { 0, 2, 3, 4 };
            accuracyOptions.type = ImbuementType.ProjectileAccuracy;
            accuracyOptions.value = "0.125";
            list.Add(new RuneEntry(accuracyOptions));

            RuneEntryOptions burstOptions = new RuneEntryOptions();
            burstOptions.description = "Improve the attack speed of your weapon!";
            burstOptions.id = RuneId.ProjectileBurst;
            burstOptions.name = "Rune: Attack Speed";
            burstOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileBurst;
            burstOptions.recipe = $"RoundLog:3, KnifeCopper:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            burstOptions.recipeUpgrade = burstOptions.recipe;
            burstOptions.tiers = new int[] { 0, 2, 3, 4 };
            burstOptions.type = ImbuementType.ProjectileBurst;
            burstOptions.value = "0.0125";
            list.Add(new RuneEntry(burstOptions));

            RuneEntryOptions speedOptions = new RuneEntryOptions();
            speedOptions.description = "Improve the projectile speed of your weapon!";
            speedOptions.id = RuneId.ProjectileSpeed;
            speedOptions.name = "Rune: Projectile Speed";
            speedOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileSpeed;
            speedOptions.recipe = $"RoundLog:3, ArrowBronze:100, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            speedOptions.recipeUpgrade = speedOptions.recipe;
            speedOptions.tiers = new int[] { 0, 2, 3, 4 };
            speedOptions.type = ImbuementType.ProjectileVelocity;
            speedOptions.value = "1";
            list.Add(new RuneEntry(speedOptions));
        }
    }
}
