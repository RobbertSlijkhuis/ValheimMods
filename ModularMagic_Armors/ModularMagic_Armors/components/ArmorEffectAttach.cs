using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Types;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Components
{
    /**
     * One effect on an armor piece. Authored in Unity, keep the field names stable, they are serialized in the prefabs.
     */
    [Serializable]
    public class EffectBinding
    {
        // The effect object, authored inactive
        public Transform target;

        // Name of the real (animated) bone the effect is moved to. Empty = the effect stays where it is and is only switched on/off (e.g. a child of another effect)
        public string bone;

        // None = the effect is on as long as the piece is worn, otherwise only while the full set is worn
        public ArmorSetId gateSet = ArmorSetId.None;

        // Use the local transform below instead of the one authored in the prefab (used for effects injected from code)
        public bool overrideLocal;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEuler = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }

    /**
     * Put on the attach object (attach / attach_skin / attach_[bone]) of an armor piece.
     * VisEquipment instantiates that object on every client when the piece is worn, but it is not parented to an animated bone itself
     * (attach_skin is bound to the skeleton through its mesh, and the armature inside it is a static copy).
     * So at Start the effects are moved to the real bones of the wearer, and removed again when the piece is taken off.
     * Start is used and not OnEnable, because VisEquipment parents helmets after activating them.
     */
    public class ArmorEffectAttach : MonoBehaviour
    {
        private const float GateInterval = 0.25f;

        private static bool helmetJointLogged = false;

        public List<EffectBinding> bindings = new List<EffectBinding>();

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

                foreach (EffectBinding binding in bindings)
                {
                    if (binding == null || binding.target == null)
                        continue;

                    if (!string.IsNullOrEmpty(binding.bone) && !MoveToBone(binding, visualRoot))
                    {
                        binding.target.gameObject.SetActive(false);
                        continue;
                    }

                    if (binding.gateSet != ArmorSetId.None)
                        hasGates = true;

                    bool active = IsActive(binding);
                    binding.target.gameObject.SetActive(active);

                    // Temporary diagnostic
                    Jotunn.Logger.LogWarning($"[ArmorEffectAttach] {name}: '{binding.target.name}' bone '{binding.bone}' parent '{(binding.target.parent != null ? binding.target.parent.name : "none")}' active {active} (gate {binding.gateSet}, {ArmorSetHelper.Describe(binding.gateSet, visEquipment)})");
                }

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
            foreach (EffectBinding binding in bindings)
            {
                if (binding == null || binding.target == null || string.IsNullOrEmpty(binding.bone))
                    continue;

                Destroy(binding.target.gameObject);
            }
        }

        private bool MoveToBone(EffectBinding binding, Transform visualRoot)
        {
            Transform bone = TransformHelper.FindChildDepthFirst(visualRoot, binding.bone, transform);

            if (bone == null)
            {
                Jotunn.Logger.LogWarning($"[ArmorEffectAttach] Bone '{binding.bone}' not found for effect '{binding.target.name}' on '{name}'");
                return false;
            }

            binding.target.SetParent(bone, false);

            if (binding.overrideLocal)
            {
                binding.target.localPosition = binding.localPosition;
                binding.target.localRotation = Quaternion.Euler(binding.localEuler);
                binding.target.localScale = binding.localScale;
            }

            return true;
        }

        private void UpdateGates()
        {
            if (visEquipment == null)
                return;

            foreach (EffectBinding binding in bindings)
            {
                if (binding == null || binding.target == null || binding.gateSet == ArmorSetId.None)
                    continue;

                bool active = IsActive(binding);

                if (binding.target.gameObject.activeSelf != active)
                {
                    binding.target.gameObject.SetActive(active);
                    Jotunn.Logger.LogWarning($"[ArmorEffectAttach] {name}: '{binding.target.name}' active {active} ({ArmorSetHelper.Describe(binding.gateSet, visEquipment)})");
                }
            }
        }

        private bool IsActive(EffectBinding binding)
        {
            if (binding.gateSet == ArmorSetId.None)
                return true;

            return ArmorSetHelper.IsSetWorn(binding.gateSet, visEquipment);
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
