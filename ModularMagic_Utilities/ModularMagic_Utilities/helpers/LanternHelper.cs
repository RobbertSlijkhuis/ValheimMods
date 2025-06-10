using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Utilities.Helpers
{
    internal class LanternHelper
    {
        public static void UpdateLanternMode(long playerId, bool status)
        {
            try
            {
                // Jotunn.Logger.LogWarning("Status: " + playerId + ", " + status);
                Player player = Player.GetPlayer(playerId);

                if (player == null)
                    throw new Exception("Could not find Player object");

                // Jotunn.Logger.LogWarning("Found Player: " + playerId);
                Inventory inv = player.GetInventory();
                ItemData itemData = inv.GetEquippedItems().Find(item => 
                    item.m_shared.m_name == PluginConfig.lantern1.name.Value || 
                    item.m_shared.m_name == PluginConfig.lantern2.name.Value || 
                    item.m_shared.m_name == PluginConfig.lantern3.name.Value);

                if (itemData == null || itemData.m_shared == null)
                    return;

                // Jotunn.Logger.LogWarning("Found Utility: " + itemData.m_shared.m_name);
                int type = LanternHelper.GetLanternType(itemData);

                if (type == 0)
                    return;

                // Jotunn.Logger.LogWarning("Found type: " + type);
                GameObject lightObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Point Light").gameObject;
                GameObject flareObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern/MMU_Lantern flare").gameObject;
                GameObject demisterObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;
                GameObject lanternObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern").gameObject;
                Light lightComp = lightObj.GetComponent<Light>();
                LightLod lightLodComp = lightObj.GetComponent<LightLod>();
                ParticleSystem flareComp = flareObj.GetComponent<ParticleSystem>();
                LanternConfig config = LanternHelper.GetLanternConfig(type);

                lightComp.color = config.presetColors.lightColor;
                lightComp.range = config.lightRange;
                lightLodComp.m_baseRange = config.lightRange;
                lightComp.intensity = config.lightIntensity;
                lightObj.SetActive(status);

                // When new status is true, disable the component to force flare to change color
                if (status)
                    flareObj.SetActive(!status);

                ParticleSystem.MainModule flareMain = flareComp.main;
                flareMain.startColor = config.presetColors.flareColor;
                flareObj.SetActive(status);

                demisterObj.SetActive(status);

                if (status)
                    config.lanternOn.SetColor("_EmissionColor", config.presetColors.emissionColor);

                SkinnedMeshRenderer meshObj = lanternObj.GetComponent<SkinnedMeshRenderer>();
                List<Material> materialList = new List<Material> { status ? config.lanternOn : config.lanternOff };
                meshObj.materials = materialList.ToArray();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update lantern on Player: " + e);
            }
        }

        public static LanternConfig GetLanternConfig(int type)
        {
            try
            {
                Material lanternOn = null;
                Material lanternOff = null;
                LightPresetColors presetColors;
                string lightColorPreset = "White";
                float? lightRange = null;
                float? lightIntensity = null;

                switch (type)
                {
                    case 1:
                        lanternOn = ModularMagic_Utilities.Instance.materials.Lantern1;
                        lanternOff = ModularMagic_Utilities.Instance.materials.Lantern1Off;
                        lightColorPreset = PluginConfig.lantern1.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern1.lightRange.Value;
                        lightIntensity = PluginConfig.lantern1.lightIntensity.Value;
                        break;
                    case 2:
                        lanternOn = ModularMagic_Utilities.Instance.materials.Lantern2;
                        lanternOff = ModularMagic_Utilities.Instance.materials.Lantern2Off;
                        lightColorPreset = PluginConfig.lantern2.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern2.lightRange.Value;
                        lightIntensity = PluginConfig.lantern2.lightIntensity.Value;
                        break;
                    case 3:
                        lanternOn = ModularMagic_Utilities.Instance.materials.Lantern3;
                        lanternOff = ModularMagic_Utilities.Instance.materials.Lantern3Off;
                        lightColorPreset = PluginConfig.lantern3.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern3.lightRange.Value;
                        lightIntensity = PluginConfig.lantern3.lightIntensity.Value;
                        break;
                }

                presetColors = LightColorPresetHelper.GetColors(lightColorPreset);

                return new LanternConfig
                {
                    lanternOn = lanternOn,
                    lanternOff = lanternOff,
                    presetColors = presetColors,
                    lightColorPreset = lightColorPreset,
                    lightIntensity = (float)lightIntensity,
                    lightRange = (float)lightRange,
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not get lantern config: " + e);
                return null;
            }
        }

        public static int GetLanternType(ItemData itemData)
        {
            try
            {
                int type = 0;

                if (itemData.m_shared.m_name == PluginConfig.lantern1.name.Value)
                    type = 1;
                else if (itemData.m_shared.m_name == PluginConfig.lantern2.name.Value)
                    type = 2;
                else if (itemData.m_shared.m_name == PluginConfig.lantern3.name.Value)
                    type = 3;

                return type;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not get lantern type: " + e);
                return 0;
            }
        }
    }
}
