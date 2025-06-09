using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ModularMagic_Utilities.components
{
    internal class WeatherZoneSettingsControls : MonoBehaviour, Hoverable, Interactable
    {
        public WeatherZone weatherZone;
        public Transform cogTransform;
        public bool isRotating;

        private void Awake ()
        {
            weatherZone = transform.parent.parent.Find("weatherzone").gameObject.GetComponent<WeatherZone>();
            cogTransform = transform.Find("cog");
        }

        public string GetHoverText()
        {
            StartRotateCog(cogTransform, 5f);
            return "[<color=yellow>E</color>] Settings";
        }
        public string GetHoverName()
        {
            return PluginConfig.piece1.name.Value;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
                return false;

            weatherZone.ShowWeatherZoneGUI();
            return true;
        }

        public void StartRotateCog(Transform trans, float duration)
        {
            if (!isRotating)
            {
                StartCoroutine(LerpTransform(trans, duration));
                isRotating = true;
                Invoke(nameof(resetIsRotating), 5f);
            }
        }

        private IEnumerator LerpTransform(Transform trans, float duration)
        {
            float timestep = 0;

            while (timestep <= duration)
            {
                timestep = timestep + Time.deltaTime;
                float step = Mathf.Clamp01(timestep / duration);
                trans.RotateAround(trans.position, Vector3.forward, step);
                yield return null;
            }
        }

        public void resetIsRotating()
        {
            isRotating = false;
        }
    }
}
