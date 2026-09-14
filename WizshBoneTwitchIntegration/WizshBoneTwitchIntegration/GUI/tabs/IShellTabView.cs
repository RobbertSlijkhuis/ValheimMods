using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// One shell tab's content builder (round 2 of the UI redesign). Each <see cref="ShellTab"/>
    /// has its own file under GUI/tabs/, mirroring GUI_OLD/tabs/'s per-tab-file layout - but,
    /// unlike GUI_OLD's hand-duplicated per-tab fields/buttons/Create calls, wired up through this
    /// one shared interface so <see cref="WizshBoneShellGUI"/> can keep looping over
    /// <see cref="ShellTabExtensions.All"/> instead of growing a new hand-written block per tab.
    /// </summary>
    internal interface IShellTabView
    {
        /// <summary>
        /// Builds this tab's content as a new full-stretch child of <paramref name="parent"/>
        /// (the shell's content region) and returns that root, matching <see cref="UIContainer"/>'s
        /// tab-root convention. Called once, when the shell panel is first built.
        /// </summary>
        GameObject Create(GameObject parent);
    }
}
