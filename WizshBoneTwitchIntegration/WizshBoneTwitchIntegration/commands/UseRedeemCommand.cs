using Jotunn.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchSDK.Interop;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
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
            List<string> result = customRewards.GetRedeemList().Where(item => item.enabled).Select(item => item.title).ToList();
            return result;
        }

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            string title = "";

            foreach (string arg in args)
            {
                title += arg + " ";
            }

            title = title.TrimEnd();

            FireRedeem(title);
        }

        public static void FireRedeem(string title)
        {
            try
            {
                if (string.IsNullOrEmpty(title))
                {
                    return;
                }

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                // Matched against the raw (prefix-free) RedeemData.title so console usage can
                // stay short (e.g. "WBTIUseRedeem Timestop" rather than the full "WBTI Timestop").
                RedeemData redeem = customRewards.GetRedeemList().Find(item => item.title == title);
                if (redeem != null && !redeem.enabled)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Redeem \"{title}\" is disabled, skipping test invocation.");
                    return;
                }

                CustomRewardEvent currentRewardEvent = new CustomRewardEvent();
                currentRewardEvent.RedemptionId = Guid.Empty.ToString();
                currentRewardEvent.RedeemerName = "WizshBone";
                currentRewardEvent.RedeemedAt = DateTime.Now.ToShortDateString();
                // HandleRedeem looks the redeem up by its Twitch-facing (prefixed) title - fall
                // back to the raw typed title verbatim if no redeem was found, so the "could not
                // find redeem" error downstream still echoes back what was actually typed.
                currentRewardEvent.CustomRewardTitle = redeem != null ? RedeemManager.GetFullTitle(redeem) : title;
                currentRewardEvent.CustomRewardCost = 100;
                currentRewardEvent.Status = CustomRewardRedemptionState.Unfulfilled;
                FakeRedeemerCommand.Apply(currentRewardEvent); // debug: only does anything after WBTIFakeRedeemer
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
