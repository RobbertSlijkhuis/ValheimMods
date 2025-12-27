using ModularMagic_Utilities.Configs;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace ModularMagic_Utilities.Components
{
    internal class WeatherZoneProjectorControlsMMU : MonoBehaviour, Hoverable, Interactable
    {
        public WeatherZoneMMU weatherZone;
        public ParticleSystem particleSystem;
        public bool isRotating;

        private void Awake()
        {
            weatherZone = transform.parent.parent.Find("weatherzone").gameObject.GetComponent<WeatherZoneMMU>();
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
