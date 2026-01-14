using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ListBannedTwitchUsers : ConsoleCommand
    {
        public override string Name => "ListBannedTwitchUsers";

        public override string Help => "Show a list of banned Twitch users";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                return;
            }

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            Jotunn.Logger.LogWarning("Banned Twitch users:");
            foreach (string user in customRewards.m_bannedUsers)
            {
                if (string.IsNullOrEmpty(user))
                    continue;

                Jotunn.Logger.LogInfo($"- {user}");
            }
        }
    }
}
