using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
using UnityEngine;

namespace ModularMagic_Utilities.Components
{
    internal class LanternMMU : MonoBehaviour
    {
        private ZNetView netView;
        public string status;

        private void Awake()
        {
            netView = base.gameObject.GetComponent<ZNetView>();
            status = "";

            InvokeRepeating(nameof(UpdateStatus), 0f, 10.3f);
        }

        private void UpdateStatus()
        {
            try
            {
                if (!netView || !netView.IsValid())
                {
                    CancelInvoke(nameof(UpdateStatus));
                    return;
                }

                if (!Player.m_localPlayer)
                    return;

                status = netView.m_zdo.GetString(ModularMagic_Utilities.lanternStatusHash, "");

                if (status == "")
                    return;

                // Jotunn.Logger.LogWarning(status);

                LanternStatus current = StatusStringToObject(status);
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

            return StatusStringToObject(status);
        }

        public bool SetPlayerStatus(long playerId, bool value)
        {
            if (!netView || !netView.IsValid() || !Player.m_localPlayer)
                return false;

            string newStatus = StatusObjectToString(new LanternStatus(playerId, value));
            
            netView.m_zdo.Set(ModularMagic_Utilities.lanternStatusHash, newStatus);
            return true;
        }

        private LanternStatus StatusStringToObject(string value)
        {
            string[] data = status.Split(':');
            return new LanternStatus(long.Parse(data[0]), data[1].ToLower() == "true" ? true : false);
        }

        private string StatusObjectToString(LanternStatus item)
        {
            return item.playerId + ":" + item.status;
        }
    }
}
