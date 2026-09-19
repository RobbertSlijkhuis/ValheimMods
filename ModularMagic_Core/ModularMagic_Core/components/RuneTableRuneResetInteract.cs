using ModularMagic_Core.Configs;
using UnityEngine;

namespace ModularMagic_Core.components
{
    internal class RuneTableRuneResetInteract : MonoBehaviour, Hoverable, Interactable
    {
        RuneTableRuneInteract m_tableInteract;

        public void Awake()
        {
            m_tableInteract = transform.parent.parent.gameObject.GetComponent<RuneTableRuneInteract>();
        }

        public string GetHoverText()
        {
            if (m_tableInteract.m_locked)
                return "Waiting for action to finish...";

            string imbuementName = "<color=yellow>This rune will be removed on save</color>\n";
            string inputUse = Localization.instance.Localize("[<color=yellow>$ui_hold $KEY_Use</color>] Reset");
            return imbuementName + inputUse;
        }

        public string GetHoverName()
        {
            return PluginConfig.piece1.name.Value;
        }

        public float GetHoverOffset()
        {
            return 0f;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (alt || !hold || m_tableInteract.m_locked)
                return false;

            m_tableInteract.Reset();
            return true;
        }
    }
}
