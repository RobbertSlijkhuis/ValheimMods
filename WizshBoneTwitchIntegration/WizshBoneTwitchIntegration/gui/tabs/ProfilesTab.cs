using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Gui
{
    internal class ProfilesTab
    {
        private GameObject m_root;
        private InputField m_profileNameInput;
        private GameObject m_profileListContainer;
        private Text m_profileFeedbackText;

        private const float ButtonHeight = 40f;
        private const float ButtonSpacing = 5f;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
        {
            m_root = CreateTabContainer("ProfilesTab", parent);

            GUIManager.Instance.CreateText(
                text: "New profile name:",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-230f, -140f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 180f,
                height: 30f,
                addContentSizeFitter: false
            );

            m_profileNameInput = CreateInputField(m_root, new Vector2(0f, -140f), new Vector2(300f, 40f));

            GameObject createBtn = GUIManager.Instance.CreateButton(
                text: "Create",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(240f, -140f),
                width: 120f,
                height: 40f
            );
            createBtn.SetActive(true);
            createBtn.GetComponent<Button>().onClick.AddListener(OnCreateProfile);

            m_profileFeedbackText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -178f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 600f,
                height: 20f,
                addContentSizeFitter: false
            ).GetComponent<Text>();

            GUIManager.Instance.CreateText(
                text: "Available profiles:",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-280f, -205f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 180f,
                height: 20f,
                addContentSizeFitter: false
            );

            m_profileListContainer = createScrollable("ProfileList", m_root, -225f);

            return m_root;
        }

        public void Refresh()
        {
            ClearContainer(m_profileListContainer);

            List<string> profiles = ProfileManager.GetProfiles();
            float yOffset = -(ButtonHeight / 2f);

            foreach (string profile in profiles)
            {
                bool isActive = profile == ProfileManager.ActiveProfile;
                string profileName = profile;

                GameObject btn = GUIManager.Instance.CreateButton(
                    text: profile,
                    parent: m_profileListContainer.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(isActive ? 0f : -35f, yOffset),
                    width: isActive ? 400f : 330f,
                    height: ButtonHeight
                );
                btn.SetActive(true);
                btn.GetComponentInChildren<Text>().color = isActive
                    ? GUIManager.Instance.ValheimOrange
                    : GUIManager.Instance.ValheimBeige;

                if (!isActive)
                {
                    btn.GetComponent<Button>().onClick.AddListener(() => OnSelectProfile(profileName));

                    GameObject deleteBtn = GUIManager.Instance.CreateButton(
                        text: "X",
                        parent: m_profileListContainer.transform,
                        anchorMin: new Vector2(0.5f, 1f),
                        anchorMax: new Vector2(0.5f, 1f),
                        position: new Vector2(215f, yOffset),
                        width: ButtonHeight,
                        height: ButtonHeight
                    );
                    deleteBtn.SetActive(true);
                    deleteBtn.GetComponentInChildren<Text>().color = Color.red;
                    deleteBtn.GetComponent<Button>().onClick.AddListener(() => OnDeleteProfile(profileName));
                }

                yOffset -= ButtonHeight + ButtonSpacing;
            }

            RectTransform contentRt = m_profileListContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Abs(yOffset) + ButtonHeight / 2f);
        }

        private void OnCreateProfile()
        {
            if (m_profileNameInput == null)
                return;

            string name = m_profileNameInput.text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                m_profileFeedbackText.text = "Please enter a profile name.";
                return;
            }

            bool created = ProfileManager.CreateProfile(name);
            m_profileFeedbackText.text = created
                ? $"Profile '{name}' created!"
                : $"Profile '{name}' already exists.";

            m_profileNameInput.text = "";
            Refresh();
        }

        private void OnSelectProfile(string name)
        {
            ProfileManager.SelectProfile(name);
            Refresh();
        }

        private void OnDeleteProfile(string name)
        {
            bool deleted = ProfileManager.DeleteProfile(name);
            m_profileFeedbackText.text = deleted
                ? $"Profile '{name}' deleted."
                : $"Cannot delete profile '{name}'.";

            Refresh();
        }

        private static GameObject CreateTabContainer(string name, GameObject parent)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            container.SetActive(false);
            return container;
        }

        private static InputField CreateInputField(GameObject parent, Vector2 position, Vector2 size)
        {
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(parent.transform, false);

            RectTransform rt = inputObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;

            inputObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            InputField inputField = inputObj.AddComponent<InputField>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5f, 2f);
            textRt.offsetMax = new Vector2(-5f, -2f);

            Text text = textObj.AddComponent<Text>();
            text.font = GUIManager.Instance.AveriaSerifBold;
            text.fontSize = 14;
            text.color = Color.white;
            text.supportRichText = false;

            inputField.textComponent = text;
            inputField.text = "";
            inputObj.SetActive(true);

            return inputField;
        }

        private static void ClearContainer(GameObject container)
        {
            foreach (Transform child in container.transform)
                GameObject.Destroy(child.gameObject);
        }
    }
}