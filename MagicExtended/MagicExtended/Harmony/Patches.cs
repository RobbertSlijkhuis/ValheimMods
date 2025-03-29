using HarmonyLib;
using MagicExtended.Configs;
using System;
using UnityEngine;

namespace MagicExtended.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPatch(typeof(Attack), "Start")]
        [HarmonyPrefix]
        public static bool AttackStart_Prefix(Humanoid character, Attack __instance)
        {
            if (__instance == null || character == null)
                return true;

            if (__instance.m_drawStaminaDrain == 8900f)
            {
                RandomizeMushroom();
                return true;
            }

            return CheckForCooldown(character, __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            if (__instance == null)
                return;

            SetEitr(__instance, "SimpleEitrStatusEffect_DW", ConfigSpellbooks.simpleSpellbook.eitr.Value, ref eitr);
            SetEitr(__instance, "AdvancedEitrStatusEffect_DW", ConfigSpellbooks.advancedSpellbook.eitr.Value, ref eitr);
            SetEitr(__instance, "MasterEitrStatusEffect_DW", ConfigSpellbooks.masterSpellbook.eitr.Value, ref eitr);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ItemDrop.ItemData item)
        {
            updateItemEffects(item, true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static void UnequipItem_Postfix(ItemDrop.ItemData item)
        {
            updateItemEffects(item, false);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatusEffect), "RemoveStartEffects")]
        public static bool RemoveStartEffects_Prefix(StatusEffect __instance)
        {
            if (__instance == null) 
                return true;

            switch (__instance.name)
            {
                case "BlackForestMageArmorSet_DW":
                case "SwampMageArmorSet_DW":
                case "MountainMageArmorSet_DW":
                    return false;
                default:
                    return true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UpdateEquipmentStatusEffects")]
        public static void UpdateEquipmentStatusEffects_Postfix()
        {
            try
            {
                if (Player.m_localPlayer == null) return;

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode("BlackForestMageArmorSet_DW")))
                {
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_blackforest_effect_head").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_right").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.wristLeftPath + "/ME_blackforest_effect_wrist_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.wristRightPath + "/ME_blackforest_effect_wrist_right").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeLeftPath + "/ME_blackforest_effect_knee_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeRightPath + "/ME_blackforest_effect_knee_right").gameObject.SetActive(true);
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_blackforest_effect_head").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_blackforest_effect_antler_right").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.wristLeftPath + "/ME_blackforest_effect_wrist_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.wristRightPath + "/ME_blackforest_effect_wrist_right").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeLeftPath + "/ME_blackforest_effect_knee_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeRightPath + "/ME_blackforest_effect_knee_right").gameObject.SetActive(false);
                }

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode("SwampMageArmorSet_DW")))
                {
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.bodyPath).gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.helmetAttachPath).gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_swamp_effect_face").gameObject.SetActive(true);
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.bodyPath).gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.helmetAttachPath).gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_swamp_effect_face").gameObject.SetActive(false);
                }

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode("MountainMageArmorSet_DW")))
                {
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_mountain_effect_head").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.spine2Path + "/ME_mountain_effect_spine2").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.handLeftPath + "/ME_mountain_effect_hand_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.handRightPath + "/ME_mountain_effect_hand_right").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeLeftPath + "/ME_mountain_effect_knee_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeRightPath + "/ME_mountain_effect_knee_right").gameObject.SetActive(true);
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_mountain_effect_head").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.spine2Path + "/ME_mountain_effect_spine2").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.handLeftPath + "/ME_mountain_effect_hand_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.handRightPath + "/ME_mountain_effect_hand_right").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeLeftPath + "/ME_mountain_effect_knee_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeRightPath + "/ME_mountain_effect_knee_right").gameObject.SetActive(false);
                }

                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode("PlainsMageArmorSet_DW")))
                {
                    Jotunn.Logger.LogWarning("Found Plains set");
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_left/flames").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_right/flames").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.shoulderLeftPath + "/ME_plains_effect_shoulder_left").gameObject.SetActive(true);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.shoulderRightPath + "/ME_plains_effect_shoulder_right").gameObject.SetActive(true);
                    MagicExtended.Instance.prefabs.PlainsMageFootStepsPrefab.SetActive(true);
                }
                else
                {
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_left/flames").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_right/flames").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.shoulderLeftPath + "/ME_plains_effect_shoulder_left").gameObject.SetActive(false);
                    Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.shoulderRightPath + "/ME_plains_effect_shoulder_right").gameObject.SetActive(false);
                    MagicExtended.Instance.prefabs.PlainsMageFootStepsPrefab.SetActive(false);
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not de/activate set effects: "+ error);
            }
        }

        private static void updateItemEffects(ItemDrop.ItemData item, bool enable)
        {
            try
            {
                if (Player.m_localPlayer && item != null)
                {
                    GameObject eyeLeft;
                    GameObject eyeRight;
                    switch (item.m_shared.m_name)
                    {
                        case "Swamp Hood":
                            eyeLeft = Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_left").gameObject;
                            eyeRight = Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_right").gameObject;
                            eyeLeft.GetComponent<MeshRenderer>().material = MagicExtended.Instance.SwampMageArmor_eye_DW;
                            eyeRight.GetComponent<MeshRenderer>().material = MagicExtended.Instance.SwampMageArmor_eye_DW;
                            eyeLeft.SetActive(enable);
                            eyeRight.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_swamp_effect_head").gameObject.SetActive(enable);
                            break;
                        case "Swamp Chest":
                            Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.spine1Path + "/ME_swamp_effect_spine1").gameObject.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.handLeftPath + "/ME_swamp_effect_hand_left").gameObject.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.handRightPath + "/ME_swamp_effect_hand_right").gameObject.SetActive(enable);
                            break;
                        case "Swamp Legs":
                            Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeLeftPath + "/ME_swamp_effect_knee_left").gameObject.SetActive(enable);
                            Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.kneeRightPath + "/ME_swamp_effect_knee_right").gameObject.SetActive(enable);
                            break;
                        case "Plains Hood":
                            eyeLeft = Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_left").gameObject;
                            eyeRight = Player.m_localPlayer.gameObject.transform.Find(MagicExtended.Instance.playerArmature.headPath + "/ME_eye_right").gameObject;
                            eyeLeft.GetComponent<MeshRenderer>().material = MagicExtended.Instance.PlainsMageArmor_eye_DW;
                            eyeRight.GetComponent<MeshRenderer>().material = MagicExtended.Instance.PlainsMageArmor_eye_DW;
                            eyeLeft.SetActive(enable);
                            eyeRight.SetActive(enable);
                            break;
                    }
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not de/activate item effects: " + error);
            }
        }

        private static void SetEitr(Player player, string name, float amount, ref float eitr)
        {
            if (player == null)
                return;

            bool hasEffect = player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));

            if (!hasEffect)
                return;

            eitr += amount;
        }

        private static bool CheckForCooldown(Character character, Attack attack)
        {
            float configCooldownValue = 0f;
            float configEitrValue = 0f;
            string effectName = "";
            bool hasEffect = false;

            switch (attack.m_drawStaminaDrain)
            {
                case 8901f:
                    configCooldownValue = ConfigStaffs.staffEarth3.secondaryCooldown.Value;
                    configEitrValue = ConfigStaffs.staffEarth3.useEitrSecondary.Value;
                    effectName = "StaffEarth3Cooldown_DW";
                    hasEffect = character.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(effectName));
                    break;
            }

            if (effectName == "" || configCooldownValue == 0f)
                return true;

            if (!hasEffect)
            {
                if (!character.HaveEitr(configEitrValue))
                {
                    character.Message(MessageHud.MessageType.Center, "You do not have enough Eitr to perform this action!");
                    return true;
                }

                StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode(effectName));
                character.GetSEMan().AddStatusEffect(statusEffect);
                return true;
            }

            character.Message(MessageHud.MessageType.Center, "The staff is still recharging!");
            return false;
        }

        private static void RandomizeMushroom()
        {
            Transform mushroom = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Mushroom");
            Transform mushroomBlue = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/MushroomBlue");
            Transform mushroomYellow = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/MushroomYellow");
            Transform branch = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Branch");
            Transform dandelion = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Dandelion");
            Transform stone = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Stone");
            Transform flint = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Flint");
            Transform bush = MagicExtended.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/RaspberryBush");

            mushroom.gameObject.SetActive(false);
            mushroomBlue.gameObject.SetActive(false);
            mushroomYellow.gameObject.SetActive(false);
            branch.gameObject.SetActive(false);
            dandelion.gameObject.SetActive(false);
            stone.gameObject.SetActive(false);
            flint.gameObject.SetActive(false);
            bush.gameObject.SetActive(false);

            int index = UnityEngine.Random.Range(0, 8);

            switch (index)
            {
                case 0:
                    mushroom.gameObject.SetActive(true);
                    break;
                case 1:
                    mushroomBlue.gameObject.SetActive(true);
                    break;
                case 2:
                    mushroomYellow.gameObject.SetActive(true);
                    break;
                case 3:
                    branch.gameObject.SetActive(true);
                    break;
                case 4:
                    dandelion.gameObject.SetActive(true);
                    break;
                case 5:
                    stone.gameObject.SetActive(true);
                    break;
                case 6:
                    flint.gameObject.SetActive(true);
                    break;
                case 7:
                    bush.gameObject.SetActive(true);
                    break;
                default:
                    mushroom.gameObject.SetActive(true);
                    break;
            }
        }
    }
}
