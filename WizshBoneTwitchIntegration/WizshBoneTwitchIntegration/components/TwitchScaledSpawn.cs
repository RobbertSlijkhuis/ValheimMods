using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    // Carries a resized creature's scale over to a networked object it spawns: the ragdoll/statue
    // its death effects create (see DeathEffectHelper) and the Aoe prefabs its attacks spawn (see
    // AttackScaleHelper.ScaleSpawnedAoe). Vanilla only copies the creature's scale onto a spawned
    // object in the death-effect case when the effect has m_inheritParentScale set, and never for
    // Aoes, so e.g. a shrunk Frost troll's petrified statue would otherwise pop to full size.
    //
    // Same replicate-through-the-ZDO pattern as the other persistent data (no RPC): the owner
    // writes the scale once in Apply() and flags the object; every other client's
    // TwitchBasePersistentData.Awake() then attaches this component, which reads it back below.
    internal class TwitchScaledSpawn : MonoBehaviour
    {
        public static readonly int ScaleHash = "WBTI_SpawnScale".GetStableHashCode();

        // Called once, on the client that owns the freshly spawned object. alreadyScaled is true
        // for objects BornScaledSpawnHelper spawned at the final scale (jointed ragdolls): they only
        // need the scale stored in the ZDO for the other clients.
        public static void Apply(GameObject target, float scale, bool alreadyScaled = false)
        {
            if (!alreadyScaled)
                target.transform.localScale *= scale;

            ZNetView netView = target.GetComponent<ZNetView>();

            if (netView == null || !netView.IsValid() || !netView.IsOwner())
                return;

            netView.GetZDO().Set(ScaleHash, scale);
            target.GetComponent<TwitchBasePersistentData>()?.SetFlag(PersistentComponentFlags.SpawnScale, true);
        }

        public void Awake()
        {
            try
            {
                // Born-scaled objects (see BornScaledSpawnHelper) already have the scale applied.
                if (BornScaledSpawnHelper.IsActivating)
                    return;

                ZNetView netView = gameObject.GetComponent<ZNetView>();

                if (netView == null || !netView.IsValid())
                    return;

                float scale = netView.GetZDO().GetFloat(ScaleHash, 1f);

                if (scale > 0f && scale != 1f)
                {
                    transform.localScale *= scale;

                    // The owner scales an Aoe's radius itself (AttackScaleHelper.ScaleSpawnedAoe);
                    // only the transform scale is replicated, so do the same here or the damage
                    // falloff (distance / m_radius) differs between clients.
                    foreach (Aoe aoe in GetComponentsInChildren<Aoe>(true))
                        aoe.m_radius *= scale;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchScaledSpawn.Awake failed: " + e);
            }
        }
    }
}
