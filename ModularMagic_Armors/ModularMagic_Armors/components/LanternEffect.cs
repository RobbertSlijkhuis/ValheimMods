using ModularMagic_Armors.Helpers;
using System;
using UnityEngine;

namespace ModularMagic_Armors.Components
{
    /**
     * Put on the attach object (attach_skin / attach) of a lantern by LanternHelper.
     * VisEquipment instantiates that object on every client when the lantern is worn, so this runs on the wearer, for the local and the remote players.
     * It follows the lit state of the wearer and only touches the lantern when that state changes:
     * the light and demister (inside the 'equiped' child), the flare and the material of the lantern.
     * The children are found by the end of their name, so it does not matter which prefix they have in Unity.
     * Start is used and not OnEnable, because VisEquipment parents the attach object after activating it.
     */
    public class LanternEffect : MonoBehaviour
    {
        private const float PollInterval = 0.1f;

        private VisEquipment visEquipment;
        private LanternHelper.Lantern lantern;
        private GameObject equiped;
        private GameObject demister;
        private GameObject flare;
        private Renderer lanternRenderer;
        private bool? applied;

        private void Start()
        {
            try
            {
                visEquipment = GetComponentInParent<VisEquipment>();

                // Not worn by anyone (e.g. the item prefab itself), nothing to light
                if (visEquipment == null)
                    return;

                if (!LanternHelper.TryGetLantern(visEquipment.m_currentUtilityItemHash, out lantern))
                {
                    Jotunn.Logger.LogWarning($"[LanternEffect] Worn utility item of '{name}' is not a registered lantern");
                    return;
                }

                FindParts();

                // Starts at once, so the lantern never shows its prefab state
                InvokeRepeating(nameof(Refresh), 0f, PollInterval);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not start lantern effect of " + name + ": " + e);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void FindParts()
        {
            // GetComponentsInChildren also returns the inactive children, the 'equiped' child starts inactive
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform)
                    continue;

                if (child.name.EndsWith("equiped"))
                    equiped = child.gameObject;
                else if (child.name.EndsWith("Demister"))
                    demister = child.gameObject;
                else if (child.name.EndsWith("flare"))
                    flare = child.gameObject;
            }

            lanternRenderer = GetComponentInChildren<SkinnedMeshRenderer>(true);

            if (equiped == null || flare == null || lanternRenderer == null)
                Jotunn.Logger.LogWarning($"[LanternEffect] Lantern '{name}' is missing a part: equiped={equiped != null}, flare={flare != null}, renderer={lanternRenderer != null}");
        }

        private void Refresh()
        {
            if (visEquipment == null)
            {
                CancelInvoke();
                return;
            }

            bool lit = LanternHelper.IsLit(visEquipment);

            if (applied == lit)
                return;

            applied = lit;
            Apply(lit);
        }

        private void Apply(bool lit)
        {
            if (equiped != null)
                equiped.SetActive(lit);

            // The demister sits inside 'equiped', so it is only switched on when this lantern has one
            if (demister != null && lit)
                demister.SetActive(lantern.demister);

            if (flare != null)
                flare.SetActive(lit);

            SwapMaterial(lit ? lantern.onMaterial : lantern.offMaterial, lit ? lantern.offMaterial : lantern.onMaterial);
        }

        // sharedMaterials on this renderer only, the shared material assets are never changed
        private void SwapMaterial(Material to, Material from)
        {
            if (lanternRenderer == null || to == null || from == null)
                return;

            Material[] materials = lanternRenderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && SameMaterial(materials[i], from))
                {
                    materials[i] = to;
                    changed = true;
                }
            }

            if (changed)
                lanternRenderer.sharedMaterials = materials;
        }

        // The game may hand out instanced copies of a material, those are named '<name> (Instance)'
        private static bool SameMaterial(Material a, Material b)
        {
            return a == b || a.name.Replace(" (Instance)", "") == b.name.Replace(" (Instance)", "");
        }
    }
}
