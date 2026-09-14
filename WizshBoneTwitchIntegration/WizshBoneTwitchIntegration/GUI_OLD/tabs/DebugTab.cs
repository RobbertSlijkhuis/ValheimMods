using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Debug tab of <see cref="WizshBoneSettingsGUI"/> - exposes debug-only toggles and one-off
    /// diagnostic actions. Purely client-local: nothing here touches ZDOs or the network.
    /// </summary>
    internal class DebugTab
    {
        private GameObject m_root;
        private Toggle m_safeZoneDebugToggle;
        private Text m_statusText;

        private const float TitleY = -153f;
        private const float SafeZoneToggleY = -220f;
        private const float SafeZoneDescY = -255f;
        private const float ResetButtonY = -340f;
        private const float StatusTextY = -390f;

        public GameObject Create(GameObject parent, CreateScrollableContainerDelegate createScrollable)
        {
            m_root = UIContainer.Create(parent, "DebugTab");

            TabUIHelper.CreateTabTitle("Debug:", m_root, new Vector2(-200f, TitleY));

            CreateSafeZoneDebugToggle();
            CreateResetSafezonesSection();

            return m_root;
        }

        public void Refresh()
        {
            // Re-sync in case the value changed elsewhere (e.g. an external config-manager
            // mod) while this tab wasn't visible.
            m_safeZoneDebugToggle.isOn = PluginConfig.configShowSafeZoneDebug.Value;
        }

        private void CreateSafeZoneDebugToggle()
        {
            GUIManager.Instance.CreateText(
                text: "Show safezone bounds",
                parent: m_root.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, SafeZoneToggleY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 260f,
                height: 30f,
                addContentSizeFitter: false
            );

            GameObject toggleObj = GUIManager.Instance.CreateToggle(
                parent: m_root.transform,
                width: 30f,
                height: 30f
            );
            RectTransform toggleRt = toggleObj.GetComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(.5f, 1f);
            toggleRt.anchorMax = new Vector2(.5f, 1f);
            toggleRt.pivot = new Vector2(0.5f, 0.5f);
            toggleRt.anchoredPosition = new Vector2(120f, SafeZoneToggleY - 5f);

            m_safeZoneDebugToggle = toggleObj.GetComponent<Toggle>();
            m_safeZoneDebugToggle.isOn = PluginConfig.configShowSafeZoneDebug.Value;
            m_safeZoneDebugToggle.onValueChanged.AddListener(val =>
            {
                // Setting .Value fires configShowSafeZoneDebug.SettingChanged, which already
                // calls TwitchSafeZone.RefreshDebugVisuals - no extra plumbing needed here.
                PluginConfig.configShowSafeZoneDebug.Value = val;
            });

            GUIManager.Instance.CreateText(
                text: "Shows the bounds of active safezones (ships, wards, traders) in-game as a wireframe outline.",
                parent: m_root.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, SafeZoneDescY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 12,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 700f,
                height: 30f,
                addContentSizeFitter: false
            );
        }

        private void CreateResetSafezonesSection()
        {
            GameObject resetBtnObj = GUIManager.Instance.CreateButton(
                text: "Reset Safezones",
                parent: m_root.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(-100f, ResetButtonY),
                width: 220f,
                height: 50f
            );
            resetBtnObj.SetActive(true);
            resetBtnObj.GetComponent<Button>().onClick.AddListener(OnResetSafezones);

            m_statusText = GUIManager.Instance.CreateText(
                text: "",
                parent: m_root.transform,
                anchorMin: new Vector2(.5f, 1f),
                anchorMax: new Vector2(.5f, 1f),
                position: new Vector2(-100f, StatusTextY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 30f,
                addContentSizeFitter: false
            ).GetComponent<Text>();
        }

        private void OnResetSafezones()
        {
            // Clears this client's own stuck safe-zone bookkeeping (entry counter / "in safe
            // zone" flag / HUD) - never touches placed Wards, ships, or trader zones.
            TwitchSafeZone.ForceExitAllZones();
            m_statusText.text = "Safezone state reset.";
        }
    }
}
