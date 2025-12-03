using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ReloadRedeemsCommand : ConsoleCommand
    {
        public override string Name => "ReloadTwitchRedeems";

        public override string Help => "Reloads the Twitch redeems from config file";

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

            if (!authComp.isEnabledRedeems)
            {
                Jotunn.Logger.LogInfo("The Twitch redeems are currently not enabled!");
                return;
            }

            TwitchCustomRewards rewardsComp = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            if (rewardsComp.ReloadRewards())
            {
                Jotunn.Logger.LogInfo("Twitch redeems reloaded!");
            }
            else
            {
                Jotunn.Logger.LogInfo("Something went wrong while trying to reload Twitch redeems!");
            }
        }
    }
}
