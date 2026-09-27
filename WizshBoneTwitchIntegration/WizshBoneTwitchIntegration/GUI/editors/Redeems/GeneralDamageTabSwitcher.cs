using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The small "General"/"Damage" tab switcher shared by every step-2 form that has a Damage
    /// tab (Detonate + Windmill/Smite/Rain/Meteor/Trap/Root/LogRain) - two tab buttons at the top
    /// of the form's content area, and two content roots toggled via SetActive. Mirrors
    /// <see cref="SelectorList"/>'s own "capture the default color, recolor on select" highlight
    /// pattern, reusing <see cref="ShellSidebar.TabActiveColor"/> for the selected tab.
    /// </summary>
    internal class GeneralDamageTabSwitcher
    {
        private const float TabHeight = 32f;
        private const float TabWidth = 140f;
        private const float TabGap = 8f;

        private Image m_generalBg;
        private Image m_damageBg;
        private Color m_defaultColor;

        private GameObject m_scrollContent;
        private bool m_generalActive = true;
        private float m_generalHeight;
        private float m_damageHeight;

        public GameObject GeneralRoot { get; private set; }
        public GameObject DamageRoot { get; private set; }

        /// <summary>
        /// Y the General/Damage content roots' own rows should start at - always 0 now that both
        /// panes are parented inside their own <see cref="ScrollableList"/> content (whose own top
        /// edge is already anchored at the old fixed tabY-derived offset), rather than sharing the
        /// outer, non-scrolling parent's coordinate frame the way they used to.
        /// </summary>
        public float ContentTopY => 0f;

        public void Build(GameObject parent)
        {
            float tabY = RedeemWizard.BodyTopY;

            GameObject generalBtnObj = GuiHelper.CreateButton(
                text: "General",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-(TabWidth + TabGap) / 2f, tabY),
                width: TabWidth,
                height: TabHeight
            );
            generalBtnObj.SetActive(true);
            m_generalBg = generalBtnObj.GetComponent<Image>();
            m_defaultColor = m_generalBg.color;
            generalBtnObj.GetComponent<Button>().onClick.AddListener(() => SelectTab(true));

            GameObject damageBtnObj = GuiHelper.CreateButton(
                text: "Damage",
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2((TabWidth + TabGap) / 2f, tabY),
                width: TabWidth,
                height: TabHeight
            );
            damageBtnObj.SetActive(true);
            m_damageBg = damageBtnObj.GetComponent<Image>();
            damageBtnObj.GetComponent<Button>().onClick.AddListener(() => SelectTab(false));

            float contentTopY = tabY - TabHeight - RedeemWizard.RowGap;

            // One shared scroll container for both panes - only one is ever visible at a time, so
            // one scrollbar (recomputed on every tab switch/row rebuild) is simpler than juggling
            // two independent ScrollableLists' visibility in lockstep with SelectTab.
            m_scrollContent = ScrollableList.CreateFixed(
                parent, "TabContent",
                anchoredPosition: new Vector2(0f, contentTopY),
                width: RedeemWizard.Step2FieldWidth,
                // The tab buttons + gap eat into the same overall span Step2ContentHeight already
                // measures from BodyTopY, so the content box below them gets that much less height.
                height: RedeemWizard.Step2ContentHeight - TabHeight - RedeemWizard.RowGap,
                autoHideScrollbar: true);

            GeneralRoot = UIContainer.Create(m_scrollContent, "General");
            DamageRoot = UIContainer.Create(m_scrollContent, "Damage");

            SelectTab(true);
        }

        /// <summary>Called by the owning form after (re)building the General pane's rows, with that <see cref="Step2RowLayout.CurrentY"/>.</summary>
        public void SetGeneralContentHeight(float currentY)
        {
            m_generalHeight = Mathf.Abs(currentY);
            if (m_generalActive)
                ScrollableList.SetContentHeight(m_scrollContent, m_generalHeight);
        }

        /// <summary>Called by the owning form after (re)building the Damage pane's rows, with that <see cref="Step2RowLayout.CurrentY"/>.</summary>
        public void SetDamageContentHeight(float currentY)
        {
            m_damageHeight = Mathf.Abs(currentY);
            if (!m_generalActive)
                ScrollableList.SetContentHeight(m_scrollContent, m_damageHeight);
        }

        private void SelectTab(bool general)
        {
            m_generalActive = general;
            GeneralRoot.SetActive(general);
            DamageRoot.SetActive(!general);
            m_generalBg.color = general ? ShellSidebar.TabActiveColor : m_defaultColor;
            m_damageBg.color = !general ? ShellSidebar.TabActiveColor : m_defaultColor;
            ScrollableList.SetContentHeight(m_scrollContent, general ? m_generalHeight : m_damageHeight);
        }
    }
}
