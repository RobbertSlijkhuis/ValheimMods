namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchUserInfo
    {
        public string broadcasterType;
        public string displayName;
        // The lowercase login name - what IRC channels are named after. displayName can differ from it
        // (casing, or non-ASCII characters), so don't build channel names from displayName.
        public string loginName;
    }
}
