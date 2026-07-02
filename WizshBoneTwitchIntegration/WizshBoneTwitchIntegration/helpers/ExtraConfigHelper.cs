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
            ProfileManager.Init();

            if (!File.Exists(WizshBoneTwitchIntegration.redeemsSchemaPath))
            {
                Directory.CreateDirectory(WizshBoneTwitchIntegration.customConfigPath);
                WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.redeems-schema.json", WizshBoneTwitchIntegration.redeemsSchemaPath);
            }

            if (!File.Exists(WizshBoneTwitchIntegration.bannedPath))
            {
                Directory.CreateDirectory(WizshBoneTwitchIntegration.customConfigPath);
                WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.banned.txt", WizshBoneTwitchIntegration.bannedPath);
            }

            if (!File.Exists(WizshBoneTwitchIntegration.viewersPath))
            {
                Jotunn.Logger.LogWarning("Could not find viewers file! Writting...");
                Directory.CreateDirectory(WizshBoneTwitchIntegration.customConfigPath);
                WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.viewers.yaml", WizshBoneTwitchIntegration.viewersPath);
            }
        }

        public static ModData ReadRedeemsConfig(string path = null)
        {
            path = path ?? ProfileManager.GetActiveRedeemPath();
            return DeserializeYaml<ModData>(path);
        }

        public static List<ViewerEntry> ReadViewersConfig()
        {
            ViewerData data = DeserializeYaml<ViewerData>(WizshBoneTwitchIntegration.viewersPath);
            return data?.viewers;
        }

        public static List<string> ReadBannedUsersFromFile()
        {
            if (!File.Exists(WizshBoneTwitchIntegration.bannedPath))
                throw new System.Exception("Cannot find file");

            return File.ReadAllLines(WizshBoneTwitchIntegration.bannedPath).ToList();
        }

        public static void WriteBannedUsersToFile(List<string> bannedUsers)
        {
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
            WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.redeems.yaml", path);
        }

        /// <summary>
        /// Serializes a profile's creatureGroups + redeems to its redeems.yaml, matching the
        /// hand-formatted layout (creatureGroups preamble, type banners, sorted groups) that
        /// the in-game editor produces. Shared by RedeemsTab.OnSave and cross-profile redeem copy.
        /// </summary>
        public static void WriteRedeemsConfig(string path, List<CreatureGroupData> creatureGroups, List<RedeemData> redeems)
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var sb = new StringBuilder();

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

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
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