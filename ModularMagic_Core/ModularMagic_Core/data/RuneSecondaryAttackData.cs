using ModularMagic_Core.Locale;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneSecondaryAttackData
    {
        public RuneSecondaryAttackData(List<RuneEntry> list)
        {
            RuneEntryOptions creaturesOptions = new RuneEntryOptions();
            creaturesOptions.allowedWeapons = RuneData.EarthFireIceLightning;
            creaturesOptions.description = "Add a secondary ability to your weapon! This ability is different depending on the type of the weapon!";
            creaturesOptions.descriptionKey = LocaleKey.NoneDesc;
            creaturesOptions.id = "Creatures";
            creaturesOptions.name = "Rune: Summon";
            creaturesOptions.nameKey = LocaleKey.None;
            creaturesOptions.prefab = ModularMagic_Core.prefabs.RuneCreaturesStone;
            creaturesOptions.recipe = $"Stone:3, TrophySGolem:1, {ModularMagic_Core.prefabs.EitrFine.name}:6";
            creaturesOptions.tier = 2;
            creaturesOptions.type = ImbuementType.SecondaryAttack;
            creaturesOptions.value = "Summon";
            list.Add(new RuneEntry(creaturesOptions));

            creaturesOptions.tier = 3;
            creaturesOptions.level = 2;
            creaturesOptions.prefab = ModularMagic_Core.prefabs.RuneCreaturesMarble;
            creaturesOptions.recipe = "BlackMarble:3, TrophySeekerBrute:1, Eitr:6";
            list.Add(new RuneEntry(creaturesOptions));

            creaturesOptions.tier = 4;
            creaturesOptions.level = 3;
            creaturesOptions.prefab = ModularMagic_Core.prefabs.RuneCreaturesGrausten;
            creaturesOptions.recipe = "Grausten:3, TrophyFallenValkyrie:1, Eitr:6";
            list.Add(new RuneEntry(creaturesOptions));

            RuneEntryOptions novaOptions = new RuneEntryOptions();
            novaOptions.allowedWeapons = RuneData.EarthFireIceLightning;
            novaOptions.description = "Add a secondary ability to your weapon! This ability is different depending on the type of the weapon!";
            novaOptions.descriptionKey = LocaleKey.ItemRuneNovaDesc;
            novaOptions.id = "Nova";
            novaOptions.name = "Rune: Nova";
            novaOptions.nameKey = LocaleKey.ItemRuneNova;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaStone;
            novaOptions.recipe = $"Stone:3, AtgeirIron:1, {ModularMagic_Core.prefabs.EitrFine.name}:6";
            novaOptions.tier = 2;
            novaOptions.type = ImbuementType.SecondaryAttack;
            novaOptions.value = "Nova";
            list.Add(new RuneEntry(novaOptions));

            novaOptions.tier = 3;
            novaOptions.level = 2;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaMarble;
            novaOptions.recipe = "BlackMarble:3, AtgeirBlackmetal:1, Eitr:6";
            list.Add(new RuneEntry(novaOptions));

            novaOptions.tier = 4;
            novaOptions.level = 3;
            novaOptions.prefab = ModularMagic_Core.prefabs.RuneNovaGrausten;
            novaOptions.recipe = "Grausten:3, AtgeirHimminAfl:1, Eitr:6";
            list.Add(new RuneEntry(novaOptions));

            RuneEntryOptions rainOptions = new RuneEntryOptions();
            rainOptions.allowedWeapons = RuneData.EarthFireIce;
            rainOptions.description = "Add a secondary ability to your weapon! This ability is different depending on the type of the weapon!";
            rainOptions.descriptionKey = LocaleKey.ItemRuneRainDesc;
            rainOptions.id = "Rain";
            rainOptions.name = "Rune: Rain";
            rainOptions.nameKey = LocaleKey.ItemRuneRain;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainStone;
            rainOptions.recipe = $"Stone:3, ArrowObsidian:200, {ModularMagic_Core.prefabs.EitrFine.name}:6";
            rainOptions.tier = 2;
            rainOptions.type = ImbuementType.SecondaryAttack;
            rainOptions.value = "Rain";
            list.Add(new RuneEntry(rainOptions));

            rainOptions.tier = 3;
            rainOptions.level = 2;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainMarble;
            rainOptions.recipe = "BlackMarble:3, ArrowCarapace:200, Eitr:6";
            list.Add(new RuneEntry(rainOptions));

            rainOptions.tier = 4;
            rainOptions.level = 3;
            rainOptions.prefab = ModularMagic_Core.prefabs.RuneRainGrausten;
            rainOptions.recipe = "Grausten:3, ArrowCharred:200, Eitr:6";
            list.Add(new RuneEntry(rainOptions));
        }
    }
}
