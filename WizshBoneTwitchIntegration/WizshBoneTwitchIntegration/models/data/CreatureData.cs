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
        public int talkInterval = 0;
        public string talkMessage;
        public bool talks = false;
        public float size = 1f;

        [EditorHidden] public bool allowDamageStructures = true;
        [EditorHidden] public string bossEvent = "";
        [EditorHidden] public bool isHallucination = false;
        [EditorHidden] public int index;
        [EditorHidden] public bool talkInteract = false;

        public CreatureData() { }
    }
}
