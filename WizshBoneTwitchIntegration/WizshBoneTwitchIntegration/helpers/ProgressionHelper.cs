using System.Collections.Generic;
using WizshBoneTwitchIntegration.Data;
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

        public static bool IsAllowedByGlobalKeys(string add, string remove)
        {
            if ((add == "" && (remove == "" || !ZoneSystem.instance.GetGlobalKey(remove))) || (ZoneSystem.instance.GetGlobalKey(add) && !ZoneSystem.instance.GetGlobalKey(remove)))
                return true;

            return false;
        }
    }
}
