using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Catches the attacks SafeZoneProjectileHelper can't: ones that never fly as a Projectile, e.g.
    // a long-range hitscan swing (the JotunWitch's attack_magicblast is a Horizontal attack with a
    // 50 m reach that calls Damage() directly on whatever its rays hit) or an Aoe that goes off just
    // outside the zone. They all end in Character.Damage, so a hit by a Twitch-spawned monster on a
    // victim standing inside a zone is dropped there.
    //
    // Multiplayer: Character.Damage runs on the client that makes the hit (the attacker's owner)
    // before it is forwarded to the victim's owner, so dropping it here means it never gets sent.
    internal static class SafeZoneDamageHelper
    {
        public static bool ShouldBlockHit(Character victim, HitData hit)
        {
            if (!TwitchSafeZone.HasActiveSafeZones || !TwitchSafeZone.KeepsMonstersOut)
                return false;

            Character attacker = hit.GetAttacker();

            if (attacker == null || attacker == victim)
                return false;

            if (!TwitchSafeZone.IsWardTargetCreature(attacker.gameObject))
                return false;

            // Where the attacker stands doesn't matter: a monster that clipped into the zone must
            // not get to hit someone next to it just for already being "inside".
            return TwitchSafeZone.IsInsideEnabledSafeZone(victim.transform.position);
        }
    }
}
