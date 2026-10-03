namespace RestingRockFace.Helpers
{
    internal static class RockyGuiState
    {
        /// <summary>
        /// True while a Rocky name/face panel is open. Read by the GameCamera.UpdateCamera patch so
        /// the mouse wheel scrolls the dropdown instead of zooming the game camera.
        /// </summary>
        public static bool IsOpen { get; set; }
    }
}
