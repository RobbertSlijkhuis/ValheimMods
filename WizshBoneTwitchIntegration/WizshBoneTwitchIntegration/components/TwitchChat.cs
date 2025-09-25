using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using UnityEngine;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchChat : MonoBehaviour
    {
        private string username = "WizshBoneBot";
        private string password;
        private string channel;

        private TcpClient twitchClient;
        private StreamReader reader;
        private StreamWriter writer;
        public string chatText;

        private string twitchClientSecret = "kqfpjddbg5945on7ip7wuj08faps5m";
        private string twitchClientId = "8i260qk16tmvumfssr2h4klu99frjb";
        private string _sOAuth;


        void Update()
        {
            Read();
        }

        public void Connect()
        {
            GetOAuth(new string[] { "chat:read", "chat:edit", "channel:bot" });
        }

        public void LogIn()
        {
            Jotunn.Logger.LogWarning("Token: " + _sOAuth);

            if (_sOAuth == null || _sOAuth == "")
                return;

            password = "oauth:" + _sOAuth;
            channel = "DeathWizsh";

            twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(twitchClient.GetStream());
            writer = new StreamWriter(twitchClient.GetStream());

            Jotunn.Logger.LogWarning("Password: " + password);
            Jotunn.Logger.LogWarning("Nick: " + username);
            Jotunn.Logger.LogWarning("User name: " + username + " 8 * :" + username);
            Jotunn.Logger.LogWarning("Channel: " + channel);

            writer.WriteLine("PASS " + password);
            writer.WriteLine("NICK " + username.ToLower());
            writer.WriteLine("USER " + username.ToLower() + " 8 * :" + username.ToLower());
            writer.WriteLine("JOIN #" + channel.ToLower());
            writer.Flush();
            Invoke(nameof(SendMessage), 10f);
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

        private void SendMessage()
        {
            writer.WriteLine($"PRIVMSG #{channel} :This message was send from the WizshBone Twitch integration. Hello {channel}!");
            writer.Flush();
        }

        private void Read()
        {
            if (twitchClient != null && twitchClient.Available > 0)
            {
                string message = reader.ReadLine();
                Jotunn.Logger.LogWarning("Message: " + message);

                if (message.Contains("PING"))
                {
                    writer.WriteLine("PONG :tmi.twitch.tv\r\n");
                    writer.Flush();
                    return;
                }

                //if (!message.Contains("PRIVMSG"))
                //    return;

                //int splitPoint = message.IndexOf("!");
                //string author = message.Substring(0, splitPoint);
                //author = author.Substring(1);

                //splitPoint = message.IndexOf("!", 1);
                //string chat = message.Substring(splitPoint + 1);

                //chatText += $"{author}: {chat}\n";
                // Jotunn.Logger.LogWarning($"{author}: {chat}");
            }
        }
    }
}
