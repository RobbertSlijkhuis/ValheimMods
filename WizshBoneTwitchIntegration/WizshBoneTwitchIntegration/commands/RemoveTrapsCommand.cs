using Jotunn.Entities;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveTrapsCommand : ConsoleCommand
    {
        public override string Name => "WBTIRemoveTraps";

        public override string Help => "Remove nearby traps spawned in by Twitch integration";

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

                // TwitchPersistentDestruction is only attached when the redeem's duration > 0
                // (0 = indefinite/never auto-cleanup), so it can't be used to identify these -
                // use the always-set TwitchBasePersistentData.IsRedeemSpawn tag instead, narrowed
                // down by Trap to make sure it's a trap and not some other redeem-spawned object.
                Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, scanRadius, LayerMask.GetMask("piece"));

                foreach (Collider obj in objects)
                {
                    GameObject gameObject = obj.transform.root.gameObject;

                    if (gameObject.GetComponent<Trap>() != null && gameObject.GetComponent<TwitchBasePersistentData>()?.IsRedeemSpawn == true)
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
