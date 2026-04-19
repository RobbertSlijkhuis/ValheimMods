using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemData
    {
        public string backgroundColor = "#a970ff";
        public int cooldown = 0;
        public SurpriseChestData chestData = new SurpriseChestData();
        public SpawnCreatureData creatureData = new SpawnCreatureData();
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

        public RedeemData() { }
    }
}
