
using System;
using System.Collections;
using UnityEngine;

namespace WizshBoneTwitchIntegration.extensions
{
    internal static class DoorExtension
    {
        public static IEnumerator ToggleDoorContinuously(this Door door, float interval)
        {
            ZNetView netView = door.GetComponent<ZNetView>();
            while (door != null)
            {
                yield return new WaitForSeconds(interval);

                if (door != null && Player.m_localPlayer != null && netView != null && netView.IsOwner())
                {
                    try
                    {
                        door?.Interact(Player.m_localPlayer, false, false);
                    }
                    catch (Exception e)
                    {
                        Jotunn.Logger.LogWarning($"Door Interact threw, likely despawned mid-frame: {e}");
                    }
                }
            }
        }
    }
}
