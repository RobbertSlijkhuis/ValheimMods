using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModularMagic_Utilities.components
{
    internal class WeatherZoneRadiusControls : MonoBehaviour, Hoverable, Interactable, TextReceiver
    {
        public WeatherZone weatherZone;

        private void Awake ()
        {
            weatherZone = transform.parent.Find("weatherzone").gameObject.GetComponent<WeatherZone>();
        }

        public string GetHoverText()
        {
            return "Set radius";
        }
        public string GetHoverName()
        {
            return PluginConfig.piece1.name.Value;
        }

        public void SetText(string value)
        {
            float radius = float.Parse(value);

            if (radius < 10f)
                radius = 10f;
            else if (radius > 100f)
                radius = 100f;

            weatherZone.SetRadius(radius);
        }

        public string GetText()
        {
            return (int)weatherZone.radius + "";
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
            {
                return false;
            }

            TextInput.instance.RequestText(this, "Enter a new radius", 3);
            return true;
        }
    }
}
