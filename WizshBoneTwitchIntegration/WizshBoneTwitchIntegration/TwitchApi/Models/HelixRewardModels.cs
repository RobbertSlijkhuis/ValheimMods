namespace WizshBoneTwitchIntegration.TwitchApi.Models
{
    // The parts of Helix's "Get Custom Reward" response we read. Field names match the JSON exactly
    // (Newtonsoft maps by name); everything else in the answer is ignored.

    public class HelixRewardList
    {
        public HelixReward[] data;
    }

    public class HelixReward
    {
        public string id;
        public string title;
        public string prompt;
        public int cost;
        public string background_color;
        public bool is_enabled;
        public bool is_user_input_required;
        public HelixMaxPerStreamSetting max_per_stream_setting;
        public HelixMaxPerUserSetting max_per_user_per_stream_setting;
        public HelixCooldownSetting global_cooldown_setting;
    }

    public class HelixMaxPerStreamSetting
    {
        public bool is_enabled;
        public int max_per_stream;
    }

    public class HelixMaxPerUserSetting
    {
        public bool is_enabled;
        public int max_per_user_per_stream;
    }

    public class HelixCooldownSetting
    {
        public bool is_enabled;
        public int global_cooldown_seconds;
    }
}
