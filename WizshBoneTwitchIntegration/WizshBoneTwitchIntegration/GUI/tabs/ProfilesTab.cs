using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Profiles tab content. Stub for now - real content (profile list, switch/create/delete,
    /// mirroring GUI_OLD/tabs/ProfilesTab.cs) is future rounds' scope.
    /// </summary>
    internal class ProfilesTab : IShellTabView
    {
        public GameObject Create(GameObject parent)
        {
            GameObject root = UIContainer.Create(parent, "ProfilesTab");
            GuiHelper.CreateTitle(ShellTab.Profiles.Label(), root, new Vector2(0f, -30f)).alignment = TextAnchor.MiddleCenter;
            return root;
        }
    }
}
