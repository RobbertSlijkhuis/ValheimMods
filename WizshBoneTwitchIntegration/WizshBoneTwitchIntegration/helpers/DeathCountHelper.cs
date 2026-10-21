using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class DeathCountHelper
    {
        /// <summary>
        /// The playing character's lifetime Deaths stat (the number on vanilla's stats screen).
        /// Read from the raw stat set (index 0), which PlayerProfile.IncrementStat always writes
        /// regardless of cheats. The key only exists after the first death, hence TryGetValue.
        /// </summary>
        public static int GetLocalDeaths()
        {
            PlayerProfile profile = Game.instance?.GetPlayerProfile();

            if (profile == null)
                return 0;

            Dictionary<PlayerStatType, float> stats = profile.m_playerStats[0].m_stats;

            return stats.TryGetValue(PlayerStatType.Deaths, out float deaths) ? (int)deaths : 0;
        }
    }
}
