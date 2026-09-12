using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSurpriseChestInteract : MonoBehaviour, Hoverable, Interactable
    {
        public TwitchSurpriseChest m_surpriseChest;
        public Animator m_animator;

        public void Awake()
        {
            try
            {
                m_surpriseChest = transform.parent.gameObject.GetComponent<TwitchSurpriseChest>();
                m_animator = gameObject.GetComponent<Animator>();

                if (m_surpriseChest == null)
                    Jotunn.Logger.LogError("Could not find TwitchSurpriseChest on parent of TwitchSurpriseChestInteract!");

                if (m_animator == null)
                    Jotunn.Logger.LogError("Could not find Animator on TwitchSurpriseChestInteract!");
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchSurpriseChestInteract.Awake failed: " + e);
            }
        }

        public string GetHoverText()
        {
            if (!m_surpriseChest.m_interact || m_surpriseChest.IsOpened)
                return "";

            string inputString = Localization.instance.Localize("[<color=yellow>$KEY_Use</color>]");
            return $"{inputString} Open";
        }

        public string GetHoverName()
        {
            return $"Surprise {m_surpriseChest.m_type} Chest";
        }

        public float GetHoverOffset()
        {
            return 0f;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            try
            {
                if (hold || alt)
                    return false;

                if (!m_surpriseChest.m_interact || m_surpriseChest.IsOpened)
                    return false;

                return m_surpriseChest.TryTriggerOpen();
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong while interacting with a surprise chest: " + e);
                return false;
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
