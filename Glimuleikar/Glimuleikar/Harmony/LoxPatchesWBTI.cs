using Glimuleikar.Configs;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class LoxPatchesWBTI
    {
        private const string LoxName = "Lox";
        private const string RunHitDamagerName = "RunHitDamager";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "Awake")]
        public static void Character_Awake_Postfix(Character __instance)
        {
            try
            {
                if (!IsLox(__instance))
                    return;

                ApplyMovementDamage(__instance);
                ApplyOthers(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Character_Awake_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tameable), "Tame")]
        public static void Tameable_Tame_Postfix(Tameable __instance)
        {
            try
            {
                Character character = __instance.GetComponent<Character>();

                if (character == null || !IsLox(character))
                    return;

                ApplyMovementDamage(character);
                ApplyOthers(character);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Tameable_Tame_Postfix: " + e);
            }
        }

        public static void OnTamedLoxChange()
        {
            try
            {
                List<Character> allCharacters = Character.GetAllCharacters();

                foreach (Character character in allCharacters)
                {
                    if (IsLox(character) && character.IsTamed())
                    {
                        ApplyMovementDamage(character);
                        ApplyOthers(character);
                    }
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnTamedLoxDamageSettingChanged: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Sadle), "RPC_RequestRespons")]
        public static void Sadle_RPC_RequestRespons_Postfix(Sadle __instance)
        {
            try
            {
                Character lox = __instance.GetComponent<Character>();
                if (lox == null || !IsLox(lox)) return;

                ApplyMovementDamage(lox);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Sadle_RPC_RequestRespons_Postfix: " + e);
            }
        }

        private static void ApplyMovementDamage(Character lox)
        {
            if (!lox.IsTamed())
                return;

            Transform transform = lox.transform.Find("Visual/RunHitDamager");

            if (transform == null)
            {
                Jotunn.Logger.LogWarning($"Could not find RunHitDamager child object on {lox?.name}.");
                return;
            }   

            Aoe aoe = transform.gameObject.GetComponent<Aoe>();
            BoxCollider boxCollider = transform.gameObject.GetComponent<BoxCollider>();
            MovementDamage movementDamager = lox.GetComponent<MovementDamage>();

            if (aoe == null)
            {
                Jotunn.Logger.LogWarning($"Could not find Aoe component on {RunHitDamagerName} of {lox?.name}.");
                return;
            }

            if (boxCollider == null)
            {
                Jotunn.Logger.LogWarning($"Could not find BoxCollider component on {RunHitDamagerName} of {lox?.name}.");
                return;
            }

            if (movementDamager == null)
            {
                Jotunn.Logger.LogWarning($"Could not find MovementDamage component on {lox?.name}.");
                return;
            }

            movementDamager.m_speedTreshold = PluginConfig.configTamedLoxSpeedThreshold.Value;
            aoe.m_hitFriendly = PluginConfig.configTamedLoxDamageFriendliesEnable.Value;
            boxCollider.center = PluginConfig.configTamedLoxDamageBoxPosition.Value;
            boxCollider.size = PluginConfig.configTamedLoxDamageBoxScale.Value;
            Jotunn.Logger.LogWarning($"Set m_hitFriendly to {aoe.m_hitFriendly} on {lox.name}.");
        }

        private static void ApplyOthers(Character lox)
        {
            Humanoid humanoid = lox.GetComponent<Humanoid>();

            if (humanoid == null)
            {
                Jotunn.Logger.LogWarning($"Could not find Humanoid component on {lox?.name}.");
                return;
            }

            humanoid.m_turnSpeed = PluginConfig.configTamedLoxTurningSpeed.Value;
        }

        private static bool IsLox(Character character)
        {
            return character != null && character.name.Contains(LoxName);
        }
    }
}