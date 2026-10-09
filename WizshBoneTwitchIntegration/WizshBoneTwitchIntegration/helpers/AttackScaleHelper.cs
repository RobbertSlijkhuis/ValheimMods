using System.Collections.Generic;
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
        // - A Twitch-spawned creature: its root localScale (set from CreatureData.size on every
        //   client by TwitchCreaturePersistentData.ApplyVariables). Attacks are only simulated by
        //   the creature's owner, which always has that scale applied. Restricted to Twitch
        //   creatures so vanilla prefabs that happen to have a non-1 root scale stay untouched.
        public static float GetAttackScale(Character attacker)
        {
            if (attacker == null)
                return 1f;

            if (attacker.IsPlayer())
                return ReferenceEquals(attacker, Player.m_localPlayer) ? PlayerScaleHelper.CurrentScale : 1f;

            if (attacker.GetComponent<TwitchCreaturePersistentData>() == null)
                return 1f;

            float scale = attacker.transform.localScale.x;

            return scale > 0f ? scale : 1f;
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
