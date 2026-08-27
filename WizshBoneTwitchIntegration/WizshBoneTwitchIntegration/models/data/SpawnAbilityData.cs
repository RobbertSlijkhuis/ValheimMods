using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class SpawnAbilityData : CloneableData
    {
        public float? accuracy;
        public bool allowDrops = true;
        public string announceMessage;
        public bool breakOnDestroy = false;
        public DamageData damage;
        public float? dropVelocity = 0f;
        public int duration = 0;
        public float doorInterval = 0f;
        public float windmillRotationSpeed = 0f;
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
        public bool snapToterrain = false;
        public int batchSize = 1;
        public float? spawnDelay;
        public float? spawnRadius;
        public List<string> spawns = new List<string>();
        public string targetType = SpawnAbilityTargetType.Caster;
        public float? velocity;
        public float? velocityMax;

        [EditorHidden]
        public bool? randomDirection;
        [EditorHidden]
        public float? randomAngleMax;
        [EditorHidden]
        public float? randomAngleMin;
        [EditorHidden]
        public bool? randomYRotation;

        public SpawnAbilityData() { }
    }
}
