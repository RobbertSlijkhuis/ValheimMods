using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class LightColorPresetHelper
    {
        public static LightPresetColors GetColors(string value)
        {
            Color flareColor = new Color(1f, 1f, 1f, 0.098f);
            Color emissionColor = new Color(1f, 1f, 1f, 1f);
            Color lightColor = new Color(1f, 1f, 1f, 1f);

            switch (value)
            {
                case nameof(LightColorPresetType.Red):
                    flareColor = new Color(1f, 0.2117647f, 0.2261669f, 0.09803922f); // #FF363A19
                    emissionColor = new Color(2.4f, 0.150596f, 0.1345381f, 1f);
                    lightColor = new Color(1f, 0.3001108f, 0.2862746f, 1f); // #FF4D49
                    break;
                case nameof(LightColorPresetType.Orange):
                    flareColor = new Color(1f, 0.5899594f, 0.1132353f, 0.09803922f); // #FF7D3619
                    emissionColor = new Color(2.2f, 0.5504988f, 0.1382199f, 1f);
                    lightColor = new Color(1f, 0.6910468f, 0.482353f, 1f); // #FFB17C
                    break;
                case nameof(LightColorPresetType.Yellow):
                    flareColor = new Color(1f, 0.4901961f, 0.2117647f, 0.09803922f); // #FF7D3619
                    emissionColor = new Color(1.97667456f, 1.14168906f, 0.131697819f, 1f);
                    lightColor = new Color(1f, 0.7845517f, 0.2877358f, 1f); // #FFC849
                    break;
                case nameof(LightColorPresetType.LemonGreen):
                    flareColor = new Color(0.7708426f, 1, 0.05882353f, 0.09803922f); // #C5FF0F
                    emissionColor = new Color(1.056215f, 1.6f, 0.1463609f, 1f);
                    lightColor = new Color(0.8803066f, 1f, 0.504717f, 1f); // #E0FF81
                    break;
                case nameof(LightColorPresetType.Green):
                    flareColor = new Color(0f, 1, 0.04705882f, 0.09803922f); // #00FF0C19
                    emissionColor = new Color(0.382916f, 1.9f, 0f, 1f);
                    lightColor = new Color(0.6295277f, 1f, 0.4823529f, 1f); // #FFC849
                    break;
                case nameof(LightColorPresetType.LightBlue):
                    flareColor = new Color(0.5801887f, 0.8593694f, 1f, 0.09803922f); // #94DBFF19
                    emissionColor = new Color(1.26792896f, 1.7979852f, 1.96205056f, 1f);
                    lightColor = new Color(0.8349056f, 0.9467509f, 1f, 1f); // #D5F1FF
                    break;
                case nameof(LightColorPresetType.Blue):
                    flareColor = new Color(0f, 0.7254902f, 1f, 0.09803922f); // #00B9FF19
                    emissionColor = new Color(0f, 0.9814469f, 2.3f, 1f);
                    lightColor = new Color(0.482353f, 0.7708116f, 1f, 1f); // #7BC5FF
                    break;
                case nameof(LightColorPresetType.Pink):
                    flareColor = new Color(1f, 0.4784314f, 0.7215686f, 0.09803922f); // #FF7AB819
                    emissionColor = new Color(1.97667456f, 0f, 1.39524257f, 1f);
                    lightColor = new Color(1f, 0.7688679f, 0.8862273f, 1f); // #FFC4E2
                    break;
                case nameof(LightColorPresetType.Purple):
                    flareColor = new Color(0.6762155f, 0.3333334f, 1f, 0.09803922f); // #AC55FF
                    emissionColor = new Color(0.9f, 0.7814469f, 1.6f, 1f);
                    lightColor = new Color(0.7581853f, 0.504717f, 1f, 1f); // #C181FF
                    break;
                case nameof(LightColorPresetType.White):
                    flareColor = new Color(1f, 1f, 1f, 0.098f);
                    emissionColor = new Color(1.7f, 1.7f, 1.7f, 1f);
                    lightColor = new Color(1f, 1f, 1f, 1f);
                    break;
                default:
                    flareColor = new Color(1f, 1f, 1f, 0.098f);
                    emissionColor = new Color(1f, 1f, 1f, 1f);
                    lightColor = new Color(1f, 1f, 1f, 1f);
                    break;
            }

            return new LightPresetColors
            {
                lightPreset = value,
                flareColor = flareColor,
                emissionColor = emissionColor,
                lightColor = lightColor,
            };
        }

        public static Color ApplyMultiplierToColor(Color color, float multiplier)
        {
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, 1f);
        }

        public static string GetLightPresetByInt(int value)
        {
            switch (value)
            {
                case 0:
                    return LightColorPresetType.None;
                case 1:
                    return LightColorPresetType.Red;
                case 2:
                    return LightColorPresetType.Orange;
                case 3:
                    return LightColorPresetType.Yellow;
                case 4:
                    return LightColorPresetType.LemonGreen;
                case 5:
                    return LightColorPresetType.Green;
                case 6:
                    return LightColorPresetType.LightBlue;
                case 7:
                    return LightColorPresetType.Blue;
                case 8:
                    return LightColorPresetType.Pink;
                case 9:
                    return LightColorPresetType.Purple;
                case 10:
                    return LightColorPresetType.White;
                default:
                    return LightColorPresetType.Blue;
            }
        }

        public static int GetIntByLightPreset(string value)
        {
            switch (value)
            {
                case nameof(LightColorPresetType.None):
                    return 0;
                case nameof(LightColorPresetType.Red):
                    return 1;
                case nameof(LightColorPresetType.Orange):
                    return 2;
                case nameof(LightColorPresetType.Yellow):
                    return 3;
                case nameof(LightColorPresetType.LemonGreen):
                    return 4;
                case nameof(LightColorPresetType.Green):
                    return 5;
                case nameof(LightColorPresetType.LightBlue):
                    return 6;
                case nameof(LightColorPresetType.Blue):
                    return 7;
                case nameof(LightColorPresetType.Pink):
                    return 8;
                case nameof(LightColorPresetType.Purple):
                    return 9;
                case nameof(LightColorPresetType.White):
                    return 10;
                default:
                    return 0;
            }
        }
    }
}
