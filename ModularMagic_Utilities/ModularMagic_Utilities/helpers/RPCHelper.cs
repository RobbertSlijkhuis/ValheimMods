using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Utilities.Helpers
{
    internal class RPCHelper
    {
        public static LanternPackageResult ReadLanternPackage(ZPackage package)
        {
            Material lanternOn = null;
            Material lanternOff = null;

            string[] data = package.ReadString().Split(',');
            long playerId = long.Parse(data[0]);
            int type = int.Parse(data[1]);
            bool value = bool.Parse(data[2]);
            bool applyLanternChanges = bool.Parse(data[3]);

            Jotunn.Logger.LogWarning("PlayerId: " + playerId);
            Jotunn.Logger.LogWarning("Type: " + type);
            Jotunn.Logger.LogWarning("Value: " + value);
            Jotunn.Logger.LogWarning("ApplyLanternChanges: " + applyLanternChanges);

            switch (type)
            {
                case 1:
                    lanternOn = ModularMagic_Utilities.Instance.materials.lantern1Mat;
                    lanternOff = ModularMagic_Utilities.Instance.materials.lantern1OffMat;
                    break;
                case 2:
                    lanternOn = ModularMagic_Utilities.Instance.materials.lantern2Mat;
                    lanternOff = ModularMagic_Utilities.Instance.materials.lantern2OffMat;
                    break;
                case 3:
                    lanternOn = ModularMagic_Utilities.Instance.materials.lantern3Mat;
                    lanternOff = ModularMagic_Utilities.Instance.materials.lantern3OffMat;
                    break;
            }

            return new LanternPackageResult(lanternOn, lanternOff, playerId, type, value, applyLanternChanges);
        }

        public static int GetLanternType(ItemData itemData)
        {
            int type = 0;

            if (itemData.m_shared.m_name == ConfigUtilities.lantern1.name.Value)
            {
                type = 1;
            }
            else if (itemData.m_shared.m_name == ConfigUtilities.lantern2.name.Value)
            {
                type = 2;
            }
            else if (itemData.m_shared.m_name == ConfigUtilities.lantern3.name.Value)
            {
                type = 3;
            }

            return type;
        }
    }
}
