using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Utilities.Helpers
{
    internal class UpdateHelper
    {
        public static void UpdateItemDropStats(GameObject prefab, UpdateItemDropStatsOptions options)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                    throw new Exception("ItemDrop is null");

                if (options.name != null) { itemDrop.m_itemData.m_shared.m_name = options.name; }
                if (options.description != null) { itemDrop.m_itemData.m_shared.m_description = options.description; }
                if (options.maxQuality > 0) { itemDrop.m_itemData.m_shared.m_maxQuality = (int)options.maxQuality; }
                if (options.movementModifier != null) { itemDrop.m_itemData.m_shared.m_movementModifier = (float)options.movementModifier; }

                if (options.blockPower != null) { itemDrop.m_itemData.m_shared.m_blockPower = (float)options.blockPower; }
                if (options.deflectionForce != null) { itemDrop.m_itemData.m_shared.m_deflectionForce = (float)options.deflectionForce; }
                if (options.attackForce != null) { itemDrop.m_itemData.m_shared.m_attackForce = (float)options.attackForce; }
                if (options.backstabBonus != null) { itemDrop.m_itemData.m_shared.m_backstabBonus = (float)options.backstabBonus; }

                if (options.attackEitr != null) { itemDrop.m_itemData.m_shared.m_attack.m_attackEitr = (float)options.attackEitr; }
                if (options.secondaryAttackEitr != null) { itemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackEitr = (float)options.secondaryAttackEitr; }
                if (options.damageBlunt != null) { itemDrop.m_itemData.m_shared.m_damages.m_blunt = (float)options.damageBlunt; }
                if (options.damageChop != null) { itemDrop.m_itemData.m_shared.m_damages.m_chop = (float)options.damageChop; }
                if (options.damageFire != null) { itemDrop.m_itemData.m_shared.m_damages.m_fire = (float)options.damageFire; }
                if (options.damageFrost != null) { itemDrop.m_itemData.m_shared.m_damages.m_frost = (float)options.damageFrost; }
                if (options.damageLightning != null) { itemDrop.m_itemData.m_shared.m_damages.m_lightning = (float)options.damageLightning; }
                if (options.damagePickaxe != null) { itemDrop.m_itemData.m_shared.m_damages.m_pickaxe = (float)options.damagePickaxe; }
                if (options.damagePierce != null) { itemDrop.m_itemData.m_shared.m_damages.m_pierce = (float)options.damagePierce; }
                if (options.damageSlash != null) { itemDrop.m_itemData.m_shared.m_damages.m_slash = (float)options.damageSlash; }
                if (options.damageSpirit != null) { itemDrop.m_itemData.m_shared.m_damages.m_spirit = (float)options.damageSpirit; }

                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }

                if (options.demister != null)
                {
                    UpdateHelper.UpdateDemisterOnPrefab(prefab, (float)options.demister);
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + error);
            }
        }

        public static void UpdateLanternMode(Material mat, Material matOff)
        {
            if (Player.m_localPlayer == null)
            {
                Jotunn.Logger.LogWarning("Could not find local Player object");
                return;
            }

            GameObject lightObj = Player.m_localPlayer.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Point Light").gameObject;
            GameObject flareObj = Player.m_localPlayer.transform.Find("Visual/attach_skin(Clone)/Lantern/MMU_Lantern flare").gameObject;
            GameObject demisterObj = Player.m_localPlayer.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;
            GameObject lanternObj = Player.m_localPlayer.transform.Find("Visual/attach_skin(Clone)/Lantern").gameObject;

            //Jotunn.Logger.LogWarning($"{lightObj.name}");
            //Jotunn.Logger.LogWarning($"{flareObj.name}");
            //Jotunn.Logger.LogWarning($"{demisterObj.name}");
            //Jotunn.Logger.LogWarning($"Active: {lightObj.activeSelf}");

            if (lightObj.activeSelf)
            {
                lightObj.SetActive(false);
                flareObj.SetActive(false);
                demisterObj.SetActive(false);

                SkinnedMeshRenderer meshObj = lanternObj.GetComponent<SkinnedMeshRenderer>();
                List<Material> materialList = new List<Material> { matOff };
                meshObj.materials = materialList.ToArray();
            }
            else
            {
                lightObj.SetActive(true);
                flareObj.SetActive(true);
                demisterObj.SetActive(true);

                SkinnedMeshRenderer meshObj = lanternObj.GetComponent<SkinnedMeshRenderer>();
                List<Material> materialList = new List<Material> { mat };
                meshObj.materials = materialList.ToArray();
            }
        }

        public static void UpdateDemisterOnPrefab(GameObject prefab, float value)
        {
            GameObject demisterPrefab = prefab.transform.Find("attach_skin/equiped/MMU_Lantern Demister").gameObject;

            if (demisterPrefab == null)
            {
                Jotunn.Logger.LogWarning("Could not find demister obj on prefab: " + prefab.name);
                return;
            }

            if (value > 50)
                value = 50;
            else if (value < 0) 
                value = 0;

            ParticleSystemForceField comp = demisterPrefab.GetComponent<ParticleSystemForceField>();
            comp.endRange = value;
        }

        public static void UpdateDemisterOnPlayer(float value)
        {
            if (Player.m_localPlayer == null)
            {
                Jotunn.Logger.LogWarning("Could not find local Player object");
                return;
            }

            GameObject demisterPlayerObj = Player.m_localPlayer.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;

            if (demisterPlayerObj == null)
            {
                Jotunn.Logger.LogWarning("Could not find demister obj on Player");
                return;
            }

            if (value > 50)
                value = 50;
            else if (value < 0)
                value = 0;

            ParticleSystemForceField comp = demisterPlayerObj.GetComponent<ParticleSystemForceField>();
            comp.endRange = value;
        }

        public static void UpdateDemisterOnBoth(GameObject prefab, float value)
        {
            UpdateHelper.UpdateDemisterOnPrefab(prefab, value);
            UpdateHelper.UpdateDemisterOnPlayer(value);
        }

        public static void UpdateEitrRegenOnPlayer(string name, float value)
        {
            if (Player.m_localPlayer == null)
            {
                Jotunn.Logger.LogWarning("Could not find local player object to update Eitr regen");
                return;
            }

            Inventory inv = Player.m_localPlayer.GetInventory();

            if (inv == null)
            {
                Jotunn.Logger.LogWarning("Could not find local player's inventory to update Eitr regen");
                return;
            }

            if (!inv.ContainsItemByName(name))
            {
                return;
            }

            List<ItemDrop.ItemData> list = inv.GetAllItems();
            List<ItemDrop.ItemData> items = list.FindAll(item => item.m_shared.m_name == name);

            foreach (ItemDrop.ItemData item in items)
            {
                if (item == null || item.m_shared == null)
                {
                    Jotunn.Logger.LogWarning("Could not find " + name + " in inventory list to update Eitr regen");
                    continue;
                }

                Jotunn.Logger.LogWarning("Update Eitr regen to: " + value);
                item.m_shared.m_eitrRegenModifier = value;
            }
        }
    }
}
