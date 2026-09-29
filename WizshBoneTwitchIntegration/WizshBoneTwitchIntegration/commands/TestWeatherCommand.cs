using Jotunn.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class TestWeatherCommand : ConsoleCommand
    {
        public override string Name => "WBTITestWeather";
        public override string Help => "Cycle through every weather in the weather picker catalog, one after another, announcing each on screen. Usage: WBTITestWeather [seconds per weather, default 4]; 'WBTITestWeather stop' stops a running test early.";

        private const float DefaultSecondsPerWeather = 4f;

        private static Coroutine m_testCoroutine;

        public override void Run(string[] args)
        {
            if (args.Length > 0 && args[0].ToLowerInvariant() == "stop")
            {
                StopTest();
                return;
            }

            float seconds = DefaultSecondsPerWeather;
            if (args.Length > 0 && float.TryParse(args[0], out float parsed) && parsed >= 1f)
                seconds = parsed;

            StartTest(seconds);
        }

        private void StartTest(float seconds)
        {
            if (Player.m_localPlayer == null)
            {
                Jotunn.Logger.LogError("No local player, cannot start weather test!");
                return;
            }

            if (RedeemPrefabCatalog.WeatherNames.Count == 0)
                RedeemPrefabCatalog.BuildWeatherCatalogOnce();

            List<string> weathers = RedeemPrefabCatalog.WeatherNames.Select(option => option.Value).ToList();

            if (weathers.Count == 0)
            {
                Jotunn.Logger.LogError("Weather catalog is empty, cannot start weather test!");
                return;
            }

            if (m_testCoroutine != null)
            {
                Jotunn.Logger.LogWarning("Weather test already running, restarting...");
                WizshBoneTwitchIntegration.Instance.StopCoroutine(m_testCoroutine);
            }

            Jotunn.Logger.LogWarning($"[TestWeather] Starting weather test: {weathers.Count} weathers, {seconds}s each. Run 'WBTITestWeather stop' to stop early.");
            m_testCoroutine = WizshBoneTwitchIntegration.Instance.StartCoroutine(TestLoop(weathers, Mathf.RoundToInt(seconds)));
        }

        private void StopTest()
        {
            if (m_testCoroutine == null)
            {
                Jotunn.Logger.LogWarning("No weather test is currently running.");
                return;
            }

            WizshBoneTwitchIntegration.Instance.StopCoroutine(m_testCoroutine);
            m_testCoroutine = null;
            Jotunn.Logger.LogWarning("[TestWeather] Weather test stopped.");
        }

        private IEnumerator TestLoop(List<string> weathers, int duration)
        {
            for (int i = 0; i < weathers.Count; i++)
            {
                if (Player.m_localPlayer == null)
                {
                    Jotunn.Logger.LogWarning("[TestWeather] Local player is gone, aborting weather test.");
                    break;
                }

                string weather = weathers[i];
                Jotunn.Logger.LogWarning($"[TestWeather] Weather ({i + 1}/{weathers.Count}): {weather}");

                // Announced here rather than via WeatherData.announceMessage - SpawnWeather only
                // uses that to format a redeemer name, which a test has none of.
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Weather ({i + 1}/{weathers.Count})\n{weather}");

                WeatherData data = new WeatherData
                {
                    announceMessage = null,
                    duration = duration,
                    force = true,
                };
                data.items.Add(weather);

                WeatherHelper.SpawnWeather(data, null);

                // No gap needed: if the next zone spawns in the same frame the old one expires, the old
                // zone's OnDestroy clears the forced env, but the new zone's EnvZone.OnTriggerStay
                // re-applies it on the next physics step.
                yield return new WaitForSeconds(duration);
            }

            Jotunn.Logger.LogWarning("[TestWeather] Weather test finished.");
            m_testCoroutine = null;
        }
    }
}
