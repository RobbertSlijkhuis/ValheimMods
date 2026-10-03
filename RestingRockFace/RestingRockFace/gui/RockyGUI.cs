using Jotunn.Managers;
using RestingRockFace.Components;
using RestingRockFace.Helpers;
using RestingRockFace.Models;
using RestingRockFace.Types;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RestingRockFace.Gui
{
    internal class RockyGUI
    {
        private GameObject panel;

        public UnityEvent<RockyGuiResult> onAccept = new UnityEvent<RockyGuiResult>();
        public UnityEvent onClose = new UnityEvent();
        public UnityEvent onSelect = new UnityEvent();

        RockyControls rockyControls;
        RockyGuiResult currentForm;

        float panelWidth = 420;
        float panelHeight = 230;

        float fieldWidth = 385f;
        float fieldHeight = 30f;
        float labelHeight = 30f;

        float buttonWidth = 190f;
        float buttonHeight = 50f;
        float buttonYoffset = 42f;

        public RockyGUI(RockyControls rockyControls)
        {
            this.rockyControls = rockyControls;
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

            if (panel == null)
                panel = GUIManager.Instance.CreateWoodpanel(
                    parent: GUIManager.CustomGUIFront.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(0, 0),
                    width: panelWidth,
                    height: panelHeight,
                    draggable: false
                );

            currentForm = new RockyGuiResult(rockyControls.m_tameable.GetName(), rockyControls.m_face == -1 ? (int)RockyFaceEnum.NoFeeling : rockyControls.m_face);
            panel.SetActive(false);
            CreateGUI();

            panel.SetActive(true);
            GUIManager.BlockInput(true);
            RockyGuiState.IsOpen = true;
        }

        public void Close()
        {
            onClose.Invoke();
            panel.SetActive(false);
            GUIManager.BlockInput(false);
            RockyGuiState.IsOpen = false;
        }

        public void Accept()
        {
            onAccept.Invoke(currentForm);
            onClose.Invoke();
            panel.SetActive(false);
            GUIManager.BlockInput(false);
            RockyGuiState.IsOpen = false;
        }

        public void SetName(string value)
        {
            currentForm.name = value;
        }

        public void SetFace(int value)
        {
            currentForm.face = GetRockyFace(value);
        }

        public void UpdateGUI()
        {
            foreach (Transform child in panel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            CreateGUI();
        }

        private void CreateGUI()
        {
            GUIManager.Instance.CreateText(
                text: "Name",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -30f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 20,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: fieldWidth,
                height: labelHeight,
                addContentSizeFitter: false
            );

            GameObject nameField = GUIManager.Instance.CreateInputField(
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0, -60f),
                contentType: InputField.ContentType.Standard,
                placeholderText: "Name",
                fontSize: 18,
                width: fieldWidth,
                height: fieldHeight
            );
            InputField nameInput = nameField.GetComponent<InputField>();
            nameInput.text = currentForm.name;
            nameInput.onValueChanged.AddListener(SetName);
            nameInput.characterLimit = 64;

            GUIManager.Instance.CreateText(
                text: "Face",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0, -100f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 20,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: fieldWidth,
                height: labelHeight,
                addContentSizeFitter: false
            );

            GameObject faceSelect = GUIManager.Instance.CreateDropDown(
               parent: panel.transform,
               anchorMin: new Vector2(0.5f, 1f),
               anchorMax: new Vector2(0.5f, 1f),
               position: new Vector2(0, -130f),
               fontSize: 18,
               width: fieldWidth,
               height: fieldHeight
            );
            Dropdown faceDropdown = faceSelect.GetComponent<Dropdown>();
            faceDropdown.GetComponent<Dropdown>().AddOptions(new List<string>
            {
                nameof(RockyFaceTypes.Dead), nameof(RockyFaceTypes.LookLeft), nameof(RockyFaceTypes.LookRight), nameof(RockyFaceTypes.NoFeeling), nameof(RockyFaceTypes.Nothing), nameof(RockyFaceTypes.Sad),
                nameof(RockyFaceTypes.Scream), nameof(RockyFaceTypes.Smile), nameof(RockyFaceTypes.SmileBig),
            });
            faceDropdown.value = GetDropdownValue(currentForm.face);
            faceDropdown.onValueChanged.AddListener(SetFace);
            ScrollRect faceScroll = faceDropdown.template.GetComponent<ScrollRect>();
            if (faceScroll != null) faceScroll.scrollSensitivity = 1000f;

            GameObject cancelButtonObj = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-100f, buttonYoffset),
                width: buttonWidth,
                height: buttonHeight
            );
            cancelButtonObj.SetActive(true);
            Button cancelButton = cancelButtonObj.GetComponent<Button>();
            cancelButton.onClick.AddListener(Close);

            GameObject acceptButtonObj = GUIManager.Instance.CreateButton(
                text: "Accept",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(100f, buttonYoffset),
                width: buttonWidth,
                height: buttonHeight
            );
            acceptButtonObj.SetActive(true);
            Button acceptButton = acceptButtonObj.GetComponent<Button>();
            acceptButton.onClick.AddListener(Accept);
        }

        private int GetRockyFace(int value)
        {
            switch (value)
            {
                case 0:
                    return (int)RockyFaceEnum.Dead;
                case 1:
                    return (int)RockyFaceEnum.LookLeft;
                case 2:
                    return (int)RockyFaceEnum.LookRight;
                case 3:
                    return (int)RockyFaceEnum.NoFeeling;
                case 4:
                default:
                    return (int)RockyFaceEnum.Nothing;
                case 5:
                    return (int)RockyFaceEnum.Sad;
                case 6:
                    return (int)RockyFaceEnum.Scream;
                case 7:
                    return (int)RockyFaceEnum.Smile;
                case 8:
                    return (int)RockyFaceEnum.SmileBig;
            }
        }

        private int GetDropdownValue(int value)
        {
            switch (value)
            {
                case (int)RockyFaceEnum.Dead:
                    return 0;
                case (int)RockyFaceEnum.LookLeft:
                    return 1;
                case (int)RockyFaceEnum.LookRight:
                    return 2;
                case (int)RockyFaceEnum.NoFeeling:
                    return 3;
                case (int)RockyFaceEnum.Nothing:
                default:
                    return 4;
                case (int)RockyFaceEnum.Sad:
                    return 5;
                case (int)RockyFaceEnum.Scream:
                    return 6;
                case (int)RockyFaceEnum.Smile:
                    return 7;
                case (int)RockyFaceEnum.SmileBig:
                    return 8;
            }
        }
    }
}
