using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ClearMonsterClaimsCommand : ConsoleCommand
    {
        public override string Name => "ClearMonsterClaims";

        public override string Help => "Clears all current monster claims";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                return;
            }
            
            TwitchAuth authComp = Game.instance.gameObject.GetComponent<TwitchAuth>();

            if (!authComp.isLoggedIn)
            {
                Jotunn.Logger.LogInfo("You are currently not logged in to Twitch!");
                return;
            }

            TwitchChatting twitchChatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
            twitchChatting.ClearAssignedUsers();
        }
    }
}
