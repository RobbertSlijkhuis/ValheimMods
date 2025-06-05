using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Utilities.Components
{
    internal class WeatherZone : MonoBehaviour
    {
        public EnvZone envZone;
        public CircleProjector projector;
        public Transform startEffectTrans;
        public float radius;
        public bool isInitialised = false;
        public bool enableDome;
        public bool enableProjector;
        public EffectList startEffects = new EffectList();
        public EffectList stopEffects = new EffectList();
        public Color emissionColor = new Color(0f, 0.5676858f, 1.294612f, 1f);
        public Color emissionOffColor = new Color(0f, 0f, 0f, 1f);
        public Color emissionRuneOffColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public float projectorMultiplier = 2.6f;

        private void Awake()
        {
            try
            {
                startEffectTrans = transform.parent.Find("drop_spawn");
                envZone = GetComponent<EnvZone>();
                envZone.m_force = false;
                projector = transform.parent.Find("New/projector").gameObject.GetComponent<CircleProjector>();
                radius = 30f;
                enableDome = false;
                enableProjector = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not finish WeatherZone Awake: "+ e);
            }
        }

        public void InitWeather(string itemName)
        {
            Jotunn.Logger.LogWarning("WeatherZone.InitWeather");
            string newEnv = "";
            isInitialised = true;

            if (itemName == PluginConfig.spellbook1.name.Value)
                newEnv = "Clear";

            if (itemName == PluginConfig.spellbook2.name.Value)
                newEnv = "ThunderStorm";

            if (itemName == PluginConfig.lantern2.name.Value)
                newEnv = "SnowStorm";

            if (envZone.m_environment == newEnv)
                return;

            envZone.m_environment = newEnv;
            _SetEffects(true);
        }

        public void EnableWeather(ItemDrop.ItemData item)
        {
            try
            {
                Jotunn.Logger.LogWarning("WeatherZone.EnableWeather");
                List<string> nameList = new List<string>()
                {
                    PluginConfig.spellbook1.name.Value,
                    PluginConfig.spellbook2.name.Value,
                    PluginConfig.spellbook3.name.Value,
                    PluginConfig.lantern1.name.Value,
                    PluginConfig.lantern2.name.Value,
                    PluginConfig.lantern3.name.Value,
                };

                if (!nameList.Contains(item.m_shared.m_name))
                    return;

                string newEnv = "";

                if (item.m_shared.m_name == PluginConfig.spellbook1.name.Value)
                    newEnv = "Clear";

                if (item.m_shared.m_name == PluginConfig.spellbook2.name.Value)
                    newEnv = "ThunderStorm";

                if (item.m_shared.m_name == PluginConfig.lantern2.name.Value)
                    newEnv = "SnowStorm";

                if (newEnv == "")
                    return;

                envZone.m_environment = newEnv;
                _SetEffects(true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        public void DisableWeather()
        {
            Jotunn.Logger.LogWarning("WeatherZone.DisableWeather");
            if (envZone.m_environment == "")
                return;

            envZone.m_environment = "";
            _SetEffects(false);
        }

        public void SetRadius(float value)
        {
            Jotunn.Logger.LogWarning("WeatherZone.SetRadius() " + value);
            Transform weatherZoneTrans = transform.parent.Find("weatherzone");
            Transform forceFieldTrans = transform.parent.Find("New/forcefield");
            Transform projectorTrans = transform.parent.Find("New/projector");
            radius = value;

            if (weatherZoneTrans != null)
            {
                Jotunn.Logger.LogWarning("Change weatherzone radius...");
                weatherZoneTrans.gameObject.GetComponent<SphereCollider>().radius = radius;
                Jotunn.Logger.LogWarning("Radius is now: " + weatherZoneTrans.gameObject.GetComponent<SphereCollider>().radius);
            }

            if (forceFieldTrans != null)
                forceFieldTrans.localScale = new Vector3(radius * 2, radius * 2, radius * 2);

            if (projectorTrans != null)
            {
                CircleProjector projector = projectorTrans.gameObject.GetComponent<CircleProjector>();
                projector.m_radius = radius;
                projector.m_nrOfSegments = Mathf.FloorToInt(radius * projectorMultiplier);
            }
        }

        public void SetEnableDome(bool value)
        {
            Jotunn.Logger.LogWarning("WeatherZone.SetEnableDome() " + value);
            Transform forceFieldTrans = transform.parent.Find("New/forcefield");

            if (forceFieldTrans != null)
                forceFieldTrans.gameObject.SetActive(value);
        }

        public void SetEnableProjector(bool value)
        {
            Jotunn.Logger.LogWarning("WeatherZone.SetEnableProjector() " + value);
            Transform projectorTrans = transform.parent.Find("New/projector");

            if (projectorTrans != null)
                projectorTrans.gameObject.SetActive(value);
        }


        private void _SetEffects(bool value)
        {
            Jotunn.Logger.LogWarning("WeatherZone._SetEffects() " + value);
            try
            {
                Transform emissionTrans = transform.parent.Find("New/emission");
                Transform runesTrans = transform.parent.Find("New/runes emission");
                Transform forceFieldTrans = transform.parent.Find("New/forcefield");
                Transform projectorTrans = transform.parent.Find("New/projector");

                GetComponent<SphereCollider>().radius = PluginConfig.piece1.weatherZoneRadius.Value;

                if (emissionTrans != null)
                {
                    MeshRenderer meshRendererComp = emissionTrans.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];
                    StartLerpColor(mat, value ? emissionOffColor : emissionColor, value ? emissionColor : emissionOffColor, 3f);

                    Transform particleTrans = emissionTrans.gameObject.transform.Find("particles");
                    ParticleSystem particles = particleTrans.gameObject.GetComponent<ParticleSystem>();

                    Transform particleFastTrans = emissionTrans.gameObject.transform.Find("particles_fast");
                    ParticleSystem particlesFast = particleFastTrans.gameObject.GetComponent<ParticleSystem>();

                    if (value)
                    {
                        particles.Play();
                        particlesFast.Play();
                    }
                    else
                    {
                        particles.Stop();
                        particlesFast.Stop();
                    }

                    Transform lightsTrans = emissionTrans.gameObject.transform.Find("light");
                    lightsTrans.gameObject.SetActive(value);
                }

                if (runesTrans != null)
                {
                    MeshRenderer meshRendererComp = runesTrans.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];
                    StartLerpColor(mat, value ? emissionRuneOffColor : emissionColor, value ? emissionColor : emissionRuneOffColor, 1.5f);
                }

                if (forceFieldTrans != null)
                {
                    float newRadius = PluginConfig.piece1.weatherZoneRadius.Value * 2;
                    forceFieldTrans.gameObject.SetActive(PluginConfig.piece1.enableDome.Value ? value : false);
                    forceFieldTrans.localScale = new Vector3(newRadius, newRadius, newRadius);
                }

                if (projectorTrans != null)
                {
                    projectorTrans.gameObject.SetActive(PluginConfig.piece1.enableProjector.Value ? value : false);
                    CircleProjector projector = projectorTrans.gameObject.GetComponent<CircleProjector>();
                    projector.m_radius = PluginConfig.piece1.weatherZoneRadius.Value;
                    projector.m_nrOfSegments = Mathf.FloorToInt(radius * projectorMultiplier);
                }

                if (value)
                    startEffects.Create(startEffectTrans.position, startEffectTrans.rotation);
                else
                    stopEffects.Create(startEffectTrans.position, startEffectTrans.rotation);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not enable/disable Marble Item Stand effects: " + e);
            }
        }

        public void StartLerpColor(Material mat, Color fromColor, Color toColor, float duration)
        {
            StartCoroutine(_LerpColor(mat, fromColor, toColor, duration));
        }

        private IEnumerator _LerpColor(Material mat, Color fromColor, Color toColor, float duration)
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
    }
}
