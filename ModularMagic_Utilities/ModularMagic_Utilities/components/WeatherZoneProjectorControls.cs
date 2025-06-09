using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace ModularMagic_Utilities.components
{
    internal class WeatherZoneProjectorControls : MonoBehaviour, Hoverable, Interactable
    {
        public WeatherZone weatherZone;
        public ParticleSystem particleSystem;
        public bool isRotating;

        private void Awake ()
        {
            weatherZone = transform.parent.parent.Find("weatherzone").gameObject.GetComponent<WeatherZone>();
            particleSystem = transform.Find("circle").gameObject.GetComponent<ParticleSystem>();
            isRotating = false;

            Invoke(nameof(InitParticleSystem), 1f);
        }

        public string GetHoverText()
        {
            StartRotateCircle();
            return "[<color=yellow>E</color>] " + (weatherZone.projectorEnabled ? "Disable" : "Enable") + " radius projector";
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

            weatherZone.SetProjector(!weatherZone.projectorEnabled);
            return true;
        }

        public void InitParticleSystem()
        {
            particleSystem.Pause();
            VelocityOverLifetimeModule velocityOverLifeTime = particleSystem.velocityOverLifetime;
            velocityOverLifeTime.enabled = true;
        }

        public void StartRotateCircle()
        {
            if (!isRotating)
            {
                particleSystem.Play();
                isRotating = true;
                Invoke(nameof(resetIsRotating), 5f);
            }
        }

        public void resetIsRotating()
        {
            particleSystem.Pause();
            isRotating = false;
        }
    }
}
