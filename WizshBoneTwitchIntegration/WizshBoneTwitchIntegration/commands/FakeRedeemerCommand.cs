using Jotunn.Entities;
using TwitchSDK.Interop;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    /// <summary>
    /// Debug aid: console/Redeems-tab test redeems carry no RedeemerId, so they (on purpose) never
    /// reach the leaderboard or credit a death. While a fake redeemer is set, those test events get
    /// a "seed-" Twitch ID (plus a different broadcaster ID, so they aren't treated as the
    /// streamer's own), which lets the whole chain - leaderboard entry, object tags, death credit -
    /// be tested without live Twitch redeems. "seed-" entries are removed by WBTILeaderboardClear.
    /// </summary>
    internal class FakeRedeemerCommand : ConsoleCommand
    {
        private const string FakeStreamerId = LeaderboardHelper.SeedIdPrefix + "streamer";

        public static string Id { get; private set; }

        public override string Name => "WBTIFakeRedeemer";

        public override string Help => "Give console/Redeems-tab test redeems a fake viewer so leaderboard + death credit can be tested. Usage: WBTIFakeRedeemer <name> (on), WBTIFakeRedeemer off. Clear the data with WBTILeaderboardClear.";

        public override void Run(string[] args)
        {
            if (args.Length == 0 || args[0].ToLowerInvariant() == "off")
            {
                Id = null;
                Jotunn.Logger.LogWarning("[WBTI] Fake redeemer: off. Test redeems are not counted anywhere again.");
                return;
            }

            Id = args[0];
            Jotunn.Logger.LogWarning($"[WBTI] Fake redeemer: test redeems now count as viewer '{Id}' (id {LeaderboardHelper.SeedIdPrefix}{Id}).");
        }

        /// <summary>Stamps a test event with the fake viewer, if one is set.</summary>
        public static void Apply(CustomRewardEvent customRewardEvent)
        {
            if (Id == null || customRewardEvent == null)
                return;

            customRewardEvent.RedeemerId = LeaderboardHelper.SeedIdPrefix + Id;
            customRewardEvent.BroadcasterId = FakeStreamerId;
            customRewardEvent.RedeemerName = Id;
        }
    }
}
