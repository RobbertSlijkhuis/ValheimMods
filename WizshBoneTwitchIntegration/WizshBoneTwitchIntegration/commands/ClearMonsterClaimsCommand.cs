using Jotunn.Entities;
using System.Collections.Generic;
using System.Linq;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ClearMonsterClaimsCommand : ConsoleCommand
    {
        public override string Name => "RemoveCreatureClaim";

        public override string Help => "Clears a specific creature claim by user name";

        public override List<string> CommandOptionList()
        {
            TwitchChatting twitchChatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
            List<string> result = twitchChatting.GetAllAssignedUsers().Select(item => item.author).ToList();
            return result;
        }

        public override void Run(string[] args)
        {
            TwitchAuth authComp = Game.instance.gameObject.GetComponent<TwitchAuth>();

            if (!authComp.isLoggedIn)
            {
                Jotunn.Logger.LogWarning("You are currently not logged in to Twitch!");
                return;
            }

            TwitchChatting twitchChatting = Game.instance.gameObject.GetComponent<TwitchChatting>();

            if (args.Length == 1)
            {
                if (!twitchChatting.ContainsAssignedUser(args[0]))
                {
                    Jotunn.Logger.LogWarning("Could not find any creature assignment of user: " + args[0]);
                    return;
                }

                twitchChatting.RemoveAssignedUser(args[0]);
                return;
            }
        }
    }
}
