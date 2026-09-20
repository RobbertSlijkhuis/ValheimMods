using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// A rune prefab has an "attach" child with one model per level (rune_1, rune_2, ...), only one of them is shown.
    /// </summary>
    internal class RuneModelHelper
    {
        private const string ModelPrefix = "rune_";

        // The number of levels a rune prefab has a model for
        public static int CountModels(GameObject prefab)
        {
            Transform attach = prefab.transform.Find("attach");

            if (attach == null)
                return 0;

            int count = 0;

            while (attach.Find($"{ModelPrefix}{count + 1}") != null)
            {
                count++;
            }

            return count;
        }

        // Returns the model of the level, or null when the rune has no model for that level
        public static Transform ShowModel(Transform attach, int level)
        {
            Transform shown = null;

            foreach (Transform child in attach)
            {
                if (!child.name.StartsWith(ModelPrefix))
                    continue;

                bool isLevel = child.name == $"{ModelPrefix}{level}";

                child.gameObject.SetActive(isLevel);

                if (isLevel)
                    shown = child;
            }

            return shown;
        }
    }
}
