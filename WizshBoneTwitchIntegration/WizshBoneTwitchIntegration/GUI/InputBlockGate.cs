using Jotunn.Managers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Reference-counted wrapper around <see cref="GUIManager.BlockInput"/>. Several panels/dialogs
    /// can be open at once (e.g. a <see cref="ConfirmDialog"/> shown over the main shell panel);
    /// without counting, closing the top one calls BlockInput(false) and unblocks game input even
    /// though the panel underneath is still up. Callers <see cref="Push"/> on show and <see cref="Pop"/>
    /// on hide - input only unblocks once every opener has closed.
    ///
    /// This is a separate copy from GUI_OLD/InputBlockGate.cs (its own independent counter) rather
    /// than a shared one, per the new UI's isolation-from-GUI_OLD constraint - see the round 2 plan
    /// for the accepted edge case this implies if both GUIs are ever open at once.
    /// </summary>
    internal static class InputBlockGate
    {
        private static int s_count;

        public static void Push()
        {
            s_count++;
            if (s_count == 1)
                GUIManager.BlockInput(true);
        }

        public static void Pop()
        {
            if (s_count == 0)
                return;

            s_count--;
            if (s_count == 0)
                GUIManager.BlockInput(false);
        }
    }
}
