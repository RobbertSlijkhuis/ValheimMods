using ModularMagic_BloodMagic.Components;
using System.Collections.Generic;

namespace ModularMagic_BloodMagic.Helpers
{
    /// <summary>
    /// The smaller apparitions attack from their own size. The game adds the height, reach and width of an attack to the joint the attack
    /// starts from in world units, so the joint shrinks with the creature but these numbers do not. They are scaled with the size of the
    /// apparition only while the game uses them, see ApparitionPatches. Ported from AttackScaleHelper of WizshBoneTwitchIntegration.
    /// Attacks and the AI only run on the client that owns the apparition, which has its size applied, so nothing is synced.
    /// </summary>
    public static class AttackScaleHelper
    {
        /// <summary>The attack numbers of an Attack that are in world units, as they were before they were scaled.</summary>
        public struct AttackGeometrySnapshot
        {
            public float attackRange;
            public float attackHeight;
            public float attackOffset;
            public float attackRayWidth;
            public float attackHeightChar1;
            public float attackHeightChar2;
            public float attackRayWidthCharExtra;

            public static AttackGeometrySnapshot Capture(Attack attack)
            {
                return new AttackGeometrySnapshot
                {
                    attackRange             = attack.m_attackRange,
                    attackHeight            = attack.m_attackHeight,
                    attackOffset            = attack.m_attackOffset,
                    attackRayWidth          = attack.m_attackRayWidth,
                    attackHeightChar1       = attack.m_attackHeightChar1,
                    attackHeightChar2       = attack.m_attackHeightChar2,
                    attackRayWidthCharExtra = attack.m_attackRayWidthCharExtra,
                };
            }

            public void Restore(Attack attack)
            {
                attack.m_attackRange             = attackRange;
                attack.m_attackHeight            = attackHeight;
                attack.m_attackOffset            = attackOffset;
                attack.m_attackRayWidth          = attackRayWidth;
                attack.m_attackHeightChar1       = attackHeightChar1;
                attack.m_attackHeightChar2       = attackHeightChar2;
                attack.m_attackRayWidthCharExtra = attackRayWidthCharExtra;
            }
        }

        /// <summary>The AI attack range of a weapon, as it was before it was scaled.</summary>
        public struct AiRangeSnapshot
        {
            public ItemDrop.ItemData.SharedData shared;
            public float aiAttackRange;
            public float aiAttackRangeMin;
        }

        /// <summary>
        /// The factor the attack numbers of the character are multiplied with: the size of an apparition compared to its prefab. 1 for
        /// every other character, so a wild Charred, a player and a creature of another mod are never changed here.
        /// </summary>
        public static float GetAttackScale(Character character)
        {
            return ApparitionHelper.TryGet(character, out ApparitionController apparition) ? apparition.AttackScale : 1f;
        }

        /// <summary>
        /// Scales the numbers a melee or area attack uses to find what it hits (reach, height, offset and the widths of its rays). Returns
        /// the numbers to restore afterwards, or null when nothing was changed.
        /// </summary>
        public static AttackGeometrySnapshot? ScaleHitArea(Attack attack)
        {
            float scale = GetAttackScale(attack.m_character);

            if (scale == 1f)
                return null;

            AttackGeometrySnapshot snapshot = AttackGeometrySnapshot.Capture(attack);

            attack.m_attackRange             *= scale;
            attack.m_attackHeight            *= scale;
            attack.m_attackOffset            *= scale;
            attack.m_attackRayWidth          *= scale;
            attack.m_attackHeightChar1       *= scale;
            attack.m_attackHeightChar2       *= scale;
            attack.m_attackRayWidthCharExtra *= scale;

            return snapshot;
        }

        /// <summary>
        /// Scales the numbers the spawn point of a projectile is made of (height, reach and offset from the joint it starts from). Returns
        /// the numbers to restore afterwards, or null when nothing was changed.
        /// </summary>
        public static AttackGeometrySnapshot? ScaleProjectileSpawn(Attack attack)
        {
            float scale = GetAttackScale(attack.m_character);

            if (scale == 1f)
                return null;

            AttackGeometrySnapshot snapshot = AttackGeometrySnapshot.Capture(attack);

            attack.m_attackHeight *= scale;
            attack.m_attackRange  *= scale;
            attack.m_attackOffset *= scale;

            return snapshot;
        }

        /// <summary>
        /// The AI decides when to start an attack (and which weapon to use) from the distance to its target and the AI range of each
        /// weapon. Without scaling those too, a small apparition starts its swing from outside its shorter reach and misses.
        /// Scales every weapon in the inventory of the apparition and returns what to restore afterwards, or null when nothing was
        /// changed. A SharedData that two items share is only scaled once.
        /// </summary>
        public static List<AiRangeSnapshot>? ScaleAiAttackRanges(Humanoid humanoid)
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
                    shared           = shared,
                    aiAttackRange    = shared.m_aiAttackRange,
                    aiAttackRangeMin = shared.m_aiAttackRangeMin,
                });

                shared.m_aiAttackRange    *= scale;
                shared.m_aiAttackRangeMin *= scale;
            }

            return snapshots;
        }

        public static void RestoreAiAttackRanges(List<AiRangeSnapshot>? snapshots)
        {
            if (snapshots == null)
                return;

            foreach (AiRangeSnapshot snapshot in snapshots)
            {
                snapshot.shared.m_aiAttackRange    = snapshot.aiAttackRange;
                snapshot.shared.m_aiAttackRangeMin = snapshot.aiAttackRangeMin;
            }
        }
    }
}
