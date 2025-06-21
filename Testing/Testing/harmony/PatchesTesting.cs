using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Numerics;
using Testing.Configs;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace Testing.Harmony
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
            if (effectHash == Testing.barleyWineSplashHash && character.GetSEMan().HaveStatusEffect(Testing.barleyWineSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(Testing.barleyWineSplashHash);

                if (ShouldSplashRefresh(character, Testing.barleyWineHash, PluginConfig.mead1.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(Testing.barleyWineSplashHash);
            }

            else if (effectHash == Testing.frostResistSplashHash && character.GetSEMan().HaveStatusEffect(Testing.frostResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(Testing.frostResistSplashHash);

                if (ShouldSplashRefresh(character, Testing.frostResistHash, PluginConfig.mead2.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(Testing.frostResistSplashHash);
            }

            else if (effectHash == Testing.poisonResistSplashHash && character.GetSEMan().HaveStatusEffect(Testing.poisonResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(Testing.poisonResistSplashHash);

                if (ShouldSplashRefresh(character, Testing.poisonResistHash, PluginConfig.mead3.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(Testing.poisonResistSplashHash);
            }
        }

        private static void RemoveSplashWhenConsumeNormal(Player player, int effectHash)
        {
            if (effectHash == Testing.barleyWineHash && player.GetSEMan().HaveStatusEffect(Testing.barleyWineSplashHash))
                player.GetSEMan().RemoveStatusEffect(Testing.barleyWineSplashHash);

            else if (effectHash == Testing.frostResistHash && player.GetSEMan().HaveStatusEffect(Testing.frostResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(Testing.frostResistSplashHash);

            else if (effectHash == Testing.poisonResistHash && player.GetSEMan().HaveStatusEffect(Testing.poisonResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(Testing.poisonResistSplashHash);
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
