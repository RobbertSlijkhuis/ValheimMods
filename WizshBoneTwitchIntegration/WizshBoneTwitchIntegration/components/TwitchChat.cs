using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchChat : MonoBehaviour
    {
        private TwitchAuth m_auth;
        private TwitchChatting m_chatting;
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;

        private List<TwitchChatMessage> m_chatHistory = new List<TwitchChatMessage>();
        private int m_historyLength = 30;
        private int m_messageRelevanceTimer = 120;

        private Coroutine m_connectRoutine;
        private string m_channel;

        public void Update()
        {
            try
            {
                Read();
            }
            catch (Exception e)
            {
                // Drop the connection so a dead socket doesn't log this every frame.
                Jotunn.Logger.LogError("TwitchChat.Update failed, closing chat connection: " + e);
                Disconnect();
            }
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

                string userName = "WizshBoneBot".ToLower();
                string password = "oauth:" + accessToken;
                m_channel = (string.IsNullOrEmpty(m_auth.m_userInfo.loginName) ? m_auth.m_userInfo.displayName : m_auth.m_userInfo.loginName).ToLower();

                tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
                reader = new StreamReader(tcpClient.GetStream());
                writer = new StreamWriter(tcpClient.GetStream());

                writer.WriteLine("PASS " + password);
                writer.WriteLine("NICK " + userName);
                writer.WriteLine("USER " + userName + " 8 * :" + userName);
                writer.WriteLine("JOIN #" + m_channel);
                writer.Flush();
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

            CloseConnection();
        }

        private void CloseConnection()
        {
            // Closing the client closes its stream, so the reader/writer only need nulling out.
            try { tcpClient?.Close(); } catch (Exception) { }

            tcpClient = null;
            reader = null;
            writer = null;
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
            if (writer == null) return;

            // Callers are redeem handlers - a dead socket must not throw into them.
            try
            {
                writer.WriteLine($"PRIVMSG #{m_channel} :{announce} WBTI: {message}");
                writer.Flush();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not send chat message, closing chat connection: {e.Message}");
                CloseConnection();
            }
        }

        private void Read()
        {
            if (tcpClient == null || tcpClient.Available == 0)
                return;

            string message = reader.ReadLine();

            if (message == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] Chat connection was closed by Twitch.");
                CloseConnection();
                return;
            }

            if (message.Contains("PING"))
            {
                writer.WriteLine("PONG :tmi.twitch.tv\r\n");
                writer.Flush();
                return;
            }

            // Sent when the token is invalid or lacks the chat scopes (e.g. a login saved before chat reused it).
            if (message.Contains("Login authentication failed"))
            {
                Jotunn.Logger.LogWarning("[WBTI] Twitch rejected the chat login. Log out and back in to Twitch to renew it.");
                CloseConnection();
                return;
            }

            if (!message.Contains("PRIVMSG"))
                return;

            if (message.Contains("WBTI:"))
                return;

            // Example message: :deathwizsh!deathwizsh@deathwizsh.tmi.twitch.tv PRIVMSG #azeriath :Another test :P
            int splitPoint = message.IndexOf("!");
            string userName = message.Substring(0, splitPoint);
            userName = userName.Substring(1);

            if (m_chatting == null)
                m_chatting = Game.instance.GetComponent<TwitchChatting>();

            if (m_chatting.GetUserBlacklist().Contains(userName))
                return;

            splitPoint = message.IndexOf(":", 1);
            string chatMessage = message.Substring(splitPoint + 1);

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
    }
}
