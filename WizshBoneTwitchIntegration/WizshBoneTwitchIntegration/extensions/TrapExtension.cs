using System.Collections;
using UnityEngine;
using static Trap;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class TrapExtension
    {
        public static IEnumerator ArmTrapAfterDelay(this Trap trap, float delay)
        {
            yield return new WaitForSeconds(delay);

            trap.RequestStateChange(TrapState.Armed);
        }
    }
}