using AzuExtendedPlayerInventory;
using BepInEx.Configuration;
using ModularMagic_Utilities.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModularMagic_Utilities.Helpers
{
    internal class CustomSlot
    {
        public static readonly Dictionary<VisEquipment, CustomSlot> _magicSlots = new Dictionary<VisEquipment, CustomSlot>();
        public ItemDrop.ItemData _equippedTomeItem;
        private string _tomeItem;
        private List<GameObject> _tomeItemInstances;
        private int _currentTomeItemHash;
        public VisEquipment visEquipment;

        public CustomSlot(VisEquipment visEquipment)
        {
            // ISSUE: reference to a compiler-generated field
            this.visEquipment = visEquipment;
            this._tomeItem = "";
            this._tomeItemInstances = new List<GameObject>();
        }

        public static bool IsTomeItem(ItemDrop.ItemData item)
        {
            if (ConfigUtilities.configEnableSlot.Value == false)
                return false;

            return Utils.CustomEndsWith(item.m_shared.m_name, "Lantern");
        }

        private int GetHash(string zdoKey, string equippedItem)
        {
            ZDO zdo = this.visEquipment.m_nview.GetZDO();
            if (zdo != null)
                return zdo.GetInt(zdoKey, 0);
            return !string.IsNullOrEmpty(equippedItem) ? StringExtensionMethods.GetStableHashCode(equippedItem) : 0;
        }

        public void UpdateEquipmentVisuals()
        {
            if (!this.SetTomeEquipped(this.GetHash("LanternItem", this._tomeItem)))
                return;
            
            this.visEquipment.UpdateLodgroup();
        }

        private bool SetTomeEquipped(int hash)
        {
            if (this._currentTomeItemHash == hash)
                return false;
            foreach (UnityEngine.Object tomeItemInstance in this._tomeItemInstances) {
                UnityEngine.Object.Destroy(tomeItemInstance);
            }

            this._tomeItemInstances.Clear();
            this._currentTomeItemHash = hash;
            if (hash != 0)
            {
                this._tomeItemInstances = this.visEquipment.AttachArmor(hash, -1);
            }
            return true;
        }

        public void SetTomeItem(string name)
        {
            if (this._tomeItem == name)
                return;
            this._tomeItem = name;
            ZDO zdo = this.visEquipment.m_nview.GetZDO();
            if (zdo == null || !this.visEquipment.m_nview.IsOwner())
        return;
            zdo.Set("LanternItem", !string.IsNullOrEmpty(name) ? StringExtensionMethods.GetStableHashCode(name) : 0);
        }

        public static void AddCustomSlot(ConfigEntry<bool> configEntry)
        {
            if (!API.IsLoaded())
                return;
            if (configEntry == ConfigUtilities.configEnableSlot)
            {
                AddSlot("Lantern", CustomSlot.IsTomeItem, (Func<CustomSlot, ItemDrop.ItemData>)(slot => slot._equippedTomeItem));
            }

            void AddSlot(
              string name,
              Func<ItemDrop.ItemData, bool> isValid,
              Func<CustomSlot, ItemDrop.ItemData> get)
            {
                if (configEntry.Value == true)
                {
                    int index = ((IEnumerable<string>)API.GetSlots().SlotNames).ToList<string>().FindIndex((Predicate<string>)(str => str == "Lantern"));
                    CustomSlot magicSlot;
                    API.AddSlot(name, (Func<Player, ItemDrop.ItemData>)(player => CustomSlot._magicSlots.TryGetValue(((Humanoid)player).m_visEquipment, out magicSlot) ? get(magicSlot) : (ItemDrop.ItemData)null), isValid, index < 0 ? -1 : (name == "Lantern") ? index : index + 1);
                }
                else
                    API.RemoveSlot(name);
            }
        }
    }
}
