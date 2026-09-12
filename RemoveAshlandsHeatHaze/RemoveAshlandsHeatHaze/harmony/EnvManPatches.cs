using HarmonyLib;
using RemoveAshlandsHeatHaze.Helpers;
using System;

namespace RemoveAshlandsHeatHaze.Harmony
{
    [HarmonyPatch]
    public class EnvManPatches
    {
        // "env" matches EnvMan.SetEnv's private parameter name so Harmony passes it through.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnvMan), "SetEnv")]
        public static void SetEnv_Postfix(EnvSetup env)
        {
            try
            {
                AshlandsHeatHazeHelper.SuppressIfActive(env?.m_name);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in EnvMan.SetEnv_Postfix: " + e);
            }
        }
    }
}
