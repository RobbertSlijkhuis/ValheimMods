using HarmonyLib;
using ModularMagic_LightningStaffs.Helpers;
using System;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_LightningStaffs.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Start")]
        public static void GameStart_Postfix()
        {
            LightningImbuementHelper.GameIsReady = true;
        }

        // The Attack Speed rune speeds up the animation of the normal attack, see AttackSpeedHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "StartAttack")]
        public static void StartAttack_Postfix(Humanoid __instance, bool secondaryAttack, bool __result)
        {
            if (__result)
                AttackSpeedHelper.OnAttackStarted(__instance, secondaryAttack);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UpdateAttack")]
        public static void UpdateAttack_Postfix(Humanoid __instance)
        {
            AttackSpeedHelper.OnAttackUpdated(__instance);
        }

        // The bolts of the Rain rune are spawned without hit data, so they get the damage of the staff here, see LightningImbuementHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnAbility), "SetupProjectile")]
        public static void SetupProjectile_Postfix(Projectile projectile, ItemData ___m_weapon)
        {
            LightningImbuementHelper.OnProjectileSpawned(projectile, ___m_weapon);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(Humanoid __instance, ItemData item)
        {
            try
            {
                if (__instance == null || !__instance.IsPlayer() || item == null || !LightningImbuementHelper.GameIsReady)
                    return;

                if (!CoreImbuementHelper.HasImbuements(item) || !LightningImbuementHelper.IsLightningStaff(item))
                    return;

                LightningImbuementHelper.ApplyImbuements(item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }
    }
}
