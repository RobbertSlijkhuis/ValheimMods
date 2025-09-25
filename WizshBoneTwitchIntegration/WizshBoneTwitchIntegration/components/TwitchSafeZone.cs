using UnityEngine;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSafeZone : MonoBehaviour
    {
        private GameObject playerInZone;

        private void OnTriggerEnter(Collider other)
        {
            HandlePlayerInSafeZone(other, true, "Player entered");
        }

        private void OnTriggerStay(Collider other)
        {
            HandlePlayerInSafeZone(other, true);
        }

        private void OnTriggerExit(Collider other)
        {
            HandlePlayerInSafeZone(other, false, "Player left");
        }

        private void HandlePlayerInSafeZone(Collider other, bool value, string message = null)
        {
            if (other.gameObject.name != "Player(Clone)")
                return;

            TwitchCustomRewards comp = other.gameObject.GetComponent<TwitchCustomRewards>();
            comp.isPlayerInSafeZone = value;

            if (value)
                playerInZone = other.gameObject;
            else
                playerInZone = null;

            //if (message != null)
            //    Jotunn.Logger.LogWarning(message + (playerInZone ? $" {playerInZone.name}" : " NULL"));
        }

        private void OnDestroy()
        {
            if (playerInZone == null)
                return;

            // Jotunn.Logger.LogWarning("Ward destroyed, removing player from zone");
            TwitchCustomRewards comp = playerInZone.GetComponent<TwitchCustomRewards>();
            comp.isPlayerInSafeZone = false;
        }
    }
}
