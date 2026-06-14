using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class PlayerScalePatchesWBTI
    {
        public struct MeleeRangeSnapshot
        {
            public float attackRange;
            public float attackHeight;
            public float attackOffset;
            public float attackRayWidth;
            public float attackHeightChar1;
            public float attackHeightChar2;
            public float attackRayWidthCharExtra;

            public static MeleeRangeSnapshot Capture(Attack attack)
            {
                return new MeleeRangeSnapshot
                {
                    attackRange = attack.m_attackRange,
                    attackHeight = attack.m_attackHeight,
                    attackOffset = attack.m_attackOffset,
                    attackRayWidth = attack.m_attackRayWidth,
                    attackHeightChar1 = attack.m_attackHeightChar1,
                    attackHeightChar2 = attack.m_attackHeightChar2,
                    attackRayWidthCharExtra = attack.m_attackRayWidthCharExtra,
                };
            }

            public void Restore(Attack attack)
            {
                attack.m_attackRange = attackRange;
                attack.m_attackHeight = attackHeight;
                attack.m_attackOffset = attackOffset;
                attack.m_attackRayWidth = attackRayWidth;
                attack.m_attackHeightChar1 = attackHeightChar1;
                attack.m_attackHeightChar2 = attackHeightChar2;
                attack.m_attackRayWidthCharExtra = attackRayWidthCharExtra;
            }
        }

        // Melee hit detection (range, height, offset, ray widths) is expressed in flat world
        // units, so it doesn't grow/shrink with the player's visual scale on its own. Temporarily
        // scale it for the duration of the swing so reach matches the (also scaled) weapon model.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
        public static void DoMeleeAttack_Prefix(Attack __instance, out MeleeRangeSnapshot? __state)
        {
            __state = null;

            try
            {
                if (!__instance.m_character.IsPlayer() || !ReferenceEquals(__instance.m_character, Player.m_localPlayer))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale == 1f)
                    return;

                __state = MeleeRangeSnapshot.Capture(__instance);

                __instance.m_attackRange *= scale;
                __instance.m_attackHeight *= scale;
                __instance.m_attackOffset *= scale;
                __instance.m_attackRayWidth *= scale;
                __instance.m_attackHeightChar1 *= scale;
                __instance.m_attackHeightChar2 *= scale;
                __instance.m_attackRayWidthCharExtra *= scale;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DoMeleeAttack_Prefix: " + e);
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
        public static Exception DoMeleeAttack_Finalizer(Attack __instance, MeleeRangeSnapshot? __state, Exception __exception)
        {
            try
            {
                __state?.Restore(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DoMeleeAttack_Finalizer: " + e);
            }

            return __exception;
        }

        // Projectile spawn point is the (correctly-scaled) attack origin joint plus flat
        // world-unit offsets (m_attackHeight/m_attackRange/m_attackOffset), so projectiles
        // always spawn at the unscaled height/offset relative to the player. Scale those
        // offsets for the duration of the calculation, same as the melee attack range above.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Attack), "GetProjectileSpawnPoint")]
        public static void GetProjectileSpawnPoint_Prefix(Attack __instance, out MeleeRangeSnapshot? __state)
        {
            __state = null;

            try
            {
                if (!__instance.m_character.IsPlayer() || !ReferenceEquals(__instance.m_character, Player.m_localPlayer))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale == 1f)
                    return;

                __state = MeleeRangeSnapshot.Capture(__instance);

                __instance.m_attackHeight *= scale;
                __instance.m_attackRange *= scale;
                __instance.m_attackOffset *= scale;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GetProjectileSpawnPoint_Prefix: " + e);
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Attack), "GetProjectileSpawnPoint")]
        public static Exception GetProjectileSpawnPoint_Finalizer(Attack __instance, MeleeRangeSnapshot? __state, Exception __exception)
        {
            try
            {
                __state?.Restore(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GetProjectileSpawnPoint_Finalizer: " + e);
            }

            return __exception;
        }

        // Re-equipping replaces the held/sheathed item GameObjects with fresh instances at
        // scale 1, so re-apply the player's current scale whenever that happens.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetRightHandEquipped))]
        public static void SetRightHandEquipped_Postfix(VisEquipment __instance, bool __result)
        {
            try
            {
                if (!__result || !IsLocalPlayerVisEquipment(__instance))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale != 1f)
                    PlayerScaleHelper.SetWorldScale(__instance.m_rightItemInstance, scale);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetRightHandEquipped_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetLeftHandEquipped))]
        public static void SetLeftHandEquipped_Postfix(VisEquipment __instance, bool __result)
        {
            try
            {
                if (!__result || !IsLocalPlayerVisEquipment(__instance))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale != 1f)
                    PlayerScaleHelper.SetWorldScale(__instance.m_leftItemInstance, scale);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetLeftHandEquipped_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetBackEquipped))]
        public static void SetBackEquipped_Postfix(VisEquipment __instance, bool __result)
        {
            try
            {
                if (!__result || !IsLocalPlayerVisEquipment(__instance))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale == 1f)
                    return;

                PlayerScaleHelper.SetWorldScale(__instance.m_leftBackItemInstance, scale);
                PlayerScaleHelper.SetWorldScale(__instance.m_rightBackItemInstance, scale);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetBackEquipped_Postfix: " + e);
            }
        }

        // Helmet/hair/beard visuals are attached to the head joint the same way back items
        // are attached to the back joints, so they need the same re-apply on (re)equip.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), "SetHelmetEquipped")]
        public static void SetHelmetEquipped_Postfix(VisEquipment __instance, bool __result)
        {
            try
            {
                if (!__result || !IsLocalPlayerVisEquipment(__instance))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale != 1f)
                    PlayerScaleHelper.SetWorldScale(__instance.m_helmetItemInstance, scale);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetHelmetEquipped_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), "SetHairEquipped")]
        public static void SetHairEquipped_Postfix(VisEquipment __instance, bool __result)
        {
            try
            {
                if (!__result || !IsLocalPlayerVisEquipment(__instance))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale != 1f)
                    PlayerScaleHelper.SetWorldScale(__instance.m_hairItemInstance, scale);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetHairEquipped_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), "SetBeardEquipped")]
        public static void SetBeardEquipped_Postfix(VisEquipment __instance, bool __result)
        {
            try
            {
                if (!__result || !IsLocalPlayerVisEquipment(__instance))
                    return;

                float scale = PlayerScaleHelper.CurrentScale;

                if (scale != 1f)
                    PlayerScaleHelper.SetWorldScale(__instance.m_beardItemInstance, scale);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in SetBeardEquipped_Postfix: " + e);
            }
        }

        private static bool IsLocalPlayerVisEquipment(VisEquipment vis)
        {
            return Player.m_localPlayer != null && ReferenceEquals(vis, Player.m_localPlayer.m_visEquipment);
        }

        // Character.GetRadius/GetHeight special-case the player to always return the
        // unscaled collider values (Valheim assumes the player is never resized), so enemy
        // AI and attack-range checks still treat a shrunk/grown player as normal-sized even
        // though the collider itself is scaled. Scale the result to match the player's
        // actual current size.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetRadius))]
        public static void GetRadius_Postfix(Character __instance, ref float __result)
        {
            try
            {
                if (!ReferenceEquals(__instance, Player.m_localPlayer))
                    return;

                float scale = __instance.transform.localScale.x;

                if (scale != 1f)
                    __result *= scale;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GetRadius_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetHeight))]
        public static void GetHeight_Postfix(Character __instance, ref float __result)
        {
            try
            {
                if (!ReferenceEquals(__instance, Player.m_localPlayer))
                    return;

                float scale = __instance.transform.localScale.x;

                if (scale != 1f)
                    __result *= scale;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in GetHeight_Postfix: " + e);
            }
        }
    }
}
