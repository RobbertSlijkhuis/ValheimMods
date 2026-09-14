using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Settings tab content. Stub for now - real content (profile settings, mirroring
    /// GUI_OLD/tabs/RulesTab.cs) is future rounds' scope.
    /// </summary>
    internal class SettingsTab : IShellTabView
    {
        public GameObject Create(GameObject parent)
        {
            GameObject root = UIContainer.Create(parent, "SettingsTab");
            GuiHelper.CreateTitle(ShellTab.Settings.Label(), root, new Vector2(0f, -30f)).alignment = TextAnchor.MiddleCenter;
            return root;
        }
    }
}
