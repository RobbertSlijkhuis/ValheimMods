using TwitchSDK;
using TwitchSDK.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http.Headers;

#region Twitch API Singleton
class UnityTwitch : TwitchSDKApi
{
    UnityPAL PAL;
    public UnityTwitch(string clientId, bool useESProxy) : base(clientId, useESProxy)
    {
    }

    public void InitializeInternally()
    {
        PAL.Start();
    }

    protected override PlatformAbstractionLayer CreatePAL()
    {
        // We need to save this in a variable, so we can call InitializeInternally later.
        return (PAL = new UnityPAL());
    }

    class UnityPAL : ManagedPAL
    {
        TaskCompletionSource<string> FileIOBasePathTCS = new TaskCompletionSource<string>();

        static UnityPAL()
        {
            TaskScheduler.UnobservedTaskException += (a, exc) =>
            {
                if (exc.Exception.InnerException.GetType() == typeof(CoreLibraryException))
                {
                    Debug.LogWarning("Unhandled Twitch Exception: " + exc.Exception.InnerException);
                }
            };
        }

        public void Start()
        {
            FileIOBasePathTCS.SetResult(Application.persistentDataPath);
        }

        protected override Task Log(LogRequest req)
        {
            switch (req.Level)
            {
                case LogLevel.Debug:
                    // don't show
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(req.Message);
                    break;
                case LogLevel.Error:
                    Debug.LogError(req.Message);
                    break;
                default:
                    Debug.Log(req.Message);
                    break;
            }
            return Task.CompletedTask;
        }

        // ManagedPAL.WebRequest Console.WriteLines every request (including the Authorization bearer token) and every
        // response body (including access/refresh tokens), which BepInEx then writes to LogOutput.log. ManagedPAL's
        // HttpClient is private, so this is its implementation copied minus those two log calls. Nothing is logged here.
        private readonly HttpClient m_http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly CancellationTokenSource m_cancel = new CancellationTokenSource();

        protected override void DisposeManaged()
        {
            m_cancel.Cancel();
            lock (m_webSockets)
            {
                foreach (KeyValuePair<int, ClientWebSocket> webSocket in m_webSockets)
                    webSocket.Value.Dispose();
                m_webSockets.Clear();
            }
            base.DisposeManaged();
        }

        protected override async Task<WebRequestResult> WebRequest(WebRequestRequest request)
        {
            HttpRequestMessage message = new HttpRequestMessage { RequestUri = new Uri(request.Uri) };
            bool hasBody = false;
            switch (request.Method)
            {
                case TwitchSDK.Interop.HttpMethod.Get:
                    message.Method = System.Net.Http.HttpMethod.Get;
                    break;
                case TwitchSDK.Interop.HttpMethod.Post:
                    message.Method = System.Net.Http.HttpMethod.Post;
                    hasBody = true;
                    break;
                case TwitchSDK.Interop.HttpMethod.Put:
                    message.Method = System.Net.Http.HttpMethod.Put;
                    hasBody = true;
                    break;
                case TwitchSDK.Interop.HttpMethod.Patch:
                    message.Method = new System.Net.Http.HttpMethod("PATCH");
                    hasBody = true;
                    break;
                case TwitchSDK.Interop.HttpMethod.Delete:
                    message.Method = System.Net.Http.HttpMethod.Delete;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (hasBody)
                message.Content = new StringContent(request.RequestBody, System.Text.Encoding.UTF8, request.ContentType);
            if (!string.IsNullOrEmpty(request.ClientId))
                message.Headers.Add("Client-Id", request.ClientId);
            if (!string.IsNullOrEmpty(request.Authorization))
            {
                message.Headers.Add("Authorization", request.Authorization);
                // Lets chat reuse this login instead of authorizing a second time.
                WizshBoneTwitchIntegration.TwitchIntegration.TwitchTokenCapture.Capture(request.Authorization);
            }
            message.Headers.UserAgent.Add(new ProductInfoHeaderValue(HttpUserAgent, "0.2"));

            using (HttpResponseMessage response = await m_http.SendAsync(message, HttpCompletionOption.ResponseContentRead, m_cancel.Token).ConfigureAwait(false))
            {
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                // Token responses carry the real expires_in, which TwitchAuth uses for its logout countdown.
                WizshBoneTwitchIntegration.TwitchIntegration.TwitchTokenCapture.CaptureResponse(body);
                return new WebRequestResult { HttpStatus = (int)response.StatusCode, ResponseBody = body };
            }
        }

        // Same reason for the websocket: ManagedPAL.RecvWebSocketMessage Console.WriteLines every EventSub message in full
        // (redeemer names, viewer-typed input, ...). Its socket table is private, so all four websocket methods are copied
        // here minus the console output. Nothing is logged here.
        private static readonly System.Text.Encoding Utf8NoBom = new System.Text.UTF8Encoding(false);
        private readonly Dictionary<int, ClientWebSocket> m_webSockets = new Dictionary<int, ClientWebSocket>();
        private int m_nextWebSocketHandle = 1;

        private ClientWebSocket GetWebSocket(int handle)
        {
            lock (m_webSockets)
                return m_webSockets[handle];
        }

        protected override async Task<int> CreateWebSocket(CreateWebSocketRequest req)
        {
            ClientWebSocket webSocket = new ClientWebSocket();
            int handle;
            lock (m_webSockets)
            {
                handle = m_nextWebSocketHandle++;
                m_webSockets[handle] = webSocket;
            }

            await webSocket.ConnectAsync(new Uri(req.Url), m_cancel.Token).ConfigureAwait(false);
            return handle;
        }

        protected override async Task SendWebSocketMessage(SendWebSocketMessageRequest req)
        {
            ClientWebSocket webSocket = GetWebSocket(req.Handle);
            ArraySegment<byte> buffer = new ArraySegment<byte>(Utf8NoBom.GetBytes(req.Message));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, m_cancel.Token).ConfigureAwait(false);
        }

        protected override async Task<string> RecvWebSocketMessage(RecvWebSocketMessageRequest req)
        {
            ClientWebSocket webSocket = GetWebSocket(req.Handle);
            using (MemoryStream stream = new MemoryStream())
            {
                using (CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(req.TimeoutSeconds)))
                using (CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(m_cancel.Token, timeout.Token))
                {
                    WebSocketReceiveResult result;
                    do
                    {
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                        result = await webSocket.ReceiveAsync(buffer, linked.Token).ConfigureAwait(false);

                        if (result.MessageType == WebSocketMessageType.Close)
                            throw new EndOfStreamException("websocket is closed");
                        if (result.MessageType != WebSocketMessageType.Text)
                            throw new InvalidOperationException("not receiving text");

                        stream.Write(buffer.Array, 0, result.Count);
                    }
                    while (!result.EndOfMessage);
                }

                return Utf8NoBom.GetString(stream.ToArray());
            }
        }

        protected override async Task CloseWebSocket(CloseWebSocketRequest req)
        {
            ClientWebSocket webSocket = GetWebSocket(req.Handle);
            lock (m_webSockets)
                m_webSockets.Remove(req.Handle);

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", m_cancel.Token).ConfigureAwait(false);
            webSocket.Dispose();
        }

        protected override Task<string> GetFileIOBasePath(CancellationToken _)
        {
            return FileIOBasePathTCS.Task;
        }

        protected override string HttpUserAgent => "Twitch-Route-66-Unity";
    }
}

public class Twitch : MonoBehaviour
{
    private static object Lock = new object();

    private static Twitch _Twitch;

    public static TwitchSDKApi API
    {
        get
        {
            lock (Lock)
            {
                if (_Twitch != null && _Twitch.Instance != null)
                    return _Twitch.Instance;

                try
                {
                    _Twitch = FindObjectOfType<Twitch>();
                }
                catch (UnityException e) when (e.HResult == -2147467261)
                {
                    throw new Exception("The Twitch API can only be initialized on the main thread. Make sure the first invocation of Twitch.API happens in the Unity Main thread (e.g. the Start or Update method, and not a constructor)");
                }

                if (_Twitch != null && _Twitch.Instance != null)
                    Destroy(_Twitch.gameObject);

                if (_Twitch == null)
                {
                    var singletonObject = new GameObject();
                    _Twitch = singletonObject.AddComponent<Twitch>();
                    _Twitch.CreateInstance();
                    singletonObject.name = "TwitchApi (Singleton)";

                    // Make instance persistent.
                    DontDestroyOnLoad(singletonObject);
                }

                return _Twitch.Instance;
            }
        }
    }
    private TwitchSDKApi Instance;

    public Twitch()
    {
    }

    private void CreateInstance()
    {
        var settings = TwitchSDKSettings.Instance;

        if (settings.ClientId == TwitchSDKSettings.InitialClientId)
        {
            Debug.LogError("Twitch: No OAuth ClientId set. Please open the Twitch settings at Twitch->Edit Settings.");
        }

        Instance = new UnityTwitch(settings.ClientId, settings.UseEventSubProxy);
        ((UnityTwitch)Instance).InitializeInternally();
    }


    private void OnApplicationQuit()
    {
        if (Instance != null)
        {
            Debug.Log("OnApplicationQuit Twitch API");
            Instance.Dispose();
            Instance = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Debug.Log("OnDestroy Twitch API");
            Instance.Dispose();
            Instance = null;
        }
    }
}

#endregion