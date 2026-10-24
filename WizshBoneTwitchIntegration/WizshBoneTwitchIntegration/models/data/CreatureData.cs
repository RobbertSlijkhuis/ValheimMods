using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class CreatureData : CloneableData
    {
        public string announceMessage;
        public bool allowDrops = false;
        public int amount = 1;
        public bool commandable = false;

        public string color = null;

        public string emissionColor = null;
        public float damageScale = 0;
        public bool friendly = false;
        public string globalKeyAdd = "";
        public string globalKeyRemove = "";
        // The creature's Character group (m_group): creatures sharing a non-empty group never treat each
        // other as enemies, even when their factions would. Null = keep the prefab's own group.
        public string group = null;
        public float healthScale = 0;
        public bool isBoss = false;
        public int level = 1;
        public float maxHealth = 0;
        public int maxSpawned = 0;
        public bool mistVision = true;
        public string name;

        public string prefabName;
        public bool facePlayer = false;
        public string position = SpawnPositionType.Random;
        public PositionOffsetData positionOffset = new PositionOffsetData();
        public float positionRadius = 10f;
        public bool rename = true;
        public float talkInterval = 0;
        public string talkMessage;
        public bool talks = false;
        public float size = 1f;
        public float speedMultiplier = 1f;

        public float idleSoundInterval = 0;

        public bool muteIdleSound = false;

        public bool allowDamageStructures = true;
        public string bossEvent = "";
        public bool talkInteract = false;

        // When set, forces the color override above to win even when the redeemer is also a
        // registered viewer with their own personal color - normal GUI-authored redeems leave
        // this false, so a viewer's own color still takes priority over an incidental color set
        // there.
        public bool forceColor = false;

        // Vanilla item prefab names to equip on the creature right after spawn (e.g. "Tankard_dvergr",
        // "HelmetCelebration"). Each must be an ItemDrop prefab; the slot is picked from the item's own
        // type and replaces whatever was there. See CreatureGearHelper.
        public List<string> equipItems = new List<string>();

        // Parts of the creature's default loadout to remove before equipItems is applied, by
        // ItemDrop.ItemData.ItemType name (Shield, Helmet, Chest, Legs, Shoulder, Utility, Trinket, ...)
        // plus "Weapon" (any weapon type, equipped or not) and "All". Removed items leave the creature's
        // inventory, so its AI can't re-equip them. See CreatureGearHelper.
        public List<string> removeEquipment = new List<string>();

        // Creature-behavior feature. Forces a friendly
        // creature to always follow the player, independent of the commandable/isFollowing toggle,
        // and to keep closing the distance instead of stopping ~3m out like a normal followed
        // creature. See TwitchCreaturePersistentData.ApplyTameable/WantsToCloseDistance and
        // harmony/SpecialRedeemPatchesWBTI.cs's Follow prefix.
        public bool alwaysFollowOwner = false;

        // Creature-behavior feature. Makes a tamed
        // creature never treat anything as an enemy in either direction - it never targets anything
        // (a tamed creature normally still fights off wild monsters that get close, see
        // BaseAI.IsEnemy's tamed-vs-non-tamed branch), and wild monsters never target it back either.
        // See TwitchCreaturePersistentData.IsFullyPassive and harmony/SpecialRedeemPatchesWBTI.cs.
        public bool fullyPassive = false;

        public CreatureData() { }
    }
}
