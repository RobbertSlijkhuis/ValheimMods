using System.Collections.Generic;
using System.IO;
using WizshBoneTwitchIntegration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class YAMLHelper
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
                    Jotunn.Logger.LogWarning(redeem.globalKey);

                    if (redeem.creatures != null)
                    {
                        foreach (SpawnCreatureData creature in redeem.creatures)
                        {
                            Jotunn.Logger.LogWarning(creature.prefabName);
                            Jotunn.Logger.LogWarning(creature.level);
                            Jotunn.Logger.LogWarning(creature.amount);
                            Jotunn.Logger.LogWarning(creature.allowDrops);
                            Jotunn.Logger.LogWarning(creature.talks);
                            Jotunn.Logger.LogWarning(creature.position);
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
                StreamWriter writer = File.CreateText(WizshBoneTwitchIntegration.redeemsConfigPath);
                writer.WriteLine(DefaultRedeemsConfig());
                writer.Close();
            }
        }

        private static string DefaultRedeemsConfig()
        {
            return "# In this file you can add and configure your redeems for the WizshBone Twitch Integration\r\n#\r\n# The following properties are required for every redeem, these values will be checked upon adding the redeems to Twitch\r\n# If any of them are omitted or empty, the redeem will not be loaded!\r\n# - type\r\n# - title\r\n# - points\r\n#\r\n# Every redeem requires a type, the following types are available:\r\n# - ExplodeFish (this currently has no options, right now only explodes fish within a certain area of the player)\r\n# - PlayerGrow (this currently has no options)\r\n# - PlayerShrink (this currently has no options)\r\n# - SpawnCreature\r\n# - SpawnHallucination (this currently has no options, right now randomly spawns biome appropiate creatures over time)\r\n# - SpawnShower (this currently has no options, right now spawns a fish rain)\r\n# - StatusEffectRandom (this currently has no options, right now gives you a random buff or debuff from a hardcoded list)\r\n# - TerrainRemove (this currently has no options, right now it will greate a hole under the player with hardcoded values)\r\n# - Undefined (This does nothing and is the default value of the redeem when the type is omitted in this config, this redeem will not be loaded)\r\n#\r\n# Creatures require a position to spawn, the following spawn positions are available:\r\n# - Flying (spawns in front of the player and a little bit in the air)\r\n# - OnPlayer (spawns on the player)\r\n# - RandomBehind (spawns in a random location behind the player, within 30-50 meters)\r\n# - Undefined (This does nothing and will prevent this creature from spawning)\r\n#\r\nredeems:\r\n  - type: SpawnCreature             # The type of the redeem, determines what will happen in-game (see list above for more info)\r\n    title: \"WBTI: Spawn Greyling!\"  # The title of the redeem\r\n    points: 100                     # The amount of points this redeem will cost\r\n    backgroundColor: \"#395c32\"      # (optional) The color of the redeems background, defaults to Twitch purple\r\n    cooldown: 0                     # (optional) wether this redeem has a shared global cooldown, defaults to 0\r\n    userInput: false                # (optional) wether this redeem requires user input, defaults to false (this is required to be true when you want to allow viewers to determine what a creatures says)\r\n    globalKey: null                 # (optional) Wether this redeem will become available when a global key is active(ted), defaults to null (global keys are set when a boss is killed for example)\r\n    creatures:                # Creature options are only available for redeems of the type \"SpawnCreature\", this can be omitted for other types of redeems\r\n      - prefabName: Greyling  # The \"Internal ID\" of the creature in the game, you can find these on the Valheim Wiki\r\n        level: 1              # (optional) The level of the creature, defaults to 1 (level 1 = 0 stars, 2 = 1 stars, 3 = 2 stars)\r\n        amount: 1              # (optional) The amount of this creature to spawn, defaults to 1\r\n        allowDrops: false     # (optional) Wether this creature will drop its normal loot, defaults to false\r\n        talks: false          # (optional) Weather this creature will say something when spawned (currently has a limited list of 4 standard responses, when userInput is enabled allows viewers to enter text to say)\r\n        position: OnPlayer    # (optional) Where this creature will spawn, defaults to OnPlayer (see list above for more info)\r\n  - type: StatusEffectRandom\r\n    title: \"WBTI: Random buff/debuff\"\r\n    points: 500\r\n    backgroundColor: \"#bb33ff\"";
        }
    }
}
