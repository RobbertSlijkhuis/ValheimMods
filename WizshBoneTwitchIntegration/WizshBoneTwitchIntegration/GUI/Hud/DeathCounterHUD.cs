using Jotunn.Managers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Top-center "Deaths: N" label for the playing character's vanilla Deaths stat, built like
    /// <see cref="StatusMessageHUD"/> and owned/ticked by <see cref="WizshBoneGUI"/>. Under it, a smaller
    /// list of the top viewers by deaths caused in the current world, from the Leaderboards.
    /// </summary>
    internal class DeathCounterHUD
    {
        private GameObject m_text;
        private Text m_label;
        private Text m_list;
        private int m_shownDeaths = -1;
        private int m_worldDeaths;
        private float m_nextRefresh;
        private bool m_wasShowingList;
        private bool m_wasShowingTotal = true;

        private const int TopCount = 3;

        // Gold, silver, bronze - indexed by place, so it must have TopCount entries.
        private static readonly string[] PlaceColors = { "#FFD700", "#C0C0C0", "#CD7F32" };

        // Must fit the label's 36-high box - Unity's default Text overflow truncates a line that
        // doesn't fit vertically, which is what hid it at the vanilla center-message size (40).
        private const int FontSize = 24;
        private const int ListFontSize = 18;
        private const float LabelHeight = 36f;
        private const float ListHeight = 76f;
        private const float RefreshInterval = 1f;

        public void Init()
        {
            if (m_text != null)
                return;

            m_text = GUIManager.Instance.CreateText(
                text: "",
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: FontSize,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 220f,
                height: LabelHeight,
                addContentSizeFitter: false
            );

            m_label = m_text.GetComponent<Text>();
            m_label.alignment = TextAnchor.MiddleCenter;
            m_label.raycastTarget = false; // purely informational - never swallow clicks along the top of the screen

            // A child of the label, anchored to its bottom edge, so it follows the label's
            // position, visibility and Ctrl+F3 hiding without any extra bookkeeping.
            GameObject list = GUIManager.Instance.CreateText(
                text: "",
                parent: m_text.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0f, -ListHeight / 2f - 2f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: ListFontSize,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 300f,
                height: ListHeight,
                addContentSizeFitter: false
            );

            m_list = list.GetComponent<Text>();
            m_list.alignment = TextAnchor.UpperCenter;
            m_list.raycastTarget = false;

            m_text.SetActive(false);
        }

        /// <summary>
        /// Called every frame by <see cref="WizshBoneGUI"/>: applies the setting, vanilla's Ctrl+F3
        /// "hide UI" toggle (this label lives on Jotunn's own canvas), the current death count and,
        /// about once a second, the current world's death count and the top viewers list.
        /// </summary>
        public void Refresh()
        {
            if (m_text == null)
                return;

            bool visible = ProfileSettingsHelper.Current.showDeathCounter
                && HudHelper.PlayerHasSpawned
                && !Hud.IsUserHidden();

            if (m_text.activeSelf != visible)
                m_text.SetActive(visible);

            if (!visible)
                return;

            bool showTotal = ProfileSettingsHelper.Current.showTotalDeathCount;
            bool showList = ProfileSettingsHelper.Current.showTopDeathCausers;

            if (m_list.gameObject.activeSelf != showList)
                m_list.gameObject.SetActive(showList);

            // Rebuild right away when the list is switched on or the count source changes, instead of
            // showing the old values until the timer is due.
            if (showList != m_wasShowingList || showTotal != m_wasShowingTotal)
            {
                m_wasShowingList = showList;
                m_wasShowingTotal = showTotal;
                m_nextRefresh = 0f;
            }

            // The leaderboard is summed, sorted and filtered on every call, so don't do it per frame.
            if (Time.unscaledTime >= m_nextRefresh)
            {
                m_nextRefresh = Time.unscaledTime + RefreshInterval;

                if (!showTotal)
                    m_worldDeaths = LeaderboardHelper.GetCurrentWorldDeaths();

                if (showList)
                    m_list.text = BuildList();
            }

            int deaths = showTotal ? DeathCountHelper.GetLocalDeaths() : m_worldDeaths;

            if (deaths != m_shownDeaths)
            {
                m_shownDeaths = deaths;
                m_label.text = $"Deaths: {deaths}";
            }
        }

        private static string BuildList()
        {
            List<LeaderboardRow> top = LeaderboardHelper.GetTopDeathCausers(TopCount);
            StringBuilder text = new StringBuilder();

            for (int i = 0; i < top.Count; i++)
            {
                if (i > 0)
                    text.Append('\n');

                text.Append($"<color={PlaceColors[i]}>{i + 1}. {top[i].name}: {top[i].deaths}</color>");
            }

            return text.ToString();
        }
    }
}
