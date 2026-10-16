using ModularMagic_Armors.Types;
using System;

namespace ModularMagic_Armors.Helpers
{
    internal class BeardHelper
    {
        private static bool errorLogged = false;

        /**
         * The beard is only hidden visually, the beard stored on the player is never touched.
         * It is shown again for the local player while the barber is open, so the preview of the beard is visible.
         */
        public static bool ShouldHideBeard(VisEquipment visEquipment)
        {
            try
            {
                if (visEquipment == null)
                    return false;

                if (PlayerCustomizaton.IsBarberGuiVisible() && IsLocalPlayer(visEquipment))
                    return false;

                return ArmorSetHelper.IsSetWorn(ArmorSetId.Wraith, visEquipment);
            }
            catch (Exception e)
            {
                // This runs every frame, only log the first error
                if (!errorLogged)
                {
                    errorLogged = true;
                    Jotunn.Logger.LogError("Could not check if the beard should be hidden: " + e);
                }

                return false;
            }
        }

        private static bool IsLocalPlayer(VisEquipment visEquipment)
        {
            return Player.m_localPlayer != null && visEquipment.GetComponentInParent<Player>() == Player.m_localPlayer;
        }
    }
}
