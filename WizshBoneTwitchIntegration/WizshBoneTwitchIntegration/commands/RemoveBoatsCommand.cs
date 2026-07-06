using Jotunn.Entities;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveBoatsCommand : ConsoleCommand
    {
        public override string Name => "WBTIRemoveBoats";

        public override string Help => "Remove nearby boats spawned in by Twitch integration";

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

                // Boats aren't Pieces, so they aren't confined to the "piece" layer like doors/windmills -
                // scan every layer. TwitchPersistentDamage is only attached when the redeem's spawnAbilityData
                // sets a damage block, which isn't guaranteed for every boat redeem, so it can't be used to
                // identify these - use the always-set TwitchBasePersistentData.IsRedeemSpawn tag instead,
                // narrowed down by Ship to make sure it's a boat and not some other redeem-spawned object.
                Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, scanRadius, ~0);

                foreach (Collider obj in objects)
                {
                    GameObject gameObject = obj.transform.root.gameObject;

                    if (gameObject.GetComponent<Ship>() != null && gameObject.GetComponent<TwitchBasePersistentData>()?.IsRedeemSpawn == true)
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
