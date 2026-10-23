using System;
using System.Net.Http;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>The one HttpClient every Twitch request goes through (Helix and the OAuth endpoints).</summary>
    internal static class TwitchHttp
    {
        public static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }
}
