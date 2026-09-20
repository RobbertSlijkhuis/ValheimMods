using HarmonyLib;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Helpers;
using System;
using UnityEngine;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_EarthStaffs.Harmony
{
    [HarmonyPatch]
    public class PatchesMMES
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Start")]
        public static void GameStart_Postfix()
        {
            try
            {
                ModularMagic_EarthStaffs.gameIsReady = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GameStart_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ref Humanoid __instance, ItemData item)
        {
            try
            {
                if (__instance == null || !__instance.IsPlayer() || item == null || !ModularMagic_EarthStaffs.gameIsReady)
                    return;

                if (!CoreImbuementHelper.HasImbuements(item) || !EarthImbuementHelper.IsEarthStaff(item))
                    return;

                EarthImbuementHelper.ApplyImbuements(item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }

        [HarmonyPatch(typeof(Attack), "Start")]
        [HarmonyPrefix]
        public static bool AttackStart_Prefix(Attack __instance, Humanoid character)
        {
            try
            {
                if (__instance == null || character == null)
                    return true;

                if (!AttackHelper.IsSecondaryAttack(__instance))
                    return true;

                return CheckForCooldown(character, __instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in AttackStart_Prefix: " + e);
                return true;
            }
        }

        /// <summary>
        /// The game uses the eitr cost of the main attack of the weapon for every attack, also the secondary one.
        /// The secondary attacks of the staffs use their own cost, so the eitr saving of the runes works.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackEitr), new[] { typeof(Character), typeof(ItemData) })]
        public static void GetAttackEitr_Postfix(Attack __instance, Character character, ItemData weapon, ref float __result)
        {
            if (character == null || weapon == null || !AttackHelper.IsSecondaryAttack(__instance))
                return;

            __result = AttackHelper.GetSecondaryAttackEitr(__instance, character, weapon);
        }

        private static bool CheckForCooldown(Humanoid character, Attack attack)
        {
            float cooldownValue = 0f;
            float eitrValue = 0f;
            bool hasEffect = false;
            StatusEffect statusEffect = null;

            switch (attack.m_attackProjectile.name)
            {
                case "projectile_spawn_boulder_MMES":
                    cooldownValue = PluginConfig.secondaryAttackRain.cooldown.Value;
                    eitrValue = AttackHelper.GetSecondaryAttackEitr(attack, character, character.GetCurrentWeapon());
                    statusEffect = ModularMagic_EarthStaffs.effects.BoulderCooldown;
                    hasEffect = character.GetSEMan().HaveStatusEffect(statusEffect.name.GetHashCode());
                    break;
                case "script_roots_MMES":
                    cooldownValue = PluginConfig.secondaryAttackSummon.cooldown.Value;
                    eitrValue = AttackHelper.GetSecondaryAttackEitr(attack, character, character.GetCurrentWeapon());
                    statusEffect = ModularMagic_EarthStaffs.effects.RootsCooldown;
                    hasEffect = character.GetSEMan().HaveStatusEffect(statusEffect.name.GetHashCode());
                    break;
            }

            if (statusEffect == null || cooldownValue == 0f)
                return true;

            if (!hasEffect)
            {
                if (!character.HaveEitr(eitrValue))
                {
                    character.Message(MessageHud.MessageType.Center, "You do not have enough Eitr to perform this action!");
                    return true;
                }

                character.GetSEMan().AddStatusEffect(statusEffect);
                return true;
            }

            character.Message(MessageHud.MessageType.Center, "The staff is still recharging!");
            return false;
        }
    }
}
