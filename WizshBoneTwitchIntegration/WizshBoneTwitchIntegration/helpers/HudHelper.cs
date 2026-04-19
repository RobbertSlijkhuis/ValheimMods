using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class HudHelper
    {
        /// <summary>
        /// Generate hud game objects to display more stars
        /// </summary>
        /// <param name="hudBase"></param>
        public static void GenerateLevels(RectTransform hudBase)
        {
            RectTransform refLevel = hudBase.transform.Find("level_2") as RectTransform;
            float moveRight = 16f;

            for (int index = 0; index < 10; index++)
            {
                GameObject level = UnityEngine.Object.Instantiate(refLevel.gameObject, hudBase);
                level.name = $"level_custom_{index + 2}";
                level.SetActive(false);

                GameObject star = level.transform.GetChild(0).gameObject;
                star.transform.localPosition = new Vector3(moveRight * index, 0f, 0f);
            }
        }
    }
}
