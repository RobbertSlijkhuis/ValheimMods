using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Types;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Components
{
    /**
     * Put on the attach object (attach / attach_skin / attach_[bone]) of an armor piece by ArmorEffectSetup.
     * VisEquipment instantiates that object on every client when the piece is worn, but it is not parented to an animated bone itself
     * (attach_skin is bound to the skeleton through its mesh, and the armature inside it is a static copy).
     * So at Start the effects (children of this object that are known to ArmorEffectSetup) are moved to the real bones of the wearer,
     * and removed again when the piece is taken off.
     * Start is used and not OnEnable, because VisEquipment parents helmets after activating them.
     */
    public class ArmorEffectAttach : MonoBehaviour
    {
        private const float GateInterval = 0.25f;

        private static bool helmetJointLogged = false;

        private class AttachedEffect
        {
            public Transform target;
            public ArmorEffectSetup.Effect effect;
            public bool moved;
        }

        private readonly List<AttachedEffect> attached = new List<AttachedEffect>();
        private VisEquipment visEquipment;
        private bool hasGates = false;

        private void Start()
        {
            try
            {
                visEquipment = GetComponentInParent<VisEquipment>();

                // Not worn by anyone (e.g. the item prefab itself), nothing to attach to
                if (visEquipment == null)
                    return;

                LogHelmetJoint();

                Transform visualRoot = visEquipment.m_visual != null ? visEquipment.m_visual.transform : visEquipment.transform;

                // GetComponentsInChildren returns a copy, so the effects can be moved while going through it
                foreach (Transform child in GetComponentsInChildren<Transform>(true))
                {
                    if (child == transform || !ArmorEffectSetup.TryGetEffect(child.name, out ArmorEffectSetup.Effect effect))
                        continue;

                    bool moved = !string.IsNullOrEmpty(effect.bone);

                    if (moved && !MoveToBone(child, effect, visualRoot))
                    {
                        child.gameObject.SetActive(false);
                        continue;
                    }

                    if (effect.gateSet != ArmorSetId.None)
                        hasGates = true;

                    attached.Add(new AttachedEffect() { target = child, effect = effect, moved = moved });

                    bool active = IsActive(effect);
                    child.gameObject.SetActive(active);

                    // Temporary diagnostic
                    Jotunn.Logger.LogWarning($"[ArmorEffectAttach] {name}: '{child.name}' bone '{effect.bone}' parent '{(child.parent != null ? child.parent.name : "none")}' active {active} (gate {effect.gateSet}, {ArmorSetHelper.Describe(effect.gateSet, visEquipment)})");
                }

                // Temporary diagnostic
                Jotunn.Logger.LogWarning($"[ArmorEffectAttach] Start on '{name}': {attached.Count} effects attached");

                if (hasGates)
                    InvokeRepeating(nameof(UpdateGates), GateInterval, GateInterval);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not attach armor effects of " + name + ": " + e);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();

            // Moved effects are no longer children of this object, so they have to be removed here
            foreach (AttachedEffect item in attached)
            {
                if (item.moved && item.target != null)
                    Destroy(item.target.gameObject);
            }
        }

        private bool MoveToBone(Transform target, ArmorEffectSetup.Effect effect, Transform visualRoot)
        {
            Transform bone = TransformHelper.FindChildDepthFirst(visualRoot, effect.bone, transform);

            if (bone == null)
            {
                Jotunn.Logger.LogWarning($"[ArmorEffectAttach] Bone '{effect.bone}' not found for effect '{target.name}' on '{name}'");
                return false;
            }

            // Read before moving, a null rotation in the table means the rotation authored in the prefab
            Vector3 euler = effect.euler ?? target.localEulerAngles;

            target.SetParent(bone, false);
            target.localPosition = effect.position;
            target.localScale = effect.scale;

            // From the wearer: the rotation is set in the world, relative to the player, so the tilt of the bone does not matter. The effect then follows the bone
            if (effect.eulerFromWearer)
                target.rotation = visEquipment.transform.rotation * Quaternion.Euler(euler);
            else
                target.localRotation = Quaternion.Euler(euler);

            return true;
        }

        private void UpdateGates()
        {
            if (visEquipment == null)
                return;

            foreach (AttachedEffect item in attached)
            {
                if (item.target == null || item.effect.gateSet == ArmorSetId.None)
                    continue;

                bool active = IsActive(item.effect);

                if (item.target.gameObject.activeSelf != active)
                {
                    item.target.gameObject.SetActive(active);
                    Jotunn.Logger.LogWarning($"[ArmorEffectAttach] {name}: '{item.target.name}' active {active} ({ArmorSetHelper.Describe(item.effect.gateSet, visEquipment)})");
                }
            }
        }

        private bool IsActive(ArmorEffectSetup.Effect effect)
        {
            if (effect.gateSet == ArmorSetId.None)
                return true;

            return ArmorSetHelper.IsSetWorn(effect.gateSet, visEquipment);
        }

        // Temporary diagnostic: is m_helmet the Helmet_attach bone?
        private void LogHelmetJoint()
        {
            if (helmetJointLogged)
                return;

            helmetJointLogged = true;
            Jotunn.Logger.LogWarning($"[ArmorEffectAttach] VisEquipment.m_helmet = {(visEquipment.m_helmet != null ? visEquipment.m_helmet.name : "null")}");
        }
    }
}
