using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Reusable, select-only profile switcher: lists all profiles and lets the user click one to
    /// make it active. No create/rename/delete/import/export - that's <see cref="ProfilesTab"/>.
    /// </summary>
    internal class ProfileSidebar
    {
        public const float Width = 140f;

        private const float ButtonHeight  = 50f;
        private const float ButtonSpacing = 10f;
        private const float ListTopPadding = 15f;

        private GameObject m_listContainer;
        private Action m_onProfileChanged;

        /// <param name="titlePosition">Top-left position of the "Profiles" title.</param>
        /// <param name="listTopY">
        /// Top edge Y for the list box, in the same coordinate space as <paramref name="titlePosition"/>.
        /// Pass the caller's own list-top constant directly (e.g. the redeem list's own top offset)
        /// so the two boxes align exactly, rather than deriving it from a guessed title height/gap.
        /// </param>
        public void Create(GameObject parent, Vector2 titlePosition, float listTopY, float listHeight, Action onProfileChanged)
        {
            m_onProfileChanged = onProfileChanged;

            // CreateTabTitle's position is the box's center, not its left edge - offset by half-width
            // so the title actually starts flush with titlePosition.x.
            TabUIHelper.CreateTabTitle("Profiles", parent, new Vector2(titlePosition.x + Width / 2f, titlePosition.y), width: Width);

            Vector2 listPosition = new Vector2(titlePosition.x + Width / 2f, listTopY);

            // Matches the redeem list's own background instead of ListEditor.CreateScrollableList's
            // slightly lighter default.
            m_listContainer = ListEditor.CreateScrollableList(parent, listPosition, Width, listHeight, "ProfileSidebarList", backgroundColor: ScrollableView.DarkBackground);

            Refresh();
        }

        public void Refresh()
        {
            if (m_listContainer == null)
                return;

            TabUIHelper.ClearContainer(m_listContainer);

            List<string> profiles = ProfileManager.GetProfiles();
            float yOffset = -(ListTopPadding + ButtonHeight / 2f);

            foreach (string profile in profiles)
            {
                string capturedName = profile;
                bool isActive = profile == ProfileManager.ActiveProfile;

                GameObject btn = GUIManager.Instance.CreateButton(
                    text: profile,
                    parent: m_listContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, yOffset),
                    width: Width - 10f,
                    height: ButtonHeight
                );
                btn.SetActive(true);

                Button button = btn.GetComponent<Button>();
                btn.GetComponentInChildren<Text>().color = isActive
                    ? GUIManager.Instance.ValheimOrange
                    : GUIManager.Instance.ValheimBeige;

                // Active profile: just leave it colored differently, same as ProfilesTab.Refresh()
                // does for its own list - no onClick listener, but interactable stays true so it
                // doesn't render with Unity's grayed-out disabled look.
                if (!isActive)
                {
                    button.onClick.AddListener(() =>
                    {
                        ProfileManager.SelectProfile(capturedName);
                        Refresh();
                        m_onProfileChanged?.Invoke();
                    });
                }

                yOffset -= ButtonHeight + ButtonSpacing;
            }

            RectTransform contentRt = m_listContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ButtonHeight / 2f);
        }
    }
}
