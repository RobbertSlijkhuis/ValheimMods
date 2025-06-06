using Jotunn.Managers;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Models;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace ModularMagic_Armors.Components
{
    internal class ArmorMMA : MonoBehaviour
    {
        private ZNetView _netView;
        public string status;

        private void Awake()
        {
            _netView = base.gameObject.GetComponent<ZNetView>();
            status = "";

            InvokeRepeating(nameof(UpdateStatus), 0f, 0.3f);
        }

        private void UpdateStatus()
        {
            try
            {
                if (!_netView || !_netView.IsValid())
                {
                    CancelInvoke(nameof(UpdateStatus));
                    return;
                }

                if (!Player.m_localPlayer)
                    return;

                status = _netView.m_zdo.GetString(ModularMagic_Armors.armorStatusHashCode, "");

                if (status == "")
                    return;

                ArmorStatus current = StatusStringToObject(status);
                UpdateHelper.UpdateItemEffects(current.playerId, current.items);
                UpdateHelper.UpdateSetEffects(current.playerId, current.set);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Unexpected error occured in update of ArmorMMA: " + e);
            }
        }

        public ArmorStatus GetPlayerStatus()
        {
            if (status == "")
                return null;

            return StatusStringToObject(status);
        }

        public bool SetPlayerStatus(long playerId, ArmorStatus status)
        {
            if (!_netView || !_netView.IsValid() || !Player.m_localPlayer)
                return false;

            string newStatus = StatusObjectToString(status);

            _netView.m_zdo.Set(ModularMagic_Armors.armorStatusHashCode, newStatus);
            return true;
        }

        public int? GetArmorSetHashFromPlayer(long playerId)
        {
            Player player = Player.GetPlayer(playerId);
            return GetArmorSetHashFromPlayer(player);
        }

        public int? GetArmorSetHashFromPlayer(Player player)
        {
            if (player == null) 
                return null;

            if (player.GetSEMan().HaveStatusEffect(ModularMagic_Armors.ShamanArmorSetHashCode))
                return ModularMagic_Armors.ShamanArmorSetHashCode;

            if (player.GetSEMan().HaveStatusEffect(ModularMagic_Armors.WraithArmorSetHashCode))
                return ModularMagic_Armors.WraithArmorSetHashCode;

            if (player.GetSEMan().HaveStatusEffect(ModularMagic_Armors.FrostWolfArmorSetHashCode))
                return ModularMagic_Armors.FrostWolfArmorSetHashCode;

            if (player.GetSEMan().HaveStatusEffect(ModularMagic_Armors.DarkWizardArmorSetHashCode))
                return ModularMagic_Armors.DarkWizardArmorSetHashCode;

            if (player.GetSEMan().HaveStatusEffect(ModularMagic_Armors.EitrWeaveArmorSetHashCode))
                return ModularMagic_Armors.EitrWeaveArmorSetHashCode;

            if (player.GetSEMan().HaveStatusEffect(ModularMagic_Armors.EmblaArmorSetHashCode))
                return ModularMagic_Armors.EmblaArmorSetHashCode;

            return null;
        }

        public List<int> GetArmorHashesFromPlayer(long playerId)
        {
            Player player = Player.GetPlayer(playerId);
            return GetArmorHashesFromPlayer(player);
        }

        public List<int> GetArmorHashesFromPlayer(Player player)
        {
            if (player == null)
                return null;

            List<int> list = new List<int>();
            List<ItemDrop.ItemData> equiped = player.GetInventory().GetEquippedItems();

            if (equiped.Find(item => item.m_shared.m_name == PluginConfig.armor2Helmet.name.Value) != null)
                list.Add(ModularMagic_Armors.WraithHelmetHashCode);

            if (equiped.Find(item => item.m_shared.m_name == PluginConfig.armor2Chest.name.Value) != null)
               list.Add(ModularMagic_Armors.WraithChestHashCode);

            if (equiped.Find(item => item.m_shared.m_name == PluginConfig.armor2Legs.name.Value) != null)
               list.Add(ModularMagic_Armors.WraithLegsHashCode);

            if (equiped.Find(item => item.m_shared.m_name == PluginConfig.armor4Helmet.name.Value) != null)
               list.Add(ModularMagic_Armors.DarkWizardHelmetHashCode);

            if (equiped.Find(item => item.m_shared.m_name == PluginConfig.armor6Helmet.name.Value) != null)
               list.Add(ModularMagic_Armors.EmblaHelmetHashCode);

            if (equiped.Find(item => item.m_shared.m_name == PluginConfig.armor6Chest.name.Value) != null)
               list.Add(ModularMagic_Armors.EmblaChestHashCode);

            return list;
        }

        private ArmorStatus StatusStringToObject(string value)
        {
            List<int> itemList = new List<int>();
            string[] data = status.Split(':');

            if (data[2] != "null")
            {
                string[] items = data[2].Split(',');

                foreach (string item in items)
                {
                    itemList.Add(int.Parse(item));
                }
            }
            
            return new ArmorStatus(long.Parse(data[0]), data[1] == "null" ? null : int.Parse(data[1]), itemList);
        }

        private string StatusObjectToString(ArmorStatus status)
        {
            string items = "";

            if (status.items.Count != 0)
            {
                foreach (int item in status.items)
                {
                    items += item + ",";
                }

                if (items != "")
                    items = items.Remove(items.Length - 1);
            }

            return status.playerId + ":" + (status.set != null ? status.set : "null") + ":" + (items != "" ? items : "null");
        }
    }
}
