using RestingRockFace.Gui;

namespace RestingRockFace.Helpers
{
    internal static class RockyGuiState
    {
        private static RockyGUI s_gui;

        /// <summary>
        /// True while the Rocky name/face panel is visible. Derived from the panel itself rather than a
        /// manual flag, so it can't get stuck if the panel is destroyed (e.g. logging out) while open.
        /// Read by the GameCamera.UpdateCamera patch so the mouse wheel scrolls the dropdown instead of
        /// zooming the game camera.
        /// </summary>
        public static bool IsOpen => s_gui != null && s_gui.IsVisible;

        public static void SetGui(RockyGUI gui)
        {
            s_gui = gui;
        }

        /// <summary>
        /// Closes the open panel (if any), releasing Jötunn's input block like the Close button does.
        /// </summary>
        public static void CloseOpenPanel()
        {
            if (IsOpen)
                s_gui.Close();
        }
    }
}
