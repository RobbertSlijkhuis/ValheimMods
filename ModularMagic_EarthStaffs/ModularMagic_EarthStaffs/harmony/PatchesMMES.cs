using HarmonyLib;
using ModularMagic_EarthStaffs.Helpers;
using System;
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
                EarthImbuementHelper.GameIsReady = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GameStart_Postfix: " + e);
            }
        }

        // The terrain op of the nova must be in the table of every ObjectDB, see TerrainOpHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ObjectDB), "UpdateRegisters")]
        public static void ObjectDBUpdateRegisters_Postfix(ObjectDB __instance)
        {
            TerrainOpHelper.Register(__instance);
        }

        // The boulders of the Rain rune are spawned without hit data, so they get the damage of the staff here, see EarthImbuementHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnAbility), "SetupProjectile")]
        public static void SetupProjectile_Postfix(Projectile projectile, ItemData ___m_weapon)
        {
            EarthImbuementHelper.OnProjectileSpawned(projectile, ___m_weapon);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ref Humanoid __instance, ItemData item)
        {
            try
            {
                if (__instance == null || !__instance.IsPlayer() || item == null || !EarthImbuementHelper.GameIsReady)
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
    }
}
