using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class WizshBoneSettingsGUI
    {
        private GameObject panel;

        public void ShowSettings()
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
                return;

            if (panel != null)
            {
                panel.transform.SetAsLastSibling();
                panel.SetActive(true);
                return;
            }

            panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 600,
                height: 500,
                draggable: false
            );

            panel.transform.SetAsLastSibling();
            CreateGUI();
            panel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (panel == null)
                return;

            panel.SetActive(false);
        }

        private void CreateGUI()
        {
            GUIManager.Instance.CreateText(
                text: "Settings",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 24,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 30f,
                addContentSizeFitter: false
            );

            GameObject closeButtonObj = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(0f, 40f),
                width: 200f,
                height: 60f
            );
            closeButtonObj.SetActive(true);
            closeButtonObj.GetComponent<Button>().onClick.AddListener(CloseSettings);
        }
    }
}