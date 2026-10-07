using System.Collections.Generic;
using System.IO;
using WizshBoneTwitchIntegration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Upgrades a profile.yaml written by an older version of the mod to the current on-disk
    /// format. Each profile.yaml carries a top-level "version:" (<see cref="ModData.version"/>);
    /// a file without one reads as 0, i.e. legacy. Migration runs inside
    /// <see cref="ExtraConfigHelper.ReadRedeemsConfig(string)"/> (every load/import/sync path funnels
    /// through it), so in-memory data is always current - and <see cref="ExtraConfigHelper.WriteRedeemsConfig"/>
    /// stamps <see cref="CurrentVersion"/> on every save, which persists the upgrade. Profiles that
    /// never get opened are upgraded on disk at startup by <see cref="MigrateAllProfilesOnDisk"/>.
    ///
    /// To add a future format change: bump <see cref="CurrentVersion"/> and add an
    /// "if (data.version &lt; N)" step to <see cref="Migrate"/> - and also update the
    /// "version:" line in the embedded resources/profile.yaml seed, since
    /// <see cref="ExtraConfigHelper.WriteDefaultRedeemsTo"/> copies it verbatim for brand-new
    /// profiles rather than going through <see cref="ExtraConfigHelper.WriteRedeemsConfig"/>;
    /// forgetting this makes every new profile look legacy and get needlessly migrated on
    /// first read.
    /// </summary>
    internal static class ProfileMigrationHelper
    {
        public const int CurrentVersion = 2;

        // Only the keys the migration needs from the raw file - ProfileSettingsData no longer has
        // allowRedeemsOnBoats, so it's silently dropped by the normal (IgnoreUnmatchedProperties)
        // load and has to be read separately.
        private class LegacyModData
        {
            public LegacySettings settings;
        }

        private class LegacySettings
        {
            public bool? allowRedeemsOnBoats;
        }

        /// <summary>
        /// Upgrades <paramref name="data"/> (just deserialized from <paramref name="path"/>) in
        /// place to <see cref="CurrentVersion"/>. Returns true if anything was upgraded. Idempotent.
        /// </summary>
        public static bool Migrate(ModData data, string path)
        {
            if (data == null || data.version >= CurrentVersion)
                return false;

            Jotunn.Logger.LogWarning($"[WBTI] Migrating profile file '{path}' from format v{data.version} to v{CurrentVersion}.");

            // v0 -> v1: new-UI settings/title changes.
            if (data.version < 1)
            {
                MigrateBoatSafezone(data, path);
                StripBakedTitlePrefix(data);
            }

            // v1 -> v2: ItemData.stackSize renamed to amount.
            if (data.version < 2)
                MigrateItemStackSize(data);

            data.version = CurrentVersion;
            return true;
        }

        /// <summary>
        /// Surprise chest item entries used to store their count as "stackSize"; it is now "amount".
        /// ItemData still deserializes the old key into its legacy stackSize field, which is carried
        /// over here and cleared so the next save writes only the new key.
        /// </summary>
        private static void MigrateItemStackSize(ModData data)
        {
            if (data.redeems == null)
                return;

            foreach (RedeemData redeem in data.redeems)
            {
                if (redeem.chestData?.items == null)
                    continue;

                foreach (SurpriseChestSpawnData entry in redeem.chestData.items)
                {
                    ItemData item = entry?.itemData;
                    if (item?.stackSize == null)
                        continue;

                    item.amount = item.stackSize.Value;
                    item.stackSize = null;
                }
            }
        }

        /// <summary>
        /// allowRedeemsOnBoats (true = redeems work on boats) was replaced by safezoneBoats
        /// (true = boats act as a safezone), so the value is inverted. A profile that never set the
        /// old key already gets the equivalent default (allow = true, safezoneBoats = false).
        /// </summary>
        private static void MigrateBoatSafezone(ModData data, string path)
        {
            if (data.settings == null)
                return;

            bool? allowOnBoats = ReadLegacySettings(path)?.allowRedeemsOnBoats;
            if (allowOnBoats.HasValue)
                data.settings.safezoneBoats = !allowOnBoats.Value;
        }

        /// <summary>
        /// Redeem titles used to be stored with the "WBTI " prefix baked in; RedeemData.title is now
        /// prefix-free and RedeemManager.GetFullTitle() re-adds the profile's prefix wherever the
        /// Twitch-facing title is needed, so the Twitch reward title stays identical. A title is left
        /// alone if stripping would empty it or collide with another redeem's title.
        /// </summary>
        private static void StripBakedTitlePrefix(ModData data)
        {
            string prefix = (data.settings ?? new ProfileSettingsData()).redeemTitlePrefix;
            if (string.IsNullOrEmpty(prefix) || data.redeems == null)
                return;

            string prefixWithSpace = prefix + " ";
            HashSet<string> titles = new HashSet<string>();
            foreach (RedeemData redeem in data.redeems)
                titles.Add(redeem.title);

            foreach (RedeemData redeem in data.redeems)
            {
                if (redeem.title == null || !redeem.title.StartsWith(prefixWithSpace))
                    continue;

                string stripped = redeem.title.Substring(prefixWithSpace.Length);
                if (string.IsNullOrWhiteSpace(stripped) || titles.Contains(stripped))
                    continue;

                titles.Remove(redeem.title);
                titles.Add(stripped);
                redeem.title = stripped;
            }
        }

        private static LegacySettings ReadLegacySettings(string path)
        {
            try
            {
                IDeserializer deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                using (StreamReader reader = new StreamReader(path))
                    return deserializer.Deserialize<LegacyModData>(reader)?.settings;
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read legacy settings keys from '{path}': {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Persists the migration for every profile on disk (one-time, at startup), so profiles
        /// that are never opened - and therefore never re-saved - still end up in the current
        /// format. The pre-migration file is kept next to it as profile.yaml.premigration.bak.
        /// Synced (read-only) profiles are skipped: they're re-downloaded and migrated in memory
        /// on read anyway.
        /// </summary>
        public static void MigrateAllProfilesOnDisk()
        {
            foreach (string profileName in ProfileManager.GetProfiles())
            {
                if (ProfileManager.IsSyncedProfile(profileName))
                    continue;

                string path = ProfileManager.GetRedeemPath(profileName);

                try
                {
                    if (!File.Exists(path))
                        continue;

                    ModData data = ExtraConfigHelper.ReadRedeemsConfig(path, out bool migrated);
                    if (!migrated)
                        continue;

                    string backupPath = path + ".premigration.bak";
                    if (!File.Exists(backupPath))
                        File.Copy(path, backupPath);

                    ExtraConfigHelper.WriteRedeemsConfig(path, data.settings, data.creatureGroups, data.redeems);
                }
                catch (System.Exception e)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Failed to migrate profile '{profileName}' on disk: {e}");
                }
            }
        }
    }
}
