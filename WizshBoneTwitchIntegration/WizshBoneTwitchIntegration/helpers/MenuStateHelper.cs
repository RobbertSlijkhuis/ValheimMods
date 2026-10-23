using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class MenuStateHelper
    {
        /// <summary>
        /// Whether the player is paused or busy in a menu: Valheim's own (the ESC menu also pauses a single-player
        /// game), or the WizshBone window, which blocks input but pauses nothing. What "Reject if in menus" checks.
        /// </summary>
        public static bool IsInMenu()
        {
            if (Game.IsPaused() || Menu.IsVisible())
                return true;

            WizshBoneGUI gui = Game.instance?.gameObject.GetComponent<WizshBoneGUI>();

            return gui != null && gui.IsVisible;
        }
    }
}
