using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class IndestructibleHelper
    {
        public static bool ShouldProtect(GameObject target)
        {
            if (target.GetComponent<TwitchPiecePersistentData>() != null)
                return false;

            return (ProfileSettingsHelper.Current.indestructibleBoats && target.GetComponent<Ship>() != null) ||
                (ProfileSettingsHelper.Current.indestructibleChests && target.GetComponent<Container>() != null && target.GetComponent<Piece>()?.m_primaryTarget == true) ||
                (ProfileSettingsHelper.Current.indestructiblePortals && target.GetComponent<TeleportWorld>() != null) ||
                (ProfileSettingsHelper.Current.indestructibleVegetables && target.GetComponent<Plant>()?.m_needCultivatedGround == true);
        }
    }
}
