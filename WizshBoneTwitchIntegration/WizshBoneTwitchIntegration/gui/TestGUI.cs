using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Test panel for experimenting with reusable UI components.
    /// </summary>
    internal class TestGUI
    {
        private GameObject m_panel;
        private GameObject m_contentContainer;

        private TestDataObject m_testData = new TestDataObject();

        public void ShowTestPanel()
        {
            if (GUIManager.Instance == null || !GUIManager.CustomGUIFront)
                return;

            if (m_panel != null)
            {
                m_panel.transform.SetAsLastSibling();
                m_panel.SetActive(true);
                return;
            }

            m_panel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, 0f),
                width: 900f,
                height: 700f,
                draggable: true
            );

            m_panel.transform.SetAsLastSibling();
            CreateTestContent();
            m_panel.SetActive(true);
        }

        public void CloseTestPanel()
        {
            if (m_panel == null)
                return;

            m_panel.SetActive(false);
        }

        private void CreateTestContent()
        {
            // Title
            GameObject titleObj = GUIManager.Instance.CreateText(
                text: "FieldUIBuilder Test Panel",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 24,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 600f,
                height: 30f,
                addContentSizeFitter: false
            );
            titleObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            // Description
            GUIManager.Instance.CreateText(
                text: "Testing all supported field types: string, int, float, bool, List<string> (input & dropdown)",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -75f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 13,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 700f,
                height: 20f,
                addContentSizeFitter: false
            );

            // Scrollable content area — sits between the header (~100 px) and the footer buttons (~80 px)
            m_contentContainer = ScrollableView.CreateStretched(
                parent:    m_panel,
                name:      "TestContent",
                offsetMin: new Vector2(50f, 80f),
                offsetMax: new Vector2(-50f, -100f)
            );

            // Build all fields and get total height so the ScrollRect knows how far to scroll
            ObjectEditor editor = new ObjectEditor(startX: -200f, startY: -20f, fieldWidth: 250f);
            float contentHeight = editor.Build(m_contentContainer, m_testData);

            RectTransform contentRt = m_contentContainer.GetComponent<RectTransform>();
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, contentHeight);

            // Debug button
            GameObject debugBtn = GUIManager.Instance.CreateButton(
                text: "Print Values to Console",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-100f, 50f),
                width: 200f,
                height: 40f
            );
            debugBtn.SetActive(true);
            debugBtn.GetComponent<Button>().onClick.AddListener(PrintTestDataValues);

            // Close button
            GameObject closeBtn = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: m_panel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(100f, 50f),
                width: 150f,
                height: 40f
            );
            closeBtn.SetActive(true);
            closeBtn.GetComponent<Button>().onClick.AddListener(CloseTestPanel);
        }

        private void PrintTestDataValues()
        {
            Debug.Log("=== Test Data Values ===");
            Debug.Log($"testString: '{m_testData.testString}'");
            Debug.Log($"testInt: {m_testData.testInt}");
            Debug.Log($"testFloat: {m_testData.testFloat}");
            Debug.Log($"testBool: {m_testData.testBool}");
            Debug.Log($"testList: [{string.Join(", ", m_testData.testList)}]");
            Debug.Log($"testDropdownList: [{string.Join(", ", m_testData.testDropdownList)}]");
            Debug.Log("========================");

            MessageHud.instance?.ShowMessage(MessageHud.MessageType.Center, "Values printed to console (F5)");
        }

        public bool IsVisible => m_panel != null && m_panel.activeSelf;

        // =====================================================================
        // Test Data Class
        // =====================================================================

        private class TestDataObject
        {
            public string testString = "Hello World";
            public int testInt = 42;
            public float testFloat = 3.14f;
            public bool testBool = true;
            public List<string> testList = new List<string> { "Item1", "Item2", "Item3" };

            [DropdownOptions("OptionA", "OptionB", "OptionC")]
            public List<string> testDropdownList = new List<string> { "OptionA" };
        }
    }
}