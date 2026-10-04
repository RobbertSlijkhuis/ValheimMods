using Jotunn.Entities;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    /// <summary>
    /// Debug aid for the Leaderboards window: real redeems only count with a real Twitch
    /// RedeemerId (WBTIUseRedeem / Redeems-tab test events have none and are skipped on purpose),
    /// so this fills the current world's stats with fake viewers instead.
    /// </summary>
    internal class LeaderboardSeedCommand : ConsoleCommand
    {
        private const int DefaultCount = 12;
        private const int MaxCount = 500;

        public override string Name => "WBTILeaderboardSeed";

        public override string Help =>
            $"Fill this world's leaderboard with fake viewers (default {DefaultCount}, max {MaxCount}); replaces earlier seeded ones. Includes two tied viewers and one renamed viewer.";

        public override void Run(string[] args)
        {
            int count = DefaultCount;

            if (args.Length > 0 && (!int.TryParse(args[0], out count) || count < 1 || count > MaxCount))
            {
                Jotunn.Logger.LogWarning($"[WBTI] Usage: WBTILeaderboardSeed [1-{MaxCount}]");
                return;
            }

            int seeded = LeaderboardHelper.Seed(count);

            if (seeded < 0)
                Jotunn.Logger.LogWarning("[WBTI] Leaderboard seed: no world loaded.");
            else
                Jotunn.Logger.LogWarning($"[WBTI] Leaderboard seed: {seeded} fake viewers added to the current world.");
        }
    }

    internal class LeaderboardClearCommand : ConsoleCommand
    {
        public override string Name => "WBTILeaderboardClear";

        public override string Help => "Remove the seeded fake viewers from this world's leaderboard. Add 'all' to wipe every viewer (real ones too).";

        public override void Run(string[] args)
        {
            bool all = args.Length > 0 && args[0].ToLowerInvariant() == "all";

            int removed = LeaderboardHelper.ClearSeeded(all);

            if (removed < 0)
                Jotunn.Logger.LogWarning("[WBTI] Leaderboard clear: no world loaded.");
            else
                Jotunn.Logger.LogWarning($"[WBTI] Leaderboard clear: removed {removed} viewer(s) from the current world.");
        }
    }
}
