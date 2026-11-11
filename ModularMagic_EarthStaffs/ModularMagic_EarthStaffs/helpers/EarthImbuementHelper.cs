using ModularMagic_Core.Types;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using Imbuement = ModularMagic_Core.Models.Imbuement;

namespace ModularMagic_EarthStaffs.Helpers
{
    /// <summary>
    /// Applies the imbuements of an Earth staff (read through the shared Core API) to its item data.
    /// What a rune does is specific to the staff type, so this stays in the staff mod.
    /// </summary>
    internal class EarthImbuementHelper
    {
        // The runes the Earth staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.DamagePierce, RuneId.DamageSlash, RuneId.EitrCost, RuneId.ProjectileAccuracy, RuneId.AttackSpeed,
            RuneId.ProjectileSpeed, RuneId.Cone, RuneId.Creatures, RuneId.Nova, RuneId.Rain,
        };

        // Set when the game has started, the equip patch also runs while the player is being loaded
        public static bool GameIsReady = false;

        // Other staff mods share the imbuement API, so only handle the staffs of this mod
        public static bool IsEarthStaff(ItemData itemData)
        {
            return itemData != null && itemData.m_dropPrefab != null && GetItemDataSnapshot(itemData.m_dropPrefab.name) != null;
        }

        // Applies the runes again on the staff in the hand, so a changed config value reaches it together with the runes
        public static void ReapplyOnHeldStaff()
        {
            try
            {
                Player player = Player.m_localPlayer;

                if (player == null || !GameIsReady)
                    return;

                ItemData weapon = player.GetCurrentWeapon();

                if (weapon == null || !CoreImbuementHelper.HasImbuements(weapon) || !IsEarthStaff(weapon))
                    return;

                ApplyImbuements(weapon);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not apply the imbuements to the staff in the hand: " + e);
            }
        }

        public static void ApplyImbuements(ItemData itemData)
        {
            List<Imbuement> imbuements = CoreImbuementHelper.Read(itemData);
            ApplyImbuements(itemData, imbuements);
        }

        public static void ApplyImbuements(ItemData itemData, List<Imbuement> imbuements)
        {
            UpdateItemDataOptions options = new UpdateItemDataOptions();
            ItemDataSnapShot snapshot = GetItemDataSnapshot(itemData.m_dropPrefab.name);

            if (snapshot == null)
                throw new Exception("Snapshot is null");

            options.damageBlunt = snapshot.damageBlunt;
            options.damageSlash = 0;
            options.damagePierce = 0;
            options.damageBluntPerLevel = snapshot.damageBluntPerlevel;
            options.damageSlashPerLevel = 0;
            options.damagePiercePerLevel = 0;
            // The cone rune replaces the main attack of the staff. The cone costs a part of the eitr of the staff per burst, has its own burst interval
            // and fires an AOE, so the accuracy and speed of the staff do not apply to it (the Eitr Cost rune still lowers the eitr of the cone,
            // the Attack Speed rune is ignored)
            bool cone = imbuements.Exists(i => i.type == ImbuementType.MainAttack && i.value == "Cone");

            // The base stats come from the config, which also holds the values a server syncs, so the runes are always applied on top of the current base stats
            options.attackEitr = cone ? snapshot.attackEitr * PluginConfig.mainAttackCone.eitrMultiplier.Value : snapshot.attackEitr;
            // options.timedBlockBonus = snapshot.timedBlockBonus;
            options.projectileAccuracy = snapshot.projectileAccuracy;
            options.projectileBurst = cone ? PluginConfig.mainAttackCone.burstInterval.Value : snapshot.projectileBurst;
            options.projectileVelocity = snapshot.projectileVelocity;
            options.mainAttack = snapshot.mainAttack;

            // The projectile that shows the damage type. Every damage type has its own prefab, so all clients spawn the same visual
            GameObject damageProjectile = null;

            foreach (Imbuement imbuement in imbuements)
            {
                switch (imbuement.type)
                {
                    case ImbuementType.DamageType:
                        if (imbuement.value == "Slash")
                        {
                            options.damageSlash = snapshot.damageBlunt;
                            options.damageSlashPerLevel = snapshot.damageBluntPerlevel;
                            damageProjectile = ModularMagic_EarthStaffs.prefabs.ProjectileSlash;
                        }
                        else if (imbuement.value == "Pierce")
                        {
                            options.damagePierce = snapshot.damageBlunt;
                            options.damagePiercePerLevel = snapshot.damageBluntPerlevel;
                            damageProjectile = ModularMagic_EarthStaffs.prefabs.ProjectilePierce;
                        }

                        options.damageBlunt = 0;
                        options.damageBluntPerLevel = 0;
                        break;
                    case ImbuementType.EitrCost:
                        options.attackEitr -= PluginConfig.imbuementConfig.EitrCost.Value * imbuement.level;
                        break;
                    //case ImbuementType.ParryBonus:
                    //    options.timedBlockBonus += float.Parse(imbuement.value, CultureInfo.InvariantCulture);
                    //    break;
                    case ImbuementType.ProjectileAccuracy:
                        options.projectileAccuracy -= PluginConfig.imbuementConfig.ProjectileAccuracy.Value * imbuement.level;
                        break;
                    case ImbuementType.AttackSpeed:
                        // The cone is timed by its own burst interval, so the rune only applies to the normal attack
                        if (!cone)
                            options.projectileBurst -= PluginConfig.imbuementConfig.AttackSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileVelocity:
                        options.projectileVelocity += PluginConfig.imbuementConfig.ProjectileSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.MainAttack:
                        if (imbuement.value == "Cone")
                            options.mainAttack = ModularMagic_EarthStaffs.prefabs.mainAttackCone.GetComponent<ItemDrop>().m_itemData;
                        break;
                    case ImbuementType.SecondaryAttack:
                        if (imbuement.value == "Nova")
                            options.secondaryAttack = ModularMagic_EarthStaffs.prefabs.SecondaryAttackNova.GetComponent<ItemDrop>().m_itemData;
                        else if (imbuement.value == "Rain")
                            options.secondaryAttack = ModularMagic_EarthStaffs.prefabs.SecondaryAttackBoulder.GetComponent<ItemDrop>().m_itemData;
                        else if (imbuement.value == "Summon")
                            options.secondaryAttack = ModularMagic_EarthStaffs.prefabs.SecondaryAttackRoots.GetComponent<ItemDrop>().m_itemData;

                        // Every level above 1 saves a part of the eitr cost. The base cost of the nova and the boulders is a multiplier of the eitr of the staff,
                        // the base cost of the summon is the one of the attack itself. Both are read again on every equip, so a changed or synced config value is used
                        if (options.secondaryAttack != null)
                        {
                            float saved = Mathf.Clamp01(PluginConfig.imbuementConfig.SecondaryAttackEitr.Value * (imbuement.level - 1));
                            float baseEitr = options.secondaryAttack.m_shared.m_attack.m_attackEitr;

                            if (imbuement.value == "Nova")
                                baseEitr = (float)snapshot.attackEitr * PluginConfig.secondaryAttackNova.eitrMultiplier.Value;
                            else if (imbuement.value == "Rain")
                                baseEitr = (float)snapshot.attackEitr * PluginConfig.secondaryAttackRain.eitrMultiplier.Value;

                            options.secondaryAttackEitr = baseEitr * (1f - saved);
                        }
                        break;
                }
            }

            // A rune can not make the eitr cost, the spread or the burst interval negative
            options.attackEitr = Mathf.Max(0f, options.attackEitr.Value);
            options.projectileAccuracy = Mathf.Max(0f, options.projectileAccuracy.Value);
            options.projectileBurst = Mathf.Max(0f, options.projectileBurst.Value);

            // The cone is not a projectile, so the accuracy and the velocity of the cone attack stay as they are
            if (cone)
            {
                options.projectileAccuracy = null;
                options.projectileVelocity = null;
            }

            // The cone main attack has its own projectile, so the damage type projectile only replaces the default one
            if (damageProjectile != null && options.mainAttack == snapshot.mainAttack)
                options.attackProjectile = damageProjectile;

            Jotunn.Logger.LogWarning($"[Imbuements] {itemData.m_dropPrefab.name}: damage (B, P, S) {options.damageBlunt}, {options.damagePierce}, {options.damageSlash}, eitr {options.attackEitr}, accuracy {options.projectileAccuracy}, burst {options.projectileBurst}, speed {options.projectileVelocity}, projectile {(options.attackProjectile != null ? options.attackProjectile.name : "default")}, main {(options.mainAttack == null ? "null" : options.mainAttack.m_shared?.m_attack?.m_attackProjectile?.name)}, secondary {(options.secondaryAttack == null ? "null" : options.secondaryAttack.m_shared?.m_attack?.m_attackProjectile?.name)}, secondary eitr {(options.secondaryAttackEitr == null ? "default" : options.secondaryAttackEitr.ToString())}");

            StatusEffect ImbuementEffect = ScriptableObject.CreateInstance<StatusEffect>();
            List<string> tooltipLines = new List<string>();

            foreach (Imbuement imbuement in imbuements)
            {
                if (imbuement.type == ImbuementType.None)
                    continue;

                // The cone is timed by its own burst interval, so the Attack Speed rune does nothing while it is slotted
                if (cone && imbuement.type == ImbuementType.AttackSpeed)
                {
                    tooltipLines.Add($"<color=red>{imbuement.name} {(imbuement.level > 0 ? imbuement.level : "")} (no effect with the Cone rune)</color>");
                    continue;
                }

                tooltipLines.Add($"<color=green>{imbuement.name} {(imbuement.level > 0 ? imbuement.level : "")} </color>");
            }

            ImbuementEffect.m_tooltip = string.Join("\n", tooltipLines);
            ImbuementEffect.name = "Imbuements_MMES";
            ImbuementEffect.m_name = "Imbuements";
            options.equipStatusEffect = ImbuementEffect;

            StatusEffect previousEquipEffect = itemData.m_shared.m_equipStatusEffect;

            UpdateHelper.UpdateItemData(itemData, options);

            DestroyReplacedEquipStatusEffect(previousEquipEffect, itemData);
        }

        // The script that spawns the boulders (SpawnAbility) does not pass the damage of the staff on, so every boulder it spawns gets the damage of the
        // staff that cast the rain (apart from chop and pickaxe) from the staff (quality and damage type rune included) times the damage multiplier,
        // and the attack force from the attack force of the staff times the force multiplier. It is set on the spawned boulder, not on the prefab,
        // so it never holds the damage of another staff. Called for every projectile a SpawnAbility spawns, see PatchesMMES.
        // The boulders are hit by the client of the shooter, and the chop and pickaxe damage stay config values
        public static void OnProjectileSpawned(Projectile projectile, ItemData staff)
        {
            try
            {
                if (projectile == null || !IsEarthStaff(staff) || projectile.name.Replace("(Clone)", "") != ModularMagic_EarthStaffs.prefabs.ProjectileBoulder.name)
                    return;

                HitData.DamageTypes damage = staff.GetDamage();
                SecondaryAttackConfig rain = PluginConfig.secondaryAttackRain;
                float multiplier = rain.damageMultiplier.Value;

                // A boulder is blunt, whatever the damage type rune of the staff is. Only one of blunt, pierce and slash is set on the staff,
                // they are never added up, the one that is set is the main damage
                float main = Mathf.Max(damage.m_blunt, Mathf.Max(damage.m_pierce, damage.m_slash));

                projectile.m_damage.m_blunt = main * multiplier;
                projectile.m_damage.m_slash = 0f;
                projectile.m_damage.m_pierce = 0f;
                projectile.m_damage.m_spirit = damage.m_spirit * multiplier;
                projectile.m_damage.m_poison = damage.m_poison * multiplier;
                projectile.m_attackForce = staff.m_shared.m_attackForce * rain.forceMultiplier.Value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set the damage of the boulder: " + e);
            }
        }

        // Every apply makes a new effect for the tooltip, so the one of the previous apply is destroyed. Only an effect of this mod is destroyed.
        // The player keeps a copy of the effect it runs, and the game only reads the name hash of the old one, which it cached when the effect was added
        private static void DestroyReplacedEquipStatusEffect(StatusEffect previous, ItemData itemData)
        {
            if (previous == null || previous.name != "Imbuements_MMES" || ReferenceEquals(previous, itemData.m_shared.m_equipStatusEffect))
                return;

            UnityEngine.Object.Destroy(previous);
        }

        private static ItemDataSnapShot GetItemDataSnapshot(string name)
        {
            switch (name)
            {
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth0.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth0;
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth1.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth1;
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth2.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth2;
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth3.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth3;
                default:
                    return null;
            }
        }
    }
}
