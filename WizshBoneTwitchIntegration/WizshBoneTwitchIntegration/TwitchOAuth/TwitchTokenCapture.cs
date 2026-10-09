using System;
using System.Text.RegularExpressions;

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
        private static DateTime? m_expiresAtUtc;

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

        /// <summary>
        /// Real expiry of the captured token (UTC), read from the token response's expires_in. Null until a token
        /// response has been seen this login.
        /// </summary>
        public static DateTime? ExpiresAtUtc
        {
            get { lock (m_lock) return m_expiresAtUtc; }
        }

        /// <summary>
        /// Called with every SDK response body. Only a token response (access_token + expires_in) counts - the
        /// /oauth2/device response also has an expires_in, but that is the device code's 30 minutes.
        /// </summary>
        public static void CaptureResponse(string responseBody)
        {
            if (string.IsNullOrEmpty(responseBody) || !responseBody.Contains("\"access_token\""))
                return;

            Match match = Regex.Match(responseBody, "\"expires_in\"\\s*:\\s*(\\d+)");
            if (!match.Success || !double.TryParse(match.Groups[1].Value, out double seconds))
                return;

            lock (m_lock)
                m_expiresAtUtc = DateTime.UtcNow.AddSeconds(seconds);
        }

        public static void Clear()
        {
            lock (m_lock)
            {
                m_accessToken = null;
                m_expiresAtUtc = null;
            }
        }

        private static string StripPrefix(string value, string prefix)
        {
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? value.Substring(prefix.Length).Trim() : null;
        }
    }
}
