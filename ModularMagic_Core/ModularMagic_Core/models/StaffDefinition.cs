using ModularMagic_Core.Components;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Helpers;
using ModularMagic_Core.Types;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// A secondary attack of a staff mod that a rune gives to the staff.
    /// </summary>
    public class SecondaryAttackEntry
    {
        // The value of the rune that gives the attack ("Nova", "Rain", "Summon")
        public string runeValue;
        // The wrapper prefab that holds the attack (its attack is copied to the staff)
        public GameObject attack;
        public SecondaryAttackConfig config;
        // Only for an attack that summons a creature: the prefab of the creature. Its damage scales with the staff that cast the summon
        public GameObject creature;
    }

    /// <summary>
    /// A main attack rune that keeps the attack of the staff (its damage, reach, angle and costs) and only changes how it is swung: the
    /// animation of the attack, the number of hits of its chain and how the weapon is held.
    /// </summary>
    public class MainAttackAnimationEntry
    {
        // The value of the rune ("Greatsword")
        public string runeValue;
        // The trigger of the attack animation (the game adds the number of the hit in the chain, "greatsword" plays "greatsword0" and on)
        public string attackAnimation;
        public int chainLevels;
        // How the character holds the weapon while the rune is slotted
        public ItemData.AnimationState animationState;
    }

    /// <summary>
    /// What a staff mod tells Core about its staffs, see StaffImbuementHelper.Register. Core then handles the staffs: it gives them their
    /// rune slots, applies the runes when a staff is equipped, speeds up the attack animation and scales summoned creatures. The mod only
    /// loads its prefabs and fills in this definition.
    /// </summary>
    public class StaffDefinition
    {
        // The name of the status effect that shows the runes in the tooltip of a staff, unique for the mod (for example "Imbuements_MMFS")
        public string effectName;
        // The suffix of the rune texts of the staffs, see WeaponType
        public string weaponType = WeaponType.None;
        // The colour of the rune table while one of the staffs is on it, see LightColorPresetType
        public string colorPreset = LightColorPresetType.None;
        // The ids of the runes the staffs support, see RuneId
        public string[] allowedRunes = new string[0];
        public ImbuementConfig imbuementConfig;

        // True when the Attack Speed rune speeds up the attack animation (Fire, Lightning), false when it lowers the burst interval of the
        // attack (Ice, Earth)
        public bool attackSpeedAnimation = false;

        // The cone rune: the wrapper prefab that holds the cone attack, and its config. Null for a mod without a cone
        public GameObject coneAttack;
        public MainAttackConfig coneConfig;

        // The physical damage type the staffs deal ("Blunt", "Pierce"). A damage type rune moves the main damage of a staff to another type.
        // Null for a mod without damage type runes, the damage of its staffs is then left as it is
        public string physicalDamageType;
        // The projectile prefab to fire for a damage type (by the name of the type), when it is not the projectile of the staff
        public Dictionary<string, GameObject> damageProjectiles = new Dictionary<string, GameObject>();

        internal readonly Dictionary<string, SecondaryAttackEntry> secondaryAttacks = new Dictionary<string, SecondaryAttackEntry>();
        internal readonly Dictionary<string, MainAttackAnimationEntry> mainAttackAnimations = new Dictionary<string, MainAttackAnimationEntry>();
        private readonly Dictionary<string, ItemDataSnapShot> snapshots = new Dictionary<string, ItemDataSnapShot>();

        /// <summary>
        /// Adds a secondary attack that a rune gives to the staffs.
        /// </summary>
        public void AddSecondaryAttack(string runeValue, GameObject attack, SecondaryAttackConfig config, GameObject creature = null)
        {
            secondaryAttacks[runeValue] = new SecondaryAttackEntry() { runeValue = runeValue, attack = attack, config = config, creature = creature };
        }

        /// <summary>
        /// Adds a main attack rune that swings the staff with other animations (for example the Greatsword rune). The attack of the staff
        /// stays the same, only the animation, the number of hits of the chain and how the staff is held change. The rune has the type
        /// MainAttack, so it can not be slotted together with the Cone rune.
        /// </summary>
        public void AddMainAttackAnimation(string runeValue, string attackAnimation, int chainLevels, ItemData.AnimationState animationState)
        {
            mainAttackAnimations[runeValue] = new MainAttackAnimationEntry()
            {
                runeValue = runeValue,
                attackAnimation = attackAnimation,
                chainLevels = chainLevels,
                animationState = animationState,
            };
        }

        /// <summary>
        /// Adds a staff: its rune slots, its item with the recipe and stats of its config, and the base stats the runes are applied on top of.
        /// Call it when the vanilla prefabs are available, after the config is bound.
        /// </summary>
        public void AddStaff(GameObject prefab, StaffConfig config, int slots, int tier)
        {
            ImbuementSlots imbuementSlots = prefab.AddComponent<ImbuementSlots>();
            imbuementSlots.m_slots = slots;
            imbuementSlots.m_tier = tier;
            imbuementSlots.m_weaponType = weaponType;
            imbuementSlots.m_allowedRunes = new List<string>(allowedRunes);
            imbuementSlots.m_colorPreset = colorPreset;

            StaffHelper.CreateStaff(prefab, config);

            snapshots[prefab.name] = new ItemDataSnapShot(prefab.GetComponent<ItemDrop>().m_itemData, config);
        }

        /// <summary>
        /// Whether an item is one of the imbuable staffs of this mod. Other staff mods share the imbuement API, so each only handles its own.
        /// </summary>
        public bool IsStaff(ItemData itemData)
        {
            return itemData != null && itemData.m_dropPrefab != null && snapshots.ContainsKey(itemData.m_dropPrefab.name);
        }

        public ItemDataSnapShot GetSnapshot(string prefabName)
        {
            return snapshots.TryGetValue(prefabName, out ItemDataSnapShot snapshot) ? snapshot : null;
        }
    }
}
