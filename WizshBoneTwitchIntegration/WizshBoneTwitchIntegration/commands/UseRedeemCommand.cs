using Jotunn.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using WizshBoneTwitchIntegration.TwitchApi.Models;
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
                RedeemData redeem = customRewards.GetRedeemList().Find(item => RedeemManager.SameTitle(item.title, title));

                if (redeem == null)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Redeem \"{title}\" was not found, skipping test invocation.");
                    return;
                }

                if (!redeem.enabled)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Redeem \"{title}\" is disabled, skipping test invocation.");
                    return;
                }

                CustomRewardEvent currentRewardEvent = TestRedeemEventHelper.Create(redeem, 100);
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
