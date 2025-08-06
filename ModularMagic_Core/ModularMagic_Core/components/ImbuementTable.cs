using ModularMagic_Core.components;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static ItemDrop;

namespace ModularMagic_Core.Components
{
    internal class ImbuementTable : MonoBehaviour
    {
        private ZNetView netView;
        public List<Imbuement> m_imbuements = new List<Imbuement>();
        public List<Imbuement> m_imbued = new List<Imbuement>();
        public ItemData m_itemData;
        public string m_imbuementMaterial = "$item_resin";

        public UnityEvent m_onItemAttach = new UnityEvent();
        public UnityEvent m_onItemRemove = new UnityEvent();
        public UnityEvent m_onRuneActivation = new UnityEvent();

        private void Awake ()
        {
            netView = transform.Find("itemstand").gameObject.GetComponent<ItemStand>().m_netViewOverride;
        }

        public void StaffAttach(string imbuementsString, ItemData itemData)
        {
            m_imbuements = ImbuementHelper.StringToList(imbuementsString);
            m_itemData = itemData;
            m_imbued = m_imbuements.FindAll(item => item.isImbued);
            m_onItemAttach.Invoke();
            CreateRunes();
        }

        public void StaffRemove()
        {
            m_imbuements = null;
            m_itemData = null;
            m_imbued = null;
            m_onItemRemove.Invoke();
            RemoveRunes();
        }

        public void CreateRunes()
        {
            if (m_imbuements == null || m_imbuements.Count == 0)
                return;

            Transform defaultRuneTransform = transform.Find("default_rune");
            Transform runesTransform = transform.Find("runes");

            if (defaultRuneTransform == null || defaultRuneTransform.gameObject == null)
                throw new Exception("Could not find default rune GameObject");

            if (runesTransform == null || runesTransform.gameObject == null)
                throw new Exception("Could not find runes parent GameObject ");

            int index = 0;

            foreach (var imbuement in m_imbuements)
            {
                GameObject rune = UnityEngine.Object.Instantiate(defaultRuneTransform.gameObject, defaultRuneTransform);
                rune.transform.SetParent(runesTransform);

                RuneMaterials runeMats = ImbuementHelper.GetRuneMaterialByInteger(index);
                ImbuementRune runeComp = rune.AddComponent<ImbuementRune>();
                runeComp.m_imbuement = imbuement;
                runeComp.m_runeMaterials = runeMats;
                runeComp.Init();

                index++;

                if (index > 5)
                    index = 0;
            }
        }

        public void RemoveRunes()
        {
            Transform runesTransform = transform.Find("runes");

            if (runesTransform == null || runesTransform.gameObject == null)
                throw new Exception("Could not find runes parent GameObject ");

            foreach (Transform child in runesTransform)
            {
                //GameObject.Destroy(child.gameObject);
                ImbuementRune runeComp = child.gameObject.GetComponent<ImbuementRune>();
                runeComp.Destroy();
            }
        }

        public string CanImbue()
        {
            if (Player.m_localPlayer == null)
                return CanImbueType.Yes;

            Inventory inventory = Player.m_localPlayer.GetInventory();
            int materialInInventory = inventory.CountItems(m_imbuementMaterial);
            int totalMaterialRequired = CountMaterialRequired();

            //Jotunn.Logger.LogWarning("=== CanImbue ===================================");
            //Jotunn.Logger.LogWarning("materialInInventory: " + materialInInventory);

            if (materialInInventory == 0)
                return CanImbueType.No;

            Jotunn.Logger.LogWarning("Change?: " + m_imbued.All(item => item.enabled));

            if (totalMaterialRequired == 0 && m_imbued.All(item => item.enabled))
                return CanImbueType.NoChange;

            Jotunn.Logger.LogWarning("CanImbue: " + (materialInInventory >= totalMaterialRequired));

            if (materialInInventory >= totalMaterialRequired)
                return CanImbueType.Yes;

            return CanImbueType.Yes;
        }

        public int CountMaterialRequired()
        {
            int totalMaterialRequired = 0;

            foreach (Imbuement imbuement in m_imbuements.FindAll(item => !item.isImbued))
            {
                if (!imbuement.enabled)
                    continue;

                totalMaterialRequired += imbuement.materialRequired;
            }

            return totalMaterialRequired;
        }

        public bool Save()
        {
            try
            {
                string canImbue = CanImbue();

                if (canImbue == CanImbueType.No || canImbue == CanImbueType.NoChange)
                    return false;

                string imbuementsString = ImbuementHelper.ListToString(m_imbuements);
                Jotunn.Logger.LogWarning("=== Save ===================================");
                m_itemData.m_customData[ModularMagic_Core.imbuementMMESDataKey] = imbuementsString;
                SaveToZDO(m_itemData, netView.GetZDO());
                Game.instance.GetPlayerProfile().SavePlayerData(Player.m_localPlayer);
                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not save imbuements to item: " + e);
                return false;
            }
        }
    }
}
