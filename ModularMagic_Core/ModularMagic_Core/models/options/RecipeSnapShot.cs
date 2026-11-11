namespace ModularMagic_Core.Models
{
    /// <summary>
    /// A copy of the values of a recipe (see RecipeHelper.TakeSnapShot), to restore the recipe later.
    /// </summary>
    public class RecipeSnapShot
    {
        public bool enabled;
        public Piece.Requirement[] resources;
        public CraftingStation craftingStation;
        public int minStationLevel;
    }
}
