namespace WizshBoneTwitchIntegration.Models
{
    internal class TerrainEditData : CloneableData
    {
        public string announceMessage;

        public bool level = false;

        public float levelOffset = 0f;

        public float levelRadius = 0f;

        public bool levelSquare = false;

        public bool raise = true;

        public float raiseDelta = -6f;

        public float raisePower = 0f;

        public float raiseRadius = 8f;

        public bool smooth = false;

        public float smoothRadius = 0f;

        public float smoothPower = 0f;

        public bool paintClear = false;

        public bool paintHeightCheck = false;

        public float paintRadius = 0f;

        public string paintType = "Dirt";

        public float duration = 0f;

        public string shape = "Circle";

        public PositionOffsetData positionOffset = new PositionOffsetData();

        public bool noFallDamage = true;

        public TerrainEditData() { }
    }
}
