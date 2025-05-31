using HarmonyLib;
using Jotunn.Managers;
using ModularMagic_Armors.Configs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Harmony
{
    [HarmonyPatch]
    public class Patches_MMA
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            try
            {
                if (__instance == null)
                    return;

                _SetEitr(__instance, PluginConfig.armor1Helmet.magicStatusEffectName, PluginConfig.armor1Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor1Cape.magicStatusEffectName, PluginConfig.armor1Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor1Chest.magicStatusEffectName, PluginConfig.armor1Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor1Legs.magicStatusEffectName, PluginConfig.armor1Legs.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Helmet.magicStatusEffectName, PluginConfig.armor2Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Cape.magicStatusEffectName, PluginConfig.armor2Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Chest.magicStatusEffectName, PluginConfig.armor2Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor2Legs.magicStatusEffectName, PluginConfig.armor2Legs.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Helmet.magicStatusEffectName, PluginConfig.armor3Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Cape.magicStatusEffectName, PluginConfig.armor3Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Chest.magicStatusEffectName, PluginConfig.armor3Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor3Legs.magicStatusEffectName, PluginConfig.armor3Legs.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Helmet.magicStatusEffectName, PluginConfig.armor4Helmet.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Cape.magicStatusEffectName, PluginConfig.armor4Cape.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Chest.magicStatusEffectName, PluginConfig.armor4Chest.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.armor4Legs.magicStatusEffectName, PluginConfig.armor4Legs.eitr.Value, ref eitr);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update Eitr in GetTotalFoodValue_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Skills), "GetSkillLevel")]
        public static void GetSkillLevel_Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
        {
            try
            {
                if (skillType != Skills.SkillType.ElementalMagic && skillType != Skills.SkillType.BloodMagic)
                    return;

                float value = 0f;

                if (_HaveStatusEffect(__instance, PluginConfig.armor1Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor1Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor1Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor1Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor1Legs);

                if (_HaveStatusEffect(__instance, PluginConfig.armor2Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Legs);

                if (_HaveStatusEffect(__instance, PluginConfig.armor3Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor3Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor3Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor3Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor3Legs);

                if (_HaveStatusEffect(__instance, PluginConfig.armor2Helmet.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Helmet);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Cape.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Cape);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Chest.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Chest);
                if (_HaveStatusEffect(__instance, PluginConfig.armor2Legs.magicStatusEffectName))
                    value += _GetValueFromConfig(skillType, PluginConfig.armor2Legs);

                float newLevel = __result + value;
                __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref newLevel);
                __result = newLevel;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update skills in GetSkillLevel_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ItemDrop.ItemData item)
        {
            try
            {
                _UpdateItemEffects(item, true);
                _UpdateSetEffects();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static void UnequipItem_Postfix(ItemDrop.ItemData item)
        {
            try
            {
                _UpdateItemEffects(item, false);
                _UpdateSetEffects();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/sets effects in UnequipItem_Postfix: " + e);
            }
        }

        public static void _UpdateSetEffects()
        {
            try
            {
                if (Player.m_localPlayer == null) return;

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(ModularMagic_Armors.Instance.effects.ShamanArmorSetSE.name)))
                {
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_head").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_right").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristLeftPath + "/ME_blackforest_effect_wrist_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristRightPath + "/ME_blackforest_effect_wrist_right").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_blackforest_effect_knee_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_blackforest_effect_knee_right").gameObject.SetActive(true);
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_head").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_right").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristLeftPath + "/ME_blackforest_effect_wrist_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.wristRightPath + "/ME_blackforest_effect_wrist_right").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_blackforest_effect_knee_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_blackforest_effect_knee_right").gameObject.SetActive(false);
                }

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(ModularMagic_Armors.Instance.effects.WraithArmorSetSE.name)))
                {

                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.bodyPath).gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_swamp_effect_face").gameObject.SetActive(true);
                    if (Player.m_localPlayer.GetBeard() != "")
                    {
                        ModularMagic_Armors.Instance.playerBeard = Player.m_localPlayer.GetBeard();
                        Player.m_localPlayer.SetBeard("BeardNone");
                    }
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.bodyPath).gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_swamp_effect_face").gameObject.SetActive(false);
                    if (ModularMagic_Armors.Instance.playerBeard != null)
                        Player.m_localPlayer.SetBeard(ModularMagic_Armors.Instance.playerBeard);
                }

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(ModularMagic_Armors.Instance.effects.FrostWolfArmorSetSE.name)))
                {
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_mountain_effect_head").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.spine2Path + "/ME_mountain_effect_spine2").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handLeftPath + "/ME_mountain_effect_hand_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handRightPath + "/ME_mountain_effect_hand_right").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_mountain_effect_knee_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_mountain_effect_knee_right").gameObject.SetActive(true);
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_mountain_effect_head").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.spine2Path + "/ME_mountain_effect_spine2").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handLeftPath + "/ME_mountain_effect_hand_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handRightPath + "/ME_mountain_effect_hand_right").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_mountain_effect_knee_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_mountain_effect_knee_right").gameObject.SetActive(false);
                }

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(ModularMagic_Armors.Instance.effects.DarkWizardArmorSetSE.name)))
                {
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left/flames").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right/flames").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderLeftPath + "/ME_plains_effect_shoulder_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderRightPath + "/ME_plains_effect_shoulder_right").gameObject.SetActive(true);
                    // ModularMagic_Armors.Instance.prefabs.PlainsMageFootStepsPrefab.SetActive(true);
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left/flames").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right/flames").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderLeftPath + "/ME_plains_effect_shoulder_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderRightPath + "/ME_plains_effect_shoulder_right").gameObject.SetActive(false);
                    // ModularMagic_Armors.Instance.prefabs.PlainsMageFootStepsPrefab.SetActive(false);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not de/activate set effects: " + e);
            }
        }

        private static void _UpdateItemEffects(ItemDrop.ItemData item, bool enable)
        {
            try
            {
                if (Player.m_localPlayer && item != null)
                {
                    GameObject eyeLeft;
                    GameObject eyeRight;

                    switch (item.m_shared.m_name)
                    {
                        case var value when value == PluginConfig.armor2Helmet.name.Value:
                            eyeLeft = Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left").gameObject;
                            eyeRight = Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right").gameObject;
                            eyeLeft.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.WraithArmorEye;
                            eyeRight.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.WraithArmorEye;
                            eyeLeft.SetActive(enable);
                            eyeRight.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_swamp_effect_head").gameObject.SetActive(enable);
                            break;
                        case var value when value == PluginConfig.armor2Chest.name.Value:
                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.spine1Path + "/ME_swamp_effect_spine1").gameObject.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handLeftPath + "/ME_swamp_effect_hand_left").gameObject.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.handRightPath + "/ME_swamp_effect_hand_right").gameObject.SetActive(enable);
                            break;
                        case var value when value == PluginConfig.armor2Legs.name.Value:
                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeLeftPath + "/ME_swamp_effect_knee_left").gameObject.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.kneeRightPath + "/ME_swamp_effect_knee_right").gameObject.SetActive(enable);
                            break;
                        case var value when value == PluginConfig.armor4Helmet.name.Value:
                            eyeLeft = Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_left").gameObject;
                            eyeRight = Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.headPath + "/ME_eye_right").gameObject;
                            eyeLeft.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.DarkWizardArmorEye;
                            eyeRight.GetComponent<MeshRenderer>().material = ModularMagic_Armors.Instance.materials.DarkWizardArmorEye;
                            eyeLeft.SetActive(enable);
                            eyeRight.SetActive(enable);
                            break;
                        case var value when value != null && value == PluginConfig.armor6Helmet.name.Value:
                            if (!PluginConfig.adjustEmbla.Value)
                                return;

                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.helmetAttachPath + "/EmblaHood_Effects_MMA")?.gameObject.SetActive(enable);
                            break;
                        case var value when value != null && value == PluginConfig.armor6Chest.name.Value:
                            if (!PluginConfig.adjustEmbla.Value)
                                return;

                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderLeftPath + "/EmblaChest_Left_Effects_MMA")?.gameObject.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(ModularMagic_Armors.Instance.playerArmature.shoulderRightPath + "/EmblaChest_Right_Effects_MMA")?.gameObject.SetActive(enable);
                            break;
                        case "Audacious Tiara":
                            GameObject prefab = PrefabManager.Instance.GetPrefab("MMES_TheForestFlinger");
                            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
                            List<EffectList.EffectData> effectList = new List<EffectList.EffectData>();
                            EffectList.EffectData effectData = new EffectList.EffectData();
                            effectData.m_prefab = ModularMagic_Armors.Instance.prefabs.AudaciousSFX;
                            effectData.m_variant = -1;
                            effectData.m_enabled = true;
                            effectList.Add(effectData);
                            itemDrop.m_itemData.m_shared.m_startEffect.m_effectPrefabs = effectList.ToArray();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not de/activate item effects: " + e);
            }
        }

        private static void _SetEitr(Player player, string name, float amount, ref float eitr)
        {
            if (player == null)
                return;

            bool hasEffect = player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));

            if (!hasEffect)
                return;

            eitr += amount;
        }

        private static bool _HaveStatusEffect(Skills __instance, string statusEffect)
        {
            return __instance.m_player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(statusEffect));
        }

        private static float _GetValueFromConfig(Skills.SkillType skillType, ArmorConfig config)
        {
            if (skillType == Skills.SkillType.ElementalMagic)
                return config.elementalMagic.Value;
            else if (skillType == Skills.SkillType.BloodMagic)
                return config.bloodMagic.Value;
            else return 0f;
        }
    }
}
