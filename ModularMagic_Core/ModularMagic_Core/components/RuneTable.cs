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
        private ZNetView netView;
        // The working copy of the editor, this includes the runes that are not saved yet
        public List<Imbuement> m_imbuements = new List<Imbuement>();
        public ItemData m_itemData;
        // The saved imbuements (normalized), to see if there is anything to save
        public string m_imbuementsString;
        // The runes (id and level) that are saved in every slot, a slot without a rune is empty
        private List<Imbuement> m_savedImbuements = new List<Imbuement>();

        // Player id of the character that put the weapon on the table, only this character may edit
        private long m_editorId;
        // The saved data string of the stand the last time it was synced
        private string m_standDataString;
        // The unsaved changes (draft) of the stand the last time it was synced, null when there are none
        private string m_standDraftString;
        // The data revision of the stand the last time it was synced
        private uint m_syncedRevision;

        public UnityEvent m_onItemAttach = new UnityEvent();
        public UnityEvent m_onItemRemove = new UnityEvent();
        public UnityEvent m_onRuneActivation = new UnityEvent();
        public UnityEvent m_onSave = new UnityEvent();

        private Transform m_altarTransform;
        private Transform m_staffTransform;
        private ParticleSystem m_particles;
        private Material m_tableMat;
        private string m_colorPreset;
        private Color m_emissionHigh;
        private Color m_emissionLow;
        private float m_emissionMultiplier = 0.3f;
        private float m_emissionDurationIdle = 3f;
        private bool m_emissionReverseIdle = false;
        private bool m_emissiveIdleIsStarting = true;
        private IEnumerator m_emission;

        // Played at the table and at every rune that is new to a save. The effects are networked,
        // so only the player that saves creates them and everyone nearby sees them
        public EffectList saveTableEffects = new EffectList();
        public EffectList saveRuneEffects = new EffectList();
        public EffectList saveRemoveEffects = new EffectList();

        public void Awake()
        {
            netView = transform.Find("itemstand").gameObject.GetComponent<ItemStand>().m_netViewOverride;
            m_staffTransform = transform.Find("itemstand/attach_other");
            m_altarTransform = transform.Find("new/altar");

            Transform particlesTransform = transform.Find("effects/particles");
            m_particles = particlesTransform != null ? particlesTransform.GetComponent<ParticleSystem>() : null;

            if (m_particles == null)
                Jotunn.Logger.LogWarning("[Imbuements] The rune table has no effects/particles, the particles of the table will not show");

            MeshRenderer meshComp = m_altarTransform.gameObject.GetComponent<MeshRenderer>();
            m_tableMat = meshComp.materials[0];

            saveTableEffects.m_effectPrefabs = CreateEffectData(ModularMagic_Core.prefabs.SaveTableFX);
            saveRuneEffects.m_effectPrefabs = CreateEffectData(ModularMagic_Core.prefabs.SaveRuneFX);
            saveRemoveEffects.m_effectPrefabs = CreateEffectData(ModularMagic_Core.prefabs.SaveRemoveFX);

            if (netView == null)
                Jotunn.Logger.LogWarning("[Imbuements] The rune table stand has no ZNetView, saving imbuements will not work");
        }

        private static EffectList.EffectData[] CreateEffectData(GameObject prefab)
        {
            EffectList.EffectData effectData = new EffectList.EffectData();
            effectData.m_enabled = true;
            effectData.m_prefab = prefab;
            effectData.m_variant = -1;

            return new[] { effectData };
        }

        // The rune table the given item stand belongs to, or null for any other item stand
        public static RuneTable FromStand(ItemStand stand)
        {
            if (stand == null || stand.transform.parent == null)
                return null;

            return stand.transform.parent.GetComponent<RuneTable>();
        }

        // The ZDO of the stand is the source of truth, the table follows it as soon as its data changes
        private void Update()
        {
            ZDO zdo = netView != null ? netView.GetZDO() : null;

            if (zdo == null || zdo.DataRevision == m_syncedRevision)
                return;

            // A revision only counts as synced when the stand could be read, otherwise try again next frame
            if (SyncFromStand())
                m_syncedRevision = zdo.DataRevision;
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
        /// Makes the table match what is on the stand. Called on every client whenever the data of the stand changes
        /// and whenever the stand updates its visual, so players that did not place the staff, or arrive later, see the same runes.
        /// Returns false when the stand could not be read yet, so the caller has to try again.
        /// </summary>
        public bool SyncFromStand()
        {
            ZDO zdo = netView != null ? netView.GetZDO() : null;

            // Who is the editor can only be told once the local player is there, for example not right when a world loads.
            // The item of the stand is looked up in the ObjectDB
            if (zdo == null || Player.m_localPlayer == null || Player.m_localPlayer.GetPlayerID() == 0 || ObjectDB.instance == null)
                return false;

            ItemData standItem = ImbuementHelper.LoadItemFromZDO(zdo);

            if (standItem == null || !ImbuementHelper.HasImbuements(standItem))
            {
                if (m_itemData != null)
                    StaffRemove();

                return true;
            }

            string dataString = standItem.m_customData.GetValueSafe(ImbuementHelper.DataKey);
            long editorId = ImbuementHelper.GetEditor(standItem);

            // A different placement of a staff, for example removed and placed again by someone else, or another staff in its place
            if (m_itemData != null && (editorId != m_editorId || standItem.m_dropPrefab != m_itemData.m_dropPrefab))
                StaffRemove();

            if (m_itemData == null)
            {
                StaffAttach(dataString, standItem);
                return true;
            }

            string draftString = ImbuementHelper.GetDraft(standItem);

            // Neither the saved runes nor the unsaved changes changed
            if (dataString == m_standDataString && draftString == m_standDraftString)
                return true;

            m_standDataString = dataString;
            m_standDraftString = draftString;

            // The editor keeps their own working copy
            if (!IsLocalEditor())
                StaffRefresh(dataString, standItem);

            return true;
        }

        // Shows the saved runes and the unsaved changes of the editor to a player that is not editing
        private void StaffRefresh(string imbuementsString, ItemData itemData)
        {
            ImbuementSlots slots = ImbuementHelper.GetSlots(itemData);

            if (slots == null)
                return;

            List<Imbuement> savedImbuements = ImbuementHelper.Deserialize(imbuementsString, slots);
            string draft = ImbuementHelper.GetDraft(itemData);

            m_savedImbuements = savedImbuements.Select(imbuement => imbuement.CopyRune()).ToList();
            m_imbuements = draft != null ? ApplyDraft(draft, savedImbuements, slots) : savedImbuements;
            m_itemData = itemData;
            m_imbuementsString = ImbuementHelper.Serialize(savedImbuements);

            List<RuneTableRuneInteract> shownRunes = GetShownRunes();

            if (shownRunes.Count == m_imbuements.Count)
            {
                // The runes stay in their slot and play what the editor sees, without flying out and in
                for (int i = 0; i < shownRunes.Count; i++)
                {
                    shownRunes[i].ShowState(m_imbuements[i], GetReplacedRune(i));
                }
            }
            else
            {
                RemoveRunes();
                CreateRunes();
            }

            m_onRuneActivation.Invoke();
        }

        // The runes in slot order, without the ones that are flying out
        private List<RuneTableRuneInteract> GetShownRunes()
        {
            List<RuneTableRuneInteract> shownRunes = new List<RuneTableRuneInteract>();
            Transform runesTransform = transform.Find("runes");

            if (runesTransform == null)
                return shownRunes;

            foreach (Transform child in runesTransform)
            {
                RuneTableRuneInteract runeInteract = child.GetComponent<RuneTableRuneInteract>();

                if (runeInteract != null && !runeInteract.m_removing)
                    shownRunes.Add(runeInteract);
            }

            return shownRunes.OrderBy(runeInteract => runeInteract.m_index).ToList();
        }

        public void StaffAttach(string imbuementsString, ItemData itemData)
        {
            ImbuementSlots slots = ImbuementHelper.GetSlots(itemData);

            if (slots == null)
            {
                Jotunn.Logger.LogWarning($"[Imbuements] Cannot attach '{itemData.m_shared.m_name}', its prefab has no ImbuementSlots");
                return;
            }

            // The weapon decides the colour, before the runes are created so they start in it
            ApplyColor(slots.m_colorPreset);

            if (m_particles != null)
            {
                ParticleSystem.MainModule particlesMain = m_particles.main;
                particlesMain.startColor = LightColorPresetHelper.GetParticleColor(slots.m_colorPreset);
                m_particles.Play();
            }

            List<Imbuement> savedImbuements = ImbuementHelper.Deserialize(imbuementsString, slots);

            m_itemData = itemData;
            m_editorId = ImbuementHelper.GetEditor(itemData);
            m_standDataString = imbuementsString;
            m_savedImbuements = savedImbuements.Select(imbuement => imbuement.CopyRune()).ToList();
            // Normalized, so an untouched staff (or one in an old format) reports no changes
            m_imbuementsString = ImbuementHelper.Serialize(savedImbuements);

            // Everyone sees the unsaved changes of the editor, the editor continues with them
            string draft = ImbuementHelper.GetDraft(itemData);
            m_standDraftString = draft;
            m_imbuements = draft != null ? ApplyDraft(draft, savedImbuements, slots) : savedImbuements;
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

            // Stops emitting, the particles that are alive fade out
            if (m_particles != null)
                m_particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            m_imbuements = new List<Imbuement>();
            m_savedImbuements = new List<Imbuement>();
            m_itemData = null;
            m_imbuementsString = null;
            m_editorId = 0;
            m_standDataString = null;
            m_standDraftString = null;
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

        // A rune that is marked for removal in any slot is still on the staff until the changes are saved,
        // so the same rune can not be slotted again before that
        public bool IsRuneMarkedForRemoval(string runeId)
        {
            for (int i = 0; i < m_imbuements.Count; i++)
            {
                Imbuement replacedRune = GetReplacedRune(i);

                if (replacedRune != null && replacedRune.runeId == runeId)
                    return true;
            }

            return false;
        }

        // A saved rune (id and level) that is removed or replaced in the working copy, it stays visible until the changes are saved
        private Imbuement GetReplacedRune(int index)
        {
            if (index >= m_savedImbuements.Count || m_savedImbuements[index].rune == null)
                return null;

            Imbuement savedRune = m_savedImbuements[index];

            return m_imbuements[index].HasSameRune(savedRune) ? null : savedRune;
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

                string imbuementsString = ImbuementHelper.Serialize(m_imbuements);

                bool written = WriteToStand((standItem, slots) =>
                {
                    // Only accept slots and runes that exist
                    standItem.m_customData[ImbuementHelper.DataKey] = ImbuementHelper.Serialize(ImbuementHelper.Deserialize(imbuementsString, slots));
                    // Everything is saved now, so there are no unsaved changes anymore
                    ImbuementHelper.SetDraft(standItem, null);
                });

                // Nothing changes when the write failed, otherwise the old runes would drop without being replaced
                if (!written)
                    return false;

                // The runes that are not saved yet are the ones that are new to this save
                List<int> newSlots = new List<int>();

                for (int i = 0; i < m_imbuements.Count; i++)
                {
                    if (m_imbuements[i].type == ImbuementType.None)
                        continue;

                    if (!m_imbuements[i].saved)
                        newSlots.Add(i);

                    m_imbuements[i].saved = true;
                }

                // A saved rune that is gone or replaced is removed by this save
                bool runeRemoved = false;

                for (int i = 0; i < m_savedImbuements.Count && i < m_imbuements.Count; i++)
                {
                    if (m_savedImbuements[i].rune != null && !m_imbuements[i].HasSameRune(m_savedImbuements[i]))
                        runeRemoved = true;
                }

                m_itemData.m_customData[ImbuementHelper.DataKey] = imbuementsString;
                m_imbuementsString = imbuementsString;
                m_standDataString = imbuementsString;
                m_savedImbuements = m_imbuements.Select(imbuement => imbuement.CopyRune()).ToList();
                m_onSave.Invoke();
                PlaySaveEffects(newSlots, runeRemoved);
                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not save imbuements to item: " + e);
                return false;
            }
        }

        // When a rune is added, the table effect plays at the staff and the rune effect at every new rune.
        // When a rune is removed, the remove effect plays once at the staff. A save that only removes runes plays only the remove effect.
        // The effects are only cosmetic, so a failing effect must never turn a saved staff into a failed save
        private void PlaySaveEffects(List<int> newSlots, bool runeRemoved)
        {
            try
            {
                if (runeRemoved)
                {
                    foreach (GameObject effect in saveRemoveEffects.Create(m_staffTransform.position, m_staffTransform.rotation))
                        SaveFXColor.SetPreset(effect, m_colorPreset);
                }

                if (newSlots.Count > 0)
                {
                    foreach (GameObject effect in saveTableEffects.Create(m_staffTransform.position, m_staffTransform.rotation))
                        SaveFXColor.SetPreset(effect, m_colorPreset);
                }

                foreach (int slot in newSlots)
                {
                    GetRuneDropPoint(slot, out Vector3 position, out Quaternion rotation);

                    foreach (GameObject effect in saveRuneEffects.Create(position, rotation))
                        SaveFXColor.SetPreset(effect, m_colorPreset);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not play the save effect: " + e);
            }
        }

        /// <summary>
        /// The editor owns the stand while editing, so the data is a plain write to the ZDO of the stand and reaches
        /// everybody through the ZDO sync. Returns false when nothing was written, for example when the staff of this
        /// editor is not on the stand anymore.
        /// </summary>
        private bool WriteToStand(Action<ItemData, ImbuementSlots> change)
        {
            if (netView == null || !netView.IsValid())
                return false;

            ZDO zdo = netView.GetZDO();
            ItemData standItem = ImbuementHelper.LoadItemFromZDO(zdo);
            ImbuementSlots slots = standItem != null ? ImbuementHelper.GetSlots(standItem) : null;

            // Only the editor of the staff on the stand writes, and only then the ownership is taken
            if (slots == null || m_editorId == 0 || m_editorId != ImbuementHelper.GetEditor(standItem))
            {
                Jotunn.Logger.LogWarning("[Imbuements] Could not write to the stand, the staff of this editor is not on it");
                return false;
            }

            // The stand can have moved to another owner while the editor was away, only the owner's writes reach the others
            netView.ClaimOwnership();

            change(standItem, slots);
            SaveToZDO(standItem, zdo);

            return true;
        }

        // Remembers the working copy of the editor on the stand, so unsaved runes are not lost when the editor walks away or relogs.
        // Called by the editor before a change is final, returns false when it could not be remembered
        public bool WriteDraft()
        {
            string draft = ImbuementHelper.Serialize(m_imbuements);

            return WriteToStand((standItem, slots) =>
            {
                string draftString = ImbuementHelper.Serialize(ImbuementHelper.Deserialize(draft, slots));
                string savedString = ImbuementHelper.Serialize(ImbuementHelper.Deserialize(standItem.m_customData.GetValueSafe(ImbuementHelper.DataKey), slots));

                // There is nothing to remember when the working copy is the same as what is saved
                ImbuementHelper.SetDraft(standItem, draftString == savedString ? null : draftString);
            });
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

            foreach (KeyValuePair<int, Imbuement> unsaved in ImbuementHelper.GetUnsavedRunes(savedString, draft, slots))
            {
                GetRuneDropPoint(unsaved.Key, out Vector3 position, out Quaternion rotation);
                ImbuementHelper.DropRune(unsaved.Value.rune, unsaved.Value.level, position, rotation);
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

        // Sets the colour of the table glow, the runes and their particles. A missing or unknown preset gives white
        private void ApplyColor(string preset)
        {
            m_colorPreset = preset;
            m_emissionHigh = LightColorPresetHelper.GetColors(preset).emissionColor;
            m_emissionLow = new Color(m_emissionHigh.r * m_emissionMultiplier, m_emissionHigh.g * m_emissionMultiplier, m_emissionHigh.b * m_emissionMultiplier, m_emissionHigh.a);
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
