using BepInEx.Configuration;
using ModularMagic_LightningStaffs.Helpers;
using ModularMagic_LightningStaffs.Models;
using ModularMagic_LightningStaffs.Types;
using System;

namespace ModularMagic_LightningStaffs.Configs
{
    /// <summary>
    /// The config of a secondary attack (the nova and the lightning bolts). The attack of the wrapper prefab is copied to the staff
    /// on equip. The damage and the attack force come from the staff, scaled by the multipliers of this config. The nova applies them
    /// through the attack (see MMAoe). The bolts are spawned by a script that does not get them, so they are set on the bolt prefab
    /// when the runes are applied to the staff (see LightningImbuementHelper).
    /// </summary>
    internal class SecondaryAttackConfig
    {
        public ConfigEntry<float> eitrMultiplier;
        public ConfigEntry<float> aoe;
        public ConfigEntry<float> damageMultiplier;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> forceMultiplier;
        public ConfigEntry<float> cooldown;
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
            ConfigFile config = ModularMagic_LightningStaffs.Instance.Config;
            bool projectile = options.type == SecondaryAttackType.Projectile;
            cooldownStatusEffectName = options.cooldownStatusEffectName;

            // The base eitr cost depends on the staff, so it is worked out when the runes are applied to the staff.
            // The held staff is updated by the SettingChanged hook of the plugin
            eitrMultiplier = config.Bind(new ConfigDefinition(options.sectionName, "Eitr cost multiplier"), options.eitrMultiplier,
                new ConfigDescription("The eitr cost of the secondary attack as a multiplier of the eitr cost of the staff's attack (4 is four times, so a staff of 20 eitr costs 80)", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            // Core reads the value every time the cooldown starts, so a changed value is used without a handler
            cooldown = config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack cooldown"), options.cooldown,
                new ConfigDescription("Cooldown duration of the secondary ability in seconds", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            aoe = BindFloat(config, options, "AOE radius", options.aoe, "The damage radius of the attack",
                value => UpdateTarget(options, aoe: value));

            // The wrapper attack is copied to the staff on equip, so for the nova these are the base multipliers of the attack.
            // For the bolts they are used when the runes are applied to the staff
            damageMultiplier = BindFloat(config, options, "Damage multiplier", options.damageMultiplier,
                "The damage of the attack as a multiplier of the damage of the staff (including quality). 1 is the damage of the staff, 0.5 is half",
                value => { if (!projectile) UpdateAttack(options, damageMultiplier: value); });
            forceMultiplier = BindFloat(config, options, "Force multiplier", options.forceMultiplier,
                "The attack force of the attack as a multiplier of the attack force of the staff. 1 is the attack force of the staff, 0 is no knockback",
                value => { if (!projectile) UpdateAttack(options, forceMultiplier: value); });

            if (!projectile)
                UpdateAttack(options, damageMultiplier: damageMultiplier.Value, forceMultiplier: forceMultiplier.Value);

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

        // The area hit or the projectile that carries the attack
        private static void UpdateTarget(SecondaryAttackConfigOptions options, float? aoe = null, float? damageChop = null, float? damagePickaxe = null)
        {
            if (options.type == SecondaryAttackType.Projectile)
            {
                UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                {
                    aoe = aoe,
                    damageChop = damageChop,
                    damagePickaxe = damagePickaxe,
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
        private static void UpdateAttack(SecondaryAttackConfigOptions options, float? damageMultiplier = null, float? forceMultiplier = null)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                damageMultiplier = damageMultiplier,
                forceMultiplier = forceMultiplier,
            });
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
