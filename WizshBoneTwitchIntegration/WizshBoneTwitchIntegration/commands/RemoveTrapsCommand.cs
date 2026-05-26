using Jotunn.Entities;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

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

                Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, scanRadius, LayerMask.GetMask("piece"));

                foreach (Collider obj in objects)
                {
                    GameObject gameObject = obj.transform.root.gameObject;

                    if (gameObject.GetComponent<Trap>() != null && gameObject.GetComponent<TwitchPersistentDestruction>() != null)
                    {
                        ZNetView netView = gameObject.GetComponent<ZNetView>();

                        if (netView == null || !netView.IsValid())
                            return;

                        netView.Destroy();
                        GameObject.Destroy(gameObject);
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
