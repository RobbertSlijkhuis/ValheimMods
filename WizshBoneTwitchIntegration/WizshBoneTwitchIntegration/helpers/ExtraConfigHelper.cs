using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class ExtraConfigHelper
    {
        public static void InitExtraConfigs()
        {
            try
            {
                ProfileManager.Init();
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Failed to initialize the default profile: {e}");
            }

            EnsureFileExists(WizshBoneTwitchIntegration.bannedPath, "WizshBoneTwitchIntegration.resources.banned.txt", "banned users");
            EnsureFileExists(WizshBoneTwitchIntegration.viewersPath, "WizshBoneTwitchIntegration.resources.viewers.yaml", "viewers");
        }

        /// <summary>
        /// Recreates a single embedded-resource-backed config file from its default if missing.
        /// Called both eagerly at startup (InitExtraConfigs) and lazily from the matching Read*
        /// method below, so the file self-heals whenever it's next needed - not only on first run -
        /// and one file's creation failing never blocks another's.
        /// </summary>
        private static void EnsureFileExists(string path, string embeddedResourceName, string description)
        {
            if (File.Exists(path))
                return;

            try
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not find {description} file at '{path}' - creating it from the default.");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                WriteFromEmbeddedResourceTo(embeddedResourceName, path);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Failed to create default {description} file at '{path}': {e}");
            }
        }

        /// <summary>
        /// Same self-healing idea as EnsureFileExists, but for a profile's profile.yaml - seeded
        /// via WriteDefaultRedeemsTo rather than a single fixed embedded resource path, since the
        /// destination varies per profile.
        /// </summary>
        private static void EnsureRedeemsFileExists(string path)
        {
            if (File.Exists(path))
                return;

            Jotunn.Logger.LogWarning($"[WBTI] Could not find profile data file at '{path}' - recreating from the default.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            WriteDefaultRedeemsTo(path);
        }

        public static ModData ReadRedeemsConfig(string path = null)
        {
            return ReadRedeemsConfig(path, out _);
        }

        /// <param name="migrated">True if the file was in an older format and the returned data was
        /// upgraded in memory (see ProfileMigrationHelper) - the file itself is only rewritten by
        /// the next save.</param>
        public static ModData ReadRedeemsConfig(string path, out bool migrated)
        {
            path = path ?? ProfileManager.GetActiveRedeemPath();
            EnsureRedeemsFileExists(path);
            ModData data = DeserializeYaml<ModData>(path);
            migrated = ProfileMigrationHelper.Migrate(data, path);

            // A profile with zero redeems/creatureGroups serializes as a bare "redeems:"/
            // "creatureGroups:" key (see WriteRedeemsConfig), which YamlDotNet deserializes back as
            // null rather than an empty list - guarantee non-null lists here, once, rather than
            // making every one of this method's many callers guard against it individually.
            if (data != null)
            {
                data.redeems = data.redeems ?? new List<RedeemData>();
                data.creatureGroups = data.creatureGroups ?? new List<CreatureGroupData>();
            }

            return data;
        }

        public static ProfileSettingsData ReadSettingsConfig(string path = null)
        {
            path = path ?? ProfileManager.GetActiveSettingsPath();

            // Any profile folder created before settings.yaml existed (every profile on an
            // install upgrading from an earlier version) has no settings.yaml of its own yet -
            // reset it to defaults here rather than at every profile-creation call site, so every
            // read is guaranteed a file to open regardless of how the profile came to exist.
            if (!File.Exists(path))
                WriteDefaultSettingsTo(path);

            return DeserializeYaml<ProfileSettingsData>(path);
        }

        public static void WriteSettingsConfig(string path, ProfileSettingsData settings)
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, serializer.Serialize(settings));
        }

        public static void WriteDefaultSettingsTo(string path)
        {
            WriteSettingsConfig(path, new ProfileSettingsData());
        }

        public static List<ViewerEntry> ReadViewersConfig()
        {
            EnsureFileExists(WizshBoneTwitchIntegration.viewersPath, "WizshBoneTwitchIntegration.resources.viewers.yaml", "viewers");
            ViewerData data = DeserializeYaml<ViewerData>(WizshBoneTwitchIntegration.viewersPath);
            List<ViewerEntry> viewers = data?.viewers ?? new List<ViewerEntry>();

            foreach (ViewerEntry viewer in viewers)
                viewer.MigrateLegacyFields();

            EnsureRequiredViewers(viewers);

            return viewers;
        }

        private const string RequiredViewerName = "deathwizsh";

        // Single source of truth for deathwizsh's required defaults. Both the "entry missing
        // entirely" and "entry exists but a field is blank" paths in EnsureRequiredViewers pull
        // from this, so a new ViewerEntry field only needs to be added here, plus one backfill
        // line below.
        private static ViewerEntry RequiredViewerDefault => new ViewerEntry
        {
            name = RequiredViewerName,
            color = "#3489EB",
            effects = new List<string> { "lightning" }
        };

        /// <summary>
        /// Guarantees the "deathwizsh" entry is present in the given viewers list and that every
        /// field on it is at least at its default value, healing whatever was lost - whether the
        /// whole entry was deleted (hand-edited yaml, older save) or it merely predates a newer
        /// ViewerEntry field. Case-insensitive on name. Never overwrites a field the user has
        /// already set to something other than that field's default - only blank/default fields
        /// are backfilled. Shared by ReadViewersConfig (every read) and ViewersTab.Save (every
        /// write) so there is one source of truth for this guarantee.
        /// </summary>
        public static void EnsureRequiredViewers(List<ViewerEntry> viewers)
        {
            ViewerEntry existing = viewers.Find(v =>
                string.Equals(v.name, RequiredViewerName, System.StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Required viewer '{RequiredViewerName}' missing from viewers config, re-adding it.");
                viewers.Add(RequiredViewerDefault);
                return;
            }

            ViewerEntry defaults = RequiredViewerDefault;

            if (string.IsNullOrEmpty(existing.color))
                existing.color = defaults.color;

            if (existing.effects == null || existing.effects.Count == 0)
                existing.effects = defaults.effects;

            // Add one more "if existing.<field> is blank/default: existing.<field> = defaults.<field>"
            // line here for each new ViewerEntry field added in the future.
        }

        /// <summary>
        /// Serializes viewers straight to WizshBoneTwitchIntegration.viewersPath, same shape as
        /// ViewersTab.Save()'s write. Used by RecolorHelper.ReloadViewersConfig() to restore the
        /// file from the currently loaded viewers if it went missing after already being loaded
        /// once, instead of silently resetting to the embedded stock template.
        /// </summary>
        public static void WriteViewersConfig(List<ViewerEntry> viewers)
        {
            viewers = viewers ?? new List<ViewerEntry>();
            EnsureRequiredViewers(viewers);

            List<Dictionary<string, object>> dicts = viewers.Select(v => v.ToDictionary()).ToList();
            Dictionary<string, object> output = new Dictionary<string, object> { { "viewers", dicts } };

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Directory.CreateDirectory(Path.GetDirectoryName(WizshBoneTwitchIntegration.viewersPath));
            File.WriteAllText(WizshBoneTwitchIntegration.viewersPath, serializer.Serialize(output));
        }

        public static List<string> ReadBannedUsersFromFile()
        {
            EnsureFileExists(WizshBoneTwitchIntegration.bannedPath, "WizshBoneTwitchIntegration.resources.banned.txt", "banned users");
            return File.ReadAllLines(WizshBoneTwitchIntegration.bannedPath).ToList();
        }

        public static void WriteBannedUsersToFile(List<string> bannedUsers)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(WizshBoneTwitchIntegration.bannedPath));
            File.WriteAllLines(WizshBoneTwitchIntegration.bannedPath, bannedUsers);
        }

        public static void BanTwitchUser(string user)
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
            string userToLower = user.ToLower();

            if (customRewards == null || customRewards.m_bannedUsers.Contains(userToLower))
                return;

            customRewards.m_bannedUsers.Add(userToLower);
        }

        public static void UnbanTwitchUser(string user)
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (customRewards == null)
                return;

            customRewards.m_bannedUsers.Remove(user);
        }

        public static void WriteDefaultRedeemsTo(string path)
        {
            WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.profile.yaml", path);
        }

        /// <summary>
        /// Seeds the extra "Chatting only" profile (no redeems, nothing that affects the game) on a
        /// fresh install - see ProfileManager.Init.
        /// </summary>
        public static void WriteChattingOnlyProfileTo(string path)
        {
            WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.profile_chatting_only.yaml", path);
        }

        /// <summary>
        /// Serializes a profile's settings + creatureGroups + redeems to its profile.yaml,
        /// matching the hand-formatted layout (settings/creatureGroups preambles, type banners,
        /// sorted groups) that the in-game editor produces. Shared by RedeemsTab.OnSave,
        /// cross-profile redeem copy, and ProfileSettingsHelper.Save.
        /// </summary>
        public static void WriteRedeemsConfig(string path, ProfileSettingsData settings, List<CreatureGroupData> creatureGroups, List<RedeemData> redeems)
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var sb = new StringBuilder();

            // Stamped on every save so a rewritten file is never mistaken for legacy again (and
            // re-migrated) - see ProfileMigrationHelper.
            sb.AppendLine($"version: {ProfileMigrationHelper.CurrentVersion}");

            if (settings != null)
            {
                var settingsPreamble = new Dictionary<string, object> { { "settings", settings } };
                sb.Append(serializer.Serialize(settingsPreamble));
            }

            if (creatureGroups != null && creatureGroups.Count > 0)
            {
                var minimalGroups = creatureGroups.Select(g => RedeemData.MinimalDictionary(g)).ToList();
                var preamble = new Dictionary<string, object> { { "creatureGroups", minimalGroups } };
                sb.Append(serializer.Serialize(preamble));
            }

            var groups = redeems
                .OrderBy(r => r.type?.ToString() ?? "")
                .ThenBy(r => r.title ?? "")
                .GroupBy(r => r.type?.ToString() ?? "");

            sb.AppendLine("redeems:");

            foreach (var group in groups)
            {
                string typeName = string.IsNullOrEmpty(group.Key) ? "Unknown" : group.Key;

                sb.AppendLine($"  #######################");
                sb.AppendLine($"  # {typeName}");
                sb.AppendLine($"  #######################");

                foreach (RedeemData redeem in group)
                {
                    // Serialize a single-item list so YamlDotNet emits the "- key: value" block format,
                    // then strip the leading "- " list wrapper we get from a root sequence.
                    var single = new List<Dictionary<string, object>> { redeem.ToDictionary() };
                    string itemYaml = serializer.Serialize(single);
                    // itemYaml looks like "- key: value\n  key2: value2\n"
                    // Indent every line by 2 spaces to sit under "redeems:"
                    foreach (string line in itemYaml.Split('\n'))
                    {
                        if (line.Length == 0) continue;
                        sb.Append("  ");
                        sb.AppendLine(line.TrimEnd('\r'));
                    }
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, sb.ToString());
        }

        private static T DeserializeYaml<T>(string path) where T : class
        {
            string fileContent;

            using (StreamReader reader = new StreamReader(path))
                fileContent = reader.ReadToEnd();

            if (string.IsNullOrEmpty(fileContent))
            {
                Jotunn.Logger.LogError($"Could not read yaml file or it is empty: {path}");
                return null;
            }

            // IgnoreUnmatchedProperties: an on-disk profile.yaml can predate a field being removed
            // from a model (e.g. the CreatureData/SpawnAbilityData fields only ever set by the old
            // Saphonette-special redeems) - without this, YamlDotNet throws on the now-unrecognized
            // property instead of just ignoring it, which would otherwise nuke the whole profile
            // back to an empty redeem list on next load.
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            using (StringReader stringReader = new StringReader(fileContent))
                return deserializer.Deserialize<T>(stringReader);
        }

        private static void WriteFromEmbeddedResourceTo(string resourceFullName, string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceFullName)))
            using (StreamWriter writer = File.CreateText(path))
                writer.WriteLine(reader.ReadToEnd());
        }
    }
}