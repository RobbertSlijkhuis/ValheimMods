namespace DeathWizshAPI.Helpers.Models
{
    public class RecipeSnapShot
    {
        public bool enabled;
        public Piece.Requirement[] resources;
        public CraftingStation craftingStation;
        public int minStationLevel;
    }
}
