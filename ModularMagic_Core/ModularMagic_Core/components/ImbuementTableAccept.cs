using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System.Collections;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    internal class ImbuementTableAccept : MonoBehaviour, Hoverable, Interactable
    {
        public ImbuementTable m_imbuementTable;
        public Material m_tableMat;
        private Vector3 m_spellbookStartIdle;
        private Vector3 m_spellbookEndIdle;
        private float m_spellbookDurationIdle = 10f;
        private float m_emissionDuration = 1.5f;
        private float m_rotationDuration = 0.75f;
        private bool m_spellbookReverseIdle = true;
        private IEnumerator m_spellbookIdle;

        private void Awake()
        {
            m_imbuementTable = transform.parent.parent.gameObject.GetComponent<ImbuementTable>();
            m_imbuementTable.m_onItemAttach.AddListener(EmissionStart);
            m_imbuementTable.m_onItemRemove.AddListener(EmissionStop);
            m_imbuementTable.m_onRuneActivation.AddListener(EmissionStart);
            m_imbuementTable.m_onSave.AddListener(EmissionStart);
            m_tableMat = gameObject.GetComponent<MeshRenderer>().materials[0];

            //m_spellbookStartIdle = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            //m_spellbookEndIdle = new Vector3(transform.localPosition.x, transform.localPosition.y + 0.06f, transform.localPosition.z);

            m_spellbookStartIdle = new Vector3(0.402f, 1.223f, 0.893f);
            m_spellbookEndIdle = new Vector3(m_spellbookStartIdle.x, m_spellbookStartIdle.y + 0.06f, m_spellbookStartIdle.z);
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

        //private void UpdateEmission()
        //{
        //    LightPresetColors colors;
        //    MeshRenderer meshComp = gameObject.GetComponent<MeshRenderer>();
        //    string canImbue = m_imbuementTable.CanImbue();
        //    meshComp.materials = new Material[1] { ModularMagic_Core.Instance.materials.SpellBook };

        //    if (canImbue == CanImbueType.Yes)
        //        colors = LightColorPresetHelper.GetColors(LightColorPresetType.Green);
        //    else if (canImbue == CanImbueType.NoChange)
        //        colors = LightColorPresetHelper.GetColors(LightColorPresetType.Yellow);
        //    else
        //        colors = LightColorPresetHelper.GetColors(LightColorPresetType.Red);

        //    Material mat = meshComp.materials[0];
        //    mat.SetColor("_EmissionColor", colors.emissionColor);
        //}

        //private void InitEmission()
        //{
        //    LightPresetColors colors = LightColorPresetHelper.GetColors(LightColorPresetType.Yellow);
        //    MeshRenderer meshComp = gameObject.GetComponent<MeshRenderer>();
        //    meshComp.materials = new Material[1] { ModularMagic_Core.Instance.materials.SpellBook };

        //    Material mat = meshComp.materials[0];
        //    mat.SetColor("_EmissionColor", colors.emissionColor);
        //}

        //private void ResetEmission()
        //{
        //    MeshRenderer meshComp = gameObject.GetComponent<MeshRenderer>();
        //    meshComp.materials = new Material[1] { ModularMagic_Core.Instance.materials.SpellBookOff };
        //}

        public void EmissionStart()
        {
            LightPresetColors colors;
            string canImbue = m_imbuementTable.CanImbue();

            if (canImbue == CanImbueType.Yes)
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Green);
            else if (canImbue == CanImbueType.NoChange)
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Yellow);
            else
                colors = LightColorPresetHelper.GetColors(LightColorPresetType.Red);

            Color fromColor = m_tableMat.GetColor("_EmissionColor");
            Color toColor = colors.emissionColor;
            StartCoroutine(LerpHelper.LerpColor(m_tableMat, fromColor, toColor, m_emissionDuration));

            // Vector3 fromPos = new Vector3(0.354f, 1.054f, 0.897f);
            Vector3 fromPos = transform.localPosition;
            Vector3 toPos = new Vector3(0.402f, 1.223f, 0.893f);
            Vector3 fromRot = new Vector3(28.607f, -196.08f, -90.506f);
            //Vector3 fromRot = transform.localRotation.eulerAngles;
            Vector3 toRot = new Vector3(0f, -145f, -35f);
            StartCoroutine(LerpHelper.LerpPositionAndRotation(transform, fromPos, toPos, fromRot, toRot, m_rotationDuration));
            InvokeRepeating(nameof(SpellbookIdle), m_emissionDuration + 0.1f, m_spellbookDurationIdle + 0.1f);
        }

        public void EmissionStop()
        {
            CancelInvoke(nameof(SpellbookIdle));
            StopCoroutine(m_spellbookIdle);

            Color fromColor = m_tableMat.GetColor("_EmissionColor");
            Color toColor = new Color(0f, 0f, 0f, 1f);
            StartCoroutine(LerpHelper.LerpColor(m_tableMat, fromColor, toColor, m_emissionDuration));

            Vector3 toPos = new Vector3(0.354f, 1.054f, 0.897f);
            //Vector3 fromPos = new Vector3(0.402f, 1.223f, 0.893f);
            Vector3 fromPos = transform.localPosition;
            Vector3 toRot = new Vector3(28.607f, -196.08f, -90.506f);
            Vector3 fromRot = new Vector3(0f, -145f, -35f);
            //Vector3 fromRot = transform.localRotation.eulerAngles;
            StartCoroutine(LerpHelper.LerpPositionAndRotation(transform, fromPos, toPos, fromRot, toRot, m_rotationDuration));
        }

        public void SpellbookIdle()
        {
            m_spellbookReverseIdle = !m_spellbookReverseIdle;
            Vector3 start = m_spellbookReverseIdle ? m_spellbookEndIdle : m_spellbookStartIdle;
            Vector3 end = m_spellbookReverseIdle ? m_spellbookStartIdle : m_spellbookEndIdle;
            m_spellbookIdle = LerpHelper.LerpTransform(transform, start, end, m_spellbookDurationIdle);
            StartCoroutine(m_spellbookIdle);
        }
    }
}
