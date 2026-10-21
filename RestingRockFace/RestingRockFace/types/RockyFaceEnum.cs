namespace RestingRockFace.Types
{
    internal enum RockyFaceEnum
    {
        Dead = 5,
        LookLeft = 8,
        LookRight = 7,
        NoFeeling = 2,
        Nothing = 6,
        // Not a material index: means "let the game decide the face" (no stored face).
        Random = -1,
        Sad = 1,
        Scream = 3,
        Smile = 4,
        SmileBig = 0,
    }
}
