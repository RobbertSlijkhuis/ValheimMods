using BepInEx.Configuration;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
using ModularMagic_Utilities.Types;
using System.Security.Policy;

namespace ModularMagic_Utilities.Configs
{
    internal class WeatherZoneConfig
    {
        // General options
        public static string[] lightColorPresetOptions = new string[] {
            LightColorPresetType.Red, LightColorPresetType.Orange, LightColorPresetType.Yellow, LightColorPresetType.Green, LightColorPresetType.LemonGreen,
            LightColorPresetType.LightBlue, LightColorPresetType.Blue, LightColorPresetType.Purple, LightColorPresetType.Pink, LightColorPresetType.White };

        // The  fields to generate
        public ConfigEntry<bool> enable;
        // public ConfigEntry<string> weatherName;
        public ConfigEntry<string> prefabName;
        public ConfigEntry<string> lightColorPreset;
        public string weatherName;

        public void GenerateConfig(WeatherZoneConfigOptions options)
        {
            ConfigFile Config = ModularMagic_Utilities.Instance.Config;
            weatherName = options.weatherName;

            enable = Config.Bind(new ConfigDefinition(options.sectionName, options.weatherName + " Enable"), options.enable,
               new ConfigDescription("Wether this entry is enabled in the game", null,
               new ConfigurationManagerAttributes { IsAdminOnly = true, Order = options.order }));
            enable.SettingChanged += (obj, attr) =>
            {
            };

            prefabName = Config.Bind(new ConfigDefinition(options.sectionName, options.weatherName + " Prefab name"), options.prefabName,
                new ConfigDescription("The name of the prefab (item) that will activate this weather", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = options.order }));
            prefabName.SettingChanged += (obj, attr) =>
            {
            };

            lightColorPreset = Config.Bind(new ConfigDefinition(options.sectionName, options.weatherName + " Light color preset"), options.lightColorPreset,
                new ConfigDescription("A preset of colors for light, particles, flare and emissions",
                new AcceptableValueList<string>(lightColorPresetOptions),
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = options.order }));
        }
    }
}
