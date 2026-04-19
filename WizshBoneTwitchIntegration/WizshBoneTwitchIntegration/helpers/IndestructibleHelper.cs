using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class IndestructibleHelper
    {
        public static Dictionary<string, float> originalHealth = new Dictionary<string, float>();

        public static void SetBoats(bool value)
        {
            List<GameObject> prefabs = ZNetScene.instance.m_prefabs.FindAll(item => item.GetComponent<Ship>() != null);
            SetHealthWearNTear(prefabs, value);
        }

        public static void SetChests(bool value)
        {
            List<GameObject> prefabs = ZNetScene.instance.m_prefabs.FindAll(item => item.GetComponent<Container>() != null && item.GetComponent<Piece>()?.m_primaryTarget == true);
            SetHealthWearNTear(prefabs, value);
        }

        public static void SetPortals(bool value)
        {
            List<GameObject> prefabs = ZNetScene.instance.m_prefabs.FindAll(item => item.GetComponent<TeleportWorld>() != null);
            SetHealthWearNTear(prefabs, value);
        }

        private static void SetHealthWearNTear(List<GameObject> prefabs, bool value)
        {
            // ADD changing ZDO data aswell, so earlier placed pieces will update to be indestructible and vice versa
            foreach (GameObject prefab in prefabs)
            {
                //Jotunn.Logger.LogWarning($"{prefab.name}!");
                WearNTear wearNTear = prefab.GetComponent<WearNTear>();

                if (wearNTear == null)
                    continue;

                float original = originalHealth.GetValueSafe(prefab.name);

                if (original == 0)
                    originalHealth.Add(prefab.name, wearNTear.m_health);

                if (value)
                {
                    wearNTear.m_health = -1f;
                    wearNTear.m_healthPercentage = 1f;
                }
                else if (original > 0)
                {
                    wearNTear.m_health = original;
                    wearNTear.m_healthPercentage = 100f;
                }
            }
        }

        public static void SetVegetables(bool value)
        {
            List<GameObject> prefabs = ZNetScene.instance.m_prefabs.FindAll(item => item.GetComponent<Pickable>()?.m_harvestable == true || item.GetComponent<Plant>()?.m_needCultivatedGround == true);

            foreach (GameObject prefab in prefabs)
            {
                //Jotunn.Logger.LogWarning(prefab.name);
                Destructible destructible = prefab.GetComponent<Destructible>();

                if (destructible != null)
                    destructible.enabled = !value;
            }
        }
    }
}
