using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Debug tab content. Stub for now - real content (debug toggles/actions, mirroring
    /// GUI_OLD/tabs/DebugTab.cs) is future rounds' scope.
    /// </summary>
    internal class DebugTab : IShellTabView
    {
        public GameObject Create(GameObject parent)
        {
            GameObject root = UIContainer.Create(parent, "DebugTab");
            GuiHelper.CreateTitle(ShellTab.Debug.Label(), root, new Vector2(0f, -30f)).alignment = TextAnchor.MiddleCenter;
            return root;
        }
    }
}
