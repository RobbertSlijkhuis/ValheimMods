using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnAbilityData
    {
        public float? accuracy;
        public string announceMessage;
        public DamageData damage;
        public float? dropVelocity = 0f;
        public float? groundOffset;
        public float? initialSpawnDelay;
        public bool isBiomeList = false;
        public float? maxTargetRange;
        public int? maxToSpawn;
        public int? maxSpawned;
        public bool noSpawnEffect = false;
        public int? minToSpawn;
        public bool isOwner = true;
        public string prefabName;
        public bool? randomDirection;
        public float? randomAngleMax;
        public float? randomAngleMin;
        public bool? randomYRotation;
        public float? spawnDelay;
        public float? spawnRadius;
        public List<string> spawns = new List<string>();
        public string targetType = SpawnAbilityTargetType.Caster;
        public float? velocity;
        public float? velocityMax;

        public SpawnAbilityData() { }
    }
}
