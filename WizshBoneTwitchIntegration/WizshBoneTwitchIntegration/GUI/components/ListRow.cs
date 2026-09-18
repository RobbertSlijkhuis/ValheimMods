using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Shared row-building mechanics for a sortable/searchable list tab (row shell, hover-reveal
    /// wiring, action-button factory) - extracted from <see cref="Tabs.ProfilesTab"/>'s row code
    /// once <see cref="Tabs.ViewersTab"/> needed the exact same plumbing. Column layout and cell
    /// content stay bespoke per tab (name text vs. a color swatch vs. a status badge differ enough
    /// per data model that forcing them through one generic row/column spec just adds indirection -
    /// see GUI_OLD/components/TabListLayout.cs, which only ever generalized the chrome around a
    /// list, never the rows themselves).
    /// </summary>
    internal static class ListRow
    {
        public const float ItemHeight = 40f;
        public const float ItemSpacing = 5f;
        public const float ListTopPadding = 10f;

        /// <summary>
        /// Left inset for a list's first column, shared by every sortable-list tab (Profiles/
        /// Viewers/Redeems) so their row content isn't flush against the list's true left edge -
        /// matches RedesignUI.dc.html's consistent 12px left column padding.
        /// </summary>
        public const float LeftPadding = 12f;

        /// <summary>
        /// Creates a row inside <paramref name="container"/>, anchored at <paramref name="yOffset"/>.
        /// Returns two separate <see cref="GameObject"/>s with two separate jobs - see the comment
        /// below for why they can't be the same object:
        ///
        /// - <c>Background</c>'s <see cref="GameObject"/> (also carries <see cref="RowHoverReveal"/>
        ///   once <see cref="AttachHoverReveal"/> runs) sits flush with <paramref name="container"/>'s
        ///   actual bounds, unshifted, so the hover-darken background and click/hover detection cover
        ///   the row's full real width with no gap.
        /// - <c>Row</c> is where callers should parent their own cell content (text, swatches,
        ///   <see cref="CreateActionButton"/> buttons) - it's nudged <see cref="ScrollableList.ScrollbarWidth"/>/2
        ///   to the right of Background/container to cancel a drift <see cref="ScrollableList"/>
        ///   introduces: its viewport only insets for the scrollbar on the right
        ///   (<c>offsetMax = -ScrollbarWidth</c>, never the left), so each nested stretch
        ///   (viewport -> content -> row) that recenters on its own local pivot shifts a naively-
        ///   centered row's contents that same amount left of where a column header built directly
        ///   under the tab root would render at the identical literal X. Without this second,
        ///   shifted object, compensating by moving the row itself (background included) just trades
        ///   the clipped-text bug for a background that no longer reaches the row's true left edge.
        /// </summary>
        public static (GameObject Row, Image Background) Create(GameObject container, string name, float yOffset)
        {
            GameObject outer = new GameObject(name);
            outer.transform.SetParent(container.transform, false);

            RectTransform outerRt = outer.AddComponent<RectTransform>();
            outerRt.anchorMin        = new Vector2(0f, 1f);
            outerRt.anchorMax        = new Vector2(1f, 1f);
            outerRt.pivot            = new Vector2(0.5f, 0.5f);
            outerRt.anchoredPosition = new Vector2(0f, yOffset);
            outerRt.sizeDelta        = new Vector2(0f, ItemHeight);

            Image background = outer.AddComponent<Image>();

            GameObject content = new GameObject(name + "Content");
            content.transform.SetParent(outer.transform, false);

            RectTransform contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin        = Vector2.zero;
            contentRt.anchorMax        = Vector2.one;
            contentRt.pivot            = new Vector2(0.5f, 0.5f);
            contentRt.anchoredPosition = new Vector2(ScrollableList.ScrollbarWidth / 2f, 0f);
            contentRt.sizeDelta        = Vector2.zero;

            return (content, background);
        }

        /// <summary>
        /// Creates a small colored-text, colored-border action button (Edit/Delete/Copy/etc.)
        /// centered at <paramref name="x"/> within <paramref name="row"/>.
        /// </summary>
        public static GameObject CreateActionButton(GameObject row, string text, float x, float width, float height, Color color, UnityAction onClick)
        {
            GameObject btnObj = GUIManager.Instance.CreateButton(
                text: text,
                parent: row.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(x, 0f),
                width: width,
                height: height
            );
            btnObj.GetComponentInChildren<Text>().color = color;
            GuiHelper.AddBorder(btnObj, color);
            btnObj.GetComponent<Button>().onClick.AddListener(onClick);
            return btnObj;
        }

        /// <summary>
        /// Wires up the standard hover-darken-and-reveal-actions behavior via
        /// <see cref="RowHoverReveal"/>. Attaches to <paramref name="background"/>'s own
        /// <see cref="GameObject"/> - not <paramref name="row"/> - since Unity's UI pointer events
        /// are delivered to whichever <see cref="GameObject"/> actually owns the raycast-hit
        /// <see cref="Graphic"/>, not bubbled up to parents; <c>Row</c> (see <see cref="Create"/>)
        /// has no <see cref="Graphic"/> of its own to be hit.
        /// </summary>
        /// <param name="normalColor">
        /// The row's idle background color, restored on pointer-exit - pass <see cref="ZebraColor"/>
        /// for a striped list, or <see cref="Color.clear"/> for a plain one.
        /// </param>
        public static void AttachHoverReveal(GameObject row, Image background, List<GameObject> revealOnHover, Color normalColor)
        {
            background.gameObject.AddComponent<RowHoverReveal>().Init(background, revealOnHover, normalColor);
        }

        /// <summary>
        /// The alternating-row-stripe color used for every list - shared here so Profiles/Viewers/
        /// Redeems/RedeemHistory all compute the same stripe instead of each hardcoding it. Alpha
        /// bumped to 0.4 from RedesignUI.dc.html's own 0.3 (<c>zebra: i % 2 ? 'transparent' :
        /// '#00000030'</c>) per Robbert's request.
        /// </summary>
        public static Color ZebraColor(int rowIndex) => rowIndex % 2 == 0 ? new Color(0f, 0f, 0f, 0.4f) : Color.clear;
    }
}
