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

                ApplyHitFriendly(__instance);
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

                ApplyHitFriendly(character);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Tameable_Tame_Postfix: " + e);
            }
        }

        public static void OnTamedLoxDamageSettingChanged()
        {
            try
            {
                List<Character> allCharacters = Character.GetAllCharacters();

                foreach (Character character in allCharacters)
                {
                    if (IsLox(character) && character.IsTamed())
                        ApplyHitFriendly(character);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnTamedLoxDamageSettingChanged: " + e);
            }
        }

        private static void ApplyHitFriendly(Character lox)
        {
            if (!lox.IsTamed())
                return;

            Aoe aoe = GetRunHitDamagerAoe(lox);

            if (aoe == null)
            {
                Jotunn.Logger.LogWarning($"Could not find Aoe component on {RunHitDamagerName} of {lox.name}.");
                return;
            }

            aoe.m_hitFriendly = PluginConfig.configDamageTamedLoxEnable.Value;
            Jotunn.Logger.LogWarning($"Set m_hitFriendly to {aoe.m_hitFriendly} on {lox.name}.");
        }

        private static Aoe GetRunHitDamagerAoe(Character lox)
        {
            Aoe[] aoes = lox.GetComponentsInChildren<Aoe>(includeInactive: true);

            foreach (Aoe aoe in aoes)
            {
                if (aoe.gameObject.name == RunHitDamagerName)
                    return aoe;
            }

            return null;
        }

        private static bool IsLox(Character character)
        {
            return character != null && character.name.Contains(LoxName);
        }
    }
}