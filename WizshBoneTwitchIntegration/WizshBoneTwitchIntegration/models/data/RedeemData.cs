using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;
using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemData : CloneableData
    {
        [EditorLabel("Title")]
        [EditorTooltip("The title of the redeem shown in Twitch")]
        public string title = "";

        [EditorLabel("Description")]
        public string description = "";

        [EditorLabel("Point cost")]
        [EditorTooltip("The number of points required to redeem this")]
        public int points = 0;

        [EditorLabel("Background color")]
        [EditorTooltip("The background color of the redeem shown in Twitch.")]
        public string backgroundColor = "#a970ff";

        [EditorLabel("Cooldown")]
        [EditorTooltip("Cooldown of the redeem in seconds.")] 
        public int cooldown = 0;

        [EditorLabel("Add condition")]
        [EditorTooltip("The global key that will enable this redeem")]
        public string globalKeyAdd = "";

        [EditorLabel("Remove condition")]
        [EditorTooltip("The global key that will disable this redeem")]
        public string globalKeyRemove = "";

        [EditorLabel("Ignore safezone")]
        [EditorTooltip("Whether to ignore the safezone for this redeem")]
        public bool ignoreWard = false;

        [EditorLabel("User input")]
        [EditorTooltip("Whether this redeem requires input")] 
        public bool userInput = false;

        [EditorHidden] public string type = RedeemType.Undefined;
        [EditorHidden] public SurpriseChestData chestData = new SurpriseChestData();
        [EditorHidden] public SpawnCreatureData creatureData = new SpawnCreatureData();
        [EditorHidden] public DetonateData detonateData = new DetonateData();
        [EditorHidden] public FlashBangData flashbangData = new FlashBangData();
        [EditorHidden] public MistData mistData = new MistData();
        [EditorHidden] public TerrainEditData terrainEditData = new TerrainEditData();
        [EditorHidden] public SpawnAbilityData spawnAbilityData = new SpawnAbilityData();
        [EditorHidden] public List<StatusEffectData> statusEffectData = new List<StatusEffectData>();
        [EditorHidden] public WeatherData weatherData = new WeatherData();

        public RedeemData() { }
    }
}
