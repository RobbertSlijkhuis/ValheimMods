using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    // What a Twitch creature's death effects (ragdoll, statue, ...) need that vanilla doesn't do for
    // them: they spawn at prefab scale and in the prefab's colors, so a resized or recolored
    // creature would leave a full-size, uncolored corpse. Character.OnDeath is bracketed with
    // BeginDeath/EndDeath so the EffectList.Create call it makes can be recognised, and
    // HandleDeathEffects then scales and recolors what that call created.
    public class DeathEffectHelper
    {
        private struct DyingEntry
        {
            public Character character;
            public float scale;
        }

        // One entry per Character.OnDeath currently running (a stack, because one death can trigger
        // another character's OnDeath before it finishes). The entry is empty for anything that
        // isn't a Twitch creature, so every BeginDeath is matched by an EndDeath. A Twitch
        // creature is tracked at any size - one at its native scale still has its recolor handed
        // to the ragdoll.
        private static readonly Stack<DyingEntry> s_dying = new Stack<DyingEntry>();

        private static Character DyingCharacter => s_dying.Count > 0 ? s_dying.Peek().character : null;
        private static float DyingScale => s_dying.Count > 0 ? s_dying.Peek().scale : 1f;

        public static void BeginDeath(Character character)
        {
            DyingEntry entry = default;

            try
            {
                if (character != null && !character.IsPlayer() && character.GetComponent<TwitchCreaturePersistentData>() != null)
                {
                    entry.character = character;
                    entry.scale = AttackScaleHelper.GetAttackScale(character);
                }
            }
            finally
            {
                s_dying.Push(entry);
            }
        }

        public static void EndDeath()
        {
            if (s_dying.Count > 0)
                s_dying.Pop();

            if (s_dying.Count == 0)
                BornScaledSpawnHelper.ClearOwnerSpawns();
        }

        // The scale factor of the creature whose OnDeath is running, for BornScaledSpawnHelper
        // (jointed ragdolls are spawned at that scale up front) - only for the EffectList.Create
        // call made with that creature as parent, i.e. its own death effects.
        public static bool TryGetDyingScale(Transform baseParent, out float scale)
        {
            scale = DyingScale;
            return DyingCharacter != null && baseParent == DyingCharacter.transform;
        }

        // Entry point from the EffectList.Create postfix: only acts on the call made with the dying
        // creature as parent, i.e. its m_deathEffects.
        public static void HandleDeathEffects(EffectList list, Transform baseParent, int variant, GameObject[] created)
        {
            if (DyingCharacter == null || list == null || created == null || baseParent != DyingCharacter.transform)
                return;

            ScaleDeathEffects(list, baseParent, variant, created);
            RecolorDeathEffects(created);
        }

        // Vanilla only scales a death effect to its parent when the effect opts in via
        // m_inheritParentScale (otherwise it spawns at prefab scale, parentless). Scale the ones
        // that didn't. Which effects inherited is worked out from their EffectData (mirroring
        // EffectList.Create) rather than by comparing scales - a prefab whose own scale happens to
        // equal the creature's (e.g. 1 at size 1) would otherwise look like it had inherited.
        private static void ScaleDeathEffects(EffectList list, Transform baseParent, int variant, GameObject[] created)
        {
            if (DyingScale == 1f)
                return;

            int createdIndex = 0;

            foreach (EffectList.EffectData data in list.m_effectPrefabs)
            {
                // Same filter EffectList.Create uses, so created[] lines up with the kept entries.
                if (!data.m_enabled || (variant >= 0 && data.m_variant >= 0 && variant != data.m_variant))
                    continue;

                if (createdIndex >= created.Length)
                    break;

                GameObject effect = created[createdIndex++];

                // Attached effects (m_attach) are children of the creature and already follow it.
                if (effect == null || effect.transform.parent != null)
                    continue;

                if (InheritsParentRootScale(data, baseParent))
                    continue;

                TwitchScaledSpawn.Apply(effect, DyingScale, alreadyScaled: BornScaledSpawnHelper.WasSpawnedScaled(effect));
            }
        }

        // Mirrors EffectList.Create's scale handling: with m_scale set the parent's scale is copied
        // when m_inheritParentScale is on; without it, m_multiplyParentVisualScale (when the
        // parent has a "Visual" child) uses the Visual's scale instead of the root's, otherwise
        // m_inheritParentScale copies the parent's root scale.
        private static bool InheritsParentRootScale(EffectList.EffectData data, Transform baseParent)
        {
            if (!data.m_inheritParentScale)
                return false;

            if (data.m_scale)
                return true;

            return !(data.m_multiplyParentVisualScale && baseParent.Find("Visual") != null);
        }

        // Hands the dying creature's recolor to the ragdoll(s) among its death effects (see
        // TwitchRagdollRecolor, which also replicates it to the other clients).
        private static void RecolorDeathEffects(GameObject[] created)
        {
            TwitchCreaturePersistentData data = DyingCharacter.GetComponent<TwitchCreaturePersistentData>();

            // Same condition the creature itself was recolored under (TwitchCreaturePersistentData.Start).
            if (data.RedeemerName == null
                || !RecolorHelper.CanRecolorCreature(data.RedeemerName, DyingCharacter.name, data.ColorOverride, data.EmissionColorOverride))
                return;

            string creatureKey = DyingCharacter.name.Replace("(Clone)", "");

            // Most creatures have no ragdoll recolor entries yet; skip the ZDO writes for those.
            if (!RecolorHelper.HasRagdollRecolor(creatureKey))
                return;

            foreach (GameObject effect in created)
            {
                if (effect == null || effect.GetComponent<Ragdoll>() == null)
                    continue;

                TwitchRagdollRecolor.Apply(effect, creatureKey, data.RedeemerName, data.ColorOverride, data.EmissionColorOverride, data.ForceColorOverride);
            }
        }
    }
}
