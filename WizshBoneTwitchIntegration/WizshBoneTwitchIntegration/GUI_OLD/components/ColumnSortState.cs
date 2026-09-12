namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Tracks which column a list/table tab is currently sorted by and in which direction.
    /// One instance per tab, held for as long as the tab stays open - clicking a sortable column
    /// header toggles direction if it's already the active column, or switches to that column
    /// (ascending) otherwise. See <see cref="TabListLayout"/>/<see cref="ColumnHeaderSpec.SortKey"/>
    /// for how headers wire into this.
    /// </summary>
    internal class ColumnSortState
    {
        public string Key;
        public bool Ascending = true;

        public ColumnSortState(string defaultKey)
        {
            Key = defaultKey;
        }

        public void ToggleOrSet(string key)
        {
            if (Key == key)
                Ascending = !Ascending;
            else
            {
                Key = key;
                Ascending = true;
            }
        }
    }
}
