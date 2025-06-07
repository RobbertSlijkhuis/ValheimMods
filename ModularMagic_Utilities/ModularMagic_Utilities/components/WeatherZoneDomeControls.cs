using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace ModularMagic_Utilities.components
{
    internal class WeatherZoneDomeControls : MonoBehaviour, Hoverable, Interactable
    {
        public WeatherZone weatherZone;

        private void Awake ()
        {
            weatherZone = transform.parent.Find("weatherzone").gameObject.GetComponent<WeatherZone>();
        }

        public string GetHoverText()
        {
            return (weatherZone.domeEnabled ? "Disable" : "Enable") + " dome";
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

            weatherZone.SetEnableDome(!weatherZone.domeEnabled);
            return true;
        }
    }
}
