using Jotunn.Entities;
using System.Collections.Generic;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class UnbanTwitchUserCommand : ConsoleCommand
    {
        public override string Name => "UnbanTwitchUser";

        public override string Help => "Unban a Twitch user";

        public override List<string> CommandOptionList()
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            return customRewards.m_bannedUsers;
        }

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            ExtraConfigHelper.UnbanTwitchUser(args[0]);
        }
    }
}
