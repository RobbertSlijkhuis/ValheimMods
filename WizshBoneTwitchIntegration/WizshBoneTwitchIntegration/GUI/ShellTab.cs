namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Sidebar tabs for the new UI shell. Each tab's content lives in its own
    /// <see cref="IShellTabView"/> under GUI/tabs/ (see <see cref="WizshBoneShellGUI"/>).
    /// </summary>
    internal enum ShellTab
    {
        Home,
        Profiles,
        Redeems,
        Settings,
        CreatureGroups,
        Viewers,
        Help
    }

    internal static class ShellTabExtensions
    {
        /// <summary>
        /// All tabs, in the order they should appear in the sidebar.
        /// </summary>
        public static readonly ShellTab[] All =
        {
            ShellTab.Home,
            ShellTab.Profiles,
            ShellTab.Redeems,
            ShellTab.Settings,
            ShellTab.CreatureGroups,
            ShellTab.Viewers,
            ShellTab.Help
        };

        public static string Label(this ShellTab tab)
        {
            switch (tab)
            {
                case ShellTab.Home: return "Home";
                case ShellTab.Profiles: return "Profiles";
                case ShellTab.Redeems: return "Redeems";
                case ShellTab.Settings: return "Settings";
                case ShellTab.CreatureGroups: return "Creature groups";
                case ShellTab.Viewers: return "Viewers";
                case ShellTab.Help: return "Help";
                default: return tab.ToString();
            }
        }
    }
}
