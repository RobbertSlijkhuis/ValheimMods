using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.GUI;
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
            //HandleDeleteBySafeZone(collider);
            //Jotunn.Logger.LogWarning($"Enter: {collider.gameObject.name}");
        }

        public void OnTriggerStay(Collider collider)
        {
            // HandlePlayer(collider, true);
            HandleCreatures(collider);
            // Jotunn.Logger.LogWarning($"Stay: {collider.gameObject.name}");
        }

        public void OnTriggerExit(Collider collider)
        {
            HandlePlayer(collider, false, "Player left");
            //Jotunn.Logger.LogWarning($"Leave: {collider.gameObject.name}");
        }

        private void HandlePlayer(Collider collider, bool value, string message = null)
        {
            if (transform.parent.gameObject.GetComponent<Ship>() != null && PluginConfig.configAllowRedeemsOnBoats.Value)
                return;

            if (collider.gameObject.name != playerIdentifier)
                return;

            Player player = collider.gameObject.GetComponent<Player>();

            if (player.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                m_customRewards.m_playerIsInSafeZone = value;

            if (message != null && message != "")
                Jotunn.Logger.LogWarning(message);

            if (value)
            {
                m_playerInZone = collider.gameObject;
                Game.instance.gameObject.GetComponent<SafeZoneHUDPanel>()?.Show();
            }
            else
            {
                m_playerInZone = null;
                Game.instance.gameObject.GetComponent<SafeZoneHUDPanel>()?.Hide();
            }
        }

        public void HandleDeleteBySafeZone(Collider collider)
        {
            if (transform.parent.gameObject.GetComponent<Ship>() != null && PluginConfig.configAllowRedeemsOnBoats.Value)
                return;

            TwitchDeleteBySafeZone deleteBySafeZone = collider.gameObject.GetComponent<TwitchDeleteBySafeZone>();

            if (deleteBySafeZone == null)
                return;

            ZNetView netView = collider.gameObject.GetComponent<ZNetView>();
            netView.Destroy();
            GameObject.Destroy(collider.gameObject);
        }

        public void HandleCreatures(Collider collider)
        {
            if (collider.gameObject.name != playerIdentifier)
            {
                if (!PluginConfig.configWardBurnCreatures.Value && !PluginConfig.configWardPushCreatures.Value)
                    return;

                if (transform.parent.gameObject.GetComponent<Ship>() != null && PluginConfig.configAllowRedeemsOnBoats.Value)
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

                if (humanoid != null && PluginConfig.configWardBurnCreatures.Value && !humanoid.GetSEMan().HaveStatusEffect(WizshBoneTwitchIntegration.Instance.effects.Burning.m_nameHash))
                {
                    float duration = 10f;
                    SE_Stats burning = Instantiate(WizshBoneTwitchIntegration.Instance.effects.Burning);
                    burning.m_healthPerTick = Mathf.Round(humanoid.GetMaxHealth() / (duration - 1f) * -1f);
                    burning.m_ttl = duration;
                    humanoid.GetSEMan().AddStatusEffect(burning);
                }

                if (humanoid != null)
                {
                    float force = PluginConfig.configWardPushForce.Value;
                    Vector3 pushDir = (collider.transform.position - transform.position).normalized;
                    pushDir.y = 0;
                    Rigidbody rb = collider.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        // rb.AddForce((transform.forward * -force) + (transform.up * force), ForceMode.Acceleration);
                        rb.AddForce(pushDir * force, ForceMode.Acceleration);
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (m_playerInZone == null)
                return;

            Player player = m_playerInZone.GetComponent<Player>();

            if (player.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
            {
                m_customRewards.m_playerIsInSafeZone = false;
                Game.instance.gameObject.GetComponent<SafeZoneHUDPanel>()?.Hide();
            }
        }
    }
}
