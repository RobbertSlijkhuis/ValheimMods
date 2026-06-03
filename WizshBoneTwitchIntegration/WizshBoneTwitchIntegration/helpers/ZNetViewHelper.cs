using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class ZNetViewHelper
    {
        public static void Destroy(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            ZNetView netView = gameObject.GetComponent<ZNetView>();

            if (netView != null)
                netView.Destroy();
            else
                UnityEngine.Object.Destroy(gameObject);
        }

        public static void Destroy(GameObject gameObject, float delay)
        {
            if (gameObject == null)
                return;

            ZNetView netView = gameObject.GetComponent<ZNetView>();

            if (netView != null)
                netView.Destroy();
            else
                UnityEngine.Object.Destroy(gameObject, delay);
        }
    }
}