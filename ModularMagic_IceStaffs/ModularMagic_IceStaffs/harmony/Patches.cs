using HarmonyLib;
using ModularMagic_IceStaffs.Helpers;
using System;
using UnityEngine;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_IceStaffs.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
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

        // The ice sheet on the water a frost shot hits, see IceSheetHelper
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        [HarmonyPostfix]
        public static void ProjectileOnHit_Postfix(Projectile __instance, Vector3 hitPoint, bool water, ItemData ___m_weapon)
        {
            IceSheetHelper.OnProjectileHit(__instance, ___m_weapon, hitPoint, water);
        }
    }
}
