using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

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
                if (options.maxQuality > 0) { itemDrop.m_itemData.m_shared.m_maxQuality = (int)options.maxQuality; }
                if (options.movementModifier != null) { itemDrop.m_itemData.m_shared.m_movementModifier = (float)options.movementModifier; }

                if (options.blockPower != null) { itemDrop.m_itemData.m_shared.m_blockPower = (float)options.blockPower; }
                if (options.deflectionForce != null) { itemDrop.m_itemData.m_shared.m_deflectionForce = (float)options.deflectionForce; }
                if (options.attackForce != null) { itemDrop.m_itemData.m_shared.m_attackForce = (float)options.attackForce; }
                if (options.backstabBonus != null) { itemDrop.m_itemData.m_shared.m_backstabBonus = (float)options.backstabBonus; }

                if (options.attackEitr != null) { itemDrop.m_itemData.m_shared.m_attack.m_attackEitr = (float)options.attackEitr; }
                if (options.secondaryAttackEitr != null) { itemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackEitr = (float)options.secondaryAttackEitr; }
                if (options.damageBlunt != null) { itemDrop.m_itemData.m_shared.m_damages.m_blunt = (float)options.damageBlunt; }
                if (options.damageChop != null) { itemDrop.m_itemData.m_shared.m_damages.m_chop = (float)options.damageChop; }
                if (options.damageFire != null) { itemDrop.m_itemData.m_shared.m_damages.m_fire = (float)options.damageFire; }
                if (options.damageFrost != null) { itemDrop.m_itemData.m_shared.m_damages.m_frost = (float)options.damageFrost; }
                if (options.damageLightning != null) { itemDrop.m_itemData.m_shared.m_damages.m_lightning = (float)options.damageLightning; }
                if (options.damagePickaxe != null) { itemDrop.m_itemData.m_shared.m_damages.m_pickaxe = (float)options.damagePickaxe; }
                if (options.damagePierce != null) { itemDrop.m_itemData.m_shared.m_damages.m_pierce = (float)options.damagePierce; }
                if (options.damageSlash != null) { itemDrop.m_itemData.m_shared.m_damages.m_slash = (float)options.damageSlash; }
                if (options.damageSpirit != null) { itemDrop.m_itemData.m_shared.m_damages.m_spirit = (float)options.damageSpirit; }

                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }

                if (options.demister != null)
                {
                    UpdateHelper.UpdateDemisterOnPrefab(prefab, (float)options.demister);
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + error);
            }
        }

        public static void UpdateLanternMode(LanternPackageResult result)
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
                ParticleSystem flareComp = flareObj.GetComponent<ParticleSystem>();

                Color flareColor;
                Color lightColor;
                Color materialColor;

                if (ColorUtility.TryParseHtmlString(result.lightColor, out lightColor))
                    lightComp.color = lightColor;
                
                lightComp.range = (float)result.lightRange;
                lightComp.intensity = (float)result.lightIntensity;
                lightObj.SetActive(result.value);

                if (ColorUtility.TryParseHtmlString(result.flareColor, out flareColor))
                {
                    flareComp.startColor = flareColor;
                    flareComp.startSize = 2f;
                }

                flareObj.SetActive(result.value);
                demisterObj.SetActive(result.value);

                if (result.value)
                {
                    Jotunn.Logger.LogWarning("materialColor: " + result.materialColor);
                    string[] colorData = result.materialColor.Trim().Split(',');
                    Jotunn.Logger.LogWarning(float.Parse(colorData[0], NumberStyles.Any, CultureInfo.InvariantCulture));
                    materialColor = new Color(
                        float.Parse(colorData[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                        float.Parse(colorData[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                        float.Parse(colorData[2], NumberStyles.Any, CultureInfo.InvariantCulture),
                        float.Parse(colorData[3], NumberStyles.Any, CultureInfo.InvariantCulture));

                    Jotunn.Logger.LogWarning("Old Emission color: "+ result.lanternOn.GetColor("_EmissionColor").ToString());
                    Jotunn.Logger.LogWarning("New Emission color: " + materialColor.ToString());
                    result.lanternOn.SetColor("_EmissionColor", materialColor);
                }

                SkinnedMeshRenderer meshObj = lanternObj.GetComponent<SkinnedMeshRenderer>();
                List<Material> materialList = new List<Material> { result.value ? result.lanternOn : result.lanternOff };
                meshObj.materials = materialList.ToArray();
            }

            ModularMagic_Utilities.Instance.lanternStatusDictionary[result.playerId] = result.value;
            Jotunn.Logger.LogWarning($"Lantern of {player.GetPlayerName()} is now {result.value}");
        }

        public static void UpdateDemisterOnPrefab(GameObject prefab, float value)
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

        public static void UpdateDemisterOnPlayer(float value)
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

        public static void UpdateDemisterOnBoth(GameObject prefab, float value)
        {
            UpdateHelper.UpdateDemisterOnPrefab(prefab, value);
            UpdateHelper.UpdateDemisterOnPlayer(value);
        }

        public static void UpdateEitrRegenOnPlayer(string name, float value)
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

        public static Color32? HexToColor32(string hex)
        {
            Jotunn.Logger.LogWarning("Value: " + hex);
            if (hex.Length < 6)
            {
                throw new FormatException("Needs a string with a length of at least 6");
            }

            var r = hex.Substring(0, 2);
            var g = hex.Substring(2, 2);
            var b = hex.Substring(4, 2);
            string a;
            if (hex.Length >= 8)
                a = hex.Substring(6, 2);
            else
                a = "FF";

            int red = int.Parse(r, NumberStyles.HexNumber) / 255;
            int green = int.Parse(g, NumberStyles.HexNumber) / 255;
            int blue = int.Parse(b, NumberStyles.HexNumber) / 255;
            int alpha = int.Parse(a, NumberStyles.HexNumber) / 255;

            Jotunn.Logger.LogWarning($"Color: rgba({red}, {green}, {blue}, {alpha})");

            return new Color(red, green, blue, alpha);
        }
    }
}
