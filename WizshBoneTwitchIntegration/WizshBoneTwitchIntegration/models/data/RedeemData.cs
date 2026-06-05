using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;
using WizshBoneTwitchIntegration.Gui;
using System.Reflection;
using System;
using System.Collections;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemData : CloneableData
    {
        [EditorLabel("Title")]
        [EditorTooltip("The title of the redeem shown in Twitch.")]
        public string title = "";

        [EditorLabel("Description")]
        [EditorTooltip("The description of the redeem shown when selected in Twitch.")]
        public string description = "";

        [EditorLabel("Point cost")]
        [EditorTooltip("The number of points required to redeem this.")]
        public int points = 0;

        [EditorLabel("Background color")]
        [EditorTooltip("The background color of the redeem shown in Twitch.")]
        [ColorPicker]
        public string backgroundColor = "#a970ff";

        [EditorLabel("Cooldown")]
        [EditorTooltip("Cooldown of the redeem in seconds.")] 
        public int cooldown = 0;

        [EditorLabel("Add condition")]
        [EditorTooltip("The global key that will enable this redeem.")]
        public string globalKeyAdd = "";

        [EditorLabel("Remove condition")]
        [EditorTooltip("The global key that will disable this redeem.")]
        public string globalKeyRemove = "";

        [EditorLabel("Ignore safezone")]
        [EditorTooltip("Whether to ignore the safezone for this redeem.")]
        public bool ignoreWard = false;

        [EditorLabel("User input")]
        [EditorTooltip("Whether this redeem requires input.")] 
        public bool userInput = false;

        [EditorHidden] public string type = RedeemType.Undefined;
        [EditorHidden] public SurpriseChestData chestData = new SurpriseChestData();
        [EditorHidden] public SpawnCreatureData creatureData = new SpawnCreatureData();
        [EditorHidden] public DetonateData detonateData = new DetonateData();
        [EditorHidden] public FlashBangData flashbangData = new FlashBangData();
        [EditorHidden] public MistData mistData = new MistData();
        [EditorHidden] public TerrainEditData terrainEditData = new TerrainEditData();
        [EditorHidden] public SpawnAbilityData spawnAbilityData = new SpawnAbilityData();
        [EditorHidden] public StatusEffectData statusEffectData = new StatusEffectData();
        [EditorHidden] public WeatherData weatherData = new WeatherData();

        // Maps each RedeemType to the one sub-data field that should be serialized for it.
        private static readonly Dictionary<string, string> TypeToDataField = new Dictionary<string, string>
        {
            { RedeemType.Detonate,           nameof(detonateData)     },
            { RedeemType.Flashbang,          nameof(flashbangData)    },
            { RedeemType.SpawnMist,          nameof(mistData)         },
            { RedeemType.TerrainEdit,        nameof(terrainEditData)  },
            { RedeemType.SpawnWeather,       nameof(weatherData)      },
            { RedeemType.SpawnCreature,      nameof(creatureData)     },
            { RedeemType.SpawnAbility,       nameof(spawnAbilityData) },
            { RedeemType.SurpriseChest,      nameof(chestData)        },
            { RedeemType.StatusEffect,       nameof(statusEffectData) },
            { RedeemType.StatusEffectRandom, nameof(statusEffectData) },
        };

        private static readonly HashSet<string> AllDataFields = new HashSet<string>
        {
            nameof(chestData),
            nameof(creatureData),
            nameof(detonateData),
            nameof(flashbangData),
            nameof(mistData),
            nameof(terrainEditData),
            nameof(spawnAbilityData),
            nameof(statusEffectData),
            nameof(weatherData),
        };

        /// <summary>
        /// Returns a dictionary with all standard fields and only the
        /// sub-data block that corresponds to the current <see cref="type"/>.
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            object defaults = new RedeemData();
            var result      = new Dictionary<string, object>();

            TypeToDataField.TryGetValue(type, out string relevantField);

            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (AllDataFields.Contains(field.Name))
                {
                    if (field.Name != relevantField)
                        continue;

                    object value = field.GetValue(this);
                    if (value is CloneableData subData)
                    {
                        Dictionary<string, object> subDict = MinimalDictionary(subData);
                        if (subDict.Count > 0)
                            result[field.Name] = subDict;
                    }
                }
                else
                {
                    object value = field.GetValue(this);
                    if (value != null && !IsEqual(value, field.GetValue(defaults)))
                        result[field.Name] = value;
                }
            }

            return result;
        }

        private static Dictionary<string, object> MinimalDictionary(CloneableData data)
        {
            object defaults = Activator.CreateInstance(data.GetType());
            var result      = new Dictionary<string, object>();

            foreach (FieldInfo field in data.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(data);

                if (value == null)
                    continue;

                if (value is CloneableData nested)
                {
                    Dictionary<string, object> nestedDict = MinimalDictionary(nested);
                    if (nestedDict.Count > 0)
                        result[field.Name] = nestedDict;
                }
                else if (!IsEqual(value, field.GetValue(defaults)))
                {
                    result[field.Name] = value;
                }
            }

            return result;
        }

        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            if (a is IList listA && b is IList listB)
            {
                if (listA.Count != listB.Count) return false;
                for (int i = 0; i < listA.Count; i++)
                    if (!Equals(listA[i], listB[i])) return false;
                return true;
            }

            return a.Equals(b);
        }

        public RedeemData() { }
    }
}
