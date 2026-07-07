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

        /// <summary>
        /// Populates profile buttons into a content container already built by
        /// <see cref="TabListLayout"/> (title and scrollable box are its responsibility, so both
        /// share the same scrollbar styling as the tab's main list).
        /// </summary>
        public void Create(GameObject contentContainer, Action onProfileChanged)
        {
            m_listContainer = contentContainer;
            m_onProfileChanged = onProfileChanged;

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
                    // Width is the total column width including the scrollbar's own track
                    // (ScrollableView.ScrollbarWidth) - subtract that first so the button actually
                    // fits inside the content area instead of overflowing into the scrollbar.
                    width: Width - ScrollableView.ScrollbarWidth - 10f,
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
