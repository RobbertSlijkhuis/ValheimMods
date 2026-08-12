using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Models.Views
{
    /// <summary>
    /// Narrowed editor view over a single <see cref="CreatureData"/> list entry for the
    /// SpawnCreature redeem's simple form - exposes only the fields most redeems need,
    /// while every edit writes straight through to the real <see cref="CreatureData"/> instance.
    /// </summary>
    internal sealed class CreatureSimpleView
    {
        [EditorLabel("Prefab name")]
        [EditorTooltip("The creature prefab to spawn (e.g. Neck, Greyling, Boar).")]
        [CreaturePrefabNameDropdown]
        public BoundField<string> prefabName;

        [EditorLabel("Amount")]
        [EditorTooltip("Number of of this creature to spawn.")]
        public BoundField<int> amount;

        [EditorLabel("Level")]
        [EditorTooltip("Star level (1 = no star, 10 is max).")]
        public BoundField<int> level;

        [EditorLabel("Size")]
        [EditorTooltip("Scale multiplier applied to the spawned creature.")]
        public BoundField<float> size;

        [EditorLabel("Speed multiplier")]
        [EditorTooltip("Movement speed multiplier applied to the spawned creature (1 = normal speed).")]
        public BoundField<float> speedMultiplier;

        [EditorLabel("Friendly")]
        public BoundField<bool> friendly;

        [EditorLabel("Commandable")]
        [EditorTooltip("Requires Friendly to be enabled.")]
        public BoundField<bool> commandable;

        [EditorLabel("Rename")]
        [EditorTooltip("Name of the creature, if left blank it will use the redeemer name")]
        public BoundField<bool> rename;

        //public BoundField<bool> allowDrops;
        //public BoundField<bool> isBoss;
        //public BoundField<bool> MistVis;

        [EditorLabel("Creature color")]
        [EditorTooltip("Overrides the creature's color in hex format (e.g. #ffffff for white). Leave blank for default.")]
        [ColorPicker]
        public BoundField<string> color;

        //public BoundField<string> name;
        //public BoundField<string> globalKeyAdd;
        //public BoundField<string> globalKeyRemove;

        [EditorLabel("Announce message")]
        [EditorTooltip("Optional chat message announced when this redeem triggers.")]
        public BoundField<string> announceMessage;

        public CreatureSimpleView(CreatureData real)
        {
            prefabName = new BoundField<string>(
                () => real.prefabName,
                v => real.prefabName = v);

            amount = new BoundField<int>(
                () => real.amount,
                v => real.amount = v);

            level = new BoundField<int>(
                () => real.level,
                v => real.level = v);

            size = new BoundField<float>(
                () => real.size,
                v => real.size = v);

            speedMultiplier = new BoundField<float>(
                () => real.speedMultiplier,
                v => real.speedMultiplier = v);

            friendly = new BoundField<bool>(
                () => real.friendly,
                v => real.friendly = v);

            commandable = new BoundField<bool>(
                () => real.commandable,
                v => real.commandable = v);

            rename = new BoundField<bool>(
                () => real.rename,
                v => real.rename = v);

            color = new BoundField<string>(
                () => real.color,
                v => real.color = v);

            announceMessage = new BoundField<string>(
                () => real.announceMessage,
                v => real.announceMessage = v);
        }
    }
}
