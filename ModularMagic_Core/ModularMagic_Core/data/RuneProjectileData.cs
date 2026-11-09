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

            RuneEntryOptions attackSpeedOptions = new RuneEntryOptions();
            attackSpeedOptions.description = "Improve the attack speed of your weapon!";
            attackSpeedOptions.id = RuneId.AttackSpeed;
            attackSpeedOptions.name = "Rune: Attack Speed";
            attackSpeedOptions.prefab = ModularMagic_Core.prefabs.RuneAttackSpeed;
            attackSpeedOptions.recipe = $"RoundLog:3, KnifeCopper:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            attackSpeedOptions.recipeUpgrade = attackSpeedOptions.recipe;
            attackSpeedOptions.tiers = new int[] { 0, 2, 3, 4 };
            attackSpeedOptions.type = ImbuementType.AttackSpeed;
            attackSpeedOptions.value = "0.0125";
            list.Add(new RuneEntry(attackSpeedOptions));

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
