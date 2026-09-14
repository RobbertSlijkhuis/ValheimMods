using UnityEngine;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Renders every per-profile "rule" setting (profiles/&lt;name&gt;/profile.yaml's "settings:"
    /// key, see ProfileSettingsHelper/ProfileSettingsData) grouped the same way PluginConfig.cs used to
    /// group them. Rebuilt from scratch on every Refresh() (tab shown, or profile switched) so
    /// rows always reflect ProfileSettingsHelper.Current - there's no working-copy/Save button,
    /// each field writes straight through on edit (see RulesSettingsViews.cs).
    /// </summary>
    internal class RulesTab
    {
        private GameObject m_root;
        private GameObject m_scrollContent;

        // Mirrors the title/scroll-top-offset gap used by the other tabs' "title + scrollable
        // form" sub-views (e.g. ViewersTab.CreateCreateView), since Rules has the same plain
        // shape - a title, then one big scrollable body - with no search bar or sidebar.
        private const float TitleY = -153f;
        private const float ScrollTopOffset = -175f;
        private const float SectionStartX = -341f;
        private const float FieldWidth = 320f;
        private const float SectionHeaderHeight = 25f;
        private const float SectionSpacing = 20f;
        private const float StartY = -20f;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
        {
            m_root = UIContainer.Create(parent, "RulesTab");

            TabUIHelper.CreateTabTitle("Rules:", m_root, new Vector2(-200f, TitleY));

            m_scrollContent = createScrollable("RulesContent", m_root, ScrollTopOffset);

            Rebuild();

            return m_root;
        }

        public void Refresh()
        {
            Rebuild();
        }

        private void Rebuild()
        {
            TabUIHelper.ClearContainer(m_scrollContent);

            float yOffset = StartY;

            yOffset -= BuildSection("Chatting", new ChattingSettingsView(), yOffset) + SectionSpacing;
            yOffset -= BuildSection("Creatures", new CreaturesSettingsView(), yOffset) + SectionSpacing;
            yOffset -= BuildSection("Indestructible", new IndestructibleSettingsView(), yOffset) + SectionSpacing;
            yOffset -= BuildSection("Redeems", new RedeemsSettingsView(), yOffset) + SectionSpacing;
            yOffset -= BuildSection("Twitchy Ward", new WardSettingsView(), yOffset) + SectionSpacing;
            yOffset -= BuildSection("HUD", new HudSettingsView(), yOffset) + SectionSpacing;

            TabUIHelper.UpdateScrollContentHeight(m_scrollContent, StartY, Mathf.Abs(yOffset - StartY));
        }

        /// <returns>Total height consumed by this section's header + fields.</returns>
        private float BuildSection(string title, object sectionView, float yOffset)
        {
            TabUIHelper.CreateTabTitle(title, m_scrollContent, new Vector2(SectionStartX, yOffset), width: FieldWidth);

            float fieldsY = yOffset - SectionHeaderHeight;
            float fieldsHeight = new ObjectEditor(startX: SectionStartX, startY: fieldsY, fieldWidth: FieldWidth).Build(m_scrollContent, sectionView);

            return SectionHeaderHeight + fieldsHeight;
        }
    }
}
