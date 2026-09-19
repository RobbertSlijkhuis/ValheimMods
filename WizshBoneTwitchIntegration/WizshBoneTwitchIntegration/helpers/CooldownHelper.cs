using System;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Converts a redeem's cooldown between the unit the wizard shows (seconds/minutes/hours,
    /// <see cref="Models.RedeemData.cooldownUnit"/>) and the seconds it's actually stored and sent
    /// to Twitch as (<see cref="Models.RedeemData.cooldown"/>), and caps it at
    /// <see cref="MaxCooldownSeconds"/>.
    /// </summary>
    internal static class CooldownHelper
    {
        public const string Seconds = "seconds";
        public const string Minutes = "minutes";
        public const string Hours   = "hours";

        public const int MaxCooldownSeconds = 24 * 60 * 60;

        public static readonly string[] Units = { Seconds, Minutes, Hours };

        public static string Label(string unit)
        {
            switch (unit)
            {
                case Minutes: return "Minutes";
                case Hours:   return "Hours";
                default:      return "Seconds";
            }
        }

        public static int SecondsPerUnit(string unit)
        {
            switch (unit)
            {
                case Minutes: return 60;
                case Hours:   return 3600;
                default:      return 1;
            }
        }

        /// <summary><paramref name="value"/> in <paramref name="unit"/>, as seconds, clamped to 0..<see cref="MaxCooldownSeconds"/>.</summary>
        public static int ToSeconds(int value, string unit)
        {
            return ClampSeconds((long)Math.Max(0, value) * SecondsPerUnit(unit));
        }

        public static int ClampSeconds(long seconds)
        {
            return (int)Math.Max(0L, Math.Min(seconds, MaxCooldownSeconds));
        }

        /// <summary>The number to show for <paramref name="seconds"/> in <paramref name="unit"/> (whole units, rounded down).</summary>
        public static int FromSeconds(int seconds, string unit)
        {
            return ClampSeconds(seconds) / SecondsPerUnit(unit);
        }
    }
}
