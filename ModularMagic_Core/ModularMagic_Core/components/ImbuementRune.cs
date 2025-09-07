using ModularMagic_Core.Components;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using ModularMagic_EarthStaffs.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModularMagic_Core.components
{
    internal class ImbuementRune : MonoBehaviour, Hoverable, Interactable
    {
        public ImbuementTable m_imbuementTable;
        public Imbuement m_imbuement;
        public RuneMaterials m_runeMaterials;
        public string m_canActivate;

        public float m_durationMoveIn = 0.5f;
        private float m_durationIdle = 5f;
        private Vector3 m_origin;
        private Vector3 m_startIdle;
        private float m_minIdle = -0.02f;
        private float m_maxIdle = 0.02f;

        public void Init()
        {
            m_imbuementTable = transform.parent.parent.gameObject.GetComponent<ImbuementTable>();
            m_imbuementTable.m_onSave.AddListener(UpdateEmission);
            m_imbuementTable.m_onRuneActivation.AddListener(UpdateEmission);

            m_origin = transform.localPosition;
            transform.localRotation = TransformHelper.GenerateRotation(new Vector3(Random.Range(265, 275), Random.Range(-5f, 5), Random.Range(265, 275)));
            transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
            m_canActivate = CanActivate();

            UpdateEmission();

            Invoke(nameof(StartMoveIn), m_durationMoveIn);
            InvokeRepeating(nameof(StartIdle), m_durationMoveIn + 0.5f, m_durationIdle + 1f);
        }

        public string GetHoverText()
        {
            if (m_imbuement == null)
                return "Something went wrong, contact author pls";

            m_canActivate = CanActivate();

            string inputString = m_canActivate == CanRuneActivateType.Yes ? Localization.instance.Localize("\n[<color=yellow>$KEY_Use</color>] Upgrade\n[<color=yellow>$KEY_AltPlace + $KEY_Use</color>] Downgrade") : "\n" + StatusMessage(m_canActivate);
            string nameColor = m_canActivate == CanRuneActivateType.Yes ? (m_imbuement.enabled ? "green" : "yellow") : "red";
            string name = $"<color={nameColor}>{m_imbuement.name} {(m_imbuement.level > 0 ? m_imbuement.level : "")}</color>";
            string description = $"\n {m_imbuement.description}";
            string divider = "\n <color=#808080ff>--------------------------------------</color>";
            // string cost = "\n" + m_imbuement.materialRequired + " Magic powder, " + m_imbuement.skillRequired + " skill";

            return name + description + inputString;
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

            if (m_canActivate != CanRuneActivateType.Yes)
                return false;

            if (m_imbuement.category == ImbuementCategoryType.Normal)
            {
                if (alt)
                {
                    m_imbuement.level--;

                    if (m_imbuement.level < 0)
                        m_imbuement.level = 0;

                    if (m_imbuement.level == 0)
                        m_imbuement.enabled = false;

                    UpdateEmission();
                    m_imbuementTable.m_onRuneActivation.Invoke();
                    return true;
                }

                m_imbuement.level++;

                if (m_imbuement.level > m_imbuement.maxLevel)
                    m_imbuement.level = m_imbuement.maxLevel;

                if (m_imbuement.level > 0)
                    m_imbuement.enabled = true;

                UpdateEmission();
                m_imbuementTable.m_onRuneActivation.Invoke();
                return true;
            }

            if (m_imbuement.category == ImbuementCategoryType.Attack)
            {
                if (alt)
                {
                    m_imbuement.enabled = false;
                    UpdateEmission();
                    return true;
                }

                Imbuement exists = m_imbuementTable.m_imbuements.Find(item => item.category == ImbuementCategoryType.Attack && item.enabled);

                if (exists != null)
                {
                    exists.enabled = false;
                }

                m_imbuement.enabled = true;
                UpdateEmission();
                m_imbuementTable.m_onRuneActivation.Invoke();
                return true;
            }

            if (m_imbuement.category == ImbuementCategoryType.SecondaryAttack)
            {
                if (alt)
                {
                    m_imbuement.enabled = false;
                    UpdateEmission();
                    m_imbuementTable.m_onRuneActivation.Invoke();
                    return true;
                }

                Imbuement exists = m_imbuementTable.m_imbuements.Find(item => item.category == ImbuementCategoryType.SecondaryAttack && item.enabled);

                if (exists != null)
                {
                    exists.enabled = false;
                }

                m_imbuement.enabled = true;
                UpdateEmission();
                m_imbuementTable.m_onRuneActivation.Invoke();
                return true;
            }

            return false;
        }

        public void Destroy()
        {
            CancelInvoke(nameof(StartIdle));
            Invoke(nameof(StartMoveOut), m_durationMoveIn);
        }

        private string CanActivate()
        {
            if (Player.m_localPlayer == null)
                throw new Exception("Player is null");

            //if (m_imbuement.category == ImbuementCategoryType.Attack)
            //{
            //    Imbuement imbuement = m_imbuementTable.m_imbuements.Find(item => item.category == ImbuementCategoryType.Attack && item.enabled);

            //    if (imbuement != null)
            //        return CanRuneActivateType.HasAttackUpgrade;
            //}

            //if (m_imbuement.category == ImbuementCategoryType.SecondaryAttack)
            //{
            //    Imbuement imbuement = m_imbuementTable.m_imbuements.Find(item => item.category == ImbuementCategoryType.SecondaryAttack && item.enabled);

            //    if (imbuement != null)
            //        return CanRuneActivateType.HasSecondaryAttack;
            //}

            //if (Player.m_localPlayer.GetSkillLevel(Skills.SkillType.ElementalMagic) < m_imbuement.skillRequired)
            //    return CanRuneActivateType.NotEnoughSkill;

            //Inventory inventory = Player.m_localPlayer.GetInventory();
            //if ((inventory.CountItems(m_imbuementTable.m_imbuementMaterial) - m_imbuementTable.CountMaterialRequired()) < m_imbuement.materialRequired)
            //    return CanRuneActivateType.NotEnoughMaterial;

            return CanRuneActivateType.Yes;
        }

        public string StatusMessage(string status)
        {
            switch (status)
            {
                case nameof(CanRuneActivateType.NotEnoughSkill):
                    return "Not enough skill";
                case nameof(CanRuneActivateType.NotEnoughMaterial):
                    return "Not enough materials";
                case nameof(CanRuneActivateType.HasAttackUpgrade):
                    return "There is already another attack upgrade active!";
                case nameof(CanRuneActivateType.HasSecondaryAttack):
                    return "There is already another secondary attack active!";
                default:
                    return "Something went wrong, contact author pls";
            }
        }

        private void UpdateEmission()
        {          
            MeshRenderer meshComp = gameObject.GetComponent<MeshRenderer>();

            if (m_imbuement.enabled)
            {
                LightPresetColors colors = LightColorPresetHelper.GetColors(m_imbuement.isImbued ? LightColorPresetType.Green : LightColorPresetType.Yellow);
                Material runeMat = m_runeMaterials.wood;

                switch (m_imbuement.level)
                {
                    case 1:
                        runeMat = m_runeMaterials.wood;
                        break;
                    case 2:
                        runeMat = m_runeMaterials.stone;
                        break;
                    case 3:
                        runeMat = m_runeMaterials.marble;
                        break;
                    case 4:
                        runeMat = m_runeMaterials.grausten;
                        break;
                    default:
                        runeMat = m_runeMaterials.wood;
                        break;
                }

                meshComp.materials = new Material[1] { runeMat };

                Material mat = meshComp.materials[0];
                mat.SetColor("_EmissionColor", colors.emissionColor);
            }
            else
                meshComp.materials = new Material[1] { m_runeMaterials.woodOff };
        }

        public void StartMoveIn()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = ImbuementHelper.CalculatePosition(m_imbuement.category, m_imbuement.column);
            m_startIdle = end;
            StartCoroutine(LerpHelper.LerpTransform(transform, start, end, m_durationMoveIn));
        }

        public void StartMoveOut()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = m_origin;
            StartCoroutine(LerpHelper.LerpTransform(transform, start, end, m_durationMoveIn, gameObject, true));
        }

        public void StartIdle()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = LerpHelper.RandomPosition(m_startIdle, m_minIdle, m_maxIdle);
            StartCoroutine(LerpHelper.LerpTransform(transform, start, end, m_durationIdle));
        }

        private string FormatNamesMessage(List<Imbuement> imbuements, bool filterEnabled = false)
        {
            string names = "";
            string multipleString = imbuements.Count > 1 ? " any of: " : ": ";

            if (filterEnabled)
                imbuements = imbuements.FindAll(item => item.enabled);

            foreach (Imbuement imbuement in imbuements)
            {
                names += imbuement.name + ", ";
            }

            if (names != "")
                names = names.Remove(names.Length - 2);

            return multipleString + names;
        }
    }
}
