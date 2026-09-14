using UnityEngine;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Factory for creating invisible full-stretch <see cref="GameObject"/> containers
    /// used as tab roots, view groups, or other logical UI groupings.
    /// </summary>
    internal static class UIContainer
    {
        /// <summary>
        /// Creates an invisible full-stretch container parented to <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent">The parent <see cref="GameObject"/>.</param>
        /// <param name="name">Name of the generated <see cref="GameObject"/>.</param>
        /// <param name="startActive">Whether the container starts active. Defaults to <c>false</c>.</param>
        /// <returns>The created container.</returns>
        public static GameObject Create(GameObject parent, string name, bool startActive = false)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot     = new Vector2(0.5f, 0.5f);

            container.SetActive(startActive);
            return container;
        }
    }
}
