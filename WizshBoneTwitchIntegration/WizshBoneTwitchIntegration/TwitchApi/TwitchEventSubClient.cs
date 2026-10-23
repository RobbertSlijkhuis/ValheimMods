using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.TwitchApi.Models;

namespace WizshBoneTwitchIntegration.TwitchApi
{
    /// <summary>
    /// Receives the channel point redemptions of the logged-in channel through Twitch's EventSub WebSocket and hands
    /// them out as <see cref="CustomRewardEvent"/>s. The socket work runs off the main thread (one
    /// <see cref="EventSubConnection"/> per socket); everything else - reading the messages, subscribing, the
    /// reconnect timer - happens on the main thread in <see cref="Update"/>, because the Helix client reads the token
    /// and client id there. Same layout as TwitchChat's connection.
    /// </summary>
    internal class TwitchEventSubClient
    {
        private const string RedemptionType = "channel.channel_points_custom_reward_redemption.add";

        // Twitch sends a keepalive at least this often when nothing else happens; a little longer than that of
        // silence means the connection is dead. The same value is asked for in the URL, and a reconnect_url that
        // Twitch supplies carries it along.
        private const float KeepaliveSeconds = 30f;
        private static readonly string Url = "wss://eventsub.wss.twitch.tv/ws?keepalive_timeout_seconds=" + (int)KeepaliveSeconds;
        private const float KeepaliveGraceSeconds = 5f;

        private const float ReconnectMinDelay = 5f;
        private const float ReconnectMaxDelay = 60f;
        private const int MaxMessagesPerFrame = 50;
        private const int SeenMessagesMax = 100;
        private const int SeenRedemptionsMax = 1000;

        private readonly Queue<CustomRewardEvent> m_events = new Queue<CustomRewardEvent>();
        private readonly HashSet<string> m_seenRedemptions = new HashSet<string>();
        private readonly Queue<string> m_seenRedemptionOrder = new Queue<string>();
        private readonly Queue<string> m_seenMessages = new Queue<string>();
        private readonly HashSet<string> m_seenMessageSet = new HashSet<string>();

        private string m_broadcasterId;
        private bool m_running;
        private EventSubConnection m_connection;

        // Opened after a session_reconnect; takes over from m_connection when its welcome arrives.
        private EventSubConnection m_pending;

        private float m_reconnectAt;
        private float m_reconnectDelay = ReconnectMinDelay;

        // Set once a connection has been subscribed this run, so a later subscribe is a comeback and not the first one.
        private bool m_wasSubscribed;

        // When redemptions started to go undelivered: the last message before a connection was lost, or - when it was
        // lost before the first subscription went through - the start of this run. Cleared by RaiseResubscribed.
        private DateTime? m_lostSinceUtc;
        private DateTime m_lastMessageUtc;
        private DateTime m_startedUtc;

        public bool IsRunning => m_running;

        /// <summary>
        /// Counts the logins this client has been started for. Work that outlives a call (the catch-up) takes the
        /// value when it starts and drops its result when it has changed: it belongs to a login that is over, even if
        /// the client is running again by now.
        /// </summary>
        public int Session { get; private set; }

        /// <summary>
        /// Raised on the main thread when the subscription is back after the connection was lost, with the time of the
        /// last message received before it was lost (or the start, when it was lost before the first subscription):
        /// redemptions made since then were not delivered.
        /// </summary>
        public event Action<DateTime> Resubscribed;

        /// <summary>
        /// Raised on the main thread when the client stopped by itself and will not try again - Twitch ended the
        /// subscription, or refused it for the login - with a message for the streamer. A lost connection is not
        /// this: that one is retried.
        /// </summary>
        public event Action<string> Stopped;

        public void Start(string broadcasterId)
        {
            if (m_running)
                return;

            m_broadcasterId = broadcasterId;
            m_running = true;
            m_wasSubscribed = false;
            m_lostSinceUtc = null;
            m_startedUtc = DateTime.UtcNow;
            m_reconnectAt = 0f;
            m_reconnectDelay = ReconnectMinDelay;
            m_events.Clear();
            m_seenRedemptions.Clear();
            m_seenRedemptionOrder.Clear();
            Session++;
            m_seenMessages.Clear();
            m_seenMessageSet.Clear();

            Jotunn.Logger.LogWarning("[WBTI] Connecting to Twitch EventSub.");
            m_connection = Open();
        }

        public void Stop()
        {
            if (!m_running)
                return;

            m_running = false;
            m_connection?.Close();
            m_pending?.Close();
            m_connection = null;
            m_pending = null;
            m_reconnectAt = 0f;
            m_events.Clear();
        }

        /// <summary>The next redemption that arrived, if any.</summary>
        public bool TryDequeue(out CustomRewardEvent redemption)
        {
            if (m_events.Count == 0)
            {
                redemption = null;
                return false;
            }

            redemption = m_events.Dequeue();
            return true;
        }

        /// <summary>
        /// Adds a redemption that did not come through the socket (the catch-up). Ignored when this redemption was
        /// already handed out or is waiting; a redemption is only ever delivered once per login.
        /// </summary>
        public bool Enqueue(CustomRewardEvent redemption)
        {
            if (!string.IsNullOrEmpty(redemption.RedemptionId))
            {
                if (!m_seenRedemptions.Add(redemption.RedemptionId))
                    return false;

                // Only the recent ones matter: a catch-up never reaches back further than the last outage.
                m_seenRedemptionOrder.Enqueue(redemption.RedemptionId);

                if (m_seenRedemptionOrder.Count > SeenRedemptionsMax)
                    m_seenRedemptions.Remove(m_seenRedemptionOrder.Dequeue());
            }

            m_events.Enqueue(redemption);
            return true;
        }

        public void Update()
        {
            if (!m_running)
                return;

            float now = Time.realtimeSinceStartup;

            if (m_connection == null && m_pending == null)
            {
                if (m_reconnectAt > 0f && now >= m_reconnectAt)
                {
                    m_reconnectAt = 0f;
                    m_connection = Open();
                }

                return;
            }

            if (m_pending != null)
                Pump(m_pending);

            if (m_connection != null)
                Pump(m_connection);

            // Silence from a connection (the welcome counts as a message, so this also catches one that never gets one).
            if (m_pending != null && now - m_pending.LastMessageAt > KeepaliveSeconds + KeepaliveGraceSeconds)
                Lost(m_pending, $"no message for {KeepaliveSeconds + KeepaliveGraceSeconds:0} s");

            if (m_connection != null && now - m_connection.LastMessageAt > KeepaliveSeconds + KeepaliveGraceSeconds)
                Lost(m_connection, $"no message for {KeepaliveSeconds + KeepaliveGraceSeconds:0} s");
        }

        private EventSubConnection Open(string url = null)
        {
            EventSubConnection connection = new EventSubConnection { LastMessageAt = Time.realtimeSinceStartup };
            connection.Start(url ?? Url);
            return connection;
        }

        private void Pump(EventSubConnection connection)
        {
            // Read before draining: the receive loop sets Ended only after its last message is queued.
            bool ended = connection.Ended;

            for (int i = 0; i < MaxMessagesPerFrame && connection.Messages.TryDequeue(out string message); i++)
            {
                try
                {
                    Handle(connection, message);
                }
                catch (Exception e)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Skipped an EventSub message that could not be handled: {e.Message}");
                }

                // The message ended this connection (a revocation or a switch to the other one).
                if (!m_running || (connection != m_connection && connection != m_pending))
                    return;
            }

            if (ended && connection.Messages.IsEmpty)
                Lost(connection, connection.EndReason ?? "closed by Twitch");
        }

        private void Handle(EventSubConnection connection, string message)
        {
            JObject json = CustomRewardEventParser.ParseObject(message);
            JToken metadata = json["metadata"];
            string messageId = (string)metadata?["message_id"];
            string type = (string)metadata?["message_type"];

            connection.LastMessageAt = Time.realtimeSinceStartup;
            m_lastMessageUtc = DateTime.UtcNow;

            // Twitch may send a message again, notably around a reconnect.
            if (!string.IsNullOrEmpty(messageId) && !RememberMessage(messageId))
                return;

            switch (type)
            {
                case "session_welcome":
                    OnWelcome(connection, (string)json["payload"]?["session"]?["id"]);
                    break;

                case "session_reconnect":
                    OnReconnectRequested(connection, (string)json["payload"]?["session"]?["reconnect_url"]);
                    break;

                case "notification":
                    if ((string)json["payload"]?["subscription"]?["type"] == RedemptionType && json["payload"]?["event"] is JToken redemption)
                        Enqueue(CustomRewardEventParser.FromJson(redemption));
                    break;

                case "revocation":
                    StopWithReason($"Twitch ended the redemption subscription ({(string)json["payload"]?["subscription"]?["status"]}).");
                    break;

                // session_keepalive only has to reset the silence timer, which every message does.
            }
        }

        private bool RememberMessage(string messageId)
        {
            if (!m_seenMessageSet.Add(messageId))
                return false;

            m_seenMessages.Enqueue(messageId);

            if (m_seenMessages.Count > SeenMessagesMax)
                m_seenMessageSet.Remove(m_seenMessages.Dequeue());

            return true;
        }

        private void OnWelcome(EventSubConnection connection, string sessionId)
        {
            if (connection == m_pending)
            {
                // Twitch moved the subscription to this session along with the connection. What the old connection still
                // has queued was sent before that, so it is read before the old one is closed.
                EventSubConnection old = m_connection;
                DrainBeforeClose(old);

                // A revocation among those messages stopped the client.
                if (!m_running)
                    return;

                m_connection = connection;
                m_pending = null;
                old?.Close();
                Jotunn.Logger.LogWarning("[WBTI] Twitch EventSub moved to a new connection.");

                // The old connection was lost before this one took over: what was made in between was not delivered.
                RaiseResubscribed();
                return;
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                Lost(connection, "the welcome has no session id");
                return;
            }

            _ = SubscribeAsync(connection, sessionId);
        }

        private void DrainBeforeClose(EventSubConnection connection)
        {
            if (connection == null)
                return;

            while (m_running && connection.Messages.TryDequeue(out string message))
            {
                try
                {
                    Handle(connection, message);
                }
                catch (Exception e)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Skipped an EventSub message that could not be handled: {e.Message}");
                }
            }
        }

        private void OnReconnectRequested(EventSubConnection connection, string reconnectUrl)
        {
            // The URL is used exactly as sent. Only the connection in use can be asked to move.
            if (connection != m_connection || m_pending != null || string.IsNullOrEmpty(reconnectUrl))
                return;

            Jotunn.Logger.LogWarning("[WBTI] Twitch asked EventSub to reconnect.");
            m_pending = Open(reconnectUrl);
        }

        // Runs on the main thread: it is started from Update and resumes there after the await. Never throws.
        private async Task SubscribeAsync(EventSubConnection connection, string sessionId)
        {
            try
            {
                JObject body = new JObject
                {
                    ["type"] = RedemptionType,
                    ["version"] = "1",
                    ["condition"] = new JObject { ["broadcaster_user_id"] = m_broadcasterId },
                    ["transport"] = new JObject { ["method"] = "websocket", ["session_id"] = sessionId }
                };

                TwitchHelixClient.Response response = await TwitchHelixClient.SendAsync(HttpMethod.Post, "eventsub/subscriptions", null, body.ToString(Formatting.None));

                // Stopped while the request was out.
                if (!m_running)
                    return;

                // Handed over to a new connection while the request was out: Twitch moved the subscription along with
                // it, so what matters is only that this one went through.
                if (connection != m_connection)
                {
                    if (response.IsSuccess)
                        m_wasSubscribed = true;

                    return;
                }

                if (!response.IsSuccess)
                {
                    // A login Twitch refuses stays refused: retrying every minute would only hide that nothing arrives.
                    if (response.Status == 401 || response.Status == 403)
                        StopWithReason($"Twitch refused the redemption subscription for this login ({response.Status}).");
                    else
                        Lost(connection, $"Twitch refused the subscription: {response}");

                    return;
                }

                m_reconnectDelay = ReconnectMinDelay;
                Jotunn.Logger.LogWarning("[WBTI] Subscribed to Twitch redemptions.");

                // Does nothing for a login that went straight through; after a loss (also one before the first
                // subscription) it hands the owner the time to catch up from.
                RaiseResubscribed();

                m_wasSubscribed = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not subscribe to Twitch redemptions: {e.Message}");

                if (m_running && connection == m_connection)
                    Lost(connection, e.Message);
            }
        }

        private void StopWithReason(string reason)
        {
            Jotunn.Logger.LogWarning($"[WBTI] {reason} Redemptions are not received until you log in to Twitch again.");
            Stop();
            Stopped?.Invoke(reason);
        }

        // Tells the owner that redemptions made since the connection was lost were not delivered, once.
        private void RaiseResubscribed()
        {
            if (!m_lostSinceUtc.HasValue)
                return;

            DateTime since = m_lostSinceUtc.Value;
            m_lostSinceUtc = null;
            Resubscribed?.Invoke(since);
        }

        private void Lost(EventSubConnection connection, string reason)
        {
            if (connection == m_pending)
            {
                // The move failed; the connection in use stays until Twitch closes it - unless that is gone already.
                m_pending.Close();
                m_pending = null;
                Jotunn.Logger.LogWarning($"[WBTI] Twitch EventSub could not move to a new connection: {reason}");

                if (m_connection == null)
                    ScheduleReconnect();

                return;
            }

            if (connection != m_connection)
                return;

            if (!m_lostSinceUtc.HasValue)
                m_lostSinceUtc = m_wasSubscribed ? m_lastMessageUtc : m_startedUtc;

            m_connection.Close();
            m_connection = null;
            Jotunn.Logger.LogWarning($"[WBTI] Twitch EventSub connection lost: {reason}.");

            // A connection that is still opening takes over by itself.
            if (m_pending == null)
                ScheduleReconnect();
        }

        private void ScheduleReconnect()
        {
            Jotunn.Logger.LogWarning($"[WBTI] Twitch EventSub: trying again in {m_reconnectDelay:0} s.");
            m_reconnectAt = Time.realtimeSinceStartup + m_reconnectDelay;
            m_reconnectDelay = Mathf.Min(m_reconnectDelay * 2f, ReconnectMaxDelay);
        }

        /// <summary>
        /// One WebSocket. <see cref="RunAsync"/> executes off the main thread and queues the messages it receives;
        /// the main thread drains <see cref="Messages"/>. Used once: a reconnect gets a new instance.
        /// </summary>
        private sealed class EventSubConnection
        {
            public readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

            /// <summary>Set after the last message is queued, when the socket failed or Twitch closed it.</summary>
            public volatile bool Ended;

            /// <summary>Why it ended, or null when Twitch simply closed it.</summary>
            public volatile string EndReason;

            // Main thread only.
            public float LastMessageAt;

            private readonly CancellationTokenSource m_cancel = new CancellationTokenSource();
            private readonly object m_lock = new object();
            private ClientWebSocket m_socket;

            public void Start(string url)
            {
                Task.Run(() => RunAsync(url));
            }

            public void Close()
            {
                ClientWebSocket socket;

                lock (m_lock)
                    socket = m_socket;

                try { m_cancel.Cancel(); } catch (Exception) { }
                try { socket?.Abort(); } catch (Exception) { }
            }

            private async Task RunAsync(string url)
            {
                ClientWebSocket socket = new ClientWebSocket();

                // Twitch closes a connection whose client sends anything but pongs, and the default sends pings.
                socket.Options.KeepAliveInterval = TimeSpan.Zero;

                try
                {
                    lock (m_lock)
                        m_socket = socket;

                    await socket.ConnectAsync(new Uri(url), m_cancel.Token).ConfigureAwait(false);

                    byte[] buffer = new byte[16 * 1024];

                    while (!m_cancel.IsCancellationRequested)
                    {
                        // A message can arrive in several frames.
                        using (MemoryStream message = new MemoryStream())
                        {
                            WebSocketReceiveResult result;

                            do
                            {
                                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), m_cancel.Token).ConfigureAwait(false);

                                if (result.MessageType == WebSocketMessageType.Close)
                                {
                                    EndReason = $"closed by Twitch (code {(int?)result.CloseStatus})";
                                    return;
                                }

                                message.Write(buffer, 0, result.Count);
                            }
                            while (!result.EndOfMessage);

                            if (result.MessageType == WebSocketMessageType.Text)
                                Messages.Enqueue(Encoding.UTF8.GetString(message.ToArray()));
                        }
                    }
                }
                catch (Exception e)
                {
                    // Closing on purpose (logout, a switch to a new connection) makes the pending call throw.
                    if (!m_cancel.IsCancellationRequested)
                        EndReason = e.Message;
                }
                finally
                {
                    Ended = true;

                    try { socket.Dispose(); } catch (Exception) { }
                }
            }
        }
    }
}
