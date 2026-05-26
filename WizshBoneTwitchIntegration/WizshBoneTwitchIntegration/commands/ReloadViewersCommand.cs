using Jotunn.Entities;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ReloadViewersCommand : ConsoleCommand
    {
        public override string Name => "WBTIReloadViewers";

        public override string Help => "Reloads the viewers config file";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                return;
            }

            RecolorHelper.ReloadViewersConfig();
        }
    }
}
