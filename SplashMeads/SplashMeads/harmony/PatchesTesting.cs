using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Numerics;
using SplashMeads.Configs;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace SplashMeads.Harmony
{
    [HarmonyPatch]
    public class PatchesTesting
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static void ApplyDamage_Postfix(ref Character __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return;

                RefreshSplash(__instance, hit.m_statusEffectHash);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in ApplyDamage_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "ConsumeItem")]
        public static void ConsumeItem_Postfix(ref Player __instance, ItemDrop.ItemData item)
        {
            try
            {
                if (__instance == null || item == null)
                    return;

                string itemName = item.m_shared.m_consumeStatusEffect?.name;

                if (itemName == null)
                    return;

                int effectHash = itemName.GetStableHashCode();

                if (effectHash == 0)
                    return;

                RemoveSplashWhenConsumeNormal(__instance, effectHash);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in ConsumeItem_Postfix: " + e);
            }
        }

        private static void RefreshSplash(Character character, int effectHash)
        {
            if (effectHash == SplashMeads.barleyWineSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.barleyWineSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.barleyWineSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.barleyWineHash, PluginConfig.mead1.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(SplashMeads.barleyWineSplashHash);
            }

            else if (effectHash == SplashMeads.frostResistSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.frostResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.frostResistSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.frostResistHash, PluginConfig.mead2.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(SplashMeads.frostResistSplashHash);
            }

            else if (effectHash == SplashMeads.poisonResistSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.poisonResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.poisonResistSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.poisonResistHash, PluginConfig.mead3.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(SplashMeads.poisonResistSplashHash);
            }
        }

        private static void RemoveSplashWhenConsumeNormal(Player player, int effectHash)
        {
            if (effectHash == SplashMeads.barleyWineHash && player.GetSEMan().HaveStatusEffect(SplashMeads.barleyWineSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.barleyWineSplashHash);

            else if (effectHash == SplashMeads.frostResistHash && player.GetSEMan().HaveStatusEffect(SplashMeads.frostResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.frostResistSplashHash);

            else if (effectHash == SplashMeads.poisonResistHash && player.GetSEMan().HaveStatusEffect(SplashMeads.poisonResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.poisonResistSplashHash);
        }

        private static bool ShouldSplashRefresh(Character character, int originalEffectHash, int splashDuration, bool removeOriginal = false)
        {
            if (!character.GetSEMan().HaveStatusEffect(originalEffectHash))
                return true;

            StatusEffect currentOriginal = character.GetSEMan().GetStatusEffect(originalEffectHash);

            if (currentOriginal.GetRemaningTime() < splashDuration)
            {
                if (removeOriginal)
                    character.GetSEMan().RemoveStatusEffect(currentOriginal);

                return true;
            }

            return false;
        }
    }
}
