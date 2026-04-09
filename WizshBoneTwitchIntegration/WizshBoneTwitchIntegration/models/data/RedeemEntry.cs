using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemEntry
    {
        public string backgroundColor = "#a970ff";
        public int cooldown = 0;
        public SurpriseChestData chestData = new SurpriseChestData();
        public List<CreatureData> creatureData = new List<CreatureData>();
        public DetonateData detonateData = new DetonateData();
        public FlashBangData flashbangData = new FlashBangData();
        public string globalKeyAdd = "";
        public string globalKeyRemove = "";
        public bool ignoreWard = false;
        public MistData mistData = new MistData();
        public int points = 0;
        public TerrainEditData terrainEditData = new TerrainEditData();
        public SpawnAbilityData spawnAbilityData = new SpawnAbilityData();
        public List<StatusEffectData> statusEffectData = new List<StatusEffectData>();
        public string title = "";
        public string type = RedeemType.Undefined;
        public bool userInput = false;
        public WeatherData weatherData = new WeatherData();

        public RedeemEntry() { }

        public RedeemEntry(string type, string title, int points, string backgroundColor, int cooldown = 0, bool userInput = false, string globalKeyAdd = null, string globalKeyRemove = null, List<CreatureData> creatureData = null)
        {
            this.backgroundColor = backgroundColor;
            this.cooldown = cooldown;
            this.creatureData = creatureData;
            this.globalKeyAdd = globalKeyAdd;
            this.globalKeyRemove = globalKeyRemove;
            this.points = points;
            this.title = title;
            this.type = type;
            this.userInput = userInput;
        }
    }
}
