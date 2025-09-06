using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    internal class ImbuementTableAccept : MonoBehaviour, Hoverable, Interactable
    {
        public ImbuementTable m_imbuementTable;

        private void Awake()
        {
            m_imbuementTable = transform.parent.parent.gameObject.GetComponent<ImbuementTable>();
            m_imbuementTable.m_onItemAttach.AddListener(InitEmission);
            m_imbuementTable.m_onItemRemove.AddListener(ResetEmission);
            m_imbuementTable.m_onRuneActivation.AddListener(UpdateEmission);
            m_imbuementTable.m_onSave.AddListener(UpdateEmission);
        }

        public string GetHoverText()
        {
            if (m_imbuementTable.m_imbuements.Count == 0)
                return "Need magical item to imbue";

            string canImbue = m_imbuementTable.CanImbue();
            string inputString = canImbue == CanImbueType.Yes ? Localization.instance.Localize("[<color=yellow>$KEY_Use</color>] ") : "";
            string message = "";

            switch (canImbue)
            {
                case nameof(CanImbueType.Yes):
                    message = "Apply imbuements";
                    break;
                //case nameof(CanImbueType.No):
                //    message = "Requires " + m_imbuementTable.CountMaterialRequired() + " Magic powder";
                //    break;
                case nameof(CanImbueType.NoChange):
                    message = "No changes to apply";
                    break;
                default:
                    message = "Something went wrong, contact author pls";
                    break;
            }

            return inputString + message;
        }
        public string GetHoverName()
        {
            return PluginConfig.piece1.name.Value;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
                return false;

            return m_imbuementTable.Save();
        }

        private void UpdateEmission()
        {
            LightPresetColors colors;
            MeshRenderer meshComp = gameObject.GetComponent<MeshRenderer>();
            string canImbue = m_imbuementTable.CanImbue();
            meshComp.materials = new Material[1] { ModularMagic_Core.Instance.materials.SpellBook };

            if (canImbue == CanImbueType.Yes)
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Green);
            else if (canImbue == CanImbueType.NoChange)
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Yellow);
            else
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Red);

            Material mat = meshComp.materials[0];
            mat.SetColor("_EmissionColor", colors.emissionColor);
        }

        private void InitEmission()
        {
            LightPresetColors colors = LightColorPresetHelper.GetColors(LightColorPresetType.Yellow);
            MeshRenderer meshComp = gameObject.GetComponent<MeshRenderer>();
            meshComp.materials = new Material[1] { ModularMagic_Core.Instance.materials.SpellBook };

            Material mat = meshComp.materials[0];
            mat.SetColor("_EmissionColor", colors.emissionColor);
        }

        private void ResetEmission()
        {
            MeshRenderer meshComp = gameObject.GetComponent<MeshRenderer>();
            meshComp.materials = new Material[1] { ModularMagic_Core.Instance.materials.SpellBookOff };
        }
    }
}
