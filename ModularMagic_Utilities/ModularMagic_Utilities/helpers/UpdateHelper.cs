using ModularMagic_Utilities.Configs;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Utilities.helpers
{
    internal class UpdateHelper
    {

        public static void updateLanternMode(Material mat, Material matOff)
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

            Jotunn.Logger.LogWarning($"{lightObj.name}");
            Jotunn.Logger.LogWarning($"{flareObj.name}");
            Jotunn.Logger.LogWarning($"{demisterObj.name}");
            Jotunn.Logger.LogWarning($"Active: {lightObj.activeSelf}");

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

        public static void updateDemisterOnPrefab(GameObject prefab, float value)
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

        public static void updateDemisterOnPlayer(float value)
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

        public static void updateDemisterOnBoth(GameObject prefab, float value)
        {
            UpdateHelper.updateDemisterOnPrefab(prefab, value);
            UpdateHelper.updateDemisterOnPlayer(value);
        }

        public static void updateEitrRegenOnPlayer(string name, float value)
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
