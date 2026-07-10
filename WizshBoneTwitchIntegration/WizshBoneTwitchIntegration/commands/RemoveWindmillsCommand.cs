using Jotunn.Entities;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class RemoveWindmillsCommand : ConsoleCommand
    {
        public override string Name => "WBTIRemoveWindmills";

        public override string Help => "Remove nearby windmills spawned in by Twitch integration";

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

                    if (gameObject.GetComponent<TwitchWindmillPersistentData>() != null && gameObject.GetComponent<TwitchPiecePersistentData>() != null)
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
