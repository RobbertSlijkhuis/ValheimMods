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
        public override string Name => "UseTwitchRedeem";

        public override string Help => "Invoke a Twitch redeem via command";

        public override List<string> CommandOptionList()
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            List<string> result = customRewards.GetRedeemList().Select(item => item.title).ToList();
            return result;
        }

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }
            
            // TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            //if (!auth.m_loggedIn)
            //{
            //    Jotunn.Logger.LogWarning("You are currently not logged in to Twitch!");
            //    return;
            //}

            //if (!customRewards.m_enabled)
            //{
            //    Jotunn.Logger.LogWarning("The Twitch redeems are currently not enabled!");
            //    return;
            //}

            string title = "";

            foreach (string arg in args)
            {
                title += arg + " ";
            }

            CustomRewardEvent currentRewardEvent = new CustomRewardEvent();
            currentRewardEvent.RedeemerName = "DevWizsh";
            currentRewardEvent.RedeemedAt = DateTime.Now.ToShortDateString();
            currentRewardEvent.CustomRewardTitle = title.TrimEnd();
            currentRewardEvent.CustomRewardCost = 100;
            currentRewardEvent.Status = CustomRewardRedemptionState.Unfulfilled;
            WizshBoneTwitchIntegration.useRedeemCommand = true;

            customRewards.HandleRedeem(currentRewardEvent);
        }
    }
}
