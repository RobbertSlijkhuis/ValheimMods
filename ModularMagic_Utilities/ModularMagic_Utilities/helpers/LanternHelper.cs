using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Utilities.Helpers
{
    internal class LanternHelper
    {
        public static LanternConfig GetLanternConfig(int type)
        {
            try
            {
                Material lanternOn = null;
                Material lanternOff = null;
                Color flareColor = new Color(1f, 1f, 1f, 0.098f);
                Color glassColor = new Color(1f, 1f, 1f, 1f);
                Color lightColor = new Color(1f, 1f, 1f, 1f);
                string lightColorPreset = "White";
                float? lightRange = null;
                float? lightIntensity = null;

                switch (type)
                {
                    case 1:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern1Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern1OffMat;
                        lightColorPreset = PluginConfig.lantern1.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern1.lightRange.Value;
                        lightIntensity = PluginConfig.lantern1.lightIntensity.Value;
                        break;
                    case 2:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern2Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern2OffMat;
                        lightColorPreset = PluginConfig.lantern2.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern2.lightRange.Value;
                        lightIntensity = PluginConfig.lantern2.lightIntensity.Value;
                        break;
                    case 3:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern3Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern3OffMat;
                        lightColorPreset = PluginConfig.lantern3.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern3.lightRange.Value;
                        lightIntensity = PluginConfig.lantern3.lightIntensity.Value;
                        break;
                }

                switch (lightColorPreset)
                {
                    case "Red":
                        flareColor = new Color(1f, 0.2117647f, 0.2261669f, 0.09803922f); // #FF363A19
                        glassColor = new Color(2.4f, 0.150596f, 0.1345381f, 1f);
                        lightColor = new Color(1f, 0.3001108f, 0.2862746f, 1f); // #FF4D49
                        break;
                    case "Orange":
                        flareColor = new Color(1f, 0.4899594f, 0.2132353f, 0.09803922f); // #FF7D3619
                        glassColor = new Color(2.1f, 0.325123f, 0f, 1f);
                        lightColor = new Color(1f, 0.6207767f, 0.4823529f, 1f); // #FF9E7B
                        break;
                    case "Yellow":
                        flareColor = new Color(1f, 0.4901961f, 0.2117647f, 0.09803922f); // #FF7D3619
                        glassColor = new Color(1.97667456f, 1.14168906f, 0.131697819f, 1f);
                        lightColor = new Color(1f, 0.7845517f, 0.2877358f, 1f); // #FFC849
                        break;
                    case "Green":
                        flareColor = new Color(0f, 1, 0.04705882f, 0.09803922f); // #00FF0C19
                        glassColor = new Color(0.382916f, 1.9f, 0f, 1f);
                        lightColor = new Color(0.6295277f, 1f, 0.4823529f, 1f); // #FFC849
                        break;
                    case "LemonGreen":
                        flareColor = new Color(0.4578981f, 1, 0f, 0.09803922f); // #75FF0019
                        glassColor = new Color(0.9056982f, 1.9f, 0f, 1f);
                        lightColor = new Color(0.8171905f, 1f, 0.482353f, 1f); // #FFC849
                        break;
                    case "LightBlue":
                        flareColor = new Color(0.5801887f, 0.8593694f, 1f, 0.09803922f); // #94DBFF19
                        glassColor = new Color(1.26792896f, 1.7979852f, 1.96205056f, 1f);
                        lightColor = new Color(0.8349056f, 0.9467509f, 1f, 1f); // #D5F1FF
                        break;
                    case "Blue":
                        flareColor = new Color(0f, 0.7254902f, 1f, 0.09803922f); // #00B9FF19
                        glassColor = new Color(0f, 0.9814469f, 2.3f, 1f);
                        lightColor = new Color(0.482353f, 0.7708116f, 1f, 1f); // #7BC5FF
                        break;
                    case "Pink":
                        flareColor = new Color(1f, 0.4784314f, 0.7215686f, 0.09803922f); // #FF7AB819
                        glassColor = new Color(1.97667456f, 0f, 1.39524257f, 1f);
                        lightColor = new Color(1f, 0.7688679f, 0.8862273f, 1f); // #FFC4E2
                        break;
                    case "White":
                        flareColor = new Color(1f, 1f, 1f, 0.098f);
                        glassColor = new Color(1.7f, 1.7f, 1.7f, 1f);
                        lightColor = new Color(1f, 1f, 1f, 1f);
                        break;
                    default:
                        flareColor = new Color(1f, 1f, 1f, 0.098f);
                        glassColor = new Color(1f, 1f, 1f, 1f);
                        lightColor = new Color(1f, 1f, 1f, 1f);
                        break;
                }

                return new LanternConfig
                {
                    lanternOn = lanternOn,
                    lanternOff = lanternOff,
                    flareColor = flareColor,
                    glassColor = glassColor,
                    lightColor = lightColor,
                    lightColorPreset = lightColorPreset,
                    lightIntensity = (float)lightIntensity,
                    lightRange = (float)lightRange,

                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not get lantern config: " + e);
                return null;
            }
        }

        public static int GetLanternType(ItemData itemData)
        {
            try
            {
                int type = 0;

                if (itemData.m_shared.m_name == PluginConfig.lantern1.name.Value)
                {
                    type = 1;
                }
                else if (itemData.m_shared.m_name == PluginConfig.lantern2.name.Value)
                {
                    type = 2;
                }
                else if (itemData.m_shared.m_name == PluginConfig.lantern3.name.Value)
                {
                    type = 3;
                }

                return type;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not get lantern type: " + e);
                return 0;
            }
        }
    }
}
