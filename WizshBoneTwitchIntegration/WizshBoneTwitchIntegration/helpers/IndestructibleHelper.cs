using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class IndestructibleHelper
    {
        public static bool ShouldProtect(GameObject target)
        {
            if (target.GetComponent<TwitchPiecePersistentData>() != null)
                return false;

            return (PluginConfig.configIndestructibleBoats.Value && target.GetComponent<Ship>() != null) ||
                (PluginConfig.configIndestructibleChests.Value && target.GetComponent<Container>() != null && target.GetComponent<Piece>()?.m_primaryTarget == true) ||
                (PluginConfig.configIndestructiblePortals.Value && target.GetComponent<TeleportWorld>() != null) ||
                (PluginConfig.configIndestructibleVegetables.Value && target.GetComponent<Plant>()?.m_needCultivatedGround == true);
        }
    }
}
