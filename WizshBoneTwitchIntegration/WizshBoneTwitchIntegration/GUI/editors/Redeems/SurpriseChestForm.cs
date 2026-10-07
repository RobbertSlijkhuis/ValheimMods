using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for SurpriseChest: the chest-level settings as rows above a loot list
    /// (<see cref="EntryListEditor{TEntry}"/>, its header slot). Each loot entry is shown as either a
    /// Creature or an Item, never both - a creature entry is a 1-element
    /// <see cref="SurpriseChestSpawnData.creatureData"/> list, an item entry is
    /// <see cref="SurpriseChestSpawnData.itemData"/>. Creature entries reuse
    /// <see cref="CreatureEntryFields"/> (same rows as SpawnCreature).
    /// <para>
    /// The chest itself still spawns BOTH sides of an entry that has both (hand-edited YAML) and
    /// is deliberately left that way. Switching an entry's Loot type only hides the other side
    /// for the rest of the session so switching back restores it; on save
    /// (<see cref="IForcesValuesOnSave"/>) the hidden side of every entry the user switched is
    /// dropped, since it would otherwise still spawn. Entries that were never switched are saved
    /// exactly as loaded. <see cref="ChestType"/> Iron/Gold is the chest model; redeemTitle,
    /// mimicChance, position and positionOffset are dead/hidden and not shown.
    /// </para>
    /// </summary>
    internal class SurpriseChestForm : IRedeemStep2Form, IForcesValuesOnSave, IAppliesDefaultsOnSelect
    {
        private const string DefaultAnnounceMessage = "{{user}} has sent you a surprise!";
        private const int DefaultAmount = 5;
        private const int DefaultYeetChance = 5;
        private const float DefaultSpawnDelay = 0.7f;

        private const string CreatureType = "Creature";
        private const string ItemType = "Item";

        private static readonly List<DropdownOption> LootTypeOptions = new List<DropdownOption>
        {
            new DropdownOption(CreatureType, "Creature"),
            new DropdownOption(ItemType, "Item"),
        };

        private static readonly List<DropdownOption> ChestTypeOptions = new List<DropdownOption>
        {
            new DropdownOption(ChestType.Iron, "Iron"),
            new DropdownOption(ChestType.Gold, "Gold"),
        };

        // Per-entry UI state, keyed by the entry object itself so it survives step changes
        // (Populate runs again on Back into step 2) without living in the saved data.
        private sealed class EntryMode
        {
            public bool ShowItem;
            public bool Explicit;
        }

        private readonly ConditionalWeakTable<SurpriseChestSpawnData, EntryMode> m_modes = new ConditionalWeakTable<SurpriseChestSpawnData, EntryMode>();
        private readonly EntryListEditor<SurpriseChestSpawnData> m_list = new EntryListEditor<SurpriseChestSpawnData>();

        private RedeemData m_working;

        private InputField m_announceMessage;
        private InputField m_amount;
        private InputField m_spawnDelay;
        private SearchableDropdown m_type;
        private InputField m_yeetChance;

        private SurpriseChestData Data => m_working.chestData;

        public void Build(GameObject parent)
        {
            m_list.Build(parent, RedeemWizard.BodyTopY, EntryLabel, BuildEntryFields, BuildHeader,
                randomLabel: "Pick loot at random",
                randomDescription: "If enabled, the chest picks Amount entries at random from the list below (the same entry can come up more than once). Otherwise every entry spawns once, in list order, and Amount is ignored.");
        }

        // Chest-level rows above the loot list. Amount + Yeet chance come last, directly above the
        // random toggle Amount depends on. Face player, Interact and Force are hidden - see
        // ApplyDefaults.
        private float BuildHeader(GameObject content, float width)
        {
            var layout = new Step2RowLayout(content, 0f, width);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when the redeem triggers, a few seconds before the chest appears. {{user}} is replaced with the redeemer's name.",
                DefaultAnnounceMessage, "Optional announcement", v => Data.announceMessage = string.IsNullOrEmpty(v) ? null : v, defaultValue: DefaultAnnounceMessage);

            layout.PairRow(
                () => m_spawnDelay = layout.FloatRow("Spawn delay", "Time in seconds between each loot entry coming out of the chest.",
                    DefaultSpawnDelay, v => Data.spawnDelay = v, defaultValue: DefaultSpawnDelay),
                () => m_type = layout.DropdownRow("Type", "Which chest model is spawned.",
                    ChestTypeOptions, ChestType.Iron, v => Data.type = v, defaultValue: ChestType.Iron, showSearch: false));

            layout.PairRow(
                () => m_amount = layout.IntRow("Amount", "How many loot entries are picked when \"Pick loot at random\" (below) is on. Ignored otherwise - every entry spawns once.",
                    DefaultAmount, v => Data.amount = v, defaultValue: DefaultAmount),
                () => m_yeetChance = layout.IntRow("Yeet chance", "Percent chance (0-100) that a piece of loot is launched extremely far instead of just popping out of the chest.",
                    DefaultYeetChance, v => Data.yeetChance = v, defaultValue: DefaultYeetChance));

            return layout.CurrentY;
        }

        // ── loot entries ─────────────────────────────────────────────────────

        private static bool HasCreature(SurpriseChestSpawnData entry)
        {
            return entry.creatureData != null && entry.creatureData.Count > 0;
        }

        // An entry shows as an Item only when the user picked that, or (untouched) when it has an
        // item and no creature - creatures take precedence for display, same as a new entry
        // defaulting to Creature.
        private bool IsItemMode(SurpriseChestSpawnData entry)
        {
            EntryMode mode = m_modes.GetOrCreateValue(entry);
            if (mode.Explicit)
                return mode.ShowItem;

            return !HasCreature(entry) && entry.itemData != null;
        }

        // Chest loot creatures start with Allow drops on (SpawnCreature's own default is off);
        // only applies to newly created entries, saved ones keep their value.
        private static CreatureData NewChestCreature()
        {
            return new CreatureData { allowDrops = true };
        }

        private void SetLootType(SurpriseChestSpawnData entry, bool showItem)
        {
            EntryMode mode = m_modes.GetOrCreateValue(entry);
            mode.Explicit = true;
            mode.ShowItem = showItem;

            // The other side's data is left alone (hidden) so switching back restores it.
            if (showItem && entry.itemData == null)
                entry.itemData = new ItemData();
            else if (!showItem && !HasCreature(entry))
                entry.creatureData = new List<CreatureData> { NewChestCreature() };
        }

        private string EntryLabel(SurpriseChestSpawnData entry)
        {
            return EntryName(entry) + ChanceSuffix(entry);
        }

        private string EntryName(SurpriseChestSpawnData entry)
        {
            if (IsItemMode(entry))
            {
                ItemData item = entry.itemData;
                if (item == null || string.IsNullOrEmpty(item.prefabName))
                    return "New item";

                return $"{RedeemPrefabCatalog.GetItemDisplayName(item.prefabName)} x{item.amount}";
            }

            return HasCreature(entry) ? CreatureEntryFields.Label(entry.creatureData[0]) : "New loot";
        }

        // " (25%)" - the entry's share of the total weight. Only meaningful (and only shown) while
        // the chest picks its loot at random; every entry's share shifts when any weight changes,
        // so the whole list's labels are refreshed on each weight/random/add/delete change.
        private string ChanceSuffix(SurpriseChestSpawnData entry)
        {
            SurpriseChestData data = m_working?.chestData;
            if (data == null || !data.random || data.items == null)
                return "";

            float total = 0f;
            foreach (SurpriseChestSpawnData item in data.items)
                total += Mathf.Max(0f, item.weight);

            if (total <= 0f)
                return "";

            return $" ({Mathf.Max(0f, entry.weight) / total * 100f:0.#}%)";
        }

        private void BuildEntryFields(GameObject cardRoot, SurpriseChestSpawnData entry)
        {
            GuiHelper.ClearContainer(cardRoot);

            if (IsItemMode(entry))
            {
                // Item entries are a short plain card with no tabs (the tab strip is only for creatures).
                m_list.HideTabs();

                var layout = new Step2RowLayout(cardRoot, m_list.CardContentTopY, m_list.CardContentWidth);
                BuildEntryHead(layout, cardRoot, entry, isItem: true);
                BuildItemFields(layout, cardRoot, entry);

                // Runs on every (re)build of this card, including the Loot-type-triggered rebuild,
                // so the card and page scroll range always match the currently rendered rows.
                m_list.SetCardContentHeight(Mathf.Abs(layout.CurrentY));
                return;
            }

            // A new (or emptied) entry starts with one default creature; extra creatures in a
            // hand-edited list are left untouched and just not shown.
            if (!HasCreature(entry))
                entry.creatureData = new List<CreatureData> { NewChestCreature() };

            // Creature entries use the creature form's tabs; the Loot type and Weight rows lead the
            // General tab. BuildTabbed reports every tab's height to the list.
            CreatureEntryFields.BuildTabbed(m_list, cardRoot, entry.creatureData[0], m_list.RefreshListLabels,
                allowDropsDefault: true, generalHead: general => BuildEntryHead(general, cardRoot, entry, isItem: false));
        }

        // The Loot type and Weight rows every entry starts with, whichever loot type it is.
        private void BuildEntryHead(Step2RowLayout layout, GameObject cardRoot, SurpriseChestSpawnData entry, bool isItem)
        {
            layout.DropdownRow("Loot type", "Whether this entry spawns a creature or an item.",
                LootTypeOptions, isItem ? ItemType : CreatureType, v =>
                {
                    SetLootType(entry, v == ItemType);
                    m_list.RefreshListLabels();
                    BuildEntryFields(cardRoot, entry);
                },
                defaultValue: CreatureType, showSearch: false);

            layout.FloatRow("Weight", "How likely this entry is picked when \"Pick loot at random\" is on, relative to the other entries: weight 3 comes up three times as often as weight 1. 0 = never picked. Ignored when random is off. The list on the left shows each entry's current chance.",
                entry.weight, v =>
                {
                    entry.weight = v;
                    m_list.RefreshListLabels();
                },
                defaultValue: 1f, min: 0f);
        }

        private void BuildItemFields(Step2RowLayout layout, GameObject cardRoot, SurpriseChestSpawnData entry)
        {
            if (entry.itemData == null)
                entry.itemData = new ItemData();
            ItemData item = entry.itemData;

            // The dropdown shows its first option when the saved value is missing, so save that
            // option too - otherwise it would display an item that was never actually stored.
            if (string.IsNullOrEmpty(item.prefabName) && RedeemPrefabCatalog.ItemPrefabs.Count > 0)
                item.prefabName = RedeemPrefabCatalog.ItemPrefabs[0].Value;

            // The amount is limited to one stack of the chosen item (the game caps it at spawn anyway).
            int maxStack = RedeemPrefabCatalog.GetItemMaxStack(item.prefabName);
            item.amount = Mathf.Clamp(item.amount, 1, maxStack);

            List<DropdownOption> options = RedeemPrefabCatalog.EnsureIncludesCurrentValue(RedeemPrefabCatalog.ItemPrefabs, item.prefabName);
            layout.DropdownRow("Item prefab", "Which item comes out of the chest. Only real, holdable items are listed.",
                options, item.prefabName, v =>
                {
                    item.prefabName = v;
                    m_list.RefreshListLabels();
                    BuildEntryFields(cardRoot, entry);
                },
                defaultValue: null);

            layout.PairRow(
                () => layout.IntRow("Amount", "How many of the item come out of the chest (as one stack, up to the item's max stack size).",
                    item.amount, v =>
                    {
                        item.amount = v;
                        m_list.RefreshListLabels();
                    },
                    defaultValue: 1, min: 1, max: maxStack),
                () => layout.IntRow("Quality", "Quality level of the item (1 = normal, max 10). Can exceed the item's max quality, like Refinement Forge upgrades, but only on items that support it.",
                    item.quality, v => item.quality = v, defaultValue: 1, min: 1, max: ItemData.MaxQuality));
        }

        // ── lifecycle ────────────────────────────────────────────────────────

        public void Populate(RedeemData working)
        {
            m_working = working;
            SurpriseChestData data = working.chestData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_yeetChance.text = data.yeetChance.ToString();
            m_spawnDelay.text = data.spawnDelay.ToString("G");
            m_type.Value = data.type;
            m_amount.text = data.amount.ToString();

            if (data.items == null)
                data.items = new List<SurpriseChestSpawnData>();

            // A chest with no loot does nothing, so a fresh (or emptied) list starts with one
            // default entry - same as SpawnCreatureForm/StatusEffectForm.
            if (data.items.Count == 0)
                data.items.Add(new SurpriseChestSpawnData());

            m_list.Populate(data.items, () => data.random, v =>
            {
                data.random = v;
                m_list.RefreshListLabels();
            });
        }

        // Starting values for a freshly selected Surprise Chest (never applied when re-entering step
        // 2 or opening an existing redeem). Face player, Interact and Force have no rows; they're
        // set here so a new chest always gets them, and an existing redeem keeps whatever it has.
        public void ApplyDefaults(RedeemData working)
        {
            SurpriseChestData data = working.chestData;
            data.announceMessage = DefaultAnnounceMessage;
            data.amount = DefaultAmount;
            data.yeetChance = DefaultYeetChance;
            data.random = true;
            data.facePlayer = true;
            data.interact = true;
            data.force = 200f;
        }

        // Drops the hidden side of every entry the user switched Loot type on - the chest spawns
        // both sides of an entry, so leaving it would still spawn what the form says isn't there.
        public void ApplyForcedValues(RedeemData working)
        {
            if (working.chestData?.items == null)
                return;

            foreach (SurpriseChestSpawnData entry in working.chestData.items)
            {
                if (!m_modes.TryGetValue(entry, out EntryMode mode) || !mode.Explicit)
                    continue;

                if (mode.ShowItem)
                    entry.creatureData = null;
                else
                    entry.itemData = null;
            }
        }
    }
}
