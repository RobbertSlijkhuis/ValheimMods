using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// Twitch's OAuth endpoints (id.twitch.tv): the device code flow that logs the streamer in, and validating and
    /// revoking the token. The flow needs no client secret and no redirect: the player authorizes on twitch.tv while
    /// the game polls. Every failure (no network, error status) comes back as a result instead of an exception.
    /// Tokens are never logged.
    /// </summary>
    internal static class TwitchOAuthClient
    {
        private const string DeviceUrl = "https://id.twitch.tv/oauth2/device";
        private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
        private const string ValidateUrl = "https://id.twitch.tv/oauth2/validate";
        private const string RevokeUrl = "https://id.twitch.tv/oauth2/revoke";
        private const string DeviceGrantType = "urn:ietf:params:oauth:grant-type:device_code";

        private static readonly HttpClient m_http = TwitchHttp.Client;

        /// <summary>The page the player authorizes on, and how long it and the code behind it stay valid.</summary>
        public sealed class DeviceCode
        {
            public string Code;
            public string VerificationUri;
            public int ExpiresInSeconds;

            /// <summary>How long to wait between two polls.</summary>
            public int IntervalSeconds;
        }

        public enum PollStatus
        {
            /// <summary>The player has not authorized yet: ask again later.</summary>
            Pending,

            /// <summary>Twitch could not be reached, or failed on its side: ask again, but not forever.</summary>
            Unreachable,

            /// <summary>Asking too fast: ask less often.</summary>
            SlowDown,

            /// <summary>Authorized: <see cref="PollResult.AccessToken"/> is set.</summary>
            Granted,

            /// <summary>Denied, expired or otherwise refused: the code is of no use anymore.</summary>
            Failed
        }

        public sealed class PollResult
        {
            public PollStatus Status;
            public string AccessToken;
            public int ExpiresInSeconds;
            public string Message;
        }

        public enum TokenState
        {
            Valid,

            /// <summary>Twitch says the token is no good (expired, revoked, the app was disconnected).</summary>
            Invalid,

            /// <summary>Twitch could not be asked.</summary>
            Unknown
        }

        public sealed class ValidateResult
        {
            public TokenState State;
            public int ExpiresInSeconds;
        }

        /// <summary>Starts a login. null when Twitch could not be reached or refused (the reason is logged).</summary>
        public static async Task<DeviceCode> RequestDeviceCodeAsync(string scopes)
        {
            Reply reply = await PostFormAsync(DeviceUrl, new Dictionary<string, string>
            {
                ["client_id"] = TwitchApp.ClientId,
                ["scopes"] = scopes
            });

            if (!reply.IsSuccess)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not start the Twitch login: {reply.Status} {reply.Message}");
                return null;
            }

            try
            {
                JObject json = JObject.Parse(reply.Body);
                string code = (string)json["device_code"];
                string uri = (string)json["verification_uri"];

                // The page is opened in the player's browser, so only a secure web address is accepted.
                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(uri) || !uri.StartsWith("https://", StringComparison.Ordinal))
                {
                    Jotunn.Logger.LogWarning("[WBTI] Twitch's answer to the login request has no device code or secure page.");
                    return null;
                }

                return new DeviceCode
                {
                    Code = code,
                    VerificationUri = uri,
                    ExpiresInSeconds = (int?)json["expires_in"] ?? 1800,
                    IntervalSeconds = Math.Max(1, (int?)json["interval"] ?? 5)
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read Twitch's answer to the login request: {e.Message}");
                return null;
            }
        }

        /// <summary>Asks whether the player has authorized yet.</summary>
        public static async Task<PollResult> PollAsync(string deviceCode, string scopes)
        {
            Reply reply = await PostFormAsync(TokenUrl, new Dictionary<string, string>
            {
                ["client_id"] = TwitchApp.ClientId,
                ["scopes"] = scopes,
                ["device_code"] = deviceCode,
                ["grant_type"] = DeviceGrantType
            });

            if (reply.IsSuccess)
            {
                try
                {
                    JObject json = JObject.Parse(reply.Body);
                    string token = (string)json["access_token"];

                    if (!string.IsNullOrEmpty(token))
                        return new PollResult { Status = PollStatus.Granted, AccessToken = token, ExpiresInSeconds = (int?)json["expires_in"] ?? 14400 };
                }
                catch (Exception e)
                {
                    return new PollResult { Status = PollStatus.Failed, Message = "unreadable answer: " + e.Message };
                }

                return new PollResult { Status = PollStatus.Failed, Message = "the answer has no access token" };
            }

            // No answer, or Twitch itself failing: the next poll may work.
            if (reply.Status == 0 || reply.Status >= 500)
                return new PollResult { Status = PollStatus.Unreachable, Message = reply.Status == 0 ? reply.Message : $"{reply.Status} {reply.Message}" };

            if (reply.Message != null && reply.Message.Contains("authorization_pending"))
                return new PollResult { Status = PollStatus.Pending };

            if (reply.Message != null && reply.Message.Contains("slow_down"))
                return new PollResult { Status = PollStatus.SlowDown };

            // Rate limited or timed out while the code is still good: ask again, less often.
            if (reply.Status == 429)
                return new PollResult { Status = PollStatus.SlowDown };

            if (reply.Status == 408)
                return new PollResult { Status = PollStatus.Pending };

            return new PollResult { Status = PollStatus.Failed, Message = $"{reply.Status} {reply.Message}" };
        }

        /// <summary>
        /// Asks Twitch whether the token is still good. Twitch wants this when a login starts and then every hour; a
        /// token that stopped being valid for another reason than its time (the player disconnected the app on
        /// twitch.tv) is only found this way.
        /// </summary>
        public static async Task<ValidateResult> ValidateAsync(string accessToken)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ValidateUrl))
                {
                    request.Headers.Add("Authorization", "OAuth " + accessToken);

                    using (HttpResponseMessage response = await m_http.SendAsync(request).ConfigureAwait(false))
                    {
                        if ((int)response.StatusCode == 401)
                            return new ValidateResult { State = TokenState.Invalid };

                        if (!response.IsSuccessStatusCode)
                            return new ValidateResult { State = TokenState.Unknown };

                        JObject json = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                        return new ValidateResult { State = TokenState.Valid, ExpiresInSeconds = (int?)json["expires_in"] ?? 0 };
                    }
                }
            }
            catch (Exception)
            {
                return new ValidateResult { State = TokenState.Unknown };
            }
        }

        /// <summary>Ends the token on Twitch's side. Best effort: a login that is over anyway needs no answer.</summary>
        public static async Task RevokeAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                return;

            Reply reply = await PostFormAsync(RevokeUrl, new Dictionary<string, string>
            {
                ["client_id"] = TwitchApp.ClientId,
                ["token"] = accessToken
            });

            if (!reply.IsSuccess)
                Jotunn.Logger.LogWarning($"[WBTI] Could not revoke the Twitch login: {reply.Status} {reply.Message}");
        }

        private struct Reply
        {
            public int Status;
            public string Body;

            /// <summary>The <c>message</c> of an error answer, or the reason when there was no answer.</summary>
            public string Message;

            public bool IsSuccess => Status >= 200 && Status < 300;
        }

        private static async Task<Reply> PostFormAsync(string url, Dictionary<string, string> form)
        {
            try
            {
                using (FormUrlEncodedContent content = new FormUrlEncodedContent(form))
                using (HttpResponseMessage response = await m_http.PostAsync(url, content).ConfigureAwait(false))
                {
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    string message = null;

                    if (!response.IsSuccessStatusCode)
                    {
                        try { message = (string)JObject.Parse(body)["message"]; }
                        catch (Exception) { message = body; }
                    }

                    return new Reply { Status = (int)response.StatusCode, Body = body, Message = message };
                }
            }
            catch (Exception e)
            {
                return new Reply { Status = 0, Message = e.Message };
            }
        }
    }
}
