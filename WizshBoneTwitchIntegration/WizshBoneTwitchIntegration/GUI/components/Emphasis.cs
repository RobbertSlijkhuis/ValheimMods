using Jotunn.Managers;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Highlights words inside UI text (descriptions, tooltips) in Valheim's yellow. Wraps the text
    /// in a rich-text color tag, so it only works on <see cref="UnityEngine.UI.Text"/> with
    /// supportRichText on (Unity's default, and what <see cref="TooltipTrigger"/> sets explicitly).
    /// Usage: <c>$"Capped at {Emphasis.Of("1")} for unstackable items."</c>
    /// </summary>
    internal static class Emphasis
    {
        // Resolved on first use rather than in a static initializer: GUIManager.Instance isn't
        // guaranteed to be ready when this class is first touched.
        private static string s_hex;

        public static string Of(string text)
        {
            if (s_hex == null)
                s_hex = ColorUtility.ToHtmlStringRGB(GUIManager.Instance.ValheimYellow);

            return $"<color=#{s_hex}>{text}</color>";
        }
    }
}
