using ModularMagic_Armors.Helpers;
using ModularMagic_Armors.Types;
using System;
using UnityEngine;

namespace ModularMagic_Armors.Components
{
    /**
     * Hides the body mesh of the wearer while the full set is worn (used by the Wraith set).
     * Put on the attach object of one piece of the set. The body is shown again when the set breaks or the piece is taken off.
     * The body object is switched off (not only its renderer), because the LODGroup of the player toggles the renderer by itself.
     */
    public class SetBodyHider : MonoBehaviour
    {
        private const float CheckInterval = 0.25f;

        public ArmorSetId set = ArmorSetId.None;

        private VisEquipment visEquipment;
        private bool hidden = false;

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
                bool hide = ArmorSetHelper.IsSetWorn(set, visEquipment);

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

        private void SetBodyVisible(bool visible)
        {
            if (visEquipment != null && visEquipment.m_bodyModel != null)
                visEquipment.m_bodyModel.gameObject.SetActive(visible);
        }
    }
}
