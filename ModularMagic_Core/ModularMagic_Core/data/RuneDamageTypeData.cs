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
            damageBluntOptions.id = "DamageBlunt";
            damageBluntOptions.name = "Rune: Damage Blunt";
            damageBluntOptions.nameKey = LocaleKey.ItemDamageTypeBlunt;
            damageBluntOptions.prefab = ModularMagic_Core.prefabs.RuneDamageBlunt;
            damageBluntOptions.recipe = $"RoundLog:3, MaceBronze:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            damageBluntOptions.recipeUpgrade = damageBluntOptions.recipe;
            damageBluntOptions.tiers = new int[] { 1, 2, 3, 4 };
            damageBluntOptions.type = ImbuementType.DamageType;
            damageBluntOptions.value = "Blunt";
            list.Add(new RuneEntry(damageBluntOptions));

            RuneEntryOptions damagePierceOptions = new RuneEntryOptions();
            damagePierceOptions.allowedWeapons = RuneData.Earth;
            damagePierceOptions.description = "Change the physical damage to Pierce";
            damagePierceOptions.descriptionKey = LocaleKey.ItemDamageTypePierceDesc;
            damagePierceOptions.id = "DamagePierce";
            damagePierceOptions.name = "Rune: Damage Pierce";
            damagePierceOptions.nameKey = LocaleKey.ItemDamageTypePierce;
            damagePierceOptions.prefab = ModularMagic_Core.prefabs.RuneDamagePierce;
            damagePierceOptions.recipe = $"RoundLog:3, SpearBronze:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            damagePierceOptions.recipeUpgrade = damagePierceOptions.recipe;
            damagePierceOptions.tiers = new int[] { 1, 2, 3, 4 };
            damagePierceOptions.type = ImbuementType.DamageType;
            damagePierceOptions.value = "Pierce";
            list.Add(new RuneEntry(damagePierceOptions));

            RuneEntryOptions damageSlashOptions = new RuneEntryOptions();
            damageSlashOptions.allowedWeapons = RuneData.EarthIce;
            damageSlashOptions.description = "Change the physical damage to Slash";
            damageSlashOptions.descriptionKey = LocaleKey.ItemDamageTypeSlashDesc;
            damageSlashOptions.id = "DamageSlash";
            damageSlashOptions.name = "Rune: Damage Slash";
            damageSlashOptions.nameKey = LocaleKey.ItemDamageTypeSlash;
            damageSlashOptions.prefab = ModularMagic_Core.prefabs.RuneDamageSlash;
            damageSlashOptions.recipe = $"RoundLog:3, SwordBronze:1, {ModularMagic_Core.prefabs.EitrCrude.name}:3";
            damageSlashOptions.recipeUpgrade = damageSlashOptions.recipe;
            damageSlashOptions.tiers = new int[] { 1, 2, 3, 4 };
            damageSlashOptions.type = ImbuementType.DamageType;
            damageSlashOptions.value = "Slash";
            list.Add(new RuneEntry(damageSlashOptions));
        }
    }
}
