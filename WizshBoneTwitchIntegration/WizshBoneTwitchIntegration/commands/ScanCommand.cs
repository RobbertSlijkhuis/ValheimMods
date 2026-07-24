using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ScanCommand : ConsoleCommand
    {
        public override string Name => "WBTIScan";

        public override string Help => "Manually trigger one chatting-scan pass (offers a nearby eligible creature to a random recent chatter), bypassing the login/enabled gate";

        public override void Run(string[] args)
        {
            TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
            chatting.ScanAndAssignUsers(bypassGate: true);
        }
    }
}
