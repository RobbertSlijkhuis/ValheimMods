using ModularMagic_Core.Locale;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneDamageTypeData
    {
        public RuneDamageTypeData(List<RuneEntry> list)
        {
            RuneEntryOptions damageBluntOptions = new RuneEntryOptions();
            damageBluntOptions.allowedWeapons = RuneData.Ice;
            damageBluntOptions.description = "Change the physical damage to Blunt";
            damageBluntOptions.descriptionKey = LocaleKey.ItemDamageTypeBluntDesc;
            damageBluntOptions.name = "Rune: Damage Blunt";
            damageBluntOptions.nameKey = LocaleKey.ItemDamageTypeBlunt;
            damageBluntOptions.prefab = ModularMagic_Core.prefabs.RuneDamageBluntWood;
            damageBluntOptions.recipe = $"RoundLog:3, MaceBronze:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            damageBluntOptions.type = ImbuementType.DamageType;
            damageBluntOptions.value = "Blunt";
            list.Add(new RuneEntry(damageBluntOptions));

            damageBluntOptions.tier = 2;
            damageBluntOptions.level = 2;
            damageBluntOptions.prefab = ModularMagic_Core.prefabs.RuneDamageBluntStone;
            damageBluntOptions.recipe = $"Stone:3, MaceSilver:1, {ModularMagic_Core.prefabs.EitrFine.name}:3";
            list.Add(new RuneEntry(damageBluntOptions));

            damageBluntOptions.tier = 3;
            damageBluntOptions.level = 3;
            damageBluntOptions.prefab = ModularMagic_Core.prefabs.RuneDamageBluntMarble;
            damageBluntOptions.recipe = "BlackMarble:3, MaceNeedle:1, Eitr:3";
            list.Add(new RuneEntry(damageBluntOptions));

            damageBluntOptions.tier = 4;
            damageBluntOptions.level = 4;
            damageBluntOptions.prefab = ModularMagic_Core.prefabs.RuneDamageBluntGrausten;
            damageBluntOptions.recipe = "Grausten:3, MaceEldner:1, Eitr:3";
            list.Add(new RuneEntry(damageBluntOptions));

            RuneEntryOptions damagePierceOptions = new RuneEntryOptions();
            damagePierceOptions.allowedWeapons = RuneData.Earth;
            damagePierceOptions.description = "Change the physical damage to Pierce";
            damagePierceOptions.descriptionKey = LocaleKey.ItemDamageTypePierceDesc;
            damagePierceOptions.name = "Rune: Damage Pierce";
            damagePierceOptions.nameKey = LocaleKey.ItemDamageTypePierce;
            damagePierceOptions.prefab = ModularMagic_Core.prefabs.RuneDamagePierceWood;
            damagePierceOptions.recipe = $"RoundLog:3, SpearBronze:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            damagePierceOptions.type = ImbuementType.DamageType;
            damagePierceOptions.value = "Pierce";
            list.Add(new RuneEntry(damagePierceOptions));

            damagePierceOptions.tier = 2;
            damagePierceOptions.level = 2;
            damagePierceOptions.prefab = ModularMagic_Core.prefabs.RuneDamagePierceStone;
            damagePierceOptions.recipe = $"Stone:3, SpearWolfFang:1, {ModularMagic_Core.prefabs.EitrFine.name}:3";
            list.Add(new RuneEntry(damagePierceOptions));

            damagePierceOptions.tier = 3;
            damagePierceOptions.level = 3;
            damagePierceOptions.prefab = ModularMagic_Core.prefabs.RuneDamagePierceMarble;
            damagePierceOptions.recipe = "BlackMarble:3, SpearCarapace:1, Eitr:3";
            list.Add(new RuneEntry(damagePierceOptions));

            damagePierceOptions.tier = 4;
            damagePierceOptions.level = 4;
            damagePierceOptions.prefab = ModularMagic_Core.prefabs.RuneDamagePierceGrausten;
            damagePierceOptions.recipe = "Grausten:3, SpearSplitner:1, Eitr:3";
            list.Add(new RuneEntry(damagePierceOptions));

            RuneEntryOptions damageSlashOptions = new RuneEntryOptions();
            damageSlashOptions.allowedWeapons = RuneData.EarthIce;
            damageSlashOptions.description = "Change the physical damage to Slash";
            damageSlashOptions.descriptionKey = LocaleKey.ItemDamageTypeSlashDesc;
            damageSlashOptions.name = "Rune: Damage Slash";
            damageSlashOptions.nameKey = LocaleKey.ItemDamageTypeSlash;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashWood;
            damageSlashOptions.recipe = $"RoundLog:3, SwordBronze:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            damageSlashOptions.type = ImbuementType.DamageType;
            damageSlashOptions.value = "Slash";
            list.Add(new RuneEntry(damageSlashOptions));

            damageSlashOptions.tier = 2;
            damageSlashOptions.level = 2;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashStone;
            damageSlashOptions.recipe = $"Stone:3, SwordSilver:1, {ModularMagic_Core.prefabs.EitrFine.name}:3";
            list.Add(new RuneEntry(damageSlashOptions));

            damageSlashOptions.tier = 3;
            damageSlashOptions.level = 3;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashMarble;
            damageSlashOptions.recipe = "BlackMarble:3, SwordBlackmetal:1, Eitr:3";
            list.Add(new RuneEntry(damageSlashOptions));

            damageSlashOptions.tier = 4;
            damageSlashOptions.level = 4;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlashGrausten;
            damageSlashOptions.recipe = "Grausten:3, SwordNiedhogg:1, Eitr:3";
            list.Add(new RuneEntry(damageSlashOptions));
        }
    }
}
