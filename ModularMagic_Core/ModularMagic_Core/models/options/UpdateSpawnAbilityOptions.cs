#nullable enable

namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The values that are applied to a SpawnAbility component. A value that is null is left as it is.
    /// </summary>
    public class UpdateSpawnAbilityOptions
    {
        public int? minToSpawn = null;
        public int? maxToSpawn = null;
        public int? maxSpawns = null;
        public float? spawnRadius = null;
        public float? projectileAccuracy = null;
        public float? spawnDelay = null;
    }
}
