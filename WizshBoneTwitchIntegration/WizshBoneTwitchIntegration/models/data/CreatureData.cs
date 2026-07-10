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

        [EditorLabel("Idle sound interval")]
        [EditorTooltip("Overrides the interval (seconds) between the creature's idle sound effect (e.g. a Fuling's laugh), and makes it always play instead of the default ~50% chance. Leave at 0 to use the creature's default behavior.")]
        public float idleSoundInterval = 0;

        [EditorHidden] public bool allowDamageStructures = true;
        [EditorHidden] public string bossEvent = "";
        [EditorHidden] public bool isHallucination = false;
        [EditorHidden] public int index;
        [EditorHidden] public bool talkInteract = false;

        // Scripted/hardcoded redeems (see SpecialRedeemHelper) set this to force their color
        // choice to win even when the redeemer is also a registered special viewer with their
        // own personal color - normal GUI-authored redeems leave this false, so a special
        // viewer's own color still takes priority over an incidental color set there.
        [EditorHidden] public bool forceColor = false;

        // Temporary hack: lets SpawnCreature spawn non-creature prefabs (e.g. a hot tub) that
        // have no MonsterAI/Humanoid components. See CreatureHelper.SpawnCreature.
        [EditorHidden] public bool requireMonsterComponents = true;

        // Temporary hack: if > 0, fills the spawned prefab's Smelter (e.g. the hot tub's fuel
        // tank) up to this amount (clamped to Smelter.m_maxFuel) right after spawn.
        [EditorHidden] public float smelterFuelAmount = 0f;

        public CreatureData() { }
    }
}
