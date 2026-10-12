using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Keeps ward-target creatures from choosing a destination in a safe zone. Two hooks:
    //  - BaseAI.IsValidRandomMovePoint (idle wandering; vanilla uses it to keep monsters out of fire and
    //    lava): an invalid point just makes RandomMovement pick another one next tick. Also keeps the
    //    point WanderMargin clear of the edge.
    //  - BaseAI.HavePath(Vector3): a point inside a zone counts as unreachable. Callers: Flee (tries
    //    other directions), MonsterAI's "unable to attack for 15 s, go for a building instead" fallback
    //    and its static (building) target reachability check (a building inside a zone stays targeted,
    //    the monster may just prefer a random nearby static target), and item pickup (items inside a
    //    zone are ignored). Ranged attacks don't depend on it (range, line of sight and facing only).
    //
    // Deliberately nothing more than that. Creatures still keep their target and still try to walk
    // into a zone after someone inside it (the push in SafeZonePushHelper keeps them out), and
    // BaseAI.MoveTo is left alone: dropping a MoveTo made circling monsters flip between "arrived"
    // and a new point every tick (shaking).
    internal static class SafeZoneAIHelper
    {
        // Roughly a body radius plus slack, so a wander point at the edge doesn't put the creature's
        // collider into the trigger.
        public const float WanderMargin = 2f;

        public static bool ShouldAvoidPoint(BaseAI monster, Vector3 point, float margin)
        {
            if (monster == null)
                return false;

            if (!TwitchSafeZone.HasActiveSafeZones || !ProfileSettingsHelper.Current.wardPushCreatures)
                return false;

            // The creature test goes first: it costs at most three GetComponent calls (one for the vast
            // majority of monsters, which aren't Twitch spawns), while the zone test scales with the
            // number of zones.
            if (!TwitchSafeZone.IsWardTargetCreature(monster.gameObject))
                return false;

            // Random destinations often carry a stale y, and a flyer's is well off the ground, so the
            // zone is tested at the monster's own height.
            point.y = monster.transform.position.y;

            return TwitchSafeZone.IsNearEnabledSafeZone(point, margin);
        }
    }
}
