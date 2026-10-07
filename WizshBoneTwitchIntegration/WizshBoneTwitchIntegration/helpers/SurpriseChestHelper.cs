using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class SurpriseChestHelper
    {
        public static void SpawnSupriseChest(GameObject prefab, SurpriseChestData chestData, CustomRewardEvent customRewardEvent)
        {
            Transform transform = Player.m_localPlayer.transform;

            // "Random" is used here instead of a fixed offset like "InFrontOfPlayer": both now
            // get the same floor+ceiling+obstruction validation, but "Random" also gets several
            // tries at different nearby spots before falling back to on-player, so one fixed
            // point landing past a thin dungeon wall/railing doesn't end the search early.
            Vector3 spawnPosition = TransformHelper.UpdateSpawnLocation(chestData.position, transform, chestData.positionOffset);

            Quaternion spawnRotation = chestData.facePlayer
                ? TransformHelper.FacePlayer(spawnPosition, transform.rotation)
                : TransformHelper.RandomFacing();

            GameObject chest = ZNetViewHelper.Instantiate(prefab, spawnPosition, spawnRotation);
            TwitchSurpriseChest surpriseChest = chest.GetComponent<TwitchSurpriseChest>();
            surpriseChest.Init(chestData, customRewardEvent);
        }

        /// <summary>
        /// Whether the chest would still have something to spawn right now: the dungeon filter
        /// (<see cref="GetEligibleItems"/>) plus the Max spawned limit leave at least one entry. Checked by
        /// the redeem before the chest spawns, so a chest with nothing left can still be refunded. A chest
        /// with no configured entries at all has no loot either.
        /// </summary>
        public static bool HasEligibleLoot(SurpriseChestData chestData)
        {
            if (chestData == null || chestData.items == null || chestData.items.Count == 0)
                return false;

            // With random on the chest makes `amount` picks, so an amount of 0 hands out nothing.
            if (chestData.random && chestData.amount <= 0)
                return false;

            LootDraw draw = new LootDraw();

            foreach (SurpriseChestSpawnData entry in GetEligibleItems(chestData.items))
            {
                if (draw.IsSpawnable(entry))
                    return true;
            }

            return false;
        }

        private static bool SpawnsAnything(SurpriseChestSpawnData entry)
        {
            if (entry.itemData != null && entry.itemData.amount > 0)
                return true;

            return entry.creatureData != null && entry.creatureData.Exists(creature => creature.amount > 0);
        }

        /// <summary>
        /// Returns the chest's configured items, minus any dungeon-forbidden creatures, so a
        /// dungeon-forbidden roll can never happen (rather than silently spawning nothing once
        /// picked). Items are only cloned/modified when something actually needs stripping;
        /// an item is dropped entirely only if that would leave it with nothing left to spawn.
        /// </summary>
        public static List<SurpriseChestSpawnData> GetEligibleItems(List<SurpriseChestSpawnData> items)
        {
            if (!Player.m_localPlayer.InInterior())
                return items;

            List<string> forbidden = CreatureHelper.GetDungeonForbiddenCreatures();
            List<SurpriseChestSpawnData> eligible = new List<SurpriseChestSpawnData>();

            foreach (SurpriseChestSpawnData item in items)
            {
                if (item.creatureData == null)
                {
                    eligible.Add(item);
                    continue;
                }

                List<CreatureData> allowedCreatures = item.creatureData.FindAll(c => !forbidden.Contains(c.prefabName));

                if (allowedCreatures.Count == item.creatureData.Count)
                {
                    eligible.Add(item);
                    continue;
                }

                if (allowedCreatures.Count == 0 && item.itemData == null)
                    continue;

                SurpriseChestSpawnData filtered = item.Clone<SurpriseChestSpawnData>();
                filtered.creatureData = allowedCreatures;
                eligible.Add(filtered);
            }

            return eligible;
        }

        /// <summary>
        /// The state of one loot draw (or of the redeem-time eligibility check): which entries can still
        /// come out, given the Max spawned limits. Counting the redeem-spawned creatures near the player
        /// scans every AI in the world, so the count per prefab is looked up once and cached here, and
        /// creatures earlier picks of the same draw will spawn (they don't exist yet) are tracked in
        /// <see cref="AddPlanned"/>. Create a fresh one per draw.
        /// </summary>
        public class LootDraw
        {
            private readonly Dictionary<string, int> m_existing = new Dictionary<string, int>();
            private readonly Dictionary<string, int> m_planned = new Dictionary<string, int>();

            /// <summary>
            /// Whether <paramref name="entry"/> would actually hand out something: it spawns at least one
            /// creature (amount above 0) or an item stack (size above 0), and isn't capped by Max spawned.
            /// Capped is judged per entry, so a hand-edited entry holding both a creature and an item is
            /// dropped whole when its creature is capped; the form never produces such an entry. An
            /// item-only entry is never capped, so items keep a chest valid even when every creature
            /// entry was removed.
            /// </summary>
            public bool IsSpawnable(SurpriseChestSpawnData entry)
            {
                return SpawnsAnything(entry) && !IsAtMaxSpawned(entry);
            }

            /// <summary>Records the creatures <paramref name="entry"/> will spawn, so later checks count them.</summary>
            public void AddPlanned(SurpriseChestSpawnData entry)
            {
                if (entry.creatureData == null)
                    return;

                foreach (CreatureData creature in entry.creatureData)
                {
                    if (creature.amount <= 0)
                        continue;

                    m_planned.TryGetValue(creature.prefabName, out int alreadyPlanned);
                    m_planned[creature.prefabName] = alreadyPlanned + creature.amount;
                }
            }

            // Whether any creature of the entry has reached its Max spawned limit, counting the creatures
            // alive near the player (cached) plus the planned ones.
            private bool IsAtMaxSpawned(SurpriseChestSpawnData entry)
            {
                if (entry.creatureData == null)
                    return false;

                foreach (CreatureData creature in entry.creatureData)
                {
                    m_planned.TryGetValue(creature.prefabName, out int alsoPlanned);

                    if (CreatureHelper.IsAtMaxSpawned(creature, alsoPlanned, m_existing))
                        return true;
                }

                return false;
            }
        }
    }
}
