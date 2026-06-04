using BepInEx;
using Glimuleikar.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Reflection;

namespace Glimuleikar
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Glimuleikar : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.Glimuleikar";
        public const string PluginName = "Glimuleikar";
        public const string PluginVersion = "0.0.1";
        public static Glimuleikar Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            Instance = this;
            //InitAssetBundle();
            PluginConfig.Init();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //PrefabManager.OnVanillaPrefabsAvailable += InitExtraConfigFiles;
        }
    }
}

