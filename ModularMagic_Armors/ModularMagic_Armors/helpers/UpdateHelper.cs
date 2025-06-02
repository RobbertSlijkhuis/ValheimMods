using Jotunn.Managers;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    internal class UpdateHelper
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static EffectList.EffectData[] OriginalEffects;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
                bool tiara = items.Contains(ModularMagic_Armors.TiaraHashCode);

                eyeLeft = player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left").gameObject;
                eyeRight = player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right").gameObject;
                eyeLeft.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.WraithArmorEye;
                eyeRight.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.WraithArmorEye;
                eyeLeft.SetActive(wraithHelmet);
                eyeRight.SetActive(wraithHelmet);
                player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_swamp_effect_head").gameObject.SetActive(wraithHelmet);

                player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.spine1Path + "/ME_swamp_effect_spine1").gameObject.SetActive(wraithChest);
                player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handLeftPath + "/ME_swamp_effect_hand_left").gameObject.SetActive(wraithChest);
                player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handRightPath + "/ME_swamp_effect_hand_right").gameObject.SetActive(wraithChest);

                player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_swamp_effect_knee_left").gameObject.SetActive(wraithLegs);
                player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_swamp_effect_knee_right").gameObject.SetActive(wraithLegs);

                eyeLeft = player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left").gameObject;
                eyeRight = player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right").gameObject;
                eyeLeft.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.DarkWizardArmorEye;
                eyeRight.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.DarkWizardArmorEye;
                eyeLeft.SetActive(darkWizardHelmet);
                eyeRight.SetActive(darkWizardHelmet);

                if (!PluginConfig.adjustEmbla.Value)
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.helmetAttachPath + "/EmblaHood_Effects_MMA")?.gameObject.SetActive(emblaHelmet);
                }

                if (!PluginConfig.adjustEmbla.Value)
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderLeftPath + "/EmblaChest_Left_Effects_MMA")?.gameObject.SetActive(emblaChest);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderRightPath + "/EmblaChest_Right_Effects_MMA")?.gameObject.SetActive(emblaChest);
                }

                GameObject prefab = PrefabManager.Instance.GetPrefab("MMES_TheForestFlinger");
                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
                List<EffectList.EffectData> effectList = new List<EffectList.EffectData>();
                EffectList.EffectData effectData = new EffectList.EffectData();
                effectData.m_prefab = ModularMagic_Armors.Instance.prefabs.AudaciousSFX;
                effectData.m_variant = -1;
                effectData.m_enabled = true;
                effectList.Add(effectData);

                if (OriginalEffects == null)
                    OriginalEffects = itemDrop.m_itemData.m_shared.m_startEffect.m_effectPrefabs;

                itemDrop.m_itemData.m_shared.m_startEffect.m_effectPrefabs = tiara ? effectList.ToArray() : OriginalEffects;

                List<Player> players = Player.GetAllPlayers();

                foreach (Player p in players)
                {
                    Inventory inv = p.GetInventory();

                    if (!inv.ContainsItemByName("The Forest Flinger"))
                        continue;

                    var staffs = inv.GetAllItemsOfType(ItemDrop.ItemData.ItemType.TwoHandedWeapon);

                    foreach (ItemDrop.ItemData staff in staffs)
                    {
                        if (staff.m_dropPrefab.name != "MMES_TheForestFlinger")
                            continue;

                        staff.m_shared.m_startEffect.m_effectPrefabs = tiara ? effectList.ToArray() : OriginalEffects;
                    }
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
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_head").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_left").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_right").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristLeftPath + "/ME_blackforest_effect_wrist_left").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristRightPath + "/ME_blackforest_effect_wrist_right").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_blackforest_effect_knee_left").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_blackforest_effect_knee_right").gameObject.SetActive(true);
                }
                else
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_head").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_left").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_right").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristLeftPath + "/ME_blackforest_effect_wrist_left").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristRightPath + "/ME_blackforest_effect_wrist_right").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_blackforest_effect_knee_left").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_blackforest_effect_knee_right").gameObject.SetActive(false);
                }

                if (setHash == ModularMagic_Armors.WraithArmorSetHashCode)
                {

                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.bodyPath).gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_swamp_effect_face").gameObject.SetActive(true);
                    if (player.GetBeard() != "")
                    {
                        ModularMagic_Armors.Instance.playerBeard = player.GetBeard();
                        player.SetBeard("BeardNone");
                    }
                }
                else
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.bodyPath).gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_swamp_effect_face").gameObject.SetActive(false);
                    if (ModularMagic_Armors.Instance.playerBeard != null)
                        player.SetBeard(ModularMagic_Armors.Instance.playerBeard);
                }

                if (setHash == ModularMagic_Armors.FrostWolfArmorSetHashCode)
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_mountain_effect_head").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.spine2Path + "/ME_mountain_effect_spine2").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handLeftPath + "/ME_mountain_effect_hand_left").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handRightPath + "/ME_mountain_effect_hand_right").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_mountain_effect_knee_left").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_mountain_effect_knee_right").gameObject.SetActive(true);
                }
                else
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_mountain_effect_head").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.spine2Path + "/ME_mountain_effect_spine2").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handLeftPath + "/ME_mountain_effect_hand_left").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handRightPath + "/ME_mountain_effect_hand_right").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_mountain_effect_knee_left").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_mountain_effect_knee_right").gameObject.SetActive(false);
                }

                if (setHash == ModularMagic_Armors.DarkWizardArmorSetHashCode)
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left/flames").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right/flames").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderLeftPath + "/ME_plains_effect_shoulder_left").gameObject.SetActive(true);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderRightPath + "/ME_plains_effect_shoulder_right").gameObject.SetActive(true);
                    // ModularMagic_Armors.Instance.prefabs.PlainsMageFootStepsPrefab.SetActive(true);
                }
                else
                {
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left/flames").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right/flames").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderLeftPath + "/ME_plains_effect_shoulder_left").gameObject.SetActive(false);
                    player.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderRightPath + "/ME_plains_effect_shoulder_right").gameObject.SetActive(false);
                    // ModularMagic_Armors.Instance.prefabs.PlainsMageFootStepsPrefab.SetActive(false);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not de/activate set effects: " + e);
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

        public static void UpdateEitrRegenOnPlayer(string name, float value)
        {
            try
            {
                if (Player.m_localPlayer == null)
                    throw new Exception("Local player is null");

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
    }
}
