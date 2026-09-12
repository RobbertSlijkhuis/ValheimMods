using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreatureInteract : MonoBehaviour, Hoverable, Interactable
    {
        private TwitchCreatureClaim m_creatureClaim;
        private Humanoid m_humanoid;

        public void Awake()
        {
            try
            {
                m_creatureClaim = gameObject.GetComponent<TwitchCreatureClaim>();
                m_humanoid = gameObject.GetComponent<Humanoid>();
                m_humanoid.SetTamed(true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchCreatureInteract.Awake failed: " + e);
            }
        }

        public string GetHoverText()
        {
            string inputString = Localization.instance.Localize("[<color=yellow>$KEY_Use</color>]");
            string text = $"{inputString} Listen to a random history fact";
            return text;
        }

        public string GetHoverName()
        {
            return m_humanoid.m_name;
        }

        public float GetHoverOffset()
        {
            return 0f;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            try
            {
                if (m_creatureClaim == null || hold || alt)
                    return false;

                m_creatureClaim.SayAMessage();
                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while interacting with a creature: " + e);
                return false;
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
