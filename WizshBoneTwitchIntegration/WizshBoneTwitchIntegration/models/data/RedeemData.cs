using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;
using System.Reflection;
using System;
using System.Collections;

namespace WizshBoneTwitchIntegration.Models
{
    internal class RedeemData : CloneableData
    {
        public string title = "";

        // The redeem's permanent identity. The title can change (rename, prefix), this never does, so whatever
        // is linked to a redeem (its reward on Twitch) survives a rename. Assigned when the profile is loaded
        // (RedeemIdHelper.EnsureIds), saved with the profile, and new for every new or copied redeem.
        public string id = "";

        public string description = "";

        public int points = 0;

        public string backgroundColor = "#a970ff";

        public int cooldown = 0;

        // Display-only: which unit (see CooldownHelper) the wizard shows `cooldown` in. `cooldown`
        // itself is always stored in seconds. Only the wizard writes it, together with `cooldown`,
        // since only it does the CooldownHelper conversion that keeps the two in sync.
        public string cooldownUnit = "seconds";

        public int maxPerStream = 0;
        public int maxPerUserPerStream = 0;

        public string globalKeyAdd = "";

        public string globalKeyRemove = "";

        public bool ignoreWard = false;

        public bool userInput = false;

        public string type    = RedeemType.Undefined;
        public bool   enabled = true;
        public SurpriseChestData chestData = new SurpriseChestData();
        public SpawnCreatureData creatureData = new SpawnCreatureData();
        public DetonateData detonateData = new DetonateData();
        public FlashBangData flashbangData = new FlashBangData();
        public MistData mistData = new MistData();
        public TerrainEditData terrainEditData = new TerrainEditData();
        public SpawnAbilityData spawnAbilityData = new SpawnAbilityData();
        public StatusEffectData statusEffectData = new StatusEffectData();
        public TimeStopData timeStopData = new TimeStopData();
        public WeatherData weatherData = new WeatherData();
        // Maps each RedeemType to the one sub-data field that should be serialized for it.
        private static readonly Dictionary<string, string> TypeToDataField = new Dictionary<string, string>
        {
            { RedeemType.Detonate,           nameof(detonateData)     },
            { RedeemType.Flashbang,          nameof(flashbangData)    },
            { RedeemType.Mist,               nameof(mistData)         },
            { RedeemType.SpawnCreature,      nameof(creatureData)     },
            { RedeemType.SpawnAbility,       nameof(spawnAbilityData) },
            { RedeemType.Door,               nameof(spawnAbilityData) },
            { RedeemType.Windmill,           nameof(spawnAbilityData) },
            { RedeemType.Smite,              nameof(spawnAbilityData) },
            { RedeemType.Rain,               nameof(spawnAbilityData) },
            { RedeemType.Meteor,             nameof(spawnAbilityData) },
            { RedeemType.Trap,               nameof(spawnAbilityData) },
            { RedeemType.Root,               nameof(spawnAbilityData) },
            { RedeemType.LogRain,            nameof(spawnAbilityData) },
            { RedeemType.BoatRain,           nameof(spawnAbilityData) },
            { RedeemType.SurpriseChest,      nameof(chestData)        },
            { RedeemType.StatusEffect,       nameof(statusEffectData) },
            { RedeemType.TerrainEdit,        nameof(terrainEditData)  },
            { RedeemType.TimeStop,           nameof(timeStopData)     },
            { RedeemType.Weather,            nameof(weatherData)      },        };

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
            nameof(timeStopData),
            nameof(weatherData),        };

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

        internal static Dictionary<string, object> MinimalDictionary(CloneableData data)
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
                else if (value is IList list)
                {
                    if (IsEqual(value, field.GetValue(defaults)))
                        continue;

                    bool hasCloneable = false;
                    foreach (object item in list)
                    {
                        if (item is CloneableData) { hasCloneable = true; break; }
                    }

                    if (hasCloneable)
                    {
                        var minimalItems = new List<object>();
                        foreach (object item in list)
                            minimalItems.Add(item is CloneableData c ? (object)MinimalDictionary(c) : item);
                        result[field.Name] = minimalItems;
                    }
                    else
                    {
                        result[field.Name] = value;
                    }
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

        /// <summary>
        /// Returns the DamageData relevant to this redeem's type, or null if its type has none.
        /// </summary>
        public DamageData GetDamageData()
        {
            if (RedeemType.SpawnAbilityFamily.Contains(type))
                return spawnAbilityData?.damage;

            if (type == RedeemType.Detonate)
                return detonateData?.damageData;

            return null;
        }

        public RedeemData() { }
    }
}
