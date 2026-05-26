using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ReloadRedeemsCommand : ConsoleCommand
    {
        public override string Name => "WBTIReloadRedeems";

        public override string Help => "Reloads the Twitch redeems from config file";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                return;
            }
            
            TwitchAuth authComp = Game.instance.gameObject.GetComponent<TwitchAuth>();
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (customRewards.ReloadRewards())
            {
                Jotunn.Logger.LogInfo("Twitch redeems reloaded!");
            }
            else
            {
                Jotunn.Logger.LogError("Something went wrong while trying to reload Twitch redeems!");
            }
        }
    }
}
