using Jotunn.Managers;
using ModularMagic_Core.Components;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using UnityEngine;
using static ItemDrop;
using Random = UnityEngine.Random;

namespace ModularMagic_Core.components
{
    internal class ImbuementTableInteract : MonoBehaviour, Hoverable, Interactable
    {
        public ImbuementTable m_imbuementTable;
        public Imbuement m_imbuement;
        public int m_index;

        public ItemData m_itemNew;
        public string m_dropOldItem;

        public float m_durationMoveIn = 0.5f;
        private float m_durationIdle = 5f;
        private Vector3 m_origin;
        private Vector3 m_startIdle;
        private float m_minIdle = -0.02f;
        private float m_maxIdle = 0.02f;

        public void Init(Imbuement imbuement, int index)
        {
            m_imbuement = imbuement;
            m_imbuementTable = transform.parent.parent.gameObject.GetComponent<ImbuementTable>();
            m_imbuementTable.m_onSave.AddListener(OnSave);
            m_imbuementTable.m_onItemRemove.AddListener(OnRemove);
            m_index = index;

            m_origin = transform.localPosition;
            transform.localRotation = TransformHelper.GenerateRotation(new Vector3(Random.Range(265, 275), Random.Range(-5f, 5), Random.Range(265, 275)));
            transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);

            GameObject prefab = PrefabManager.Instance.GetPrefab(m_imbuement.prefab);
            UpdateRune(prefab);

            Invoke(nameof(StartMoveIn), m_durationMoveIn);
            InvokeRepeating(nameof(StartIdle), m_durationMoveIn + 0.5f, m_durationIdle + 1f);
        }

        private void UpdateRune(GameObject prefab)
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (prefab == null)
            {
                Transform attachCloneTransform = transform.Find("attach(Clone)");

                if (attachCloneTransform == null)
                    return;

                if (m_dropOldItem != null)
                {
                    //Jotunn.Logger.LogWarning($"Looking for: removed_pile/removed_{m_index + 1}");
                    //Transform removedTransform = m_imbuementTable.transform.Find($"removed_pile/removed_{m_index + 1}");

                    //if (removedTransform == null)
                    //{
                    //    Jotunn.Logger.LogWarning("Could not find removed rune to copy to");
                    //    return;
                    //}

                    //if (removedTransform.childCount > 0)
                    //{
                    //    Destroy(attachCloneTransform.gameObject);
                    //}
                    //else
                    //{
                    //    attachCloneTransform.SetParent(removedTransform);
                    //    attachCloneTransform.localPosition = new Vector3(0, 0, 0);
                    //    attachCloneTransform.localRotation = TransformHelper.GenerateRotation(new Vector3(0, 0, 0));
                    //}

                    if (transform.childCount == 1)
                    {
                        Transform oldAttachTransform = transform.GetChild(0);
                        Transform oldRuneTransform = oldAttachTransform.Find($"rune_{m_imbuement.level}");
                        HoverText hoverText = oldRuneTransform.gameObject.AddComponent<HoverText>();
                        oldAttachTransform.localPosition = new Vector3(0f, 0f, -40f);
                        oldRuneTransform.gameObject.GetComponent<BoxCollider>().enabled = false;
                        hoverText.m_text = "This rune is to be removed on save!";

                    }
                    else if (transform.childCount == 2)
                    {
                        Destroy(transform.GetChild(1).gameObject);
                    }
                }
                else
                {
                    Destroy(attachCloneTransform.gameObject);
                }

                meshRenderer.enabled = true;
                return;
            }

            Transform attachTransform = prefab.transform.Find("attach");

            if (attachTransform == null)
            {
                Jotunn.Logger.LogError("Could not find attach for rune");
                return;
            }

            GameObject attached = Instantiate(attachTransform.gameObject, transform);
            Transform runeTransform = attached.transform.Find($"rune_{m_imbuement.level}");

            if (runeTransform == null)
            {
                Jotunn.Logger.LogError("Could not find the rune transform");
                return;
            }

            runeTransform.localRotation = TransformHelper.GenerateRotation(new Vector3(0, 0, 0));
            runeTransform.localScale = new Vector3(1f, 1f, 1f);
            meshRenderer.enabled = false;
        }

        public string GetHoverText()
        {
            if (m_imbuement == null)
                return "Something went wrong, contact author pls";

            if (m_imbuement.type == ImbuementType.None)
            {
                string imbuementNameEmpty = $"<color=yellow>{m_imbuement.name}</color>\n";
                string inputItem = "[<color=yellow>1-8</color>] Attach item";
                return imbuementNameEmpty + inputItem;
            }

            string imbuementName = $"<color=green>{m_imbuement.name} {(m_imbuement.level > 0 ? m_imbuement.level : "")}</color>\n";
            string description = m_imbuement.description + "\n";
            string inputUse = Localization.instance.Localize($"[<color=yellow>$KEY_Use</color>] Remove item");
            return imbuementName + description + inputUse;
        }

        public string GetHoverName()
        {
            return PluginConfig.piece1.name.Value;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
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
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Already slotted a rune of the same type!");
                return false;
            }

            if (!imbuementRune.m_allowedWeapons.Contains(m_imbuement.weaponType))
                return false;

            if (!m_imbuement.allowSecondary && imbuementRune.m_type == ImbuementType.SecondaryAttack)
                return false;

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

            UpdateRune(prefab);

            return true;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (alt || !hold || m_imbuement.type == ImbuementType.None)
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

            UpdateRune(null);

            return true;
        }

        public void OnSave()
        {
            m_itemNew = null;
            
            if (m_dropOldItem == null)
                return;

            GameObject prefab = PrefabManager.Instance.GetPrefab(m_dropOldItem);

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not drop old rune, prefab is null");
                return;
            }

            Transform oldAttachTransform = transform.GetChild(0);
            GameObject oldRune = Instantiate(prefab, oldAttachTransform.position, oldAttachTransform.rotation);


            //Transform attachTransform = m_imbuementTable.transform.Find($"removed_pile/removed_{m_index + 1}/attach(Clone)");

            //if (attachTransform == null)
            //{
            //    Jotunn.Logger.LogWarning("Could not find removed rune to destroy!");
            //    return;
            //}

            //GameObject oldRune = Instantiate(prefab, attachTransform.position, attachTransform.rotation);
            ImbuementRune imbuementRune = oldRune.GetComponent<ImbuementRune>();
            imbuementRune.m_charged = false;
            // Destroy(attachTransform.gameObject);
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

        public void Destroy()
        {
            CancelInvoke(nameof(StartIdle));
            Invoke(nameof(StartMoveOut), m_durationMoveIn);
        }

        public void StartMoveIn()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = ImbuementHelper.CalculatePosition(m_index);
            m_startIdle = end;
            StartCoroutine(LerpHelper.SlerpPosition(transform, start, end, m_durationMoveIn));
        }

        public void StartMoveOut()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = m_origin;
            StartCoroutine(LerpHelper.SlerpPosition(transform, start, end, m_durationMoveIn, gameObject, true));
        }

        public void StartIdle()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = LerpHelper.RandomPosition(m_startIdle, m_minIdle, m_maxIdle);
            StartCoroutine(LerpHelper.SlerpPosition(transform, start, end, m_durationIdle));
        }
    }
}
