using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn.Entities;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Commands
{
    /// <summary>
    /// Automated self-test of redeem add/update/toggle/copy/delete (see helpers/RedeemManager.cs
    /// and ProfileManager.CopyRedeemToOtherProfile), run entirely against disposable
    /// "__wbti_r_*" profiles created by the test itself. Never reads, writes, or
    /// assumes anything about the player's real profiles - the only real-world value touched is
    /// the active profile's *name*, captured once at the start and restored at the end. Safe to
    /// run repeatedly.
    /// </summary>
    internal class TestRedeemsCommand : ConsoleCommand
    {
        public override string Name => "WBTITestRedeems";
        public override string Help => "Runs an automated self-test of redeem add/update/toggle/copy/delete flows against disposable test profiles only. Usage: WBTITestRedeems [--keep] - pass --keep to leave the test profiles on disk afterward for inspection instead of deleting them.";

        // Own non-overlapping "__wbti_r_" prefix - see TestProfilesCommand's Prefix
        // comment for why every WBTITest* command needs one that isn't a prefix of another's.
        private const string Prefix = "__wbti_r_";
        private const string HomeProfile = Prefix + "home";
        private const string TargetProfile = Prefix + "target";

        private SelfTestReport m_report;

        public override void Run(string[] args)
        {
            m_report = new SelfTestReport("WBTI");
            m_report.LogHeader("Redeem");

            bool keepProfiles = args.Any(a => a.Equals("--keep", StringComparison.OrdinalIgnoreCase));

            string originalActive = ProfileManager.ActiveProfile;
            CleanupLeftoverTestProfiles(originalActive);

            try
            {
                RunScenarios(originalActive, keepProfiles);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"[WBTI] Redeem self-test threw before finishing: {e}");
            }
            finally
            {
                // Safety net only - the Delete scenario (last in RunScenarios) already restores
                // the active profile and deletes both test profiles on a normal pass. This only
                // does anything if an earlier scenario threw before Delete got to run.
                ProfileManager.SelectProfile(originalActive);
                if (!keepProfiles)
                {
                    foreach (string name in new[] { HomeProfile, TargetProfile })
                    {
                        if (ProfileManager.ProfileExists(name))
                            ProfileManager.DeleteProfile(name);
                    }
                }
            }

            m_report.LogSummary("Redeem");
        }

        private void RunScenarios(string originalActive, bool keepProfiles)
        {
            m_report.Check(ProfileManager.CreateProfile(HomeProfile, out _), "Setup: create home test profile");
            m_report.Check(ProfileManager.SelectProfile(HomeProfile), "Setup: home test profile becomes active");

            // Seed one creatureGroup directly - RedeemManager has no API for creatureGroups
            // (that's CreatureGroupsTab's job) - so Add below can verify it survives untouched.
            var seedGroup = new CreatureGroupData { group = "WBTITestGroup" };
            ExtraConfigHelper.WriteRedeemsConfig(
                ProfileManager.GetActiveRedeemPath(),
                null,
                new List<CreatureGroupData> { seedGroup },
                RedeemHelper.redeems);
            RedeemHelper.Reload();

            // 1. Add
            var redeemA = new RedeemData { title = "WBTI Test Redeem A", points = 100 };
            m_report.Check(RedeemManager.AddRedeem(redeemA, out _), "Add: new redeem succeeds");
            m_report.Check(RedeemHelper.redeems.Exists(r => r.title == redeemA.title), "Add: redeem present after reload");

            ModData onDiskAfterAdd = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetActiveRedeemPath());
            m_report.Check(onDiskAfterAdd?.redeems?.Exists(r => r.title == redeemA.title) == true, "Add: redeem persisted to disk");
            m_report.Check(
                onDiskAfterAdd?.creatureGroups?.Exists(g => g.group == "WBTITestGroup") == true,
                "Add: pre-existing creatureGroups survive the write");

            var dupeRedeem = new RedeemData { title = redeemA.title, points = 5 };
            m_report.Check(!RedeemManager.AddRedeem(dupeRedeem, out _), "Add: duplicate title rejected");

            var redeemB = new RedeemData { title = "WBTI Test Redeem B", points = 50 };
            m_report.Check(RedeemManager.AddRedeem(redeemB, out _), "Add: second redeem succeeds");

            // 2. Update
            RedeemData toUpdate = RedeemHelper.redeems.First(r => r.title == redeemA.title);
            RedeemData updated = toUpdate.DeepClone<RedeemData>();
            updated.points = 999;
            m_report.Check(RedeemManager.UpdateRedeem(toUpdate, updated, out _), "Update: field change succeeds");
            m_report.Check(
                RedeemHelper.redeems.Find(r => r.title == redeemA.title)?.points == 999,
                "Update: change persisted and reloaded");

            RedeemData currentA = RedeemHelper.redeems.First(r => r.title == redeemA.title);
            RedeemData collidingRename = currentA.DeepClone<RedeemData>();
            collidingRename.title = redeemB.title;
            m_report.Check(!RedeemManager.UpdateRedeem(currentA, collidingRename, out _), "Update: rename colliding with another title rejected");

            // 3. SetEnabled
            RedeemData toDisable = RedeemHelper.redeems.First(r => r.title == redeemB.title);
            m_report.Check(RedeemManager.SetEnabled(toDisable, false, out _), "SetEnabled: disable succeeds");
            m_report.Check(RedeemHelper.redeems.Find(r => r.title == redeemB.title)?.enabled == false, "SetEnabled: disabled state persisted");

            RedeemData toEnable = RedeemHelper.redeems.First(r => r.title == redeemB.title);
            m_report.Check(RedeemManager.SetEnabled(toEnable, true, out _), "SetEnabled: re-enable succeeds");
            m_report.Check(RedeemHelper.redeems.Find(r => r.title == redeemB.title)?.enabled == true, "SetEnabled: re-enabled state persisted");

            // 4. CopyRedeemWithinProfile (active)
            RedeemData toCopy = RedeemHelper.redeems.First(r => r.title == redeemB.title);
            const string copyTitle = "WBTI Test Redeem B - copy";
            m_report.Check(RedeemManager.CopyRedeemWithinProfile(toCopy, copyTitle, out _), "Copy: within active profile succeeds");
            m_report.Check(RedeemHelper.redeems.Exists(r => r.title == copyTitle), "Copy: copy present after reload");
            m_report.Check(!RedeemManager.CopyRedeemWithinProfile(toCopy, copyTitle, out _), "Copy: duplicate target title rejected");

            // 5. CopyRedeemToOtherProfile (cross-profile) + copy independence
            m_report.Check(ProfileManager.CreateProfile(TargetProfile, out _), "CopyToOtherProfile: create target profile");

            var redeemC = new RedeemData { title = "WBTI Test Redeem C", points = 100 };
            m_report.Check(RedeemManager.AddRedeem(redeemC, out _), "CopyToOtherProfile: create source redeem");

            RedeemData sourceRedeem = RedeemHelper.redeems.First(r => r.title == redeemC.title);
            const string crossCopyTitle = "WBTI Test Redeem C - copy";
            m_report.Check(
                ProfileManager.CopyRedeemToOtherProfile(sourceRedeem, TargetProfile, crossCopyTitle, out _),
                "CopyToOtherProfile: copy into another profile succeeds");

            ModData targetDataAfterCopy = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(TargetProfile));
            RedeemData copiedRedeem = targetDataAfterCopy?.redeems?.Find(r => r.title == crossCopyTitle);
            m_report.Check(
                copiedRedeem != null && copiedRedeem.points == redeemC.points,
                "CopyToOtherProfile: copy appears in target profile with matching data");

            m_report.Check(
                !ProfileManager.CopyRedeemToOtherProfile(sourceRedeem, TargetProfile, crossCopyTitle, out _),
                "CopyToOtherProfile: duplicate title in target profile rejected");

            // Independence: editing the copy in the target profile must NOT affect the original
            // back in the home profile (guards RedeemData.DeepClone actually being deep).
            m_report.Check(ProfileManager.SelectProfile(TargetProfile), "CopyToOtherProfile: switch active to target profile");

            RedeemData copyToEdit = RedeemHelper.redeems.First(r => r.title == crossCopyTitle);
            RedeemData editedCopy = copyToEdit.DeepClone<RedeemData>();
            editedCopy.points = 55555;
            m_report.Check(RedeemManager.UpdateRedeem(copyToEdit, editedCopy, out _), "CopyToOtherProfile: editing the copy succeeds");

            ModData homeDataAfterEdit = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(HomeProfile));
            RedeemData originalStillIntact = homeDataAfterEdit?.redeems?.Find(r => r.title == redeemC.title);
            m_report.Check(
                originalStillIntact != null && originalStillIntact.points == redeemC.points,
                "CopyToOtherProfile: editing the copy does not affect the original");

            m_report.Check(ProfileManager.SelectProfile(HomeProfile), "CopyToOtherProfile: switch active back to home profile");

            // 6. Edits addressed to a non-active profile (home is active, target is not)
            TestNonActiveEdits();

            // 7. Delete - last on purpose: doubles as the run's actual teardown (unless --keep).
            TestDelete(originalActive, keepProfiles, new[] { redeemA.title, redeemB.title, copyTitle, redeemC.title });
        }

        /// <summary>
        /// RedeemManager's profileName parameter with a profile that is NOT active: edits must land
        /// in that profile's file only, leaving the active profile's in-memory list and file alone.
        /// Expects HomeProfile active and TargetProfile (holding at least one redeem) inactive.
        /// </summary>
        private void TestNonActiveEdits()
        {
            string homePath = ProfileManager.GetRedeemPath(HomeProfile);
            string homeFileBefore = File.ReadAllText(homePath);
            int liveCountBefore = RedeemHelper.redeems.Count;

            var redeemD = new RedeemData { title = "WBTI Test Redeem D", points = 10 };
            const string copyTitle = "WBTI Test Redeem D - copy";

            m_report.Check(RedeemManager.AddRedeem(redeemD, out _, TargetProfile), "NonActive Add: succeeds");
            m_report.Check(RedeemManager.GetRedeems(TargetProfile).Exists(r => r.title == redeemD.title), "NonActive Add: persisted to the target profile's file");
            m_report.Check(!RedeemManager.AddRedeem(new RedeemData { title = redeemD.title }, out _, TargetProfile), "NonActive Add: duplicate title rejected");

            RedeemData fromDisk = RedeemManager.GetRedeems(TargetProfile).First(r => r.title == redeemD.title);
            RedeemData updated = fromDisk.DeepClone<RedeemData>();
            updated.points = 77;
            m_report.Check(RedeemManager.UpdateRedeem(fromDisk, updated, out _, TargetProfile), "NonActive Update: field change succeeds");
            m_report.Check(RedeemManager.GetRedeems(TargetProfile).Find(r => r.title == redeemD.title)?.points == 77, "NonActive Update: change persisted");

            RedeemData collidingRename = updated.DeepClone<RedeemData>();
            collidingRename.title = RedeemManager.GetRedeems(TargetProfile).First(r => r.title != redeemD.title).title;
            m_report.Check(!RedeemManager.UpdateRedeem(updated, collidingRename, out _, TargetProfile), "NonActive Update: rename colliding with another title rejected");

            fromDisk = RedeemManager.GetRedeems(TargetProfile).First(r => r.title == redeemD.title);
            m_report.Check(RedeemManager.SetEnabled(fromDisk, false, out _, TargetProfile), "NonActive SetEnabled: disable succeeds");
            m_report.Check(
                RedeemManager.GetRedeems(TargetProfile).Find(r => r.title == redeemD.title)?.enabled == false && !fromDisk.enabled,
                "NonActive SetEnabled: persisted and caller's copy kept in step");

            m_report.Check(RedeemManager.CopyRedeemWithinProfile(fromDisk, copyTitle, out _, TargetProfile), "NonActive Copy: within the target profile succeeds");
            m_report.Check(RedeemManager.GetRedeems(TargetProfile).Exists(r => r.title == copyTitle), "NonActive Copy: copy present in the target profile");
            m_report.Check(!RedeemManager.CopyRedeemWithinProfile(fromDisk, copyTitle, out _, TargetProfile), "NonActive Copy: duplicate title rejected");

            m_report.Check(!RedeemManager.AddRedeem(new RedeemData { title = "x" }, out _, Prefix + "missing"), "NonActive: missing profile rejected");
            m_report.Check(!ProfileManager.ProfileExists(Prefix + "missing"), "NonActive: missing profile is not created");

            // Prefix of a non-active profile comes from its own file, not from the active settings.
            ModData targetData = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(TargetProfile));
            ExtraConfigHelper.WriteRedeemsConfig(
                ProfileManager.GetRedeemPath(TargetProfile),
                new ProfileSettingsData { redeemTitlePrefix = "[T]" },
                targetData.creatureGroups,
                targetData.redeems);
            m_report.Check(RedeemManager.GetFullTitle(redeemD, TargetProfile) == "[T] " + redeemD.title, "NonActive GetFullTitle: uses the target profile's prefix");
            m_report.Check(RedeemManager.GetFullTitle(redeemD) != "[T] " + redeemD.title, "NonActive GetFullTitle: active overload unaffected");

            m_report.Check(RedeemManager.DeleteRedeem(fromDisk, out _, TargetProfile), "NonActive Delete: removes the redeem");
            m_report.Check(RedeemManager.DeleteRedeem(RedeemManager.GetRedeems(TargetProfile).First(r => r.title == copyTitle), out _, TargetProfile), "NonActive Delete: removes the copy");
            m_report.Check(!RedeemManager.DeleteRedeem(fromDisk, out _, TargetProfile), "NonActive Delete: already-removed redeem reported as not found");

            m_report.Check(RedeemHelper.redeems.Count == liveCountBefore, "NonActive: active profile's in-memory list untouched");
            m_report.Check(File.ReadAllText(homePath) == homeFileBefore, "NonActive: active profile's file untouched");
        }

        /// <summary>
        /// Deletes every redeem this run added to the home profile, asserting each deletion
        /// succeeds, then restores the real active profile and - unless
        /// <paramref name="keepProfiles"/> is set - deletes both test profiles. This *is* the
        /// cleanup, not a rehearsal for one.
        /// </summary>
        private void TestDelete(string originalActive, bool keepProfiles, string[] titlesToDelete)
        {
            foreach (string title in titlesToDelete)
            {
                RedeemData redeem = RedeemHelper.redeems.Find(r => r.title == title);
                if (redeem == null)
                    continue; // already gone somehow

                m_report.Check(RedeemManager.DeleteRedeem(redeem, out _), $"Delete: removes '{title}'");
            }
            m_report.Check(
                !RedeemHelper.redeems.Exists(r => titlesToDelete.Contains(r.title)),
                "Delete: none of the test redeems remain");

            m_report.Check(ProfileManager.SelectProfile(originalActive), "Delete: restore the real active profile");

            if (keepProfiles)
            {
                Jotunn.Logger.LogWarning(
                    "[WBTI] --keep passed: leaving the test profiles in place for inspection - delete them by hand (or rerun without --keep) when done.");
                return;
            }

            foreach (string name in new[] { HomeProfile, TargetProfile })
            {
                if (!ProfileManager.ProfileExists(name))
                    continue;

                m_report.Check(ProfileManager.DeleteProfile(name), $"Delete: removes test profile '{name}'");
            }
        }

        /// <summary>
        /// Best-effort removal of any "__wbti_r_*" profile left behind by a previous
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
