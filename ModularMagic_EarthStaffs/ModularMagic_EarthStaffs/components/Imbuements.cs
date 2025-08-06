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
            m_imbuements.Add(new Imbuement("Accuracy 1", ImbuementType.ProjectileAccuracy, "Desc", "0.2", 10, 5, new ImbuementPath(1, 1)));
            m_imbuements.Add(new Imbuement("Accuracy 2", ImbuementType.ProjectileAccuracy, "Desc", "0.2", 20, 10, new ImbuementPath(1, 2)));
            m_imbuements.Add(new Imbuement("Accuracy 3", ImbuementType.ProjectileAccuracy, "Desc", "0.2", 40, 20, new ImbuementPath(1, 4)));

            m_imbuements.Add(new Imbuement("Secondary Attack", ImbuementType.SecondaryAttack, "Desc", "Nova", 30, 15, new ImbuementPath(2, 3, true)));
            m_imbuements.Add(new Imbuement("Max Quality", ImbuementType.MaxQuality, "Desc", "1", 50, 25, new ImbuementPath(2, 5, true)));

            m_imbuements.Add(new Imbuement("Burst 1", ImbuementType.ProjectileBurst, "Desc", "0.02", 10, 5, new ImbuementPath(3, 1)));
            m_imbuements.Add(new Imbuement("Burst 2", ImbuementType.ProjectileBurst, "Desc", "0.02", 20, 10, new ImbuementPath(3, 2)));
            m_imbuements.Add(new Imbuement("Burst 3", ImbuementType.ProjectileBurst, "Desc", "0.02", 40, 20, new ImbuementPath(3, 4)));

            m_imbuements.Add(new Imbuement("Parry Master", ImbuementType.ParryBonus, "Desc", "1", 10, 5, new ImbuementPath(4, 1)));
            m_imbuements.Add(new Imbuement("Damage Ratio 1", ImbuementType.DamageRatio, "Desc", "80", 20, 10, new ImbuementPath(4, 2)));
            m_imbuements.Add(new Imbuement("Damage Ratio 2", ImbuementType.DamageRatio, "Desc", "90", 30, 15, new ImbuementPath(4, 3)));
            m_imbuements.Add(new Imbuement("Damage Ratio 3", ImbuementType.DamageRatio, "Desc", "100", 40, 20, new ImbuementPath(4, 4)));
            m_imbuements.Add(new Imbuement("Speed", ImbuementType.ProjectileVelocity, "Desc", "5", 50, 25, new ImbuementPath(4, 5)));

            string imbuementsString = ImbuementHelper.ListToString(m_imbuements);
            GetComponent<ItemDrop>().m_itemData.m_customData[ModularMagic_EarthStaffs.imbuementDataKey] = imbuementsString;
            //Jotunn.Logger.LogWarning("======================================");
            //Jotunn.Logger.LogWarning("AWAKE: " + imbuementsString);
        }
    }
}
