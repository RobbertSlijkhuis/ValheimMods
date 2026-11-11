using BepInEx.Configuration;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;
using UnityEngine;

namespace ModularMagic_Core.Configs
{
    /// <summary>
    /// The config of a secondary attack (the nova, the rain projectiles, the summoned creatures). The attack of the wrapper prefab is copied
    /// to the staff on equip. For the nova the damage and the attack force come from the staff, scaled by the multipliers of this config
    /// (through the attack, see MMAoe). A projectile that is spawned by a script does not get them (see SpawnedProjectileHelper), apart from
    /// its chop and pickaxe damage, which are set on its prefab. A value that is null in the options is not bound as a config entry.
    /// </summary>
    public class SecondaryAttackConfig
    {
        public ConfigEntry<float> health;
        public ConfigEntry<int> minToSpawn;
        public ConfigEntry<int> maxToSpawn;
        public ConfigEntry<int> maxSpawns;
        public ConfigEntry<float> spawnRadius;
        public ConfigEntry<float> spawnAccuracy;
        public ConfigEntry<float> spawnDelay;
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
        public ConfigEntry<float> eitrMultiplier;
        public ConfigEntry<float> damageMultiplier;
        public ConfigEntry<float> forceMultiplier;
        public ConfigEntry<float> cooldown;

        // The name of the status effect that is the cooldown of this attack
        public string cooldownStatusEffectName;
        public string type;
        private int entryCount = 100;

        // The entries are bound in this order, which is also the order they have in the config file and the config manager.
        // The names (and so the saved values) of the entries must not change
        public void GenerateConfig(ConfigFile config, SecondaryAttackConfigOptions options)
        {
            cooldownStatusEffectName = options.cooldownStatusEffectName;
            type = options.type;

            bool projectile = options.type == SecondaryAttackType.Projectile;
            bool humanoid = options.type == SecondaryAttackType.Humanoid;
            // A projectile or a creature that a script spawns does not get the damage and the force of the staff through the attack, see SpawnedProjectileHelper
            // and SpawnedCreatureHelper. Only the attack of a nova (an area hit) has the multipliers
            bool spawnedByScript = projectile || humanoid;
            string entityDesc = projectile ? "projectile" : humanoid ? "summoned creatures(s)" : "projectile / summoned creatures(s)";

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
                useEitr.SettingChanged += (obj, attr) =>
                {
                    UpdateAttackEitr(options);

                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        secondaryAttackEitr = useEitr.Value,
                    });
                };
            }

            // Core reads the value every time the cooldown starts, so a changed value is used without a handler
            cooldown = config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack cooldown"), options.cooldown,
                new ConfigDescription("Cooldown duration of the secondary ability in seconds", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));

            if (options.health != null)
            {
                health = BindFloat(config, options, "Health", (float)options.health, "The health of summoned creature(s)",
                    value => UpdateHelper.UpdateHumanoid(options.secondPrefab, new UpdateHumanoidOptions() { health = value }));
            }

            if (options.aoe != null)
            {
                aoe = BindFloat(config, options, "AOE radius", (float)options.aoe, "The damage radius of the " + (projectile ? "projectile" : "attack"),
                    value => UpdateTarget(options, aoe: value));
            }

            if (options.damageMultiplier != null)
            {
                // The wrapper attack is copied to the staff on equip, so for the nova this is the base multiplier of the attack.
                // The projectiles of the rain are spawned by a script that does not get the damage of the staff, so their damage is
                // set on the spawned projectile (see SpawnedProjectileHelper)
                damageMultiplier = BindFloat(config, options, "Damage multiplier", (float)options.damageMultiplier,
                    "The damage of the attack as a multiplier of the damage of the staff (including quality). 1 is the damage of the staff, 0.5 is half",
                    value => { if (!spawnedByScript) UpdateAttack(options, damageMultiplier: value); });

                if (!spawnedByScript)
                    UpdateAttack(options, damageMultiplier: damageMultiplier.Value);
            }

            if (options.forceMultiplier != null)
            {
                forceMultiplier = BindFloat(config, options, "Force multiplier", (float)options.forceMultiplier,
                    "The attack force of the attack as a multiplier of the attack force of the staff. 1 is the attack force of the staff, 0 is no knockback",
                    value => { if (!spawnedByScript) UpdateAttack(options, forceMultiplier: value); });

                if (!spawnedByScript)
                    UpdateAttack(options, forceMultiplier: forceMultiplier.Value);
            }

            if (options.damageBlunt != null)
            {
                damageBlunt = BindFloat(config, options, "Blunt damage", (float)options.damageBlunt, "The blunt damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damageBlunt: value));
            }

            if (options.damageChop != null)
            {
                damageChop = BindFloat(config, options, "Chop damage", (float)options.damageChop, "The chop damage on the " + entityDesc + " (not visible ingame, effects wood cutting damage)",
                    value => UpdateTarget(options, damageChop: value));
            }

            if (options.damageGeneral != null)
            {
                damageGeneral = BindFloat(config, options, "General damage", (float)options.damageGeneral, "The general damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damageGeneral: value));
            }

            if (options.damagePickaxe != null)
            {
                damagePickaxe = BindFloat(config, options, "Pickaxe damage", (float)options.damagePickaxe, "The pickaxe damage on the " + entityDesc + " (not visible ingame, effects mining damage)",
                    value => UpdateTarget(options, damagePickaxe: value));
            }

            if (options.damagePierce != null)
            {
                damagePierce = BindFloat(config, options, "Pierce damage", (float)options.damagePierce, "The pierce damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damagePierce: value));
            }

            if (options.damagePoison != null)
            {
                damagePoison = BindFloat(config, options, "Poison damage", (float)options.damagePoison, "The poison damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damagePoison: value));
            }

            if (options.damageSpirit != null)
            {
                damageSpirit = BindFloat(config, options, "Spirit damage", (float)options.damageSpirit, "The spirit damage on the " + entityDesc + " (not visible ingame)",
                    value => UpdateTarget(options, damageSpirit: value));
            }

            if (options.attackForce != null)
            {
                attackForce = BindFloat(config, options, "Attack force", (float)options.attackForce, "The attack force of the " + entityDesc + " applied to targets hit",
                    value => UpdateTarget(options, attackForce: value));
            }

            string spawned = humanoid ? "creature(s)" : "projectiles";

            if (options.minToSpawn != null)
            {
                minToSpawn = BindInt(config, options, "Minimum to spawn", (int)options.minToSpawn, $"The minimum amount of {spawned} that will be {(humanoid ? "summoned" : "spawned")}",
                    value => UpdateHelper.UpdateSpawnAbility(GetSpawnAbilityPrefab(options), new UpdateSpawnAbilityOptions() { minToSpawn = value }));
            }

            if (options.maxToSpawn != null)
            {
                maxToSpawn = BindInt(config, options, "Maximum to spawn", (int)options.maxToSpawn, $"The maximum amount of {spawned} that will be {(humanoid ? "summoned" : "spawned")}",
                    value => UpdateHelper.UpdateSpawnAbility(GetSpawnAbilityPrefab(options), new UpdateSpawnAbilityOptions() { maxToSpawn = value }));
            }

            if (options.maxSpawns != null)
            {
                maxSpawns = BindInt(config, options, "Maximum spawns", (int)options.maxSpawns, "The maximum amount of summoned creature(s)",
                    value => UpdateHelper.UpdateSpawnAbility(GetSpawnAbilityPrefab(options), new UpdateSpawnAbilityOptions() { maxSpawns = value }));
            }

            if (options.spawnRadius != null)
            {
                spawnRadius = BindFloat(config, options, "Spawn radius", (float)options.spawnRadius, $"The radius in which the {spawned} will be {(humanoid ? "summoned" : "spawned")}",
                    value => UpdateHelper.UpdateSpawnAbility(GetSpawnAbilityPrefab(options), new UpdateSpawnAbilityOptions() { spawnRadius = value }));
            }

            if (options.spawnAccuracy != null)
            {
                spawnAccuracy = BindFloat(config, options, "Spawn accuracy", (float)options.spawnAccuracy,
                    "The accuracy (spread in degrees) of the spawned projectiles. They fall towards the center, so a higher value spreads them over a wider area and is less accurate",
                    value => UpdateHelper.UpdateSpawnAbility(GetSpawnAbilityPrefab(options), new UpdateSpawnAbilityOptions() { projectileAccuracy = value }));
            }

            if (options.spawnDelay != null)
            {
                spawnDelay = BindFloat(config, options, "Spawn delay", (float)options.spawnDelay,
                    "The time in seconds between two spawned projectiles. A higher value spreads the projectiles over a longer time",
                    value => UpdateHelper.UpdateSpawnAbility(GetSpawnAbilityPrefab(options), new UpdateSpawnAbilityOptions() { spawnDelay = value }));
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

        // The prefab that has the SpawnAbility: the one the options name, or the projectile of the attack of the wrapper prefab
        private static GameObject GetSpawnAbilityPrefab(SecondaryAttackConfigOptions options)
        {
            return options.spawnAbilityPrefab != null ? options.spawnAbilityPrefab : UpdateHelper.GetAttackProjectile(options.prefab);
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

        // Sets the multipliers on the attack of the wrapper prefab itself. Only the prefab data is updated, the staff in the hand is updated by the SettingChanged hook of the plugin
        private static void UpdateAttack(SecondaryAttackConfigOptions options, float? damageMultiplier = null, float? forceMultiplier = null)
        {
            UpdateHelper.UpdateItemData(options.prefab.GetComponent<ItemDrop>().m_itemData, new UpdateItemDataOptions()
            {
                damageMultiplier = damageMultiplier,
                forceMultiplier = forceMultiplier,
            });
        }

        // Sets the eitr cost on the attack of the wrapper prefab itself. Only the prefab data is updated, not the staff in the hand
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
