using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

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
                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }

                if (options.demister != null)
                    UpdateDemisterOnPrefab(prefab, (float)options.demister);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + e);
            }
        }

        public static void UpdateLanternMode(long playerId, bool status)
        {
            try
            {
                Player player = Player.GetPlayer(playerId);

                if (player == null)
                {
                    Jotunn.Logger.LogWarning("Could not find Player object");
                    return;
                }

                ItemData itemData = Player.m_localPlayer.m_utilityItem;

                if (itemData == null || itemData.m_shared == null)
                    return;

                int type = LanternHelper.GetLanternType(itemData);

                if (type == 0)
                    return;

                GameObject lightObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Point Light").gameObject;
                GameObject flareObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern/MMU_Lantern flare").gameObject;
                GameObject demisterObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;
                GameObject lanternObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern").gameObject;
                Light lightComp = lightObj.GetComponent<Light>();
                LightLod lightLodComp = lightObj.GetComponent<LightLod>();
                ParticleSystem flareComp = flareObj.GetComponent<ParticleSystem>();

                LanternConfig config = LanternHelper.GetLanternConfig(type);

                lightComp.color = config.lightColor;
                lightComp.range = config.lightRange;
                lightLodComp.m_baseRange = config.lightRange;
                lightComp.intensity = config.lightIntensity;
                lightObj.SetActive(status);

                // When new status is true, disable the component to force flare to change color
                if (status)
                    flareObj.SetActive(!status);

                flareComp.startColor = config.flareColor;
                flareObj.SetActive(status);

                demisterObj.SetActive(status);

                if (status)
                    config.lanternOn.SetColor("_EmissionColor", config.glassColor);

                SkinnedMeshRenderer meshObj = lanternObj.GetComponent<SkinnedMeshRenderer>();
                List<Material> materialList = new List<Material> { status ? config.lanternOn : config.lanternOff };
                meshObj.materials = materialList.ToArray();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update lantern on Player: " + e);
            }
        }

        public static void UpdateDemisterOnPrefab(GameObject prefab, float value)
        {
            try
            {
                GameObject demisterPrefab = prefab.transform.Find("attach_skin/equiped/MMU_Lantern Demister").gameObject;

                if (demisterPrefab == null)
                    return;

                if (value > 50)
                    value = 50;
                else if (value < 0) 
                    value = 0;

                ParticleSystemForceField comp = demisterPrefab.GetComponent<ParticleSystemForceField>();
                comp.endRange = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not alter demister effect on prefab: " + e);
            }
}

        public static void UpdateDemisterOnPlayer(float value)
        {
            try
            {
                if (Player.m_localPlayer == null)
                    return;

                GameObject demisterPlayerObj = Player.m_localPlayer.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;

                if (demisterPlayerObj == null)
                    return;

                if (value > 50)
                    value = 50;
                else if (value < 0)
                    value = 0;

                ParticleSystemForceField comp = demisterPlayerObj.GetComponent<ParticleSystemForceField>();
                comp.endRange = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not alter demister effect on player: " + e);
            }
        }

        public static void UpdateDemisterOnBoth(GameObject prefab, float value)
        {
            UpdateDemisterOnPrefab(prefab, value);
            UpdateDemisterOnPlayer(value);
        }

        public static void UpdateEitrRegenOnPlayer(string name, float value)
        {
            try
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

                List<ItemData> list = inv.GetAllItems();
                List<ItemData> items = list.FindAll(item => item.m_shared.m_name == name);

                foreach (ItemData item in items)
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
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update eitr regen on player: " + e);
            }
        }
    }
}
