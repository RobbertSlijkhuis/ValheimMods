namespace ModularMagic_Core.Types
{
    /// <summary>
    /// Which part of a recipe an update changes, see RecipeHelper.UpdateRecipe.
    /// </summary>
    public enum RecipeUpdateType
    {
        ALL = 0,
        ENABLE = 1,
        AMOUNT = 2,
        RECIPE = 3,
        CRAFTINGSTATION = 4,
        MINREQUIREDSTATIONLEVEL = 5,
        MINREQUIREDREPAIRLEVEL = 6,
        FROM_RECIPE = 7,
    }
}
