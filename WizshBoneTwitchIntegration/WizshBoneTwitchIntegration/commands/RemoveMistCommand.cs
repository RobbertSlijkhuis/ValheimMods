using Jotunn.Entities;
using System;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveMistCommand : ConsoleCommand
    {
        public override string Name => "WBTIRemoveMist";

        public override string Help => "Remove nearby mist spawned in by Twitch integration";

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

                // Mister has no Collider, so unlike the other Remove*Command targets (chests, doors,
                // creatures, etc.) it can't be found via Physics.OverlapSphere - it's tracked in its
                // own static instance list and looked up by distance instead. TwitchPersistentDestruction
                // is only attached when the redeem's duration > 0 (0 = indefinite/never auto-cleanup),
                // so it can't be used to identify these - use the always-set
                // TwitchBasePersistentData.IsRedeemSpawn tag instead.
                foreach (Mister mister in Mister.GetMisters().ToList())
                {
                    GameObject gameObject = mister.gameObject;

                    if (Vector3.Distance(gameObject.transform.position, Player.m_localPlayer.transform.position) > scanRadius)
                        continue;

                    if (gameObject.GetComponent<TwitchBasePersistentData>()?.IsRedeemSpawn == true)
                    {
                        ZNetViewHelper.Destroy(gameObject);
                    }
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }
    }
}
