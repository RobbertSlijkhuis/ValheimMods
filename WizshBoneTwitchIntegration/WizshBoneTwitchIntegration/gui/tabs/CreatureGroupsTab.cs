using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class CreatureGroupsTab
    {
        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
            => UIContainer.Create(parent, "CreatureGroupsTab");

        public void Refresh() { }
    }
}
