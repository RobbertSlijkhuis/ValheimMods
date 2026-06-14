using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class IndestructibleHelper
    {
        private static readonly int s_origHealthHash = "WBTI_origHealth".GetStableHashCode();

        public static void SetBoats(bool value)
        {
            SetIndestructible(item => item.GetComponent<Ship>() != null, value);
        }

        public static void SetChests(bool value)
        {
            SetIndestructible(item => item.GetComponent<Container>() != null && item.GetComponent<Piece>()?.m_primaryTarget == true, value);
        }

        public static void SetPortals(bool value)
        {
            SetIndestructible(item => item.GetComponent<TeleportWorld>() != null, value);
        }

        public static void SetVegetables(bool value)
        {
            SetIndestructible(item => item.GetComponent<Pickable>()?.m_harvestable == true || item.GetComponent<Plant>()?.m_needCultivatedGround == true, value);
        }

        private static void SetIndestructible(Func<GameObject, bool> matchesPrefab, bool value)
        {
            if (ZNetScene.instance == null || ZDOMan.instance == null)
                return;

            IEnumerable<string> prefabNames = ZNetScene.instance.m_prefabs.FindAll(item => matchesPrefab(item)).Select(item => item.name);

            foreach (string prefabName in prefabNames)
            {
                List<ZDO> zdos = new List<ZDO>();
                int index = 0;

                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(prefabName, zdos, ref index)) { }

                foreach (ZDO zdo in zdos)
                    ApplyIndestructible(zdo, value);
            }
        }

        /**
         * Marks a single placed piece as indestructible (or restores it), by writing
         * the "health" ZDO value that WearNTear/Destructible check on every hit.
         * This only ever touches the placed instance's ZDO, never the prefab template.
         */
        public static void ApplyIndestructible(ZDO zdo, bool value)
        {
            if (value)
            {
                float current = zdo.GetFloat(ZDOVars.s_health, GetDefaultHealth(zdo));

                if (current > 0f)
                    zdo.Set(s_origHealthHash, current);

                zdo.Set(ZDOVars.s_health, -1f);
            }
            else
            {
                float original = zdo.GetFloat(s_origHealthHash, GetDefaultHealth(zdo));

                zdo.Set(ZDOVars.s_health, original);
                zdo.RemoveFloat(s_origHealthHash);
            }
        }

        private static float GetDefaultHealth(ZDO zdo)
        {
            GameObject prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());

            if (prefab == null)
                return 0f;

            WearNTear wearNTear = prefab.GetComponent<WearNTear>();

            if (wearNTear != null)
                return wearNTear.m_health;

            Destructible destructible = prefab.GetComponent<Destructible>();

            return destructible != null ? destructible.m_health : 0f;
        }
    }
}
