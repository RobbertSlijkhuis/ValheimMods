using Jotunn.Entities;
using System;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveTerrainEditCommand : ConsoleCommand
    {
        public override string Name => "WBTIRemoveTerrainEdit";

        public override string Help => "Undo nearby terrain edits made by Twitch integration (including permanent ones) and restore the original terrain";

        public override void Run(string[] args)
        {
            try
            {
                float scanRadius = 100f;

                if (args.Length > 0)
                {
                    scanRadius = float.Parse(args[0]);
                }

                if (Player.m_localPlayer == null)
                    throw new Exception("Player is null");

                // The terrain-edit marker has no Collider, so it's looked up in its own static
                // instance list by distance. Every terrain edit spawns one, permanent or timed.
                int removed = 0;

                foreach (TwitchTerrainReset marker in TwitchTerrainReset.Instances.ToList())
                {
                    if (marker == null)
                        continue;

                    if (Vector3.Distance(marker.transform.position, Player.m_localPlayer.transform.position) > scanRadius)
                        continue;

                    marker.RemoveNow();
                    removed++;
                }

                Jotunn.Logger.LogWarning($"[WBTI] Removed {removed} terrain edit(s)");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }
    }
}
