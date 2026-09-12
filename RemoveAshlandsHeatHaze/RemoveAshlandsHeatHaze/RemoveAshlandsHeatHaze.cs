using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;

namespace RemoveAshlandsHeatHaze
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.Minor)]
    internal class RemoveAshlandsHeatHaze : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.RemoveAshlandsHeatHaze";
        public const string PluginName = "RemoveAshlandsHeatHaze";
        public const string PluginVersion = "0.0.1";

        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
