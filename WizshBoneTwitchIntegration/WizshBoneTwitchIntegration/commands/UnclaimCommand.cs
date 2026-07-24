using Jotunn.Entities;
using System.Collections.Generic;
using System.Linq;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class UnclaimCommand : ConsoleCommand
    {
        public override string Name => "WBTIUnclaim";

        public override string Help => "Release a viewer's claim(s), as if they typed !unclaim themselves. Usage: WBTIUnclaim <username> [name-or-index]";

        public override List<string> CommandOptionList()
        {
            TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
            return chatting.GetAllCreatureAssignments().Select(a => a.userName).Distinct().ToList();
        }

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                Jotunn.Logger.LogWarning("[WBTI] WBTIUnclaim requires a username. Usage: WBTIUnclaim <username> [name-or-index]");
                return;
            }

            string userName = args[0].ToLower();
            string target = args.Length > 1 ? string.Join(" ", args.Skip(1)) : "";

            TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
            chatting.UnclaimForUser(userName, target);
        }
    }
}
