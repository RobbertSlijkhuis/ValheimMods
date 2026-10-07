using HarmonyLib;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Harmony
{
    // Creature-behavior patches backing CreatureData.fullyPassive/alwaysFollowOwner (set from the
    // creature form's Behavior tab). See the field comments on CreatureData.
    [HarmonyPatch]
    public class SpecialRedeemPatchesWBTI
    {
        // BaseAI.IsEnemy(Character) is what both FindEnemy() and UpdateTarget()'s immediate
        // re-validation of a just-found target funnel through - forcing it false here means a
        // fully-passive creature never keeps a target long enough to reach DoAttack, for wild
        // monsters and the player alike (unlike a plain tamed creature, which still fights off
        // monsters that get close - see BaseAI.IsEnemy's tamed-vs-non-tamed branch).
        //
        // Also short-circuits when the *other* character is our fully-passive creature, so wild
        // monsters never pick it as a target in the first place. Faction alone can't do this: the
        // creature is tamed (friendly -> MakeTame()), and IsEnemy's tamed-vs-non-tamed branch
        // always treats a non-tamed attacker as an enemy of a tamed target regardless of faction
        // (same rule that lets wolves attack your tamed lox). This only gates AI target
        // acquisition, not damage, so players can still kill it manually.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.IsEnemy), new Type[] { typeof(Character) })]
        public static bool IsEnemy_Prefix(BaseAI __instance, Character other, ref bool __result)
        {
            TwitchCreaturePersistentData selfData = __instance.GetComponent<TwitchCreaturePersistentData>();

            if (selfData != null && selfData.IsFullyPassive)
            {
                __result = false;
                return false;
            }

            TwitchCreaturePersistentData otherData = other != null ? other.GetComponent<TwitchCreaturePersistentData>() : null;

            if (otherData != null && otherData.IsFullyPassive)
            {
                __result = false;
                return false;
            }

            return true;
        }

        // Vanilla BaseAI.Follow hardcodes StopMoving() once within 3m of the followed target
        // (confirmed by decompiling assembly_valheim.dll) - a normal followed/tamed creature keeps
        // a bit of distance. Skip that early-out for our marked creature so it keeps closing the
        // gap instead, mirroring Follow's own logic otherwise. MoveTo is protected on BaseAI, so
        // it's invoked via Traverse rather than a direct call.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseAI), "Follow")]
        public static bool Follow_Prefix(BaseAI __instance, GameObject go, float dt)
        {
            TwitchCreaturePersistentData persistentData = __instance.GetComponent<TwitchCreaturePersistentData>();

            if (persistentData == null || !persistentData.WantsToCloseDistance)
                return true;

            float distance = Vector3.Distance(go.transform.position, __instance.transform.position);
            bool run = distance > 10f;

            Traverse.Create(__instance).Method("MoveTo", dt, go.transform.position, 0f, run).GetValue();

            return false;
        }
    }
}
