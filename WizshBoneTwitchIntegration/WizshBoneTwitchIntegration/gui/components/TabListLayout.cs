using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// One button placed in a tab's header row. Position is always computed by
    /// <see cref="TabListLayout"/> - only size/appearance/behavior are supplied here.
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
    /// One column header label shown above the scrollable list (see
    /// <see cref="TabListLayoutOptions.ColumnHeaders"/>). X/Width should reuse the exact same
    /// constants the tab already uses to position that column's row content inside MainContainer -
    /// their coordinate spaces coincide, since MainContainer is never left-inset.
    /// </summary>
    internal struct ColumnHeaderSpec
    {
        public string Text;
        public float X;
        public float Width;
        public TextAnchor Alignment;

        public ColumnHeaderSpec(string text, float x, float width, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            Text = text;
            X = x;
            Width = width;
            Alignment = alignment;
        }
    }

    /// <summary>
    /// Configures the reusable list-view shell built by <see cref="TabListLayout"/>. Only content
    /// (text, sizes the caller genuinely needs to choose, callbacks) lives here - every position and
    /// row-Y is computed internally by the layout itself, so a tab never re-derives the panel's
    /// content margins.
    /// </summary>
    internal class TabListLayoutOptions
    {
        public string TitleText;
        public float TitleWidth = 400f;

        public bool ShowSearchBar = true;
        public UnityAction<string> OnSearchChanged;
        public string SearchPlaceholder = "Search...";

        // Header buttons chain left-to-right, starting right after the search bar (or at the
        // content edge, if ShowSearchBar is false).
        public List<HeaderButtonSpec> HeaderButtons = new List<HeaderButtonSpec>();

        // Optional column headers shown just above the scrollable list, e.g. "Name" / "Actions".
        // Null (default) shows no header row.
        public List<ColumnHeaderSpec> ColumnHeaders;

        public string MainContainerName = "MainList";
    }

    internal class TabListLayoutResult
    {
        public GameObject ListView;
        public Text TitleLabel;
        public Text FeedbackText;
        public GameObject MainContainer;

        // The main container's top edge Y, in the same coordinate space as ListView - exposed so a
        // tab can place its own elements flush above the scrollable list without duplicating
        // TabListLayout's internal offset constants.
        public float ContentTopY;
    }

    /// <summary>
    /// Builds the list-view shell shared by every "browse a list, create/edit in a separate view"
    /// settings tab (Redeems, Viewers, Creature Groups, Profiles). Built once per tab open; never
    /// re-invoked on refresh - populating <see cref="TabListLayoutResult.MainContainer"/> stays
    /// entirely up to the caller.
    ///
    /// Every position/offset below is owned here, not by individual tabs, so all tabs stay visually
    /// consistent by construction.
    /// </summary>
    internal static class TabListLayout
    {
        private const float HeaderTopPadding = 45f;
        private const float HeaderRowY        = -(110f + HeaderTopPadding);
        private const float TitleRowGap       = 33f;
        private const float TitleRowY         = HeaderRowY - TitleRowGap;
        private const float ScrollGap         = 71f;
        private const float ContentTopOffset  = HeaderRowY - ScrollGap;

        private const float ButtonSpacing  = 5f;

        private const float SearchWidth       = 380f;

        private const float FeedbackTextWidth  = 400f;
        private const float FeedbackTextHeight = 25f;

        private const float ColumnHeaderGap    = 12f;
        private const float ColumnHeaderHeight = 20f;

        public static TabListLayoutResult Create(GameObject parent, string listViewName, CreateScrollableContainerDelegate createScrollable, TabListLayoutOptions options)
        {
            GameObject listView = UIContainer.Create(parent, listViewName);

            float effectiveLeftEdge = WizshBoneSettingsGUI.ContentLeftEdgeX;
            float rightEdge = -WizshBoneSettingsGUI.ContentLeftEdgeX;

            if (options.ShowSearchBar)
            {
                InputField searchField = FieldUIBuilder.CreateInputField(
                    parent: listView,
                    position: new Vector2(effectiveLeftEdge + SearchWidth / 2f, HeaderRowY),
                    width: SearchWidth
                );
                searchField.placeholder.GetComponent<Text>().text = options.SearchPlaceholder;
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

            GameObject mainContainer = createScrollable(options.MainContainerName, listView, ContentTopOffset);

            if (options.ColumnHeaders != null)
                CreateColumnHeaders(listView, options.ColumnHeaders);

            return new TabListLayoutResult
            {
                ListView = listView,
                TitleLabel = titleLabel,
                FeedbackText = feedbackText,
                MainContainer = mainContainer,
                ContentTopY = ContentTopOffset
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

        private static void CreateColumnHeaders(GameObject listView, List<ColumnHeaderSpec> columnHeaders)
        {
            float headerY = ContentTopOffset + ColumnHeaderGap;

            foreach (ColumnHeaderSpec spec in columnHeaders)
            {
                Text header = GUIManager.Instance.CreateText(
                    text: spec.Text,
                    parent: listView.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(spec.X, headerY),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: FieldUIBuilder.LabelFontSize,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: spec.Width,
                    height: ColumnHeaderHeight,
                    addContentSizeFitter: false
                ).GetComponent<Text>();
                header.alignment = spec.Alignment;
            }
        }
    }
}
