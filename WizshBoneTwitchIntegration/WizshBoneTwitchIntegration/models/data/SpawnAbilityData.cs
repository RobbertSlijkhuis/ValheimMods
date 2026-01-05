using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnAbilityData
    {
        public float? accuracy;
        public string announceMessage;
        public float? groundOffset;
        public float? initialSpawnDelay;
        public float? maxTargetRange;
        public int? maxToSpawn;
        public int? maxSpawned;
        public int? minToSpawn;
        public List<string> spawns = new List<string>();
        public string prefabName;
        public bool? randomDirection;
        public float? randomAngleMax;
        public float? randomAngleMin;
        public bool? randomYRotation;
        public float? spawnDelay;
        public float? spawnRadius;
        public string targetType = SpawnAbilityTargetType.Caster;
        public float? velocity;
        public float? velocityMax;

        public SpawnAbilityData() { }
    }
}
