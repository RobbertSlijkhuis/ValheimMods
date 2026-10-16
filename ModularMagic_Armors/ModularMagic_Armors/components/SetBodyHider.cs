using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Types;
using System;
using UnityEngine;

namespace ModularMagic_Armors.Components
{
    /**
     * Hides the body mesh of the wearer while the full Wraith set is worn.
     * Added to the Wraith chest by ArmorEffectSetup. The body is shown again when the set breaks or the piece is taken off.
     * The body renderer is not switched off with enabled (the LODGroup of the player toggles that by itself) and its object is not switched off either
     * (that stopped the animations of the player at times), only its rendering is forced off.
     * The set is a constant and not a field, because values stored on a component are lost when the game copies the piece onto the wearer.
     */
    public class SetBodyHider : MonoBehaviour
    {
        private const float CheckInterval = 0.25f;
        private const ArmorSetId HiddenForSet = ArmorSetId.Wraith;
        private const string BodyName = "Visual/body";

        private VisEquipment visEquipment;
        private bool hidden = false;
        private bool cullingChanged = false;
        private AnimatorCullingMode previousCullingMode;

        private void Start()
        {
            visEquipment = GetComponentInParent<VisEquipment>();

            if (visEquipment == null)
                return;

            UpdateBody();
            InvokeRepeating(nameof(UpdateBody), CheckInterval, CheckInterval);
        }

        private void OnDestroy()
        {
            CancelInvoke();

            if (hidden)
                SetBodyVisible(true);
        }

        private void UpdateBody()
        {
            try
            {
                bool hide = ArmorSetHelper.IsSetWorn(HiddenForSet, visEquipment);

                if (hide == hidden)
                    return;

                hidden = hide;
                SetBodyVisible(!hide);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update the body visibility: " + e);
                CancelInvoke();
            }
        }

        // VisEquipment sits on the player root, so this is the same Visual/body path the old code used
        private void SetBodyVisible(bool visible)
        {
            if (visEquipment == null)
                return;

            Transform body = visEquipment.transform.Find(BodyName);

            if (body == null)
            {
                Jotunn.Logger.LogWarning($"[SetBodyHider] No '{BodyName}' object found under '{visEquipment.name}'");
                return;
            }

            Renderer renderer = body.GetComponent<Renderer>();

            if (renderer == null)
            {
                Jotunn.Logger.LogWarning($"[SetBodyHider] '{BodyName}' has no renderer");
                return;
            }

            // The object stays active: switching it off stopped the animations of the player
            renderer.forceRenderingOff = !visible;

            SetAnimatorCulling(!visible);
        }

        // The Animator of the player is set to CullUpdateTransforms: it stops updating the skeleton when none of its renderers is visible.
        // With the body hidden that happens at times (the armor meshes can be culled on their own), so the Animator always animates while the body is hidden
        private void SetAnimatorCulling(bool alwaysAnimate)
        {
            Animator animator = visEquipment.GetComponentInChildren<Animator>();

            if (animator == null)
                return;

            if (alwaysAnimate && !cullingChanged)
            {
                previousCullingMode = animator.cullingMode;
                cullingChanged = true;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            else if (!alwaysAnimate && cullingChanged)
            {
                cullingChanged = false;
                animator.cullingMode = previousCullingMode;
            }
        }
    }
}
