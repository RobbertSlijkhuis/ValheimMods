using System.Collections;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class LerpHelper
    {
        public static IEnumerator LerpTransform(Transform transform, Vector3 start, Vector3 end, float duration, GameObject gameObject = null, bool destroy = false)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                transform.localPosition = Vector3.Lerp(start, end, t);
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = end;

            if (gameObject && destroy)
                GameObject.Destroy(gameObject);
        }

        public static Vector3 RandomPosition(Vector3 start, float min, float max)
        {
            Vector3 end = new Vector3(start.x, start.y, start.z);
            end.x = start.x + Random.Range(min, max);
            end.y = start.y + Random.Range(min, max);

            return end;
        }
    }
}
