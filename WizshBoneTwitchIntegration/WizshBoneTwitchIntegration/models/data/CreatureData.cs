using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class CreatureData : CloneableData
    {
        public string announceMessage;
        public bool aggravatable = false;
        public bool allowDrops = false;
        public int amount = 1;
        public bool commandable = false;

        [EditorLabel("Creature color")]
        [EditorTooltip("Overrides the creature's color in hex format (e.g. #ffffff for white). Leave blank for default.")]
        [ColorPicker]
        public string color = null;
        public float damageScale = 0;
        public bool friendly = false;
        public string globalKeyAdd = "";
        public string globalKeyRemove = "";
        public string group = null;
        public float healthScale = 0;
        public bool isBoss = false;
        public int level = 1;
        public float maxHealth = 0;
        public int maxSpawned = 0;
        public bool mistVision = true;
        public string name;

        [CreaturePrefabNameDropdown]
        public string prefabName;
        public string position = SpawnPositionType.Random;
        public PositionOffsetData positionOffset = new PositionOffsetData();
        public float positionRadius = 10f;
        public bool random = false;
        public bool rename = true;
        public float talkInterval = 0;
        public string talkMessage;
        public bool talks = false;
        public float size = 1f;
        public float speedMultiplier = 1f;

        [EditorLabel("Idle sound interval")]
        [EditorTooltip("Overrides the interval (seconds) between the creature's idle sound effect (e.g. a Fuling's laugh), and makes it always play instead of the default ~50% chance. Leave at 0 to use the creature's default behavior.")]
        public float idleSoundInterval = 0;

        [EditorHidden] public bool allowDamageStructures = true;
        [EditorHidden] public string bossEvent = "";
        [EditorHidden] public bool isHallucination = false;
        [EditorHidden] public int index;
        [EditorHidden] public bool talkInteract = false;

        // When set, forces the color override above to win even when the redeemer is also a
        // registered viewer with their own personal color - normal GUI-authored redeems leave
        // this false, so a viewer's own color still takes priority over an incidental color set
        // there.
        [EditorHidden] public bool forceColor = false;

        // Temporary hack: lets SpawnCreature spawn non-creature prefabs (e.g. a hot tub) that
        // have no MonsterAI/Humanoid components. See CreatureHelper.SpawnCreature.
        [EditorHidden] public bool requireMonsterComponents = true;

        // Temporary hack: if > 0, fills the spawned prefab's Smelter (e.g. the hot tub's fuel
        // tank) up to this amount (clamped to Smelter.m_maxFuel) right after spawn.
        [EditorHidden] public float smelterFuelAmount = 0f;

        // SAPHONETTE-CLEANUP: optional prefab name of a vanilla item to equip in the creature's
        // helmet slot right after spawn (e.g. "HelmetCelebration"). Must be an ItemDrop prefab with
        // ItemType.Helmet. Leave null/empty to spawn with no helmet override. See
        // CreatureHelper.EquipGear. Remove once the "Don't forget your coffee!" bit is over.
        [EditorHidden] public string helmetItem = null;

        // SAPHONETTE-CLEANUP: optional prefab name of a vanilla item to equip in the creature's
        // hand right after spawn (e.g. "Tankard_dvergr"). Works for any equippable one-handed/tool
        // item; not intended for real weapons since a fullyPassive creature never attacks and a
        // non-passive one could still swap it out mid-combat during AI weapon selection (see
        // CreatureHelper.EquipGear). Leave null/empty to spawn with no held item. Remove once the
        // "Don't forget your coffee!" bit is over.
        [EditorHidden] public string heldItem = null;

        // SAPHONETTE-CLEANUP: if true, unequips whatever the creature's own default loadout put in
        // its off-hand/shield slot right after spawn (e.g. a Draugr's random default shield).
        // Independent of heldItem - a redeem can hold something in the main hand and still keep the
        // shield if desired. See CreatureHelper.EquipGear. Remove once the "Don't forget your
        // coffee!" bit is over.
        [EditorHidden] public bool removeShield = false;

        // Permanent creature-behavior feature (currently only wired up for the coffee draugr, but
        // not tied to that bit going away). Forces a friendly creature to always follow the player,
        // independent of the commandable/isFollowing toggle, and to keep closing the distance
        // instead of stopping ~3m out like a normal followed creature. See
        // TwitchCreaturePersistentData.ApplyTameable/WantsToCloseDistance and
        // harmony/SpecialRedeemPatchesWBTI.cs's Follow prefix.
        [EditorHidden] public bool alwaysFollowOwner = false;

        // Permanent creature-behavior feature (currently only wired up for the coffee draugr, but
        // not tied to that bit going away). Makes a tamed creature never treat anything as an enemy
        // in either direction - it never targets anything (a tamed creature normally still fights
        // off wild monsters that get close, see BaseAI.IsEnemy's tamed-vs-non-tamed branch), and
        // wild monsters never target it back either. See TwitchCreaturePersistentData.IsFullyPassive
        // and harmony/SpecialRedeemPatchesWBTI.cs.
        [EditorHidden] public bool fullyPassive = false;

        public CreatureData() { }
    }
}
