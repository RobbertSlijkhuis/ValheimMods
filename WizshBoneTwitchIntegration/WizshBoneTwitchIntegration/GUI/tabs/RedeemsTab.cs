using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Redeems tab content. Stub for now - real content (redeem list, the redeem wizard,
    /// mirroring GUI_OLD/tabs/RedeemsTab.cs) is future rounds' scope.
    /// </summary>
    internal class RedeemsTab : IShellTabView
    {
        public GameObject Create(GameObject parent)
        {
            GameObject root = UIContainer.Create(parent, "RedeemsTab");
            GuiHelper.CreateTitle(ShellTab.Redeems.Label(), root, new Vector2(0f, -30f)).alignment = TextAnchor.MiddleCenter;
            return root;
        }
    }
}
