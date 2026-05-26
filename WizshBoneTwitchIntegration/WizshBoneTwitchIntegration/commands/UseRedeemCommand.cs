using Jotunn.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchSDK.Interop;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class UseRedeemCommand : ConsoleCommand
    {
        public override string Name => "WBTIUseRedeem";

        public override string Help => "Invoke a Twitch redeem via command";

        public override List<string> CommandOptionList()
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            List<string> result = customRewards.GetRedeemList().Select(item => item.title).ToList();
            return result;
        }

        public override void Run(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    return;
                }

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                string title = "";

                foreach (string arg in args)
                {
                    title += arg + " ";
                }

                CustomRewardEvent currentRewardEvent = new CustomRewardEvent();
                currentRewardEvent.RedemptionId = Guid.Empty.ToString();
                currentRewardEvent.RedeemerName = "WizshBone";
                currentRewardEvent.RedeemedAt = DateTime.Now.ToShortDateString();
                currentRewardEvent.CustomRewardTitle = title.TrimEnd();
                currentRewardEvent.CustomRewardCost = 100;
                currentRewardEvent.Status = CustomRewardRedemptionState.Unfulfilled;
                WizshBoneTwitchIntegration.useRedeemCommand = true;

                customRewards.HandleRedeem(currentRewardEvent);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError(error);
            }
        }
    }
}
