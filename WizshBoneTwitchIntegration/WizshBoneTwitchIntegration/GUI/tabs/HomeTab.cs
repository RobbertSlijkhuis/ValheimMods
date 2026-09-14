using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Home tab content. Stub for now - real content (login status cards, quick links, etc.,
    /// mirroring GUI_OLD/tabs/HomeTab.cs) is future rounds' scope.
    /// </summary>
    internal class HomeTab : IShellTabView
    {
        public GameObject Create(GameObject parent)
        {
            GameObject root = UIContainer.Create(parent, "HomeTab");
            GuiHelper.CreateTitle(ShellTab.Home.Label(), root, new Vector2(0f, -30f)).alignment = TextAnchor.MiddleCenter;
            return root;
        }
    }
}
