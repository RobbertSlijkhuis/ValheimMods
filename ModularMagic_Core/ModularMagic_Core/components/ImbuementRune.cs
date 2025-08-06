using ModularMagic_Core.Components;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
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
        public List<Imbuement> m_requiredImbuements = new List<Imbuement>();
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
            m_origin = transform.localPosition;
            transform.localRotation = TransformHelper.generateRotation(new Vector3(Random.Range(265, 275), Random.Range(-5f, 5), Random.Range(265, 275)));
            transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
            m_canActivate = CanActivate();
            InitRequiredImbuements();
            UpdateEmission();

            //m_idleOriginalStart = position;
            Invoke(nameof(StartMoveIn), m_durationMoveIn);
            InvokeRepeating(nameof(StartIdle), m_durationMoveIn + 0.5f, m_durationIdle + 1f);
        }

        public string GetHoverText()
        {
            if (m_imbuement == null)
                return "Something went wrong, contact author pls";

            m_canActivate = CanActivate();

            string inputString = m_canActivate == CanRuneActivateType.Yes ? "\n[<color=yellow>E</color>] " : "\n" + StatusMessage(m_canActivate);
            string name = "<color=" + (m_canActivate == CanRuneActivateType.Yes ? (m_imbuement.enabled ? "green" : "yellow") : "red") + ">" + m_imbuement.name + "</color>";
            string description = "\n" + m_imbuement.description;
            string cost = "\n" + m_imbuement.materialRequired + " Magic powder, " + m_imbuement.skillRequired + " skill";
            string enableMessage = m_canActivate == CanRuneActivateType.Yes ? (m_imbuement.enabled ? "Disable" : "Enable") : "";

            return name + description + cost + inputString + enableMessage;
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

            if (m_canActivate == CanRuneActivateType.Yes)
            {
                Jotunn.Logger.LogWarning("=== Activate =====================");
                m_imbuement.enabled = !m_imbuement.enabled;
                m_imbuementTable.m_onRuneActivation.Invoke();
                UpdateEmission();
                return true;
            }

            return false;
        }

        public void Destroy()
        {
            CancelInvoke(nameof(StartIdle));
            Invoke(nameof(StartMoveOut), m_durationMoveIn);
        }

        private void InitRequiredImbuements()
        {
            if (m_imbuement.path.row == 1) return;

            Imbuement prev = m_imbuementTable.m_imbuements.Find(item => item.path.row == m_imbuement.path.row - 1 && item.path.column == m_imbuement.path.column);

            if (prev != null)
            {
                m_requiredImbuements.Add(prev);
                return;
            }

            Imbuement Left = m_imbuementTable.m_imbuements.Find(item => item.path.row == m_imbuement.path.row - 1 && item.path.column == m_imbuement.path.column - 1);
            Imbuement right = m_imbuementTable.m_imbuements.Find(item => item.path.row == m_imbuement.path.row - 1 && item.path.column == m_imbuement.path.column + 1);

            if (Left != null && (Left.path.allowIntersect || m_imbuement.path.allowIntersect))
                m_requiredImbuements.Add(Left);

            if (right != null && (right.path.allowIntersect || m_imbuement.path.allowIntersect))
                m_requiredImbuements.Add(right);
        }

        private string CanActivate()
        {
            if (m_imbuement.enabled)
                return CanRuneActivateType.Yes;

            if (Player.m_localPlayer == null)
                throw new Exception("Player is null");

            if (m_requiredImbuements.Count != 0 && m_requiredImbuements.Find(item => item.enabled) == null)
                return CanRuneActivateType.RequiresOtherImbuement;
            
            if (Player.m_localPlayer.GetSkillLevel(Skills.SkillType.ElementalMagic) < m_imbuement.skillRequired)
                return CanRuneActivateType.NotEnoughSkill;

            Inventory inventory = Player.m_localPlayer.GetInventory();
            if ((inventory.CountItems(m_imbuementTable.m_imbuementMaterial) - m_imbuementTable.CountMaterialRequired()) < m_imbuement.materialRequired)
                return CanRuneActivateType.NotEnoughMaterial;

            return CanRuneActivateType.Yes;
        }

        public string StatusMessage(string status)
        {
            switch (status)
            {
                case nameof(CanRuneActivateType.RequiresOtherImbuement):
                    string names = "";
                    string multiple = m_requiredImbuements.Count > 1 ? " any of: " : ": ";

                    foreach (Imbuement imbuement in m_requiredImbuements)
                    {
                        names += imbuement.name + ", ";
                    }

                    if (names != "")
                        names = names.Remove(names.Length - 2);

                    return "Requires" + multiple + names;
                case nameof(CanRuneActivateType.NotEnoughSkill):
                    return "Not enough skill";
                case nameof(CanRuneActivateType.NotEnoughMaterial):
                    return "Not enough materials";
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
                meshComp.materials = new Material[1] { m_runeMaterials.emissive };

                Material mat = meshComp.materials[0];
                mat.SetColor("_EmissionColor", colors.emissionColor);
            }
            else
                meshComp.materials = new Material[1] { m_runeMaterials.material };
        }

        public void StartMoveIn()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = ImbuementHelper.CalculatePosition(m_imbuement.path);
            m_startIdle = end;
            StartCoroutine(LerpTransform(start, end, m_durationMoveIn));
        }

        public void StartMoveOut()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = m_origin;
            StartCoroutine(LerpTransform(start, end, m_durationMoveIn, true));
        }

        public void StartIdle()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = RandomPosition(m_startIdle, m_minIdle, m_maxIdle);
            StartCoroutine(LerpTransform(start, end, m_durationIdle));
        }

        private Vector3 RandomPosition(Vector3 start, float min, float max)
        {
            Vector3 end = new Vector3(start.x, start.y, start.z);
            end.x = start.x + Random.Range(min, max);
            end.y = start.y + Random.Range(min, max);

            return end;
        }

        private IEnumerator LerpTransform(Vector3 start, Vector3 end, float duration, bool destroy = false)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                transform.localPosition = Vector3.Lerp(start, end, t);
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = end;

            if (destroy)
                GameObject.Destroy(gameObject);
        }
    }
}
