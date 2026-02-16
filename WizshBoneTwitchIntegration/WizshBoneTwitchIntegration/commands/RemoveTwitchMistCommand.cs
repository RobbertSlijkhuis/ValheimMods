using Jotunn.Entities;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveTwitchMistCommand : ConsoleCommand
    {
        public override string Name => "RemoveTwitchMist";

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

                Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, scanRadius);

                foreach (Collider obj in objects)
                {
                    GameObject gameObject = obj.transform.root.gameObject;
                    
                    if (gameObject.GetComponent<Mister>() && gameObject.GetComponent<TwitchPersistentDestruction>())
                    {
                        ZNetView netView = gameObject.GetComponent<ZNetView>();
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
