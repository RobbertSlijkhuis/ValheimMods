using PlayAsSkeleton.Configs;
using PlayAsSkeleton.GUI;
using PlayAsSkeleton.Helpers;
using PlayAsSkeleton.models;
using PlayAsSkeleton.Models;
using PlayAsSkeleton.Types;
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayAsSkeleton.Components
{
    internal class SkeletonPAS : MonoBehaviour
    {
        private ZNetView netView;
        private long playerID;
        private bool isInitialised;
        private bool isSkeleton;
        private string skin;
        private bool canSwim;

        private bool prevIsSkeleton;
        private string prevSkin;
        private bool prevCanSwim;

        public SettingsGUI settingsGUI;

        private void Awake()
        {
            netView = base.gameObject.GetComponent<ZNetView>();
            
            if (netView != null && netView.GetZDO() != null)
            {

                playerID = netView.GetZDO().GetLong(PlayAsSkeleton.playerIDHash, -1);
                isSkeleton = netView.GetZDO().GetBool(PlayAsSkeleton.isSkeletonHash, false);
                skin = netView.GetZDO().GetString(PlayAsSkeleton.skinHash, SkinType.Normal);
                canSwim = true;
                isInitialised = false;

                // GUI
                settingsGUI = new SettingsGUI(new UpdateSettingsOptions()
                {
                    isSkeleton = isSkeleton,
                    skin = skin,
                    canSwim = canSwim,
                });
                settingsGUI.onAccept.AddListener(OnAcceptSettings);

                InvokeRepeating(nameof(UpdateStatus), 0f, 2f);
            }
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

                playerID = netView.GetZDO().GetLong(PlayAsSkeleton.playerIDHash);
                isSkeleton = netView.GetZDO().GetBool(PlayAsSkeleton.isSkeletonHash);
                skin = netView.GetZDO().GetString(PlayAsSkeleton.skinHash);
                Player player = Player.GetPlayer(playerID);

                if (!isInitialised && isSkeleton)
                {
                    SkeletonHelper.Apply(player, this);
                    isInitialised = true;
                    prevIsSkeleton = isSkeleton;
                    return;
                }

                if (prevIsSkeleton != isSkeleton)
                {
                    if (isSkeleton)
                        SkeletonHelper.Apply(player, this);
                    else
                        SkeletonHelper.Reset(player);

                    prevIsSkeleton = isSkeleton;
                }
                else if (prevSkin != skin)
                {
                    SkeletonHelper.UpdateSkin(player, skin);
                    prevSkin = skin;
                }

                if (prevCanSwim != canSwim)
                {
                    SkeletonHelper.SetCanSwim(player, canSwim);
                    prevCanSwim = canSwim;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Unexpected error occured in update of SkeletonPAS: " + e);
            }
        }

        public void UpdateCurrentSettingValues()
        {
            settingsGUI.SetCurrentValues(new UpdateSettingsOptions()
            {
                isSkeleton = isSkeleton,
                skin = skin,
                canSwim = canSwim,
            });
        }

        private void OnAcceptSettings(UpdateSettingsOptions values)
        {
            settingsGUI.SetCurrentValues(values);
            SetIsSkeleton(values.isSkeleton, false);
            SetSkin(values.skin, false);
            SetCanSwim(values.canSwim, false);
            SaveAsCustomData();
        } 

        private void SaveAsCustomData()
        {
            Player player = Player.GetPlayer(PlayAsSkeleton.playerID);
            string data = PlayAsSkeleton.playerID + "," + isSkeleton + "," + skin + "," + canSwim;
            player.m_customData[PlayAsSkeleton.playerDataKey] =  data;
            Game.instance.GetPlayerProfile().SavePlayerData(player);
            Jotunn.Logger.LogWarning("CustomData is now: " + player.m_customData[PlayAsSkeleton.playerDataKey]);
        }

        public bool GetIsInitialised()
        {
            return isInitialised;
        }

        public void SetPlayerID(long id, bool saveAsCustomData = true)
        {
            netView.GetZDO().Set(PlayAsSkeleton.playerIDHash, id);
            playerID = id;
        }

        public long GetPlayerID()
        {
            return playerID;
        }

        public void SetIsSkeleton(bool value, bool saveAsCustomData = true)
        {
            netView.GetZDO().Set(PlayAsSkeleton.isSkeletonHash, value);
            isSkeleton = value;

            if (saveAsCustomData)
                SaveAsCustomData();
        }

        public bool GetIsSkeleton()
        {
            return isSkeleton;
        }

        public void SetSkin(string value, bool saveAsCustomData = true)
        {
            netView.GetZDO().Set(PlayAsSkeleton.skinHash, value);
            skin = value;

            if (saveAsCustomData)
                SaveAsCustomData();
        }

        public string GetSkin()
        {
            return skin;
        }

        public void SetCanSwim(bool value, bool saveAsCustomData = true)
        {
            canSwim = value;

            if (saveAsCustomData)
                SaveAsCustomData();
        }

        public bool GetCanSwim()
        {
            return canSwim;
        }
    }
}
