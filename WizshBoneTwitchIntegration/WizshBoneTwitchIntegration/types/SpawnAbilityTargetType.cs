namespace WizshBoneTwitchIntegration.Types
{
    internal class SpawnAbilityTargetType
    {
        public static string ClosestEnemy => "ClosestEnemy";
        public static string RandomEnemy => "RandomEnemy";
        public static string Caster => "Caster";
        public static string Position => "Position";
        public static string RandomPathfindablePosition => "RandomPathfindablePosition";

        public static SpawnAbility.TargetType? ConvertToTargetType(string value)
        {
            switch (value)
            {
                case nameof(ClosestEnemy):
                    return SpawnAbility.TargetType.ClosestEnemy;
                case nameof(RandomEnemy):
                    return SpawnAbility.TargetType.RandomEnemy;
                case nameof(Caster):
                    return SpawnAbility.TargetType.Caster;
                case nameof(Position):
                    return SpawnAbility.TargetType.Position;
                case nameof(RandomPathfindablePosition):
                    return SpawnAbility.TargetType.RandomPathfindablePosition;
                default:
                    return null;
            }
        }
    }
}
