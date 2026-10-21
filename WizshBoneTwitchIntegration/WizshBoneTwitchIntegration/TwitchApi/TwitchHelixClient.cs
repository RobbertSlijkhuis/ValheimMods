using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// Thin wrapper around the Twitch Helix REST API: adds the bearer token and client id, retries a
    /// rate-limited call once, and turns every failure (no token, network error, error status) into a
    /// <see cref="Response"/> instead of an exception, so callers only check <see cref="Response.IsSuccess"/>.
    /// The token is the one the SDK login holds, see <see cref="TwitchTokenCapture"/>.
    /// </summary>
    internal static class TwitchHelixClient
    {
        private const string BaseUrl = "https://api.twitch.tv/helix/";
        private const int MaxRateLimitWaitSeconds = 10;

        private static readonly HttpClient m_http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public static readonly HttpMethod Patch = new HttpMethod("PATCH");

        public struct Response
        {
            /// <summary>The HTTP status, or 0 when the request never got an answer.</summary>
            public int Status;

            /// <summary>The response body, or the reason when <see cref="Status"/> is 0.</summary>
            public string Body;

            public bool IsSuccess => Status >= 200 && Status < 300;

            public override string ToString() => $"{Status} {Body}";
        }

        /// <summary>
        /// Sends one Helix request. <paramref name="path"/> is relative to /helix/, <paramref name="query"/>
        /// has no leading '?'. Call it on the main thread: the token and client id are read before the first
        /// await, and a caller that awaits the result resumes on the main thread again. A logout wipes the
        /// stored token, so work that has to finish after one (removing the rewards) passes the token it
        /// took beforehand as <paramref name="tokenOverride"/>.
        /// </summary>
        public static async Task<Response> SendAsync(HttpMethod method, string path, string query = null, string jsonBody = null, string tokenOverride = null)
        {
            string token = tokenOverride ?? TwitchTokenCapture.AccessToken;

            if (string.IsNullOrEmpty(token))
                return new Response { Status = 0, Body = "not logged in to Twitch" };

            string clientId = TwitchSDKSettings.Instance.ClientId;
            string url = BaseUrl + path + (string.IsNullOrEmpty(query) ? "" : "?" + query);

            try
            {
                for (int attempt = 0; ; attempt++)
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                    {
                        request.Headers.Add("Authorization", "Bearer " + token);
                        request.Headers.Add("Client-Id", clientId);

                        if (jsonBody != null)
                            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                        using (HttpResponseMessage response = await m_http.SendAsync(request).ConfigureAwait(false))
                        {
                            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            if ((int)response.StatusCode == 429 && attempt == 0)
                            {
                                await Task.Delay(RateLimitWaitMilliseconds(response)).ConfigureAwait(false);
                                continue;
                            }

                            return new Response { Status = (int)response.StatusCode, Body = body };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new Response { Status = 0, Body = e.Message };
            }
        }

        // Ratelimit-Reset is the unix time (seconds) the bucket refills at.
        private static int RateLimitWaitMilliseconds(HttpResponseMessage response)
        {
            int seconds = 1;

            if (response.Headers.TryGetValues("Ratelimit-Reset", out System.Collections.Generic.IEnumerable<string> values))
            {
                foreach (string value in values)
                {
                    if (long.TryParse(value, out long resetAt))
                    {
                        long untilReset = resetAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        seconds = (int)Math.Max(1, Math.Min(untilReset, MaxRateLimitWaitSeconds));
                    }

                    break;
                }
            }

            return seconds * 1000;
        }
    }
}
