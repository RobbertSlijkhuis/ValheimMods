
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
            Animator animator = door.GetComponentInChildren<Animator>();
            while (door != null)
            {
                yield return new WaitForSeconds(interval);

                // Frozen by a TimeStop zone - skip this cycle's toggle but keep the loop alive so
                // it resumes on the same interval once unfrozen.
                if (door != null && door.GetComponent<TwitchPhysicsFreezeData>() != null)
                    continue;

                if (door != null && netView != null && netView.IsOwner())
                {
                    try
                    {
                        ToggleState(netView, animator);
                    }
                    catch (Exception e)
                    {
                        Jotunn.Logger.LogWarning($"Door toggle threw, likely despawned mid-frame: {e}");
                    }
                }
            }
        }

        // Door.Interact(Player.m_localPlayer, ...) routes through PrivateArea.CheckAccess(), which
        // checks Player.m_localPlayer's ward permission regardless of who/what is "interacting" - so
        // a door inside a ward the local player can't access silently never toggles. RPC_UseDoor and
        // the ZDOVars.s_state polling that drives the animator/effects have no such check, so we flip
        // the ZDO state directly (as owner) and let vanilla's own InvokeRepeating("UpdateState", ...)
        // animate it on every client, bypassing the permission check entirely.
        private static void ToggleState(ZNetView netView, Animator animator)
        {
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsTag("open") && !stateInfo.IsTag("closed"))
                    return; // mid-transition, matches vanilla CanInteract()'s guard - skip this tick
            }

            ZDO zdo = netView.GetZDO();
            int state = zdo.GetInt(ZDOVars.s_state);
            zdo.Set(ZDOVars.s_state, state == 0 ? 1 : 0);
        }
    }
}
