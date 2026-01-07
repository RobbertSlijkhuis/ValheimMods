using System;

namespace WizshBoneTwitchIntegration.Exceptions
{
    [Serializable]
    internal class RedeemException : Exception
    {
        public string type;

        public RedeemException(string message, string type): base(message)
        {
            this.type = type;
        }
    }
}
