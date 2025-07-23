using BepInEx.Configuration;
using PlayAsSkeleton.Configs;
using PlayAsSkeleton.Models;
using System;
using UnityEngine;

namespace PlayAsSkeleton.Configs
{
    internal static class PluginConfig
    {
        public static string generalSectionname = "General";
        public static ConfigEntry<KeyboardShortcut> configLanternModKey;

        public static void Init()
        {
            InitGeneralConfig();
        }

        public static void InitGeneralConfig()
        {
            configLanternModKey = PlayAsSkeleton.Instance.Config.Bind(generalSectionname, "Show settings in-game", new KeyboardShortcut(KeyCode.F4),
                new ConfigDescription("Settings of the skeleton skin", null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, Order = 1000 }));
        }
    }
}
