using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Waits for the next key press and reports it with the modifiers (Ctrl/Shift/Alt) held at that
    /// moment. Enabled only while a rebind is in progress - disabling it (also when the card it
    /// lives on is destroyed) ends <see cref="WindowKeyHelper"/>'s capture, so the window toggle
    /// can never stay suppressed. Reads raw Unity input, since ZInput is blocked while the window is open.
    /// </summary>
    internal class KeyCaptureListener : MonoBehaviour
    {
        private static readonly KeyCode[] CapturableKeys = BuildCapturableKeys();

        // Left/right pairs of the modifiers a shortcut may include.
        private static readonly KeyCode[][] ModifierPairs =
        {
            new[] { KeyCode.LeftControl, KeyCode.RightControl },
            new[] { KeyCode.LeftShift, KeyCode.RightShift },
            new[] { KeyCode.LeftAlt, KeyCode.RightAlt },
        };

        private Action<KeyCode, List<KeyCode>> m_onKey;
        private Action m_onCancel;

        public void Init(Action<KeyCode, List<KeyCode>> onKey, Action onCancel)
        {
            m_onKey = onKey;
            m_onCancel = onCancel;
        }

        private void Update()
        {
            if (!Input.anyKeyDown)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                m_onCancel?.Invoke();
                return;
            }

            foreach (KeyCode key in CapturableKeys)
            {
                if (!Input.GetKeyDown(key))
                    continue;

                m_onKey?.Invoke(key, HeldModifiers());
                return;
            }
        }

        private void OnDisable()
        {
            WindowKeyHelper.EndCapture();
        }

        private static List<KeyCode> HeldModifiers()
        {
            List<KeyCode> held = new List<KeyCode>();

            foreach (KeyCode[] pair in ModifierPairs)
            {
                foreach (KeyCode modifier in pair)
                {
                    if (Input.GetKey(modifier))
                    {
                        held.Add(modifier);
                        break;
                    }
                }
            }

            return held;
        }

        // Keyboard keys only: no mouse/joystick buttons (KeyCode.Mouse0 and up), no modifiers on
        // their own, and Escape is the cancel key.
        private static KeyCode[] BuildCapturableKeys()
        {
            List<KeyCode> keys = new List<KeyCode>();

            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (key == KeyCode.None || key == KeyCode.Escape || key >= KeyCode.Mouse0)
                    continue;

                if (IsModifier(key))
                    continue;

                keys.Add(key);
            }

            return keys.ToArray();
        }

        private static bool IsModifier(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                case KeyCode.LeftAlt:
                case KeyCode.RightAlt:
                case KeyCode.LeftCommand:
                case KeyCode.RightCommand:
                case KeyCode.LeftWindows:
                case KeyCode.RightWindows:
                case KeyCode.AltGr:
                    return true;
                default:
                    return false;
            }
        }
    }
}
