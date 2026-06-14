using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSafeZone : MonoBehaviour
    {
        private TwitchCustomRewards m_customRewards;
        private GameObject m_playerInZone;
        private readonly string playerIdentifier = "Player(Clone)";

        // Tracks how many safe zones the local player is currently standing in,
        // so overlapping zones don't toggle the HUD/flag off while still in another zone.
        private static int s_localPlayerZoneCount = 0;

        // Called from GameAwake_Postfix so a fresh world load doesn't inherit stale state.
        public static void ResetLocalPlayerZoneCount()
        {
            s_localPlayerZoneCount = 0;
        }

        public void Awake()
        {
            try
            {
                m_customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.Awake failed: " + e);
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            try
            {
                HandlePlayer(collider, true, "Player entered");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnTriggerEnter failed: " + e);
            }
        }

        public void OnTriggerStay(Collider collider)
        {
            try
            {
                HandleCreatures(collider);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnTriggerStay failed: " + e);
            }
        }

        public void OnTriggerExit(Collider collider)
        {
            try
            {
                HandlePlayer(collider, false, "Player left");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnTriggerExit failed: " + e);
            }
        }

        private void HandlePlayer(Collider collider, bool value, string message = null)
        {
            if (transform.parent.gameObject.GetComponent<Ship>() != null && PluginConfig.configAllowRedeemsOnBoats.Value)
                return;

            if (collider.gameObject.name != playerIdentifier)
                return;

            Player player = collider.gameObject.GetComponent<Player>();

            if (message != null && message != "")
                Jotunn.Logger.LogWarning(message);

            m_playerInZone = value ? collider.gameObject : null;

            if (player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                return;

            if (value)
                EnterLocalPlayerZone();
            else
                ExitLocalPlayerZone();
        }

        private void EnterLocalPlayerZone()
        {
            s_localPlayerZoneCount++;

            if (s_localPlayerZoneCount == 1)
            {
                m_customRewards.m_playerIsInSafeZone = true;
                Game.instance.gameObject.GetComponent<SafeZoneHUDPanel>()?.Show();
            }
        }

        private void ExitLocalPlayerZone()
        {
            if (s_localPlayerZoneCount > 0)
                s_localPlayerZoneCount--;

            if (s_localPlayerZoneCount == 0)
            {
                m_customRewards.m_playerIsInSafeZone = false;
                Game.instance.gameObject.GetComponent<SafeZoneHUDPanel>()?.Hide();
            }
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
            try
            {
                if (m_playerInZone == null)
                    return;

                // Player.m_localPlayer is null during the death/respawn window
                if (Player.m_localPlayer == null)
                    return;

                Player player = m_playerInZone.GetComponent<Player>();

                if (player == null)
                    return;

                if (player.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                {
                    ExitLocalPlayerZone();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnDestroy failed: " + e);
            }
        }
    }
}
