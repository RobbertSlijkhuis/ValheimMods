using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    /// <summary>
    /// Lifecycle hook for <see cref="LeaderboardHelper"/> (which holds all the state and logic):
    /// ticks its write debounce, and flushes pending stats when the Game object goes away
    /// (logout / world exit) or the application quits.
    /// </summary>
    internal class LeaderboardFlusher : MonoBehaviour
    {
        private void Update()
        {
            LeaderboardHelper.Tick();
        }

        private void OnDestroy()
        {
            LeaderboardHelper.Flush();
        }

        private void OnApplicationQuit()
        {
            LeaderboardHelper.Flush();
        }
    }
}
