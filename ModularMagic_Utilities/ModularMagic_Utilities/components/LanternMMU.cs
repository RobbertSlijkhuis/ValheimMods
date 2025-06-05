using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
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

            InvokeRepeating(nameof(_UpdateStatus), 0f, 0.3f);
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
                    return;

                status = _netView.m_zdo.GetString(ModularMagic_Utilities.lanternStatusHashCode, "");

                if (status == "")
                    return;

                LanternStatus current = _statusStringToObject(status);
                LanternHelper.UpdateLanternMode(current.playerId, current.status);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Unexpected error occured in update of LanternMMU: "+ e);
            }
        }

        public LanternStatus GetPlayerStatus()
        {
            if (status == "")
                return null;

            return _statusStringToObject(status);
        }

        public bool SetPlayerStatus(long playerId, bool value)
        {
            if (!_netView || !_netView.IsValid() || !Player.m_localPlayer)
                return false;

            string newStatus = _statusObjectToString(new LanternStatus(playerId, value));
            
            _netView.m_zdo.Set(ModularMagic_Utilities.lanternStatusHashCode, newStatus);
            return true;
        }

        private LanternStatus _statusStringToObject(string value)
        {
            string[] data = status.Split(':');
            return new LanternStatus(long.Parse(data[0]), data[1].ToLower() == "true" ? true : false);
        }

        private string _statusObjectToString(LanternStatus item)
        {
            return item.playerId + ":" + item.status;
        }
    }
}
