
using System;
using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

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

                // Frozen by a TimeStop zone - skip this cycle's toggle but keep the loop alive so
                // it resumes on the same interval once unfrozen.
                if (door != null && door.GetComponent<TwitchPhysicsFreezeData>() != null)
                    continue;

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
