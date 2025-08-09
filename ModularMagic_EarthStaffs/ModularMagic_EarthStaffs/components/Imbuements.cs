using HarmonyLib;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Components
{
    internal class Imbuements : MonoBehaviour
    {
        public List<Imbuement> m_imbuements = new List<Imbuement>();

        private void Awake()
        {
            m_imbuements.Add(new Imbuement("Damage Slash", ImbuementType.DamageType, "Changes the projectile to resemble a circular blade, changing the main damage type to slash and increasing chopping capabilities", "Slash", 20, 10, new ImbuementPath(1, 2, true)));
            m_imbuements.Add(new Imbuement("Damage Pierce", ImbuementType.DamageType, "Cone shaped projectiles to increase armor piercing, changes the main damage type to pierce and increases mining capabilities", "Pierce", 20, 10, new ImbuementPath(2, 2, true)));

            m_imbuements.Add(new Imbuement("Accuracy 1", ImbuementType.ProjectileAccuracy, "Increase accuracy", "0.2", 10, 5, new ImbuementPath(1, 1)));
            m_imbuements.Add(new Imbuement("Accuracy 2", ImbuementType.ProjectileAccuracy, "Increase accuracy further", "0.2", 30, 15, new ImbuementPath(1, 3)));
            m_imbuements.Add(new Imbuement("Accuracy 3", ImbuementType.ProjectileAccuracy, "Increase accuracy even further", "0.2", 40, 20, new ImbuementPath(1, 4)));

            m_imbuements.Add(new Imbuement("Burst 1", ImbuementType.ProjectileBurst, "Increase attack speed", "0.02", 10, 5, new ImbuementPath(2, 1)));
            m_imbuements.Add(new Imbuement("Burst 2", ImbuementType.ProjectileBurst, "Increase attack speed further", "0.02", 30, 15, new ImbuementPath(2, 3)));
            m_imbuements.Add(new Imbuement("Burst 3", ImbuementType.ProjectileBurst, "Increase attack speed even further", "0.02", 40, 20, new ImbuementPath(2, 4)));

            m_imbuements.Add(new Imbuement("Parry master", ImbuementType.ParryBonus, "The staff has been strengthend with magic, increasing parry bonus by 1", "1", 10, 5, new ImbuementPath(3, 1)));
            m_imbuements.Add(new Imbuement("Giant boulder attack", ImbuementType.SecondaryAttack, "Rain down a giant boulder from the sky!", "Rain", 30, 15, new ImbuementPath(3, 3, true)));
            m_imbuements.Add(new Imbuement("Summon Roots", ImbuementType.SecondaryAttack, "Summon roots to attack your enemies!", "Summon", 50, 25, new ImbuementPath(3, 5, true)));

            m_imbuements.Add(new Imbuement("Eitr cost", ImbuementType.EitrCost, "Decrease Eitr cost by 1", "1", 20, 10, new ImbuementPath(4, 2)));

            m_imbuements.Add(new Imbuement("Speed 1", ImbuementType.ProjectileVelocity, "Increase projectile speed", "2", 10, 5, new ImbuementPath(4, 1)));
            m_imbuements.Add(new Imbuement("Speed 2", ImbuementType.ProjectileVelocity, "Increase projectile speed further", "2", 30, 15, new ImbuementPath(4, 3)));
            m_imbuements.Add(new Imbuement("Speed 3", ImbuementType.ProjectileVelocity, "Increase projectile speed even further", "2", 40, 20, new ImbuementPath(4, 4)));

            string imbuementsString = ImbuementHelper.ListToString(m_imbuements);
            GetComponent<ItemDrop>().m_itemData.m_customData[ModularMagic_EarthStaffs.imbuementDataKey] = imbuementsString;
            //Jotunn.Logger.LogWarning("======================================");
            //Jotunn.Logger.LogWarning("AWAKE: " + imbuementsString);
        }
    }
}
