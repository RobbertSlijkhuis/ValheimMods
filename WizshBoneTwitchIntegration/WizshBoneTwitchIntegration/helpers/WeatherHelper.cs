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

            string weather = weatherData.cycle ? weatherData.items[0] : weatherData.items[Random.Range(0, weatherData.items.Count)];

            GameObject gameObject = ZNetViewHelper.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.EnvZone, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
            _activeWeatherZone = gameObject;

            // A freezing death inside this zone is credited to whoever redeemed it (DeathCreditHelper).
            RedeemerTagHelper.Apply(gameObject, customRewardEvent);

            TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>();

            // The zone's weather, force flag and size live in its ZDO (see TwitchWeatherZone), so every
            // client's copy of it is configured the same way, not just this one.
            string[] weathers = weatherData.cycle ? weatherData.items.ToArray() : new[] { weather };
            ZDOID followTarget = weatherData.followPlayer ? Player.m_localPlayer.GetComponent<ZNetView>().GetZDO().m_uid : ZDOID.None;
            gameObject.GetComponent<TwitchWeatherZone>().Initialize(weathers, weatherData.cycleInterval, weatherData.force, weatherData.height, weatherData.radius, followTarget, weatherData.frostResist);

            // The weather name is left out while cycling, since it changes every few seconds.
            if (weatherData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, weatherData.announceMessage) + (weatherData.cycle ? "" : $" \n{weather}"), 3000);

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
