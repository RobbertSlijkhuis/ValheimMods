using ModularMagic_Core.Models;
using ModularMagic_Core.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Applies the imbuements (runes) of a staff to its item data, for every staff mod. The mod describes its staffs in a StaffDefinition and
    /// registers it once; Core then applies the runes whenever a staff is equipped, speeds up the attack animation for the Attack Speed rune,
    /// and scales summoned creatures with the staff that summoned them.
    /// </summary>
    public static class StaffImbuementHelper
    {
        /// <summary>
        /// Registers the staffs of a mod. Call it once at startup (in Awake), after the config is bound and the prefabs are loaded.
        /// </summary>
        public static void Register(StaffDefinition definition)
        {
            ImbuedStaffHelper.Register(definition.IsStaff, item => Apply(definition, item));

            if (definition.attackSpeedAnimation)
                AttackSpeedHelper.Register(weapon => GetAttackSpeedFactor(definition, weapon));

            foreach (SecondaryAttackEntry entry in definition.secondaryAttacks.Values)
            {
                if (entry.creature != null)
                {
                    SecondaryAttackEntry summon = entry;
                    SpawnedCreatureHelper.Register(summon.creature.name, (creature, staff) => ScaleSummon(definition, summon, creature, staff));
                }
            }
        }

        /// <summary>
        /// Applies the runes of the staff to its item data. The base stats come from the config, so the runes are always applied on top of the
        /// current base stats.
        /// </summary>
        public static void Apply(StaffDefinition definition, ItemData itemData)
        {
            List<Imbuement> imbuements = ImbuementHelper.Read(itemData);
            ItemDataSnapShot snapshot = definition.GetSnapshot(itemData.m_dropPrefab.name);

            if (snapshot == null)
                throw new Exception("Snapshot is null");

            // The attack of the prefab is copied for the staff, the base stats are read from the config below
            ItemData baseData = snapshot.mainAttack;

            // An item made with Inventory.AddItem(prefab, amount) shares its shared data with the prefab. Changing it would
            // put the runes (for example the secondary attack) on every new staff of this type, so leave such an item as it is
            if (ReferenceEquals(itemData.m_shared, baseData.m_shared))
            {
                Jotunn.Logger.LogWarning($"[Imbuements] {itemData.m_dropPrefab.name} shares its data with the prefab, the runes are not applied");
                return;
            }

            Configs.ImbuementConfig runes = definition.imbuementConfig;

            // The cone rune replaces the main attack of the staff. The cone costs a part of the eitr of the staff per burst, has its own burst interval
            // and fires an AOE, so the accuracy and speed of the staff do not apply to it (the Eitr Cost rune still lowers the eitr of the cone,
            // the Attack Speed rune is ignored)
            bool cone = definition.coneAttack != null && imbuements.Exists(i => i.type == ImbuementType.MainAttack && i.value == "Cone");
            ItemData mainAttack = cone ? definition.coneAttack.GetComponent<ItemDrop>().m_itemData : baseData;

            // The base stats come from the config, which also holds the values a server syncs
            float eitr = cone ? snapshot.attackEitr * definition.coneConfig.eitrMultiplier.Value : snapshot.attackEitr;
            float accuracy = snapshot.projectileAccuracy;
            float velocity = snapshot.projectileVelocity;

            // Only the cone fires bursts when the Attack Speed rune speeds up the animation. Otherwise the rune lowers the burst interval
            float burst = cone ? definition.coneConfig.burstInterval.Value : (definition.attackSpeedAnimation ? 0f : snapshot.projectileBurst);

            // The runes change the type of the physical damage, the other damage of the staff is never touched
            string physicalType = definition.physicalDamageType;

            // The secondary attack, only when the staff has the rune
            ItemData secondaryAttack = null;
            float? secondaryEitr = null;

            foreach (Imbuement imbuement in imbuements)
            {
                switch (imbuement.type)
                {
                    case ImbuementType.DamageType:
                        if (physicalType != null && (imbuement.value == "Blunt" || imbuement.value == "Pierce" || imbuement.value == "Slash"))
                            physicalType = imbuement.value;
                        break;
                    case ImbuementType.EitrCost:
                        eitr -= runes.EitrCost.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileAccuracy:
                        accuracy -= runes.ProjectileAccuracy.Value * imbuement.level;
                        break;
                    case ImbuementType.AttackSpeed:
                        // The cone is timed by its own burst interval, so the rune only applies to the normal attack
                        if (!definition.attackSpeedAnimation && !cone)
                            burst -= runes.AttackSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileVelocity:
                        velocity += runes.ProjectileSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.SecondaryAttack:
                        if (definition.secondaryAttacks.TryGetValue(imbuement.value, out SecondaryAttackEntry entry))
                        {
                            secondaryAttack = entry.attack.GetComponent<ItemDrop>().m_itemData;

                            // Every level above 1 saves a part of the eitr cost. The base cost is a multiplier of the eitr of the staff, or the cost of
                            // the attack itself. Both are read again on every equip, so a changed or synced config value is used
                            float saved = Mathf.Clamp01(runes.SecondaryAttackEitr.Value * (imbuement.level - 1));
                            float baseEitr = entry.config.eitrMultiplier != null
                                ? snapshot.attackEitr * entry.config.eitrMultiplier.Value
                                : secondaryAttack.m_shared.m_attack.m_attackEitr;

                            secondaryEitr = baseEitr * (1f - saved);
                        }
                        break;
                }
            }

            UpdateItemDataOptions options = new UpdateItemDataOptions()
            {
                mainAttack = mainAttack,
                // A rune can not make the eitr cost, the spread or the burst interval negative
                attackEitr = Mathf.Max(0f, eitr),
                // The cone is not a projectile, so the accuracy and the velocity of the cone attack stay as they are
                projectileAccuracy = cone ? (float?)null : Mathf.Max(0f, accuracy),
                projectileVelocity = cone ? (float?)null : velocity,
                burstInterval = definition.attackSpeedAnimation && !cone ? (float?)null : Mathf.Max(0f, burst),
                secondaryAttack = secondaryAttack,
                secondaryAttackEitr = secondaryEitr,
                equipStatusEffect = CreateEquipStatusEffect(definition, imbuements, cone),
            };

            if (physicalType != null)
            {
                bool pierceBase = definition.physicalDamageType == "Pierce";
                float damage = pierceBase ? snapshot.damagePierce : snapshot.damageBlunt;
                float damagePerLevel = pierceBase ? snapshot.damagePiercePerLevel : snapshot.damageBluntPerLevel;

                options.damageBlunt = physicalType == "Blunt" ? damage : 0f;
                options.damagePierce = physicalType == "Pierce" ? damage : 0f;
                options.damageSlash = physicalType == "Slash" ? damage : 0f;
                options.damageBluntPerLevel = physicalType == "Blunt" ? damagePerLevel : 0f;
                options.damagePiercePerLevel = physicalType == "Pierce" ? damagePerLevel : 0f;
                options.damageSlashPerLevel = physicalType == "Slash" ? damagePerLevel : 0f;

                // The projectile that shows the damage type. Every damage type has its own prefab, so all clients spawn the same visual.
                // The cone main attack has its own projectile, so the damage type projectile only replaces the default one
                if (!cone && definition.damageProjectiles.TryGetValue(physicalType, out GameObject damageProjectile))
                    options.attackProjectile = damageProjectile;
            }

            Jotunn.Logger.LogWarning($"[Imbuements] {itemData.m_dropPrefab.name}: main {(cone ? "cone" : "default")}{(physicalType != null ? ", " + physicalType : "")}, eitr {options.attackEitr}, accuracy {(options.projectileAccuracy == null ? "default" : options.projectileAccuracy.ToString())}, burst {(options.burstInterval == null ? "default" : options.burstInterval.ToString())}, speed {(options.projectileVelocity == null ? "default" : options.projectileVelocity.ToString())}, secondary eitr {(options.secondaryAttackEitr == null ? "none" : options.secondaryAttackEitr.ToString())}");

            StatusEffect previousEquipEffect = itemData.m_shared.m_equipStatusEffect;

            UpdateHelper.UpdateItemData(itemData, options);

            DestroyReplacedEquipStatusEffect(definition, previousEquipEffect, itemData);
        }

        // How much faster the animation of the normal attack of this weapon is, 1 is the normal speed
        private static float GetAttackSpeedFactor(StaffDefinition definition, ItemData weapon)
        {
            if (!definition.IsStaff(weapon) || !ImbuementHelper.HasImbuements(weapon))
                return 1f;

            int levels = 0;

            foreach (Imbuement imbuement in ImbuementHelper.Read(weapon))
            {
                if (imbuement.type == ImbuementType.AttackSpeed)
                    levels += imbuement.level;
            }

            return 1f + definition.imbuementConfig.AttackSpeedAnimation.Value * levels;
        }

        // The tooltip of the staff lists the runes
        private static StatusEffect CreateEquipStatusEffect(StaffDefinition definition, List<Imbuement> imbuements, bool cone)
        {
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

            StatusEffect effect = ScriptableObject.CreateInstance<StatusEffect>();
            effect.name = definition.effectName;
            effect.m_name = "Imbuements";
            effect.m_tooltip = string.Join("\n", tooltipLines);

            return effect;
        }

        // Every apply makes a new effect for the tooltip, so the one of the previous apply is destroyed. Only an effect of this mod is destroyed.
        // The player keeps a copy of the effect it runs, and the game only reads the name hash of the old one, which it cached when the effect was added
        private static void DestroyReplacedEquipStatusEffect(StaffDefinition definition, StatusEffect previous, ItemData itemData)
        {
            if (previous == null || previous.name != definition.effectName || ReferenceEquals(previous, itemData.m_shared.m_equipStatusEffect))
                return;

            UnityEngine.Object.Destroy(previous);
        }

        // A summoned creature deals the damage of its own weapon (set in its prefab), times a factor that the game multiplies into every attack
        // of the creature. The factor makes the main damage of the creature the main damage of the staff that summoned it (quality included)
        // times the damage multiplier of the summon. The factor is stored in the ZDO of the creature, so it is the same on every client, also
        // when another client becomes the owner of the creature
        private static void ScaleSummon(StaffDefinition definition, SecondaryAttackEntry summon, Character creature, ItemData staff)
        {
            if (staff == null || !definition.IsStaff(staff) || summon.config.damageMultiplier == null)
                return;

            Humanoid humanoid = summon.creature.GetComponent<Humanoid>();

            if (humanoid == null || humanoid.m_randomWeapon == null || humanoid.m_randomWeapon.Length == 0 || humanoid.m_randomWeapon[0] == null)
            {
                Jotunn.Logger.LogWarning($"[Summon] {summon.creature.name} has no weapon, its damage is not scaled");
                return;
            }

            float templateDamage = DamageHelper.GetMainPhysical(humanoid.m_randomWeapon[0].GetComponent<ItemDrop>().m_itemData.m_shared.m_damages);

            if (templateDamage <= 0f)
            {
                Jotunn.Logger.LogWarning($"[Summon] The weapon of {summon.creature.name} has no blunt, pierce or slash damage, its damage is not scaled");
                return;
            }

            float staffDamage = DamageHelper.GetMainPhysical(staff.GetDamage());

            SpawnedCreatureHelper.SetDamage(creature, staffDamage * summon.config.damageMultiplier.Value, templateDamage);
        }
    }
}
