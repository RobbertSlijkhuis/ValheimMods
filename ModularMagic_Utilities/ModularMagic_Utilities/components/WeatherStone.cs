using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Utilities.Components
{
    internal class WeatherStone : MonoBehaviour, Interactable
    {
        public EnvZone envZone;
        public bool isInitialised = false;

        private void Awake()
        {
            envZone = base.gameObject.GetComponent<EnvZone>();
            envZone.m_force = false;
        }

        public void StartLerpColor(Material mat, Color fromColor, Color toColor, float duration)
        {
            StartCoroutine(LerpColor(mat, fromColor, toColor, duration));
        }

        public IEnumerator LerpColor(Material mat, Color fromColor, Color toColor, float duration)
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

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            try
            {
                Jotunn.Logger.LogWarning("WeatherStone.UseItem");

                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
                return false;
            }
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            Jotunn.Logger.LogWarning("WeatherStone.Interact");
            return false;
        }
    }
}
