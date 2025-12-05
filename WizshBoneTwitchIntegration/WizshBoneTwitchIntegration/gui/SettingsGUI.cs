//using Jotunn.Managers;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine;
//using UnityEngine.Events;
//using ModularMagic_Utilities.Helpers;
//using WizshBoneTwitchIntegration.Models;

//namespace WizshBoneTwitchIntegration.GUI
//{
//    internal class SettingsGUI
//    {
//        private GameObject panel;
//        private UpdateSettingsOptions currentValues;
//        private UpdateSettingsOptions currentFormValues;

//        public UnityEvent<UpdateSettingsOptions> onAccept = new UnityEvent<UpdateSettingsOptions>();
//        public UnityEvent onCancel = new UnityEvent();

//        public SettingsGUI(UpdateSettingsOptions currentValues)
//        {
//            SetCurrentValues(currentValues);
//        }

//        public void SetCurrentValues(UpdateSettingsOptions currentValues)
//        {
//            this.currentValues = currentValues;
//        }

//        private void OnIsSkeletonChanged(bool value)
//        {
//            currentFormValues.isSkeleton = value;
//        }

//        private void OnSkinChanged(int value)
//        {
//            currentFormValues.skin = IntToString(value);
//        }

//        private void OnCanSwim(bool value)
//        {
//            currentFormValues.canSwim = value;
//        }

//        public void ShowGUI()
//        {
//            if (GUIManager.Instance == null)
//            {
//                Jotunn.Logger.LogError("GUIManager instance is null");
//                return;
//            }

//            if (!GUIManager.CustomGUIFront)
//            {
//                Jotunn.Logger.LogError("GUIManager CustomGUI is null");
//                return;
//            }

//            panel = GUIManager.Instance.CreateWoodpanel(
//                parent: GUIManager.CustomGUIFront.transform,
//                anchorMin: new Vector2(0.5f, 0.5f),
//                anchorMax: new Vector2(0.5f, 0.5f),
//                position: new Vector2(0, 0),
//                width: 500,
//                height: 400,
//                draggable: false
//            );
//            panel.SetActive(false);

//            GUIManager.Instance.CreateText(
//                text: "Enable Skeleton",
//                parent: panel.transform,
//                anchorMin: new Vector2(.5f, 1f),
//                anchorMax: new Vector2(.5f, 1f),
//                position: new Vector2(0f, -40f),
//                font: GUIManager.Instance.AveriaSerifBold,
//                fontSize: 18,
//                color: GUIManager.Instance.ValheimOrange,
//                outline: true,
//                outlineColor: Color.black,
//                width: 460f,
//                height: 30f,
//                addContentSizeFitter: false
//            );

//            GameObject checkboxEnable = GUIHelper.CreateToggle(
//                parent: panel.transform,
//                anchorMin: new Vector2(0.5f, 1f),
//                anchorMax: new Vector2(0.5f, 1f),
//                position: new Vector2(-200f, -70f),
//                width: 30f,
//                height: 30f
//            );
//            Toggle toggleEnable = checkboxEnable.GetComponent<Toggle>();
//            toggleEnable.isOn = currentValues.isSkeleton;
//            toggleEnable.onValueChanged.AddListener(OnIsSkeletonChanged);

//            GUIManager.Instance.CreateText(
//                text: "Skin",
//                parent: panel.transform,
//                anchorMin: new Vector2(0.5f, 1f),
//                anchorMax: new Vector2(0.5f, 1f),
//                position: new Vector2(0f, -110f),
//                font: GUIManager.Instance.AveriaSerifBold,
//                fontSize: 18,
//                color: GUIManager.Instance.ValheimOrange,
//                outline: true,
//                outlineColor: Color.black,
//                width: 460f,
//                height: 30f,
//                addContentSizeFitter: false
//            );

//            GameObject skinField = GUIManager.Instance.CreateDropDown(
//                parent: panel.transform,
//                anchorMin: new Vector2(0.5f, 1f),
//                anchorMax: new Vector2(0.5f, 1f),
//                position: new Vector2(0f, -150f),
//                fontSize: 18,
//                width: 460f,
//                height: 30f
//            );
//            Dropdown skinDropdown = skinField.GetComponent<Dropdown>();
//            skinDropdown.GetComponent<Dropdown>().AddOptions(new List<string>
//            {
//                "Test",
//            });
//            skinDropdown.value = StringToInt(currentValues.skin);
//            skinDropdown.onValueChanged.AddListener(OnSkinChanged);

//            GUIManager.Instance.CreateText(
//                text: "Can swim",
//                parent: panel.transform,
//                anchorMin: new Vector2(.5f, 1f),
//                anchorMax: new Vector2(.5f, 1f),
//                position: new Vector2(0f, -190f),
//                font: GUIManager.Instance.AveriaSerifBold,
//                fontSize: 18,
//                color: GUIManager.Instance.ValheimOrange,
//                outline: true,
//                outlineColor: Color.black,
//                width: 460f,
//                height: 30f,
//                addContentSizeFitter: false
//            );

//            GameObject checkboxCanSwim = GUIHelper.CreateToggle(
//                parent: panel.transform,
//                anchorMin: new Vector2(0.5f, 1f),
//                anchorMax: new Vector2(0.5f, 1f),
//                position: new Vector2(-200f, -220),
//                width: 30f,
//                height: 30f
//            );
//            Toggle toggleCanSwim = checkboxCanSwim.GetComponent<Toggle>();
//            toggleCanSwim.isOn = currentValues.canSwim;
//            toggleCanSwim.onValueChanged.AddListener(OnCanSwim);

//            GameObject buttonObject = GUIManager.Instance.CreateButton(
//                text: "Cancel",
//                parent: panel.transform,
//                anchorMin: new Vector2(0.5f, 0f),
//                anchorMax: new Vector2(0.5f, 0f),
//                position: new Vector2(-120f, 50f),
//                width: 225f,
//                height: 60f
//            );
//            buttonObject.SetActive(true);
//            Button button = buttonObject.GetComponent<Button>();
//            button.onClick.AddListener(CancelPanel);

//            GameObject buttonObject2 = GUIManager.Instance.CreateButton(
//                text: "Accept",
//                parent: panel.transform,
//                anchorMin: new Vector2(0.5f, 0f),
//                anchorMax: new Vector2(0.5f, 0f),
//                position: new Vector2(120f, 50f),
//                width: 225f,
//                height: 60f
//            );
//            buttonObject2.SetActive(true);
//            Button button2 = buttonObject2.GetComponent<Button>();
//            button2.onClick.AddListener(AcceptPanelValues);

//            currentFormValues = new UpdateSettingsOptions();
//            currentFormValues.isSkeleton = currentValues.isSkeleton;
//            currentFormValues.skin = currentValues.skin;
//            currentFormValues.canSwim = currentValues.canSwim;

//            panel.SetActive(true);
//            GUIManager.BlockInput(true);
//        }

//        private void AcceptPanelValues()
//        {
//            onAccept.Invoke(currentFormValues);
//            currentValues = currentFormValues;
//            currentFormValues = null;
//            panel.SetActive(false);
//            GUIManager.BlockInput(false);
//        }

//        private void CancelPanel()
//        {
//            onCancel.Invoke();
//            currentFormValues = null;
//            panel.SetActive(false);
//            GUIManager.BlockInput(false);
//        }

//        private int StringToInt(string value)
//        {
//            switch (value)
//            {
//                default:
//                    return 0;
//            }
//        }

//        private string IntToString(int value)
//        {
//            switch (value)
//            {
//                default:
//                    return "Test";
//            }

//        }
//    }
//}
