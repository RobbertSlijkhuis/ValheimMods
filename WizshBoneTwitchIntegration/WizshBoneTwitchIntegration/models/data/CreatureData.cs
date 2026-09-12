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
        public bool facePlayer = false;
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

        // Permanent creature-behavior feature, not currently set by any redeem. Forces a friendly
        // creature to always follow the player, independent of the commandable/isFollowing toggle,
        // and to keep closing the distance instead of stopping ~3m out like a normal followed
        // creature. See TwitchCreaturePersistentData.ApplyTameable/WantsToCloseDistance and
        // harmony/SpecialRedeemPatchesWBTI.cs's Follow prefix.
        [EditorHidden] public bool alwaysFollowOwner = false;

        // Permanent creature-behavior feature, not currently set by any redeem. Makes a tamed
        // creature never treat anything as an enemy in either direction - it never targets anything
        // (a tamed creature normally still fights off wild monsters that get close, see
        // BaseAI.IsEnemy's tamed-vs-non-tamed branch), and wild monsters never target it back either.
        // See TwitchCreaturePersistentData.IsFullyPassive and harmony/SpecialRedeemPatchesWBTI.cs.
        [EditorHidden] public bool fullyPassive = false;

        public CreatureData() { }
    }
}
