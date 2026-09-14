using WizshBoneTwitchIntegration.GuiOld;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TerrainEditData : CloneableData
    {
        [EditorLabel("Announce message")]
        [EditorTooltip("A custom message to announce that a terrain edit is about to occur. {{user}} will be replaced with the player's name.")]
        public string announceMessage;

        [EditorLabel("Level Terrain")]
        [EditorTooltip("Whether to level the terrain.")]
        public bool level = false;

        [EditorLabel("Level Offset")]
        [EditorTooltip("The offset for the terrain leveling.")]
        public float levelOffset = 0f;

        [EditorLabel("Level Radius")]
        [EditorTooltip("The radius of the terrain leveling.")]
        public float levelRadius = 0f;

        [EditorLabel("Level Square")]
        [EditorTooltip("Whether to level the terrain in a square shape.")]
        public bool levelSquare = false;

        [EditorLabel("Raise Terrain")]
        [EditorTooltip("Whether to raise the terrain.")]
        public bool raise = true;

        [EditorLabel("Raise Delta")]
        [EditorTooltip("How much the terrain is raised, negative values to dig down.")]
        public float raiseDelta = -6f;

        [EditorLabel("Raise Power")]
        [EditorTooltip("The power of the terrain raise.")]
        public float raisePower = 0f;

        [EditorLabel("Raise Radius")]
        [EditorTooltip("The radius of the terrain raise.")]
        public float raiseRadius = 8f;

        [EditorLabel("Smooth Terrain")]
        [EditorTooltip("Whether to smooth the terrain after editing.")]
        public bool smooth = false;

        [EditorLabel("Smooth Radius")]
        [EditorTooltip("The radius of the terrain smoothing.")]
        public float smoothRadius = 0f;

        [EditorLabel("Smooth Power")]
        [EditorTooltip("The power of the terrain smoothing.")]
        public float smoothPower = 0f;

        [EditorLabel("Paint Clear")]
        [EditorTooltip("Whether to clear the paint after editing.")]
        public bool paintClear = false;

        [EditorLabel("Paint Height Check")]
        [EditorTooltip("Whether to check the height of the paint after editing.")]
        public bool paintHeightCheck = false;

        [EditorLabel("Paint Radius")]
        [EditorTooltip("The radius of the paint.")]
        public float paintRadius = 0f;

        [EditorLabel("Paint Type")]
        [EditorTooltip("The type of paint to use.")]
        public string paintType = "Dirt";

        [EditorLabel("Duration")]
        [EditorTooltip("How long in seconds before the terrain resets. 0 = permanent.")]
        public float duration = 0f;

        [EditorHidden]
        [EditorLabel("Shape")]
        [EditorTooltip("The shape of the terrain edit.")]
        [DropdownOptions("Circle", "Rectangle", "Chevron")]
        public string shape = "Circle";

        [EditorLabel("Position offset")]
        [EditorTooltip("Where to apply the terrain edit relative to the player.")]
        public PositionOffsetData positionOffset = new PositionOffsetData();

        [EditorLabel("No fall damage")]
        [EditorTooltip("Whether to apply no fall damage to the player during the terrain edit.")]
        public bool noFallDamage = true;

        public TerrainEditData() { }
    }
}
