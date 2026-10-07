using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The per-creature field rows and list-row label shared by every step-2 form that edits a
    /// <see cref="CreatureData"/> in an <see cref="EntryListEditor{TEntry}"/> card
    /// (<see cref="SpawnCreatureForm"/>, <see cref="SurpriseChestForm"/>), so both stay identical.
    /// The rows are split over four tabs (<see cref="TabNames"/>): General, Appearance (color, size,
    /// equipment), Behavior and Spawn.
    /// </summary>
    internal static class CreatureEntryFields
    {
        public static readonly string[] TabNames = { "General", "Appearance", "Behavior", "Spawn" };

        // SpawnPositionType.Undefined is a "not set" marker, not a real choice, so it isn't listed.
        private static readonly List<DropdownOption> PositionOptions = new List<DropdownOption>
        {
            new DropdownOption(SpawnPositionType.InFrontOfPlayer, "In front of player"),
            new DropdownOption(SpawnPositionType.OnPlayer, "On player"),
            new DropdownOption(SpawnPositionType.Random, "Random"),
            new DropdownOption(SpawnPositionType.RandomBehind, "Random behind"),
            new DropdownOption(SpawnPositionType.RandomFlying, "Random flying"),
            new DropdownOption(SpawnPositionType.WorldPosition, "World position"),
        };

        // The values are what CreatureGearHelper's removeEquipment parser accepts: ItemType names plus
        // "Weapon" and "All".
        private static readonly List<DropdownOption> RemoveEquipmentOptions = new List<DropdownOption>
        {
            new DropdownOption("Shield", "Shield"),
            new DropdownOption("Helmet", "Helmet"),
            new DropdownOption("Chest", "Chest armor"),
            new DropdownOption("Legs", "Leg armor"),
            new DropdownOption("Shoulder", "Cape"),
            new DropdownOption("Utility", "Utility item"),
            new DropdownOption("Trinket", "Trinket"),
            new DropdownOption("Weapon", "All weapons (incl. natural attacks)"),
            new DropdownOption("All", "Everything"),
        };

        /// <summary>Left-list row text for a creature entry: "Neck x3", or "New creature" until a prefab is picked.</summary>
        public static string Label(CreatureData creature)
        {
            if (string.IsNullOrEmpty(creature.prefabName))
                return "New creature";

            string name = RedeemPrefabCatalog.GetCreatureDisplayName(creature.prefabName);
            return $"{name} x{creature.amount}";
        }

        /// <summary>
        /// Builds the creature's tabbed field rows into the entry card <paramref name="cardRoot"/>
        /// (already cleared by the caller): opts the card into <paramref name="list"/>'s tab strip,
        /// lays each tab's rows into its own root and reports every tab's height back to the list.
        /// <paramref name="onLabelChanged"/> runs when Prefab or Amount changes, since both appear in
        /// <see cref="Label"/>. <paramref name="allowDropsDefault"/> is only what the Allow drops reset
        /// button restores; the toggle's value itself comes from the entry. <paramref name="generalHead"/>
        /// lets a form put its own rows at the top of the General tab (e.g. SurpriseChest's Loot type).
        /// </summary>
        public static void BuildTabbed<TEntry>(EntryListEditor<TEntry> list, GameObject cardRoot, CreatureData creature,
            Action onLabelChanged, bool allowDropsDefault = false, Action<Step2RowLayout> generalHead = null)
            where TEntry : CloneableData, new()
        {
            GameObject[] roots = list.BuildTabs(cardRoot, TabNames);
            var layouts = new Step2RowLayout[roots.Length];

            for (int i = 0; i < roots.Length; i++)
                layouts[i] = new Step2RowLayout(roots[i], list.CardContentTopY, list.CardContentWidth);

            generalHead?.Invoke(layouts[0]);
            BuildGeneralTab(layouts[0], creature, onLabelChanged, allowDropsDefault);
            BuildAppearanceTab(layouts[1], creature);
            BuildBehaviorTab(layouts[2], creature);
            BuildSpawnTab(layouts[3], creature);

            for (int i = 0; i < layouts.Length; i++)
                list.SetTabContentHeight(i, Mathf.Abs(layouts[i].CurrentY));
        }

        private static void BuildGeneralTab(Step2RowLayout layout, CreatureData creature, Action onLabelChanged, bool allowDropsDefault)
        {
            List<DropdownOption> prefabOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(RedeemPrefabCatalog.CreaturePrefabs, creature.prefabName);
            layout.DropdownRow("Prefab name", "Which creature prefab gets spawned.",
                prefabOptions, creature.prefabName, v =>
                {
                    creature.prefabName = v;
                    onLabelChanged?.Invoke();
                },
                defaultValue: null);

            layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                creature.announceMessage ?? "", "Optional announcement", v => creature.announceMessage = v, defaultValue: "");

            layout.PairRow(
                () => layout.IntRow("Amount", "How many of this creature to spawn.",
                    creature.amount, v =>
                    {
                        creature.amount = v;
                        onLabelChanged?.Invoke();
                    },
                    defaultValue: 1),
                () => layout.IntRow("Max spawned", "This creature is not spawned while this many of it, spawned by redeems, already exist within 100 m of the player. 0 = no limit.",
                    creature.maxSpawned, v => creature.maxSpawned = v, defaultValue: 0, min: 0));

            layout.PairRow(
                () => layout.IntRow("Level", "Star level of the creature (1 = none, up to 10 = max).",
                    creature.level, v => creature.level = v, defaultValue: 1, min: 1, max: 10),
                () => layout.FloatRow("Max health", "Replaces the creature's maximum health. 0 = the creature's own. Health scaling from the profile settings is applied on top.",
                    creature.maxHealth, v => creature.maxHealth = v, defaultValue: 0f, min: 0f));

            layout.PairRow(
                () => layout.FloatRow("Health scale", "Overrides how strongly this creature's health scales with the player's progression. 0 = use the profile setting. Only applies when creature scaling is on in Settings.",
                    creature.healthScale, v => creature.healthScale = v, defaultValue: 0f, min: 0f),
                () => layout.FloatRow("Damage scale", "Overrides how strongly this creature's damage scales with the player's progression. 0 = use the profile setting.",
                    creature.damageScale, v => creature.damageScale = v, defaultValue: 0f, min: 0f));

            layout.PairRow(
                () => layout.ToggleRow("Is boss", "Treats the creature as a boss (boss health bar and music).",
                    creature.isBoss, v => creature.isBoss = v, defaultValue: false),
                () => layout.ToggleRow("Allow drops", "Whether the creature drops its loot when killed. Off by default to prevent loot farming.",
                    creature.allowDrops, v => creature.allowDrops = v, defaultValue: allowDropsDefault));

            layout.PairRow(
                () => layout.ToggleRow("Rename", "Whether the creature gets the name on the right (or the redeemer's name when that is empty). Off keeps the creature's own name.",
                    creature.rename, v => creature.rename = v, defaultValue: true),
                () => layout.TextRow("Name", "The name shown for the creature. Leave empty to use the redeemer's name.",
                    creature.name ?? "", "Redeemer's name", v => creature.name = string.IsNullOrEmpty(v) ? null : v, defaultValue: ""));
        }

        private static void BuildAppearanceTab(Step2RowLayout layout, CreatureData creature)
        {
            BuildColorRow(layout, creature);

            layout.PairRow(
                () => layout.FloatRow("Size", "Overall size multiplier of the creature.",
                    creature.size, v => creature.size = v, defaultValue: 1f),
                () => layout.ToggleRow("Force color", "Makes the color above win even when the redeemer is a registered viewer with their own personal color. Off by default, so a viewer's own color takes priority.",
                    creature.forceColor, v => creature.forceColor = v, defaultValue: false));

            List<string> equipValues = creature.equipItems ?? (creature.equipItems = new List<string>());
            List<DropdownOption> equipOptions = RedeemPrefabCatalog.EquippableItemPrefabs;

            // A saved item that is no longer in the catalog still has to show up (and stay checked).
            foreach (string value in equipValues)
                equipOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(equipOptions, value);

            layout.ChecklistRow("Equip items",
                "Items the creature wears or holds, e.g. a helmet or a tankard. They are added to the creature's inventory and equipped; the slot is chosen by the item type and replaces whatever was there. A held item can be swapped out for a weapon when the creature has a target.",
                equipOptions, equipValues, v => creature.equipItems = v);

            List<string> removeValues = creature.removeEquipment ?? (creature.removeEquipment = new List<string>());
            layout.ChecklistRow("Remove equipment",
                "Parts of the creature's default loadout to take away before the items above are equipped. Removed items leave its inventory, so it can't use them again. Warning: removing weapons also removes a creature's natural attacks and can leave it unable to attack. A weapon you add above sits alongside its default weapons unless you remove those too.",
                RemoveEquipmentOptions, removeValues, v => creature.removeEquipment = v);
        }

        private static void BuildBehaviorTab(Step2RowLayout layout, CreatureData creature)
        {
            layout.PairRow(
                () => layout.ToggleRow("Friendly", "Whether the creature is friendly toward the player.",
                    creature.friendly, v => creature.friendly = v, defaultValue: false),
                () => layout.ToggleRow("Commandable", "Requires Friendly to be enabled. Whether the player can command this creature.",
                    creature.commandable, v => creature.commandable = v, defaultValue: false));

            layout.PairRow(
                () => layout.ToggleRow("Fully passive", "The creature never targets anything and nothing targets it, not even wild monsters. Players can still kill it manually.",
                    creature.fullyPassive, v => creature.fullyPassive = v, defaultValue: false),
                () => layout.ToggleRow("Always follow owner", "Requires Friendly to be enabled. The creature always follows the player, independent of Commandable, and keeps closing the distance instead of stopping a few meters away.",
                    creature.alwaysFollowOwner, v => creature.alwaysFollowOwner = v, defaultValue: false));

            layout.PairRow(
                () => layout.ToggleRow("Mist vision", "Whether the creature can see through Mist when looking for targets. Off lets Mist hide the player from it.",
                    creature.mistVision, v => creature.mistVision = v, defaultValue: true),
                () => layout.ToggleRow("Aggravatable", "Lets the creature be provoked into hostility. Vanilla uses this for creatures that start out neutral towards players (such as Dvergr) and turn hostile once something aggravates them. Off = it never changes sides this way.",
                    creature.aggravatable, v => creature.aggravatable = v, defaultValue: false));

            layout.PairRow(
                () => layout.ToggleRow("Allow damage structures", "Whether the creature's attacks can damage buildings and other structures. Turn off to stop it wrecking a base.",
                    creature.allowDamageStructures, v => creature.allowDamageStructures = v, defaultValue: true),
                () => layout.FloatRow("Idle sound interval", "Seconds between the creature's idle sound effect (e.g. a Fuling's laugh); it then always plays instead of the default ~50% chance. 0 = the creature's default behavior.",
                    creature.idleSoundInterval, v => creature.idleSoundInterval = v, defaultValue: 0f, min: 0f));

            layout.PairRow(
                () => layout.ToggleRow("Talks", "Whether the creature says something above its head. Needs a talk message below.",
                    creature.talks, v => creature.talks = v, defaultValue: false),
                () => layout.FloatRow("Talk interval", "Seconds between messages. An interval of 3 or more repeats the message(s) for as long as the creature lives; anything below 3 says it just once.",
                    creature.talkInterval, v => creature.talkInterval = v, defaultValue: 0f, min: 0f));

            layout.TextRow("Talk message", "What the creature says. Separate several messages with a semicolon (;). {{user}} is replaced with the redeemer's name. Leave empty for none. Only used when Talks is on. On a redeem that asks the viewer for input, the viewer's message replaces this.",
                creature.talkMessage ?? "", "Optional message", v => creature.talkMessage = string.IsNullOrEmpty(v) ? null : v, defaultValue: "");

            layout.FloatRow("Speed multiplier", "Multiplies the creature's walking and running speed (1 = normal speed).",
                creature.speedMultiplier, v => creature.speedMultiplier = v, defaultValue: 1f, min: 0f);
        }

        private static void BuildSpawnTab(Step2RowLayout layout, CreatureData creature)
        {
            List<DropdownOption> positionOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(PositionOptions, creature.position);
            layout.PairRow(
                () => layout.DropdownRow("Position", "Where the creature spawns, relative to the player (or to the chest, for a chest's loot). World position treats the offset below as absolute world coordinates.",
                    positionOptions, creature.position, v => creature.position = v,
                    defaultValue: SpawnPositionType.Random, showSearch: false),
                () => layout.FloatRow("Position radius", "How far from the spawn point a creature can appear. Only used by the Random and Random flying positions.",
                    creature.positionRadius, v => creature.positionRadius = v, defaultValue: 10f, min: 0f));

            PositionOffsetData offset = creature.positionOffset ?? (creature.positionOffset = new PositionOffsetData());
            layout.Vector3Row("Position offset (X / Y / Z)", "Shifts the spawn point by this many meters, left to right: X (sideways), Y (up) and Z (forward). Ignored by the Random positions, except Random behind.",
                new Vector3(offset.x, offset.y, offset.z), v =>
                {
                    offset.x = v.x;
                    offset.y = v.y;
                    offset.z = v.z;
                });

            layout.ToggleRow("Face player", "Whether the creature spawns facing the player instead of in a random direction.",
                creature.facePlayer, v => creature.facePlayer = v, defaultValue: false);
        }

        // Color + Emission color on one row. The emission is "linked" to the main color while
        // CreatureData.emissionColor is empty: its swatch then follows the main swatch, and picking
        // (or resetting to) the main color stores null so a later main-color change keeps the glow in
        // step. Same behavior as ViewerEditDialog.
        private static void BuildColorRow(Step2RowLayout layout, CreatureData creature)
        {
            const string defaultColor = "#ffffff";
            GameObject emissionSwatch = null;

            layout.PairRow(
                () => layout.ColorRow("Color", "Overrides the creature's color. Leave default for no override.",
                    creature.color ?? defaultColor, v =>
                    {
                        creature.color = v;

                        if (string.IsNullOrEmpty(creature.emissionColor))
                            SetSwatchColor(emissionSwatch, v);
                    },
                    defaultValue: defaultColor),
                () => emissionSwatch = layout.ColorRow("Emission color", "Overrides the color of the creature's glow. Follows Color until a different color is picked here.",
                    string.IsNullOrEmpty(creature.emissionColor) ? creature.color ?? defaultColor : creature.emissionColor, v =>
                    {
                        string mainColor = creature.color ?? defaultColor;
                        bool follows = string.IsNullOrEmpty(v) || string.Equals(v, mainColor, StringComparison.OrdinalIgnoreCase);

                        creature.emissionColor = follows ? null : v;

                        // A reset hands back "", which the swatch itself can't paint.
                        if (follows)
                            SetSwatchColor(emissionSwatch, mainColor);
                    },
                    defaultValue: ""));
        }

        // CreateColorField's swatch has no "set color" API - paint the overlay Image the picker paints.
        private static void SetSwatchColor(GameObject swatch, string hex)
        {
            if (swatch != null && ColorUtility.TryParseHtmlString(hex, out Color color))
                swatch.transform.Find("ColorOverlay").GetComponent<Image>().color = color;
        }
    }
}
