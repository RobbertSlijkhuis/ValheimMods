using Jotunn.Managers;
using System;
using System.Collections.Generic;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class WizshBoneRedeemHistoryGUI
    {
        private GameObject m_panel;
        private readonly TwitchCustomRewards m_customRewards;

        private int m_currentPage = 0;
        private const int m_pageSize = 30;

        private GameObject m_contentRoot;
        private Text m_pageLabel;
        private Button m_prevButton;
        private Button m_nextButton;
        private bool m_hideTestRedeems = false;
        private Button m_filterButton;
        private string m_searchText = "";

        public WizshBoneRedeemHistoryGUI(TwitchCustomRewards customRewards)
        {
            m_customRewards = customRewards;
        }

        public void ShowGUI()
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
                return;

            if (m_panel != null)
            {
                m_panel.transform.SetAsLastSibling();
                m_currentPage = 0;
                RefreshPage();
                m_panel.SetActive(true);
                return;
            }

            m_panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, 0f),
                width: 800f,
                height: 700f,
                draggable: true
            );

            m_panel.transform.SetAsLastSibling();
            CreateGUI();
            m_currentPage = 0;
            RefreshPage();
            m_panel.SetActive(true);
        }

        public void CloseGUI()
        {
            if (m_panel != null)
                m_panel.SetActive(false);
        }

        private void CreateGUI()
        {
            GUIManager.Instance.CreateText(
                text: "Redeem History",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 24,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 500f,
                height: 30f,
                addContentSizeFitter: false
            );

            InputField searchField = FieldUIBuilder.CreateInputField(
                parent: m_panel,
                position: new Vector2(0f, -80f),
                width: 500f,
                initialValue: ""
            );
            searchField.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Search by name or redeem...";
            searchField.onValueChanged.AddListener(OnSearchChanged);

            m_contentRoot = ScrollableView.CreateStretched(
                parent: m_panel,
                name: "History",
                offsetMin: new Vector2(30f, 155f),
                offsetMax: new Vector2(-30f, -110f)
            );

            // Pagination row
            GameObject prevButtonObj = GUIManager.Instance.CreateButton(
                text: "<",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-150f, 105f),
                width: 60f,
                height: 40f
            );
            prevButtonObj.SetActive(true);
            m_prevButton = prevButtonObj.GetComponent<Button>();
            m_prevButton.onClick.AddListener(PrevPage);

            m_pageLabel = GUIManager.Instance.CreateText(
                text: "Page 1 / 1",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0f, 105f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 150f,
                height: 40f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            m_pageLabel.alignment = TextAnchor.MiddleCenter;

            GameObject nextButtonObj = GUIManager.Instance.CreateButton(
                text: ">",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(150f, 105f),
                width: 60f,
                height: 40f
            );
            nextButtonObj.SetActive(true);
            m_nextButton = nextButtonObj.GetComponent<Button>();
            m_nextButton.onClick.AddListener(NextPage);

            GameObject completeAllBtn = GUIManager.Instance.CreateButton(
                text: "Complete All",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-260f, 45f),
                width: 160f,
                height: 60f
            );
            completeAllBtn.SetActive(true);
            completeAllBtn.GetComponent<Button>().onClick.AddListener(OnCompleteAll);

            GameObject refundAllBtn = GUIManager.Instance.CreateButton(
                text: "Refund All",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-80f, 45f),
                width: 160f,
                height: 60f
            );
            refundAllBtn.SetActive(true);
            refundAllBtn.GetComponent<Button>().onClick.AddListener(OnRefundAll);

            GameObject filterButtonObj = GUIManager.Instance.CreateButton(
                text: "Test Redeems: ON",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(100f, 45f),
                width: 200f,
                height: 60f
            );
            filterButtonObj.SetActive(true);
            m_filterButton = filterButtonObj.GetComponent<Button>();
            m_filterButton.onClick.AddListener(ToggleTestFilter);

            GameObject closeButton = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(290f, 45f),
                width: 120f,
                height: 60f
            );
            closeButton.SetActive(true);
            closeButton.GetComponent<Button>().onClick.AddListener(CloseGUI);
        }

        private void OnSearchChanged(string value)
        {
            m_searchText  = value;
            m_currentPage = 0;
            RefreshPage();
        }

        private void RefreshPage()
        {
            if (m_contentRoot == null)
                return;

            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in m_contentRoot.transform)
                children.Add(child.gameObject);

            foreach (GameObject child in children)
                GameObject.DestroyImmediate(child);

            List<CustomRewardEvent> history = GetFilteredHistory();
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)history.Count / m_pageSize));
            m_currentPage = Mathf.Clamp(m_currentPage, 0, totalPages - 1);

            if (m_pageLabel != null)
                m_pageLabel.text = $"Page {m_currentPage + 1} / {totalPages}";

            if (m_prevButton != null)
                m_prevButton.interactable = m_currentPage > 0;

            if (m_nextButton != null)
                m_nextButton.interactable = m_currentPage < totalPages - 1;

            if (history.Count == 0)
            {
                GUIManager.Instance.CreateText(
                    text: "No redeems yet this session!",
                    parent: m_contentRoot.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -30f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 16,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: 560f,
                    height: 40f,
                    addContentSizeFitter: false
                );
                return;
            }

            int startIndex = history.Count - 1 - (m_currentPage * m_pageSize);
            float rowHeight = 60f;
            float yOffset = -10f;

            for (int i = startIndex; i >= 0 && i > startIndex - m_pageSize; i--)
            {
                CreateHistoryRow(history[i], yOffset);
                yOffset -= rowHeight;
            }

            RectTransform contentRt = m_contentRoot.GetComponent<RectTransform>();
            if (contentRt != null)
            {
                int rowCount = Mathf.Min(m_pageSize, history.Count - m_currentPage * m_pageSize);
                contentRt.sizeDelta = new Vector2(0f, Mathf.Max(rowCount * rowHeight + 20f, rowHeight));
            }
        }

        private void CreateHistoryRow(CustomRewardEvent entry, float yOffset)
        {
            bool isTestRedeem = entry.RedemptionId == Guid.Empty.ToString();
            bool isResolved = entry.Status == CustomRewardRedemptionState.Fulfilled
                || entry.Status == CustomRewardRedemptionState.Canceled;

            string statusColor = isTestRedeem ? "grey"
                : entry.Status == CustomRewardRedemptionState.Fulfilled ? "grey"
                : entry.Status == CustomRewardRedemptionState.Canceled ? "green"
                : "white";

            GUIManager.Instance.CreateText(
                text: $"<color={statusColor}><b>{entry.RedeemerName}</b>  —  {entry.CustomRewardTitle}  ({entry.CustomRewardCost} pts)</color>",
                parent: m_contentRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-100f, yOffset - 8f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 480f,
                height: 40f,
                addContentSizeFitter: false
            );

            if (isTestRedeem)
                return;

            GameObject completeBtn = GUIManager.Instance.CreateButton(
                text: "Complete",
                parent: m_contentRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(130f, yOffset - 8f),
                width: 90f,
                height: 40f
            );
            completeBtn.SetActive(true);
            Button completeBtnComp = completeBtn.GetComponent<Button>();
            completeBtnComp.interactable = !isResolved;
            CustomRewardEvent capturedEntry = entry;
            completeBtnComp.onClick.AddListener(() => OnComplete(capturedEntry));

            GameObject refundBtn = GUIManager.Instance.CreateButton(
                text: "Refund",
                parent: m_contentRoot.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(230f, yOffset - 8f),
                width: 90f,
                height: 40f
            );
            refundBtn.SetActive(true);
            Button refundBtnComp = refundBtn.GetComponent<Button>();
            refundBtnComp.interactable = !isResolved;
            refundBtnComp.onClick.AddListener(() => OnRefund(capturedEntry));
        }

        private void OnComplete(CustomRewardEvent entry)
        {
            GameTask task = Twitch.API.ResolveCustomReward(entry, CustomRewardRedemptionState.Fulfilled);
            task.GetAwaiter().OnCompleted(() => OnResolveCompleted(entry, CustomRewardRedemptionState.Fulfilled));
        }

        private void OnRefund(CustomRewardEvent entry)
        {
            GameTask task = Twitch.API.ResolveCustomReward(entry, CustomRewardRedemptionState.Canceled);
            task.GetAwaiter().OnCompleted(() => OnResolveCompleted(entry, CustomRewardRedemptionState.Canceled));
        }

        private void OnResolveCompleted(CustomRewardEvent entry, CustomRewardRedemptionState state)
        {
            entry.Status = state;
            RefreshPage();
        }

        private void OnCompleteAll()
        {
            List<CustomRewardEvent> history = GetFilteredHistory();
            foreach (CustomRewardEvent entry in history)
            {
                if (entry.Status == CustomRewardRedemptionState.Fulfilled || entry.Status == CustomRewardRedemptionState.Canceled)
                    continue;

                entry.Status = CustomRewardRedemptionState.Fulfilled;
                Twitch.API.ResolveCustomReward(entry, CustomRewardRedemptionState.Fulfilled);
            }

            RefreshPage();
        }

        private void OnRefundAll()
        {
            List<CustomRewardEvent> history = GetFilteredHistory();
            foreach (CustomRewardEvent entry in history)
            {
                if (entry.Status == CustomRewardRedemptionState.Fulfilled || entry.Status == CustomRewardRedemptionState.Canceled)
                    continue;

                entry.Status = CustomRewardRedemptionState.Canceled;
                Twitch.API.ResolveCustomReward(entry, CustomRewardRedemptionState.Canceled);
            }

            RefreshPage();
        }

        private void PrevPage()
        {
            m_currentPage--;
            RefreshPage();
        }

        private void NextPage()
        {
            m_currentPage++;
            RefreshPage();
        }

        private void ToggleTestFilter()
        {
            m_hideTestRedeems = !m_hideTestRedeems;
            m_filterButton.GetComponentInChildren<Text>().text = m_hideTestRedeems
                ? "Test Redeems: OFF"
                : "Test Redeems: ON";
            m_currentPage = 0;
            RefreshPage();
        }

        private List<CustomRewardEvent> GetFilteredHistory()
        {
            List<CustomRewardEvent> history = m_customRewards.m_redeemHistory;
            List<CustomRewardEvent> filtered = new List<CustomRewardEvent>();

            foreach (CustomRewardEvent entry in history)
            {
                if (m_hideTestRedeems && entry.RedemptionId == Guid.Empty.ToString())
                    continue;

                if (!string.IsNullOrEmpty(m_searchText))
                {
                    bool matchesName   = entry.RedeemerName.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchesRedeem = entry.CustomRewardTitle.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!matchesName && !matchesRedeem)
                        continue;
                }

                filtered.Add(entry);
            }

            return filtered;
        }

        public bool IsVisible => m_panel != null && m_panel.activeSelf;
    }
}