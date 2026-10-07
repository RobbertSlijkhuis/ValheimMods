using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Changes what a spawned creature wears/holds: removes parts of its default loadout
    // (CreatureData.removeEquipment) and equips vanilla items on top (CreatureData.equipItems).
    // Works for any creature with a Humanoid, whether or not it is passive.
    //
    // The creature's inventory is the source of truth: MonsterAI re-picks the creature's weapon from
    // its inventory (Humanoid.EquipBestWeapon) whenever it has a target. So equipped items are added
    // to the inventory, and removed items are removed from it too - merely unequipping a default
    // weapon would let the AI equip it again. Consequences worth knowing:
    // - A weapon added via equipItems sits alongside the creature's default weapons unless those are
    //   also removed, and the AI chooses between them.
    // - Removing "Weapon"/"All" also removes natural-attack items, which can leave a creature unable
    //   to attack.
    // - A held tool-type item (e.g. a tankard) can be replaced by a weapon once the creature has a
    //   target. Armor and helmets are never touched by that logic.
    //
    // Uses the real equip route (Humanoid.EquipItem) rather than parenting a bare mesh to a bone, so
    // vanilla VisEquipment/ZDO sync replicates it to other clients. Humanoid.SetupEquipment only
    // pushes visuals into the ZDO on the owning client, so what other clients see comes from the
    // owner's copy. Every client still applies this to its own copy of the creature (spawn and
    // rehydration alike), so whichever client becomes the owner later already has the gear in its
    // local equipment state instead of its default loadout.
    internal static class CreatureGearHelper
    {
        // How many frames EquipItem is retried for while the creature is mid-attack/mid-dodge, which
        // Humanoid.EquipItem refuses.
        private const int EquipRetryFrames = 60;

        // Special removeEquipment values on top of the plain ItemDrop.ItemData.ItemType names.
        private const string RemoveAll = "All";
        private const string RemoveWeapon = "Weapon";

        // Deferred a frame rather than applied immediately - MonsterAI.Start() (unconditionally
        // calls Humanoid.EquipBestWeapon(null,...)) and Humanoid.Start() (GiveDefaultItems(),
        // force-equips the prefab's default shield/armor) both run after the creature is created,
        // since Unity defers Start() on a freshly Instantiate()'d object until after the current
        // Update pass. Applying synchronously would let that vanilla arm-up logic silently overwrite
        // our gear with the creature's own default loadout. Waiting one frame guarantees both
        // Start() calls have already run, so our gear is applied last and wins the slot.
        public static void Apply(Humanoid humanoid, CreatureData creatureData)
        {
            if (humanoid == null || creatureData == null || !HasGear(creatureData))
                return;

            if (humanoid.GetComponent<VisEquipment>() == null)
                Jotunn.Logger.LogWarning($"[WBTI] CreatureGearHelper: {humanoid.name} has no VisEquipment, equipped gear will not be visible.");

            humanoid.StartCoroutine(ApplyNextFrame(humanoid, creatureData));
        }

        private static bool HasGear(CreatureData creatureData)
        {
            return (creatureData.removeEquipment != null && creatureData.removeEquipment.Count > 0)
                || (creatureData.equipItems != null && creatureData.equipItems.Count > 0);
        }

        private static IEnumerator ApplyNextFrame(Humanoid humanoid, CreatureData creatureData)
        {
            yield return null;

            // The creature may have been destroyed/despawned during that one frame of delay - bail
            // out instead of touching a dead object.
            if (humanoid == null)
                yield break;

            // Remove first so e.g. removing "Shield" can never strip a shield we are about to equip.
            RemoveEquipment(humanoid, creatureData.removeEquipment);

            yield return EquipItems(humanoid, creatureData.equipItems);
        }

        // Unequips and drops from the creature's inventory every item matching the given types.
        // Vanilla GiveDefaultItem adds weapons to the inventory without equipping them, so "Weapon"
        // and "All" sweep the whole inventory; plain item-type entries (Shield, Helmet, ...) only
        // touch what is currently equipped. Works off the inventory rather than Humanoid's protected
        // slot fields, so every slot is covered without reflection.
        private static void RemoveEquipment(Humanoid humanoid, List<string> types)
        {
            if (types == null || types.Count == 0)
                return;

            RemovalFilter filter = RemovalFilter.Parse(types);
            Inventory inventory = humanoid.GetInventory();

            // Copy: GetAllItems returns the live list and we remove from it while iterating.
            foreach (ItemDrop.ItemData item in new List<ItemDrop.ItemData>(inventory.GetAllItems()))
            {
                if (!filter.Matches(item))
                    continue;

                humanoid.UnequipItem(item, triggerEquipEffects: false);
                inventory.RemoveItem(item);
            }
        }

        // The removeEquipment list parsed once up front, so an unknown entry is warned about a single
        // time per creature instead of once per inventory item.
        private class RemovalFilter
        {
            private bool m_all;
            private bool m_weapon;
            private readonly HashSet<ItemDrop.ItemData.ItemType> m_equippedTypes = new HashSet<ItemDrop.ItemData.ItemType>();

            public static RemovalFilter Parse(List<string> types)
            {
                RemovalFilter filter = new RemovalFilter();

                foreach (string type in types)
                {
                    string trimmed = type?.Trim();

                    if (string.Equals(trimmed, RemoveAll, StringComparison.OrdinalIgnoreCase))
                        filter.m_all = true;
                    else if (string.Equals(trimmed, RemoveWeapon, StringComparison.OrdinalIgnoreCase))
                        filter.m_weapon = true;
                    else if (TryParseItemType(trimmed, out ItemDrop.ItemData.ItemType itemType))
                        filter.m_equippedTypes.Add(itemType);
                    else
                        Jotunn.Logger.LogWarning($"[WBTI] CreatureGearHelper: unknown removeEquipment type '{type}', skipping.");
                }

                return filter;
            }

            public bool Matches(ItemDrop.ItemData item)
            {
                if (m_all)
                    return true;

                if (m_weapon && item.IsWeapon())
                    return true;

                return item.m_equipped && m_equippedTypes.Contains(item.m_shared.m_itemType);
            }

            // Exact (case-insensitive) name match only - Enum.TryParse would also accept numbers and
            // comma-separated lists, which could silently match unintended item types.
            private static bool TryParseItemType(string name, out ItemDrop.ItemData.ItemType itemType)
            {
                foreach (ItemDrop.ItemData.ItemType candidate in Enum.GetValues(typeof(ItemDrop.ItemData.ItemType)))
                {
                    if (string.Equals(candidate.ToString(), name, StringComparison.OrdinalIgnoreCase))
                    {
                        itemType = candidate;
                        return true;
                    }
                }

                itemType = default;
                return false;
            }
        }

        // Equips each prefab in order. Humanoid.EquipItem picks the slot from the item's own type
        // and unequips whatever it replaces, so no per-slot handling is needed here.
        private static IEnumerator EquipItems(Humanoid humanoid, List<string> prefabNames)
        {
            if (prefabNames == null)
                yield break;

            foreach (string prefabName in prefabNames)
            {
                ItemDrop.ItemData itemData = AddItemByPrefabName(humanoid, prefabName);

                if (itemData == null)
                    continue;

                yield return EquipWithRetry(humanoid, itemData, prefabName);
            }
        }

        // Humanoid.EquipItem refuses while the creature is mid-attack or mid-dodge (plausible for a
        // creature that spawns next to a player), so keep trying for a bounded number of frames. Any
        // other refusal (durability, missing DLC) won't resolve by waiting and fails right away. On
        // failure the item is dropped from the inventory again so it can't be picked up later by
        // MonsterAI's weapon selection.
        private static IEnumerator EquipWithRetry(Humanoid humanoid, ItemDrop.ItemData itemData, string prefabName)
        {
            for (int attempt = 0; ; attempt++)
            {
                if (humanoid == null)
                    yield break;

                // No equip effects: this re-runs on every client each time the creature loads, and
                // would otherwise replay the equip sound/VFX for gear it was already wearing.
                if (humanoid.EquipItem(itemData, triggerEquipEffects: false))
                    yield break;

                bool busy = humanoid.InAttack() || humanoid.InDodge();

                if (!busy || attempt >= EquipRetryFrames)
                    break;

                yield return null;
            }

            Jotunn.Logger.LogWarning($"[WBTI] CreatureGearHelper: could not equip '{prefabName}' on {humanoid.name}.");
            humanoid.GetInventory().RemoveItem(itemData);
        }

        // Adds the item to the humanoid's own inventory via Humanoid.PickupPrefab - the same route
        // vanilla GiveDefaultItem uses for a monster's default loadout. It instantiates the prefab (so
        // ItemDrop.Awake runs and m_dropPrefab is set, which Humanoid.SetupVisEquipment needs) and
        // moves its ItemData into the inventory. Humanoid.EquipItem requires the item to already be a
        // member of the inventory. Returns null if the item could not be resolved or added. A
        // missing/unresolvable item is logged and skipped, not thrown - cosmetic gear failing to
        // resolve shouldn't abort the whole creature spawn.
        private static ItemDrop.ItemData AddItemByPrefabName(Humanoid humanoid, string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return null;

            GameObject itemPrefab = PrefabManager.Instance.GetPrefab(prefabName);

            if (itemPrefab == null)
            {
                Jotunn.Logger.LogWarning($"[WBTI] CreatureGearHelper: could not find item prefab '{prefabName}' to equip, skipping.");
                return null;
            }

            ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();

            if (itemDrop == null)
            {
                Jotunn.Logger.LogWarning($"[WBTI] CreatureGearHelper: prefab '{prefabName}' has no ItemDrop component, cannot equip.");
                return null;
            }

            // Checked before touching the inventory: Inventory.AddItem merges a stackable item into an
            // existing stack and returns true without adding the picked-up instance itself, so a
            // stackable item can never be equipped this way and must not mutate the inventory.
            if (itemDrop.m_itemData.m_shared.m_maxStackSize > 1)
            {
                Jotunn.Logger.LogWarning($"[WBTI] CreatureGearHelper: '{prefabName}' is a stackable item and cannot be equipped, skipping.");
                return null;
            }

            ItemDrop.ItemData itemData = humanoid.PickupPrefab(itemPrefab, 0, autoequip: false);

            // What Humanoid.EquipItem needs is this exact instance being in the inventory, so verify
            // that rather than trusting PickupPrefab's result alone.
            if (itemData == null || !humanoid.GetInventory().ContainsItem(itemData))
            {
                Jotunn.Logger.LogWarning($"[WBTI] CreatureGearHelper: could not add '{prefabName}' to {humanoid.name}'s inventory (inventory full?), skipping.");
                return null;
            }

            return itemData;
        }
    }
}
