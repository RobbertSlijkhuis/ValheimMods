using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Viewers tab content. Stub for now - real content (viewer list, ban/unban, mirroring
    /// GUI_OLD/tabs/ViewersTab.cs) is future rounds' scope.
    /// </summary>
    internal class ViewersTab : IShellTabView
    {
        public GameObject Create(GameObject parent)
        {
            GameObject root = UIContainer.Create(parent, "ViewersTab");
            GuiHelper.CreateTitle(ShellTab.Viewers.Label(), root, new Vector2(0f, -30f)).alignment = TextAnchor.MiddleCenter;
            return root;
        }
    }
}
