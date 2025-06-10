using Jotunn;
using Jotunn.GUI;
using Jotunn.Managers;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.GUI;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EffectList;
using static ItemDrop;
using static UnityEngine.ParticleSystem;

namespace ModularMagic_Utilities.Components
{
    internal class WeatherZoneMMU : MonoBehaviour, Hoverable, Interactable
    {
        public ZNetView netView;
        public EnvZone envZone;
        public SphereCollider sphereCollider;
        public CircleProjector circleProjector;
        public ItemStand itemStand;
        public Transform emissionTrans;
        public Transform emissionRunesTrans;
        public Transform domeTrans;
        public Transform projectorTrans;
        public Transform controlsTrans;
        public Transform startEffectTrans;

        public long creator;
        public bool isInitialised = false;
        public bool isMoving = false;
        public bool isEnabled;
        public bool isLocked;
        public bool projectorEnabled;
        public bool domeEnabled;
        public float radius;
        public float particleAmount;
        public float projectorSegmentMultiplier = 4f;
        public string lightColorPreset;
        public string lightColorPresetOverride;

        public Color emissionOffColor = new Color(0f, 0f, 0f, 1f);
        public Color emissionRuneOffColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public Vector3 hiddenPos = new Vector3(0f, 0f, -0.45f);
        public Vector3 shownPos = new Vector3(0f, 0f, 0f);
        public EffectList startEffects = new EffectList();
        public EffectList stopEffects = new EffectList();

        public WeatherZoneSettingsGUI weatherZoneSettingsGUI;

        private void Awake()
        {
            try
            {
                netView = transform.parent.gameObject.GetComponent<ZNetView>();
                if (netView != null && netView.GetZDO() != null)
                {
                    creator = Game.instance.GetPlayerProfile().GetPlayerID();

                    // Transforms
                    emissionTrans = transform.parent.Find("New/emission");
                    emissionRunesTrans = transform.parent.Find("New/runes emission");
                    domeTrans = transform.parent.Find("New/dome");
                    projectorTrans = transform.parent.Find("New/projector");
                    controlsTrans = transform.parent.Find("controls");
                    startEffectTrans = transform.parent.Find("drop_spawn");

                    // Components
                    envZone = gameObject.GetComponent<EnvZone>();
                    envZone.m_force = false;
                    sphereCollider = gameObject.GetComponent<SphereCollider>();
                    itemStand = transform.parent.Find("itemstand").gameObject.GetComponent<ItemStand>();
                    circleProjector = projectorTrans.gameObject.GetComponent<CircleProjector>();

                    // States
                    isLocked = netView.GetZDO().GetBool(ModularMagic_Utilities.weatherZoneLockedHash, false);
                    projectorEnabled = false;
                    domeEnabled = netView.GetZDO().GetBool(ModularMagic_Utilities.weatherZoneDomeHash, false);
                    radius = netView.GetZDO().GetFloat(ModularMagic_Utilities.weatherZoneRadiusHash, 30f);
                    particleAmount = netView.GetZDO().GetFloat(ModularMagic_Utilities.weatherZoneParticlesHash, 100);
                    lightColorPreset = LightColorPresetType.None;
                    lightColorPresetOverride = netView.GetZDO().GetString(ModularMagic_Utilities.weatherZoneLightColorPresetHash, LightColorPresetType.None);

                    // RPC's
                    netView.Register("ToggleIsLocked", RPC_ToggleIsLocked);
                    netView.Register<string>("EnableWeather", RPC_EnableWeather);
                    netView.Register("DisableWeather", RPC_DisableWeather);
                    netView.Register<ZPackage>("OnAcceptSettings", RPC_OnAcceptSettings);

                    // Update radius
                    SetRadius(radius, false);

                    // GUI
                    weatherZoneSettingsGUI = new WeatherZoneSettingsGUI(new UpdateWeatherZoneOptions()
                    {
                        radius = radius,
                        domeEnabled = domeEnabled,
                        particleAmount = particleAmount,
                        lightColorPresetOverride = lightColorPresetOverride,
                    });
                    weatherZoneSettingsGUI.onAccept.AddListener(InvokeOnAcceptSettings);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not finish WeatherZone Awake: "+ e);
            }
        }

        public string GetHoverText()
        {
            if (envZone.m_environment == "")
                return "";

            string name = PluginConfig.piece1.name.Value;
            string radiusString = "\nRadius: <color=yellow>" + radius + "</color>";
            string dome = "\nShow Dome: <color=yellow>" + (domeEnabled ? "On" : "Off") + "</color>";
            string amount = "\nParticle amount: <color=yellow>" + particleAmount + "</color>";
            string projector = "\nRadius projector: <color=yellow>" + (projectorEnabled ? "On" : "Off") + "</color>";
            string lockControls = "\n\n[<color=yellow>E</color>] "+ (isLocked ? "Unlock" : "Lock") + " controls";

            return name + radiusString + dome + amount + projector + lockControls;
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

            if (envZone.m_environment == "" || Game.instance.GetPlayerProfile().GetPlayerID() != creator)
                return false;

            InvokeToggleIsLocked();
            return true;
        }

        public void ToggleIsLocked()
        {
            isLocked = !isLocked;
            netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneLockedHash, isLocked);

            if (isLocked)
                MoveControls(shownPos, hiddenPos, 1f);
            else
                MoveControls(hiddenPos, shownPos, 1f);
        }

        public void InvokeToggleIsLocked()
        {
            netView.InvokeRPC(ZNetView.Everybody, "ToggleIsLocked");
        }

        public void RPC_ToggleIsLocked(long sender)
        {
            ToggleIsLocked();
        }

        public void InitWeather(string prefabName)
        {
            if (envZone == null || sphereCollider == null)
                return;

            GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);

            if (prefab == null)
            {
                isInitialised = true;
                return;
            }

            WeatherZoneConfig config = PluginConfig.weatherConfigList.Find(item => item.prefabName.Value == prefab.name);
            string newEnv = "";

            if (config != null)
            {
                newEnv = config.weatherName;
                lightColorPreset = config.lightColorPreset.Value;
            }

            envZone.m_environment = newEnv;
            sphereCollider.enabled = true;
            isInitialised = true;
            UpdateEffects(newEnv != "", true, true, true, true, true);
        }

        public void EnableWeather(string prefabName)
        {
            try
            {
                if (envZone == null || sphereCollider == null)
                    return;

                GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);
                WeatherZoneConfig config = PluginConfig.weatherConfigList.Find(item => item.prefabName.Value == prefab.name);
                string newEnv = "";

                if (config != null)
                {
                    newEnv = config.weatherName;
                    lightColorPreset = config.lightColorPreset.Value;
                }

                if (newEnv == "")
                    return;

                envZone.m_environment = newEnv;
                sphereCollider.enabled = true;
                UpdateEffects(true, true, true, true, true, true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        public void InvokeEnableWeather(string prefabName)
        {
            netView.InvokeRPC(ZNetView.Everybody, "EnableWeather", prefabName);
        }

        public void RPC_EnableWeather(long sender, string prefabName)
        {
            EnableWeather(prefabName);
        }

        public void DisableWeather()
        {
            if (envZone == null || envZone.m_environment == "" || sphereCollider == null)
                return;

            envZone.m_environment = "";
            sphereCollider.enabled = false;
            UpdateEffects(false, true, true, false, true, true);
        }

        public void InvokeDisableWeather()
        {
            netView.InvokeRPC(ZNetView.Everybody, "DisableWeather");
        }

        public void RPC_DisableWeather(long sender)
        {
            DisableWeather();
        }

        public void OnAcceptSettings(UpdateWeatherZoneOptions values)
        {            
            SetRadius(values.radius);
            SetDome(values.domeEnabled);
            SetParticles(values.particleAmount);
            SetLightColorePresetOverride(values.lightColorPresetOverride);
            UpdateEffects(envZone.m_environment != "", true, true, true, false, false);
        }

        public void InvokeOnAcceptSettings(UpdateWeatherZoneOptions values)
        {
            ZPackage package = new ZPackage();
            package.Write(values.radius.ToString());
            package.Write(values.domeEnabled);
            package.Write(values.particleAmount.ToString());
            package.Write(values.lightColorPresetOverride);

            netView.InvokeRPC(ZNetView.Everybody, "OnAcceptSettings", package);
        }

        public void RPC_OnAcceptSettings(long sender, ZPackage package)
        {
            UpdateWeatherZoneOptions values = new UpdateWeatherZoneOptions();
            values.radius = float.Parse(package.ReadString());
            values.domeEnabled = package.ReadBool();
            values.particleAmount = float.Parse(package.ReadString());
            values.lightColorPresetOverride = package.ReadString();

            OnAcceptSettings(values);
        }

        public void OnPieceDestroyed()
        {
            if (itemStand == null)
                return;

            itemStand.DropItem();
            SetDome(false, false);
        }

        public void SetProjector(bool value)
        {
            if (projectorTrans == null)
                return;

            projectorEnabled = value;
            projectorTrans.gameObject.SetActive(projectorEnabled);
        }

        public void SetDome(bool value, bool updateZDO = true)
        {
            if (netView == null || netView.GetZDO() == null || domeTrans == null)
                return;

            if (updateZDO)
                netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneDomeHash, value);

            domeEnabled = value;
        }

        public void SetRadius(float value, bool updateZDO = true)
        {
            if (netView == null || netView.GetZDO() == null)
                return;

            if (updateZDO)
                netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneRadiusHash, value);

            radius = value;

            if (sphereCollider != null)
                sphereCollider.radius = radius;

            if (domeTrans != null)
                domeTrans.localScale = new Vector3(radius * 2, radius * 2, radius * 2);

            if (circleProjector != null)
            {
                circleProjector.m_radius = radius;
                circleProjector.m_nrOfSegments = Mathf.FloorToInt(radius * projectorSegmentMultiplier);
            }
        }

        public void SetParticles(float value, bool updateZDO = true)
        {
            ParticleSystem particleSystem = domeTrans.gameObject.GetComponent<ParticleSystem>();

            if (particleSystem == null)
                return;

            if (updateZDO)
                netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneParticlesHash, value);

            particleAmount = value;
        }

        public void SetLightColorePresetOverride(string value, bool updateZDO = true)
        {
            if (updateZDO)
                netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneParticlesHash, value);

            lightColorPresetOverride = value;
        }

        private void UpdateEffects(bool enabled, bool updateEmission, bool updateDome, bool updateActionFX, bool playEffects, bool moveControls)
        {
            try
            {
                LightPresetColors presetColors = LightColorPresetHelper.GetColors(lightColorPresetOverride != LightColorPresetType.None ? lightColorPresetOverride : lightColorPreset);
                presetColors.emissionColor = LightColorPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, 0.8f);

                if (updateEmission)
                    UpdateEmissions(enabled, playEffects, presetColors);

                if (updateDome)
                    UpdateDome(enabled, presetColors);

                if (updateActionFX)
                    UpdateActivationFX(presetColors);

                if (playEffects && enabled)
                    startEffects.Create(startEffectTrans.position, startEffectTrans.rotation);
                else if (playEffects)
                    stopEffects.Create(startEffectTrans.position, startEffectTrans.rotation);

                if (moveControls && !isLocked)
                    MoveControls(enabled ? hiddenPos : shownPos, enabled ? shownPos : hiddenPos, 1f);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not enable/disable Marble Item Stand effects: " + e);
            }
        }

        private void UpdateEmissions(bool enabled, bool playEffects, LightPresetColors presetColors)
        {
            if (emissionTrans != null)
            {
                MeshRenderer meshRendererComp = emissionTrans.gameObject.gameObject.GetComponent<MeshRenderer>();
                Material mat = meshRendererComp.materials[0];
                
                if (playEffects)
                    ChangeColor(mat, enabled ? emissionOffColor : presetColors.emissionColor, enabled ? presetColors.emissionColor : emissionOffColor, 3f);
                else if (enabled)
                    mat.SetColor("_EmissionColor", presetColors.emissionColor);

                Transform particleTrans = emissionTrans.Find("particles");
                ParticleSystem particles = particleTrans.gameObject.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule particlesMain = particles.main;
                ParticleSystemRenderer particlesRenderer = particleTrans.gameObject.GetComponent<ParticleSystemRenderer>();
                particlesMain.startColor = presetColors.emissionColor;
                particlesRenderer.materials[0].SetColor("_EmissionColor", presetColors.emissionColor);

                Transform particleFastTrans = emissionTrans.Find("particles_fast");
                ParticleSystem particlesFast = particleFastTrans.gameObject.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule particlesFastMain = particles.main;
                ParticleSystemRenderer particlesFastRenderer = particleFastTrans.gameObject.GetComponent<ParticleSystemRenderer>();
                particlesFastMain.startColor = presetColors.emissionColor;
                particlesFastRenderer.materials[0].SetColor("_EmissionColor", presetColors.emissionColor);

                if (enabled)
                {
                    particles.Play();
                    particlesFast.Play();
                }
                else
                {
                    particles.Stop();
                    particlesFast.Stop();
                }

                Transform lightsTrans = emissionTrans.Find("light");
                lightsTrans.gameObject.GetComponent<Light>().color = presetColors.lightColor;
                ParticleSystem flare = lightsTrans.gameObject.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule flareMain = flare.main;
                flareMain.startColor = presetColors.flareColor;

                if (enabled)
                    lightsTrans.gameObject.SetActive(false);

                lightsTrans.gameObject.SetActive(enabled);
            }

            if (emissionRunesTrans != null)
            {
                MeshRenderer meshRendererComp = emissionRunesTrans.gameObject.GetComponent<MeshRenderer>();
                Material mat = meshRendererComp.materials[0];

                if (playEffects)
                    ChangeColor(mat, enabled ? emissionRuneOffColor : presetColors.emissionColor, enabled ? presetColors.emissionColor : emissionRuneOffColor, 1.5f);
                else if (enabled)
                    mat.SetColor("_EmissionColor", presetColors.emissionColor);
            }
        }

        private void UpdateDome(bool enabled, LightPresetColors presetColors)
        {
            if (domeTrans != null)
            {
                ParticleSystem particleSystem = domeTrans.gameObject.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule particlesMain = particleSystem.main;
                ParticleSystemRenderer particlesRenderer = domeTrans.gameObject.GetComponent<ParticleSystemRenderer>();
                particlesMain.startColor = presetColors.emissionColor;
                particlesRenderer.materials[0].SetColor("_EmissionColor", presetColors.emissionColor);

                EmissionModule emission = particleSystem.emission;
                emission.rateOverTime = enabled ? particleAmount : 0;

                if (enabled)
                    particleSystem.Play();
                else
                    particleSystem.Stop();

                MeshRenderer meshRenderer = domeTrans.gameObject.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                    meshRenderer.enabled = enabled ? domeEnabled : false;
            }
        }

        private void UpdateActivationFX(LightPresetColors presetColors)
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

                if (presetColors.lightPreset == LightColorPresetType.Green || presetColors.lightPreset == LightColorPresetType.Blue)
                    multiplier = 0.1f;

                if (presetColors.lightPreset == LightColorPresetType.Pink)
                    multiplier = 0.6f;

                ParticleSystem.MainModule particleMain = particleExplTrans.gameObject.GetComponent<ParticleSystem>().main;
                particleMain.startColor = LightColorPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, multiplier);

                ParticleSystem.MainModule trailsMain = trailsExplTrans.gameObject.GetComponent<ParticleSystem>().main;
                trailsMain.startColor = LightColorPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, multiplier);

                ParticleSystem.MainModule gloriaMain = gloriaTrans.gameObject.GetComponent<ParticleSystem>().main;
                Color gloriaColor = LightColorPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, multiplier);
                gloriaColor.a = 0.5607843f;
                gloriaMain.startColor = gloriaColor;

                lightTrans.gameObject.GetComponent<Light>().color = presetColors.lightColor;

                startEffects.m_effectPrefabs[1].m_prefab = activationFX;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update colors on ActivationFX: " + e);
            }
        }

        public void MoveControls(Vector3 fromPos, Vector3 toPos, float duration)
        {
            if (!isMoving)
            {
                StartCoroutine(LerpTransform(controlsTrans, fromPos, toPos,  duration));
                isMoving = true;
                Invoke(nameof(resetIsMoving), 1f);
            }
        }

        private IEnumerator LerpTransform(Transform trans, Vector3 from, Vector3 to, float duration)
        {
            float timestep = 0;

            while (timestep <= duration)
            {
                timestep = timestep + Time.deltaTime;
                float step = Mathf.Clamp01(timestep / duration);
                Vector3 newPos = Vector3.Lerp(from, to, step);
                trans.localPosition = newPos;
                yield return null;
            }
        }

        public void resetIsMoving()
        {
            isMoving = false;
        }

        public void ChangeColor(Material mat, Color fromColor, Color toColor, float duration)
        {
            StartCoroutine(LerpColor(mat, fromColor, toColor, duration));
        }

        private IEnumerator LerpColor(Material mat, Color fromColor, Color toColor, float duration)
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
