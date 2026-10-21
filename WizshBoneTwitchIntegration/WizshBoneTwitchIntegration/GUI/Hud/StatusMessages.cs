using System;
using Jotunn.Managers;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The status messages the mod puts on screen through <see cref="StatusMessageHUD"/>: one place for each
    /// message's id, text, color and priority, so whatever happens only has to say whether it is on or off.
    /// A higher priority is listed higher up.
    /// </summary>
    internal static class StatusMessages
    {
        private const int PriorityNotice = 0;
        private const int PriorityProblem = 10;

        /// <summary>The streamer is inside a Twitch safe zone. Can be turned off in the settings.</summary>
        public static void SetSafeZone(bool on)
        {
            Set(on, "safezone", "Streamer is in a safe zone!", GUIManager.Instance?.ValheimOrange ?? Color.yellow, PriorityNotice,
                () => ProfileSettingsHelper.Current.showSafeZoneMessage);
        }

        /// <summary>The redeems plus the channel's other rewards don't fit under Twitch's limit, so Twitch was left as it was.</summary>
        public static void SetRedeemsOverLimit(bool on)
        {
            Set(on, "redeems-over-limit", "Redeems need fixing! (over Twitch's limit)", Color.red, PriorityProblem);
        }

        private static void Set(bool on, string id, string text, Color color, int priority, Func<bool> visibleWhen = null)
        {
            WizshBoneGUI gui = Game.instance?.gameObject.GetComponent<WizshBoneGUI>();

            if (gui == null)
                return;

            if (on)
                gui.ShowStatusMessage(id, text, color, priority, visibleWhen);
            else
                gui.HideStatusMessage(id);
        }
    }
}
