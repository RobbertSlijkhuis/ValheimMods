namespace ModularMagic_Armors.Models
{
    internal class RecipeSnapShot
    {
        public bool enabled;
        public Piece.Requirement[] resources;
        public CraftingStation craftingStation;
        public int minStationLevel;
    }
}
