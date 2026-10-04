using System;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    /// <summary>
    /// Holds the access token the Twitch SDK logged in with, so chat (IRC) can reuse it instead of
    /// asking the user to authorize a second time. The SDK has no getter for its token, but it sends
    /// it as the Authorization header of every request through UnityPAL.WebRequest
    /// (TwitchSDK/TwitchSDK.cs), which feeds <see cref="Capture"/>. Requests run on background
    /// threads, hence the lock.
    /// </summary>
    internal static class TwitchTokenCapture
    {
        // Chat (IRC) scopes - requested by the SDK login in TwitchAuth so the captured token can join chat.
        public const string ChatScopes = "chat:read chat:edit channel:bot";

        private static readonly object m_lock = new object();
        private static string m_accessToken;

        public static string AccessToken
        {
            get { lock (m_lock) return m_accessToken; }
        }

        /// <summary>Called with the raw Authorization header; anything but a Bearer/OAuth token is ignored.</summary>
        public static void Capture(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
                return;

            string token = StripPrefix(authorizationHeader, "Bearer ") ?? StripPrefix(authorizationHeader, "OAuth ");

            if (string.IsNullOrEmpty(token))
                return;

            lock (m_lock)
                m_accessToken = token;
        }

        public static void Clear()
        {
            lock (m_lock)
                m_accessToken = null;
        }

        private static string StripPrefix(string value, string prefix)
        {
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? value.Substring(prefix.Length).Trim() : null;
        }
    }
}
