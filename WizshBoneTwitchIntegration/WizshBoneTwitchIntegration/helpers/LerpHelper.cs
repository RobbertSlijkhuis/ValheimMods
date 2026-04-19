using System.Collections;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
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

        public static IEnumerator LerpScale(Transform transform, Vector3 start, Vector3 end, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                transform.localScale = Vector3.Lerp(start, end, t);
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            transform.localScale = end;
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

        public static IEnumerator LerpLight(Light light, float from, float to, float duration)
        {
            float timestep = 0;

            while (timestep <= duration)
            {
                timestep = timestep + Time.deltaTime;
                float step = Mathf.Clamp01(timestep / duration);
                float result = Mathf.Lerp(from, to, step);
                light.intensity = result;
                yield return null;
            }
        }

        public static IEnumerator LerpLightFlicker(LightFlicker light, float from, float to, float duration)
        {
            float timestep = 0;

            while (timestep <= duration)
            {
                timestep = timestep + Time.deltaTime;
                float step = Mathf.Clamp01(timestep / duration);
                float result = Mathf.Lerp(from, to, step);
                light.m_flickerIntensity = result;
                yield return null;
            }
        }

        public static IEnumerator LerpCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
        {
            float timestep = 0;

            while (timestep <= duration)
            {
                timestep = timestep + Time.deltaTime;
                float step = Mathf.Clamp01(timestep / duration);
                float result = Mathf.Lerp(from, to, step);
                canvasGroup.alpha = result;
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
