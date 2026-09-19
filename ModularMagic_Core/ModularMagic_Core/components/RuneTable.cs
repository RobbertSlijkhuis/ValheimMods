using HarmonyLib;
using Jotunn.Managers;
using ModularMagic_Core.components;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static ItemDrop;

namespace ModularMagic_Core.Components
{
    internal class RuneTable : MonoBehaviour
    {
        private const string DraftRpcName = "RPC_MMC_SaveDraft";
        private const string SaveRpcName = "RPC_MMC_SaveImbuements";

        private ZNetView netView;
        // The working copy of the editor, this includes the runes that are not saved yet
        public List<Imbuement> m_imbuements = new List<Imbuement>();
        public ItemData m_itemData;
        // The saved imbuements (normalized), to see if there is anything to save
        public string m_imbuementsString;
        // The runes that are saved in every slot, null for an empty slot
        private List<ImbuementRune> m_savedRunes = new List<ImbuementRune>();

        // Player id of the character that put the weapon on the table, only this character may edit
        private long m_editorId;
        // The saved data string of the stand the last time it was synced
        private string m_standDataString;

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

            // Saving is done by the owner of the stand, the editor asks for it with this RPC
            if (netView != null)
            {
                netView.Register<string, long>(SaveRpcName, RPC_SaveImbuements);
                netView.Register<string, long>(DraftRpcName, RPC_SaveDraft);
            }
            else
                Jotunn.Logger.LogWarning("[Imbuements] The rune table stand has no ZNetView, saving imbuements will not work");

            // Every change of the editor is remembered on the stand
            m_onRuneActivation.AddListener(OnRuneActivation);
        }

        // The rune table the given item stand belongs to, or null for any other item stand
        public static RuneTable FromStand(ItemStand stand)
        {
            if (stand == null || stand.transform.parent == null)
                return null;

            return stand.transform.parent.GetComponent<RuneTable>();
        }

        public bool IsLocalEditor()
        {
            return m_itemData != null && m_editorId != 0 && Player.m_localPlayer != null && m_editorId == Player.m_localPlayer.GetPlayerID();
        }

        public string GetLockMessage()
        {
            if (m_editorId == 0)
                return "Take the staff off and place it again to edit it";

            return "Another player is editing this staff";
        }

        /// <summary>
        /// Makes the table match what is on the stand. Called on every client whenever the stand updates its visual,
        /// so players that did not place the staff, or arrive later, see the same runes.
        /// </summary>
        public void SyncFromStand()
        {
            ZDO zdo = netView != null ? netView.GetZDO() : null;

            // Who is the editor can only be told once the local player is there, for example not right when a world loads
            if (zdo == null || Player.m_localPlayer == null || Player.m_localPlayer.GetPlayerID() == 0)
                return;

            ItemData standItem = ImbuementHelper.LoadItemFromZDO(zdo);

            if (standItem == null || !ImbuementHelper.HasImbuements(standItem))
            {
                if (m_itemData != null)
                    StaffRemove();

                return;
            }

            string dataString = standItem.m_customData.GetValueSafe(ImbuementHelper.DataKey);
            long editorId = ImbuementHelper.GetEditor(standItem);

            // A different placement of a staff, for example removed and placed again by someone else
            if (m_itemData != null && editorId != m_editorId)
                StaffRemove();

            if (m_itemData == null)
            {
                StaffAttach(dataString, standItem);
                return;
            }

            if (dataString == m_standDataString)
                return;

            m_standDataString = dataString;

            // The editor keeps their own working copy
            if (!IsLocalEditor())
                StaffRefresh(dataString, standItem);
        }

        // Shows the newly saved runes to a player that is not editing
        private void StaffRefresh(string imbuementsString, ItemData itemData)
        {
            ImbuementSlots slots = ImbuementHelper.GetSlots(itemData);

            if (slots == null)
                return;

            RemoveRunes();

            m_imbuements = ImbuementHelper.Deserialize(imbuementsString, slots);
            m_savedRunes = m_imbuements.Select(imbuement => imbuement.rune).ToList();
            m_itemData = itemData;
            m_imbuementsString = ImbuementHelper.Serialize(m_imbuements);
            CreateRunes();

            m_onRuneActivation.Invoke();
        }

        public void StaffAttach(string imbuementsString, ItemData itemData)
        {
            ImbuementSlots slots = ImbuementHelper.GetSlots(itemData);

            if (slots == null)
            {
                Jotunn.Logger.LogWarning($"[Imbuements] Cannot attach '{itemData.m_shared.m_name}', its prefab has no ImbuementSlots");
                return;
            }

            List<Imbuement> savedImbuements = ImbuementHelper.Deserialize(imbuementsString, slots);

            m_itemData = itemData;
            m_editorId = ImbuementHelper.GetEditor(itemData);
            m_standDataString = imbuementsString;
            m_savedRunes = savedImbuements.Select(imbuement => imbuement.rune).ToList();
            // Normalized, so an untouched staff (or one in an old format) reports no changes
            m_imbuementsString = ImbuementHelper.Serialize(savedImbuements);

            // The editor continues with their unsaved changes, everyone else sees what is saved
            string draft = ImbuementHelper.GetDraft(itemData);
            m_imbuements = draft != null && IsLocalEditor() ? ApplyDraft(draft, savedImbuements, slots) : savedImbuements;
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
            m_savedRunes = new List<ImbuementRune>();
            m_itemData = null;
            m_imbuementsString = null;
            m_editorId = 0;
            m_standDataString = null;
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
                tableInteract.Init(imbuement, index, m_emissionHigh, GetReplacedRune(index));
                index++;
            }
        }

        // The working copy of the editor, with the runes that were changed since the last save marked as not saved
        private List<Imbuement> ApplyDraft(string draft, List<Imbuement> savedImbuements, ImbuementSlots slots)
        {
            List<Imbuement> working = ImbuementHelper.Deserialize(draft, slots);

            for (int i = 0; i < working.Count; i++)
            {
                working[i].saved = working[i].rune != null && working[i].HasSameRune(savedImbuements[i]);
            }

            return working;
        }

        // A saved rune that is removed or replaced in the working copy, it stays visible until the changes are saved
        private ImbuementRune GetReplacedRune(int index)
        {
            if (index >= m_savedRunes.Count || m_savedRunes[index] == null)
                return null;

            ImbuementRune savedRune = m_savedRunes[index];
            Imbuement working = m_imbuements[index];

            return working.runeId == savedRune.m_id && working.level == savedRune.m_level ? null : savedRune;
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
            string currentImbuementsString = ImbuementHelper.Serialize(m_imbuements);

            if (m_imbuementsString == currentImbuementsString)
                return CanImbueType.NoChange;

            return CanImbueType.Yes;
        }

        public bool Save()
        {
            try
            {
                if (!IsLocalEditor() || !netView.IsValid())
                    return false;

                if (CanSave() != CanImbueType.Yes)
                    return false;

                foreach (Imbuement imbuement in m_imbuements)
                {
                    if (imbuement.type == ImbuementType.None)
                        continue;

                    imbuement.saved = true;
                }

                string imbuementsString = ImbuementHelper.Serialize(m_imbuements);

                // The owner of the stand writes the data, only the owner can change it reliably
                netView.InvokeRPC(SaveRpcName, imbuementsString, m_editorId);

                m_itemData.m_customData[ImbuementHelper.DataKey] = imbuementsString;
                m_imbuementsString = imbuementsString;
                m_standDataString = imbuementsString;
                m_savedRunes = m_imbuements.Select(imbuement => imbuement.rune).ToList();
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

        // Runs on the owner of the stand
        private void RPC_SaveImbuements(long sender, string imbuementsString, long editorId)
        {
            try
            {
                if (!netView.IsOwner())
                    return;

                ZDO zdo = netView.GetZDO();
                ItemData standItem = ImbuementHelper.LoadItemFromZDO(zdo);
                ImbuementSlots slots = ImbuementHelper.GetSlots(standItem);

                if (slots == null)
                {
                    Jotunn.Logger.LogWarning("[Imbuements] Rejected a save, there is no imbuable staff on the stand");
                    return;
                }

                // The owner can not look up the player id of the sender, so the editor sends it along
                if (editorId == 0 || editorId != ImbuementHelper.GetEditor(standItem))
                {
                    Jotunn.Logger.LogWarning($"[Imbuements] Rejected a save from {sender}, player {editorId} is not the editor of this staff");
                    return;
                }

                // Only accept slots and runes that exist
                standItem.m_customData[ImbuementHelper.DataKey] = ImbuementHelper.Serialize(ImbuementHelper.Deserialize(imbuementsString, slots));
                // Everything is saved now, so there are no unsaved changes anymore
                ImbuementHelper.SetDraft(standItem, null);
                SaveToZDO(standItem, zdo);
                netView.InvokeRPC(ZNetView.Everybody, "RPC_UpdateVisual");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not save imbuements to the stand: " + e);
            }
        }

        private void OnRuneActivation()
        {
            // The viewers also invoke this when they refresh, they have nothing to remember
            if (IsLocalEditor())
                SendDraft();
        }

        // Remembers the working copy of the editor on the stand, so unsaved runes are not lost when the editor walks away or relogs
        private void SendDraft()
        {
            if (netView == null || !netView.IsValid())
                return;

            netView.InvokeRPC(DraftRpcName, ImbuementHelper.Serialize(m_imbuements), m_editorId);
        }

        // Runs on the owner of the stand
        private void RPC_SaveDraft(long sender, string draft, long editorId)
        {
            try
            {
                if (!netView.IsOwner())
                    return;

                ZDO zdo = netView.GetZDO();
                ItemData standItem = ImbuementHelper.LoadItemFromZDO(zdo);
                ImbuementSlots slots = ImbuementHelper.GetSlots(standItem);

                if (slots == null || editorId == 0 || editorId != ImbuementHelper.GetEditor(standItem))
                {
                    Jotunn.Logger.LogWarning($"[Imbuements] Rejected unsaved changes from {sender}, player {editorId} is not the editor of this staff");
                    return;
                }

                string draftString = ImbuementHelper.Serialize(ImbuementHelper.Deserialize(draft, slots));
                string savedString = ImbuementHelper.Serialize(ImbuementHelper.Deserialize(standItem.m_customData.GetValueSafe(ImbuementHelper.DataKey), slots));

                // There is nothing to remember when the working copy is the same as what is saved
                ImbuementHelper.SetDraft(standItem, draftString == savedString ? null : draftString);
                SaveToZDO(standItem, zdo);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not remember the unsaved changes on the stand: " + e);
            }
        }

        /// <summary>
        /// Drops the unsaved runes from their slots and forgets the unsaved changes. Runs on the owner of the stand,
        /// right before the staff leaves the stand, so it also works when the editor is not there anymore.
        /// </summary>
        public void DropDraftRunes()
        {
            ZDO zdo = netView != null ? netView.GetZDO() : null;

            if (zdo == null)
                return;

            ItemData standItem = ImbuementHelper.LoadItemFromZDO(zdo);
            ImbuementSlots slots = ImbuementHelper.GetSlots(standItem);
            string draft = standItem != null ? ImbuementHelper.GetDraft(standItem) : null;

            if (slots == null || draft == null)
                return;

            string savedString = standItem.m_customData.GetValueSafe(ImbuementHelper.DataKey);

            foreach (KeyValuePair<int, ImbuementRune> unsaved in ImbuementHelper.GetUnsavedRunes(savedString, draft, slots))
            {
                GetRuneDropPoint(unsaved.Key, out Vector3 position, out Quaternion rotation);
                Instantiate(unsaved.Value.gameObject, position, rotation);
            }

            ImbuementHelper.SetDraft(standItem, null);
            SaveToZDO(standItem, zdo);
        }

        // Where the rune of a slot is on the table, or the drop point of the stand when the runes are not shown
        private void GetRuneDropPoint(int slot, out Vector3 position, out Quaternion rotation)
        {
            Transform runesTransform = transform.Find("runes");

            if (runesTransform != null)
            {
                foreach (Transform child in runesTransform)
                {
                    RuneTableRuneInteract runeInteract = child.GetComponent<RuneTableRuneInteract>();

                    if (runeInteract != null && runeInteract.m_index == slot && runeInteract.m_transformNew != null)
                    {
                        position = runeInteract.m_transformNew.position;
                        rotation = runeInteract.m_transformNew.rotation;
                        return;
                    }
                }
            }

            Transform dropPoint = transform.Find("itemstand").GetComponent<ItemStand>().m_dropSpawnPoint;
            position = dropPoint.position;
            rotation = dropPoint.rotation;
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
