using HarmonyLib;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Components
{
    internal class Imbuements : MonoBehaviour
    {
        public List<Imbuement> m_imbuements = new List<Imbuement>();
        public int m_slots = 2;
        public int m_tier = 1;

        public void Awake()
        {
            ItemDrop itemDrop = gameObject.GetComponent<ItemDrop>();
            string imbuementsData = itemDrop.m_itemData.m_customData.GetValueSafe(ModularMagic_EarthStaffs.imbuementDataKey);

            if (imbuementsData == null)
            {
                for (int i = 0; i < m_slots; i++)
                {
                    Imbuement imbuement = new Imbuement();
                    imbuement.tier = m_tier;
                    m_imbuements.Add(imbuement);
                }

                string imbuementsString = ImbuementHelper.ListToString(m_imbuements);
                itemDrop.m_itemData.m_customData[ModularMagic_EarthStaffs.imbuementDataKey] = imbuementsString;
            }
            else
            {
                m_imbuements = ImbuementHelper.StringToList(imbuementsData);

                foreach (Imbuement imbuement in m_imbuements)
                {
                    Jotunn.Logger.LogWarning(imbuement.name);
                }
            }

            //m_imbuements.Add(new Imbuement("Accuracy", "Increase staff accuracy", ImbuementType.ProjectileAccuracy, ImbuementCategoryType.Normal, 1, "0.125", 0, 4));
            //m_imbuements.Add(new Imbuement("Attack speed", "Increase staff attack speed", ImbuementType.ProjectileBurst, ImbuementCategoryType.Normal, 2, "0.0125", 0, 4));
            //m_imbuements.Add(new Imbuement("Projectile Speed", "Increase projectile speed", ImbuementType.ProjectileVelocity, ImbuementCategoryType.Normal, 3, "1", 0, 4));
            //m_imbuements.Add(new Imbuement("Eitr cost", "Decrease Eitr usage of the main attack", ImbuementType.EitrCost, ImbuementCategoryType.Normal, 4, "0.25", 0, 4));

            //m_imbuements.Add(new Imbuement("Damage Slash", "Changes the projectile to resemble a circular blade, changing the main damage type to slash and increasing chopping capabilities", ImbuementType.DamageType, ImbuementCategoryType.Attack, 1, "Slash", 0, 1));
            //m_imbuements.Add(new Imbuement("Damage Pierce", "Cone shaped projectiles to increase armor piercing, changes the main damage type to pierce and increases mining capabilities", ImbuementType.DamageType, ImbuementCategoryType.Attack, 2, "Pierce", 0, 1));

            //m_imbuements.Add(new Imbuement("Giant boulder attack", "Rain down a giant boulder from the sky!", ImbuementType.SecondaryAttack, ImbuementCategoryType.SecondaryAttack, 1, "Rain", 0, 1));
            //m_imbuements.Add(new Imbuement("Summon Roots", "Summon roots to attack your enemies!", ImbuementType.SecondaryAttack, ImbuementCategoryType.SecondaryAttack, 2, "Summon", 0, 1));
        }
    }
}
