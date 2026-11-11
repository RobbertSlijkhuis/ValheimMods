using HarmonyLib;
using ModularMagic_IceStaffs.Helpers;
using System;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_IceStaffs.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Start")]
        public static void GameStart_Postfix()
        {
            IceImbuementHelper.GameIsReady = true;
        }

        // The shards of the Rain rune are spawned without hit data, so they get the damage of the staff here, see IceImbuementHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnAbility), "SetupProjectile")]
        public static void SetupProjectile_Postfix(Projectile projectile, ItemData ___m_weapon)
        {
            IceImbuementHelper.OnProjectileSpawned(projectile, ___m_weapon);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(Humanoid __instance, ItemData item)
        {
            try
            {
                if (__instance == null || !__instance.IsPlayer() || item == null || !IceImbuementHelper.GameIsReady)
                    return;

                if (!CoreImbuementHelper.HasImbuements(item) || !IceImbuementHelper.IsIceStaff(item))
                    return;

                IceImbuementHelper.ApplyImbuements(item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }

        [HarmonyPatch(typeof(Attack), "Start")]
        [HarmonyPrefix]
        public static bool AttackStart_Prefix(Humanoid character, Attack __instance)
        {
            if (__instance == null || character == null)
                return true;

            ItemDrop.ItemData weapon = character.GetCurrentWeapon();

            if (weapon == null || weapon.m_dropPrefab == null)
                return true;

            if (weapon.m_dropPrefab.name == ModularMagic_IceStaffs.Instance.prefabs.StaffIceAOEPrefab.name)
            {
                character.GetSEMan().AddStatusEffect(ModularMagic_IceStaffs.Instance.effects.WalkFastSE);
                return true;
            }

            return true;
        }
    }
}
