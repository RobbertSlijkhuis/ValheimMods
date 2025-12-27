using ModularMagic_Core.Locale;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneData
    {
        public static List<RuneEntry> list = new List<RuneEntry>();

        public static void Init()
        {
            List<string> EarthIce = new List<string>() { WeaponType.MMES, WeaponType.MMIS };
            List<string> EarthFireIce = new List<string>() { WeaponType.MMES, WeaponType.MMFS, WeaponType.MMIS };
            List<string> EarthFireIceLightning = new List<string>() { WeaponType.MMES, WeaponType.MMFS, WeaponType.MMIS, WeaponType.MMLS };

            RuneEntryOptions accuracyOptions = new RuneEntryOptions();
            accuracyOptions.allowedWeapons = EarthIce;
            accuracyOptions.description = "Apply this rune to your weapon to increase its accuracy! This rune might not work on certain weapons!";
            accuracyOptions.descriptionKey = LocaleKey.ItemRuneAccuracyDesc;
            accuracyOptions.name = "Rune: Accuracy";
            accuracyOptions.nameKey = LocaleKey.ItemRuneAccuracy;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyWood;
            accuracyOptions.recipe = "FineWood:3, LeatherScraps:3";
            accuracyOptions.type = ImbuementType.ProjectileAccuracy;
            accuracyOptions.value = "0.125";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.level = 2;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyStone;
            accuracyOptions.recipe = "Stone:3, LeatherScraps:3";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.level = 3;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyMarble;
            accuracyOptions.recipe = "BlackMarble:3, LeatherScraps:3";
            list.Add(new RuneEntry(accuracyOptions));

            accuracyOptions.level = 4;
            accuracyOptions.prefab = ModularMagic_Core.prefabs.RuneAccuracyGrausten;
            accuracyOptions.recipe = "Grausten:3, LeatherScraps:3";
            list.Add(new RuneEntry(accuracyOptions));

            RuneEntryOptions damageSlashOptions = new RuneEntryOptions();
            damageSlashOptions.allowedWeapons = EarthIce;
            damageSlashOptions.description = "Apply this rune to your weapon to change the main damage type! This rune might not work on certain weapons!";
            damageSlashOptions.descriptionKey = LocaleKey.ItemRuneBleedingEdgeDesc;
            damageSlashOptions.name = "Rune: Bleeding Edge";
            damageSlashOptions.nameKey = LocaleKey.ItemRuneBleedingEdge;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashWood;
            damageSlashOptions.recipe = "FineWood:3, SurtlingCore:3";
            damageSlashOptions.type = ImbuementType.DamageType;
            damageSlashOptions.value = "Slash";
            list.Add(new RuneEntry(damageSlashOptions));

            damageSlashOptions.level = 2;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashStone;
            damageSlashOptions.recipe = "Stone:3, SurtlingCore:3";
            list.Add(new RuneEntry(damageSlashOptions));

            damageSlashOptions.level = 3;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashMarble;
            damageSlashOptions.recipe = "BlackMarble:3, SurtlingCore:3";
            list.Add(new RuneEntry(damageSlashOptions));

            damageSlashOptions.level = 4;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashGrausten;
            damageSlashOptions.recipe = "Grausten:3, SurtlingCore:3";
            list.Add(new RuneEntry(damageSlashOptions));

            RuneEntryOptions novaOptions = new RuneEntryOptions();
            novaOptions.allowedWeapons = EarthFireIceLightning;
            novaOptions.description = "Apply this rune to your weapon to add a secondary ability! This ability is different depending on the type of the weapon! ";
            novaOptions.descriptionKey = LocaleKey.ItemRuneNovaDesc;
            novaOptions.name = "Rune: Nova";
            novaOptions.nameKey = LocaleKey.ItemRuneNova;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaStone;
            novaOptions.recipe = "Stone:3, Coal:3";
            novaOptions.type = ImbuementType.SecondaryAttack;
            novaOptions.value = "Nova";
            list.Add(new RuneEntry(novaOptions));

            novaOptions.level = 2;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaMarble;
            novaOptions.recipe = "BlackMarble:3, Coal:3";
            list.Add(new RuneEntry(novaOptions));

            novaOptions.level = 3;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaGrausten;
            novaOptions.recipe = "Grausten:3, Coal:3";
            list.Add(new RuneEntry(novaOptions));

            RuneEntryOptions rainOptions = new RuneEntryOptions();
            rainOptions.allowedWeapons = EarthFireIce;
            rainOptions.description = "Apply this rune to your weapon to add a secondary ability! This ability is different depending on the type of the weapon! ";
            rainOptions.descriptionKey = LocaleKey.ItemRuneRainDesc;
            rainOptions.name = "Rune: Rain";
            rainOptions.nameKey = LocaleKey.ItemRuneRain;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainStone;
            rainOptions.recipe = "Stone:3, Blueberries:3";
            rainOptions.type = ImbuementType.SecondaryAttack;
            rainOptions.value = "Rain";
            list.Add(new RuneEntry(rainOptions));

            rainOptions.level = 2;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainMarble;
            rainOptions.recipe = "BlackMarble:3, Blueberries:3";
            list.Add(new RuneEntry(rainOptions));

            rainOptions.level = 3;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainGrausten;
            rainOptions.recipe = "Grausten:3, Blueberries:3";
            list.Add(new RuneEntry(rainOptions));
        }
    }
}
