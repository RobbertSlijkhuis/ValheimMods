using Jotunn.Managers;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using Testing.Models;
using UnityEngine;

namespace Testing.Helpers
{
    internal class UpdateHelper
    {

        public static void UpdateItemDrop(GameObject prefab, UpdateItemDropOptions options)
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
                if (options.weight != null) { itemDrop.m_itemData.m_shared.m_weight = (float)options.weight; }
                if (options.maxStackSize != null) { itemDrop.m_itemData.m_shared.m_maxStackSize = (int)options.maxStackSize; }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop: " + e);
            }
        }

        public static void UpdateDuration(GameObject prefab, int duration)
        {
            try
            {
                GameObject fxPrefab = null;
                StatusEffect statusEffect = null;

                switch (prefab.name)
                {
                    case var value when value == Testing.Instance.prefabs.BarlyWineSplash.name:
                        fxPrefab = Testing.Instance.prefabs.BarlyWineSplashFX;
                        statusEffect = Testing.Instance.effects.BarlyWineSplash;
                        break;
                    case var value when value == Testing.Instance.prefabs.FrostResistSplash.name:
                        fxPrefab = Testing.Instance.prefabs.FrostResistSplashFX;
                        statusEffect = Testing.Instance.effects.FrostResistSplash;
                        break;
                    case var value when value == Testing.Instance.prefabs.PoisonResistSplash.name:
                        fxPrefab = Testing.Instance.prefabs.PoisonResistSplashFX;
                        statusEffect = Testing.Instance.effects.PoisonResistSplash;
                        break;
                }

                if (fxPrefab == null || statusEffect == null)
                    throw new Exception("Could not find corresponding fx prefab or statuseffect");

                ParticleSystem.MainModule flareMain = fxPrefab.transform.Find("lingering_effects/flare").gameObject.GetComponent<ParticleSystem>().main;
                ParticleSystem.MainModule flakesMain = fxPrefab.transform.Find("lingering_effects/flakes_up").gameObject.GetComponent<ParticleSystem>().main;
                fxPrefab.GetComponent<TimedDestruction>().m_timeout = duration;
                flareMain.duration = duration;
                flareMain.startLifetime = duration;
                flakesMain.duration = duration - 3;
                statusEffect.m_ttl = duration;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update splash mead duration: " + e);
            }
        }

        public static void UpdateRadius(GameObject prefab, float radius)
        {
            try
            {
                GameObject explPrefab = null;

                switch (prefab.name)
                {
                    case var value when value == Testing.Instance.prefabs.BarlyWineSplash.name:
                        explPrefab = Testing.Instance.prefabs.BarlyWineSplashExplosion;
                        break;
                    case var value when value == Testing.Instance.prefabs.FrostResistSplash.name:
                        explPrefab = Testing.Instance.prefabs.FrostResistSplashExplosion;
                        break;
                    case var value when value == Testing.Instance.prefabs.PoisonResistSplash.name:
                        explPrefab = Testing.Instance.prefabs.PoisonResistSplashExplosion;
                        break;
                }

                if (explPrefab == null)
                    throw new Exception("Could not find corresponding explosion prefab");

                if (radius < 2f)
                    radius = 2f;
                else if (radius > 10f)
                    radius = 10f;

                explPrefab.GetComponent<Aoe>().m_radius = radius;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update splash mead radius: " + e);
            }
        }
    }
}
