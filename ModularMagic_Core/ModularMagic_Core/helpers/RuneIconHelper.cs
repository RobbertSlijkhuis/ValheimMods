using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The game has one icon per item, but every level of a rune has its own model. The icon of every level is rendered
    /// at startup, and the icon of an item is picked by its quality.
    /// </summary>
    internal class RuneIconHelper
    {
        // The icons per rune prefab name, the index is the level minus one
        private static readonly Dictionary<string, Sprite[]> icons = new Dictionary<string, Sprite[]>();

        // Renders the icon of every level of the rune, returns them (index is the level minus one)
        public static Sprite[] Render(GameObject prefab, int maxLevel)
        {
            Transform attach = prefab.transform.Find("attach");
            Sprite[] sprites = new Sprite[maxLevel];

            for (int level = 1; level <= maxLevel; level++)
            {
                RuneModelHelper.ShowModel(attach, level);

                RenderManager.RenderRequest request = new RenderManager.RenderRequest(prefab);
                request.Rotation = RenderManager.IsometricRotation;
                sprites[level - 1] = RenderManager.Instance.Render(request);
            }

            // The prefab shows the first level by default
            RuneModelHelper.ShowModel(attach, 1);
            icons[prefab.name] = sprites;

            return sprites;
        }

        public static bool TryGetIcon(ItemDrop.ItemData item, out Sprite icon)
        {
            icon = null;

            if (item == null || item.m_dropPrefab == null)
                return false;

            if (!icons.TryGetValue(item.m_dropPrefab.name, out Sprite[] sprites))
                return false;

            int index = item.m_quality - 1;

            if (index < 0 || index >= sprites.Length || sprites[index] == null)
                return false;

            icon = sprites[index];
            return true;
        }
    }
}
