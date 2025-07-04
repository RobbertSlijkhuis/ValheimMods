using Jotunn.Managers;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using SplashMeads.Models;
using UnityEngine;

namespace SplashMeads.Helpers
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
                    case var value when value == SplashMeads.Instance.prefabs.BarlyWineSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.BarlyWineSplashFX;
                        statusEffect = SplashMeads.Instance.effects.BarlyWineSplash;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.FrostResistSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.FrostResistSplashFX;
                        statusEffect = SplashMeads.Instance.effects.FrostResistSplash;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.PoisonResistSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.PoisonResistSplashFX;
                        statusEffect = SplashMeads.Instance.effects.PoisonResistSplash;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.RatatoskSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.RatatoskSplashFX;
                        statusEffect = SplashMeads.Instance.effects.RatatoskSplash;
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
                    case var value when value == SplashMeads.Instance.prefabs.BarlyWineSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.BarlyWineSplashExplosion;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.FrostResistSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.FrostResistSplashExplosion;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.PoisonResistSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.PoisonResistSplashExplosion;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.RatatoskSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.RatatoskSplashExplosion;
                        break;
                }

                if (explPrefab == null)
                    throw new Exception("Could not find corresponding explosion prefab");

                if (radius < 2f)
                    radius = 2f;
                else if (radius > 10f)
                    radius = 10f;

                explPrefab.GetComponent<Aoe>().m_radius = radius;

                Transform wetSplash = explPrefab.transform.Find("particles/wetsplsh");

                if (wetSplash == null)
                    throw new Exception("Could not find wet splash transform");

                float scaled = 0.25f * radius;
                wetSplash.localScale = new Vector3(scaled, scaled, scaled);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update splash mead radius: " + e);
            }
        }
    }
}
