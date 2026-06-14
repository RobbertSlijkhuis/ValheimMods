using System.Collections.Generic;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnAbilityData : CloneableData
    {
        public float? accuracy;
        public bool allowDrops = true;
        public string announceMessage;
        public DamageData damage;
        public bool damageShips = false;
        public bool damageStructures = true;
        public float? dropVelocity = 0f;
        public int duration = 0;
        public float doorInterval = 0f;
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
        public bool randomRotation = true;
        public bool? randomDirection;
        public float? randomAngleMax;
        public float? randomAngleMin;
        public bool? randomYRotation;
        public bool rescanForSafezones = true;
        public bool snapToterrain = false;
        public float? spawnDelay;
        public float? spawnRadius;
        public List<string> spawns = new List<string>();
        public string targetType = SpawnAbilityTargetType.Caster;
        public float? velocity;
        public float? velocityMax;

        public SpawnAbilityData() { }
    }
}
