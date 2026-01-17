using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class ExtraConfigHelper
    {
        public static List<RedeemEntry> ReadRedeemsConfig(bool debug = false)
        {
            StreamReader streamReader = new StreamReader(WizshBoneTwitchIntegration.redeemsConfigPath);
            string fileContent = streamReader.ReadToEnd();

            if (debug)
            {
                Jotunn.Logger.LogWarning("\n" + fileContent);
                Jotunn.Logger.LogWarning("=================================================");
            }

            if (fileContent == null || fileContent == "")
            {
                Jotunn.Logger.LogError("Could not read redeems configuration or its empty");
                return null;
            }

            StringReader stringReader = new StringReader(fileContent);
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            RedeemData data = deserializer.Deserialize<RedeemData>(stringReader);

            if (debug)
            {
                foreach (RedeemEntry redeem in data.redeems)
                {
                    Jotunn.Logger.LogWarning(redeem.type);
                    Jotunn.Logger.LogWarning(redeem.title);
                    Jotunn.Logger.LogWarning(redeem.points);
                    Jotunn.Logger.LogWarning(redeem.backgroundColor);
                    Jotunn.Logger.LogWarning(redeem.cooldown);
                    Jotunn.Logger.LogWarning(redeem.userInput);
                    Jotunn.Logger.LogWarning(redeem.globalKeyAdd);
                    Jotunn.Logger.LogWarning(redeem.globalKeyRemove);

                    if (redeem.creatures != null)
                    {
                        foreach (SpawnCreatureData creature in redeem.creatures)
                        {
                            Jotunn.Logger.LogWarning(creature.prefabName);
                            Jotunn.Logger.LogWarning(creature.level);
                            Jotunn.Logger.LogWarning(creature.amount);
                            Jotunn.Logger.LogWarning(creature.position);
                            Jotunn.Logger.LogWarning(creature.allowDrops);
                            Jotunn.Logger.LogWarning(creature.friendly);
                            Jotunn.Logger.LogWarning(creature.commandable);
                            Jotunn.Logger.LogWarning(creature.talks);
                            Jotunn.Logger.LogWarning(creature.talkInteract);
                            Jotunn.Logger.LogWarning(creature.talkInterval);
                            Jotunn.Logger.LogWarning(creature.talkMessage);
                            Jotunn.Logger.LogWarning(creature.isHallucination);
                        }
                    }
                }

                Jotunn.Logger.LogWarning("=================================================");
            }

            streamReader.Close();
            stringReader.Close();
            return data.redeems;
        }

        public static void InitRedeemsConfig()
        {
            if (!File.Exists(WizshBoneTwitchIntegration.redeemsConfigPath))
            {
                Directory.CreateDirectory(WizshBoneTwitchIntegration.customConfigPath);
                WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.redeems.yaml", WizshBoneTwitchIntegration.redeemsConfigPath);
            }

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
        }

        public static void UpdateRedeemWithTesterFile()
        {
            if (!File.Exists(WizshBoneTwitchIntegration.customConfigPath + "/redeems-old.yaml"))
                File.Move(WizshBoneTwitchIntegration.redeemsConfigPath, WizshBoneTwitchIntegration.customConfigPath + "/redeems-old.yaml");
            else
            {
                string[] fileContents = File.ReadAllLines(WizshBoneTwitchIntegration.redeemsConfigPath);
                File.WriteAllLines(WizshBoneTwitchIntegration.customConfigPath + "/redeems-old.yaml", fileContents);
            }

            WriteFromEmbeddedResourceTo("WizshBoneTwitchIntegration.resources.redeems-testers.yaml", WizshBoneTwitchIntegration.redeemsConfigPath);
        }

        public static List<string> ReadBannedUsersFromFile()
        {
            if (!File.Exists(WizshBoneTwitchIntegration.bannedPath))
                throw new System.Exception("Cannot find file");

            string[] fileContents = File.ReadAllLines(WizshBoneTwitchIntegration.bannedPath);
            return fileContents.ToList();
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

        private static void WriteFromEmbeddedResourceTo(string resourceFullName, string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceFullName));
            string fileContents = reader.ReadToEnd();
            reader.Close();

            StreamWriter writer = File.CreateText(path);
            writer.WriteLine(fileContents);
            writer.Close();
        }
    }
}