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
        public ItemData m_itemNew;
        public string m_dropOldItem;
        public Color m_emission;

        public void Init(Imbuement imbuement, int index, Color emission)
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
            m_imbuementTable.m_onItemRemove.AddListener(OnRemove);
            m_imbuement = imbuement;
            m_index = index;
            m_emission = emission;

            transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);

            ParticleSystemRenderer particleRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.materials[0].SetColor("_EmissionColor", emission);

            GameObject prefab = PrefabManager.Instance.GetPrefab(m_imbuement.prefab);
            UpdateRune(prefab);
            StartMoveIn();
        }

        private void UpdateRune(GameObject prefab)
        {
            if (prefab == null)
            {
                Transform attachCloneTransform = m_transformNew.Find("attach(Clone)");

                if (attachCloneTransform == null || m_transformOld == null)
                    return;

                if (m_dropOldItem != null && m_transformOld.childCount == 0)
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
            Transform runeTransform = attached.transform.Find($"rune_{m_imbuement.level}");

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

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            if (m_locked)
                return false;

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

            if (!imbuementRune.m_allowedWeapons.Contains(m_imbuement.weaponType))
            {
                Jotunn.Logger.LogWarning("This rune is not allowed on this weapon!");
                return false;
            }

            if (m_imbuement.tier < imbuementRune.m_tier)
            {
                Jotunn.Logger.LogWarning("This weapon is not high enough tier to slot this rune!");
                return false;
            }

            string weaponTypeLower = m_imbuement.weaponType.ToLower();

            if (!Player.m_localPlayer.GetInventory().RemoveOneItem(item))
            {
                Jotunn.Logger.LogError("Coudl not remove rune from inventory");
                return false;
            }

            Jotunn.Logger.LogWarning($"Removing {item.m_shared.m_name} from inventory");
            m_itemNew = item;

            m_imbuement.description = Localization.instance.Localize($"${imbuementRune.m_descriptionKey}_{weaponTypeLower}");
            m_imbuement.level = imbuementRune.m_level;
            m_imbuement.name = Localization.instance.Localize($"${imbuementRune.m_nameKey}_{weaponTypeLower}");
            m_imbuement.prefab = prefab.name;
            m_imbuement.saved = false;
            m_imbuement.type = imbuementRune.m_type;
            m_imbuement.value = imbuementRune.m_value;

            m_imbuementTable.m_onRuneActivation.Invoke();

            UpdateRune(prefab);

            return true;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (alt || !hold || m_imbuement.type == ImbuementType.None || m_locked)
                return false;

            if (m_imbuement.saved)
            {
                m_dropOldItem = m_imbuement.prefab;
            }
            else
            {
                Player.m_localPlayer.GetInventory().AddItem(m_itemNew);
                m_itemNew = null;
            }

            m_imbuement.description = "";
            m_imbuement.name = "Empty";
            m_imbuement.prefab = null;
            m_imbuement.saved = false;
            m_imbuement.type = ImbuementType.None;
            m_imbuement.value = "";

            m_imbuementTable.m_onRuneActivation.Invoke();

            UpdateRune(null);

            return true;
        }

        public void Reset()
        {
            m_locked = true;

            if (m_itemNew == null)
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

            Jotunn.Logger.LogWarning("KICK");
            GameObject kicked = Instantiate(m_itemNew.m_dropPrefab, m_transformNew.position, m_transformNew.rotation);
            Rigidbody rigidbody = kicked.GetComponent<Rigidbody>();
            rigidbody.AddForce(kicked.transform.forward * 300);

            Destroy(m_transformNew.Find("attach(Clone)").gameObject);
            StartCoroutine(CompleteReset());
        }

        public IEnumerator CompleteReset()
        {
            yield return new WaitForSeconds(0.6f);

            Jotunn.Logger.LogWarning("Complete rest of reset");
            Transform attachTransform = m_transformOld.Find("attach(Clone)");
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            attachTransform.SetParent(m_transformNew);
            m_animatorOld.Rebind();
            m_animatorOld.Update(0f);
            m_particleSystem.Play();

            GameObject prefab = PrefabManager.Instance.GetPrefab(m_dropOldItem);
            ImbuementRune imbuementRune = prefab.GetComponent<ImbuementRune>();
            string weaponTypeLower = m_imbuement.weaponType.ToLower();

            m_imbuement.description = Localization.instance.Localize($"${imbuementRune.m_descriptionKey}_{weaponTypeLower}");
            m_imbuement.level = imbuementRune.m_level;
            m_imbuement.name = Localization.instance.Localize($"${imbuementRune.m_nameKey}_{weaponTypeLower}");
            m_imbuement.prefab = prefab.name;
            m_imbuement.saved = true;
            m_imbuement.type = imbuementRune.m_type;
            m_imbuement.value = imbuementRune.m_value;

            m_imbuementTable.m_onRuneActivation.Invoke();

            Destroy(m_resetInteract);
            m_dropOldItem = null;
            m_itemNew = null;
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
            m_itemNew = null;

            if (m_imbuement.type != ImbuementType.None)
                m_particleSystem.Play();
            
            if (m_dropOldItem == null)
                return;

            GameObject prefab = PrefabManager.Instance.GetPrefab(m_dropOldItem);

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not drop old rune, prefab is null");
                return;
            }

            Transform oldAttachTransform = m_transformOld.GetChild(0);
            GameObject oldRune = Instantiate(prefab, oldAttachTransform.position, oldAttachTransform.rotation);
            Destroy(oldAttachTransform.gameObject);
            m_dropOldItem = null;
        }

        public void OnRemove()
        {
            if (m_itemNew == null)
                return;

            Player.m_localPlayer.GetInventory().AddItem(m_itemNew);
            m_itemNew = null;
        }

        public void StartMoveIn()
        {
            if (m_imbuement.type == ImbuementType.None)
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
