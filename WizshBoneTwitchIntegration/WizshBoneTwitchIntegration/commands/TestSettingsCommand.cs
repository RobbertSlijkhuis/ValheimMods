using System;
using System.Linq;
using Jotunn.Entities;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Commands
{
    /// <summary>
    /// Automated self-test of profile settings persistence (see helpers/ProfileSettingsHelper.cs
    /// and models/data/ProfileSettingsData.cs) - Save/Reload round-trip across every field type,
    /// non-interference with redeem edits, and the live-component push - run entirely against a
    /// disposable "__wbti_test_settings_*" profile created by the test itself. Never reads,
    /// writes, or assumes anything about the player's real profiles - the only real-world value
    /// touched is the active profile's *name*, captured once at the start and restored at the
    /// end. Safe to run repeatedly.
    /// </summary>
    internal class TestSettingsCommand : ConsoleCommand
    {
        public override string Name => "WBTITestSettings";
        public override string Help => "Runs an automated self-test of profile settings persistence (Save/Reload round-trip, redeem-edit non-interference, live component push) against a disposable test profile only. Usage: WBTITestSettings [--keep] - pass --keep to leave the test profile on disk afterward for inspection instead of deleting it.";

        // Own non-overlapping "__wbti_test_settings_" prefix - see TestProfilesCommand's Prefix
        // comment for why every WBTITest* command needs one that isn't a prefix of another's.
        private const string Prefix = "__wbti_test_settings_";
        private const string TestProfile = Prefix + "home";

        private SelfTestReport m_report;

        public override void Run(string[] args)
        {
            m_report = new SelfTestReport("WBTI");

            bool keepProfile = args.Any(a => a.Equals("--keep", StringComparison.OrdinalIgnoreCase));

            string originalActive = ProfileManager.ActiveProfile;
            CleanupLeftoverTestProfiles(originalActive);

            try
            {
                RunScenarios(originalActive, keepProfile);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"[WBTI] Settings self-test threw before finishing: {e}");
            }
            finally
            {
                // Safety net only - the Delete scenario (last in RunScenarios) already restores
                // the active profile and deletes the test profile on a normal pass. This only
                // does anything if an earlier scenario threw before Delete got to run.
                ProfileManager.SelectProfile(originalActive);
                if (!keepProfile && ProfileManager.ProfileExists(TestProfile))
                    ProfileManager.DeleteProfile(TestProfile);
            }

            m_report.LogSummary("Settings");
        }

        private void RunScenarios(string originalActive, bool keepProfile)
        {
            m_report.Check(ProfileManager.CreateProfile(TestProfile, out _), "Setup: create test profile");
            m_report.Check(ProfileManager.SelectProfile(TestProfile), "Setup: test profile becomes active");

            // 1. Defaults - a freshly created profile has no "settings:" block, so Reload() should
            // fall back to ProfileSettingsData's own defaults.
            m_report.Check(
                ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetActiveRedeemPath())?.settings == null,
                "Defaults: fresh profile has no settings block on disk");
            m_report.Check(
                AllFieldsEqual(ProfileSettingsHelper.Current, new ProfileSettingsData()),
                "Defaults: Current matches ProfileSettingsData's own defaults");

            // 2. Save/Reload round-trip across every field type - bool, int, float, string (with
            // YAML-sensitive characters: commas, colons), and enum. SelectProfile forces a real
            // Reload() from disk afterward rather than trusting the in-memory reference.
            var edited = new ProfileSettingsData
            {
                chattingEnabled       = false,
                chattingBlackList     = "Weird: User, Name, With, Commas",
                chattingClaimDuration = 12345,
                chattingCullingRange  = 111.5f,
                chattingInterval      = 22.5f,
                chattingRadius        = 33.5f,
                chattingMaxTalkers    = 9,
                chattingClaimFreeForAll = true,

                creaturesSameFaction  = false,
                creaturesMaxAmount    = 200f,
                creaturesMaxRadius    = 250f,
                creaturesScaling      = false,
                creaturesDamageScale  = 0.42f,
                creaturesHealthScale  = 0.77f,
                creaturesFollowRadius = 44.5f,

                indestructibleBoats      = true,
                indestructibleChests     = true,
                indestructiblePortals    = true,
                indestructibleVegetables = true,

                enableRedeemsOnLogin = false,
                autoResolveRedeems   = false,
                redeemTitlePrefix    = "TEST",

                wardRecipe        = "Wood:99, Stone: 1, Iron:5",
                wardBurnCreatures = true,
                wardPushCreatures = false,
                wardPushForce     = 4321f,

                safezoneTraders = false,
                safezoneBoats   = true,

                hudPosition = HudPosition.Custom,
                hudOffsetX  = -12.5f,
                hudOffsetY  = 34.25f,
            };

            ProfileSettingsHelper.Current = edited;
            ProfileSettingsHelper.Save();

            m_report.Check(ProfileManager.SelectProfile(TestProfile), "Round-trip: reselect forces a fresh Reload from disk");
            ProfileSettingsData reloaded = ProfileSettingsHelper.Current;

            m_report.Check(AllFieldsEqual(edited, reloaded), "Round-trip: every field survives Save/Reload");
            m_report.Check(reloaded.chattingBlackList == edited.chattingBlackList, "Round-trip: string with commas survives");
            m_report.Check(reloaded.wardRecipe == edited.wardRecipe, "Round-trip: string with commas and colons survives");
            m_report.Check(reloaded.hudPosition == edited.hudPosition, "Round-trip: enum survives");

            // 3. Non-interference - adding/deleting a redeem must not clobber the settings block
            // that's already sitting in the same file (RedeemManager.Save reads settings back off
            // disk and passes them straight through unchanged).
            var redeem = new RedeemData { title = "WBTI Settings Test Redeem", points = 1 };
            m_report.Check(RedeemManager.AddRedeem(redeem, out _), "Non-interference: add a redeem");
            ModData afterRedeemAdd = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetActiveRedeemPath());
            m_report.Check(
                afterRedeemAdd?.settings != null && AllFieldsEqual(edited, afterRedeemAdd.settings),
                "Non-interference: settings survive a redeem add untouched");

            RedeemData toDelete = RedeemHelper.redeems.Find(r => r.title == redeem.title);
            if (toDelete != null)
                m_report.Check(RedeemManager.DeleteRedeem(toDelete, out _), "Non-interference: clean up test redeem");

            // 4. Live component push - only meaningful if a game world is actually loaded and
            // TwitchChatting exists yet; best-effort, never fails the run just because no world
            // is loaded (e.g. running this from the main menu console).
            TwitchChatting chatting = Game.instance?.gameObject.GetComponent<TwitchChatting>();
            if (chatting != null)
            {
                m_report.Check(chatting.m_enabled == edited.chattingEnabled, "Live components: TwitchChatting.m_enabled reflects settings");
                m_report.Check(chatting.m_scanRadius == edited.chattingRadius, "Live components: TwitchChatting.m_scanRadius reflects settings");
                m_report.Check(chatting.m_scanInterval == edited.chattingInterval, "Live components: TwitchChatting.m_scanInterval reflects settings");
            }
            else
            {
                Jotunn.Logger.LogWarning("[WBTI] Settings self-test: no TwitchChatting component found (no world loaded?) - skipping live component checks.");
            }

            // 5. Delete - last on purpose: doubles as the run's actual teardown (unless --keep).
            TestDelete(originalActive, keepProfile);
        }

        /// <summary>
        /// Field-by-field equality since ProfileSettingsData has no Equals override - deliberately
        /// explicit rather than reflection-based, matching how the rest of this codebase compares
        /// data objects.
        /// </summary>
        private static bool AllFieldsEqual(ProfileSettingsData a, ProfileSettingsData b)
        {
            return a.chattingEnabled == b.chattingEnabled
                && a.chattingBlackList == b.chattingBlackList
                && a.chattingClaimDuration == b.chattingClaimDuration
                && a.chattingCullingRange == b.chattingCullingRange
                && a.chattingInterval == b.chattingInterval
                && a.chattingRadius == b.chattingRadius
                && a.chattingMaxTalkers == b.chattingMaxTalkers
                && a.chattingClaimFreeForAll == b.chattingClaimFreeForAll
                && a.creaturesSameFaction == b.creaturesSameFaction
                && a.creaturesMaxAmount == b.creaturesMaxAmount
                && a.creaturesMaxRadius == b.creaturesMaxRadius
                && a.creaturesScaling == b.creaturesScaling
                && a.creaturesDamageScale == b.creaturesDamageScale
                && a.creaturesHealthScale == b.creaturesHealthScale
                && a.creaturesFollowRadius == b.creaturesFollowRadius
                && a.indestructibleBoats == b.indestructibleBoats
                && a.indestructibleChests == b.indestructibleChests
                && a.indestructiblePortals == b.indestructiblePortals
                && a.indestructibleVegetables == b.indestructibleVegetables
                && a.enableRedeemsOnLogin == b.enableRedeemsOnLogin
                && a.autoResolveRedeems == b.autoResolveRedeems
                && a.redeemTitlePrefix == b.redeemTitlePrefix
                && a.wardRecipe == b.wardRecipe
                && a.wardBurnCreatures == b.wardBurnCreatures
                && a.wardPushCreatures == b.wardPushCreatures
                && a.wardPushForce == b.wardPushForce
                && a.safezoneTraders == b.safezoneTraders
                && a.safezoneBoats == b.safezoneBoats
                && a.hudPosition == b.hudPosition
                && a.hudOffsetX == b.hudOffsetX
                && a.hudOffsetY == b.hudOffsetY;
        }

        /// <summary>
        /// Restores the real active profile, then - unless <paramref name="keepProfile"/> is set -
        /// deletes the test profile. This *is* the cleanup, not a rehearsal for one.
        /// </summary>
        private void TestDelete(string originalActive, bool keepProfile)
        {
            m_report.Check(ProfileManager.SelectProfile(originalActive), "Delete: restore the real active profile");

            if (keepProfile)
            {
                Jotunn.Logger.LogWarning(
                    "[WBTI] --keep passed: leaving the test profile in place for inspection - delete it by hand (or rerun without --keep) when done.");
                return;
            }

            m_report.Check(ProfileManager.DeleteProfile(TestProfile), "Delete: removes test profile");
        }

        /// <summary>
        /// Best-effort removal of any "__wbti_test_settings_*" profile left behind by a previous
        /// run that crashed before its own cleanup ran. Never touches a real profile.
        /// </summary>
        private static void CleanupLeftoverTestProfiles(string originalActive)
        {
            foreach (string name in ProfileManager.GetProfiles().Where(p => p.StartsWith(Prefix)).ToList())
            {
                if (name == originalActive)
                {
                    Jotunn.Logger.LogWarning(
                        $"[WBTI] Active profile '{name}' looks like leftover test residue from a previous run - leaving it as-is.");
                    continue;
                }

                ProfileManager.DeleteProfile(name);
            }
        }
    }
}
