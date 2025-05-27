using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections;
using UnityEngine;


namespace ModularMagic_Utilities.Helpers
{
    internal class RPCHelper
    {
        public static CustomRPC Init() {
            return NetworkManager.Instance.AddRPC("RPC_Lantern_MMU", RPCHelper._OnServerReceive, RPCHelper._OnClientReceive);
        }

        public static IEnumerator _OnServerReceive(long sender, ZPackage package)
        {
            yield return new WaitForSeconds(0.1f);
            UpdateHelper.UpdateLanternMode(RPCHelper.ReadLanternPackage(package));
            ModularMagic_Utilities.Instance.lanternRPC.SendPackage(ZNet.instance.m_peers, new ZPackage(package.GetArray()));
        }

        public static IEnumerator _OnClientReceive(long sender, ZPackage package)
        {
            yield return new WaitForSeconds(0.1f);
            UpdateHelper.UpdateLanternMode(RPCHelper.ReadLanternPackage(package));
        }

        public static LanternPackageResult ReadLanternPackage(ZPackage package)
        {
            try
            {
                string[] data = package.ReadString().Split(',');
                long playerId = long.Parse(data[0]);
                int type = int.Parse(data[1]);
                bool value = bool.Parse(data[2]);
                bool applyLanternChanges = bool.Parse(data[3]);

                Jotunn.Logger.LogWarning("PlayerId: " + playerId);
                Jotunn.Logger.LogWarning("Type: " + type);
                Jotunn.Logger.LogWarning("Value: " + value);
                Jotunn.Logger.LogWarning("ApplyLanternChanges: " + applyLanternChanges);

                return new LanternPackageResult
                {
                    applyLanternChanges = applyLanternChanges,
                    playerId = playerId, 
                    type = type, 
                    value = value, 
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not read lantern package: " + e);
                return null;
            }
        }
    }
}
