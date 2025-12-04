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
        private TwitchAuth twitchAuth;
        private TwitchChatting twitchChatting;
        private TcpClient twitchClient;
        private StreamReader reader;
        private StreamWriter writer;
        private bool isLoggedIn;
        public List<TwitchChatMessage> chatHistory = new List<TwitchChatMessage>();
        private int historyLength = 30;
        private int messageRelevanceTimer = 120;

        private string twitchClientSecret = "kqfpjddbg5945on7ip7wuj08faps5m";
        private string twitchClientId = "8i260qk16tmvumfssr2h4klu99frjb";
        private string _sOAuth;

        private string username = "WizshBoneBot";
        private string password;
        private string channel;

        private string loginMessage = "Welcome, GLHF!";

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
            twitchAuth = Game.instance.gameObject.GetComponent<TwitchAuth>();

            if (_sOAuth == null || _sOAuth == "")
                return;

            password = "oauth:" + _sOAuth;
            // channel = "DeathWizsh";
            channel = twitchAuth.displayName;

            twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(twitchClient.GetStream());
            writer = new StreamWriter(twitchClient.GetStream());

            Jotunn.Logger.LogWarning("Password: " + password);
            Jotunn.Logger.LogWarning("Nick: " + username);
            Jotunn.Logger.LogWarning("User name: " + username + " 8 * :" + username);
            Jotunn.Logger.LogWarning("Channel: " + channel);

            Jotunn.Logger.LogWarning("Loggin into chat");

            writer.WriteLine("PASS " + password);
            writer.WriteLine("NICK " + username.ToLower());
            writer.WriteLine("USER " + username.ToLower() + " 8 * :" + username.ToLower());
            writer.WriteLine("JOIN #" + channel.ToLower());
            writer.Flush();

            Jotunn.Logger.LogWarning("DONE!");
        }

        public void GetOAuth(params string[] scopes)
        {
            new TwitchOAuthGetter(twitchClientId, twitchClientSecret, OnOAuthTokenRecieved, scopes);
        }

        private void OnOAuthTokenRecieved(ApiCodeTokenResponse response)
        {
            _sOAuth = response.access_token;
            LogIn();
        }

        public List<string> GetAuthorsInChat()
        {
            List<string> authors = new List<string>();
            foreach (TwitchChatMessage message in chatHistory)
            {
                TimeSpan timeSpan = DateTime.Now.Subtract(message.timestamp);

                if (message.hasBeenBroadcasted || timeSpan.TotalSeconds > messageRelevanceTimer)
                    continue;

                authors.Add(message.author);
            }

            foreach (string author in authors.Distinct().ToList())
            {
                Jotunn.Logger.LogWarning(author);
            };
           
            return authors.Distinct().ToList();
        }

        public TwitchChatMessage GetFirstMessageByAuthor(string author)
        {
            return chatHistory.Find(item => item.author == author.ToLower());
        }

        public List<TwitchChatMessage> GetAllMessagesByAuthor(string author)
        {
            return chatHistory.FindAll(item => item.author == author.ToLower());
        }

        public TwitchChatMessage GetLatestMessageByAuthor(string author)
        {
            return chatHistory.FindLast(item => item.author == author.ToLower());
        }

        public void Send(string message)
        {
            writer.WriteLine($"PRIVMSG #{channel} :WTBI: {message}");
            writer.Flush();
        }

        private void Read()
        {
            if (twitchClient != null && twitchClient.Available > 0)
            {
                string message = reader.ReadLine();
                Jotunn.Logger.LogWarning(message);

                if (message.Contains("PING"))
                {
                    writer.WriteLine("PONG :tmi.twitch.tv\r\n");
                    writer.Flush();
                    return;
                }

                if (message.Contains(loginMessage))
                {
                    Jotunn.Logger.LogWarning("Twitch chat login successfull!");
                    isLoggedIn = true;
                    return;
                }

                if (!message.Contains("PRIVMSG"))
                    return;

                // Message: :deathwizsh!deathwizsh@deathwizsh.tmi.twitch.tv PRIVMSG #azeriath :Another test :P
                int splitPoint = message.IndexOf("!");
                string author = message.Substring(0, splitPoint);
                author = author.Substring(1);

                if (twitchChatting == null)
                    twitchChatting = Game.instance.GetComponent<TwitchChatting>();

                if (twitchChatting.GetBlacklist().Contains(author))
                    return;

                splitPoint = message.IndexOf(":", 1);
                string chatMessage = message.Substring(splitPoint + 1);

                chatHistory.Add(new TwitchChatMessage(author, chatMessage));
                // Jotunn.Logger.LogWarning($"{author}|{chatMessage}");

                if (chatHistory.Count > historyLength)
                    chatHistory.RemoveAt(0);
            }
        }
    }
}
