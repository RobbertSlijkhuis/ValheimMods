using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    // The ward's burn: kills a creature in about `BurnDuration` seconds with a Burning_WBTI status effect.
    //
    // SE_Stats adds the fixed step (0.02 as a float) to its tick timer and ticks once it reaches the
    // interval. 50 float steps of 0.02 sum to just under 1.0, so a 1 s interval really ticks every
    // 1.02 s: only 9 ticks fit in 10 s, and the effect (done once m_time > m_ttl) ends before a 10th.
    // Two things keep the damage exact anyway: the interval sits a hair under the even split so the
    // float error can never push a tick late, and the per-tick damage is rounded up so the total
    // always reaches max health (rounding to nearest left e.g. 1 HP on a 100 HP creature).
    //
    // Multiplayer: status effects only tick on the creature's owner (Character.CustomFixedUpdate), so
    // the burn is only applied there; a copy added on any other client would just sit idle and
    // restart as a fresh burn if ownership ever moved to it.
    internal static class SafeZoneBurnHelper
    {
        private const float BurnDuration = 10f;
        private const float TickIntervalMargin = 0.01f;

        public static void Apply(Humanoid humanoid)
        {
            if (!humanoid.IsOwner())
                return;

            SE_Stats burningPrefab = WizshBoneTwitchIntegration.Instance.effects.Burning;
            SEMan seman = humanoid.GetSEMan();

            // Checked before instantiating (this runs every physics step per creature in a zone), by
            // the prefab's own name hash: the stored effect is registered under that same hash because
            // the instance below is renamed back to the prefab's name.
            if (seman.HaveStatusEffect(burningPrefab.NameHash()))
                return;

            int ticks = Mathf.Max(1, Mathf.RoundToInt(BurnDuration));

            SE_Stats burning = Object.Instantiate(burningPrefab);
            // Instantiate names it "<name>(Clone)", which would hash differently from the prefab.
            burning.name = burningPrefab.name;
            burning.m_tickInterval = BurnDuration / ticks - TickIntervalMargin;
            burning.m_healthPerTick = -Mathf.Ceil(humanoid.GetMaxHealth() / ticks);
            // Outlives the last tick (at ~BurnDuration) so it can't be cut off by the expiry check.
            burning.m_ttl = BurnDuration + 0.5f;

            // Not destroyed afterwards: SEMan.AddStatusEffect stores a MemberwiseClone of this, which
            // shares its native Unity object, so destroying the original would break the live effect.
            seman.AddStatusEffect(burning);
        }
    }
}
