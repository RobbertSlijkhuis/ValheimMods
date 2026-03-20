namespace WizshBoneTwitchIntegration.Helpers
{
    internal class MessageHelper
    {
        public static string ParseVariables(string variable, string value, string message)
        {
            return message.Replace(variable, value);
        }
    }
}
