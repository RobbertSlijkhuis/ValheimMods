using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using RestingRockFace.Components;
using RestingRockFace.Gui;
using System;
using System.Reflection;
using UnityEngine;

namespace RestingRockFace
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class RestingRockFace : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.RestingRockFace";
        public const string PluginName = "RestingRockFace";
        public const string PluginVersion = "1.0.0";
        public static RestingRockFace Instance;
        private static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(PluginGUID);

        public RockyGUI rockyGUI;

        public void Awake()
        {
            Instance = this;
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += SetupRocky;
        }

        private void SetupRocky()
        {
            try
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab("Placeable_HardRock");
                Transform defaultTransform = prefab.transform.Find("default");
                prefab.AddComponent<RockyControls>();

                PrefabManager.OnVanillaPrefabsAvailable -= SetupRocky;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update rocky: " + e);
                PrefabManager.OnVanillaPrefabsAvailable -= SetupRocky;
            }
        }
    }
}

