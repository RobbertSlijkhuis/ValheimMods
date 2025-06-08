using Jotunn;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EffectList;
using static UnityEngine.ParticleSystem;

namespace ModularMagic_Utilities.Components
{
    internal class WeatherZone : MonoBehaviour, Hoverable
    {
        public ZNetView netView;
        public EnvZone envZone;
        public Transform startEffectTrans;
        public float radius;
        public bool isInitialised = false;
        public bool domeEnabled;
        public bool projectorEnabled;
        public float projectorSegmentMultiplier = 4f;
        //public Color emissionColor = new Color(0f, 0.5676858f, 1.294612f, 1f);
        public Color emissionOffColor = new Color(0f, 0f, 0f, 1f);
        public Color emissionRuneOffColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public EffectList startEffects = new EffectList();
        public EffectList stopEffects = new EffectList();

        private void Awake()
        {
            try
            {
                netView = transform.parent.GetComponent<ZNetView>();
                if (netView != null && netView.GetZDO() != null)
                {
                    startEffectTrans = transform.parent.Find("drop_spawn");
                    envZone = GetComponent<EnvZone>();
                    envZone.m_force = false;

                    radius = netView.m_zdo.GetFloat(ModularMagic_Utilities.weatherZoneRadiusHashCode, 30f);
                    domeEnabled = netView.m_zdo.GetBool(ModularMagic_Utilities.weatherZoneEnableDomeHashCode, false);
                    projectorEnabled = false;
                    SetRadius(radius);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not finish WeatherZone Awake: "+ e);
            }
        }

        public string GetHoverText()
        {
            string name = PluginConfig.piece1.name.Value;
            string radiusString = "\nRadius: <color=yellow>" + radius + "</color>";
            string projector = "\nRadius projector: <color=yellow>" + (projectorEnabled ? "On" : "Off") + "</color>";
            string dome = "\nDome: <color=yellow>" + (domeEnabled ? "On" : "Off") + "</color>";

            return name + radiusString + projector + dome;
        }

        public string GetHoverName()
        {
            return PluginConfig.piece1.name.Value;
        }

        public void InitWeather(string itemName)
        {
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
            SetEffects(true);
        }

        public void EnableWeather(ItemDrop.ItemData item)
        {
            try
            {
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
                SetEffects(true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        public void DisableWeather()
        {
            if (netView != null && netView.GetZDO() != null)
                return;

            if (envZone.m_environment == "")
                return;

            envZone.m_environment = "";
            SetEffects(false);
        }

        public void SetRadius(float value)
        {
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            Transform forceFieldTrans = transform.parent.Find("New/forcefield");
            Transform projectorTrans = transform.parent.Find("New/projector");
            radius = value;
            netView.m_zdo.Set(ModularMagic_Utilities.weatherZoneRadiusHashCode, radius);

            if (sphereCollider != null)
            {
                Jotunn.Logger.LogWarning("Set sphere collider to: " + radius);
                sphereCollider.radius = radius;
            }

            if (forceFieldTrans != null)
                forceFieldTrans.localScale = new Vector3(radius * 2, radius * 2, radius * 2);

            if (projectorTrans != null)
            {
                CircleProjector projector = projectorTrans.gameObject.GetComponent<CircleProjector>();
                projector.m_radius = radius;
                projector.m_nrOfSegments = Mathf.FloorToInt(radius * projectorSegmentMultiplier);
            }
        }

        public void SetEnableDome(bool value)
        {
            Transform forceFieldTrans = transform.parent.Find("New/forcefield");
            domeEnabled = value;
            netView.m_zdo.Set(ModularMagic_Utilities.weatherZoneEnableDomeHashCode, domeEnabled);

            if (forceFieldTrans != null && envZone.m_environment != "")
                forceFieldTrans.gameObject.SetActive(domeEnabled);
        }

        public void SetEnableProjector(bool value)
        {
            Transform projectorTrans = transform.parent.Find("New/projector");
            projectorEnabled = value;
            projectorTrans.gameObject.SetActive(projectorEnabled);

            //if (projectorTrans != null && envZone.m_environment != "")
            //    projectorTrans.gameObject.SetActive(projectorEnabled);            
        }

        private void SetEffects(bool value)
        {
            try
            {
                LightPresetColors presetColors = LightPresetHelper.GetColors(PluginConfig.piece1.lightColorPreset.Value);
                presetColors.emissionColor = LightPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, 0.8f);

                Transform emissionTrans = transform.parent.Find("New/emission");
                Transform runesTrans = transform.parent.Find("New/runes emission");
                Transform forceFieldTrans = transform.parent.Find("New/forcefield");
                Transform projectorTrans = transform.parent.Find("New/projector");

                if (emissionTrans != null)
                {
                    MeshRenderer meshRendererComp = emissionTrans.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];
                    StartLerpColor(mat, value ? emissionOffColor : presetColors.emissionColor, value ? presetColors.emissionColor : emissionOffColor, 3f);

                    Transform particleTrans = emissionTrans.gameObject.transform.Find("particles");
                    ParticleSystem particles = particleTrans.gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule particlesMain = particles.main;
                    ParticleSystemRenderer particlesRenderer = particleTrans.gameObject.GetComponent<ParticleSystemRenderer>();
                    particlesMain.startColor = presetColors.emissionColor;
                    particlesRenderer.sharedMaterial.SetColor("_EmissionColor", presetColors.emissionColor);

                    Transform particleFastTrans = emissionTrans.gameObject.transform.Find("particles_fast");
                    ParticleSystem particlesFast = particleFastTrans.gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule particlesFastMain = particles.main;
                    ParticleSystemRenderer particlesFastRenderer = particleFastTrans.gameObject.GetComponent<ParticleSystemRenderer>();
                    particlesFastMain.startColor = presetColors.emissionColor;
                    particlesFastRenderer.sharedMaterial.SetColor("_EmissionColor", presetColors.emissionColor);
                    
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
                    lightsTrans.gameObject.GetComponent<Light>().color = presetColors.lightColor;
                    ParticleSystem.MainModule flareMain = lightsTrans.gameObject.GetComponent<ParticleSystem>().main;
                    flareMain.startColor = presetColors.flareColor;

                }

                if (runesTrans != null)
                {
                    MeshRenderer meshRendererComp = runesTrans.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];
                    StartLerpColor(mat, value ? emissionRuneOffColor : presetColors.emissionColor, value ? presetColors.emissionColor : emissionRuneOffColor, 1.5f);
                }

                if (forceFieldTrans != null)
                {
                    ParticleSystem particles = forceFieldTrans.gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule particlesMain = particles.main;
                    ParticleSystemRenderer particlesRenderer = forceFieldTrans.gameObject.GetComponent<ParticleSystemRenderer>();
                    particlesMain.startColor = presetColors.emissionColor;
                    particlesRenderer.sharedMaterial.SetColor("_EmissionColor", presetColors.emissionColor);

                    forceFieldTrans.gameObject.SetActive(domeEnabled ? value : false);
                }

                if (projectorTrans != null)
                {
                    projectorTrans.gameObject.SetActive(projectorEnabled ? value : false);
                }

                SetColorsOnActivationFX(presetColors);

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

        private void SetColorsOnActivationFX(LightPresetColors presetColors)
        {
            try
            {
                EffectData startVFX = startEffects.m_effectPrefabs[1];

                if (startVFX == null)
                    throw new Exception("Effect is null");

                GameObject activationFX = startVFX.m_prefab;

                if (activationFX == null)
                    throw new Exception("prefab is null");

                Transform particleExplTrans = activationFX.transform.Find("Particle System _expl");
                Transform trailsExplTrans = activationFX.transform.Find("trails _expl");
                Transform gloriaTrans = activationFX.transform.Find("gloria");
                Transform lightTrans = activationFX.transform.Find("Point light");
                float multiplier = 0.3f;

                if (presetColors.lightPreset == LightPresetType.Green || presetColors.lightPreset == LightPresetType.Blue)
                    multiplier = 0.1f;

                if (presetColors.lightPreset == LightPresetType.Pink)
                    multiplier = 0.6f;

                Jotunn.Logger.LogWarning("Emission ActivationFX: " + LightPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, multiplier));

                ParticleSystem.MainModule particleMain = particleExplTrans.gameObject.GetComponent<ParticleSystem>().main;
                particleMain.startColor = LightPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, multiplier);

                ParticleSystem.MainModule trailsMain = trailsExplTrans.gameObject.GetComponent<ParticleSystem>().main;
                trailsMain.startColor = LightPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, multiplier);

                ParticleSystem.MainModule gloriaMain = gloriaTrans.gameObject.GetComponent<ParticleSystem>().main;
                Color gloriaColor = LightPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, multiplier);
                gloriaColor.a = 0.5607843f;
                gloriaMain.startColor = gloriaColor;

                lightTrans.gameObject.GetComponent<Light>().color = presetColors.lightColor;

                startEffects.m_effectPrefabs[1].m_prefab = activationFX;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set colors on ActivationFX: " + e);
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
