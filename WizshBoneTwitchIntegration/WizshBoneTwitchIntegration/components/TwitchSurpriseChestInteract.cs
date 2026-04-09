using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSurpriseChestInteract : MonoBehaviour, Hoverable, Interactable
    {
        public TwitchSurpriseChest m_surpriseChest;
        public Animator m_animator;

        public void Awake()
        {
            m_surpriseChest = transform.parent.gameObject.GetComponent<TwitchSurpriseChest>();
            m_animator = gameObject.GetComponent<Animator>();
        }

        public string GetHoverText()
        {
            if (!m_surpriseChest.m_interact)
                return "";

            string inputString = Localization.instance.Localize("[<color=yellow>$KEY_Use</color>]");
            string text = $"{inputString} Open";
            return text;
        }

        public string GetHoverName()
        {
            return $"Surprise {m_surpriseChest.m_type} Chest";
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            try
            {
                if (!m_surpriseChest.m_interact)
                    return false;

                if (hold || alt)
                    return false;

                m_surpriseChest.Open();
                return true;
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
