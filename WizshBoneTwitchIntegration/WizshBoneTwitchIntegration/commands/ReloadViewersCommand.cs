using Jotunn.Entities;
using WizshBoneTwitchIntegration.Helpers;
// using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ReloadViewersCommand : ConsoleCommand
    {
        public override string Name => "ReloadViewers";

        public override string Help => "Reloads the viewers config file";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                return;
            }
            
            // TwitchAuth authComp = Game.instance.gameObject.GetComponent<TwitchAuth>();

            //if (!authComp.m_loggedIn)
            //{
            //    Jotunn.Logger.LogWarning("You are currently not logged in to Twitch!");
            //    return;
            //}

            RecolorHelper.ReloadViewersConfig();
        }
    }
}
