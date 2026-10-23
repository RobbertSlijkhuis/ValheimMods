using System;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// The access token of the current Twitch login, kept in memory only: it is never written to disk, so a game
    /// restart always starts logged out. Read from the main thread (Helix calls, chat) and from background threads,
    /// hence the lock. Set by <c>TwitchAuth</c> when the login completes, cleared when it ends.
    /// </summary>
    internal static class TwitchSession
    {
        private static readonly object m_lock = new object();
        private static string m_accessToken;
        private static DateTime? m_expiresAtUtc;

        public static string AccessToken
        {
            get { lock (m_lock) return m_accessToken; }
        }

        /// <summary>When Twitch stops accepting the token (UTC), from the <c>expires_in</c> it came with.</summary>
        public static DateTime? ExpiresAtUtc
        {
            get { lock (m_lock) return m_expiresAtUtc; }
        }

        public static void Set(string accessToken, int expiresInSeconds)
        {
            lock (m_lock)
            {
                m_accessToken = accessToken;
                m_expiresAtUtc = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            }
        }

        public static void Clear()
        {
            lock (m_lock)
            {
                m_accessToken = null;
                m_expiresAtUtc = null;
            }
        }
    }
}
