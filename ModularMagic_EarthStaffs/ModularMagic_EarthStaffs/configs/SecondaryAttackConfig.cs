using BepInEx.Configuration;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using System;

namespace ModularMagic_EarthStaffs.Configs
{
    internal class SecondaryAttackConfig
    {
        public ConfigEntry<float> health;
        public ConfigEntry<int> minToSpawn;
        public ConfigEntry<int> maxToSpawn;
        public ConfigEntry<int> maxSpawns;
        public ConfigEntry<float> spawnRadius;
        public ConfigEntry<float> aoe;
        public ConfigEntry<float> damageBlunt;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damageGeneral;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> damagePierce;
        public ConfigEntry<float> damagePoison;
        public ConfigEntry<float> damageSpirit;
        public ConfigEntry<float> attackForce;
        public ConfigEntry<int> useEitr;
        public ConfigEntry<float> cooldown;
        public ConfigEntry<float> launchAngle;
        public ConfigEntry<float> projectileVelocity;
        public ConfigEntry<float> projectileAccuracy;

        public string cooldownStatusEffectName;
        public string type;
        private int entryCount = 100;

        // The entries are bound in this order, which is also the order they have in the config file and the config manager.
        // The names (and so the saved values) of the entries must not change
        public void GenerateConfig(SecondaryAttackConfigOptions options)
        {
            ConfigFile Config = ModularMagic_EarthStaffs.Instance.Config;
            this.cooldownStatusEffectName = options.cooldownStatusEffectName;
            this.type = options.type;

            string entityDesc = "projectile / summoned creatures(s)";

            if (options.type == SecondaryAttackType.Projectile)
                entityDesc = "projectile";

            if (options.type == SecondaryAttackType.Humanoid)
                entityDesc = "summoned creatures(s)";

            useEitr = Config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack eitr cost"), options.useEitr,
                new ConfigDescription("The secondary attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            // The attack of this prefab is copied to the staff on equip, so this is the base eitr cost of the secondary attack
            UpdateAttackEitr(options);
            useEitr.SettingChanged += (obj, attr) =>
            {
                UpdateAttackEitr(options);

                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    secondaryAttackEitr = useEitr.Value,
                });
            };

            // Core reads the value every time the cooldown starts, so a changed value is used without a handler
            cooldown = Config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack cooldown"), options.cooldown,
                new ConfigDescription("Cooldown duration of the secondary ability", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            if (options.health != null)
            {
                health = BindFloat(Config, options, "Health", (float)options.health, "The health of summoned creature(s)",
                    value => UpdateHelper.UpdateHumanoid(options.secondPrefab, new UpdateHumanoidOptions() { health = value }));
            }

            if (options.aoe != null)
            {
                aoe = BindFloat(Config, options, "AOE radius", (float)options.aoe, "The damage radius of the projectile",
                    value => UpdateTarget(options, aoe: value));
            }

            if (options.damageBlunt != null)
            {
                damageBlunt = BindFloat(Config, options, "Blunt damage", (float)options.damageBlunt, "The blunt damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damageBlunt: value));
            }

            if (options.damageChop != null)
            {
                damageChop = BindFloat(Config, options, "Chop damage", (float)options.damageChop, "The chop damage on the " + entityDesc + " (not visible ingame, effects wood cutting damage)",
                    value => UpdateTarget(options, damageChop: value));
            }

            if (options.damageGeneral != null)
            {
                damageGeneral = BindFloat(Config, options, "General damage", (float)options.damageGeneral, "The general damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damageGeneral: value));
            }

            if (options.damagePickaxe != null)
            {
                damagePickaxe = BindFloat(Config, options, "Pickaxe damage", (float)options.damagePickaxe, "The pickaxe damage on the " + entityDesc + " (not visible ingame, effects mining damage)",
                    value => UpdateTarget(options, damagePickaxe: value));
            }

            if (options.damagePierce != null)
            {
                damagePierce = BindFloat(Config, options, "Pierce damage", (float)options.damagePierce, "The pierce damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damagePierce: value));
            }

            if (options.damagePoison != null)
            {
                damagePoison = BindFloat(Config, options, "Poison damage", (float)options.damagePoison, "The poison damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damagePoison: value));
            }

            if (options.damageSpirit != null)
            {
                damageSpirit = BindFloat(Config, options, "Spirit damage", (float)options.damageSpirit, "The spirit damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damageSpirit: value));
            }

            if (options.attackForce != null)
            {
                attackForce = BindFloat(Config, options, "Attack force", (float)options.attackForce, "The attack force of the " + entityDesc + " applied to targets hit",
                    value => UpdateTarget(options, attackForce: value));
            }

            if (options.minToSpawn != null)
            {
                minToSpawn = BindInt(Config, options, "Minimum to spawn", (int)options.minToSpawn, "The minimum amount of creature(s) that will be summoned",
                    value => UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions() { minToSpawn = value }));
            }

            if (options.maxToSpawn != null)
            {
                maxToSpawn = BindInt(Config, options, "Maximum to spawn", (int)options.maxToSpawn, "The maximum amount of creature(s) that will be summoned",
                    value => UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions() { maxToSpawn = value }));
            }

            if (options.maxSpawns != null)
            {
                maxSpawns = BindInt(Config, options, "Maximum spawns", (int)options.maxSpawns, "The maximum amount of summoned creature(s)",
                    value => UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions() { maxSpawns = value }));
            }

            if (options.spawnRadius != null)
            {
                spawnRadius = BindFloat(Config, options, "Spawn radius", (float)options.spawnRadius, "The radius in which the creatures(s) will be summoned",
                    value => UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions() { spawnRadius = value }));
            }

            if (options.launchAngle != null)
            {
                launchAngle = BindFloat(Config, options, "Projectile launch angle", (float)options.launchAngle, "The angle the projectile is emitted from the staff",
                    value =>
                    {
                        // The attack of the wrapper prefab is what gets copied to the staff on equip, so that is its main attack
                        UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions() { launchAngle = value });

                        // The staff in the hand has the attack as its secondary attack
                        UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions() { secondaryLaunchAngle = value });
                    });
            }

            if (options.projectileVelocity != null)
            {
                projectileVelocity = BindFloat(Config, options, "Projectile velocity", (float)options.projectileVelocity, "The velocity of the projectile emitted from the staff",
                    value =>
                    {
                        // The attack of the wrapper prefab is what gets copied to the staff on equip, so that is its main attack
                        UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions() { projectileVelocity = value });

                        // The staff in the hand has the attack as its secondary attack
                        UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions() { secondaryProjectileVelocity = value });
                    });
            }

            if (options.projectileAccuracy != null)
            {
                projectileAccuracy = BindFloat(Config, options, "Projectile accuracy", (float)options.projectileAccuracy, "The accuracy (spread) of the projectile emitted from the staff",
                    value =>
                    {
                        UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions() { projectileAccuracy = value });
                        UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions() { secondaryProjectileAccuracy = value });
                    });
            }
        }

        // Binds an admin only entry and applies its value whenever it changes
        private ConfigEntry<float> BindFloat(ConfigFile config, SecondaryAttackConfigOptions options, string name, float defaultValue, string description, Action<float> apply)
        {
            ConfigEntry<float> entry = config.Bind(new ConfigDefinition(options.sectionName, name), defaultValue,
                new ConfigDescription(description, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            entry.SettingChanged += (obj, attr) => apply(entry.Value);

            return entry;
        }

        private ConfigEntry<int> BindInt(ConfigFile config, SecondaryAttackConfigOptions options, string name, int defaultValue, string description, Action<int> apply)
        {
            ConfigEntry<int> entry = config.Bind(new ConfigDefinition(options.sectionName, name), defaultValue,
                new ConfigDescription(description, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            entry.SettingChanged += (obj, attr) => apply(entry.Value);

            return entry;
        }

        // Applies a changed value to what the attack spawns: the AOE, the projectile or the summoned creature(s)
        private static void UpdateTarget(SecondaryAttackConfigOptions options, float? aoe = null, float? damageBlunt = null, float? damageChop = null,
            float? damageGeneral = null, float? damagePickaxe = null, float? damagePierce = null, float? damagePoison = null, float? damageSpirit = null,
            float? attackForce = null)
        {
            if (options.type == SecondaryAttackType.AOE)
            {
                UpdateHelper.UpdateAoe(options.secondPrefab, new UpdateAoeOptions()
                {
                    aoe = aoe,
                    damageBlunt = damageBlunt,
                    damageChop = damageChop,
                    damageGeneral = damageGeneral,
                    damagePickaxe = damagePickaxe,
                    damagePierce = damagePierce,
                    damagePoison = damagePoison,
                    damageSpirit = damageSpirit,
                    attackForce = attackForce,
                });
            }
            else if (options.type == SecondaryAttackType.Projectile)
            {
                UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                {
                    aoe = aoe,
                    damageBlunt = damageBlunt,
                    damageChop = damageChop,
                    damageGeneral = damageGeneral,
                    damagePickaxe = damagePickaxe,
                    damagePierce = damagePierce,
                    damagePoison = damagePoison,
                    damageSpirit = damageSpirit,
                    attackForce = attackForce,
                });
            }
            else if (options.type == SecondaryAttackType.Humanoid)
            {
                UpdateHelper.UpdateHumanoidAttackItemData(options.secondPrefab, new UpdateItemDataOptions()
                {
                    damageBlunt = damageBlunt,
                    damageChop = damageChop,
                    damageGeneral = damageGeneral,
                    damagePickaxe = damagePickaxe,
                    damagePierce = damagePierce,
                    damagePoison = damagePoison,
                    damageSpirit = damageSpirit,
                    attackForce = attackForce,
                });
            }
        }

        // Sets the eitr cost on the attack of the prefab itself. Only the prefab data is updated, not the staff in the hand
        private void UpdateAttackEitr(SecondaryAttackConfigOptions options)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                attackEitr = useEitr.Value,
            });
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
