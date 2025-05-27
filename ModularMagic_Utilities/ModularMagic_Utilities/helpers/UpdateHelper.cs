using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Utilities.Helpers
{
    internal class UpdateHelper
    {
        public static void UpdateItemDropStats(GameObject prefab, UpdateItemDropStatsOptions options)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                    throw new Exception("ItemDrop is null");

                if (options.name != null) { itemDrop.m_itemData.m_shared.m_name = options.name; }
                if (options.description != null) { itemDrop.m_itemData.m_shared.m_description = options.description; }
                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }

                if (options.demister != null)
                    UpdateDemisterOnPrefab(prefab, (float)options.demister);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + e);
            }
        }

        public static void UpdateLanternMode(long playerId, bool status)
        {
            try
            {
                Player player = Player.GetPlayer(playerId);

                if (player == null)
                {
                    Jotunn.Logger.LogWarning("Could not find Player object");
                    return;
                }

                ItemDrop.ItemData itemData = Player.m_localPlayer.m_utilityItem;

                if (itemData == null || itemData.m_shared == null)
                    return;

                int type = UpdateHelper.GetLanternType(itemData);

                if (type == 0)
                    return;

                GameObject lightObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Point Light").gameObject;
                GameObject flareObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern/MMU_Lantern flare").gameObject;
                GameObject demisterObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;
                GameObject lanternObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern").gameObject;
                Light lightComp = lightObj.GetComponent<Light>();
                LightLod lightLodComp = lightObj.GetComponent<LightLod>();
                ParticleSystem flareComp = flareObj.GetComponent<ParticleSystem>();

                LanternConfig config = UpdateHelper.GetLanternConfig(type);

                lightComp.color = config.lightColor;
                lightComp.range = config.lightRange;
                lightLodComp.m_baseRange = config.lightRange;
                lightComp.intensity = config.lightIntensity;
                lightObj.SetActive(status);

                // When new status is true, disable the component to force flare to change color
                if (status)
                    flareObj.SetActive(!status);

                flareComp.startColor = config.flareColor;
                flareObj.SetActive(status);

                demisterObj.SetActive(status);

                if (status)
                    config.lanternOn.SetColor("_EmissionColor", config.glassColor);

                SkinnedMeshRenderer meshObj = lanternObj.GetComponent<SkinnedMeshRenderer>();
                List<Material> materialList = new List<Material> { status ? config.lanternOn : config.lanternOff };
                meshObj.materials = materialList.ToArray();

                // ModularMagic_Utilities.Instance.lanternStatusDict[playerId] = status;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update lantern on Player: " + e);
            }
        }

        public static void UpdateLanternMode(LanternPackageResult result)
        {
            try
            {
                Player player = Player.GetPlayer(result.playerId);

                if (player == null)
                {
                    Jotunn.Logger.LogWarning("Could not find Player object");
                    return;
                }

                if (result.applyLanternChanges)
                {
                    GameObject lightObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Point Light").gameObject;
                    GameObject flareObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern/MMU_Lantern flare").gameObject;
                    GameObject demisterObj = player.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;
                    GameObject lanternObj = player.transform.Find("Visual/attach_skin(Clone)/Lantern").gameObject;
                    Light lightComp = lightObj.GetComponent<Light>();
                    LightLod lightLodComp = lightObj.GetComponent<LightLod>();
                    ParticleSystem flareComp = flareObj.GetComponent<ParticleSystem>();

                    LanternConfig config = UpdateHelper.GetLanternConfig(result.type);

                    lightComp.color = config.lightColor;
                    lightComp.range = config.lightRange;
                    lightLodComp.m_baseRange = config.lightRange;
                    lightComp.intensity = config.lightIntensity;
                    lightObj.SetActive(result.value);

                    flareComp.startColor = config.flareColor;
                    flareComp.startSize = 2f;
                    flareObj.SetActive(result.value);
                    
                    demisterObj.SetActive(result.value);

                    if (result.value)
                        config.lanternOn.SetColor("_EmissionColor", config.glassColor);

                    SkinnedMeshRenderer meshObj = lanternObj.GetComponent<SkinnedMeshRenderer>();
                    List<Material> materialList = new List<Material> { result.value ? config.lanternOn : config.lanternOff };
                    meshObj.materials = materialList.ToArray();
                }

                ModularMagic_Utilities.Instance.lanternStatusDict[result.playerId] = result.value;
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
                Color flareColor = new Color(1f, 1f, 1f, 0.098f);
                Color glassColor = new Color(1f, 1f, 1f, 1f);
                Color lightColor = new Color(1f, 1f, 1f, 1f);
                string lightColorPreset = "White";
                float? lightRange = null;
                float? lightIntensity = null;
                //bool? enableCustomGlassColor = null;
                //string customGlassColor = null;

                switch (type)
                {
                    case 1:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern1Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern1OffMat;
                        //flareColor = PluginConfig.lantern1.flareColor.Value;
                        //lightColor = PluginConfig.lantern1.lightColor.Value;
                        lightColorPreset = PluginConfig.lantern1.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern1.lightRange.Value;
                        lightIntensity = PluginConfig.lantern1.lightIntensity.Value;
                        //enableCustomGlassColor = PluginConfig.lantern1.enableCustomGlassColor.Value;
                        //customGlassColor = PluginConfig.lantern1.customGlassColor.Value;
                        break;
                    case 2:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern2Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern2OffMat;
                        //flareColor = PluginConfig.lantern2.flareColor.Value;
                        //lightColor = PluginConfig.lantern2.lightColor.Value;
                        lightColorPreset = PluginConfig.lantern2.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern2.lightRange.Value;
                        lightIntensity = PluginConfig.lantern2.lightIntensity.Value;
                        //enableCustomGlassColor = PluginConfig.lantern2.enableCustomGlassColor.Value;
                        //customGlassColor = PluginConfig.lantern2.customGlassColor.Value;
                        break;
                    case 3:
                        lanternOn = ModularMagic_Utilities.Instance.materials.lantern3Mat;
                        lanternOff = ModularMagic_Utilities.Instance.materials.lantern3OffMat;
                        //flareColor = PluginConfig.lantern3.flareColor.Value;
                        //lightColor = PluginConfig.lantern3.lightColor.Value;
                        lightColorPreset = PluginConfig.lantern3.lightColorPreset.Value;
                        lightRange = PluginConfig.lantern3.lightRange.Value;
                        lightIntensity = PluginConfig.lantern3.lightIntensity.Value;
                        //enableCustomGlassColor = PluginConfig.lantern3.enableCustomGlassColor.Value;
                        //customGlassColor = PluginConfig.lantern3.customGlassColor.Value;
                        break;
                }

                switch (lightColorPreset)
                {
                    case "Red":
                        flareColor = new Color(1f, 0.2117647f, 0.2261669f, 0.09803922f); // #FF363A19
                        glassColor = new Color(2.4f, 0.150596f, 0.1345381f, 1f);
                        lightColor = new Color(1f, 0.3001108f, 0.2862746f, 1f); // #FF4D49
                        break;
                    case "Orange":
                        flareColor = new Color(1f, 0.4899594f, 0.2132353f, 0.09803922f); // #FF7D3619
                        glassColor = new Color(2.1f, 0.325123f, 0f, 1f);
                        lightColor = new Color(1f, 0.6207767f, 0.4823529f, 1f); // #FF9E7B
                        break;
                    case "Yellow":
                        flareColor = new Color(1f, 0.4901961f, 0.2117647f, 0.09803922f); // #FF7D3619
                        glassColor = new Color(1.97667456f, 1.14168906f, 0.131697819f, 1f);
                        lightColor = new Color(1f, 0.7845517f, 0.2877358f, 1f); // #FFC849
                        break;
                    case "Green":
                        flareColor = new Color(0f, 1, 0.04705882f, 0.09803922f); // #00FF0C19
                        glassColor = new Color(0.382916f, 1.9f, 0f, 1f);
                        lightColor = new Color(0.6295277f, 1f, 0.4823529f, 1f); // #FFC849
                        break;
                    case "LemonGreen":
                        flareColor = new Color(0.4578981f, 1, 0f, 0.09803922f); // #75FF0019
                        glassColor = new Color(0.9056982f, 1.9f, 0f, 1f);
                        lightColor = new Color(0.8171905f, 1f, 0.482353f, 1f); // #FFC849
                        break;
                    case "LightBlue":
                        flareColor = new Color(0.5801887f, 0.8593694f, 1f, 0.09803922f); // #94DBFF19
                        glassColor = new Color(1.26792896f, 1.7979852f, 1.96205056f, 1f);
                        lightColor = new Color(0.8349056f, 0.9467509f, 1f, 1f); // #D5F1FF
                        break;
                    case "Blue":
                        flareColor = new Color(0f, 0.7254902f, 1f, 0.09803922f); // #00B9FF19
                        glassColor = new Color(0f, 0.9814469f, 2.3f, 1f);
                        lightColor = new Color(0.482353f, 0.7708116f, 1f, 1f); // #7BC5FF
                        break;
                    case "Pink":
                        flareColor = new Color(1f, 0.4784314f, 0.7215686f, 0.09803922f); // #FF7AB819
                        glassColor = new Color(1.97667456f, 0f, 1.39524257f, 1f);
                        lightColor = new Color(1f, 0.7688679f, 0.8862273f, 1f); // #FFC4E2
                        break;
                    case "White":
                        flareColor = new Color(1f, 1f, 1f, 0.098f);
                        glassColor = new Color(1.7f, 1.7f, 1.7f, 1f);
                        lightColor = new Color(1f, 1f, 1f, 1f);
                        break;
                    default:
                        flareColor = new Color(1f, 1f, 1f, 0.098f);
                        glassColor = new Color(1f, 1f, 1f, 1f);
                        lightColor = new Color(1f, 1f, 1f, 1f);
                        break;
                }
                
                return new LanternConfig
                {
                    lanternOn = lanternOn,
                    lanternOff = lanternOff,
                    flareColor = flareColor,
                    glassColor = glassColor,
                    lightColor = lightColor,
                    lightColorPreset = lightColorPreset,
                    lightIntensity = (float)lightIntensity,
                    lightRange = (float)lightRange,
                    //enableCustomGlassColor = (bool)enableCustomGlassColor,
                    //customGlassColor = customGlassColor,

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
                {
                    type = 1;
                }
                else if (itemData.m_shared.m_name == PluginConfig.lantern2.name.Value)
                {
                    type = 2;
                }
                else if (itemData.m_shared.m_name == PluginConfig.lantern3.name.Value)
                {
                    type = 3;
                }

                return type;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not get lantern type: " + e);
                return 0;
            }
        }

        public static void UpdateDemisterOnPrefab(GameObject prefab, float value)
        {
            try
            {
                GameObject demisterPrefab = prefab.transform.Find("attach_skin/equiped/MMU_Lantern Demister").gameObject;

                if (demisterPrefab == null)
                {
                    Jotunn.Logger.LogWarning("Could not find demister obj on prefab: " + prefab.name);
                    return;
                }

                if (value > 50)
                    value = 50;
                else if (value < 0) 
                    value = 0;

                ParticleSystemForceField comp = demisterPrefab.GetComponent<ParticleSystemForceField>();
                comp.endRange = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not alter demister effect on prefab: " + e);
            }
}

        public static void UpdateDemisterOnPlayer(float value)
        {
            try
            {
                if (Player.m_localPlayer == null)
                {
                    Jotunn.Logger.LogWarning("Could not find local Player object");
                    return;
                }

                GameObject demisterPlayerObj = Player.m_localPlayer.transform.Find("Visual/attach_skin(Clone)/equiped/MMU_Lantern Demister").gameObject;

                if (demisterPlayerObj == null)
                {
                    Jotunn.Logger.LogWarning("Could not find demister obj on Player");
                    return;
                }

                if (value > 50)
                    value = 50;
                else if (value < 0)
                    value = 0;

                ParticleSystemForceField comp = demisterPlayerObj.GetComponent<ParticleSystemForceField>();
                comp.endRange = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not alter demister effect on player: " + e);
            }
        }

        public static void UpdateDemisterOnBoth(GameObject prefab, float value)
        {
            UpdateHelper.UpdateDemisterOnPrefab(prefab, value);
            UpdateHelper.UpdateDemisterOnPlayer(value);
        }

        public static void UpdateEitrRegenOnPlayer(string name, float value)
        {
            try
            {
                if (Player.m_localPlayer == null)
                {
                    Jotunn.Logger.LogWarning("Could not find local player object to update Eitr regen");
                    return;
                }

                Inventory inv = Player.m_localPlayer.GetInventory();

                if (inv == null)
                {
                    Jotunn.Logger.LogWarning("Could not find local player's inventory to update Eitr regen");
                    return;
                }

                if (!inv.ContainsItemByName(name))
                {
                    return;
                }

                List<ItemDrop.ItemData> list = inv.GetAllItems();
                List<ItemDrop.ItemData> items = list.FindAll(item => item.m_shared.m_name == name);

                foreach (ItemDrop.ItemData item in items)
                {
                    if (item == null || item.m_shared == null)
                    {
                        Jotunn.Logger.LogWarning("Could not find " + name + " in inventory list to update Eitr regen");
                        continue;
                    }

                    Jotunn.Logger.LogWarning("Update Eitr regen to: " + value);
                    item.m_shared.m_eitrRegenModifier = value;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update eitr regen on player: " + e);
            }
        }
    }
}
