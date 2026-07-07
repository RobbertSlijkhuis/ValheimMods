using System;
using System.Collections.Generic;
using TwitchSDK.Interop;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Temporary hack: hardcoded redeems that aren't in the YAML redeem list.
    internal static class SpecialRedeemHelper
    {
        private static readonly string[] SomaUsers = { "soma_af", "DeathWizsh" };
        private static readonly string[] SomaTriggerWords = { "hi", "hey", "hello" };
        private const string SomaRedeemTitle = "WBTI: soma_af special";

        private static readonly string[] GraziUsers = { "GraziFM", "DeathWizsh" };
        private static readonly string[] HottubTriggerWords = { "hottub", "hot tub", "hot-tub" };
        private const string GraziRedeemTitle = "WBTI: GraziFM hottub";

        // The "Who is Crys?" reward already exists on Twitch, created outside the mod.
        public static RedeemData TryGetRedeem(string title)
        {
            Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: checking unmatched redeem title: \"{title}\"");

            if (title == "Who is Crys?")
                return BuildCrysRedeem();

            // HandleRedeem re-looks-up the redeem by title after TryGetChatWordRedeem already
            // matched it, so the synthesized chat-word titles must resolve here too.
            if (title == SomaRedeemTitle)
                return BuildSomaRedeem();

            if (title == GraziRedeemTitle)
                return BuildGraziRedeem();

            return null;
        }

        // soma_af saying "hi"/"hey"/"hello" in chat spawns a Deathsquito.
        // GraziFM saying "hottub"/"hot tub"/"hot-tub" in chat spawns a hot tub.
        public static RedeemData TryGetChatWordRedeem(string userName, string message)
        {
            if (userName == null || message == null)
                return null;

            // Each trigger is checked independently (no early return on a non-match) since the
            // test usernames can collide (e.g. "DeathWizsh" is in both lists for local testing) -
            // an early return here would make the second block unreachable in that case.
            if (MatchesAny(userName, SomaUsers))
            {
                Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: checking message from {userName}: \"{message}\"");

                if (ContainsAny(message, SomaTriggerWords))
                {
                    Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: soma trigger word validated in message from {userName}");
                    return BuildSomaRedeem();
                }
            }

            if (MatchesAny(userName, GraziUsers))
            {
                Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: checking message from {userName}: \"{message}\"");

                if (ContainsAny(message, HottubTriggerWords))
                {
                    Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: hottub trigger word validated in message from {userName}");
                    return BuildGraziRedeem();
                }
            }

            return null;
        }

        private static bool MatchesAny(string userName, string[] users)
        {
            foreach (string user in users)
            {
                if (userName.Equals(user, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ContainsAny(string message, string[] words)
        {
            foreach (string word in words)
            {
                if (message.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static RedeemData BuildCrysRedeem()
        {
            return new RedeemData
            {
                title = "Who is Crys?",
                type = RedeemType.SpawnCreature,
                creatureData = new SpawnCreatureData
                {
                    list = new List<CreatureData>
                    {
                        new CreatureData
                        {
                            prefabName = "Goblin",
                            name = "Crys_V, the maniacal Fuling!",
                            rename = true,
                            level = 2,
                            size = 2f,
                            isBoss = true,
                            bossEvent = "boss_goblinking",
                            talkMessage = "Dont die Nik, DONT DIE!;HAHAHAHAHA;I'm coming for you Nik!;I'll gut you like a fizsh!;Me crazy? NO NO NOOOO... who plays with this mod! HAHAHAHA",
                            talkInterval = 5,
                            talks = true,
                        }
                    }
                }
            };
        }

        private static RedeemData BuildSomaRedeem()
        {
            return new RedeemData
            {
                title = SomaRedeemTitle,
                type = RedeemType.SpawnCreature,
                creatureData = new SpawnCreatureData
                {
                    list = new List<CreatureData>
                    {
                        new CreatureData
                        {
                            prefabName = "Deathsquito",
                            name = "Somasquito",
                            rename = true,
                            level = 3,
                            size = 1.5f,
                            color = "#FFBBDF",
                            talkMessage = "I'm gonna sting ya Nik!;Aint I... stunging?;Ready or not! Here I sting!;NikAboutToBeStung!",
                            talkInterval = 5,
                            talks = true,
                        }
                    }
                }
            };
        }

        private static RedeemData BuildGraziRedeem()
        {
            Jotunn.Logger.LogWarning("[WBTI] SpecialRedeemHelper: building hottub redeem for GraziFM trigger");

            return new RedeemData
            {
                title = GraziRedeemTitle,
                type = RedeemType.SpawnCreature,
                creatureData = new SpawnCreatureData
                {
                    list = new List<CreatureData>
                    {
                        new CreatureData
                        {
                            prefabName = "piece_bathtub",
                            position = SpawnPositionType.InFrontOfPlayer,
                            requireMonsterComponents = false,
                            smelterFuelAmount = float.MaxValue,
                            talkMessage = "Come join me in the hot tub Nik! I'm moist :);I like to Nik in the tub!;Is it hot in here? Or is that just Nik!;Oh my, come sit inside me big boi!",
                            talkInterval = 5,
                            talks = true,
                        }
                    }
                }
            };
        }

        // Checks the chat word triggers and, on a match, dispatches it through the normal
        // HandleRedeem pipeline via a synthesized CustomRewardEvent (same pattern as WBTIUseRedeem).
        public static void TryHandleChatWord(TwitchCustomRewards customRewards, string userName, string message)
        {
            RedeemData redeem = TryGetChatWordRedeem(userName, message);
            if (redeem == null || customRewards == null)
                return;

            CustomRewardEvent chatWordEvent = new CustomRewardEvent();
            chatWordEvent.RedemptionId = Guid.Empty.ToString();
            chatWordEvent.RedeemerName = userName;
            chatWordEvent.RedeemedAt = DateTime.Now.ToShortDateString();
            chatWordEvent.CustomRewardTitle = redeem.title;
            chatWordEvent.CustomRewardCost = 0;
            chatWordEvent.Status = CustomRewardRedemptionState.Unfulfilled;

            customRewards.HandleRedeem(chatWordEvent);
        }
    }
}
