using HarmonyLib;
using System.Collections.Generic;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class BornScaledSpawnPatchesWBTI
    {
        // Both methods instantiate the object with a single Object.Instantiate(prefab, pos, rot);
        // the helper swaps that call so a resized creature's jointed ragdoll is born at its final
        // scale (see BornScaledSpawnHelper). Owner: EffectList.Create (death effects).
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EffectList), nameof(EffectList.Create))]
        public static IEnumerable<CodeInstruction> EffectListCreate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return BornScaledSpawnHelper.ReplaceInstantiate(instructions, "EffectList.Create", passBaseParentArg: true);
        }

        // Every other client (and world reloads): ZNetScene builds the object from its ZDO.
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ZNetScene), "CreateObject")]
        public static IEnumerable<CodeInstruction> ZNetSceneCreateObject_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return BornScaledSpawnHelper.ReplaceInstantiate(instructions, "ZNetScene.CreateObject", passBaseParentArg: false);
        }
    }
}
