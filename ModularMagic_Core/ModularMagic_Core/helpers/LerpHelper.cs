using System.Collections;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class LerpHelper
    {
        public static IEnumerator LerpPosition(Transform transform, Vector3 start, Vector3 end, float duration, GameObject gameObject = null, bool destroy = false)
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

        public static IEnumerator SlerpPosition(Transform transform, Vector3 start, Vector3 end, float duration, GameObject gameObject = null, bool destroy = false)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                transform.localPosition = Vector3.Slerp(start, end, t);
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = end;

            if (gameObject && destroy)
                GameObject.Destroy(gameObject);
        }

        public static IEnumerator LerpPositionAndRotation(Transform transform, Vector3 startPos, Vector3 endPos, Vector3 startRot, Vector3 endRot, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                transform.localRotation = TransformHelper.GenerateRotation(Vector3.Lerp(startRot, endRot, t));
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = endPos;
            transform.localRotation = TransformHelper.GenerateRotation(endRot);
        }

        public static IEnumerator SlerpPositionAndRotation(Transform transform, Vector3 startPos, Vector3 endPos, Vector3 startRot, Vector3 endRot, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                transform.localPosition = Vector3.Slerp(startPos, endPos, t);
                transform.localRotation = TransformHelper.GenerateRotation(Vector3.Slerp(startRot, endRot, t));
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = endPos;
            transform.localRotation = TransformHelper.GenerateRotation(endRot);
        }

        public static IEnumerator LerpColor(Material mat, Color fromColor, Color toColor, float duration)
        {
            float timestep = 0;

            while (timestep <= duration)
            {
                timestep = timestep + Time.deltaTime;
                float step = Mathf.Clamp01(timestep / duration);
                Color color = Color.Lerp(fromColor, toColor, step);
                mat.SetColor("_EmissionColor", color);
                yield return null;
            }
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
