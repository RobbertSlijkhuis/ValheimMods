using HarmonyLib;
using RestingRockFace.Components;
using RestingRockFace.Helpers;
using RestingRockFace.Types;
using System;

namespace RestingRockFace.Harmony
{
    [HarmonyPatch]
    public class PatchesRRF
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Tameable), "Interact")]
        public static bool Interact_Prefix(ref Tameable __instance, Humanoid user, bool hold, bool alt)
        {
            try
            {
                if (__instance == null || hold == true || alt == false)
                    return true;

                if (__instance.gameObject.name != "Placeable_HardRock(Clone)")
                    return true;

                RockyControls rockyControls = __instance.gameObject.GetComponent<RockyControls>();
                rockyControls.m_gui.ShowGUI();
                return false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Somethign went wrong in GetText_Prefix: " + e);
                return true;
            }
        }

        // GameCamera.UpdateCamera applies the mouse wheel to the zoom distance. While the Rocky panel
        // is open, skip it only on frames where the wheel moved so it only scrolls the dropdown.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
        public static bool UpdateCamera_Prefix()
        {
            return !(RockyGuiState.IsOpen && ZInput.GetMouseScrollWheel() != 0f);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pet), "SetFace")]
        public static bool SetFace_Prefix(ref Tameable __instance, int index)
        {
            try
            {
                if (__instance == null)
                    return true;

                if (__instance.gameObject.name != "Placeable_HardRock(Clone)")
                    return true;

                RockyControls rockyControls = __instance.gameObject.GetComponent<RockyControls>();

                if (rockyControls == null)
                    throw new Exception("RockyControls is null somehow");

                if (rockyControls.m_face == (int)RockyFaceEnum.Nothing)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Somethign went wrong in SetFace_Prefix: " + e);
                return true;
            }
        }
    }
}
