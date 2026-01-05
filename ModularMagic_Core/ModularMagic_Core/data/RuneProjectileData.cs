using ModularMagic_Core.Locale;
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
            accuracyOptions.allowedWeapons = RuneData.EarthIce;
            accuracyOptions.description = "Improve the accuracy of your weapon!";
            accuracyOptions.descriptionKey = LocaleKey.ItemRuneProjectileAccuracyDesc;
            accuracyOptions.name = "Rune: Accuracy";
            accuracyOptions.nameKey = LocaleKey.ItemRuneProjectileAccuracy;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileAccuracyWood;
            accuracyOptions.recipe = $"RoundLog:3, BowFineWood:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            accuracyOptions.tier = 0;
            accuracyOptions.type = ImbuementType.ProjectileAccuracy;
            accuracyOptions.value = "0.125";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.tier = 2;
            accuracyOptions.level = 2;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileAccuracyStone;
            accuracyOptions.recipe = $"Stone:3, BowDraugrFang:1, {ModularMagic_Core.prefabs.EitrFine.name}:3";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.tier = 3;
            accuracyOptions.level = 3;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileAccuracyMarble;
            accuracyOptions.recipe = "BlackMarble:3, BowSpineSnap:1, Eitr:3";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.tier = 4;
            accuracyOptions.level = 4;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileAccuracyGrausten;
            accuracyOptions.recipe = "Grausten:3, BowAshlands:1, Eitr:3";
            list.Add(new RuneEntry(accuracyOptions));

            RuneEntryOptions burstOptions = new RuneEntryOptions();
            burstOptions.allowedWeapons = RuneData.EarthIce;
            burstOptions.description = "Improve the attack speed of your weapon!";
            burstOptions.descriptionKey = LocaleKey.ItemRuneProjectileBurstDesc;
            burstOptions.name = "Rune: Attack Speed";
            burstOptions.nameKey = LocaleKey.ItemRuneProjectileBurst;
            burstOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileBurstWood;
            burstOptions.recipe = $"RoundLog:3, KnifeCopper:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            burstOptions.tier = 0;
            burstOptions.type = ImbuementType.ProjectileBurst;
            burstOptions.value = "0.0125";
            list.Add(new RuneEntry(burstOptions));

            burstOptions.tier = 2;
            burstOptions.level = 2;
            burstOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileBurstStone;
            burstOptions.recipe = $"Stone:3, KnifeSilver:1, {ModularMagic_Core.prefabs.EitrFine.name}:3";
            list.Add(new RuneEntry(burstOptions));

            burstOptions.tier = 3;
            burstOptions.level = 3;
            burstOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileBurstMarble;
            burstOptions.recipe = "BlackMarble:3, KnifeBlackMetal:1, Eitr:3";
            list.Add(new RuneEntry(burstOptions));

            burstOptions.tier = 4;
            burstOptions.level = 4;
            burstOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileBurstGrausten;
            burstOptions.recipe = "Grausten:3, KnifeSkollAndHati:1, Eitr:3";
            list.Add(new RuneEntry(burstOptions));

            RuneEntryOptions speedOptions = new RuneEntryOptions();
            speedOptions.allowedWeapons = RuneData.EarthIce;
            speedOptions.description = "Improve the projectile speed of your weapon!";
            speedOptions.descriptionKey = LocaleKey.ItemRuneProjectileSpeedDesc;
            speedOptions.name = "Rune: Projectile Speed";
            speedOptions.nameKey = LocaleKey.ItemRuneProjectileSpeed;
            speedOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileSpeedWood;
            speedOptions.recipe = $"RoundLog:3, ArrowBronze:100, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            speedOptions.tier = 0;
            speedOptions.type = ImbuementType.ProjectileVelocity;
            speedOptions.value = "1";
            list.Add(new RuneEntry(speedOptions));

            speedOptions.tier = 2;
            speedOptions.level = 2;
            speedOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileSpeedStone;
            speedOptions.recipe = $"Stone:3, ArrowObsidian:100, {ModularMagic_Core.prefabs.EitrFine.name}:3";
            list.Add(new RuneEntry(speedOptions));

            speedOptions.tier = 3;
            speedOptions.level = 3;
            speedOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileSpeedMarble;
            speedOptions.recipe = "BlackMarble:3, ArrowCarapace:100, Eitr:3";
            list.Add(new RuneEntry(speedOptions));

            speedOptions.tier = 4;
            speedOptions.level = 4;
            speedOptions.prefab = ModularMagic_Core.prefabs.RuneProjectileSpeedGrausten;
            speedOptions.recipe = "Grausten:3, ArrowCharred:100, Eitr:3";
            list.Add(new RuneEntry(speedOptions));
        }
    }
}
