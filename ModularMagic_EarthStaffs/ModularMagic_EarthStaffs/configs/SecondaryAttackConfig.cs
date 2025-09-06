using BepInEx.Configuration;
using ModularMagic_EarthStaffs.Helpers;
using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;

namespace ModularMagic_EarthStaffs.Configs
{
    internal class SecondaryAttackConfig
    {
        // The  fields to generate
        public ConfigEntry<float> health;
        public ConfigEntry<int> minToSpawn;
        public ConfigEntry<int> maxToSpawn;
        public ConfigEntry<int> maxSpawns;
        public ConfigEntry<float> spawnRadius;
        public ConfigEntry<float> aoe;
        public ConfigEntry<float> damageBlunt;
        public ConfigEntry<float> damageChop;
        public ConfigEntry<float> damagePickaxe;
        public ConfigEntry<float> damagePoison;
        public ConfigEntry<float> damageSpirit;
        public ConfigEntry<float> attackForce;
        public ConfigEntry<int> useEitr;
        public ConfigEntry<float> cooldown;
        public ConfigEntry<float> launchAngle;
        public ConfigEntry<float> projectileVelocity;
        public ConfigEntry<float> projectileAccuracy;

        // Other
        public string cooldownStatusEffectName;
        public string type;
        private int entryCount = 100;

        public void GenerateConfig(SecondaryAttackConfigOptions options)
        {
            ConfigFile Config = ModularMagic_EarthStaffs.Instance.Config;
            this.cooldownStatusEffectName = options.cooldownStatusEffectName;
            this.type = options.type;

            string entityDesc = "projectile / summoned creatures(s)";
            if (options.type == SecondaryAttackType.Projectile) entityDesc = "projectile";
            if (options.type == SecondaryAttackType.Humanoid) entityDesc = "summoned creatures(s)";

            useEitr = Config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack eitr cost"), options.useEitr,
                new ConfigDescription("The secondary attack eitr cost", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            useEitr.SettingChanged += (obj, attr) =>
            {
                UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                {
                    secondaryAttackEitr = useEitr.Value,
                });
            };

            cooldown = Config.Bind(new ConfigDefinition(options.sectionName, "Secondary attack cooldown"), options.cooldown,
                new ConfigDescription("Cooldown duration of the secondary ability", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
            cooldown.SettingChanged += (obj, attr) =>
            {
                Jotunn.Logger.LogWarning("Effect name: " + options.cooldownStatusEffectName);
                Jotunn.Logger.LogWarning("Effect hash: " + options.cooldownStatusEffectName.GetStableHashCode());
                StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(options.cooldownStatusEffectName.GetStableHashCode());

                if (statusEffect != null)
                {
                    Jotunn.Logger.LogWarning("Name: " + statusEffect.m_name);
                    Jotunn.Logger.LogWarning("TTL: " + statusEffect.m_ttl);
                    Jotunn.Logger.LogWarning("New value: " + cooldown.Value);
                    statusEffect.m_ttl = cooldown.Value;
                }
            };

            if (options.health != null)
            {
                health = Config.Bind(new ConfigDefinition(options.sectionName, "Health"), (float)options.health,
                    new ConfigDescription("The health of summoned creature(s)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                health.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateHumanoid(options.secondPrefab, new UpdateHumanoidOptions()
                    {
                        health = health.Value,
                    });
                };
            }

            if (options.aoe != null)
            {
                aoe = Config.Bind(new ConfigDefinition(options.sectionName, "AOE radius"), (float)options.aoe,
                    new ConfigDescription("The damage radius of the projectile", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                aoe.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                    {
                        aoe = aoe.Value,
                    });
                };
            }

            if (options.damageBlunt != null)
            {
                damageBlunt = Config.Bind(new ConfigDefinition(options.sectionName, "Blunt damage"), (float)options.damageBlunt,
                    new ConfigDescription("The blunt damage on the " + entityDesc + " (not visible ingame)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageBlunt.SettingChanged += (obj, attr) =>
                {
                    if (options.type == SecondaryAttackType.Projectile)
                    {
                        UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                        {
                            damageBlunt = damageBlunt.Value,
                        });
                    }
                    else if (options.type == SecondaryAttackType.Humanoid)
                    {
                        UpdateHelper.UpdateHumanoidAttackItemData(options.secondPrefab, new UpdateItemDataOptions()
                        {
                            damageBlunt = damageBlunt.Value,
                        });
                    }
                };
            }

            if (options.damageChop != null)
            {
                damageChop = Config.Bind(new ConfigDefinition(options.sectionName, "Chop damage"), (float)options.damageChop,
                    new ConfigDescription("The chop damage on the " + entityDesc + " (not visible ingame, effects wood cutting damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageChop.SettingChanged += (obj, attr) =>
                {
                    if (options.type == SecondaryAttackType.Projectile)
                    {
                        UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                        {
                            damageChop = damageChop.Value,
                        });
                    }
                    else if (options.type == SecondaryAttackType.Humanoid)
                    {
                        UpdateHelper.UpdateHumanoidAttackItemData(options.secondPrefab, new UpdateItemDataOptions()
                        {
                            damageChop = damageChop.Value,
                        });
                    }
                };
            }

            if (options.damagePickaxe != null)
            {
                damagePickaxe = Config.Bind(new ConfigDefinition(options.sectionName, "Pickaxe damage"), (float)options.damagePickaxe,
                    new ConfigDescription("The pickaxe damage on the " + entityDesc + " (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePickaxe.SettingChanged += (obj, attr) =>
                {
                    if (options.type == SecondaryAttackType.Projectile)
                    {
                        UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                        {
                            damagePickaxe = damagePickaxe.Value,
                        });
                    }
                    else if (options.type == SecondaryAttackType.Humanoid)
                    {
                        UpdateHelper.UpdateHumanoidAttackItemData(options.secondPrefab, new UpdateItemDataOptions()
                        {
                            damagePickaxe = damagePickaxe.Value,
                        });
                    }
                };
            }

            if (options.damagePoison != null)
            {
                damagePoison = Config.Bind(new ConfigDefinition(options.sectionName, "Poison damage"), (float)options.damagePoison,
                    new ConfigDescription("The poison damage on the " + entityDesc + " (not visible ingame, effects mining damage)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damagePoison.SettingChanged += (obj, attr) =>
                {
                    if (options.type == SecondaryAttackType.Projectile)
                    {
                        UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                        {
                            damagePoison = damagePoison.Value,
                        });
                    }
                    else if (options.type == SecondaryAttackType.Humanoid)
                    {
                        UpdateHelper.UpdateHumanoidAttackItemData(options.secondPrefab, new UpdateItemDataOptions()
                        {
                            damagePoison = damagePoison.Value,
                        });
                    }
                };
            }

            if (options.damageSpirit != null)
            {
                damageSpirit = Config.Bind(new ConfigDefinition(options.sectionName, "Spirit damage"), (float)options.damageSpirit,
                    new ConfigDescription("The spirit damage on the " + entityDesc + " (not visible ingame)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                damageSpirit.SettingChanged += (obj, attr) =>
                {
                    if (options.type == SecondaryAttackType.Projectile)
                    {
                        UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                        {
                            damageSpirit = damageSpirit.Value,
                        });
                    }
                    else if (options.type == SecondaryAttackType.Humanoid)
                    {
                        UpdateHelper.UpdateHumanoidAttackItemData(options.secondPrefab, new UpdateItemDataOptions()
                        {
                            damageSpirit = damageSpirit.Value,
                        });
                    }
                };
            }

            if (options.attackForce != null)
            {
                attackForce = Config.Bind(new ConfigDefinition(options.sectionName, "Attack force"), (float)options.attackForce,
                    new ConfigDescription("The attack force of the " + entityDesc + " applied to targets hit", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                attackForce.SettingChanged += (obj, attr) =>
                {
                    if (options.type == SecondaryAttackType.Projectile)
                    {
                        UpdateHelper.UpdateProjectile(options.secondPrefab, new UpdateProjectileOptions()
                        {
                            attackForce = attackForce.Value,
                        });
                    }
                    else if (options.type == SecondaryAttackType.Humanoid)
                    {
                        UpdateHelper.UpdateHumanoidAttackItemData(options.secondPrefab, new UpdateItemDataOptions()
                        {
                            attackForce = attackForce.Value,
                        });
                    }
                };
            }

            if (options.minToSpawn != null)
            {
                minToSpawn = Config.Bind(new ConfigDefinition(options.sectionName, "Minimum to spawn"), (int)options.minToSpawn,
                    new ConfigDescription("The minimum amount of creature(s) that will be summoned", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                minToSpawn.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions()
                    {
                        minToSpawn = minToSpawn.Value,
                    });
                };
            }

            if (options.maxToSpawn != null)
            {
                maxToSpawn = Config.Bind(new ConfigDefinition(options.sectionName, "Maximum to spawn"), (int)options.maxToSpawn,
                    new ConfigDescription("The maximum amount of creature(s) that will be summoned", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                maxToSpawn.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions()
                    {
                        maxToSpawn = maxToSpawn.Value,
                    });
                };
            }

            if (options.maxSpawns != null)
            {
                maxSpawns = Config.Bind(new ConfigDefinition(options.sectionName, "Maximum spawns"), (int)options.maxSpawns,
                    new ConfigDescription("The maximum amount of summoned creature(s)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                maxSpawns.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions()
                    {
                        maxSpawns = maxSpawns.Value,
                    });
                };
            }

            if (options.spawnRadius != null)
            {
                spawnRadius = Config.Bind(new ConfigDefinition(options.sectionName, "Spawn radius"), (float)options.spawnRadius,
                    new ConfigDescription("The radius in which the creatures(s) will be summoned", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                spawnRadius.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateSpawnAbility(options.prefab, new UpdateSpawnAbilityOptions()
                    {
                        spawnRadius = spawnRadius.Value,
                    });
                };
            }

            if (options.launchAngle != null)
            {
                launchAngle = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile launch angle"), (float)options.launchAngle,
                new ConfigDescription("The angle the projectile is emitted from the staff", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                launchAngle.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        secondaryLaunchAngle = launchAngle.Value,
                    });
                };
            }

            if (options.projectileVelocity != null)
            {
                projectileVelocity = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile velocity"), (float)options.projectileVelocity,
                new ConfigDescription("The velocity of the projectile emitted from the staff", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                projectileVelocity.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        secondaryProjectileVelocity = projectileVelocity.Value,
                    });
                };
            }

            if (options.projectileAccuracy != null)
            {
                projectileAccuracy = Config.Bind(new ConfigDefinition(options.sectionName, "Projectile accuracy"), (float)options.projectileAccuracy,
                new ConfigDescription("The accuracy (spread) of the projectile emitted from the staff", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, Order = HandleOrder() }));
                projectileAccuracy.SettingChanged += (obj, attr) =>
                {
                    UpdateHelper.UpdateItemData(options.prefab, new UpdateItemDataOptions()
                    {
                        secondaryProjectileAccuracy = projectileAccuracy.Value,
                    });
                };
            }
        }

        private int HandleOrder()
        {
            entryCount = entryCount - 1;
            return entryCount < 0 ? 0 : entryCount;
        }
    }
}
