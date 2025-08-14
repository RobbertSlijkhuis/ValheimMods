using System;
using System.Collections.Generic;
using SplashMeads.Models;
using UnityEngine;
using static ItemDrop;

namespace SplashMeads.Helpers
{
    internal class UpdateHelper
    {
        public static void UpdateItemData(GameObject prefab, UpdateItemDataOptions options)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            ItemData itemData = prefab.GetComponent<ItemDrop>().m_itemData;
            UpdateItemData(itemData, options);
            UpdateItemDataInInventory(itemData, options);
        }

        public static void UpdateItemData(ItemData itemData, UpdateItemDataOptions options)
        {
            if (itemData == null)
                throw new Exception("ItemData is null");

            if (options.name != null) { itemData.m_shared.m_name = options.name; }
            if (options.description != null) { itemData.m_shared.m_description = options.description; }
            if (options.weight != null) { itemData.m_shared.m_weight = (float)options.weight; }
            if (options.maxStackSize != null) { itemData.m_shared.m_maxStackSize = (int)options.maxStackSize; }
        }

        public static void UpdateItemDataInInventory(ItemData itemData, UpdateItemDataOptions options)
        {
            if (Player.m_localPlayer == null)
                return;

            Inventory inventory = Player.m_localPlayer.GetInventory();

            if (inventory == null)
                return;

            List<ItemData> items = inventory.GetAllItems().FindAll(item => item.m_shared.m_name == itemData.m_shared.m_name);

            if (items == null)
                throw new Exception("Could not find items from Inventory");

            foreach (ItemData item in items)
            {
                UpdateItemData(item, options);
            }
        }

        public static void UpdateFXEnabled(GameObject prefab, bool value)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            Transform lingeringEffects = prefab.transform.Find("lingering_effects/particles");

            if (lingeringEffects == null)
                return;

            lingeringEffects.gameObject.SetActive(value);
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
                    case var value when value == SplashMeads.Instance.prefabs.VananidirSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.VananidirSplashFX;
                        statusEffect = SplashMeads.Instance.effects.VananidirSplash;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.AntiStingSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.AntiStingSplashFX;
                        statusEffect = SplashMeads.Instance.effects.AntiStingSplash;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.MajorHealthSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.MajorHealthSplashFX;
                        statusEffect = SplashMeads.Instance.effects.MajorHealthSplash;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.MediumHealthSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.MediumHealthSplashFX;
                        statusEffect = SplashMeads.Instance.effects.MediumHealthSplash;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.MinorHealthSplash.name:
                        fxPrefab = SplashMeads.Instance.prefabs.MinorHealthSplashFX;
                        statusEffect = SplashMeads.Instance.effects.MinorHealthSplash;
                        break;
                }

                if (fxPrefab == null || statusEffect == null)
                    throw new Exception("Could not find corresponding fx prefab or statuseffect");

                fxPrefab.GetComponent<TimedDestruction>().m_timeout = duration;
                statusEffect.m_ttl = duration;

                Transform flareTrans = fxPrefab.transform.Find("lingering_effects/flare");
                Transform flakeTrans = fxPrefab.transform.Find("lingering_effects/flakes_up");
                Transform particlesTrans = fxPrefab.transform.Find("lingering_effects/particles");

                if (flareTrans != null)
                {
                    ParticleSystem.MainModule flareMain = flareTrans.gameObject.GetComponent<ParticleSystem>().main;
                    flareMain.duration = duration;
                    flareMain.startLifetime = duration;
                }

                if (flakeTrans != null)
                {
                    ParticleSystem.MainModule flakesMain = flakeTrans.gameObject.GetComponent<ParticleSystem>().main;
                    flakesMain.duration = duration - 3;
                }

                if (particlesTrans != null)
                {
                    ParticleSystem.MainModule particlesMain = particlesTrans.gameObject.GetComponent<ParticleSystem>().main;
                    particlesMain.duration = duration - 3;
                }
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
                    case var value when value == SplashMeads.Instance.prefabs.VananidirSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.VananidirSplashExplosion;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.AntiStingSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.AntiStingSplashExplosion;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.MajorHealthSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.MajorHealthSplashExplosion;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.MediumHealthSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.MediumHealthSplashExplosion;
                        break;
                    case var value when value == SplashMeads.Instance.prefabs.MinorHealthSplash.name:
                        explPrefab = SplashMeads.Instance.prefabs.MinorHealthSplashExplosion;
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
