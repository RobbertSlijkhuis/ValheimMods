using HarmonyLib;
using ModularMagic_BloodMagic.Helpers;
using System.Collections.Generic;

namespace ModularMagic_BloodMagic.Harmony
{
    [HarmonyPatch]
    public class ApparitionPatches
    {
        // An apparition does not die from losing its health: without health it is a husk, see ApparitionController. This is the only place
        // where the game lets a creature die, and it only runs on the client that owns the creature. A husk only dies when its summoner
        // hits it, see CharacterRPCDamage_Prefix
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "CheckDeath")]
        public static bool CharacterCheckDeath_Prefix(Character __instance)
        {
            return !ApparitionHelper.IsApparition(__instance);
        }

        // The game ignores a hit on a creature without health, so a husk can not be hit. A hit of its summoner kills it, so the summoner
        // can clear their apparitions. The hit is sent to the client that owns the creature, which decides
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "RPC_Damage")]
        public static bool CharacterRPCDamage_Prefix(Character __instance, HitData hit)
        {
            return !ApparitionHelper.TryKillHusk(__instance, hit);
        }

        // The game sets the max health of a creature back to the one of its prefab when it is loaded at full health. The max health of an
        // apparition comes from the scythe that made it and is set once when it is summoned
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SetupMaxHealth")]
        public static bool CharacterSetupMaxHealth_Prefix(Character __instance)
        {
            return !ApparitionHelper.HasApparitionData(__instance);
        }

        // An enemy that was attacking an apparition drops it as its target when it becomes a husk: the AI of the game keeps a target for
        // as long as it is an enemy. A new target is not picked because of Character.m_aiSkipTarget, see ApparitionController. Only this
        // method of the AI is patched, the static IsEnemy also decides who is a friend to heal and what an attack can hit
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.IsEnemy), new[] { typeof(Character) })]
        public static void BaseAIIsEnemy_Postfix(Character other, ref bool __result)
        {
            __result = __result && !ApparitionHelper.IsHusk(other);
        }

        // Using a husk reanimates it for its summoner, the game would pet it
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Tameable), "Interact")]
        public static bool TameableInteract_Prefix(Tameable __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            return !ApparitionHelper.TryUseHusk(__instance, user, hold, alt, out __result);
        }

        // A husk shows that it is banished and what it costs to reanimate it
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tameable), "GetHoverText")]
        public static void TameableGetHoverText_Postfix(Tameable __instance, ref string __result)
        {
            __result = ApparitionHelper.GetHuskHoverText(__instance) ?? __result;
        }

        // Every way the game removes a summon (its summoner too far away, or gone for too long) goes through UnSummon, on the client that
        // owns it. The config can keep apparitions in the world, see ApparitionHelper.CanUnsummon
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Tameable), "UnSummon")]
        public static bool TameableUnSummon_Prefix(Tameable __instance)
        {
            return ApparitionHelper.CanUnsummon(__instance);
        }

        // A smaller apparition finds what its melee or area attack hits from its own size, see AttackScaleHelper. The numbers are put
        // back in the finalizer, which also runs when the game throws
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
        [HarmonyPatch(typeof(Attack), "DoAreaAttack")]
        public static void AttackHitArea_Prefix(Attack __instance, out AttackScaleHelper.AttackGeometrySnapshot? __state)
        {
            __state = AttackScaleHelper.ScaleHitArea(__instance);
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
        [HarmonyPatch(typeof(Attack), "DoAreaAttack")]
        public static void AttackHitArea_Finalizer(Attack __instance, AttackScaleHelper.AttackGeometrySnapshot? __state)
        {
            __state?.Restore(__instance);
        }

        // The arrow of a smaller apparition starts from its own bow, see AttackScaleHelper
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Attack), "GetProjectileSpawnPoint")]
        public static void AttackGetProjectileSpawnPoint_Prefix(Attack __instance, out AttackScaleHelper.AttackGeometrySnapshot? __state)
        {
            __state = AttackScaleHelper.ScaleProjectileSpawn(__instance);
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Attack), "GetProjectileSpawnPoint")]
        public static void AttackGetProjectileSpawnPoint_Finalizer(Attack __instance, AttackScaleHelper.AttackGeometrySnapshot? __state)
        {
            __state?.Restore(__instance);
        }

        // A smaller apparition starts its attack at its own reach. The AI picks the moment and the weapon in UpdateAI, see AttackScaleHelper
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
        public static void MonsterAIUpdateAI_Prefix(MonsterAI __instance, out List<AttackScaleHelper.AiRangeSnapshot>? __state)
        {
            __state = AttackScaleHelper.ScaleAiAttackRanges(__instance.m_character as Humanoid);
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
        public static void MonsterAIUpdateAI_Finalizer(List<AttackScaleHelper.AiRangeSnapshot>? __state)
        {
            AttackScaleHelper.RestoreAiAttackRanges(__state);
        }
    }
}
