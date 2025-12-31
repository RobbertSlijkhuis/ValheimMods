using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections.Generic;

namespace ModularMagic_Core.Data
{
    internal class RuneData
    {
        public static List<RuneEntry> list = new List<RuneEntry>();
        public static List<string> Earth = new List<string>() { WeaponType.MMES };
        public static List<string> EarthIce = new List<string>() { WeaponType.MMES, WeaponType.MMIS };
        public static List<string> EarthFireIce = new List<string>() { WeaponType.MMES, WeaponType.MMFS, WeaponType.MMIS };
        public static List<string> EarthFireIceLightning = new List<string>() { WeaponType.MMES, WeaponType.MMFS, WeaponType.MMIS, WeaponType.MMLS };
        public static List<string> Ice = new List<string>() { WeaponType.MMIS };

        public static void Init()
        {
            new RuneDamageTypeData(list);
            new RuneProjectileData(list);
            new RuneSecondaryAttackData(list);
        }
    }
}
