using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ClaimCreatureCommand : ConsoleCommand
    {
        public override string Name => "ClaimCreature";

        public override string Help => "Claim a nearby creature";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                return;
            }

            TwitchChatting chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();

            if (chatting == null)
            {
                Jotunn.Logger.LogWarning("Could not find chatting in claim creature command");
                return;
            }

            chatting.ScanAndAssignUsers(true);
        }
    }
}
