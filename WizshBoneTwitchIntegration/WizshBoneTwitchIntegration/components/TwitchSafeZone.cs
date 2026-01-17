using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSafeZone : MonoBehaviour
    {
        private TwitchCustomRewards m_customRewards;
        private GameObject m_playerInZone;
        private readonly string playerIdentifier = "Player(Clone)";

        public void Awake()
        {
            m_customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
        }

        public void OnTriggerEnter(Collider collider)
        {
            HandlePlayer(collider, true, "Player entered");
        }

        public void OnTriggerStay(Collider collider)
        {
            HandlePlayer(collider, true);
            HandleSpawns(collider);
        }

        public void OnTriggerExit(Collider collider)
        {
            HandlePlayer(collider, false, "Player left");
        }

        private void HandlePlayer(Collider collider, bool value, string message = null)
        {
            if (collider.gameObject.name != playerIdentifier)
                return;

            Player player = collider.gameObject.GetComponent<Player>();

            if (player.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                m_customRewards.m_playerIsInSafeZone = value;

            if (value)
                m_playerInZone = collider.gameObject;
            else
                m_playerInZone = null;
        }

        public void HandleSpawns(Collider collider)
        {
            if (collider.gameObject.name != playerIdentifier)
            {
                if (!PluginConfig.configWardBurnCreatures.Value)
                    return;

                TwitchCreaturePersistentData persistentData = collider.gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData == null || persistentData.m_ignoreWard)
                    return;

                TwitchCreatureClaim creatureClaim = collider.gameObject.GetComponent<TwitchCreatureClaim>();

                if (creatureClaim == null)
                    return;

                Humanoid humanoid = collider.gameObject.GetComponent<Humanoid>();

                if (humanoid != null && humanoid.m_tamed)
                    return;

                if (!creatureClaim.m_isSpawn)
                    return;

                if (humanoid != null && !humanoid.GetSEMan().HaveStatusEffect(WizshBoneTwitchIntegration.Instance.effects.Burning.m_nameHash))
                {
                    float duration = 10f;
                    SE_Stats burning = Instantiate(WizshBoneTwitchIntegration.Instance.effects.Burning);
                    burning.m_healthPerTick = Mathf.RoundToInt(humanoid.GetMaxHealth() / (duration - 1f) * -1f);
                    burning.m_ttl = duration;
                    humanoid.GetSEMan().AddStatusEffect(burning);
                }
            }
        }

        public void OnDestroy()
        {
            if (m_playerInZone == null)
                return;

            Player player = m_playerInZone.GetComponent<Player>();

            if (player.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                m_customRewards.m_playerIsInSafeZone = false;
        }
    }
}
