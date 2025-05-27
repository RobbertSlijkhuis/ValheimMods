using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Utilities.Components
{
    internal class LanternMMU : MonoBehaviour
    {
        private ZNetView _netView;
        public string status;

        private void Awake()
        {
            _netView = base.gameObject.GetComponent<ZNetView>();
            status = "";

            InvokeRepeating(nameof(_UpdateStatus), 0f, 1f);
        }

        private void _UpdateStatus()
        {
            try
            {
                if (!_netView || !_netView.IsValid() || (!PluginConfig.lantern1.enable.Value && !PluginConfig.lantern2.enable.Value && !PluginConfig.lantern3.enable.Value))
                {
                    CancelInvoke(nameof(_UpdateStatus));
                    return;
                }

                if (!Player.m_localPlayer)
                {
                    return;
                }

                status = _netView.m_zdo.GetString(ModularMagic_Utilities.lanternStatusHashCode, "");

                if (!ContainsPlayer(Player.m_localPlayer.GetPlayerID()))
                    SetPlayerStatus(Player.m_localPlayer.GetPlayerID(), true);

                if (status == "")
                    return;

                List<LanternStatus> list = _StatusStringToList(status);

                foreach (LanternStatus item in list)
                {
                    //Jotunn.Logger.LogWarning($"Updating {item.playerId} - {item.status}");
                    UpdateHelper.UpdateLanternMode(item.playerId, item.status);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Unexpected error occured in update of LanternMMU: "+ e);
            }
        }

        public LanternStatus GetPlayerStatus(long playerId)
        {
            List<LanternStatus> list = _StatusStringToList(status);
            return list.Find(item => item.playerId == playerId);
        }

        public bool SetPlayerStatus(long playerId, bool value)
        {
            if (!_netView || !_netView.IsValid() || !Player.m_localPlayer)
                return false;

            bool hasPlayer = ContainsPlayer(playerId);
            string newStatus = "";

            if (hasPlayer)
            {
                List<LanternStatus> list = _StatusStringToList(status);
                LanternStatus entry = list.Find(item => item.playerId == playerId);
                entry.status = value;
                newStatus = _StatusListToString(list);
            }
            else
            {
                string currentStatus = status == "" ? "" : status + ",";
                newStatus = $"{currentStatus}{playerId}:{value}";
            }

            if (newStatus == "")
            {
                Jotunn.Logger.LogError("New status is empty!");
                return false;
            }
            
            _netView.m_zdo.Set(ModularMagic_Utilities.lanternStatusHashCode, newStatus);
            return true;
        }

        public bool ContainsPlayer(long playerId)
        {
            return status.Contains(playerId.ToString());
        }

        private List<LanternStatus> _StatusStringToList(string status)
        {
            List<LanternStatus> list = new List<LanternStatus>();
            string[] result = status.Split(',');

            foreach (string item in result)
            {
                string[] keyValuePair = item.Split(':');
                list.Add(new LanternStatus(long.Parse(keyValuePair[0]), bool.Parse(keyValuePair[1])));
            }

            return list;
        }

        private string _StatusListToString(List<LanternStatus> status)
        {
            string result = "";

            foreach (LanternStatus item in status)
            {
                result += $"{item.playerId}:{item.status}";
            }

            return result;
        }
    }
}
