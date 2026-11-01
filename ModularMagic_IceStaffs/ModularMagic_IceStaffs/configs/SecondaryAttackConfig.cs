using BepInEx.Configuration;
using ModularMagic_IceStaffs.Helpers;
using ModularMagic_IceStaffs.Models;
using System;

namespace ModularMagic_IceStaffs.configs
{
    /// <summary>
    /// The config of a secondary attack (the nova and the ice shards). The attack of the wrapper prefab is copied to the staff
    /// on equip and the spawned AOE or projectile has its own damage, so these values do not depend on the stats of the staff.
    /// </summary>
    internal class SecondaryAttackConfig
    {
        public ConfigEntry<int> useEitr;
        public ConfigEntry<float> aoe;
        public ConfigEntry<float> damageFrost;
        public ConfigEntry<float> damagePierce;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> attackForce;
        public ConfigEntry<float> cooldown;
        public ConfigEntry<float> launchAngle;
        public ConfigEntry<float> projectileVelocity;
        public ConfigEntry<float> projectileAccuracy;
        public ConfigEntry<int> minToSpawn;
        public ConfigEntry<int> maxToSpawn;
        public ConfigEntry<float> spawnRadius;

        // The name of the status effect that is the cooldown of this attack
        public string cooldownStatusEffectName;

        private int entryCount = 100;

        public void GenerateConfig(SecondaryAttackConfigOptions options)
        {
            ConfigFile config = ModularMagic_IceStaffs.Instance.Config;
            cooldownStatusEffectName = options.cooldownStatusEffectName;

            useEitr = config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack eitr cost"), options.useEitr,
                new ConfigDescription("The secondary attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            // The attack of the wrapper prefab is copied to the staff on equip, so this is the base eitr cost of the secondary attack
            UpdateAttackEitr(options);
            useEitr.SettingChanged += (obj, attr) => UpdateAttackEitr(options);

            if (options.cooldown != null)
            {
                // Core reads the value every time the cooldown starts, so a changed value is used without a handler
                cooldown = config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack cooldown"), (float)options.cooldown,
                    new ConfigDescription("Cooldown duration of the secondary ability in seconds", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }

            aoe = BindFloat(config, options, "AOE radius", options.aoe, "The damage radius of the attack",
                value => UpdateTarget(options, aoe: value));
            damageFrost = BindFloat(config, options, "Frost damage", options.damageFrost, "The frost damage of the attack (not visible ingame)",
                value => UpdateTarget(options, damageFrost: value));
            damagePierce = BindFloat(config, options, "Pierce damage", options.damagePierce, "The pierce damage of the attack (not visible ingame)",
                value => UpdateTarget(options, damagePierce: value));

            if (options.damageChop != null)
            {
                damageChop = BindFloat(config, options, "Chop damage", (float)options.damageChop, "The chop damage of the attack (not visible ingame, effects wood cutting damage)",
                    value => UpdateTarget(options, damageChop: value));
            }

            if (options.damagePickaxe != null)
            {
                damagePickaxe = BindFloat(config, options, "Pickaxe damage", (float)options.damagePickaxe, "The pickaxe damage of the attack (not visible ingame, effects mining damage)",
                    value => UpdateTarget(options, damagePickaxe: value));
            }

            attackForce =BindFloat(config, options, "Attack force", options.attackForce, "The attack force of the attack applied to targets hit",
                value => UpdateTarget(options, attackForce: value));

            if (options.launchAngle != null)
            {
                launchAngle = BindFloat(config, options, "Projectile launch angle", (float)options.launchAngle, "The angle the projectile is emitted from the staff",
                    value => UpdateAttack(options, launchAngle: value));
            }

            if (options.projectileVelocity != null)
            {
                projectileVelocity = BindFloat(config, options, "Projectile velocity", (float)options.projectileVelocity, "The velocity of the projectile emitted from the staff",
                    value => UpdateAttack(options, projectileVelocity: value));
            }

            if (options.projectileAccuracy != null)
            {
                projectileAccuracy = BindFloat(config, options, "Projectile accuracy", (float)options.projectileAccuracy, "The accuracy (spread) of the projectile emitted from the staff",
                    value => UpdateAttack(options, projectileAccuracy: value));
            }

            if (options.spawnAbilityPrefab != null)
            {
                if (options.minToSpawn != null)
                {
                    minToSpawn = BindInt(config, options, "Minimum to spawn", (int)options.minToSpawn, "The minimum amount of projectiles that will be spawned",
                        value => UpdateHelper.UpdateSpawnAbility(options.spawnAbilityPrefab, new UpdateSpawnAbilityOptions() { minToSpawn = value }));
                }

                if (options.maxToSpawn != null)
                {
                    maxToSpawn = BindInt(config, options, "Maximum to spawn", (int)options.maxToSpawn, "The maximum amount of projectiles that will be spawned",
                        value => UpdateHelper.UpdateSpawnAbility(options.spawnAbilityPrefab, new UpdateSpawnAbilityOptions() { maxToSpawn = value }));
                }

                if (options.spawnRadius != null)
                {
                    spawnRadius = BindFloat(config, options, "Spawn radius", (float)options.spawnRadius, "The radius in which the projectiles will be spawned",
                        value => UpdateHelper.UpdateSpawnAbility(options.spawnAbilityPrefab, new UpdateSpawnAbilityOptions() { spawnRadius = value }));
                }
            }
        }

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

        // The AOE or the projectile that carries the damage of the attack
        private static void UpdateTarget(SecondaryAttackConfigOptions options, float? aoe = null, float? damageFrost = null, float? damagePierce = null,
            float? damageChop = null, float? damagePickaxe = null, float? attackForce = null)
        {
            if (options.type == SecondaryAttackType.Projectile)
            {
                UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                {
                    aoe = aoe,
                    damageFrost = damageFrost,
                    damagePierce = damagePierce,
                    damageChop = damageChop,
                    damagePickaxe = damagePickaxe,
                    attackForce = attackForce,
                });
            }
            else
            {
                UpdateHelper.UpdateAoe(options.secondPrefab, new UpdateAoeOptions()
                {
                    aoe = aoe,
                    damageFrost = damageFrost,
                    damagePierce = damagePierce,
                    damageChop = damageChop,
                    damagePickaxe = damagePickaxe,
                    attackForce = attackForce,
                });
            }
        }

        // The attack of the wrapper prefab itself. Only the prefab data is updated, not the staff in the hand
        private static void UpdateAttack(SecondaryAttackConfigOptions options, float? launchAngle = null, float? projectileVelocity = null, float? projectileAccuracy = null)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                launchAngle = launchAngle,
                projectileVelocity = projectileVelocity,
                projectileAccuracy = projectileAccuracy,
            });
        }

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
