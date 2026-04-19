using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class WeatherHelper
    {
        public static void SpawnWeather(WeatherData weatherData, CustomRewardEvent customRewardEvent)
        {
            //List<string> weathers = new List<string>();
            //weathers.Add("Clear");
            //weathers.Add("Heath_clear");
            //weathers.Add("Mistlands_clear");
            //weathers.Add("Twilight_Clear");
            //weathers.Add("Eikthyr");
            //weathers.Add("GDKing");
            //weathers.Add("Bonemass");
            //weathers.Add("Moder");
            //weathers.Add("GoblinKing");
            //weathers.Add("Queen");
            //weathers.Add("Fader");
            //weathers.Add("Misty");
            //weathers.Add("ThunderStorm");
            //weathers.Add("SnowStorm");
            //weathers.Add("Twilight_SnowStorm");
            //weathers.Add("Mistlands_thunder");
            //weathers.Add("Ghosts");
            //weathers.Add("CavesHildir");

            if (weatherData.items.Count == 0)
            {
                //Jotunn.Logger.LogWarning("Could not find any weather in the list, adding clear!");
                weatherData.items.Add("Clear");
            }

            string weather = weatherData.items[Random.Range(0, weatherData.items.Count)];
            //Jotunn.Logger.LogWarning("Chosen weather: " + weather);

            GameObject gameObject = UnityEngine.Object.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.WeatherZone, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
            //GameObject gameObject;

            //if (weatherData.attach)
            //    gameObject = UnityEngine.Object.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.WeatherZone, Player.m_localPlayer.transform);
            //else
            //    gameObject = UnityEngine.Object.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.WeatherZone, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);

            CapsuleCollider capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
            EnvZone envZone = gameObject.GetComponent<EnvZone>();
            TwitchPersistentDestruction persistentDestruction = gameObject.GetComponent<TwitchPersistentDestruction>();
            envZone.m_environment = weather;
            envZone.m_force = weatherData.force;

            capsuleCollider.height = weatherData.height;
            capsuleCollider.radius = weatherData.radius;

            if (weatherData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, weatherData.announceMessage), 3000);

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

            //Jotunn.Logger.LogWarning($"Weather {envZone.m_environment} has been reset!");
            envZone.m_environment = null;

            if (envZone.m_force)
                EnvMan.instance.SetForceEnvironment("");
        }
    }
}
