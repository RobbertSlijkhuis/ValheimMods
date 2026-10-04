using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    /// <summary>
    /// One world's leaderboard stats for one profile - the on-disk shape of
    /// profiles/&lt;name&gt;/leaderboards/&lt;worldname&gt;_&lt;uid&gt;.yaml. Kept out of profile.yaml on purpose:
    /// that file is copied/exported/imported/synced, runtime stats must not be.
    /// </summary>
    internal class LeaderboardFile
    {
        public int version = 1;
        public string worldName = "";

        /// <summary>Keyed by the viewer's Twitch user ID (stable across renames).</summary>
        public Dictionary<string, LeaderboardViewer> viewers = new Dictionary<string, LeaderboardViewer>();
    }

    internal class LeaderboardViewer
    {
        /// <summary>Latest display name, refreshed on every redeem.</summary>
        public string name = "";

        /// <summary>Earlier display names, newest first.</summary>
        public List<string> previousNames = new List<string>();

        public long pointsSpent;
        public int redeemCount;

        /// <summary>
        /// Raw (prefix-free) redeem title to how often this viewer used it. Never pruned against
        /// the profile - a deleted/renamed redeem keeps its old counts. The "favourite" tie-break
        /// ("first entry wins") relies on this Dictionary keeping insertion order through a
        /// YamlDotNet save/load round-trip.
        /// </summary>
        public Dictionary<string, int> redeems = new Dictionary<string, int>();
    }

    /// <summary>One world's stats as handed to the Leaderboards window.</summary>
    internal class LeaderboardWorld
    {
        public long uid;
        public string name;
        public bool isCurrent;
        public LeaderboardFile file;
    }

    /// <summary>One display row: a viewer's stats summed over the selected world(s).</summary>
    internal class LeaderboardRow
    {
        public string id;
        public string name;
        public List<string> previousNames = new List<string>();
        public long points;
        public int redeemCount;
        public string favouriteTitle;
        public int favouriteCount;
    }
}
