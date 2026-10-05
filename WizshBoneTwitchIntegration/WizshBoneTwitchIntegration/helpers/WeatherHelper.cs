using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class WeatherHelper
    {
        private static GameObject _activeWeatherZone;

        public static void SpawnWeather(WeatherData weatherData, CustomRewardEvent customRewardEvent)
        {
            if (weatherData.items.Count == 0)
            {
                weatherData.items.Add("Clear");
                Jotunn.Logger.LogWarning("Could not find any weather in the list, adding clear!");
            }

            if (Player.m_localPlayer == null)
            {
                Jotunn.Logger.LogWarning("Could not find local player, cannot spawn weather zone.");
                return;
            }

            if (_activeWeatherZone != null)
            {
                try
                {
                    Jotunn.Logger.LogInfo("Replacing active weather zone with new one.");
                    ZNetScene.instance.Destroy(_activeWeatherZone);
                }
                catch (System.Exception e)
                {
                    Jotunn.Logger.LogWarning($"Failed to destroy previous weather zone, skipping removal: {e.Message}");
                }
                finally
                {
                    _activeWeatherZone = null;
                }
            }

            string weather = weatherData.items[Random.Range(0, weatherData.items.Count)];

            GameObject gameObject = ZNetViewHelper.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.EnvZone, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
            _activeWeatherZone = gameObject;

            // A freezing death inside this zone is credited to whoever redeemed it (DeathCreditHelper).
            RedeemerTagHelper.Apply(gameObject, customRewardEvent);

            CapsuleCollider capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
            EnvZone envZone = gameObject.GetComponent<EnvZone>();
            TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>();
            envZone.m_environment = weather;
            envZone.m_force = weatherData.force;

            capsuleCollider.height = weatherData.height;
            capsuleCollider.radius = weatherData.radius;

            if (weatherData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, weatherData.announceMessage) + $" \n{weather}", 3000);

            // duration == 0 means indefinite/never auto-cleanup - don't start the destruction timer,
            // since SetStarted(0) would trigger TimedDestruction with a 0s timeout and destroy the
            // zone almost immediately instead of leaving it in place.
            if (weatherData.duration > 0)
                persistentDestruction.SetStarted(weatherData.duration);

            persistentDestruction.onEnd = OnDestroy;
        }

        public static void OnDestroy(GameObject prefab)
        {
            EnvZone envZone = prefab.GetComponent<EnvZone>();

            if (envZone == null)
            {
                Jotunn.Logger.LogWarning("Could not find EnvZone to set");
                return;
            }

            envZone.m_environment = null;

            if (envZone.m_force)
                EnvMan.instance.SetForceEnvironment("");

            _activeWeatherZone = null;
        }
    }
}
