using Jotunn;
using Jotunn.GUI;
using Jotunn.Managers;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EffectList;
using static UnityEngine.ParticleSystem;

namespace ModularMagic_Utilities.Components
{
    internal class WeatherZone : MonoBehaviour, Hoverable, Interactable
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
        public bool isLocked = false;
        public bool isMoving = false;
        public float radius;
        public bool domeEnabled;
        public bool domeVisualEnabled;
        public bool domeParticlesEnabled;
        public float domeParticlesAmount;
        public bool projectorEnabled;
        public string lightColorPreset;
        public float projectorSegmentMultiplier = 4f;
        public Color emissionOffColor = new Color(0f, 0f, 0f, 1f);
        public Color emissionRuneOffColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public Vector3 hiddenPos = new Vector3(0f, 0f, -0.45f);
        public Vector3 shownPos = new Vector3(0f, 0f, 0f);
        public EffectList startEffects = new EffectList();
        public EffectList stopEffects = new EffectList();
        public GameObject weatherZonePanel;
        public UpdateWeatherZoneOptions currentFormValues;
        public Piece piece; 

        private void Awake()
        {
            try
            {
                netView = transform.parent.gameObject.GetComponent<ZNetView>();
                if (netView != null && netView.GetZDO() != null)
                {
                    creator = Game.instance.GetPlayerProfile().GetPlayerID();

                    emissionTrans = transform.parent.Find("New/emission");
                    emissionRunesTrans = transform.parent.Find("New/runes emission");
                    domeTrans = transform.parent.Find("New/dome");
                    projectorTrans = transform.parent.Find("New/projector");
                    controlsTrans = transform.parent.Find("controls");
                    startEffectTrans = transform.parent.Find("drop_spawn");

                    envZone = gameObject.GetComponent<EnvZone>();
                    envZone.m_force = false;
                    sphereCollider = gameObject.GetComponent<SphereCollider>();
                    itemStand = transform.parent.Find("itemstand").gameObject.GetComponent<ItemStand>();
                    circleProjector = projectorTrans.gameObject.GetComponent<CircleProjector>();

                    radius = netView.GetZDO().GetFloat(ModularMagic_Utilities.weatherZoneRadiusHashCode, PluginConfig.piece1.weatherZoneRadius.Value);
                    domeEnabled = true;
                    projectorEnabled = false;
                    domeVisualEnabled = netView.GetZDO().GetBool(ModularMagic_Utilities.weatherZoneEnableDomeVisualHashCode, PluginConfig.piece1.enableDomeVisual.Value);
                    domeParticlesEnabled = netView.GetZDO().GetBool(ModularMagic_Utilities.weatherZoneEnableDomeParticlesHashCode, PluginConfig.piece1.enableDomeParticles.Value);
                    domeParticlesAmount = netView.GetZDO().GetFloat(ModularMagic_Utilities.weatherZoneDomeParticlesAmountHashCode, PluginConfig.piece1.domeParticlesAmount.Value);
                    lightColorPreset = netView.GetZDO().GetString(ModularMagic_Utilities.weatherZoneLightPresetHashCode, PluginConfig.piece1.lightColorPreset.Value);
                    SetRadius(radius);
                    ApplyDomeEffects();

                    Jotunn.Logger.LogWarning("Awake, domeParticlesEnabled: "+ domeParticlesEnabled);
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
            string dome = "\nShow Dome: <color=yellow>" + (domeEnabled ? "On" : "Off") + "</color>";
            string sphereVisible = "\nShow sphere: <color=yellow>" + (domeVisualEnabled ? "On" : "Off") + "</color>";
            string particlesVisible = "\nShow particles: <color=yellow>" + (domeParticlesEnabled ? "On" : "Off") + "</color>";
            string particleAmount = "\nParticle amount: <color=yellow>" + domeParticlesAmount + "</color>";
            string lockControls = "\n\n[<color=yellow>E</color>] "+ (isLocked ? "Unlock" : "Lock") + " controls";

            return name + radiusString + projector + dome + sphereVisible + particlesVisible + particleAmount + lockControls;
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

            if (Game.instance.GetPlayerProfile().GetPlayerID() != creator)
                return false;

            isLocked = !isLocked;

            if (isLocked)
                MoveControls(shownPos, hiddenPos, 1f);
            else
                MoveControls(hiddenPos, shownPos, 1f);

            return true;
        }

        public void InitWeather(string itemName)
        {
            if (envZone == null || sphereCollider == null)
                return;

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
            sphereCollider.enabled = true;
            ShowEffects(true);
        }

        public void EnableWeather(ItemDrop.ItemData item)
        {
            try
            {
                if (envZone == null || sphereCollider == null)
                    return;

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
                sphereCollider.enabled = true;
                ShowEffects(true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        public void DisableWeather()
        {
            if (envZone == null || envZone.m_environment == "" || sphereCollider == null)
                return;

            Jotunn.Logger.LogWarning("DisableWeather");

            envZone.m_environment = "";
            sphereCollider.enabled = false;
            ShowEffects(false);
        }

        public void OnPieceDestroyed()
        {
            Jotunn.Logger.LogWarning("OnPieceDestroyed(), " + (itemStand != null ? itemStand.m_currentItemName : "None"));
            if (itemStand == null)
                return;

            itemStand.DropItem();
            SetEnableDome(false);
        }

        public void SetRadius(float value)
        {
            if (netView == null || netView.GetZDO() == null)
                return;

            radius = value;
            netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneRadiusHashCode, radius);

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

        public void SetEnableDome(bool value)
        {
            if (netView == null || netView.GetZDO() == null)
                return;

            domeEnabled = value;
            netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneEnableDomeHashCode, domeEnabled);

            if (domeTrans != null && envZone.m_environment != "")
                domeTrans.gameObject.SetActive(domeEnabled);
        }

        public void SetEnableProjector(bool value)
        {
            projectorEnabled = value;
            projectorTrans.gameObject.SetActive(projectorEnabled);
        }

        public void SetEnableDomeVisual(bool value)
        {
            domeVisualEnabled = value;
            ApplyDomeEffects();
        }

        public void SetEnableDomeParticles(bool value)
        {
            domeParticlesEnabled = value;
            ApplyDomeEffects();
        }

        public void SetDomeParticlesAmount(float value)
        {
            domeParticlesAmount = value;
            ApplyDomeEffects();
        }

        public void ApplyDomeEffects()
        {
            if (domeTrans == null) domeTrans = transform.parent.Find("New/dome");
            if (domeTrans == null)
            {
                Jotunn.Logger.LogWarning("Dome trans is null");
                return;
            }

            Jotunn.Logger.LogWarning("ApplyDomeEffects, domeParticlesEnabled: " + domeParticlesEnabled);

            MeshRenderer meshRenderer = domeTrans.gameObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
                meshRenderer.enabled = domeVisualEnabled;

            ParticleSystem particleSystem = domeTrans.gameObject.GetComponent<ParticleSystem>();

            if (particleSystem != null)
            {
                EmissionModule emission = particleSystem.emission;
                emission.rateOverTime = domeParticlesAmount;

                if (domeParticlesEnabled)
                    particleSystem.Play();
                else
                    particleSystem.Stop();
            }
        }

        private void ShowEffects(bool value)
        {
            try
            {
                if (!isLocked)
                    MoveControls(value ? hiddenPos : shownPos, value ? shownPos : hiddenPos, 1f);

                LightPresetColors presetColors = LightPresetHelper.GetColors(lightColorPreset);
                presetColors.emissionColor = LightPresetHelper.ApplyMultiplierToColor(presetColors.emissionColor, 0.8f);

                if (emissionTrans != null)
                {
                    MeshRenderer meshRendererComp = emissionTrans.gameObject.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];
                    StartLerpColor(mat, value ? emissionOffColor : presetColors.emissionColor, value ? presetColors.emissionColor : emissionOffColor, 3f);

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

                    Transform lightsTrans = emissionTrans.Find("light");
                    lightsTrans.gameObject.GetComponent<Light>().color = presetColors.lightColor;
                    ParticleSystem flare = lightsTrans.gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule flareMain = flare.main;
                    flareMain.startColor = presetColors.flareColor;
                    
                    if (value)
                        lightsTrans.gameObject.SetActive(false);

                    lightsTrans.gameObject.SetActive(value);
                }

                if (emissionRunesTrans != null)
                {
                    MeshRenderer meshRendererComp = emissionRunesTrans.gameObject.GetComponent<MeshRenderer>();
                    Material mat = meshRendererComp.materials[0];
                    StartLerpColor(mat, value ? emissionRuneOffColor : presetColors.emissionColor, value ? presetColors.emissionColor : emissionRuneOffColor, 1.5f);
                }

                if (domeTrans != null)
                {
                    ParticleSystem particles = domeTrans.gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule particlesMain = particles.main;
                    ParticleSystemRenderer particlesRenderer = domeTrans.gameObject.GetComponent<ParticleSystemRenderer>();
                    particlesMain.startColor = presetColors.emissionColor;
                    particlesRenderer.materials[0].SetColor("_EmissionColor", presetColors.emissionColor);

                    domeTrans.gameObject.SetActive(domeEnabled ? value : false);
                }

                if (projectorTrans != null)
                {
                    projectorTrans.gameObject.SetActive(projectorEnabled ? value : false);
                }

                ApplyDomeEffects();
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

        public string GetLightPresetByInt(int value)
        {
            switch (value)
            {
                case 0:
                    return LightPresetType.Red;
                case 1:
                    return LightPresetType.Orange;
                case 2:
                    return LightPresetType.Yellow;
                case 3:
                    return LightPresetType.LemonGreen;
                case 4:
                    return LightPresetType.Green;
                case 5:
                    return LightPresetType.LightBlue;
                case 6:
                    return LightPresetType.Blue;
                case 7:
                    return LightPresetType.Pink;
                case 8:
                    return LightPresetType.Purple;
                case 9:
                    return LightPresetType.White;
                default:
                    return LightPresetType.Blue;
            }
        }

        public int GetIntByLightPreset(string value)
        {
            switch (value)
            {
                case nameof(LightPresetType.Red):
                    return 0;
                case nameof(LightPresetType.Orange):
                    return 1;
                case nameof(LightPresetType.Yellow):
                    return 2;
                case nameof(LightPresetType.LemonGreen):
                    return 3;
                case nameof(LightPresetType.Green):
                    return 4;
                case nameof(LightPresetType.LightBlue):
                    return 5;
                case nameof(LightPresetType.Blue):
                    return 6;
                case nameof(LightPresetType.Pink):
                    return 7;
                case nameof(LightPresetType.Purple):
                    return 8;
                case nameof(LightPresetType.White):
                    return 9;
                default:
                    return 6;
            }
        }

        public void RadiusChanged(string value)
        {
            Jotunn.Logger.LogWarning("RadiusChanged(), " + value);
            currentFormValues.radius = float.Parse(value);
        }

        public void ShowSphereChanged(bool value)
        {
            Jotunn.Logger.LogWarning("ShowSphereChanged(), " + value);
            currentFormValues.enableDomeVisual = value;
        }

        public void ShowParticlesChanged(bool value)
        {
            Jotunn.Logger.LogWarning("ShowParticlesChanged(), " + value);
            currentFormValues.enableDomeParticles = value;
        }

        public void AmountChanged(string value)
        {
            Jotunn.Logger.LogWarning("AmountChanged(), " + value);
            currentFormValues.domeParticlesAmount = int.Parse(value);
        }

        public void lightPresetChanged(int value)
        {
            Jotunn.Logger.LogWarning("lightPresetChanged(), " + value);
            switch (value)
            {
                case 0:
                    currentFormValues.lightPreset = LightPresetType.Red;
                    break;
                case 1:
                    currentFormValues.lightPreset = LightPresetType.Orange;
                    break;
                case 2:
                    currentFormValues.lightPreset = LightPresetType.Yellow;
                    break;
                case 3:
                    currentFormValues.lightPreset = LightPresetType.LemonGreen;
                    break;
                case 4:
                    currentFormValues.lightPreset = LightPresetType.Green;
                    break;
                case 5:
                    currentFormValues.lightPreset = LightPresetType.LightBlue;
                    break;
                case 6:
                    currentFormValues.lightPreset = LightPresetType.Blue;
                    break;
                case 7:
                    currentFormValues.lightPreset = LightPresetType.Pink;
                    break;
                case 8:
                    currentFormValues.lightPreset = LightPresetType.Purple;
                    break;
                case 9:
                    currentFormValues.lightPreset = LightPresetType.White;
                    break;
                default:
                    currentFormValues.lightPreset = LightPresetType.Blue;
                    break;
            }
        }

        public void ShowWeatherZoneGUI()
        {
            // Create the panel if it does not exist
            if (!weatherZonePanel)
            {
                if (GUIManager.Instance == null)
                {
                    Jotunn.Logger.LogError("GUIManager instance is null");
                    return;
                }

                if (!GUIManager.CustomGUIFront)
                {
                    Jotunn.Logger.LogError("GUIManager CustomGUI is null");
                    return;
                }

                // Create the panel object
                weatherZonePanel = GUIManager.Instance.CreateWoodpanel(
                    parent: GUIManager.CustomGUIFront.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(0, 0),
                    width: 500,
                    height: 500,
                    draggable: false
                );
                weatherZonePanel.SetActive(false);

                GUIManager.Instance.CreateText(
                    text: "Radius",
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(.5f, 1f),
                    anchorMax: new Vector2(.5f, 1f),
                    position: new Vector2(0f, -40f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 460f,
                    height: 30f,
                    addContentSizeFitter: false
                );

                GameObject radiusField = GUIManager.Instance.CreateInputField(
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -70f),
                    contentType: InputField.ContentType.IntegerNumber,
                    placeholderText: "Radius",
                    fontSize: 18,
                    width: 460f,
                    height: 30f
                );
                InputField radiusInput = radiusField.GetComponent<InputField>();
                radiusInput.text = radius.ToString();
                radiusInput.onValueChanged.AddListener(RadiusChanged);

                GUIManager.Instance.CreateText(
                    text: "Show dome sphere",
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -110f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 460f,
                    height: 30f,
                    addContentSizeFitter: false
                );

                GameObject checkboxShowSphere = GUIHelper.CreateToggle(
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(-200f, -150f),
                    width: 30f,
                    height: 30f
                );
                Toggle toggleShowSphere = checkboxShowSphere.GetComponent<Toggle>();
                toggleShowSphere.isOn = domeVisualEnabled;
                toggleShowSphere.onValueChanged.AddListener(ShowSphereChanged);

                GUIManager.Instance.CreateText(
                    text: "Show dome particles",
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -190f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 460f,
                    height: 30f,
                    addContentSizeFitter: false
                );

                GameObject checkboxShowParticles = GUIHelper.CreateToggle(
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(-200f, -230f),
                    width: 30f,
                    height: 30f
                );
                Toggle toggleShowParticles = checkboxShowParticles.GetComponent<Toggle>();
                toggleShowParticles.isOn = domeParticlesEnabled;
                toggleShowParticles.onValueChanged.AddListener(ShowParticlesChanged);

                GUIManager.Instance.CreateText(
                    text: "Dome particles amount",
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -265f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 460f,
                    height: 30f,
                    addContentSizeFitter: false
                );

                GameObject amountField = GUIManager.Instance.CreateInputField(
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -300f),
                    contentType: InputField.ContentType.IntegerNumber,
                    placeholderText: "Amount",
                    fontSize: 18,
                    width: 460f,
                    height: 30f
                );
                InputField amountInput = amountField.GetComponent<InputField>();
                amountInput.text = domeParticlesAmount.ToString();
                amountInput.onValueChanged.AddListener(AmountChanged);

                GUIManager.Instance.CreateText(
                    text: "Light color preset",
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -340f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 18,
                    color: GUIManager.Instance.ValheimOrange,
                    outline: true,
                    outlineColor: Color.black,
                    width: 460f,
                    height: 30f,
                    addContentSizeFitter: false
                );

                GameObject lightPresetField = GUIManager.Instance.CreateDropDown(
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0f, -380f),
                    fontSize: 18,
                    width: 460f,
                    height: 30f
                );
                Dropdown lightPresetDropdown = lightPresetField.GetComponent<Dropdown>();
                lightPresetDropdown.GetComponent<Dropdown>().AddOptions(new List<string>
                {
                    LightPresetType.Red, LightPresetType.Orange, LightPresetType.Yellow, LightPresetType.LemonGreen, LightPresetType.Green,
                    LightPresetType.LightBlue, LightPresetType.Blue, LightPresetType.Pink, LightPresetType.Purple, LightPresetType.White,
                });
                lightPresetDropdown.value = GetIntByLightPreset(lightColorPreset);
                lightPresetDropdown.onValueChanged.AddListener(lightPresetChanged);

                GameObject buttonObject = GUIManager.Instance.CreateButton(
                    text: "Cancel",
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 0f),
                    anchorMax: new Vector2(0.5f, 0f),
                    position: new Vector2(-120f, 50f),
                    width: 225f,
                    height: 60f
                );
                buttonObject.SetActive(true);
                Button button = buttonObject.GetComponent<Button>();
                button.onClick.AddListener(ShowWeatherZoneGUI);

                GameObject buttonObject2 = GUIManager.Instance.CreateButton(
                    text: "Accept",
                    parent: weatherZonePanel.transform,
                    anchorMin: new Vector2(0.5f, 0f),
                    anchorMax: new Vector2(0.5f, 0f),
                    position: new Vector2(120f, 50f),
                    width: 225f,
                    height: 60f
                );
                buttonObject2.SetActive(true);
                Button button2 = buttonObject2.GetComponent<Button>();
                button2.onClick.AddListener(AcceptPanelValues);
            }

            currentFormValues = new UpdateWeatherZoneOptions();
            currentFormValues.radius = radius;
            currentFormValues.enableDomeVisual = domeVisualEnabled;
            currentFormValues.enableDomeParticles = domeParticlesEnabled;
            currentFormValues.domeParticlesAmount = domeParticlesAmount;
            currentFormValues.lightPreset = lightColorPreset;

            // Switch the current state
            bool state = !weatherZonePanel.activeSelf;

            // Set the active state of the panel
            weatherZonePanel.SetActive(state);

            // Toggle input for the player and camera while displaying the GUI
            GUIManager.BlockInput(state);
        }

        public void AcceptPanelValues()
        {
            radius = (float)currentFormValues.radius;
            domeVisualEnabled = (bool)currentFormValues.enableDomeVisual;
            domeParticlesEnabled = (bool)currentFormValues.enableDomeParticles;
            domeParticlesAmount = (float)currentFormValues.domeParticlesAmount;
            lightColorPreset = (string)currentFormValues.lightPreset;

            if (radius < 10f)
                radius = 10f;
            else if (radius > 100f)
                radius = 100f;

            netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneEnableDomeVisualHashCode, domeVisualEnabled);
            netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneEnableDomeParticlesHashCode, domeParticlesEnabled);
            netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneDomeParticlesAmountHashCode, domeParticlesAmount);
            netView.GetZDO().Set(ModularMagic_Utilities.weatherZoneLightPresetHashCode, lightColorPreset);

            SetRadius(radius);
            ShowEffects(envZone.m_environment != "" ? true : false);

            Jotunn.Logger.LogWarning("AcceptPanelValues, domeParticlesEnabled: " + domeParticlesEnabled);

            currentFormValues = null;

            // Set the active state of the panel
            weatherZonePanel.SetActive(false);

            // Toggle input for the player and camera while displaying the GUI
            GUIManager.BlockInput(false);
        }
    }
}
