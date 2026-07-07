using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class RulesTab
    {
        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
            => UIContainer.Create(parent, "RulesTab");

        public void Refresh() { }
    }
}
