using System.Reflection;
using HarmonyLib;
using Jotunn.Managers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Reference-counted wrapper around <see cref="GUIManager.BlockInput"/>. Several panels/dialogs
    /// can be open at once (e.g. a <see cref="ConfirmDialog"/> shown over the main shell panel);
    /// without counting, closing the top one calls BlockInput(false) and unblocks game input even
    /// though the panel underneath is still up. Callers <see cref="Push"/> on show and <see cref="Pop"/>
    /// on hide - input only unblocks once every opener has closed.
    /// </summary>
    internal static class InputBlockGate
    {
        private static int s_count;

        // Jötunn keeps these internal/private; read to detect its block being lost (see Verify).
        private static readonly FieldInfo s_requestsField = AccessTools.Field(typeof(GUIManager), "InputBlockRequests");
        private static readonly FieldInfo s_blockedField = AccessTools.Field(typeof(GUIManager), "InputBlocked");

        /// <summary>Number of openers currently holding the gate (the shell itself counts as one).</summary>
        public static int Count => s_count;

        /// <summary>
        /// True while any new-UI panel/dialog is open. Read by the GameCamera.UpdateCamera patch so
        /// the mouse wheel scrolls the UI instead of zooming the game camera.
        /// </summary>
        public static bool IsBlocking => s_count > 0;

        /// <summary>
        /// Drops every outstanding push. Jötunn zeroes its own request counter on every scene load,
        /// so if a scene change (disconnect, kick, logout) destroys the UI while it holds pushes, the
        /// static count here would stay inflated for the rest of the process - and the next session's
        /// first <see cref="Push"/> would then never reach BlockInput(true).
        /// </summary>
        public static void Reset()
        {
            while (s_count > 0)
                Pop();
        }

        /// <summary>
        /// Safety net, called every frame by <see cref="WizshBoneGUI"/>. Jötunn's request counter is
        /// shared by every mod and is zeroed by <c>GUIManager.TryCreateGUI</c> (Game.Start, scene
        /// loads), so anything else touching it (an unbalanced BlockInput(false) from another mod, a
        /// GUI rebuild) silently unblocks game input while our panel is still up. If Jötunn holds
        /// no request at all, or no longer reports input as blocked, re-apply ours and log what it
        /// looked like so the source can be traced.
        ///
        /// Our whole gate is exactly ONE Jötunn request however many openers are stacked
        /// (<see cref="Push"/> only calls BlockInput(true) on 0 to 1, <see cref="Pop"/> only releases
        /// on 1 to 0), so the expected minimum is 1 - not <see cref="Count"/>. Comparing against the
        /// count would add a second request whenever a dialog is open over the shell, which the single
        /// release on close can never take back (input stuck blocked).
        /// </summary>
        public static void Verify()
        {
            if (s_count == 0 || s_requestsField == null || s_blockedField == null)
                return;

            int requests = (int)s_requestsField.GetValue(null);
            bool blocked = (bool)s_blockedField.GetValue(null);

            if (requests >= 1 && blocked)
                return;

            Jotunn.Logger.LogWarning($"[WBTI] InputBlockGate: Jötunn's input block was lost (requests={requests}, blocked={blocked}, openers={s_count}) - re-applying.");

            if (requests < 1)
            {
                GUIManager.BlockInput(true);
                return;
            }

            // The request is still counted but the flag was cleared behind its back: a push/pop pair
            // re-enables it without leaving an extra request behind.
            GUIManager.BlockInput(true);
            GUIManager.BlockInput(false);
        }

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
