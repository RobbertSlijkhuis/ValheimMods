using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Spawns a jointed ragdoll already at its final scale. A ragdoll's CharacterJoints are built the
    // moment it is instantiated, at prefab scale, so scaling the root afterwards (what vanilla's
    // m_inheritParentScale does too) pulls the limbs apart. Here the object is instantiated under an
    // inactive holder - which defers Awake/OnEnable - scaled, and only then released, so every
    // component (joints included) initialises at the final scale. The prefab itself is never touched.
    //
    // Used from both places that instantiate it: EffectList.Create (the owner's death effects) and
    // ZNetScene.CreateObject (every other client, and reloads), via the transpilers in
    // BornScaledSpawnPatchesWBTI which swap their Object.Instantiate call for InstantiateDeathEffect()
    // and InstantiateFromZdo() below.
    public class BornScaledSpawnHelper
    {
        // True while a born-scaled object is being released, so TwitchScaledSpawn.Awake (attached
        // during that very activation on remote clients) knows not to scale it a second time.
        public static bool IsActivating { get; private set; }

        // Objects the owner just spawned scaled, so DeathEffectHelper doesn't scale them again.
        private static readonly HashSet<int> s_ownerSpawned = new HashSet<int>();

        private static readonly Dictionary<int, bool> s_hasJoints = new Dictionary<int, bool>();

        public static bool WasSpawnedScaled(GameObject spawned)
        {
            return spawned != null && s_ownerSpawned.Contains(spawned.GetInstanceID());
        }

        public static void ClearOwnerSpawns()
        {
            s_ownerSpawned.Clear();
        }

        // Replacement for Object.Instantiate(prefab, position, rotation) in EffectList.Create (the
        // owner). baseParent is that Create call's own parent argument: only the call made with the
        // dying creature as parent is its death effects - any other effect created meanwhile is
        // left alone.
        public static GameObject InstantiateDeathEffect(GameObject prefab, Vector3 position, Quaternion rotation, Transform baseParent)
        {
            if (!DeathEffectHelper.TryGetDyingScale(baseParent, out float scale))
                return Object.Instantiate(prefab, position, rotation);

            return InstantiateScaled(prefab, position, rotation, scale, ownerSpawn: true);
        }

        // Replacement for Object.Instantiate(prefab, position, rotation) in ZNetScene.CreateObject
        // (every other client, and reloads): the owner stored the scale in the ZDO being instantiated.
        public static GameObject InstantiateFromZdo(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            ZDO zdo = ZNetView.m_initZDO;
            float scale = zdo != null ? zdo.GetFloat(TwitchScaledSpawn.ScaleHash, 1f) : 1f;

            return InstantiateScaled(prefab, position, rotation, scale, ownerSpawn: false);
        }

        private static GameObject InstantiateScaled(GameObject prefab, Vector3 position, Quaternion rotation, float scale, bool ownerSpawn)
        {
            if (scale <= 0f || scale == 1f || !HasJoints(prefab))
                return Object.Instantiate(prefab, position, rotation);

            GameObject holder = new GameObject("WBTI_SpawnHolder");
            holder.SetActive(false);

            try
            {
                GameObject spawned = Object.Instantiate(prefab, position, rotation, holder.transform);
                spawned.transform.localScale = prefab.transform.localScale * scale;

                IsActivating = true;
                spawned.transform.SetParent(null, true);

                if (ownerSpawn)
                    s_ownerSpawned.Add(spawned.GetInstanceID());

                return spawned;
            }
            finally
            {
                IsActivating = false;
                Object.Destroy(holder);
            }
        }

        private static bool HasJoints(GameObject prefab)
        {
            int id = prefab.GetInstanceID();

            if (!s_hasJoints.TryGetValue(id, out bool hasJoints))
            {
                hasJoints = prefab.GetComponentInChildren<Joint>(true) != null;
                s_hasJoints[id] = hasJoints;
            }

            return hasJoints;
        }

        // Transpiler body shared by the two patches: swaps the method's Object.Instantiate<GameObject>
        // (prefab, position, rotation) call for one of the replacements above. Both vanilla methods
        // contain exactly one such call; anything else is logged so a game update that changes them
        // is noticed. With passBaseParentArg the replacement is InstantiateDeathEffect and the
        // method's own baseParent argument (arg 3 of the instance method EffectList.Create) is pushed
        // as its extra parameter.
        public static IEnumerable<CodeInstruction> ReplaceInstantiate(IEnumerable<CodeInstruction> instructions, string methodName, bool passBaseParentArg)
        {
            MethodInfo replacement = AccessTools.Method(typeof(BornScaledSpawnHelper), passBaseParentArg ? nameof(InstantiateDeathEffect) : nameof(InstantiateFromZdo));
            int replaced = 0;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo method && IsGameObjectInstantiate(method))
                {
                    if (passBaseParentArg)
                    {
                        // Keep any branch label on the first instruction we emit for this call.
                        CodeInstruction loadBaseParent = new CodeInstruction(OpCodes.Ldarg_3);
                        loadBaseParent.labels.AddRange(instruction.labels);
                        instruction.labels.Clear();

                        yield return loadBaseParent;
                    }

                    instruction.operand = replacement;
                    replaced++;
                }

                yield return instruction;
            }

            if (replaced != 1)
                Jotunn.Logger.LogWarning($"[WBTI] BornScaledSpawn: expected 1 Instantiate call in {methodName}, replaced {replaced} - jointed ragdolls may not scale correctly.");
        }

        private static bool IsGameObjectInstantiate(MethodInfo method)
        {
            if (method.Name != "Instantiate" || method.DeclaringType != typeof(Object) || !method.IsGenericMethod)
                return false;

            ParameterInfo[] parameters = method.GetParameters();

            return parameters.Length == 3
                && method.GetGenericArguments()[0] == typeof(GameObject)
                && parameters[1].ParameterType == typeof(Vector3)
                && parameters[2].ParameterType == typeof(Quaternion);
        }
    }
}
