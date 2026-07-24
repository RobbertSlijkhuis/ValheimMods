using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ClaimCommand : ConsoleCommand
    {
        public override string Name => "WBTIClaim";

        public override string Help => "Immediately claim the nearest eligible creature under the given name (defaults to the current alias). Usage: WBTIClaim [name]";

        public override void Run(string[] args)
        {
            TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
            chatting.ScanAndAssignUsers(bypassGate: true, forceClaim: true, forceClaimUserName: args.Length > 0 ? args[0] : null);
        }
    }
}
