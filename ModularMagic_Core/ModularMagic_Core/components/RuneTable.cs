using Jotunn.Managers;
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
    internal class RuneTable : MonoBehaviour
    {
        private ZNetView netView;
        public List<Imbuement> m_imbuements = new List<Imbuement>();
        public ItemData m_itemData;
        public string m_imbuementsString;

        public UnityEvent m_onItemAttach = new UnityEvent();
        public UnityEvent m_onItemRemove = new UnityEvent();
        public UnityEvent m_onRuneActivation = new UnityEvent();
        public UnityEvent m_onSave = new UnityEvent();

        private Transform m_altarTransform;
        private Transform m_staffTransform;
        private Material m_tableMat;
        private Color m_emissionHigh;
        private Color m_emissionLow;
        private float m_emissionMultiplier = 0.3f;
        private float m_emissionDurationIdle = 3f;
        private bool m_emissionReverseIdle = false;
        private bool m_emissiveIdleIsStarting = true;
        private IEnumerator m_emission;

        public EffectList saveEffects = new EffectList();

        public void Awake()
        {
            netView = transform.Find("itemstand").gameObject.GetComponent<ItemStand>().m_netViewOverride;
            m_staffTransform = transform.Find("itemstand/attach_other");
            m_altarTransform = transform.Find("new/altar");

            MeshRenderer meshComp = m_altarTransform.gameObject.GetComponent<MeshRenderer>();
            m_tableMat = meshComp.materials[0];

            // TODO: Move this to staff attach when specific weapon colors are introduced
            m_emissionHigh = LightColorPresetHelper.GetColors(LightColorPresetType.Green).emissionColor;
            m_emissionLow = new Color(m_emissionHigh.r * m_emissionMultiplier, m_emissionHigh.g * m_emissionMultiplier, m_emissionHigh.b * m_emissionMultiplier, m_emissionHigh.a);

            EffectList.EffectData saveEffectData = new EffectList.EffectData();
            saveEffectData.m_enabled = true;
            saveEffectData.m_prefab = ModularMagic_Core.prefabs.SaveFX;
            saveEffectData.m_variant = -1;

            List<EffectList.EffectData> saveEffectsList = new List<EffectList.EffectData>();
            saveEffectsList.Add(saveEffectData);

            saveEffects.m_effectPrefabs = saveEffectsList.ToArray();
        }

        public void StaffAttach(string imbuementsString, ItemData itemData)
        {
            // Jotunn.Logger.LogWarning("=== Staff Attach ===========================");
            // Jotunn.Logger.LogWarning(imbuementsString);
            m_imbuements = ImbuementHelper.StringToList(imbuementsString);
            m_itemData = itemData;
            m_imbuementsString = imbuementsString;
            CreateRunes();

            m_onItemAttach.Invoke();

            Invoke(nameof(EmissionStart), 0f);
            InvokeRepeating(nameof(EmissionIdle), m_emissionDurationIdle + 0.1f, m_emissionDurationIdle + 0.1f);

            Animator animatorStaff = m_staffTransform.gameObject.GetComponent<Animator>();
            animatorStaff.SetTrigger("Idle");
        }

        public void StaffRemove()
        {
            if (m_emission != null)
                StopCoroutine(m_emission);

            CancelInvoke(nameof(EmissionStart));
            CancelInvoke(nameof(EmissionIdle));
            Invoke(nameof(EmissionStop), 0f);

            m_imbuements = new List<Imbuement>();
            m_itemData = null;
            m_imbuementsString = null;
            RemoveRunes();

            m_onItemRemove.Invoke();

            Animator animatorStaff = m_staffTransform.gameObject.GetComponent<Animator>();
            animatorStaff.Rebind();
            animatorStaff.Update(0f);
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

            foreach (Imbuement imbuement in m_imbuements)
            {
                GameObject interactRune = Instantiate(defaultRuneTransform.gameObject, defaultRuneTransform);
                interactRune.transform.SetParent(runesTransform);
                interactRune.SetActive(true);

                RuneTableRuneInteract tableInteract = interactRune.AddComponent<RuneTableRuneInteract>();
                tableInteract.Init(imbuement, index, m_emissionHigh);
                index++;
            }
        }

        public void RemoveRunes()
        {
            Transform runesTransform = transform.Find("runes");

            if (runesTransform == null || runesTransform.gameObject == null)
                throw new Exception("Could not find runes parent GameObject ");

            foreach (Transform child in runesTransform)
            {
                RuneTableRuneInteract tableInteract = child.gameObject.GetComponent<RuneTableRuneInteract>();
                tableInteract.StartMoveOut();
            }
        }

        public string CanSave()
        {
            string currentImbuementsString = ImbuementHelper.ListToString(m_imbuements);

            if (m_imbuementsString == currentImbuementsString)
                return CanImbueType.NoChange;

            return CanImbueType.Yes;
        }

        public bool Save()
        {
            try
            {
                string canSave = CanSave();

                if (canSave == CanImbueType.No || canSave == CanImbueType.NoChange)
                    return false;

                foreach (Imbuement imbuement in m_imbuements)
                {
                    if (imbuement.type == ImbuementType.None)
                        continue;

                    imbuement.saved = true;
                }

                string imbuementsString = ImbuementHelper.ListToString(m_imbuements);
                //Jotunn.Logger.LogWarning("=== Save ===================================");
                //Jotunn.Logger.LogWarning("STAFF SAVE: " + m_imbuementsString);

                m_itemData.m_customData[ModularMagic_Core.imbuementDataKey] = imbuementsString;
                SaveToZDO(m_itemData, netView.GetZDO());
                Game.instance.GetPlayerProfile().SavePlayerData(Player.m_localPlayer);

                m_imbuementsString = imbuementsString;
                m_onSave.Invoke();
                // saveEffects.Create(m_staffTransform.position, m_staffTransform.rotation);
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
    }
}
