using Jotunn.Entities;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class BanTwitchUserCommand : ConsoleCommand
    {
        public override string Name => "BanTwitchUser";

        public override string Help => "Ban a Twitch user from interacting with the mod";

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            ExtraConfigHelper.BanTwitchUser(args[0]);
        }
    }
}
