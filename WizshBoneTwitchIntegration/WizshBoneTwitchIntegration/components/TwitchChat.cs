using System;
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
        private TwitchCustomRewards m_customRewards;
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;

        private List<TwitchChatMessage> m_chatHistory = new List<TwitchChatMessage>();
        private int m_historyLength = 30;
        private int m_messageRelevanceTimer = 120;

        private string tcpClientSecret = "kqfpjddbg5945on7ip7wuj08faps5m";
        private string tcpClientId = "8i260qk16tmvumfssr2h4klu99frjb";
        private string m_sOAuth;
        private string m_channel;
        private string m_loginMessage = "Welcome, GLHF!";
        public bool m_loggedIn = false;

        public void Update()
        {
            try
            {
                Read();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchChat.Update failed: " + e);
            }
        }

        public void Connect()
        {
            try
            {
                GetOAuth(new string[] { "chat:read", "chat:edit", "channel:bot" });
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

                if (m_sOAuth == null || m_sOAuth == "")
                    return;

                string userName = "WizshBoneBot".ToLower();
                string password = "oauth:" + m_sOAuth;
                m_channel = m_auth.m_userInfo.displayName.ToLower();

                tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
                reader = new StreamReader(tcpClient.GetStream());
                writer = new StreamWriter(tcpClient.GetStream());

                // SAPHONETTE-CLEANUP: only requested so Read() can pull a chatter's numeric user-id
                // out of the message tags for TryHandleToggleCommand's authorization check. Safe to
                // remove (along with the tag stripping in Read()) once that command is gone.
                writer.WriteLine("CAP REQ :twitch.tv/tags");
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

        public void GetOAuth(params string[] scopes)
        {
            new TwitchOAuthGetter(tcpClientId, tcpClientSecret, OnOAuthTokenRecieved, scopes);
        }

        private void OnOAuthTokenRecieved(ApiCodeTokenResponse response)
        {
            m_sOAuth = response.access_token;
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
            if (WizshBoneTwitchIntegration.useRedeemCommand && writer == null) return;

            writer.WriteLine($"PRIVMSG #{m_channel} :{announce} WBTI: {message}");
            writer.Flush();
        }

        private void Read()
        {
            if (tcpClient == null || tcpClient.Available == 0)
                return;

            string message = reader.ReadLine();

            // SAPHONETTE-CLEANUP: tag stripping only exists to recover a chatter's numeric user-id
            // for TryHandleToggleCommand's authorization check - remove along with the CAP REQ in
            // LogIn() once that command is gone. Untagged lines (PING, login notice, CAP ACK) don't
            // start with "@" and fall through unchanged.
            string tags = null;
            if (message.StartsWith("@"))
            {
                int tagEnd = message.IndexOf(' ');
                tags = message.Substring(1, tagEnd - 1);
                message = message.Substring(tagEnd + 1);
            }

            if (message.Contains("PING"))
            {
                writer.WriteLine("PONG :tmi.twitch.tv\r\n");
                writer.Flush();
                return;
            }

            if (message.Contains(m_loginMessage))
            {
                m_loggedIn = true;
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

            // SAPHONETTE-CLEANUP: userId is only resolved for TryHandleToggleCommand's authorization
            // check - remove along with the tag stripping above once that command is gone.
            string userId = GetTagValue(tags, "user-id");

            Jotunn.Logger.LogWarning($"{userName} (id={userId}): {chatMessage}");

            if (m_chatting.GetChosenUser() == userName && chatMessage.Equals("!claim", StringComparison.OrdinalIgnoreCase))
            {
                m_chatting.AcceptClaim();
                return;
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

            if (m_customRewards == null)
                m_customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            // SAPHONETTE-CLEANUP: remove once SpecialRedeemHelper's toggles are no longer needed.
            if (chatMessage.StartsWith("!toggle", StringComparison.OrdinalIgnoreCase))
            {
                SpecialRedeemHelper.TryHandleToggleCommand(this, userId, chatMessage);
                return;
            }

            // SAPHONETTE-CLEANUP: remove once the coffee redeem is no longer needed.
            if (chatMessage.StartsWith("!killcoffee", StringComparison.OrdinalIgnoreCase))
            {
                SpecialRedeemHelper.TryHandleKillCoffeeCommand(this, userId);
                return;
            }

            // SAPHONETTE-CLEANUP: remove this call (and the Helpers using above) once the special
            // redeems in SpecialRedeemHelper are no longer needed.
            SpecialRedeemHelper.TryHandleChatWord(m_customRewards, userName, chatMessage);

            m_chatHistory.Add(new TwitchChatMessage(userName, chatMessage));

            if (m_chatHistory.Count > m_historyLength)
                m_chatHistory.RemoveAt(0);

            m_chatting.onNewMessage.Invoke(m_chatHistory.Last());
        }

        // SAPHONETTE-CLEANUP: only needed to recover a chatter's numeric user-id for
        // TryHandleToggleCommand's authorization check - remove once that command is gone.
        private static string GetTagValue(string tags, string key)
        {
            if (tags == null)
                return null;

            foreach (string pair in tags.Split(';'))
            {
                int eq = pair.IndexOf('=');
                if (eq > 0 && eq == key.Length && string.Compare(pair, 0, key, 0, eq) == 0)
                    return pair.Substring(eq + 1);
            }

            return null;
        }
    }
}
