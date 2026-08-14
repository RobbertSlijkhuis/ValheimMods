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
    /// Automated self-test of every Profiles-tab flow (create/select/copy/export/import/
    /// rename/delete + guard rails), run entirely against disposable "__wbti_test_*" profiles
    /// created by the test itself. Never reads, writes, or assumes anything about the player's
    /// real profiles - the only real-world value touched is the active profile's *name*,
    /// captured once at the start and restored at the end. Safe to run repeatedly.
    /// </summary>
    internal class TestProfilesCommand : ConsoleCommand
    {
        public override string Name => "WBTITestProfiles";
        public override string Help => "Runs an automated self-test of profile create/select/copy/export/import/rename/delete flows against disposable test profiles only. Usage: WBTITestProfiles [--keep] - pass --keep to leave the test profiles on disk afterward for inspection instead of deleting them.";

        // Not just "__wbti_test_" - that would also match (and get cleaned up by) other
        // WBTITest* commands' own "__wbti_test_<subject>_..." profiles, e.g. TestRedeemsCommand's
        // "__wbti_test_redeems_*". Every self-test command should use its own non-overlapping
        // "__wbti_test_<subject>_" prefix so none of them can ever sweep up another's profiles.
        private const string Prefix = "__wbti_test_profiles_";

        private static readonly List<string> m_testProfiles = new List<string>();
        private static SelfTestReport m_report;

        public override void Run(string[] args)
        {
            m_report = new SelfTestReport("WBTI");
            m_testProfiles.Clear();

            bool keepProfiles = args.Any(a => a.Equals("--keep", StringComparison.OrdinalIgnoreCase));

            string originalActive = ProfileManager.ActiveProfile;
            CleanupLeftoverTestProfiles(originalActive);

            try
            {
                RunScenarios(originalActive, keepProfiles);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"[WBTI] Profile self-test threw before finishing: {e}");
            }
            finally
            {
                // Safety net only - the Delete scenario (last in RunScenarios) already restores
                // the active profile and deletes every test profile on a normal pass. This only
                // does anything if an earlier scenario threw before Delete got to run.
                ProfileManager.SelectProfile(originalActive);
                if (!keepProfiles)
                {
                    foreach (string name in m_testProfiles.ToList())
                    {
                        if (ProfileManager.ProfileExists(name))
                            ProfileManager.DeleteProfile(name);
                    }
                }
            }

            m_report.LogSummary("Profile");
        }

        private void RunScenarios(string originalActive, bool keepProfiles)
        {
            // 1. Create
            string createName = NewTestName("create");
            m_report.Check(ProfileManager.CreateProfile(createName, out _), "Create: fresh name succeeds");
            Track(createName);
            m_report.Check(!ProfileManager.CreateProfile(createName, out _), "Create: duplicate name rejected");
            m_report.Check(File.Exists(ProfileManager.GetRedeemPath(createName)), "Create: profile.yaml written");
            m_report.Check(
                ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(createName))?.settings == null,
                "Create: no settings block written (ProfileSettingsHelper falls back to defaults)");
            string invalidCreateName = Prefix + "invalid_" + Path.GetInvalidFileNameChars()[0];
            m_report.Check(!ProfileManager.CreateProfile(invalidCreateName, out _), "Create: invalid name rejected");

            // 2. GetUniqueProfileName
            string uniqueBase = NewTestName("unique");
            string firstSuggestion = ProfileManager.GetUniqueProfileName(uniqueBase);
            m_report.Check(firstSuggestion == $"{uniqueBase} - copy 1", "GetUniqueProfileName: first suggestion is '- copy 1'");
            m_report.Check(ProfileManager.CreateProfile(firstSuggestion, out _), "GetUniqueProfileName: create suggested copy");
            Track(firstSuggestion);
            string secondSuggestion = ProfileManager.GetUniqueProfileName(uniqueBase);
            m_report.Check(secondSuggestion == $"{uniqueBase} - copy 2", "GetUniqueProfileName: second suggestion increments");

            // 3. Select a dedicated test profile as active - every "active profile" scenario
            // below operates on this (or another test profile), never a real one.
            string source = NewTestName("source");
            m_report.Check(ProfileManager.CreateProfile(source, out _), "Select: create source profile");
            Track(source);
            m_report.Check(ProfileManager.SelectProfile(source), "Select: source profile becomes active");
            m_report.Check(ProfileManager.ActiveProfile == source, "Select: ActiveProfile reflects source");

            // Give this profile a distinguishing, non-default setting so later copy/export/
            // import checks can verify settings actually travel with profile.yaml instead of
            // just being structurally present.
            const int SourceClaimDuration = 12345;
            ProfileSettingsHelper.Current.chattingClaimDuration = SourceClaimDuration;
            ProfileSettingsHelper.Save();

            // 4. Copy
            string copyTarget = NewTestName("copy_target");
            m_report.Check(ProfileManager.CopyProfileTo(copyTarget, out _), "Copy: creates new profile");
            Track(copyTarget);
            m_report.Check(
                File.ReadAllText(ProfileManager.GetRedeemPath(copyTarget)) == File.ReadAllText(ProfileManager.GetActiveRedeemPath()),
                "Copy: file (redeems + settings) matches source");
            m_report.Check(!ProfileManager.CopyProfileTo(copyTarget, out _), "Copy: existing target name rejected");

            // 5. Export
            string exportPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.yaml");
            m_report.Check(ProfileManager.ExportProfile(exportPath), "Export: writes to temp path");
            m_report.Check(
                File.Exists(exportPath) && File.ReadAllText(exportPath) == File.ReadAllText(ProfileManager.GetActiveRedeemPath()),
                "Export: content matches active profile");
            m_report.Check(
                ExtraConfigHelper.ReadRedeemsConfig(exportPath)?.settings?.chattingClaimDuration == SourceClaimDuration,
                "Export: settings travel with the file");

            // 6. Import - brand-new profile
            string importNewName = NewTestName("import_new");
            bool importedNew = ProfileManager.ImportProfileTo(exportPath, importNewName, out bool createdNew, out _);
            m_report.Check(importedNew && createdNew, "Import (new): creates a new profile");
            Track(importNewName);
            m_report.Check(
                ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(importNewName))?.settings?.chattingClaimDuration == SourceClaimDuration,
                "Import (new): settings carried over from imported file");

            // 7. Import - overwrite an existing, non-active profile
            string overwriteTarget = NewTestName("import_overwrite");
            m_report.Check(ProfileManager.CreateProfile(overwriteTarget, out _), "Import (overwrite): create target profile");
            Track(overwriteTarget);
            bool importedOver = ProfileManager.ImportProfileTo(exportPath, overwriteTarget, out bool createdOver, out _);
            m_report.Check(importedOver && !createdOver, "Import (overwrite): updates existing profile, not created");
            m_report.Check(
                File.ReadAllText(ProfileManager.GetRedeemPath(overwriteTarget)) == File.ReadAllText(exportPath),
                "Import (overwrite): profile.yaml replaced with imported file");
            m_report.Check(
                ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(overwriteTarget))?.settings?.chattingClaimDuration == SourceClaimDuration,
                "Import (overwrite): settings replaced with imported file's settings");

            // 7b. Import into the currently-active test profile triggers a reload
            bool importedActive = ProfileManager.ImportProfileTo(exportPath, source, out _, out _);
            ModData reloadedActive = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(source));
            m_report.Check(importedActive, "Import (overwrite, active): reports success");
            m_report.Check(
                RedeemHelper.redeems?.Count == (reloadedActive?.redeems?.Count ?? -1),
                "Import (overwrite, active): RedeemHelper reflects the reload");

            // 8. Import into a synced (read-only) profile is rejected
            string syncedName = NewTestName("synced");
            m_report.Check(ProfileManager.CreateProfile(syncedName, out _), "Synced: create profile");
            Track(syncedName);
            ProfileManager.MarkAsSynced(syncedName);
            m_report.Check(!ProfileManager.ImportProfileTo(exportPath, syncedName, out _, out _), "Import: rejected for synced profile");

            // 9. ResolveImportProfileName (pure string logic, no filesystem involved)
            m_report.Check(ProfileManager.ResolveImportProfileName("myprofile (1).yaml") == "myprofile", "ResolveImportProfileName: strips '(1)' suffix");
            m_report.Check(ProfileManager.ResolveImportProfileName("myprofile_redeems.yaml") == "myprofile", "ResolveImportProfileName: strips '_redeems' stem");
            m_report.Check(ProfileManager.ResolveImportProfileName("myprofile_redeems (2).yaml") == "myprofile", "ResolveImportProfileName: strips both");

            // 10. Select/Reload between test profiles
            m_report.Check(ProfileManager.SelectProfile(copyTarget), "Select/Reload: switch to copyTarget");
            ModData copyRedeems = ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(copyTarget));
            m_report.Check(
                RedeemHelper.redeems?.Count == (copyRedeems?.redeems?.Count ?? -1),
                "Select/Reload: RedeemHelper reflects copyTarget");
            m_report.Check(
                ProfileSettingsHelper.Current.chattingClaimDuration == SourceClaimDuration,
                "Select/Reload: ProfileSettingsHelper reflects copyTarget's settings");
            m_report.Check(ProfileManager.SelectProfile(source), "Select/Reload: switch back to source");

            // 11. Legacy redeems.yaml -> profile.yaml filename migration - a profile still on the
            // old filename should have it renamed transparently the first time its path is
            // resolved, no matter which caller triggers the resolve.
            string filenameLegacyName = NewTestName("filename_legacy");
            m_report.Check(ProfileManager.CreateProfile(filenameLegacyName, out _), "Filename migration: create profile");
            Track(filenameLegacyName);
            string newPath = ProfileManager.GetRedeemPath(filenameLegacyName);
            string oldPath = newPath.Replace("profile.yaml", "redeems.yaml");
            File.Move(newPath, oldPath);
            m_report.Check(
                !File.Exists(newPath) && File.Exists(oldPath),
                "Filename migration: simulated legacy redeems.yaml in place");
            string resolvedPath = ProfileManager.GetRedeemPath(filenameLegacyName);
            m_report.Check(
                resolvedPath == newPath && File.Exists(newPath) && !File.Exists(oldPath),
                "Filename migration: GetRedeemPath renames redeems.yaml to profile.yaml on resolve");

            // 12. Legacy settings.yaml migration - a profile from the old two-file scheme
            // (profile.yaml with no "settings:" key + a standalone settings.yaml) should adopt
            // the standalone file's values on first Reload, then fold them into profile.yaml so
            // every later Reload takes the merged-file fast path.
            string legacyName = NewTestName("legacy");
            m_report.Check(ProfileManager.CreateProfile(legacyName, out _), "Legacy: create profile");
            Track(legacyName);
            const float LegacyCullingRange = 999f;
            var legacySettings = new ProfileSettingsData { chattingCullingRange = LegacyCullingRange };
            ExtraConfigHelper.WriteSettingsConfig(ProfileManager.GetSettingsPath(legacyName), legacySettings);
            m_report.Check(
                ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(legacyName))?.settings == null,
                "Legacy: profile.yaml has no settings block yet");
            m_report.Check(ProfileManager.SelectProfile(legacyName), "Legacy: select migrates on Reload");
            m_report.Check(
                ProfileSettingsHelper.Current.chattingCullingRange == LegacyCullingRange,
                "Legacy: adopted values from standalone settings.yaml");
            m_report.Check(
                ExtraConfigHelper.ReadRedeemsConfig(ProfileManager.GetRedeemPath(legacyName))?.settings?.chattingCullingRange == LegacyCullingRange,
                "Legacy: values folded into profile.yaml after migration");
            m_report.Check(ProfileManager.SelectProfile(source), "Legacy: switch back to source");

            // 13. Rename
            string renameTarget = NewTestName("rename_target");
            m_report.Check(ProfileManager.CreateProfile(renameTarget, out _), "Rename: create target profile");
            Track(renameTarget);
            string renameTargetNew = renameTarget + "_renamed";
            m_report.Check(ProfileManager.RenameProfile(renameTarget, renameTargetNew, out _), "Rename: non-active rename succeeds");
            Replace(renameTarget, renameTargetNew);
            m_report.Check(
                !ProfileManager.ProfileExists(renameTarget) && ProfileManager.ProfileExists(renameTargetNew),
                "Rename: folder actually renamed on disk");

            m_report.Check(!ProfileManager.RenameProfile(renameTargetNew, copyTarget, out _), "Rename: colliding name rejected");
            m_report.Check(!ProfileManager.RenameProfile(syncedName, syncedName + "_x", out _), "Rename: synced profile rejected");

            string sourceRenamed = source + "_renamed";
            m_report.Check(ProfileManager.RenameProfile(source, sourceRenamed, out _), "Rename: active profile rename succeeds");
            m_report.Check(ProfileManager.ActiveProfile == sourceRenamed, "Rename: ActiveProfile updated after renaming the active profile");
            Replace(source, sourceRenamed);
            source = sourceRenamed;

            // 14. Delete - last on purpose: doubles as the run's actual teardown (unless --keep).
            TestDelete(originalActive, keepProfiles);
        }

        /// <summary>
        /// Asserts the "can't delete the active profile" guard rail once, restores the real
        /// active profile, then - unless <paramref name="keepProfiles"/> is set - deletes every
        /// test profile created during the run. This *is* the cleanup, not a rehearsal for one.
        /// </summary>
        private void TestDelete(string originalActive, bool keepProfiles)
        {
            string stillActive = ProfileManager.ActiveProfile;
            m_report.Check(!ProfileManager.DeleteProfile(stillActive), "Delete: cannot delete the active profile");

            m_report.Check(ProfileManager.SelectProfile(originalActive), "Delete: restore the real active profile");

            if (keepProfiles)
            {
                Jotunn.Logger.LogWarning(
                    $"[WBTI] --keep passed: leaving {m_testProfiles.Count} test profile(s) in place for inspection - delete them by hand (or rerun without --keep) when done.");
                return;
            }

            foreach (string name in m_testProfiles.ToList())
            {
                if (!ProfileManager.ProfileExists(name))
                    continue; // already gone (e.g. renamed away earlier)

                m_report.Check(ProfileManager.DeleteProfile(name), $"Delete: removes test profile '{name}'");
            }
        }

        private static string NewTestName(string suffix) => Prefix + suffix;

        private static void Track(string profileName) => m_testProfiles.Add(profileName);

        private static void Replace(string oldName, string newName)
        {
            int index = m_testProfiles.IndexOf(oldName);
            if (index >= 0)
                m_testProfiles[index] = newName;
            else
                m_testProfiles.Add(newName);
        }

        /// <summary>
        /// Best-effort removal of any "__wbti_test_*" profile left behind by a previous run that
        /// crashed before its own cleanup ran. Never touches a real profile.
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
