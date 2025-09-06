using ModularMagic_Core.components;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;
using System.Collections.Generic;
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
        public string m_imbuementsString;
        public string m_imbuementMaterial = "$item_resin";

        public UnityEvent m_onItemAttach = new UnityEvent();
        public UnityEvent m_onItemRemove = new UnityEvent();
        public UnityEvent m_onRuneActivation = new UnityEvent();
        public UnityEvent m_onSave = new UnityEvent();

        private Transform m_spellbookTransform;
        private Vector3 m_spellbookStartIdle;
        private Vector3 m_spellbookEndIdle;
        private float m_spellbookDurationIdle = 10f;
        private bool m_spellbookReverseIdle = false;

        private Transform m_staffTransform;
        private Vector3 m_staffStartIdle;
        private Vector3 m_staffEndIdle;
        private float m_staffDurationIdle = 10f;
        private bool m_staffReverseIdle = false;

        private void Awake()
        {
            netView = transform.Find("itemstand").gameObject.GetComponent<ItemStand>().m_netViewOverride;

            m_spellbookTransform = transform.Find("controls/accept_book");
            m_spellbookStartIdle = new Vector3(m_spellbookTransform.localPosition.x, m_spellbookTransform.localPosition.y, m_spellbookTransform.localPosition.z);
            m_spellbookEndIdle = new Vector3(m_spellbookTransform.localPosition.x, m_spellbookTransform.localPosition.y + 0.06f, m_spellbookTransform.localPosition.z);

            m_staffTransform = transform.Find("itemstand/attach_other");
            m_staffStartIdle = new Vector3(m_staffTransform.localPosition.x, m_staffTransform.localPosition.y, m_staffTransform.localPosition.z);
            m_staffEndIdle = new Vector3(m_staffTransform.localPosition.x, m_staffTransform.localPosition.y + 0.06f, m_staffTransform.localPosition.z);

            InvokeRepeating(nameof(StartSpellbookIdle), 0f, m_spellbookDurationIdle + 0.1f);
        }

        public void StaffAttach(string imbuementsString, ItemData itemData)
        {
            
            m_imbuements = ImbuementHelper.StringToList(imbuementsString);
            m_imbued = m_imbuements.FindAll(item => item.isImbued);
            m_itemData = itemData;
            m_imbuementsString = imbuementsString;
            m_onItemAttach.Invoke();
            CreateRunes();
            InvokeRepeating(nameof(StartStaffIdle), 0f, m_staffDurationIdle + 0.1f);
        }

        public void StaffRemove()
        {
            CancelInvoke(nameof(StartStaffIdle));

            m_imbuements = new List<Imbuement>();
            m_imbued = new List<Imbuement>();
            m_itemData = null;
            m_imbuementsString = null;
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
            if (Player.m_localPlayer == null || m_imbuements == null)
                return CanImbueType.No;

            //Inventory inventory = Player.m_localPlayer.GetInventory();
            //int materialInInventory = inventory.CountItems(m_imbuementMaterial);
            //int totalMaterialRequired = CountMaterialRequired();

            //if (materialInInventory == 0)
            //    return CanImbueType.No;

            //if (totalMaterialRequired == 0 && m_imbued.All(item => item.enabled))
            //    return CanImbueType.NoChange;

            //if (materialInInventory >= totalMaterialRequired)
            //    return CanImbueType.Yes;

            string currentImbuementsString = ImbuementHelper.ListToString(m_imbuements);

            Jotunn.Logger.LogWarning("CanImbue: " + m_imbuementsString == currentImbuementsString);
            if (m_imbuementsString == currentImbuementsString)
                return CanImbueType.NoChange;

            return CanImbueType.Yes;
        }

        //public int CountMaterialRequired()
        //{
        //    if (m_imbuements == null)
        //        return 0;

        //    int totalMaterialRequired = 0;

        //    foreach (Imbuement imbuement in m_imbuements.FindAll(item => !item.isImbued))
        //    {
        //        if (!imbuement.enabled)
        //            continue;

        //        totalMaterialRequired += imbuement.materialRequired;
        //    }

        //    return totalMaterialRequired;
        //}

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

                foreach (Imbuement imbuement in m_imbuements.FindAll(item => item.enabled))
                {
                    imbuement.isImbued = true;
                }

                m_imbuementsString = imbuementsString;
                m_imbued = m_imbuements.FindAll(item => item.isImbued);
                m_onSave.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not save imbuements to item: " + e);
                return false;
            }
        }

        public void StartSpellbookIdle()
        {
            m_spellbookReverseIdle = !m_spellbookReverseIdle;
            Vector3 start = m_spellbookReverseIdle ? m_spellbookEndIdle : m_spellbookStartIdle;
            Vector3 end = m_spellbookReverseIdle ? m_spellbookStartIdle : m_spellbookEndIdle;
            StartCoroutine(LerpHelper.LerpTransform(m_spellbookTransform, start, end, m_spellbookDurationIdle));
        }

        public void StartStaffIdle()
        {
            m_staffReverseIdle = !m_staffReverseIdle;
            Vector3 start = m_staffReverseIdle ? m_staffEndIdle : m_staffStartIdle;
            Vector3 end = m_staffReverseIdle ? m_staffStartIdle : m_staffEndIdle;
            StartCoroutine(LerpHelper.LerpTransform(m_staffTransform, start, end, m_staffDurationIdle));
        }
    }
}
