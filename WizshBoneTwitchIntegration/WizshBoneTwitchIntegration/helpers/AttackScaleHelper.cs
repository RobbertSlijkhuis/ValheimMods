using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    public class AttackScaleHelper
    {
        public struct AiRangeSnapshot
        {
            public ItemDrop.ItemData.SharedData shared;
            public float aiAttackRange;
            public float aiAttackRangeMin;
        }

        // The factor an attacker's flat world-unit attack geometry (range, height, offset, ray
        // widths - see EntityScalePatchesWBTI) must be multiplied by to match its visual size.
        // Returns 1 when nothing should be adjusted.
        //
        // - The local player: PlayerScaleHelper.CurrentScale. Other players' attacks are never
        //   simulated on this client, so they're ignored.
        // - A Twitch-spawned creature: its current root localScale (set from CreatureData.size on
        //   every client by TwitchCreaturePersistentData.ApplyVariables) relative to the prefab's
        //   native root scale. The attack numbers are authored for the native scale (e.g. TrollFrost's
        //   prefab is 4x), so a creature resized to 0.3 needs 0.3 / 4, not 0.3. Attacks are only
        //   simulated by the creature's owner, which always has that scale applied. Restricted to
        //   Twitch creatures so vanilla prefabs that happen to have a non-1 root scale stay untouched.
        public static float GetAttackScale(Character attacker)
        {
            if (attacker == null)
                return 1f;

            if (attacker.IsPlayer())
                return ReferenceEquals(attacker, Player.m_localPlayer) ? PlayerScaleHelper.CurrentScale : 1f;

            TwitchCreaturePersistentData creatureData = attacker.GetComponent<TwitchCreaturePersistentData>();

            if (creatureData == null)
                return 1f;

            float scale = attacker.transform.localScale.x;
            float native = creatureData.NativeScale;

            if (scale <= 0f || native <= 0f)
                return 1f;

            return scale / native;
        }

        // Aoes (radius/box) and spawned root objects (transform) ScaleSpawnedAoe has already scaled.
        // Keyed by object identity and weak, so an entry disappears with its object and there is
        // nothing to clean up. Separate because one spawned root can hold several Aoes.
        private static readonly ConditionalWeakTable<Aoe, object> s_scaledAoes = new ConditionalWeakTable<Aoe, object>();
        private static readonly ConditionalWeakTable<GameObject, object> s_scaledRoots = new ConditionalWeakTable<GameObject, object>();
        private static readonly object s_scaledMarker = new object();

        // An Aoe's damage area is a flat world-unit m_radius (or box size) that its transform scale
        // doesn't affect, so an Aoe a resized creature's attack spawns (a stomp shockwave from the
        // weapon's m_spawnOnHit, a SpawnAbility/projectile Aoe, ...) would keep its full vanilla
        // reach. Scales the radius, the box and the object itself (visuals + trigger
        // colliders) by the owner's attack scale. Called from Aoe.Setup, i.e. once per spawn, on the
        // client that owns the creature. Players are skipped - this is for resized creatures only.
        public static void ScaleSpawnedAoe(Aoe aoe, Character owner)
        {
            if (owner == null || owner.IsPlayer())
                return;

            float scale = GetAttackScale(owner);

            if (scale == 1f)
                return;

            // Setup is a public entry point; scaling is relative, so a repeat call on the same Aoe
            // would compound it.
            if (s_scaledAoes.TryGetValue(aoe, out _))
                return;

            s_scaledAoes.Add(aoe, s_scaledMarker);

            aoe.m_radius *= scale;

            // Without triggers (FindHits) the box size/center are flat world units the transform scale
            // doesn't touch; with triggers the box is a real collider that already follows the
            // transform scale applied below, so scaling it here would count the scale twice.
            if (!aoe.m_useTriggers && aoe.m_useCollider != null)
            {
                aoe.m_useCollider.size *= scale;
                aoe.m_useCollider.center *= scale;
            }

            // The transform scale (and its replication to the other clients) belongs to the spawn's
            // root object, whichever of its Aoes gets here first - and only once, however many Aoes
            // it holds.
            ZNetView netView = aoe.GetComponentInParent<ZNetView>();
            GameObject root = netView != null ? netView.gameObject : aoe.gameObject;

            if (s_scaledRoots.TryGetValue(root, out _))
                return;

            s_scaledRoots.Add(root, s_scaledMarker);
            TwitchScaledSpawn.Apply(root, scale);
        }

        // MonsterAI decides when to start a swing (and which weapon to pick) by comparing the
        // distance to its target against each weapon's flat m_aiAttackRange / m_aiAttackRangeMin.
        // Without scaling those too, a shrunk monster starts swinging from far outside its (scaled)
        // reach and misses, and a grown one waits until it's closer than it needs to be.
        //
        // Scales every weapon in the humanoid's inventory and returns what to restore afterwards, or
        // null when nothing was changed. SharedData is de-duplicated by reference so an instance
        // shared by two items can never be scaled twice.
        public static List<AiRangeSnapshot> ScaleAiAttackRanges(Humanoid humanoid)
        {
            float scale = GetAttackScale(humanoid);

            if (scale == 1f)
                return null;

            Inventory inventory = humanoid.GetInventory();

            if (inventory == null)
                return null;

            List<AiRangeSnapshot> snapshots = new List<AiRangeSnapshot>();
            HashSet<ItemDrop.ItemData.SharedData> seen = new HashSet<ItemDrop.ItemData.SharedData>();

            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                ItemDrop.ItemData.SharedData shared = item.m_shared;

                if (shared == null || !seen.Add(shared))
                    continue;

                snapshots.Add(new AiRangeSnapshot
                {
                    shared = shared,
                    aiAttackRange = shared.m_aiAttackRange,
                    aiAttackRangeMin = shared.m_aiAttackRangeMin,
                });

                shared.m_aiAttackRange *= scale;
                shared.m_aiAttackRangeMin *= scale;
            }

            return snapshots;
        }

        public static void RestoreAiAttackRanges(List<AiRangeSnapshot> snapshots)
        {
            if (snapshots == null)
                return;

            foreach (AiRangeSnapshot snapshot in snapshots)
            {
                snapshot.shared.m_aiAttackRange = snapshot.aiAttackRange;
                snapshot.shared.m_aiAttackRangeMin = snapshot.aiAttackRangeMin;
            }
        }
    }
}
