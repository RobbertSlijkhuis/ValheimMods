using HarmonyLib;
using Jotunn.Managers;
using ModularMagic_Armors.Components;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Harmony
{
    [HarmonyPatch]
    public class PatchesMMA
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerController), "Awake")]
        public static void AwakePlayerController_Postfix(ref PlayerController __instance)
        {
            try
            {
                __instance.gameObject.AddComponent<ArmorMMA>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add component in AwakePlayerController_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(ref Player __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                ArmorMMA comp = __instance.GetComponent<ArmorMMA>();
                long playerId = __instance.GetPlayerID();
                int? armorSetHash = comp.GetArmorSetFromPlayer(playerId);
                List<int> armorPieces = comp.GetArmorPiecesFromPlayer(playerId);
                ArmorStatus armorStatus = new ArmorStatus(playerId, armorSetHash, armorPieces);
                comp.SetPlayerStatus(playerId, armorStatus);

                if (!ModularMagic_Armors.gameIsReady)
                    ModularMagic_Armors.gameIsReady = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ItemDrop.ItemData item)
        {
            try
            {
                if (!ModularMagic_Armors.gameIsReady || item == null)
                    return;

                _CheckEquipment();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static void UnequipItem_Postfix(ItemDrop.ItemData item)
        {
            try
            {
                if (!ModularMagic_Armors.gameIsReady || item == null)
                    return;

                _CheckEquipment();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/sets effects in UnequipItem_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            try
            {
                if (__instance == null)
                    return;

                _SetEitr(__instance, PluginConfig.armor1Helmet.magicStatusEffectName, PluginConfig.armor1Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor1Cape.magicStatusEffectName, PluginConfig.armor1Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor1Chest.magicStatusEffectName, PluginConfig.armor1Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor1Legs.magicStatusEffectName, PluginConfig.armor1Legs.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Helmet.magicStatusEffectName, PluginConfig.armor2Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Cape.magicStatusEffectName, PluginConfig.armor2Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Chest.magicStatusEffectName, PluginConfig.armor2Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Legs.magicStatusEffectName, PluginConfig.armor2Legs.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Helmet.magicStatusEffectName, PluginConfig.armor3Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Cape.magicStatusEffectName, PluginConfig.armor3Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Chest.magicStatusEffectName, PluginConfig.armor3Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Legs.magicStatusEffectName, PluginConfig.armor3Legs.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Helmet.magicStatusEffectName, PluginConfig.armor4Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Cape.magicStatusEffectName, PluginConfig.armor4Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Chest.magicStatusEffectName, PluginConfig.armor4Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Legs.magicStatusEffectName, PluginConfig.armor4Legs.eitr.Value, ref eitr);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update Eitr in GetTotalFoodValue_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Skills), "GetSkillLevel")]
        public static void GetSkillLevel_Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
        {
            try
            {
                if (skillType != Skills.SkillType.ElementalMagic && skillType != Skills.SkillType.BloodMagic)
                    return;

                float value = 0f;

                if (_HaveStatusEffect(__instance, PluginConfig.armor1Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor1Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor1Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor1Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Legs);

                if (_HaveStatusEffect(__instance, PluginConfig.armor2Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Legs);

                if (_HaveStatusEffect(__instance, PluginConfig.armor3Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor3Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor3Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor3Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Legs);

                if (_HaveStatusEffect(__instance, PluginConfig.armor2Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Legs);

                float newLevel = __result + value;
                __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref newLevel);
                __result = newLevel;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update skills in GetSkillLevel_Postfix: " + e);
            }
        }

        private static void _CheckEquipment()
        {
            if (Player.m_localPlayer == null)
                return;

            ArmorMMA comp = Player.m_localPlayer.GetComponent<ArmorMMA>();
            ArmorStatus armorStatus = comp.GetPlayerStatus();

            if (armorStatus == null)
                return;

            long playerId = Player.m_localPlayer.GetPlayerID();
            int? armorSetHash = comp.GetArmorSetFromPlayer(playerId);
            List<int> armorPieces = comp.GetArmorPiecesFromPlayer(playerId);
            armorStatus.set = armorSetHash;
            armorStatus.items = armorPieces;
            comp.SetPlayerStatus(playerId, armorStatus);
        }

        private static void _SetEitr(Player player, string name, float amount, ref float eitr)
        {
            if (player == null)
                return;

            bool hasEffect = player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));

            if (!hasEffect)
                return;

            eitr += amount;
        }

        private static bool _HaveStatusEffect(Skills __instance, string statusEffect)
        {
            return __instance.m_player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(statusEffect));
        }

        private static float _GetValueFromConfig(Skills.SkillType skillType, ArmorConfig config)
        {
            if (skillType == Skills.SkillType.ElementalMagic)
                return config.elementalMagic.Value;
            else if (skillType == Skills.SkillType.BloodMagic)
                return config.bloodMagic.Value;
            else return 0f;
        }
    }
}
