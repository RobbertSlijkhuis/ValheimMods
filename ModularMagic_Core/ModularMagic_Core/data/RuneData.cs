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
        }
    }
}
