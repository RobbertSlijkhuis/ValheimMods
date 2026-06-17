using System;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Harmony;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class TimeStopHelper
    {
        public const string RPC_Name = "WBTI_TimeStop";

        public static void RegisterRPCs()
        {
            if (ZRoutedRpc.instance == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] TimeStopHelper: ZRoutedRpc.instance is null, cannot register RPCs.");
                return;
            }

            ZRoutedRpc.instance.Register<ZPackage>(RPC_Name, RPC_OnTimeStop);
        }

        public static void Apply(TimeStopData data, CustomRewardEvent rewardEvent)
        {
            if (data == null || Player.m_localPlayer == null)
                return;

            var pkg = new ZPackage();
            pkg.Write(data.duration);
            pkg.Write(data.freezePlayer);

            var zdoids = new List<ZDOID>();

            if (data.freezeEnemies)
            {
                Vector3 playerPos = Player.m_localPlayer.transform.position;

                foreach (Character character in Character.GetAllCharacters())
                {
                    if (character.IsPlayer())
                        continue;

                    if (data.radius > 0f && Vector3.Distance(playerPos, character.transform.position) > data.radius)
                        continue;

                    ZNetView netView = character.GetComponent<ZNetView>();
                    if (netView == null || !netView.IsValid())
                        continue;

                    zdoids.Add(netView.GetZDO().m_uid);
                }
            }

            pkg.Write(zdoids.Count);
            foreach (ZDOID id in zdoids)
                pkg.Write(id);

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RPC_Name, pkg);

            if (!string.IsNullOrEmpty(data.announceMessage))
                Player.m_localPlayer.Message(
                    MessageHud.MessageType.Center,
                    MessageHelper.ParseVariables("{{user}}", rewardEvent.RedeemerName, data.announceMessage));
        }

        public static void RPC_OnTimeStop(long sender, ZPackage pkg)
        {
            try
            {
                float duration    = pkg.ReadSingle();
                bool freezePlayer = pkg.ReadBool();
                int count         = pkg.ReadInt();

                for (int i = 0; i < count; i++)
                {
                    ZDOID zdoid = pkg.ReadZDOID();

                    GameObject go = ZNetScene.instance.FindInstance(zdoid);
                    if (go == null)
                        continue;

                    if (go.GetComponent<TwitchFreezeData>() != null)
                        continue;

                    TwitchFreezeData freeze = go.AddComponent<TwitchFreezeData>();
                    freeze.Initialize(duration);
                }

                if (freezePlayer)
                    TimeStopPatchesWBTI.FreezePlayer(duration);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TimeStopHelper.RPC_OnTimeStop failed: " + e);
            }
        }
    }
}
