using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    internal class ImbuementRune : MonoBehaviour
    {
        public List<string> m_allowedWeapons = new List<string>();
        public bool m_charged;
        public string m_descriptionKey;
        public int m_level;
        public string m_nameKey;
        public string m_type;
        public string m_value;

        //public void Awake()
        //{
        //    if (!m_charged)
        //    {
        //        Transform runeTransform = transform.Find($"attach/rune_{m_level}");
        //        MeshRenderer meshRenderer = runeTransform.gameObject.GetComponent<MeshRenderer>();

        //        Material[] mats = new Material[1] { meshRenderer.sharedMaterials[0] };
        //        mats[0].SetColor("_EmissionColor", new Color(0.1f, 0.1f, 0.1f));
        //    }

        //    ItemDrop itemDrop = gameObject.GetComponent<ItemDrop>();
        //    RenderManager.RenderRequest request = new RenderManager.RenderRequest(gameObject);
        //    request.Rotation = RenderManager.IsometricRotation;
        //    itemDrop.m_itemData.m_shared.m_icons = new Sprite[1] { RenderManager.Instance.Render(request) };
        //}

        public void Init(string type, string value, int level, bool charged, List<string> allowedWeapons, string nameKey, string descriptionKey)
        {
            m_allowedWeapons = allowedWeapons;
            m_charged = charged;
            m_descriptionKey = descriptionKey;
            m_level = level;
            m_nameKey = nameKey;
            m_type = type;
            m_value = value;
        }
    }
}
