using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Creature groups tab content. Stub for now - real content (creature group list/editor,
    /// mirroring GUI_OLD/tabs/CreatureGroupsTab.cs) is future rounds' scope.
    /// </summary>
    internal class CreatureGroupsTab : IShellTabView
    {
        public GameObject Create(GameObject parent)
        {
            GameObject root = UIContainer.Create(parent, "CreatureGroupsTab");
            GuiHelper.CreateTitle(ShellTab.CreatureGroups.Label(), root, new Vector2(0f, -30f)).alignment = TextAnchor.MiddleCenter;
            return root;
        }

        // No live state to refresh yet - stub content only.
        public void Refresh() { }
    }
}
