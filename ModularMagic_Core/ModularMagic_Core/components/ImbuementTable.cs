using ModularMagic_Core.components;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;
using System.Collections;
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

        private Material m_tableMat;
        private Color m_emissionHigh;
        private Color m_emissionLow;
        private float m_emissionMultiplier = 0.3f;
        private float m_emissionDurationIdle = 3f;
        private bool m_emissionReverseIdle = false;
        private bool m_emissiveIdleIsStarting = true;
        private IEnumerator m_emission;

        private Transform m_staffTransform;
        private Vector3 m_staffPositionStartIdle;
        private Vector3 m_staffPositionEndIdle;
        private Vector3 m_staffRotateStartIdle;
        private Vector3 m_staffRotateEndIdle;
        private float m_staffDurationIdle = 10f;
        private bool m_staffReverseIdle = false;
        private bool m_staffFirstRotate = true;
        private IEnumerator m_staffIdle;

        private void Awake()
        {
            netView = transform.Find("itemstand").gameObject.GetComponent<ItemStand>().m_netViewOverride;

            MeshRenderer meshComp = transform.Find("new/altar").gameObject.GetComponent<MeshRenderer>();
            m_tableMat = meshComp.materials[0];
            m_emissionHigh = LightColorPresetHelper.GetColors(LightColorPresetType.Green).emissionColor;
            m_emissionLow = new Color(m_emissionHigh.r * m_emissionMultiplier, m_emissionHigh.g * m_emissionMultiplier, m_emissionHigh.b * m_emissionMultiplier, m_emissionHigh.a);

            m_staffTransform = transform.Find("itemstand/attach_other");
            m_staffPositionStartIdle = new Vector3(m_staffTransform.localPosition.x, m_staffTransform.localPosition.y, m_staffTransform.localPosition.z);
            m_staffPositionEndIdle = new Vector3(m_staffTransform.localPosition.x, m_staffTransform.localPosition.y + 0.1f, m_staffTransform.localPosition.z);
            m_staffRotateStartIdle = new Vector3(0f, 0f, 45f);
            m_staffRotateEndIdle = new Vector3(0f, 0f, -45f);
        }

        public void StaffAttach(string imbuementsString, ItemData itemData)
        {
            m_imbuements = ImbuementHelper.StringToList(imbuementsString);
            m_imbued = m_imbuements.FindAll(item => item.isImbued);
            m_itemData = itemData;
            m_imbuementsString = imbuementsString;
            m_onItemAttach.Invoke();
            CreateRunes();

            Invoke(nameof(EmissionStart), 0f);
            InvokeRepeating(nameof(EmissionIdle), m_emissionDurationIdle + 0.1f, m_emissionDurationIdle + 0.1f);
            InvokeRepeating(nameof(StaffIdle), 0f, m_staffDurationIdle + 0.1f);
        }

        public void StaffRemove()
        {
            if (m_emission != null)
                StopCoroutine(m_emission);

            CancelInvoke(nameof(EmissionStart));
            CancelInvoke(nameof(EmissionIdle));
            CancelInvoke(nameof(StaffIdle));
            Invoke(nameof(EmissionStop), 0f);

            m_imbuements = new List<Imbuement>();
            m_imbued = new List<Imbuement>();
            m_itemData = null;
            m_imbuementsString = null;
            m_staffFirstRotate = true;
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

            // Jotunn.Logger.LogWarning("CanImbue: " + m_imbuementsString == currentImbuementsString);
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

        public void EmissionStart()
        {
            m_emissiveIdleIsStarting = true;
            Color fromColor = m_tableMat.GetColor("_EmissionColor");
            Color toColor = m_emissionHigh;
            m_emission = LerpHelper.LerpColor(m_tableMat, fromColor, toColor, m_emissionDurationIdle);
            StartCoroutine(m_emission);
        }

        public void EmissionStop()
        {
            Color fromColor = m_tableMat.GetColor("_EmissionColor");
            Color toColor = new Color(0f, 0f, 0f, 1f);
            m_emission = LerpHelper.LerpColor(m_tableMat, fromColor, toColor, m_emissionDurationIdle);
            StartCoroutine(m_emission);
        }

        public void EmissionIdle()
        {
            if (m_emissiveIdleIsStarting)
            {
                m_emissionReverseIdle = false;
                m_emissiveIdleIsStarting = false;
            }
            else
                m_emissionReverseIdle = !m_emissionReverseIdle;
                
            Color fromColor = m_emissionReverseIdle ? m_emissionLow : m_emissionHigh;
            Color toColor = m_emissionReverseIdle ? m_emissionHigh : m_emissionLow;
            m_emission = LerpHelper.LerpColor(m_tableMat, fromColor, toColor, m_emissionDurationIdle);
            StartCoroutine(m_emission);
        }

        public void StaffIdle()
        {
            m_staffReverseIdle = !m_staffReverseIdle;
            Vector3 startPos = m_staffReverseIdle ? m_staffPositionEndIdle : m_staffPositionStartIdle;
            Vector3 endPos = m_staffReverseIdle ? m_staffPositionStartIdle : m_staffPositionEndIdle;
            Vector3 startRot = m_staffFirstRotate ? new Vector3(0f, 0f, 0f) : m_staffReverseIdle ? m_staffRotateEndIdle : m_staffRotateStartIdle;
            Vector3 endRot = m_staffReverseIdle ? m_staffRotateStartIdle : m_staffRotateEndIdle;
            m_staffFirstRotate = false;
            // m_staffIdle = LerpHelper.SlerpPosition(m_staffTransform, start, end, m_staffDurationIdle);
            m_staffIdle = LerpHelper.LerpPositionAndRotation(m_staffTransform, startPos, endPos, startRot, endRot, m_staffDurationIdle);
            StartCoroutine(m_staffIdle);
        }

        //private float GetPercentage(float total, float part)
        //{
        //    Jotunn.Logger.LogWarning("Total: " + total);
        //    Jotunn.Logger.LogWarning("Part: " + part);

        //    if (total == 0f)
        //        return 0f;

        //    if (part == 0f)
        //        return 1;

        //    float result = part / total;

        //    if (result < 0) result = 0;
        //    if (result > 1) result = 1;

        //    Jotunn.Logger.LogWarning("Calc: " + part / total);
        //    Jotunn.Logger.LogWarning("Result: " + result);

        //    return result;
        //}
    }
}
