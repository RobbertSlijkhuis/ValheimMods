using ModularMagic_Core.Models;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneData
    {
        public static List<RuneEntry> list = new List<RuneEntry>();

        public static void Init()
        {
            new RuneDamageTypeData(list);
            new RuneMiscData(list);
            new RuneProjectileData(list);
            new RuneMainAttackData(list);
            new RuneSecondaryAttackData(list);
            new RuneWeaponData(list);
        }

        /// <summary>
        /// Adds a rune, unless its prefab is not in the asset bundle (see ModularMagic_Core.AddRunePrefab). The entry binds the config of the
        /// rune item, which needs the prefab.
        /// </summary>
        public static void Add(List<RuneEntry> list, RuneEntryOptions options)
        {
            if (options.prefab == null)
                return;

            list.Add(new RuneEntry(options));
        }
    }
}
