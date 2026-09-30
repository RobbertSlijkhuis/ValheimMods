using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    /// <summary>
    /// Twitch Device Code Grant Flow. Needs only the (public) client ID, no client secret.
    /// https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#device-code-grant-flow
    /// </summary>
    internal static class TwitchDeviceFlow
    {
        private const string DeviceUrl = "https://id.twitch.tv/oauth2/device";
        private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
        private const string DeviceGrantType = "urn:ietf:params:oauth:grant-type:device_code";

        [Serializable]
        private class DeviceCodeResponse
        {
            public string device_code;
            public string user_code;
            public string verification_uri;
            public int expires_in;
            public int interval;
        }

        [Serializable]
        private class TwitchErrorResponse
        {
            public int status;
            public string message;
        }

        /// <summary>
        /// Coroutine: requests a device code, opens the browser on the Twitch activation page, then polls
        /// until the user approves. Calls onToken on success; on failure/expiry it only logs.
        /// </summary>
        public static IEnumerator Authorize(string clientId, string[] scopes, Action<ApiCodeTokenResponse> onToken)
        {
            string scopeList = string.Join(" ", scopes);

            WWWForm deviceForm = new WWWForm();
            deviceForm.AddField("client_id", clientId);
            deviceForm.AddField("scopes", scopeList);

            DeviceCodeResponse device = null;
            using (UnityWebRequest request = UnityWebRequest.Post(DeviceUrl, deviceForm))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Jotunn.Logger.LogError($"[WBTI] Twitch device code request failed: {request.error} {request.downloadHandler?.text}");
                    yield break;
                }

                device = JsonUtility.FromJson<DeviceCodeResponse>(request.downloadHandler.text);
            }

            if (device == null || string.IsNullOrEmpty(device.device_code))
            {
                Jotunn.Logger.LogError("[WBTI] Twitch device code response was empty or invalid.");
                yield break;
            }

            Jotunn.Logger.LogWarning($"[WBTI] Twitch chat login: opening {device.verification_uri} (code {device.user_code}, expires in {device.expires_in}s)");
            Application.OpenURL(device.verification_uri);

            float deadline = Time.realtimeSinceStartup + device.expires_in;
            float pollInterval = Mathf.Max(device.interval, 1);

            while (Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForSecondsRealtime(pollInterval);

                WWWForm tokenForm = new WWWForm();
                tokenForm.AddField("client_id", clientId);
                tokenForm.AddField("scopes", scopeList);
                tokenForm.AddField("device_code", device.device_code);
                tokenForm.AddField("grant_type", DeviceGrantType);

                using (UnityWebRequest request = UnityWebRequest.Post(TokenUrl, tokenForm))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        ApiCodeTokenResponse token = JsonUtility.FromJson<ApiCodeTokenResponse>(request.downloadHandler.text);
                        onToken?.Invoke(token);
                        yield break;
                    }

                    // While the user hasn't approved yet Twitch answers 400 {"message":"authorization_pending"}.
                    string body = request.downloadHandler?.text;
                    TwitchErrorResponse error = string.IsNullOrEmpty(body) ? null : JsonUtility.FromJson<TwitchErrorResponse>(body);
                    if (request.result == UnityWebRequest.Result.ProtocolError && error?.message == "authorization_pending")
                        continue;

                    // Transient network errors: keep polling until the code expires.
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Jotunn.Logger.LogWarning($"[WBTI] Twitch token poll connection error, retrying: {request.error}");
                        continue;
                    }

                    Jotunn.Logger.LogError($"[WBTI] Twitch device login failed: {request.error} {body}");
                    yield break;
                }
            }

            Jotunn.Logger.LogError("[WBTI] Twitch device login timed out before it was approved.");
        }
    }
}
