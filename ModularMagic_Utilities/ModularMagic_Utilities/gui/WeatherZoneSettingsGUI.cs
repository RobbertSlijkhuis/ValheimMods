using Jotunn.Managers;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.Types;
using PlayFab.EconomyModels;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ModularMagic_Utilities.GUI
{
    internal class WeatherZoneSettingsGUI
    {
        public GameObject weatherZonePanel;
        public UpdateWeatherZoneOptions currentValues;
        public UpdateWeatherZoneOptions currentFormValues;

        public UnityEvent<UpdateWeatherZoneOptions> onAccept = new UnityEvent<UpdateWeatherZoneOptions>();
        public UnityEvent onCancel = new UnityEvent();

        public WeatherZoneSettingsGUI(UpdateWeatherZoneOptions currentValues)
        {
            this.currentValues = currentValues;
        }

        public void OnRadiusChanged(string value)
        {
            if (value == null || value == "") return;

            float floatValue = Mathf.Clamp(float.Parse(value), 10f, 100f);
            currentFormValues.radius = floatValue;
        }

        public void OnDomeChanged(bool value)
        {
            currentFormValues.domeEnabled = value;
        }

        public void OnParticlesChanged(string value)
        {
            if (value == null || value == "") return;

            int intValue = Mathf.Clamp(int.Parse(value), 0, 1000);
            currentFormValues.particleAmount = intValue;
        }

        public void OnlightPresetChanged(int value)
        {
            Jotunn.Logger.LogWarning("lightPresetChanged(), " + LightColorPresetHelper.GetLightPresetByInt(value));
            switch (value)
            {
                case 0:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.None;
                    break;
                case 1:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Red;
                    break;
                case 2:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Orange;
                    break;
                case 3:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Yellow;
                    break;
                case 4:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.LemonGreen;
                    break;
                case 5:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Green;
                    break;
                case 6:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.LightBlue;
                    break;
                case 7:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Blue;
                    break;
                case 8:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Pink;
                    break;
                case 9:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Purple;
                    break;
                case 10:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.White;
                    break;
                default:
                    currentFormValues.lightColorPresetOverride = LightColorPresetType.Blue;
                    break;
            }
        }

        public void ShowGUI()
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

            weatherZonePanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 500,
                height: 400,
                draggable: false
            );
            weatherZonePanel.SetActive(false);

            GUIManager.Instance.CreateText(
                text: "Radius (10 - 100)",
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
            radiusInput.text = currentValues.radius.ToString();
            radiusInput.onValueChanged.AddListener(OnRadiusChanged);
            radiusInput.characterLimit = 3;

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
            toggleShowSphere.isOn = currentValues.domeEnabled;
            toggleShowSphere.onValueChanged.AddListener(OnDomeChanged);

            GUIManager.Instance.CreateText(
                text: "Dome particles (0 - 1000)",
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

            GameObject amountField = GUIManager.Instance.CreateInputField(
                parent: weatherZonePanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -220f),
                contentType: InputField.ContentType.IntegerNumber,
                placeholderText: "Amount",
                fontSize: 18,
                width: 460f,
                height: 30f
            );
            InputField amountInput = amountField.GetComponent<InputField>();
            amountInput.text = currentValues.particleAmount.ToString();
            amountInput.onValueChanged.AddListener(OnParticlesChanged);
            amountInput.characterLimit = 4;

            GUIManager.Instance.CreateText(
                text: "Light color preset",
                parent: weatherZonePanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -260f),
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
                position: new Vector2(0f, -290f),
                fontSize: 18,
                width: 460f,
                height: 30f
            );
            Dropdown lightPresetDropdown = lightPresetField.GetComponent<Dropdown>();
            lightPresetDropdown.GetComponent<Dropdown>().AddOptions(new List<string>
            {
                LightColorPresetType.None, LightColorPresetType.Red, LightColorPresetType.Orange, LightColorPresetType.Yellow, LightColorPresetType.LemonGreen, LightColorPresetType.Green,
                LightColorPresetType.LightBlue, LightColorPresetType.Blue, LightColorPresetType.Pink, LightColorPresetType.Purple, LightColorPresetType.White,
            });
            lightPresetDropdown.value = LightColorPresetHelper.GetIntByLightPreset(currentValues.lightColorPresetOverride);
            lightPresetDropdown.onValueChanged.AddListener(OnlightPresetChanged);

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
            button.onClick.AddListener(CancelPanel);

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

            currentFormValues = new UpdateWeatherZoneOptions();
            currentFormValues.radius = currentValues.radius;
            currentFormValues.domeEnabled = currentValues.domeEnabled;
            currentFormValues.particleAmount = currentValues.particleAmount;
            currentFormValues.lightColorPresetOverride = currentValues.lightColorPresetOverride;

            weatherZonePanel.SetActive(true);
            GUIManager.BlockInput(true);
        }

        public void AcceptPanelValues()
        {
            onAccept.Invoke(currentFormValues);
            currentValues = currentFormValues;
            currentFormValues = null;
            weatherZonePanel.SetActive(false);
            GUIManager.BlockInput(false);
        }

        public void CancelPanel()
        {
            onCancel.Invoke();
            currentFormValues = null;
            weatherZonePanel.SetActive(false);
            GUIManager.BlockInput(false);
        }
    }
}
