using System.Collections;
using UnityEngine;
using static Trap;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class TrapExtension
    {
        public static IEnumerator ArmTrapsAfterDelay(this Trap trap, float delay, Rigidbody rigidbody)
        {
            yield return new WaitForSeconds(delay);

            rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            trap.RequestStateChange(TrapState.Armed);
        }

        public static IEnumerator CheckForGrounded(this Trap trap, float delay, Rigidbody rigidbody)
        {
            yield return new WaitForSeconds(delay);

            trap.StartCoroutine(ArmTrap(trap, rigidbody));
        }

        public static string ArmTrap(this Trap trap, Rigidbody rigidbody)
        {
            // Check if vertical movement has essentially stopped
            if (Mathf.Abs(rigidbody.linearVelocity.y) < 0.01f)
            {
                trap.CancelInvoke(nameof(ArmTrap));
                Jotunn.Logger.LogWarning("Object is no longer moving vertically.");
                rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                trap.RequestStateChange(TrapState.Armed);
            }

            return "";
        }
    }
}
