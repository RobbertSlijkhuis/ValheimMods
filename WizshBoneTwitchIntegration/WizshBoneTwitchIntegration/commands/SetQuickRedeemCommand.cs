using Jotunn.Entities;
using System.Collections.Generic;
using System.Linq;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class SetQuickRedeemCommand : ConsoleCommand
    {
        public override string Name => "WBTISetQuickRedeem";

        public override string Help => "Set the redeem title fired by the quick-test keybind";

        public override List<string> CommandOptionList()
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            List<string> result = customRewards.GetRedeemList().Where(item => item.enabled).Select(item => item.title).ToList();
            return result;
        }

        public override void Run(string[] args)
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (args.Length == 0)
            {
                customRewards.m_quickTestRedeem = null;
                return;
            }

            string title = "";

            foreach (string arg in args)
            {
                title += arg + " ";
            }

            customRewards.m_quickTestRedeem = title.TrimEnd();
        }
    }
}
