using Jotunn.Entities;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveLogsCommand : ConsoleCommand
    {
        public override string Name => "WBTIRemoveLogs";

        public override string Help => "Remove nearby logs spawned in by Twitch integration";

        public override void Run(string[] args)
        {
            try
            {
                float scanRadius = 30f;

                if (args.Length > 0)
                {
                    scanRadius = float.Parse(args[0]);
                }

                if (Player.m_localPlayer == null)
                    throw new Exception("Player is null");

                // Logs aren't Pieces, so they aren't confined to the "piece" layer - scan every layer.
                // They fall through the ImpactEffect branch of SpawnAbilityExtension (falling debris that
                // deals impact damage). TwitchPersistentDestruction is only attached there when the redeem's
                // duration > 0 (0 = indefinite/never auto-cleanup), so it can't be used to identify these -
                // use the always-set TwitchBasePersistentData.IsRedeemSpawn tag instead, narrowed down by
                // ImpactEffect to make sure it's debris and not some other redeem-spawned object.
                Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, scanRadius, ~0);

                foreach (Collider obj in objects)
                {
                    GameObject gameObject = obj.transform.root.gameObject;

                    if (gameObject.GetComponentInChildren<ImpactEffect>(true) != null
                        && gameObject.GetComponent<TwitchBasePersistentData>()?.IsRedeemSpawn == true)
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
