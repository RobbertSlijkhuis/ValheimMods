using Jotunn.Managers;
using ModularMagic_Core.Components;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections;
using UnityEngine;
using static ItemDrop;
using Color = UnityEngine.Color;

namespace ModularMagic_Core.components
{
    internal class RuneTableRuneInteract : MonoBehaviour, Hoverable, Interactable
    {
        public RuneTable m_imbuementTable;
        public RuneTableRuneResetInteract m_resetInteract;
        public Animator m_animatorNew;
        public Animator m_animatorOld;
        public Animator m_animatorRune;
        public MeshRenderer m_meshRenderer;
        public ParticleSystem m_particleSystem;
        public Transform m_transformNew;
        public Transform m_transformOld;

        public Imbuement m_imbuement;
        public int m_index;
        public bool m_locked = false;
        // The saved rune (id and level) that was removed or replaced, it drops when the changes are saved
        public Imbuement m_oldRune;
        public Color m_emission;

        // replacedRune is a saved rune that was removed or replaced by the editor, it stays visible until the changes are saved
        public void Init(Imbuement imbuement, int index, Color emission, Imbuement replacedRune)
        {
            m_transformNew = transform.Find("new");
            m_transformOld = transform.Find("old");
            m_animatorNew = m_transformNew.gameObject.GetComponent<Animator>();
            m_animatorOld = m_transformOld.gameObject.GetComponent<Animator>();
            m_animatorRune = gameObject.GetComponent<Animator>();
            m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
            m_particleSystem = gameObject.GetComponent<ParticleSystem>();
            m_imbuementTable = transform.parent.parent.gameObject.GetComponent<RuneTable>();
            m_imbuementTable.m_onSave.AddListener(OnSave);
            m_imbuement = imbuement;
            m_index = index;
            m_emission = emission;

            transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);

            ParticleSystemRenderer particleRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.materials[0].SetColor("_EmissionColor", emission);

            ImbuementRune rune = m_imbuement.rune;
            UpdateRune(rune != null ? rune.gameObject : null);

            if (replacedRune != null)
                RestoreReplacedRune(replacedRune);

            StartMoveIn();
        }

        // Shows the saved rune that is marked for removal, the same as it looks after marking it for removal
        private void RestoreReplacedRune(Imbuement replaced)
        {
            ImbuementRune rune = replaced.rune;
            Transform attachTransform = rune != null ? rune.transform.Find("attach") : null;

            if (attachTransform == null)
            {
                Jotunn.Logger.LogError("Could not find attach for the replaced rune");
                return;
            }

            GameObject attached = Instantiate(attachTransform.gameObject, m_transformOld);
            Transform runeTransform = RuneModelHelper.ShowModel(attached.transform, replaced.level);

            if (runeTransform == null)
            {
                Jotunn.Logger.LogError("Could not find the rune transform of the replaced rune");
                Destroy(attached);
                return;
            }

            MeshRenderer meshRenderer = runeTransform.gameObject.GetComponent<MeshRenderer>();
            meshRenderer.materials[0].SetColor("_EmissionColor", m_emission);
            runeTransform.localRotation = TransformHelper.GenerateRotation(new Vector3(0, 0, 0));
            runeTransform.localScale = new Vector3(1f, 1f, 1f);

            attached.transform.localRotation = TransformHelper.GenerateRotation(new Vector3(0, 0, 0));
            attached.transform.localPosition = new Vector3(0, 0, -40);

            m_oldRune = replaced;
            m_resetInteract = attached.AddComponent<RuneTableRuneResetInteract>();
        }

        private void OnDestroy()
        {
            // The table lives on when the runes are removed and shown again, so the listener would pile up
            if (m_imbuementTable != null)
                m_imbuementTable.m_onSave.RemoveListener(OnSave);
        }

        // A rune that is taken out of a slot drops from that slot, just like a removed saved rune drops when the changes are saved
        private void DropRune(Imbuement imbuement)
        {
            if (imbuement.rune != null)
                ImbuementHelper.DropRune(imbuement.rune, imbuement.level, m_transformNew.position, m_transformNew.rotation);
        }

        private void UpdateRune(GameObject prefab)
        {
            if (prefab == null)
            {
                Transform attachCloneTransform = m_transformNew.Find("attach(Clone)");

                if (attachCloneTransform == null || m_transformOld == null)
                    return;

                if (m_oldRune != null && m_transformOld.childCount == 0)
                    MarkForRemoval();
                else
                    Destroy(attachCloneTransform.gameObject);

                m_meshRenderer.enabled = true;
                return;
            }

            Transform attachTransform = prefab.transform.Find("attach");

            if (attachTransform == null)
            {
                Jotunn.Logger.LogError("Could not find attach for rune");
                return;
            }

            GameObject attached = Instantiate(attachTransform.gameObject, m_transformNew);
            Transform runeTransform = RuneModelHelper.ShowModel(attached.transform, m_imbuement.level);

            if (runeTransform == null)
            {
                Jotunn.Logger.LogError("Could not find the rune transform");
                return;
            }

            MeshRenderer meshRenderer = runeTransform.gameObject.GetComponent<MeshRenderer>();
            meshRenderer.materials[0].SetColor("_EmissionColor", m_emission);
            runeTransform.localRotation = TransformHelper.GenerateRotation(new Vector3(0, 0, 0));
            runeTransform.localScale = new Vector3(1f, 1f, 1f);
            m_meshRenderer.enabled = false;
        }

        public string GetHoverText()
        {
            if (m_locked)
                return "Waiting for action to finish...";

            if (!m_imbuementTable.IsLocalEditor())
            {
                string lockMessage = m_imbuementTable.GetLockMessage();

                if (m_imbuement.type == ImbuementType.None)
                    return $"<color=yellow>{m_imbuement.name}</color>\n{lockMessage}";

                return $"<color=green>{m_imbuement.name} {(m_imbuement.level > 0 ? m_imbuement.level : "")}</color>\n{m_imbuement.description}\n{lockMessage}";
            }

            if (m_imbuement.type == ImbuementType.None)
            {
                string imbuementNameEmpty = $"<color=yellow>{m_imbuement.name}</color>\n";
                string inputItem = "[<color=yellow>1-8</color>] Attach rune";
                return imbuementNameEmpty + inputItem;
            }

            string imbuementName = $"<color=green>{m_imbuement.name} {(m_imbuement.level > 0 ? m_imbuement.level : "")}</color>\n";
            string description = m_imbuement.description + "\n";
            string inputUse = Localization.instance.Localize("[<color=yellow>$ui_hold $KEY_Use</color>] Remove rune");
            return imbuementName + description + inputUse;
        }

        public string GetHoverName()
        {
            return PluginConfig.piece1.name.Value;
        }

        public float GetHoverOffset()
        {
            return 0f;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            if (m_locked)
                return false;

            if (!m_imbuementTable.IsLocalEditor())
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, m_imbuementTable.GetLockMessage());
                return false;
            }

            if (m_imbuement.type != ImbuementType.None)
            {
                Jotunn.Logger.LogWarning("Already imbued...");
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "That slot already is already occupied!");
                return false;
            }

            if (item == null)
            {
                Jotunn.Logger.LogWarning("Item is null!");
                return false;
            }

            GameObject prefab = item.m_dropPrefab;

            if (prefab == null)
            {
                Jotunn.Logger.LogWarning("No drop prefab found!");
                return false;
            }

            ImbuementRune imbuementRune = prefab.GetComponent<ImbuementRune>();

            if (imbuementRune == null)
                return false;

            if (m_imbuementTable.m_imbuements.Find(item => item.type == imbuementRune.m_type) != null)
            {
                Jotunn.Logger.LogWarning("Already slotted a rune of the same type!");
                return false;
            }

            // The same rune can not be on the staff twice, also not while the old one is waiting to be removed by saving
            if (m_imbuementTable.IsRuneMarkedForRemoval(imbuementRune.m_id))
            {
                Jotunn.Logger.LogWarning("The same rune is still on the staff, waiting to be removed!");
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "That rune is still on this staff, apply the changes first");
                return false;
            }

            if (!imbuementRune.m_allowedWeapons.Contains(m_imbuement.weaponType))
            {
                Jotunn.Logger.LogWarning("This rune is not allowed on this weapon!");
                return false;
            }

            // The level of the rune is the quality of the item
            int level = item.m_quality;

            if (!imbuementRune.IsValidLevel(level))
            {
                Jotunn.Logger.LogWarning($"Rune '{imbuementRune.m_id}' has no level {level}!");
                return false;
            }

            if (m_imbuement.tier < imbuementRune.GetRequiredTier(level))
            {
                Jotunn.Logger.LogWarning("This weapon is not high enough tier to slot this rune!");
                return false;
            }

            if (!Player.m_localPlayer.GetInventory().RemoveOneItem(item))
            {
                Jotunn.Logger.LogError("Coudl not remove rune from inventory");
                return false;
            }

            m_imbuement.SetRune(imbuementRune, level);

            m_imbuementTable.m_onRuneActivation.Invoke();

            UpdateRune(prefab);

            return true;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (alt || !hold || m_imbuement.type == ImbuementType.None || m_locked || !m_imbuementTable.IsLocalEditor())
                return false;

            if (m_imbuement.saved)
                m_oldRune = m_imbuement.CopyRune();
            else
                DropRune(m_imbuement);

            m_imbuement.Clear();

            m_imbuementTable.m_onRuneActivation.Invoke();

            UpdateRune(null);

            return true;
        }

        public void Reset()
        {
            m_locked = true;

            // The slot is empty, or it holds a new rune that is kicked out first
            if (m_imbuement.type == ImbuementType.None)
            {
                m_animatorOld.SetTrigger("Reset");
                StartCoroutine(CompleteReset());
            }
            else
            {
                m_animatorOld.SetTrigger("ResetWithKick");
                StartCoroutine(KickNewRune());
            }
        }

        public IEnumerator KickNewRune()
        {
            yield return new WaitForSeconds(0.4f);

            ItemDrop kicked = ImbuementHelper.DropRune(m_imbuement.rune, m_imbuement.level, m_transformNew.position, m_transformNew.rotation);
            Rigidbody rigidbody = kicked.GetComponent<Rigidbody>();
            rigidbody.AddForce(kicked.transform.forward * 300);

            Destroy(m_transformNew.Find("attach(Clone)").gameObject);
            StartCoroutine(CompleteReset());
        }

        public IEnumerator CompleteReset()
        {
            yield return new WaitForSeconds(0.6f);

            Transform attachTransform = m_transformOld.Find("attach(Clone)");
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            attachTransform.SetParent(m_transformNew);
            m_animatorOld.Rebind();
            m_animatorOld.Update(0f);
            m_particleSystem.Play();

            m_imbuement.SetRune(m_oldRune.rune, m_oldRune.level);
            m_imbuement.saved = true;

            m_imbuementTable.m_onRuneActivation.Invoke();

            Destroy(m_resetInteract);
            m_oldRune = null;
            meshRenderer.enabled = false;

            m_locked = false;
        }

        public void MarkForRemoval()
        {
            m_locked = true;

            Transform attachTransform = m_transformNew.Find("attach(Clone)");

            m_resetInteract = attachTransform.gameObject.AddComponent<RuneTableRuneResetInteract>();
            m_particleSystem.Stop();
            m_animatorNew.SetTrigger("MarkForRemoval");

            StartCoroutine(CompleteMarkForRemoval());
        }

        public IEnumerator CompleteMarkForRemoval()
        {
            yield return new WaitForSeconds(0.6f);

            Transform attachTransform = m_transformNew.Find("attach(Clone)");

            attachTransform.SetParent(m_transformOld);
            m_animatorNew.Rebind();
            m_animatorNew.Update(0f);

            attachTransform.localRotation = TransformHelper.GenerateRotation(new Vector3(0, 0, 0));
            attachTransform.localPosition = new Vector3(0, 0, -40);
            m_transformNew.localPosition = new Vector3(0f, 0f, 0f);

            m_locked = false;
        }

        public void OnSave()
        {
            if (m_imbuement.type != ImbuementType.None)
                m_particleSystem.Play();

            if (m_oldRune == null)
                return;

            if (m_oldRune.rune == null)
            {
                Jotunn.Logger.LogError($"Could not drop old rune, unknown rune '{m_oldRune.runeId}'");
                return;
            }

            Transform oldAttachTransform = m_transformOld.GetChild(0);
            ImbuementHelper.DropRune(m_oldRune.rune, m_oldRune.level, oldAttachTransform.position, oldAttachTransform.rotation);
            Destroy(oldAttachTransform.gameObject);
            m_oldRune = null;
        }

        public void StartMoveIn()
        {
            // The sparkles show that a rune is saved, so an empty slot and an unsaved rune do not have them
            if (m_imbuement.type == ImbuementType.None || !m_imbuement.saved)
                m_particleSystem.Stop();

            m_animatorRune.SetTrigger($"FlyIn{m_index}");
        }

        public void StartMoveOut()
        {
            m_particleSystem.Stop();
            m_animatorRune.SetTrigger($"FlyOut{m_index}");
            Destroy(gameObject, m_animatorRune.GetCurrentAnimatorClipInfo(0).Length * 2);
        }
    }
}
