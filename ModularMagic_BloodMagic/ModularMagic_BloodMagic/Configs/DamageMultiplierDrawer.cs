using BepInEx.Configuration;
using ModularMagic_BloodMagic.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using StaffConfig = ModularMagic_Core.Configs.StaffConfig;

namespace ModularMagic_BloodMagic.Configs
{
    /// <summary>
    /// The editor of a "Damage multiplier" entry in the in-game config window (ConfigurationManager, F1). It shows the value with a slider
    /// and a text field, and below it the damage of the hardest hit of the apparitions at quality 1 and at the max quality of the scythe.
    /// It is drawn every frame from the current config, so a change to the multiplier or to the damage of the scythe shows at once.
    /// The config file has no such preview, it only has the description.
    /// </summary>
    internal static class DamageMultiplierDrawer
    {
        private const float SliderMax = 2f;

        // The text in the text field of each entry while it is typed in, so a value that is not complete yet (like "0.") is not replaced
        private static readonly Dictionary<ConfigEntryBase, string> typedText = new Dictionary<ConfigEntryBase, string>();

        public static Action<ConfigEntryBase> Create(StaffConfig scythe)
        {
            return entryBase => Draw(entryBase, scythe);
        }

        private static void Draw(ConfigEntryBase entryBase, StaffConfig scythe)
        {
            if (entryBase is not ConfigEntry<float> entry)
                return;

            // The config window does not check this for a custom editor. Jötunn marks an admin only entry as read only for a player who is
            // not an admin, the server's value is used then
            bool readOnly = entry.Description.Tags.OfType<ConfigurationManagerAttributes>().Any(attributes => attributes.ReadOnly == true);
            bool wasEnabled = GUI.enabled;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUI.enabled = wasEnabled && !readOnly;

            // The slider returns its value clamped to its range, so it is only used when it is moved: a value above SliderMax from the
            // text field or the config file stays as it is
            bool changedBefore = GUI.changed;
            GUI.changed = false;

            float slider = GUILayout.HorizontalSlider(entry.Value, 0f, SliderMax, GUILayout.ExpandWidth(true));
            if (GUI.changed && !readOnly)
                entry.Value = Mathf.Round(slider * 100f) / 100f;

            GUI.changed |= changedBefore;

            DrawTextField(entry, readOnly);

            GUI.enabled = wasEnabled;

            GUILayout.EndHorizontal();
            GUILayout.Label(GetPreviewText(scythe, entry.Value));
            GUILayout.EndVertical();
        }

        private static void DrawTextField(ConfigEntry<float> entry, bool readOnly)
        {
            // The typed text is kept while it means the current value, or is not a number yet (an empty field while typing). When the
            // value changes in another way (the slider, the reset button, the server), the field shows the new value
            if (!typedText.TryGetValue(entry, out string text) || (TryParse(text, out float typed) && typed != entry.Value))
                text = entry.Value.ToString("0.###", CultureInfo.InvariantCulture);

            string newText = GUILayout.TextField(text, GUILayout.Width(60f));
            typedText[entry] = newText;

            if (!readOnly && newText != text && TryParse(newText, out float value))
                entry.Value = value;
        }

        // A comma is read as a decimal point, so "0,6" works too
        private static bool TryParse(string text, out float value)
        {
            return float.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static string GetPreviewText(StaffConfig scythe, float multiplier)
        {
            int maxQuality = scythe.maxQuality != null ? Mathf.Max(1, scythe.maxQuality.Value) : 1;
            string first = ApparitionHelper.GetPreviewHit(scythe, multiplier, 1).ToString("0.#", CultureInfo.InvariantCulture);

            if (maxQuality == 1)
                return $"Hardest hit: {first} (before Blood Magic)";

            string last = ApparitionHelper.GetPreviewHit(scythe, multiplier, maxQuality).ToString("0.#", CultureInfo.InvariantCulture);
            return $"Hardest hit: {first} (quality 1) to {last} (quality {maxQuality}), before Blood Magic";
        }
    }
}
