using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    internal class RuneTableAccept : MonoBehaviour, Hoverable, Interactable
    {
        public RuneTable m_imbuementTable;
        public Material m_tableMat;
        private float m_emissionDuration = 1.5f;
        private IEnumerator m_emission;

        public void Awake()
        {
            m_imbuementTable = transform.parent.parent.gameObject.GetComponent<RuneTable>();
            m_imbuementTable.m_onItemAttach.AddListener(EmissionStart);
            m_imbuementTable.m_onItemRemove.AddListener(EmissionStop);
            m_imbuementTable.m_onRuneActivation.AddListener(EmissionUpdate);
            m_imbuementTable.m_onSave.AddListener(OnSave);
            m_tableMat = gameObject.GetComponent<MeshRenderer>().materials[0];
        }

        public string GetHoverText()
        {
            if (m_imbuementTable.m_imbuements.Count == 0)
                return "Need magical item to imbue";

            string canSave = m_imbuementTable.CanSave();
            string inputString = canSave == CanImbueType.Yes ? Localization.instance.Localize("[<color=yellow>$KEY_Use</color>] ") : "";
            string message = "";

            switch (canSave)
            {
                case nameof(CanImbueType.Yes):
                    message = "Apply changes";
                    break;
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

        public void OnSave()
        {
            EmissionUpdate();
            ParticleSystem partComp = gameObject.GetComponent<ParticleSystem>();
            partComp.Play();
        }

        public void EmissionStart()
        {
            Animator animator = gameObject.GetComponent<Animator>();
            animator.SetTrigger("Start");
            EmissionUpdate();
        }

        public void EmissionStop()
        {
            Animator animator = gameObject.GetComponent<Animator>();
            animator.SetTrigger("End");

            Color fromColor = m_tableMat.GetColor("_EmissionColor");
            Color toColor = new Color(0f, 0f, 0f, 1f);
            m_emission = LerpHelper.LerpColor(m_tableMat, fromColor, toColor, m_emissionDuration);
            StartCoroutine(m_emission);
        }

        public void EmissionUpdate()
        {
            if (m_emission != null)
                StopCoroutine(m_emission);

            LightPresetColors colors;
            string canImbue = m_imbuementTable.CanSave();

            if (canImbue == CanImbueType.Yes)
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Green);
            else if (canImbue == CanImbueType.NoChange)
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Yellow);
            else
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Red);

            Color fromColor = m_tableMat.GetColor("_EmissionColor");
            Color toColor = colors.emissionColor;
            m_emission = LerpHelper.LerpColor(m_tableMat, fromColor, toColor, m_emissionDuration);
            StartCoroutine(m_emission);
        }
    }
}
