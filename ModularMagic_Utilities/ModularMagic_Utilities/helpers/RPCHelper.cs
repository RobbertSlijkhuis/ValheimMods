using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Utilities.Helpers
{
    internal class RPCHelper
    {
        public static LanternPackageResult ReadLanternPackage(ZPackage package)
        {
            try
            {
                Material lanternOn = null;
                Material lanternOff = null;

                string[] data = package.ReadString().Split(',');
                long playerId = long.Parse(data[0]);
                int type = int.Parse(data[1]);
                bool value = bool.Parse(data[2]);
                bool applyLanternChanges = bool.Parse(data[3]);
                Color flareColor = new Color(1f, 1f, 1f, 0.098f);
                Color lightColor = new Color(1f, 1f, 1f, 1f);
                float? lightRange = null;
                float? lightIntensity = null;
                string materialColor = null;

                Jotunn.Logger.LogWarning("PlayerId: " + playerId);
                Jotunn.Logger.LogWarning("Type: " + type);
                Jotunn.Logger.LogWarning("Value: " + value);
                Jotunn.Logger.LogWarning("ApplyLanternChanges: " + applyLanternChanges);

                switch (type)
                {
                    case 1:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern1Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern1OffMat;
                        flareColor = ConfigUtilities.lantern1.flareColor.Value;
                        lightColor = ConfigUtilities.lantern1.lightColor.Value;
                        lightRange = ConfigUtilities.lantern1.lightRange.Value;
                        lightIntensity = ConfigUtilities.lantern1.lightIntensity.Value;
                        materialColor = ConfigUtilities.lantern1.materialColor.Value;
                        break;
                    case 2:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern2Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern2OffMat;
                        flareColor = ConfigUtilities.lantern2.flareColor.Value;
                        lightColor = ConfigUtilities.lantern2.lightColor.Value;
                        lightRange = ConfigUtilities.lantern2.lightRange.Value;
                        lightIntensity = ConfigUtilities.lantern2.lightIntensity.Value;
                        materialColor = ConfigUtilities.lantern2.materialColor.Value;
                        break;
                    case 3:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern3Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern3OffMat;
                        flareColor = ConfigUtilities.lantern3.flareColor.Value;
                        lightColor = ConfigUtilities.lantern3.lightColor.Value;
                        lightRange = ConfigUtilities.lantern3.lightRange.Value;
                        lightIntensity = ConfigUtilities.lantern3.lightIntensity.Value;
                        materialColor = ConfigUtilities.lantern3.materialColor.Value;
                        break;
                }

                return new LanternPackageResult(lanternOn, lanternOff, playerId, type, value, applyLanternChanges, flareColor, lightColor, lightRange, lightIntensity, materialColor);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not get read lantern package: " + error);
                return null;
            }
        }

        public static int GetLanternType(ItemData itemData)
        {
            try
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
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not get lantern type: "+ error);
                return 0;
            }
        }
    }
}
