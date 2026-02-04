using Jotunn.Entities;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class SetRedeemAliasCommand : ConsoleCommand
    {
        public override string Name => "SetRedeemAlias";

        public override string Help => "Set an alias which will be used as the redeemer name";

        public override void Run(string[] args)
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (args.Length == 0)
            {
                customRewards.m_alias = null;
                return;
            }

            customRewards.m_alias = args[0];
        }
    }
}
