using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    // A MonoBehaviour that counts down to something (an object's removal, a terrain reset, ...) and
    // can have its time left shown by the debug-mode RedeemTimerLabel. gameObject is satisfied
    // implicitly by MonoBehaviour.
    internal interface IRedeemTimer
    {
        GameObject gameObject { get; }

        // Seconds left, or -1 if the countdown hasn't started.
        float RemainingSeconds { get; }

        bool IsPaused { get; }
    }
}
