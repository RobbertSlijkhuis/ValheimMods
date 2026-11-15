using BepInEx.Configuration;
using System;
using ImbuementConfig = ModularMagic_Core.Configs.ImbuementConfig;
using ImbuementConfigOptions = ModularMagic_Core.Models.ImbuementConfigOptions;
using StaffConfig = ModularMagic_Core.Configs.StaffConfig;
using StaffConfigOptions = ModularMagic_Core.Models.StaffConfigOptions;

namespace ModularMagic_BloodMagic.Configs
{
    internal static class PluginConfig
    {
        public static string scythe1Name          = "Death's Embrace";
        public static string scythe1Recipe        = "RoundLog:10, Iron:10, Ectoplasm:10";
        public static string scythe1UpgradeRecipe = "RoundLog:5, Iron:5, Ectoplasm:2";
        public static StaffConfig scythe1         = new StaffConfig();

        public static string scythe2Name          = "Reaper of Souls";
        public static string scythe2Recipe        = "FineWood:10, BlackMetal:10, Tar:10";
        public static string scythe2UpgradeRecipe = "FineWood:5, BlackMetal:5, Tar:2";
        public static StaffConfig scythe2         = new StaffConfig();

        public static string scythe3Name          = "Eternal Reaper";
        public static string scythe3Recipe        = "YggdrasilWood:10, FlametalNew:10, MoltenCore:10";
        public static string scythe3UpgradeRecipe = "YggdrasilWood:5, FlametalNew:5, MoltenCore:2";
        public static StaffConfig scythe3         = new StaffConfig();

        // The values of the runes of the scythes, see StaffDefinition
        public static ImbuementConfig imbuementConfig = new ImbuementConfig();

        // Apparition configs
        public static ConfigEntry<int>   scythe1ApparitionHealth;
        public static ConfigEntry<float> scythe1ApparitionDamageMultiplier;

        public static ConfigEntry<int>   scythe2ApparitionHealth;
        public static ConfigEntry<float> scythe2ApparitionDamageMultiplier;

        public static ConfigEntry<int>   scythe3ApparitionHealth;
        public static ConfigEntry<float> scythe3ApparitionDamageMultiplier;

        // Soul Reaper configs: the charge of the scythes, the number of apparitions, the reanimation of a husk and the exhaustion
        public static ConfigEntry<float> chargePerHit;
        public static ConfigEntry<float> chargeMultiplierWithoutApparitions;
        public static ConfigEntry<int>   apparitionsForNormalCharge;
        public static ConfigEntry<float> chargeEitr;
        public static ConfigEntry<int>   maxApparitions;
        public static ConfigEntry<int>   maxApparitionsPerQuality;
        public static ConfigEntry<float> reanimateHealthCost;
        public static ConfigEntry<float> exhaustionPerApparition;
        public static ConfigEntry<float> exhaustionMax;
        public static ConfigEntry<bool>  apparitionsDespawn;

        public static void Init()
        {
            InitSythe1Config();
            InitSythe2Config();
            InitSythe3Config();
            InitApparitionConfigs();
            InitSoulReaperConfigs();
            InitImbuementConfig();
        }

        private static void InitImbuementConfig()
        {
            // The Attack Speed rune speeds up the swing animation of a scythe, like it does for the fire and lightning staffs. The Eitr Cost
            // rune lowers the eitr of a charging hit (see SoulReaperHelper.GetChargeEitr), which costs more than an attack of a staff, so
            // a level saves more: 0.75 per level is 3 eitr at level 4
            imbuementConfig.GenerateConfig(ModularMagic_BloodMagic.Instance.Config, new ImbuementConfigOptions() { attackSpeedAnimation = true, eitrCost = 0.75f });
        }

        private static void InitSoulReaperConfigs()
        {
            ConfigFile Config = ModularMagic_BloodMagic.Instance.Config;
            string section = "Soul Reaper";

            chargePerHit = Config.Bind(new ConfigDefinition(section, "Charge per hit"), 0.1f,
                new ConfigDescription("The part of a full charge every hit with a scythe adds (0.1 is 10%, so 10 hits for a summon).",
                new AcceptableValueRange<float>(0.01f, 1f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 9 }));

            chargeMultiplierWithoutApparitions = Config.Bind(new ConfigDefinition(section, "Charge multiplier without apparitions"), 3f,
                new ConfigDescription("How much faster a scythe charges while you have no apparitions. The bonus gets smaller with every apparition you have, until it is gone at the number of 'Apparitions for normal charge'.",
                new AcceptableValueRange<float>(1f, 10f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 8 }));

            apparitionsForNormalCharge = Config.Bind(new ConfigDefinition(section, "Apparitions for normal charge"), 3,
                new ConfigDescription("The number of apparitions (banished ones included) from which a scythe charges at the normal rate.",
                new AcceptableValueRange<int>(1, 20),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 7 }));

            chargeEitr = Config.Bind(new ConfigDefinition(section, "Eitr per charging hit"), 10f,
                new ConfigDescription("The eitr a hit needs and uses to charge a scythe. The Eitr Cost rune lowers it.",
                new AcceptableValueRange<float>(0f, 100f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 6 }));

            maxApparitions = Config.Bind(new ConfigDefinition(section, "Maximum apparitions"), 3,
                new ConfigDescription("The number of apparitions (banished ones included) a player can have with a scythe of quality 1.",
                new AcceptableValueRange<int>(1, 20),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 5 }));

            maxApparitionsPerQuality = Config.Bind(new ConfigDefinition(section, "Extra apparitions per quality level"), 1,
                new ConfigDescription("The number of extra apparitions for every quality level of the scythe above 1.",
                new AcceptableValueRange<int>(0, 5),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 4 }));

            reanimateHealthCost = Config.Bind(new ConfigDefinition(section, "Reanimate health cost"), 0.2f,
                new ConfigDescription("The part of your maximum health it costs to reanimate a banished apparition (0.2 is 20%).",
                new AcceptableValueRange<float>(0f, 0.95f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 3 }));

            exhaustionPerApparition = Config.Bind(new ConfigDefinition(section, "Exhaustion damage reduction per apparition"), 0.1f,
                new ConfigDescription("The part of your damage you lose for every active apparition (0.1 is 10%). A banished apparition does not count.",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 2 }));

            exhaustionMax = Config.Bind(new ConfigDefinition(section, "Exhaustion maximum damage reduction"), 0.5f,
                new ConfigDescription("The largest part of your damage Foul Magic Exhaustion can take away (0.5 is 50%).",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 1 }));

            apparitionsDespawn = Config.Bind(new ConfigDefinition(section, "Despawn when away"), true,
                new ConfigDescription("Apparitions are removed when their summoner is more than 150 m away, or has been gone (offline or too far away) for 2 minutes. Turn this off to keep them until they are killed.",
                null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 0 }));
        }

        private static void InitApparitionConfigs()
        {
            ConfigFile Config = ModularMagic_BloodMagic.Instance.Config;

            scythe1ApparitionHealth = Config.Bind(new ConfigDefinition("Deaths Embrace - Apparition", "Health"), 280,
                new ConfigDescription("The health of apparitions spawned by Death's Embrace.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe1ApparitionDamageMultiplier = Config.Bind(new ConfigDefinition("Deaths Embrace - Apparition", "Damage multiplier"), 0.6f,
                new ConfigDescription(DamageMultiplierDescription, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, CustomDrawer = DamageMultiplierDrawer.Create(scythe1) }));

            scythe2ApparitionHealth = Config.Bind(new ConfigDefinition("Reaper of Souls - Apparition", "Health"), 980,
                new ConfigDescription("The health of apparitions spawned by Reaper of Souls.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe2ApparitionDamageMultiplier = Config.Bind(new ConfigDefinition("Reaper of Souls - Apparition", "Damage multiplier"), 0.67f,
                new ConfigDescription(DamageMultiplierDescription, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, CustomDrawer = DamageMultiplierDrawer.Create(scythe2) }));

            scythe3ApparitionHealth = Config.Bind(new ConfigDefinition("Eternal Reaper - Apparition", "Health"), 1680,
                new ConfigDescription("The health of apparitions spawned by Eternal Reaper.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            scythe3ApparitionDamageMultiplier = Config.Bind(new ConfigDefinition("Eternal Reaper - Apparition", "Damage multiplier"), 0.69f,
                new ConfigDescription(DamageMultiplierDescription, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, CustomDrawer = DamageMultiplierDrawer.Create(scythe3) }));
        }

        // The in-game config window also shows what the hardest hit deals, see DamageMultiplierDrawer
        private const string DamageMultiplierDescription =
            "How much of the scythe's damage (quality included) the apparitions deal: 0.6 is 60%. Their hardest hit deals this much; their weaker attacks deal less. The Blood Magic skill raises it further.";

        private static void InitSythe1Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe1, scythe1Name, scythe1Recipe, scythe1UpgradeRecipe)
                {
                    description         = "A harvester of souls that can be used in your advantage!",
                    craftingStation     = "Forge",
                    minStationLevel     = 2,
                    maxQuality          = 4,
                    weight              = 0.3f,
                    maxDurability       = 200f,
                    movementSpeed       = -0.05f,
                    damageSlash         = 50f,
                    damageSlashPerLevel = 6f,
                    blockArmor          = 28,
                    deflectionForce     = 70,
                    attackForce         = 70,
                    backStab            = 3f,
                    attackStamina       = 16,
                };
                scythe1.GenerateConfig(ModularMagic_BloodMagic.Instance.Config, options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe1Name + " config: " + error);
            }
        }

        private static void InitSythe2Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe2, scythe2Name, scythe2Recipe, scythe2UpgradeRecipe)
                {
                    description         = "A harvester of souls that can be used in your advantage!",
                    craftingStation     = "Forge",
                    minStationLevel     = 2,
                    maxQuality          = 4,
                    weight              = 0.3f,
                    maxDurability       = 200f,
                    movementSpeed       = -0.05f,
                    damageSlash         = 90f,
                    damageSlashPerLevel = 6f,
                    blockArmor          = 52,
                    deflectionForce     = 70,
                    attackForce         = 70,
                    backStab            = 3f,
                    attackStamina       = 20,
                };
                scythe2.GenerateConfig(ModularMagic_BloodMagic.Instance.Config, options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe2Name + " config: " + error);
            }
        }

        private static void InitSythe3Config()
        {
            try
            {
                StaffConfigOptions options = new StaffConfigOptions(ModularMagic_BloodMagic.Instance.prefabs.Scythe3, scythe3Name, scythe3Recipe, scythe3UpgradeRecipe)
                {
                    description         = "A harvester of souls that can be used in your advantage!",
                    craftingStation     = "Forge",
                    minStationLevel     = 2,
                    maxQuality          = 4,
                    weight              = 0.3f,
                    maxDurability       = 200f,
                    movementSpeed       = -0.05f,
                    damageSlash         = 130f,
                    damageSlashPerLevel = 6f,
                    blockArmor          = 64,
                    deflectionForce     = 70,
                    attackForce         = 70,
                    backStab            = 3f,
                    attackStamina       = 20,
                };
                scythe3.GenerateConfig(ModularMagic_BloodMagic.Instance.Config, options);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise " + scythe3Name + " config: " + error);
            }
        }
    }
}
