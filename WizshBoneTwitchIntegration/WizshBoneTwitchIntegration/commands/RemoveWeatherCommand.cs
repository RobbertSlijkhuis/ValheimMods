using Jotunn.Entities;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveWeatherCommand : ConsoleCommand
    {
        public override string Name => "WBTIRemoveWeather";

        public override string Help => "Remove weather zones spawned in by Twitch integration and restore the normal weather";

        public override void Run(string[] args)
        {
            try
            {
                int removed = 0;

                // The weather zone is a trigger volume on an unknown layer, so it's looked up by
                // component instead of an overlap query. TwitchPersistentDestruction is added to the
                // EnvZone_WBTI prefab in code and vanilla EnvZones don't have it, so it identifies
                // the Twitch-spawned ones without depending on the redeem's duration or config.
                foreach (EnvZone envZone in UnityEngine.Object.FindObjectsByType<EnvZone>(FindObjectsSortMode.None))
                {
                    if (envZone.GetComponent<TwitchPersistentDestruction>() == null)
                        continue;

                    ZNetViewHelper.Destroy(envZone.gameObject);
                    removed++;
                }

                // Destroying a zone doesn't fire OnTriggerExit, so a forced weather would otherwise
                // stay on until the player walks somewhere else.
                if (EnvMan.instance != null)
                    EnvMan.instance.SetForceEnvironment("");

                Jotunn.Logger.LogWarning($"[WBTI] Removed {removed} weather zone(s)");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }
    }
}
