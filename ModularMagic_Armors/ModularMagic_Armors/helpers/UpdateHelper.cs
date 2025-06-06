using Jotunn.Managers;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Models;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    internal class UpdateHelper
    {
        public static readonly PathHelper pathHelper = new PathHelper();

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
                if (options.armor != null) { itemDrop.m_itemData.m_shared.m_armor = (float)options.armor; }
                if (options.armorPerLevel != null) { itemDrop.m_itemData.m_shared.m_armorPerLevel = (float)options.armorPerLevel; }
                if (options.armorSetOptions != null) {
                    itemDrop.m_itemData.m_shared.m_setName = options.armorSetOptions.name;
                    itemDrop.m_itemData.m_shared.m_setSize = options.armorSetOptions.size;
                    itemDrop.m_itemData.m_shared.m_setStatusEffect = options.armorSetOptions.statusEffect;
                }
                if (options.equipStatusEffect == null || options.equipStatusEffect.name != "empty_MMA") { itemDrop.m_itemData.m_shared.m_equipStatusEffect = options.equipStatusEffect; }
                if (options.weight != null) { itemDrop.m_itemData.m_shared.m_weight = (float)options.weight; }
                if (options.maxDurability != null) { itemDrop.m_itemData.m_shared.m_maxDurability = (float)options.maxDurability; }
                if (options.maxQuality > 0) { itemDrop.m_itemData.m_shared.m_maxQuality = (int)options.maxQuality; }
                if (options.movementSpeed != null) { itemDrop.m_itemData.m_shared.m_movementModifier = (float)options.movementSpeed; }
                if (options.eitrRegen != null) { itemDrop.m_itemData.m_shared.m_eitrRegenModifier = (float)options.eitrRegen; }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update ItemDrop stats: " + e);
            }
        }

        public static void UpdateItemEffects(long playerId, List<int> items)
        {
            try
            {
                Player player = Player.GetPlayer(playerId);

                if (player == null)
                    return;

                GameObject eyeLeft;
                GameObject eyeRight;

                bool wraithHelmet = items.Contains(ModularMagic_Armors.WraithHelmetHashCode);
                bool wraithChest = items.Contains(ModularMagic_Armors.WraithChestHashCode);
                bool wraithLegs = items.Contains(ModularMagic_Armors.WraithLegsHashCode);
                bool darkWizardHelmet = items.Contains(ModularMagic_Armors.DarkWizardHelmetHashCode);
                bool emblaHelmet = items.Contains(ModularMagic_Armors.EmblaHelmetHashCode);
                bool emblaChest = items.Contains(ModularMagic_Armors.EmblaChestHashCode);

                eyeLeft = player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.EyeLeftPath).gameObject;
                eyeRight = player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.EyeRightPath).gameObject;

                if (wraithHelmet)
                {
                    eyeLeft.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.WraithArmorEye;
                    eyeRight.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.WraithArmorEye;
                }

                player.gameObject.transform.Find(pathHelper.headPath + "/"+ pathHelper.WraithHeadEffectPath).gameObject.SetActive(wraithHelmet);

                player.gameObject.transform.Find(pathHelper.spine1Path + "/" + pathHelper.WraithChestEffectPath).gameObject.SetActive(wraithChest);
                player.gameObject.transform.Find(pathHelper.handLeftPath + "/" + pathHelper.WraithHandLeftEffectPath).gameObject.SetActive(wraithChest);
                player.gameObject.transform.Find(pathHelper.handRightPath + "/" + pathHelper.WraithHandRightEffectPath).gameObject.SetActive(wraithChest);

                player.gameObject.transform.Find(pathHelper.kneeLeftPath + "/" + pathHelper.WraithKneeLeftEffectPath).gameObject.SetActive(wraithLegs);
                player.gameObject.transform.Find(pathHelper.kneeRightPath + "/" + pathHelper.WraithKneeRightEffectPath).gameObject.SetActive(wraithLegs);

                if (darkWizardHelmet)
                {
                    eyeLeft.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.DarkWizardArmorEye;
                    eyeRight.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.DarkWizardArmorEye;
                }

                eyeLeft.SetActive((wraithHelmet || darkWizardHelmet) ? true : false);
                eyeRight.SetActive((wraithHelmet || darkWizardHelmet) ? true : false);

                if (PluginConfig.adjustEmbla.Value)
                {
                    player.gameObject.transform.Find(pathHelper.helmetAttachPath + "/" + pathHelper.EmblaHoodEffectsPath)?.gameObject.SetActive(emblaHelmet);
                }

                if (PluginConfig.adjustEmbla.Value)
                {
                    player.gameObject.transform.Find(pathHelper.shoulderLeftPath + "/" + pathHelper.EmblaShoulderLeftEffectPath)?.gameObject.SetActive(emblaChest);
                    player.gameObject.transform.Find(pathHelper.shoulderRightPath + "/" + pathHelper.EmblaShoulderRightEffectPath)?.gameObject.SetActive(emblaChest);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not de/activate item effects: " + e);
            }
        }

        public static void UpdateSetEffects(long playerId, int? setHash)
        {
            try
            {
                Player player = Player.GetPlayer(playerId);

                if (player == null)
                    return;

                if (setHash == ModularMagic_Armors.ShamanArmorSetHashCode)
                {
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.ShamanHeadEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.ShamanAntlerLefEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.ShamanAntlerRightEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.wristLeftPath + "/" + pathHelper.ShamanWristLeftEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.wristRightPath + "/" + pathHelper.ShamanWristRightEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.kneeLeftPath + "/" + pathHelper.ShamanKneeLeftEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.kneeRightPath + "/" + pathHelper.ShamanKneeRightEffectPath).gameObject.SetActive(true);
                }
                else
                {
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.ShamanHeadEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.ShamanAntlerLefEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.ShamanAntlerRightEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.wristLeftPath + "/" + pathHelper.ShamanWristLeftEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.wristRightPath + "/" + pathHelper.ShamanWristRightEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.kneeLeftPath + "/" + pathHelper.ShamanKneeLeftEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.kneeRightPath + "/" + pathHelper.ShamanKneeRightEffectPath).gameObject.SetActive(false);
                }

                if (setHash == ModularMagic_Armors.WraithArmorSetHashCode)
                {

                    player.gameObject.transform.Find(pathHelper.bodyPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.WraithFaceEffectPath).gameObject.SetActive(true);

                    if (ModularMagic_Armors.Instance.updatePlayerBeard)
                    {
                        player.SetBeard("BeardNone");
                    }
                }
                else
                {
                    player.gameObject.transform.Find(pathHelper.bodyPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.WraithFaceEffectPath).gameObject.SetActive(false);

                    if (ModularMagic_Armors.Instance.updatePlayerBeard && ModularMagic_Armors.Instance.playerBeard != null)
                        player.SetBeard(ModularMagic_Armors.Instance.playerBeard);
                }

                if (setHash == ModularMagic_Armors.FrostWolfArmorSetHashCode)
                {
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.FrostwolfHeadEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.spine2Path + "/" + pathHelper.FrostwolfChestEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.handLeftPath + "/" + pathHelper.FrostwolfHandLeftEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.handRightPath + "/" + pathHelper.FrostwolfHandRightEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.kneeLeftPath + "/" + pathHelper.FrostwolfKneeLeftEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.kneeRightPath + "/" + pathHelper.FrostwolfKneeRightEffectPath).gameObject.SetActive(true);
                }
                else
                {
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.FrostwolfHeadEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.spine2Path + "/" + pathHelper.FrostwolfChestEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.handLeftPath + "/" + pathHelper.FrostwolfHandLeftEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.handRightPath + "/" + pathHelper.FrostwolfHandRightEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.kneeLeftPath + "/" + pathHelper.FrostwolfKneeLeftEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.kneeRightPath + "/" + pathHelper.FrostwolfKneeRightEffectPath).gameObject.SetActive(false);
                }

                if (setHash == ModularMagic_Armors.DarkWizardArmorSetHashCode)
                {
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.EyeLeftPath + "/flames").gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.EyeRightPath + "/flames").gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.shoulderLeftPath + "/" + pathHelper.DarkWizardShoulderLeftEffectPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(pathHelper.shoulderRightPath + "/" + pathHelper.DarkWizardShoulderRightEffectPath).gameObject.SetActive(true);
                }
                else
                {
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.EyeLeftPath + "/flames").gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.headPath + "/" + pathHelper.EyeRightPath +"/flames").gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.shoulderLeftPath + "/" + pathHelper.DarkWizardShoulderLeftEffectPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(pathHelper.shoulderRightPath + "/" + pathHelper.DarkWizardShoulderRightEffectPath).gameObject.SetActive(false);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not de/activate set effects: " + e);
            }
        }

        public static void UpdateEitrRegenOnPlayer(string name, float value)
        {
            try
            {
                if (Player.m_localPlayer == null)
                    return;

                Inventory inv = Player.m_localPlayer.GetInventory();

                if (inv == null)
                    throw new Exception("Inventory is null");

                if (!inv.ContainsItemByName(name))
                    return;

                List<ItemDrop.ItemData> list = inv.GetAllItems();
                List<ItemDrop.ItemData> items = list.FindAll(item => item.m_shared.m_name == name);

                foreach (ItemDrop.ItemData item in items)
                {
                    if (item == null || item.m_shared == null)
                    {
                        Jotunn.Logger.LogError("Could not find " + name + " in inventory list to update Eitr regen");
                        continue;
                    }

                    item.m_shared.m_eitrRegenModifier = value;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update Eitr regen on player: " + e);
            }
        }

        public static UpdateItemDropStatsOptions TakeSnapShot(GameObject prefab)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                    throw new Exception("ItemDrop is null");

                ArmorSetOptions armorSetOptions = new ArmorSetOptions()
                {
                    name = itemDrop.m_itemData.m_shared.m_setName,
                    size = itemDrop.m_itemData.m_shared.m_setSize,
                    statusEffect = itemDrop.m_itemData.m_shared.m_setStatusEffect,
                };

                return new UpdateItemDropStatsOptions()
                {
                    name = itemDrop.m_itemData.m_shared.m_name,
                    description = itemDrop.m_itemData.m_shared.m_description,
                    armor = itemDrop.m_itemData.m_shared.m_armor,
                    armorPerLevel = itemDrop.m_itemData.m_shared.m_armorPerLevel,
                    armorSetOptions = armorSetOptions,
                    equipStatusEffect = itemDrop.m_itemData.m_shared.m_equipStatusEffect,
                    weight = itemDrop.m_itemData.m_shared.m_weight,
                    maxDurability = itemDrop.m_itemData.m_shared.m_maxDurability,
                    maxQuality = itemDrop.m_itemData.m_shared.m_maxQuality,
                    movementSpeed = itemDrop.m_itemData.m_shared.m_movementModifier,
                    eitrRegen = itemDrop.m_itemData.m_shared.m_eitrRegenModifier,
                };
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not create snapshot of stats: " + e);
                return null;
            }
        }
    }
}
