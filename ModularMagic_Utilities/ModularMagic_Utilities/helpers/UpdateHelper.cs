using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Utilities.Helpers
{
    internal class UpdateHelper
    {
        public static void UpdateItemDropStats(GameObject prefab, UpdateItemDropOptions options)
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
                if (options.equipStatusEffect == null || options.equipStatusEffect.name != "empty_MMA") { itemDrop.m_itemData.m_shared.m_equipStatusEffect = options.equipStatusEffect; }
                if (options.weight != null) { itemDrop.m_itemData.m_shared.m_weight = (float)options.weight; }
                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }
                if (options.demister != null)
                    UpdateDemisterOnPrefab(prefab, (float)options.demister);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + e);
            }
        }

        public static void UpdateWeatherZone(GameObject prefab, UpdateWeatherZoneOptions options)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                WeatherZone weatherZone = prefab.transform.Find("weatherzone").GetComponent<WeatherZone>();

                if (weatherZone == null)
                    throw new Exception("WeatherZone is null");

                if (options.radius != null) { weatherZone.SetRadius((float)options.radius); }
                if (options.enableDomeVisual != null) { weatherZone.SetEnableDomeVisual((bool)options.enableDomeVisual); }
                if (options.enableDomeParticles != null) { weatherZone.SetEnableDomeParticles((bool)options.enableDomeParticles); }
                if (options.domeParticlesAmount != null) { weatherZone.SetDomeParticlesAmount((float)options.domeParticlesAmount); }

            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update WeatherZone: " + e);
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
                    throw new Exception("Could not find local player object to update Eitr regen");

                Inventory inv = Player.m_localPlayer.GetInventory();

                if (inv == null)
                    throw new Exception("Could not find local player's inventory to update Eitr regen");


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
