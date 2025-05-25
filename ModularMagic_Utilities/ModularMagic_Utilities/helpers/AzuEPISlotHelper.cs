using AzuExtendedPlayerInventory;
using ModularMagic_Utilities.Configs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModularMagic_Utilities.Helpers
{
    internal class AzuEPISlotHelper
    {
        public static readonly Dictionary<VisEquipment, AzuEPISlotHelper> slots = new Dictionary<VisEquipment, AzuEPISlotHelper>();
        private static string _slotName = "Book/Lantern";
        public ItemDrop.ItemData equipedItem;
        private List<GameObject> _instances;
        private string _itemName;
        private int _itemHash;
        private VisEquipment visEquipment;
        private string _zdoName = "Item_MMU";

        public AzuEPISlotHelper(VisEquipment visEquipment)
        {
            this.visEquipment = visEquipment;
            this._instances = new List<GameObject>();
        }

        public static void AddSlot()
        {
            if (!API.IsLoaded())
                return;

            if (ConfigUtilities.configEnableSlot.Value)
            {
                bool success = API.AddSlot(_slotName, GetUtilityItem, IsUtilityItem);
                Jotunn.Logger.LogWarning("Custom slot is added: " + success);
            }
        }

        public static void RemoveSlot()
        {
            if (!API.IsLoaded())
                return;

            if (!ConfigUtilities.configEnableSlot.Value)
            {
                bool success = API.RemoveSlot(_slotName);
                Jotunn.Logger.LogWarning("Custom slot is removed: " + success);
            }
        }

        private bool _SetItemEquipped(int hash)
        {
            if (this._itemHash == hash)
                return false;

            foreach (Object tomeItemInstance in this._instances) 
            {
                Object.Destroy(tomeItemInstance);
            }

            this._instances.Clear();
            this._itemHash = hash;

            if (hash != 0)
            {
                this._instances = this.visEquipment.AttachArmor(hash, -1);
            }

            return true;
        }

        public void SetItemName(string name)
        {
            if (this._itemName == name)
                return;

            this._itemName = name;
            ZDO zdo = this.visEquipment.m_nview.GetZDO();
            
            if (zdo == null || !this.visEquipment.m_nview.IsOwner())
                return;

            zdo.Set(_zdoName, !string.IsNullOrEmpty(name) ? StringExtensionMethods.GetStableHashCode(name) : 0);
        }

        public void UpdateEquipmentVisuals()
        {
            if (!this._SetItemEquipped(this._GetHash(_zdoName, this._itemName)))
                return;

            this.visEquipment.UpdateLodgroup();
        }

        private int _GetHash(string zdoKey, string equippedItem)
        { 
            ZDO zdo = this.visEquipment.m_nview.GetZDO();

            if (zdo != null)
                return zdo.GetInt(zdoKey, 0);

            return !string.IsNullOrEmpty(equippedItem) ? StringExtensionMethods.GetStableHashCode(equippedItem) : 0;
        }

        private static ItemDrop.ItemData GetUtilityItem(Humanoid player)
        {
            ItemDrop.ItemData utilitySlot = player.GetInventory().GetEquippedItems().FirstOrDefault(
                item => item != null &&
                item.m_dropPrefab && (
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.spellbook1Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.spellbook2Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.spellbook3Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.lantern1Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.lantern2Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.lantern3Prefab.name));

            //if (utilitySlot != null)
            //    Jotunn.Logger.LogWarning("UtilitySlot: " + utilitySlot.m_shared.m_name);

            return utilitySlot;
        }

        private static bool IsUtilityItem(ItemDrop.ItemData item)
        {
            bool isItem = item != null && item.m_dropPrefab && (
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.spellbook1Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.spellbook2Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.spellbook3Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.lantern1Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.lantern2Prefab.name ||
                item.m_dropPrefab.name == ModularMagic_Utilities.Instance.prefabs.lantern3Prefab.name);

            // Jotunn.Logger.LogWarning("IsUtilityItem: " + isItem);
            return isItem;
        }
    }
}
