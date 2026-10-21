using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class ProgressionHelper
    {
        public static List<string> GetBossDefeatedList()
        {
            List<string> list = new List<string>();
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedEikthyr))
                list.Add(GlobalKeyType.DefeatedEikthyr);

            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedElder))
                list.Add(GlobalKeyType.DefeatedElder);

            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedBonemass))
                list.Add(GlobalKeyType.DefeatedBonemass);

            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedModer))
                list.Add(GlobalKeyType.DefeatedModer);

            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedYagluth))
                list.Add(GlobalKeyType.DefeatedYagluth);

            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedQueen))
                list.Add(GlobalKeyType.DefeatedQueen);

            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedFader))
                list.Add(GlobalKeyType.DefeatedFader);

            return list;
        }

        public static float GetPlayerTier()
        {
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedFader)) return 8.5f;
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedQueen)) return 7.5f;
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedYagluth)) return 6.5f;
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedModer)) return 5.5f;
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedBonemass)) return 4.5f;
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedElder)) return 3.5f;
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeyType.DefeatedEikthyr)) return 2.5f;

            return 1f;
        }

        /// <summary>
        /// Whether the world's global keys allow something with these add/remove keys. With
        /// <paramref name="assumeKey"/> set, that key is treated as present too, which answers what would be
        /// allowed after it is added (e.g. after the next boss).
        /// </summary>
        public static bool IsAllowedByGlobalKeys(string add, string remove, string assumeKey = null)
        {
            bool HasKey(string key) => (assumeKey != null && key == assumeKey) || ZoneSystem.instance.GetGlobalKey(key);

            if ((add == "" && (remove == "" || !HasKey(remove))) || (HasKey(add) && !HasKey(remove)))
                return true;

            return false;
        }
    }
}
