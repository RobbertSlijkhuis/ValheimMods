using System;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchChatMessage
    {
        public string userName;
        public bool hasBeenBroadcasted = false;
        public string message;
        public DateTime timestamp;

        public TwitchChatMessage(string userName, string message)
        {
            this.userName = userName;
            this.message = message;
            this.timestamp = DateTime.Now;
        }
    }
}
