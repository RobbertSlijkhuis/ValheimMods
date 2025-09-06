using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Components
{
    internal class Imbuements : MonoBehaviour
    {
        public List<Imbuement> m_imbuements = new List<Imbuement>();

        private void Awake()
        {
            m_imbuements.Add(new Imbuement("Accuracy", "Increase staff accuracy", ImbuementType.ProjectileAccuracy, ImbuementCategoryType.Normal, 1, "0.125", 0, 4));
            m_imbuements.Add(new Imbuement("Attack speed", "Increase staff attack speed", ImbuementType.ProjectileBurst, ImbuementCategoryType.Normal, 2, "0.0125", 0, 4));
            m_imbuements.Add(new Imbuement("Projectile Speed", "Increase projectile speed", ImbuementType.ProjectileVelocity, ImbuementCategoryType.Normal, 3, "1", 0, 4));
            m_imbuements.Add(new Imbuement("Eitr cost", "Decrease Eitr usage of the main attack", ImbuementType.EitrCost, ImbuementCategoryType.Normal, 4, "0.25", 0, 4));

            m_imbuements.Add(new Imbuement("Damage Slash", "Changes the projectile to resemble a circular blade, changing the main damage type to slash and increasing chopping capabilities", ImbuementType.DamageType, ImbuementCategoryType.Attack, 1, "Slash", 0, 1));
            m_imbuements.Add(new Imbuement("Damage Pierce", "Cone shaped projectiles to increase armor piercing, changes the main damage type to pierce and increases mining capabilities", ImbuementType.DamageType, ImbuementCategoryType.Attack, 2, "Pierce", 0, 1));

            m_imbuements.Add(new Imbuement("Giant boulder attack", "Rain down a giant boulder from the sky!", ImbuementType.SecondaryAttack, ImbuementCategoryType.SecondaryAttack, 1, "Rain", 0, 1));
            m_imbuements.Add(new Imbuement("Summon Roots", "Summon roots to attack your enemies!", ImbuementType.SecondaryAttack, ImbuementCategoryType.SecondaryAttack, 2, "Summon", 0, 1));

            string imbuementsString = ImbuementHelper.ListToString(m_imbuements);
            GetComponent<ItemDrop>().m_itemData.m_customData[ModularMagic_EarthStaffs.imbuementDataKey] = imbuementsString;
            //Jotunn.Logger.LogWarning("======================================");
            //Jotunn.Logger.LogWarning("AWAKE: " + imbuementsString);
        }
    }
}
