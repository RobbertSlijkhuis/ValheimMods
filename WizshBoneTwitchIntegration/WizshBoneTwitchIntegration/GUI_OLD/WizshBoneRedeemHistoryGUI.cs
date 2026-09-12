using Jotunn.Managers;
using System;
using System.Collections.Generic;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
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
        private Button m_completeAllButton;
        private Button m_refundAllButton;
        private string m_searchText = "";
        private Action m_onClose;
        private bool m_blockingInput = false;

        public WizshBoneRedeemHistoryGUI(TwitchCustomRewards customRewards)
        {
            m_customRewards = customRewards;
        }

        /// <summary>
        /// Shows the panel, pushing an <see cref="InputBlockGate"/> block if one isn't already
        /// outstanding for this panel. <paramref name="onClose"/> is purely a UX follow-up (e.g.
        /// "return to Settings") - the input block is popped by this panel itself on any
        /// close/hide, regardless of who opened it or whether a callback was supplied.
        /// </summary>
        public void ShowGUI(Action onClose = null)
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
                return;

            m_onClose = onClose;

            if (!m_blockingInput)
            {
                m_blockingInput = true;
                InputBlockGate.Push();
            }

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

        /// <summary>
        /// Closes the panel, popping its input block and invoking <see cref="m_onClose"/>. Wired
        /// to the panel's own "Close" button.
        /// </summary>
        public void CloseGUI() => Close(invokeOnClose: true);

        /// <summary>
        /// Deactivates the panel and pops its input block, without invoking <see cref="m_onClose"/>.
        /// Used by full-teardown paths (F4, other "close everything" buttons) that must not trigger
        /// a "return to Settings"-style callback meant only for the panel's own Close button.
        /// </summary>
        public void Hide() => Close(invokeOnClose: false);

        private void Close(bool invokeOnClose)
        {
            if (m_panel != null)
                m_panel.SetActive(false);

            if (m_blockingInput)
            {
                m_blockingInput = false;
                InputBlockGate.Pop();
            }

            Action onClose = m_onClose;
            m_onClose = null;

            if (invokeOnClose)
                onClose?.Invoke();
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
            m_completeAllButton = completeAllBtn.GetComponent<Button>();
            m_completeAllButton.onClick.AddListener(OnCompleteAll);

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
            m_refundAllButton = refundAllBtn.GetComponent<Button>();
            m_refundAllButton.onClick.AddListener(OnRefundAll);

            GameObject closeButton = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(100f, 45f),
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

            bool bulkRunning = m_customRewards.m_bulkResolveHelper.IsRunning;

            if (m_pageLabel != null)
                m_pageLabel.text = bulkRunning ? BuildBulkProgressText() : $"Page {m_currentPage + 1} / {totalPages}";

            if (m_prevButton != null)
                m_prevButton.interactable = m_currentPage > 0;

            if (m_nextButton != null)
                m_nextButton.interactable = m_currentPage < totalPages - 1;

            if (m_completeAllButton != null)
                m_completeAllButton.interactable = !bulkRunning;

            if (m_refundAllButton != null)
                m_refundAllButton.interactable = !bulkRunning;

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

            float rowHeight = 60f;
            float yOffset = -10f;

            foreach (CustomRewardEvent entry in GetVisiblePageHistory())
            {
                CreateHistoryRow(entry, yOffset);
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
                text: $"<color={statusColor}><b>{entry.RedeemerName}</b>  �  {entry.CustomRewardTitle}  ({entry.CustomRewardCost} pts)</color>",
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

        private void OnCompleteAll() => StartBulkResolve(CustomRewardRedemptionState.Fulfilled);

        private void OnRefundAll() => StartBulkResolve(CustomRewardRedemptionState.Canceled);

        /// <summary>
        /// Restored to operate on GetFilteredHistory() (every page of the current search filter),
        /// matching pre-d51804d3 behavior - not GetVisiblePageHistory(), which only scoped this
        /// to the current pagination window. The resolve calls themselves are spread across time
        /// via BulkRedeemResolveHelper instead of firing all at once, to avoid the lag a large
        /// synchronous batch of native Twitch SDK calls can cause.
        /// </summary>
        private void StartBulkResolve(CustomRewardRedemptionState targetState)
        {
            List<CustomRewardEvent> eligible = GetFilteredHistory().FindAll(BulkRedeemResolveHelper.IsEligible);

            m_customRewards.m_bulkResolveHelper.EnqueueAll(
                host: m_customRewards,
                entries: eligible,
                targetState: targetState,
                onProgress: OnBulkResolveProgress,
                onFinished: OnBulkResolveFinished
            );

            // Immediately reflects the disabled buttons / initial progress text, even before the
            // first WaitForSecondsRealtime tick fires. If eligible is empty, this is a harmless
            // no-op refresh.
            RefreshPage();
        }

        private void OnBulkResolveProgress()
        {
            // Cheap label-only update - do NOT call the full RefreshPage() here. RefreshPage()
            // tears down and rebuilds every row GameObject under m_contentRoot; doing that
            // several times a second for the whole run would be wasteful and would flicker the
            // row buttons for no reason. Per-row state catches up once, in OnBulkResolveFinished.
            // Also guards against the panel having been closed - the queue keeps draining on
            // m_customRewards in the background regardless.
            if (m_panel == null || !m_panel.activeSelf || m_pageLabel == null)
                return;

            m_pageLabel.text = BuildBulkProgressText();
        }

        private void OnBulkResolveFinished()
        {
            // Full RefreshPage() reconciles everything at once: restores "Page X / Y", re-enables
            // Complete All/Refund All, and refreshes every row's status color / button
            // interactable state to match what the queue actually resolved.
            if (m_panel == null || !m_panel.activeSelf)
                return;

            RefreshPage();
        }

        private string BuildBulkProgressText()
        {
            BulkRedeemResolveHelper resolver = m_customRewards.m_bulkResolveHelper;
            string verb = resolver.TargetState == CustomRewardRedemptionState.Fulfilled ? "Completing" : "Refunding";
            return $"{verb} {resolver.Completed}/{resolver.Total}...";
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

        private List<CustomRewardEvent> GetFilteredHistory()
        {
            List<CustomRewardEvent> history = m_customRewards.m_redeemHistory;
            List<CustomRewardEvent> filtered = new List<CustomRewardEvent>();

            foreach (CustomRewardEvent entry in history)
            {
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

        /// <summary>
        /// The subset of <see cref="GetFilteredHistory"/> actually rendered on the current page -
        /// i.e. the filtered history sliced to the current pagination window. Used only for
        /// rendering rows in <see cref="RefreshPage"/> - "Complete All"/"Refund All" operate on
        /// the full <see cref="GetFilteredHistory"/> list instead, via <see cref="StartBulkResolve"/>.
        /// </summary>
        private List<CustomRewardEvent> GetVisiblePageHistory()
        {
            List<CustomRewardEvent> history = GetFilteredHistory();
            List<CustomRewardEvent> visible = new List<CustomRewardEvent>();

            int startIndex = history.Count - 1 - (m_currentPage * m_pageSize);
            for (int i = startIndex; i >= 0 && i > startIndex - m_pageSize; i--)
                visible.Add(history[i]);

            return visible;
        }

        public bool IsVisible => m_panel != null && m_panel.activeSelf;
    }
}