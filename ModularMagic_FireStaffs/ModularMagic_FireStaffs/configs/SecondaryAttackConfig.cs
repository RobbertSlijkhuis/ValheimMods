using BepInEx.Configuration;
using ModularMagic_FireStaffs.Helpers;
using ModularMagic_FireStaffs.Models;
using ModularMagic_FireStaffs.Types;
using System;

namespace ModularMagic_FireStaffs.Configs
{
    /// <summary>
    /// The config of a secondary attack (the nova and the meteors). The attack of the wrapper prefab is copied to the staff
    /// on equip. The spawned projectile (meteors) has its own damage, so those values do not depend on the stats of the staff.
    /// The nova AOE uses the damage and the attack force of the staff, scaled by the damage multiplier of the attack.
    /// </summary>
    internal class SecondaryAttackConfig
    {
        public ConfigEntry<int> useEitr;
        public ConfigEntry<float> eitrMultiplier;
        public ConfigEntry<float> aoe;
        public ConfigEntry<float> damageMultiplier;
        public ConfigEntry<float> damageFire;
        public ConfigEntry<float> damageBlunt;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> attackForce;
        public ConfigEntry<float> forceMultiplier;
        public ConfigEntry<float> cooldown;
        public ConfigEntry<float> launchAngle;
        public ConfigEntry<float> projectileVelocity;
        public ConfigEntry<float> projectileAccuracy;
        public ConfigEntry<int> minToSpawn;
        public ConfigEntry<int> maxToSpawn;
        public ConfigEntry<float> spawnRadius;
        public ConfigEntry<float> spawnAccuracy;
        public ConfigEntry<float> spawnDelay;

        // The name of the status effect that is the cooldown of this attack
        public string cooldownStatusEffectName;

        private int entryCount = 100;

        public void GenerateConfig(SecondaryAttackConfigOptions options)
        {
            ConfigFile config = ModularMagic_FireStaffs.Instance.Config;
            cooldownStatusEffectName = options.cooldownStatusEffectName;

            if (options.eitrMultiplier != null)
            {
                // The base eitr cost depends on the staff, so it is worked out when the runes are applied to the staff.
                // The held staff is updated by the SettingChanged hook of the plugin
                eitrMultiplier = config.Bind(new ConfigDefinition(options.sectionName, "Eitr cost multiplier"), (float)options.eitrMultiplier,
                    new ConfigDescription("The eitr cost of the secondary attack as a multiplier of the eitr cost of the staff's attack (4 is four times, so a staff of 20 eitr costs 80)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            }
            else
            {
                useEitr = config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack eitr cost"), options.useEitr,
                    new ConfigDescription("The secondary attack eitr cost", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                // The attack of the wrapper prefab is copied to the staff on equip, so this is the base eitr cost of the secondary attack
                UpdateAttackEitr(options);
                useEitr.SettingChanged += (obj, attr) => UpdateAttackEitr(options);
            }

            // Core reads the value every time the cooldown starts, so a changed value is used without a handler
            cooldown = config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack cooldown"), options.cooldown,
                new ConfigDescription("Cooldown duration of the secondary ability in seconds", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            aoe = BindFloat(config, options, "AOE radius", options.aoe, "The damage radius of the attack",
                value => UpdateTarget(options, aoe: value));

            if (options.damageMultiplier != null)
            {
                if (options.type == SecondaryAttackType.Projectile)
                {
                    // The projectiles are spawned by a script that does not get the damage of the staff, so their damage is set on the
                    // projectile prefab when the runes are applied to the staff (see FireImbuementHelper). The held staff is updated by the SettingChanged hook of the plugin
                    damageMultiplier = BindFloat(config, options, "Damage multiplier", (float)options.damageMultiplier,
                        "The damage of every projectile as a multiplier of the damage of the staff (fire and blunt, including quality). 1 is the damage of the staff, 0.5 is half",
                        value => { });
                }
                else
                {
                    // The wrapper attack is copied to the staff on equip, so this is the base multiplier of the damage of the staff
                    damageMultiplier = BindFloat(config, options, "Damage multiplier", (float)options.damageMultiplier,
                        "The damage of the attack as a multiplier of the damage of the staff (fire and blunt, including quality). 1 is the damage of the staff, 2 is double",
                        value => UpdateAttack(options, damageMultiplier: value));
                    UpdateAttack(options, damageMultiplier: damageMultiplier.Value);
                }
            }
            else
            {
                damageFire = BindFloat(config, options, "Fire damage", options.damageFire, "The fire damage of the attack (not visible ingame)",
                    value => UpdateTarget(options, damageFire: value));
            }

            if (options.damageBlunt != null)
            {
                damageBlunt = BindFloat(config, options, "Blunt damage", (float)options.damageBlunt, "The blunt damage of the attack (not visible ingame)",
                    value => UpdateTarget(options, damageBlunt: value));
            }

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

            // The attack force of an attack that uses the damage of the staff comes from the staff (see forceMultiplier)
            if (options.damageMultiplier == null)
            {
                attackForce = BindFloat(config, options, "Attack force", options.attackForce, "The attack force of the attack applied to targets hit",
                    value => UpdateTarget(options, attackForce: value));
            }

            if (options.forceMultiplier != null)
            {
                if (options.type == SecondaryAttackType.Projectile)
                {
                    // Set on the projectile prefab when the runes are applied to the staff (see FireImbuementHelper), like the damage multiplier
                    forceMultiplier = BindFloat(config, options, "Force multiplier", (float)options.forceMultiplier,
                        "The attack force of every projectile as a multiplier of the attack force of the staff. 1 is the attack force of the staff",
                        value => { });
                }
                else
                {
                    // The wrapper attack is copied to the staff on equip, so this is the base multiplier of the attack force of the staff
                    forceMultiplier = BindFloat(config, options, "Force multiplier", (float)options.forceMultiplier,
                        "The attack force of the attack as a multiplier of the attack force of the staff. 1 is the attack force of the staff, 0 is no knockback",
                        value => UpdateAttack(options, forceMultiplier: value));
                    UpdateAttack(options, forceMultiplier: forceMultiplier.Value);
                }
            }

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

                if (options.spawnAccuracy != null)
                {
                    spawnAccuracy = BindFloat(config, options, "Spawn accuracy", (float)options.spawnAccuracy,
                        "The accuracy (spread in degrees) of the spawned projectiles. They fall towards the center, so a higher value spreads them over a wider area and is less accurate",
                        value => UpdateHelper.UpdateSpawnAbility(options.spawnAbilityPrefab, new UpdateSpawnAbilityOptions() { projectileAccuracy = value }));
                }

                if (options.spawnDelay != null)
                {
                    spawnDelay = BindFloat(config, options, "Spawn delay", (float)options.spawnDelay, "The time in seconds between two spawned projectiles. A higher value spreads the projectiles over a longer time",
                        value => UpdateHelper.UpdateSpawnAbility(options.spawnAbilityPrefab, new UpdateSpawnAbilityOptions() { spawnDelay = value }));
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
        private static void UpdateTarget(SecondaryAttackConfigOptions options, float? aoe = null, float? damageFire = null, float? damageBlunt = null,
            float? damageChop = null, float? damagePickaxe = null, float? attackForce = null)
        {
            if (options.type == SecondaryAttackType.Projectile)
            {
                UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                {
                    aoe = aoe,
                    damageFire = damageFire,
                    damageBlunt = damageBlunt,
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
                });
            }
        }

        // The attack of the wrapper prefab itself. Only the prefab data is updated, not the staff in the hand
        private static void UpdateAttack(SecondaryAttackConfigOptions options, float? launchAngle = null, float? projectileVelocity = null, float? projectileAccuracy = null,
            float? damageMultiplier = null, float? forceMultiplier = null)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                damageMultiplier = damageMultiplier,
                forceMultiplier = forceMultiplier,
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
