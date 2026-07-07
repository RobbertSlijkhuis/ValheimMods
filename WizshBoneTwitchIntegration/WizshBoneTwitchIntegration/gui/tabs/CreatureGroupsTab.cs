using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class CreatureGroupsTab
    {
        private GameObject m_root;
        private GameObject m_listContainer;
        private Text m_feedbackText;

        private const float ItemHeight = 40f;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
        {
            m_root = UIContainer.Create(parent, "CreatureGroupsTab");

            var options = new TabListLayoutOptions
            {
                TitleText       = "Creature Groups:",
                ShowSearchBar   = true,
                OnSearchChanged = OnSearchChanged,
                MainContainerName = "CreatureGroupList",
            };

            TabListLayoutResult result = TabListLayout.Create(m_root, "ListView", createScrollable, options);

            m_listContainer = result.MainContainer;
            m_feedbackText  = result.FeedbackText;

            // TabListLayout builds the list view via UIContainer.Create, which starts inactive by
            // default (other tabs activate it via their own ShowListView when switching views) -
            // this tab has no alternate view yet, so its list view must be shown right away.
            result.ListView.SetActive(true);

            RefreshList();

            return m_root;
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            TabUIHelper.ClearContainer(m_listContainer);

            List<CreatureGroupData> groups = RedeemHelper.creatureGroups ?? new List<CreatureGroupData>();

            if (groups.Count == 0)
            {
                m_feedbackText.text = "No creature groups found.";

                RectTransform emptyRt = m_listContainer.GetComponent<RectTransform>();
                emptyRt.sizeDelta = new Vector2(emptyRt.sizeDelta.x, ItemHeight);
                return;
            }

            m_feedbackText.text = "";

            // Row-building (create/edit/delete) lands here once the tab's CRUD flow is implemented.
        }

        // Filtering lands here once there are rows to filter - wired now so the search bar
        // already behaves correctly (a no-op refresh) rather than being added later.
        private void OnSearchChanged(string value) => RefreshList();
    }
}
