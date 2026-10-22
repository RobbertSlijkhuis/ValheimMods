using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchChat : MonoBehaviour
    {
        private TwitchAuth m_auth;
        private TwitchChatting m_chatting;
        private const string IrcHost = "irc.chat.twitch.tv";
        private const int IrcTlsPort = 6697;
        private const int MaxLinesPerFrame = 100;
        private const float ReconnectMinDelay = 5f;
        private const float ReconnectMaxDelay = 60f;

        private ChatConnection m_connection;

        // Time (realtimeSinceStartup) of the next attempt after the connection was lost, 0 when none is planned.
        // The wait doubles with every attempt that gets no answer and starts over once Twitch has talked to us.
        private float m_reconnectAt;
        private float m_reconnectDelay = ReconnectMinDelay;

        private List<TwitchChatMessage> m_chatHistory = new List<TwitchChatMessage>();
        private int m_historyLength = 30;
        private int m_messageRelevanceTimer = 120;

        private Coroutine m_connectRoutine;
        private string m_channel;

        public void Update()
        {
            try
            {
                ReconnectIfDue();
                Read();
            }
            catch (Exception e)
            {
                // Drop the connection so a dead socket doesn't log this every frame.
                Jotunn.Logger.LogError("TwitchChat.Update failed, closing chat connection: " + e);
                Disconnect();
            }
        }

        // Game.FixedUpdate calls Game.Logout() directly on a lost connection, which skips the logout paths
        // that call Disconnect(). The connection's threads would otherwise outlive this component.
        public void OnDestroy()
        {
            CloseConnection();
        }

        public void Connect()
        {
            try
            {
                if (m_connectRoutine != null)
                    StopCoroutine(m_connectRoutine);

                m_connectRoutine = StartCoroutine(ConnectWhenTokenCaptured());
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchChat.Connect failed: " + e);
            }
        }

        public void LogIn()
        {
            try
            {
                m_auth = Game.instance.gameObject.GetComponent<TwitchAuth>();

                string accessToken = TwitchTokenCapture.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                    return;

                // Re-logins land here too - close the previous connection instead of leaking it.
                CloseConnection();
                m_reconnectAt = 0f;

                string userName = "WizshBoneBot".ToLower();
                string password = "oauth:" + accessToken;
                string channel = (string.IsNullOrEmpty(m_auth.m_userInfo.loginName) ? m_auth.m_userInfo.displayName : m_auth.m_userInfo.loginName).ToLower();
                m_channel = channel;

                // The token goes over this connection, so it is TLS only. Connecting, the handshake, reading and
                // writing all happen off the main thread: a half-received TLS record or a full send buffer would
                // otherwise block the game.
                ChatConnection connection = new ChatConnection();
                m_connection = connection;

                connection.Start(IrcHost, IrcTlsPort, userName, password, channel);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchChat.LogIn failed: " + e);
            }
        }

        /// <summary>Stops any pending connect and closes the chat connection (logout, expiry).</summary>
        public void Disconnect()
        {
            if (m_connectRoutine != null)
            {
                StopCoroutine(m_connectRoutine);
                m_connectRoutine = null;
            }

            m_reconnectAt = 0f;
            m_reconnectDelay = ReconnectMinDelay;
            CloseConnection();
        }

        // After a lost connection: tries again while the Twitch login is still there. A logout, an expired login or a
        // new login (Disconnect, LogIn) cancels or replaces the plan.
        private void ReconnectIfDue()
        {
            if (m_reconnectAt <= 0f || Time.realtimeSinceStartup < m_reconnectAt)
                return;

            m_reconnectAt = 0f;

            if (m_auth == null || !m_auth.m_loggedIn)
                return;

            LogIn();
        }

        private void CloseConnection()
        {
            m_connection?.Close();
            m_connection = null;
        }

        // Chat reuses the access token of the Twitch SDK login (see TwitchTokenCapture) rather than
        // running its own authorization. The SDK has normally already made an authenticated call by
        // the time Connect() runs, but give it a few seconds in case it hasn't.
        private IEnumerator ConnectWhenTokenCaptured()
        {
            float deadline = Time.realtimeSinceStartup + 10f;

            while (string.IsNullOrEmpty(TwitchTokenCapture.AccessToken) && Time.realtimeSinceStartup < deadline)
                yield return new WaitForSecondsRealtime(0.5f);

            m_connectRoutine = null;

            if (string.IsNullOrEmpty(TwitchTokenCapture.AccessToken))
            {
                Jotunn.Logger.LogWarning("[WBTI] Chat not connected: no Twitch access token was captured from the SDK login.");
                yield break;
            }

            LogIn();
        }

        public List<string> GetUsersInChatHistory()
        {
            List<string> users = new List<string>();

            foreach (TwitchChatMessage message in m_chatHistory)
            {
                TimeSpan timeSpan = DateTime.Now.Subtract(message.timestamp);

                if (message.hasBeenBroadcasted || timeSpan.TotalSeconds > m_messageRelevanceTimer)
                    continue;

                users.Add(message.userName);
            }
           
            return users.Distinct().ToList();
        }

        public TwitchChatMessage GetFirstMessageOfUser(string userName)
        {
            return m_chatHistory.Find(item => item.userName == userName.ToLower());
        }

        public List<TwitchChatMessage> GetAllMessagesOfUser(string userName)
        {
            return m_chatHistory.FindAll(item => item.userName == userName.ToLower());
        }

        public TwitchChatMessage GetLastMessageOfUser(string userName)
        {
            return m_chatHistory.FindLast(item => item.userName == userName.ToLower());
        }

        public void Send(string message, string announce = "")
        {
            // Queued for the connection's writer thread, so a dead socket can neither throw into the redeem
            // handlers that call this nor block them. Held back until the login is done, and dropped when there is
            // no connection.
            m_connection?.Send($"PRIVMSG #{m_channel} :{announce} WBTI: {message}");
        }

        private void Read()
        {
            ChatConnection connection = m_connection;

            if (connection == null)
                return;

            // Read before draining: the reader thread sets Ended only after its last line is queued.
            bool ended = connection.Ended;

            for (int i = 0; i < MaxLinesPerFrame && connection.Lines.TryDequeue(out string line); i++)
            {
                // Twitch is answering, so a later loss starts again with a short wait.
                m_reconnectDelay = ReconnectMinDelay;

                // One odd line must not cost the connection (and the lines queued behind it).
                try
                {
                    HandleLine(connection, line);
                }
                catch (Exception e)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Skipped a chat line that could not be handled: {e.Message}");
                }

                // The line closed the connection.
                if (m_connection != connection)
                    return;
            }

            if (ended && connection.Lines.IsEmpty)
            {
                Jotunn.Logger.LogWarning($"{connection.EndReason ?? "[WBTI] Chat connection was closed by Twitch."} Trying again in {m_reconnectDelay:0} s.");
                CloseConnection();

                m_reconnectAt = Time.realtimeSinceStartup + m_reconnectDelay;
                m_reconnectDelay = Mathf.Min(m_reconnectDelay * 2f, ReconnectMaxDelay);
            }
        }

        private void HandleLine(ChatConnection connection, string message)
        {
            // Control lines are told apart by their command, not by text anywhere in the line: a viewer controls
            // the text of a chat message.
            if (message.StartsWith("PING ", StringComparison.Ordinal))
            {
                connection.Send("PONG :tmi.twitch.tv");
                return;
            }

            // Sent when the token is invalid or lacks the chat scopes (e.g. a login saved before chat reused it).
            // Example: :tmi.twitch.tv NOTICE * :Login authentication failed
            if (message.StartsWith(":tmi.twitch.tv NOTICE", StringComparison.Ordinal) && message.Contains("Login authentication failed"))
            {
                Jotunn.Logger.LogWarning("[WBTI] Twitch rejected the chat login. Log out and back in to Twitch to renew it.");
                CloseConnection();
                return;
            }

            // Example message: :deathwizsh!deathwizsh@deathwizsh.tmi.twitch.tv PRIVMSG #azeriath :Another test :P
            int space = message.IndexOf(' ');

            if (space < 2 || message[0] != ':' || string.CompareOrdinal(message, space + 1, "PRIVMSG ", 0, 8) != 0)
                return;

            if (message.Contains("WBTI:"))
                return;

            int bang = message.IndexOf('!');

            if (bang < 2 || bang > space)
                return;

            string userName = message.Substring(1, bang - 1);

            if (m_chatting == null)
                m_chatting = Game.instance.GetComponent<TwitchChatting>();

            if (m_chatting.GetUserBlacklist().Contains(userName))
                return;

            int textStart = message.IndexOf(" :", space + 1, StringComparison.Ordinal);

            if (textStart < 0)
                return;

            string chatMessage = message.Substring(textStart + 2);

            if (chatMessage.Trim().Equals("!claim", StringComparison.OrdinalIgnoreCase))
            {
                bool freeForAll = ProfileSettingsHelper.Current.chattingClaimFreeForAll;
                bool allowed = freeForAll
                    ? m_chatting.HasOpenClaim()
                    : string.Equals(m_chatting.GetChosenUser(), userName, StringComparison.OrdinalIgnoreCase);

                if (allowed)
                {
                    m_chatting.AcceptClaim(userName);
                    return;
                }
            }

            // Resolved centrally here (once per message) rather than broadcast to every claimed
            // creature's own listener - releasing one claim shifts the live index of the others,
            // so letting each creature independently re-check itself mid-broadcast caused releasing
            // claim N to also sweep up whichever claim shifted into slot N right before its turn.
            if (chatMessage.StartsWith("!unclaim", StringComparison.OrdinalIgnoreCase))
            {
                string target = chatMessage.Length > "!unclaim".Length
                    ? chatMessage.Substring("!unclaim".Length).Trim()
                    : "";

                m_chatting.UnclaimForUser(userName, target);
                return;
            }

            m_chatHistory.Add(new TwitchChatMessage(userName, chatMessage));

            if (m_chatHistory.Count > m_historyLength)
                m_chatHistory.RemoveAt(0);

            m_chatting.onNewMessage.Invoke(m_chatHistory.Last());
        }

        /// <summary>
        /// One IRC-over-TLS connection with a reader and a writer thread of its own (both long-lived and blocking,
        /// so dedicated threads rather than thread-pool work). The main thread only touches the queues: it drains
        /// <see cref="Lines"/> and adds to the outgoing queue through <see cref="Send"/>.
        /// A new login gets a new instance, so a stale reader can never feed lines into the current connection.
        /// </summary>
        private sealed class ChatConnection
        {
            public readonly ConcurrentQueue<string> Lines = new ConcurrentQueue<string>();

            /// <summary>Set after the last line is queued, when the connection failed or Twitch closed it.</summary>
            public volatile bool Ended;

            /// <summary>Why it ended, or null when Twitch simply closed it.</summary>
            public volatile string EndReason;

            private readonly ConcurrentQueue<string> m_outgoing = new ConcurrentQueue<string>();
            private readonly SemaphoreSlim m_outgoingSignal = new SemaphoreSlim(0);
            private readonly object m_lock = new object();
            private TcpClient m_client;
            private SslStream m_stream;
            private volatile bool m_closed;

            public void Start(string host, int port, string userName, string password, string channel)
            {
                new Thread(() => Run(host, port, userName, password, channel)) { IsBackground = true, Name = "WBTI chat reader" }.Start();
            }

            /// <summary>
            /// Queues one raw IRC line. The writer thread only starts once the login lines are sent, so a line queued
            /// while the connection is still being set up goes out right after them. Dropped once the connection is closed.
            /// </summary>
            public void Send(string line)
            {
                if (m_closed)
                    return;

                m_outgoing.Enqueue(line);
                m_outgoingSignal.Release();
            }

            public void Close()
            {
                TcpClient client;
                SslStream stream;

                lock (m_lock)
                {
                    m_closed = true;
                    client = m_client;
                    stream = m_stream;
                }

                // Wakes the writer thread, and disposing the stream also closes the socket and ends the blocked read.
                m_outgoingSignal.Release();

                try { stream?.Dispose(); } catch (Exception) { }
                try { client?.Close(); } catch (Exception) { }
            }

            private void Run(string host, int port, string userName, string password, string channel)
            {
                try
                {
                    TcpClient client = new TcpClient(host, port);
                    SslStream stream = new SslStream(client.GetStream());

                    lock (m_lock)
                    {
                        if (m_closed)
                        {
                            client.Close();
                            return;
                        }

                        m_client = client;
                        m_stream = stream;
                    }

                    // Default certificate validation: the token must only reach the real Twitch server.
                    stream.AuthenticateAsClient(host);

                    StreamReader reader = new StreamReader(stream);
                    StreamWriter writer = new StreamWriter(stream);

                    writer.WriteLine("PASS " + password);
                    writer.WriteLine("NICK " + userName);
                    writer.WriteLine("USER " + userName + " 8 * :" + userName);
                    writer.WriteLine("JOIN #" + channel);
                    writer.Flush();

                    new Thread(() => WriteLoop(writer)) { IsBackground = true, Name = "WBTI chat writer" }.Start();

                    string line;

                    while ((line = reader.ReadLine()) != null)
                        Lines.Enqueue(line);
                }
                catch (Exception e)
                {
                    Fail(e);
                }
                finally
                {
                    Ended = true;
                    Close();
                }
            }

            private void WriteLoop(StreamWriter writer)
            {
                try
                {
                    while (true)
                    {
                        m_outgoingSignal.Wait();

                        if (m_closed)
                            return;

                        while (m_outgoing.TryDequeue(out string line))
                            writer.WriteLine(line);

                        writer.Flush();
                    }
                }
                catch (Exception e)
                {
                    Fail(e);
                }
            }

            /// <summary>Records why the connection ended, then closes it so the other thread ends too.</summary>
            private void Fail(Exception e)
            {
                // Closing on purpose (logout, new login) makes the pending read or write throw - not worth a warning.
                lock (m_lock)
                {
                    if (!m_closed && EndReason == null)
                        EndReason = $"[WBTI] Chat connection failed: {e.Message}";
                }

                Close();
            }
        }
    }
}
