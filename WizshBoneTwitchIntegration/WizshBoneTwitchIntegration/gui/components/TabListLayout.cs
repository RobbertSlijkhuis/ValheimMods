using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// One button placed in a tab's header row, or in the standalone sidebar accessory slot
    /// (e.g. Redeems' "Reload" button, which sits above the sidebar rather than in the header row).
    /// Position is always computed by <see cref="TabListLayout"/> - only size/appearance/behavior
    /// are supplied here.
    /// </summary>
    internal struct HeaderButtonSpec
    {
        public string Text;
        public float Width;
        public float Height;
        public Color? TextColor;
        public UnityAction OnClick;
        public Action<GameObject> OnCreated;

        public HeaderButtonSpec(string text, float width, UnityAction onClick, float height = 36f, Color? textColor = null, Action<GameObject> onCreated = null)
        {
            Text = text;
            Width = width;
            Height = height;
            TextColor = textColor;
            OnClick = onClick;
            OnCreated = onCreated;
        }
    }

    /// <summary>
    /// A standalone button above the sidebar (e.g. Redeems' "Reload"). Unlike <see cref="HeaderButtonSpec"/>,
    /// its size is never tab-supplied: width always matches the sidebar's width ("so both edges line
    /// up") and height is a fixed presentation choice - both computed by <see cref="TabListLayout"/>.
    /// </summary>
    internal struct SidebarAccessoryButtonSpec
    {
        public string Text;
        public Color? TextColor;
        public UnityAction OnClick;
        public Action<GameObject> OnCreated;

        public SidebarAccessoryButtonSpec(string text, UnityAction onClick, Color? textColor = null, Action<GameObject> onCreated = null)
        {
            Text = text;
            OnClick = onClick;
            TextColor = textColor;
            OnCreated = onCreated;
        }
    }

    /// <summary>
    /// Configures the reusable list-view shell built by <see cref="TabListLayout"/>. Only content
    /// (text, sizes the caller genuinely needs to choose, callbacks) lives here - every position,
    /// row-Y, and sidebar-dependent inset is computed internally by the layout itself, so a tab
    /// never re-derives the panel's content margins.
    /// </summary>
    internal class TabListLayoutOptions
    {
        public string TitleText;
        public float TitleWidth = 400f;

        public bool ShowSearchBar = true;
        public UnityAction<string> OnSearchChanged;

        // Header buttons chain left-to-right, starting right after the search bar (or at the
        // content edge, if ShowSearchBar is false).
        public List<HeaderButtonSpec> HeaderButtons = new List<HeaderButtonSpec>();

        // Independent of the header button chain above - e.g. Redeems' "Reload" button, which sits
        // in its own column above the sidebar, flush at the panel's true content edge.
        public SidebarAccessoryButtonSpec? SidebarAccessoryButton;

        // Sidebar - optional. When enabled, the layout builds the sidebar's title and scrollable
        // box itself (matching the main container's scrollbar styling, and auto-shifting the
        // title/search bar/header buttons/main container right to make room for it) and hands the
        // empty content container to PopulateSidebar; the caller only ever populates rows into it.
        public bool ShowSidebar;
        public float SidebarWidth;
        public string SidebarTitleText = "Profiles";
        public Action<GameObject> PopulateSidebar;

        public string MainContainerName = "MainList";
    }

    internal class TabListLayoutResult
    {
        public GameObject ListView;
        public Text TitleLabel;
        public Text FeedbackText;
        public GameObject MainContainer;
        public GameObject SidebarContainer;
    }

    /// <summary>
    /// Builds the list-view shell shared by every "browse a list, create/edit in a separate view"
    /// settings tab (Redeems, Viewers, and future tabs like Creature Groups). Built once per tab
    /// open; never re-invoked on refresh - populating <see cref="TabListLayoutResult.MainContainer"/>
    /// and <see cref="TabListLayoutResult.SidebarContainer"/> stays entirely up to the caller.
    ///
    /// Every position/offset below is owned here, not by individual tabs, so all tabs stay visually
    /// consistent by construction - when <see cref="TabListLayoutOptions.ShowSidebar"/> is set, the
    /// title, search bar, header buttons, and main container all shift right automatically to make
    /// room for it.
    /// </summary>
    internal static class TabListLayout
    {
        private const float HeaderTopPadding = 45f;
        private const float HeaderRowY        = -(110f + HeaderTopPadding);
        private const float TitleRowGap       = 53f;
        private const float TitleRowY         = HeaderRowY - TitleRowGap;
        private const float ScrollGap         = 71f;
        private const float ContentTopOffset  = HeaderRowY - ScrollGap;

        private const float SidebarGutter  = 20f;
        private const float ButtonSpacing  = 5f;

        private const float SearchWidth       = 380f;
        private const string SearchPlaceholder = "Search...";

        private const float FeedbackTextWidth  = 400f;
        private const float FeedbackTextHeight = 25f;

        private const float SidebarAccessoryButtonHeight = 50f;

        public static TabListLayoutResult Create(GameObject parent, string listViewName, CreateScrollableContainerDelegate createScrollable, TabListLayoutOptions options)
        {
            GameObject listView = UIContainer.Create(parent, listViewName);

            float effectiveLeftEdge = WizshBoneSettingsGUI.ContentLeftEdgeX
                + (options.ShowSidebar ? options.SidebarWidth + SidebarGutter : 0f);
            float rightEdge = -WizshBoneSettingsGUI.ContentLeftEdgeX;
            float mainContainerLeftInset = options.ShowSidebar ? options.SidebarWidth + SidebarGutter : 0f;

            if (options.SidebarAccessoryButton.HasValue)
            {
                SidebarAccessoryButtonSpec accessorySpec = options.SidebarAccessoryButton.Value;
                var spec = new HeaderButtonSpec(accessorySpec.Text, options.SidebarWidth, accessorySpec.OnClick,
                    SidebarAccessoryButtonHeight, accessorySpec.TextColor, accessorySpec.OnCreated);
                float accessoryCenterX = WizshBoneSettingsGUI.ContentLeftEdgeX + options.SidebarWidth / 2f;
                CreateHeaderButton(listView, spec, new Vector2(accessoryCenterX, HeaderRowY));
            }

            if (options.ShowSearchBar)
            {
                InputField searchField = FieldUIBuilder.CreateInputField(
                    parent: listView,
                    position: new Vector2(effectiveLeftEdge + SearchWidth / 2f, HeaderRowY),
                    width: SearchWidth
                );
                searchField.placeholder.GetComponent<Text>().text = SearchPlaceholder;
                if (options.OnSearchChanged != null)
                    searchField.onValueChanged.AddListener(options.OnSearchChanged);
            }

            float cursor = effectiveLeftEdge + (options.ShowSearchBar ? SearchWidth : 0f);
            foreach (HeaderButtonSpec spec in options.HeaderButtons)
            {
                float centerX = cursor + ButtonSpacing + spec.Width / 2f;
                CreateHeaderButton(listView, spec, new Vector2(centerX, HeaderRowY));
                cursor = centerX + spec.Width / 2f;
            }

            Text titleLabel = TabUIHelper.CreateTabTitle(
                options.TitleText,
                listView,
                new Vector2(effectiveLeftEdge + options.TitleWidth / 2f, TitleRowY),
                width: options.TitleWidth
            );

            Text feedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: listView.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2((effectiveLeftEdge + rightEdge) / 2f, TitleRowY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: TabUIHelper.TitleFontSize,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: FeedbackTextWidth,
                height: FeedbackTextHeight,
                addContentSizeFitter: false
            ).GetComponent<Text>();
            feedbackText.alignment = TextAnchor.MiddleCenter;

            GameObject sidebarContainer = null;
            if (options.ShowSidebar)
            {
                // CreateTabTitle's position is the box's center, not its left edge - offset by
                // half-width so the title and box both start flush with the sidebar's left edge.
                float sidebarCenterX = WizshBoneSettingsGUI.ContentLeftEdgeX + options.SidebarWidth / 2f;

                // Exactly matches the main container's own visible height (top edge at
                // ContentTopOffset, bottom edge at ContentBottomMargin above the panel's bottom),
                // so the two boxes end at the same Y.
                float sidebarListHeight = ContentTopOffset + WizshBoneSettingsGUI.PanelHeight - WizshBoneSettingsGUI.ContentBottomMargin;

                TabUIHelper.CreateTabTitle(options.SidebarTitleText, listView, new Vector2(sidebarCenterX, TitleRowY), width: options.SidebarWidth);

                sidebarContainer = ScrollableView.CreateFixed(
                    listView,
                    "Sidebar",
                    new Vector2(sidebarCenterX, ContentTopOffset),
                    options.SidebarWidth,
                    sidebarListHeight,
                    backgroundColor: ScrollableView.DarkBackground,
                    autoHideScrollbar: true
                );

                options.PopulateSidebar?.Invoke(sidebarContainer);
            }

            GameObject mainContainer = createScrollable(options.MainContainerName, listView, ContentTopOffset, mainContainerLeftInset);

            return new TabListLayoutResult
            {
                ListView = listView,
                TitleLabel = titleLabel,
                FeedbackText = feedbackText,
                MainContainer = mainContainer,
                SidebarContainer = sidebarContainer
            };
        }

        private static void CreateHeaderButton(GameObject parent, HeaderButtonSpec spec, Vector2 position)
        {
            GameObject btnObj = GUIManager.Instance.CreateButton(
                text: spec.Text,
                parent: parent.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: position,
                width: spec.Width,
                height: spec.Height
            );
            btnObj.SetActive(true);

            if (spec.TextColor.HasValue)
                btnObj.GetComponentInChildren<Text>().color = spec.TextColor.Value;

            if (spec.OnClick != null)
                btnObj.GetComponent<Button>().onClick.AddListener(spec.OnClick);

            spec.OnCreated?.Invoke(btnObj);
        }
    }
}
