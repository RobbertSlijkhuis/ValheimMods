using System.Collections;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class GameObjectExtension
    {
        public static IEnumerator SetActiveAfterDelay(this GameObject gameObject, float delay, bool value)
        {
            yield return new WaitForSeconds(delay);

            gameObject.SetActive(value);
        }

        public static IEnumerator SetActiveAllChildrenAfterDelay(this GameObject gameObject, float delay, bool value)
        {
            yield return new WaitForSeconds(delay);

            gameObject.SetActiveAllChildren(value);
        }

        public static bool SetActiveAllChildren(this GameObject gameObject, bool value)
        {
            CamShaker camShaker = gameObject.GetComponent<CamShaker>();

            if (camShaker != null)
            {
                camShaker.enabled = value;
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetActive(value);
            }

            return true;
        }

        public static bool DeleteInactiveChildren(this GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (!child.gameObject.activeSelf)
                    GameObject.Destroy(child.gameObject);
            }

            return true;
        }
    }
}