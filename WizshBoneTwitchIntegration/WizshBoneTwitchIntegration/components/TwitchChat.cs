using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
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

        private string tcpClientSecret = "kqfpjddbg5945on7ip7wuj08faps5m";
        private string tcpClientId = "8i260qk16tmvumfssr2h4klu99frjb";
        private string m_sOAuth;
        private string m_channel;
        private string m_loginMessage = "Welcome, GLHF!";
        public bool m_loggedIn = false;

        public void Update()
        {
            Read();
        }

        public void Connect()
        {
            GetOAuth(new string[] { "chat:read", "chat:edit", "channel:bot" });
        }

        public void LogIn()
        {
            m_auth = Game.instance.gameObject.GetComponent<TwitchAuth>();

            if (m_sOAuth == null || m_sOAuth == "")
                return;

            string userName = "WizshBoneBot".ToLower();
            string password = "oauth:" + m_sOAuth;
            m_channel = m_auth.displayName.ToLower();

            tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(tcpClient.GetStream());
            writer = new StreamWriter(tcpClient.GetStream());

            writer.WriteLine("PASS " + password);
            writer.WriteLine("NICK " + userName);
            writer.WriteLine("USER " + userName + " 8 * :" + userName);
            writer.WriteLine("JOIN #" + m_channel);
            writer.Flush();
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

        public void Send(string message)
        {
            writer.WriteLine($"PRIVMSG #{m_channel} :WTBI: {message}");
            writer.Flush();
        }

        private void Read()
        {
            if (tcpClient == null || tcpClient.Available == 0)
                return;

            string message = reader.ReadLine();

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

            if (message.Contains("WTBI:"))
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

            Jotunn.Logger.LogWarning($"{userName}: {chatMessage}");

            if (m_chatting.GetChosenUser() == userName && chatMessage.Equals("!claim", StringComparison.OrdinalIgnoreCase))
            {
                m_chatting.AcceptClaim();
                return;
            }

            m_chatHistory.Add(new TwitchChatMessage(userName, chatMessage));

            if (m_chatHistory.Count > m_historyLength)
                m_chatHistory.RemoveAt(0);
        }
    }
}
