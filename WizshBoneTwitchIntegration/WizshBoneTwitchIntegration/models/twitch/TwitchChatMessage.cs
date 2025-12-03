using System;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchChatMessage
    {
        public string author;
        public bool hasBeenBroadcasted = false;
        public string message;
        public DateTime timestamp;

        public TwitchChatMessage(string author, string message)
        {
            this.author = author;
            this.message = message;
            this.timestamp = DateTime.Now;
        }
    }
}
