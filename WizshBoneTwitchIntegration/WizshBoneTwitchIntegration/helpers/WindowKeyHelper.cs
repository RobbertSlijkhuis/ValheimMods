using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using Jotunn;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Rebinding of the key that opens/closes the main window (<see cref="PluginConfig.configWizshBoneWindow"/>)
    /// from the Home tab. The config entry is the single source of truth - Jötunn's button
    /// (<c>ShortcutConfig</c>), the HUD's key hint and the Configuration Manager all read it live, so
    /// writing it here is enough.
    /// </summary>
    internal static class WindowKeyHelper
    {
        // The frame a capture ended on. The key that finished the capture is "down" for that whole
        // frame, so the window toggle has to stay suppressed through it - otherwise binding e.g. F5
        // would also be read as the toggle press and close the window right away.
        private static int s_captureEndedFrame = -1;

        public static bool IsCapturing { get; private set; }

        /// <summary>True while the window toggle key must be ignored (a rebind is in progress or just ended).</summary>
        public static bool SuppressToggle => IsCapturing || Time.frameCount <= s_captureEndedFrame;

        public static void BeginCapture()
        {
            IsCapturing = true;
        }

        public static void EndCapture()
        {
            if (!IsCapturing)
                return;

            IsCapturing = false;
            s_captureEndedFrame = Time.frameCount;
        }

        public static string Describe()
        {
            return PluginConfig.configWizshBoneWindow.Value.ToString();
        }

        /// <summary>
        /// Binds the window key to <paramref name="key"/> (+ <paramref name="modifiers"/>). Returns
        /// false with a message when the combination is already taken by the quick test redeem key.
        /// </summary>
        public static bool TryApply(KeyCode key, IEnumerable<KeyCode> modifiers, out string error)
        {
            KeyboardShortcut shortcut = new KeyboardShortcut(key, new List<KeyCode>(modifiers).ToArray());

            if (shortcut.Equals(PluginConfig.configQuickTestRedeemKey.Value))
            {
                error = $"{shortcut} is already used by the quick test redeem key.";
                return false;
            }

            PluginConfig.configWizshBoneWindow.Value = shortcut;
            RebindZInputButton();
            error = null;
            return true;
        }

        /// <summary>Restores the config entry's default key (F3).</summary>
        public static bool TryReset(out string error)
        {
            KeyboardShortcut defaultShortcut = (KeyboardShortcut)PluginConfig.configWizshBoneWindow.DefaultValue;
            return TryApply(defaultShortcut.MainKey, defaultShortcut.Modifiers, out error);
        }

        /// <summary>
        /// Points the game's ZInput button for the window key at the new main key. Jötunn registers that
        /// button once at startup and only re-applies a changed shortcut when the Configuration Manager
        /// window closes - so after a rebind from our own UI, <c>ZInput.GetButtonDown</c> (what opens the
        /// window) would keep checking the old key and the window couldn't be opened until a restart.
        /// Mirrors Jötunn's internal <c>InputUtils.SetInputButtons</c> for a KeyboardShortcut entry.
        /// </summary>
        private static void RebindZInputButton()
        {
            try
            {
                ConfigEntry<KeyboardShortcut> entry = PluginConfig.configWizshBoneWindow;
                string buttonName = entry.GetBoundButtonName();
                if (ZInput.instance == null || string.IsNullOrEmpty(buttonName))
                    return;

                FieldInfo buttonsField = typeof(ZInput).GetField("m_buttons", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var buttons = buttonsField?.GetValue(ZInput.instance) as Dictionary<string, ZInput.ButtonDef>;
                if (buttons != null && buttons.TryGetValue(buttonName, out ZInput.ButtonDef button))
                    button.Rebind(ZInput.KeyCodeToPath(entry.Value.MainKey, false));
                else
                    Jotunn.Logger.LogWarning($"[WBTI] Could not find the ZInput button '{buttonName}' to rebind the window key.");
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not rebind the window key's input button: {e}");
            }
        }
    }
}
