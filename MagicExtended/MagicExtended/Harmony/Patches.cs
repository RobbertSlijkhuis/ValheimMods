using HarmonyLib;
using MagicExtended.Configs;
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
            setEvilSmoke(item, true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static void UnequipItem_Postfix(ItemDrop.ItemData item)
        {
            setEvilSmoke(item, false);
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
            if (Player.m_localPlayer == null) return;

            if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode("SwampMageArmorSet_DW")))
            {
                Jotunn.Logger.LogWarning("Wraith mode");
                Player.m_localPlayer.gameObject.transform.Find("Visual/body").gameObject.SetActive(false);
                Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/Helmet_attach").gameObject.SetActive(false);
                Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/evil_smoke_face").gameObject.SetActive(true);
            }
            else
            {
                Jotunn.Logger.LogWarning("Normal mode");
                Player.m_localPlayer.gameObject.transform.Find("Visual/body").gameObject.SetActive(true);
                Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/Helmet_attach").gameObject.SetActive(true);
                Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/evil_smoke_face").gameObject.SetActive(false);
            }
        }

        private static void setEvilSmoke(ItemDrop.ItemData item, bool enable)
        {
            if (Player.m_localPlayer && item != null)
            {
                GameObject eyeLeft;
                GameObject eyeRight;
                GameObject evilSmoke;
                GameObject evilSmokeLeft;
                GameObject evilSmokeRight;
                switch (item.m_shared.m_name)
                {
                    case "Swamp Hood":
                        eyeLeft = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_left").gameObject;
                        eyeRight = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_right").gameObject;
                        evilSmoke = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/evil_smoke").gameObject;
                        eyeLeft.SetActive(enable);
                        eyeRight.SetActive(enable);
                        evilSmoke.SetActive(enable);
                        break;
                    case "Swamp Chest":
                        evilSmoke = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/evil_smoke").gameObject;
                        evilSmokeLeft = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHand_Attach/evil_smoke_left").gameObject;
                        evilSmokeRight = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/RightHand_Attach/evil_smoke_right").gameObject;
                        evilSmoke.SetActive(enable);
                        evilSmokeLeft.SetActive(enable);
                        evilSmokeRight.SetActive(enable);
                        break;
                    case "Swamp Legs":
                        evilSmokeLeft = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/LeftUpLeg/LeftLeg/evil_smoke_left").gameObject;
                        evilSmokeRight = Player.m_localPlayer.gameObject.transform.Find("Visual/Armature/Hips/RightUpLeg/RightLeg/evil_smoke_right").gameObject;
                        evilSmokeLeft.SetActive(enable);
                        evilSmokeRight.SetActive(enable);
                        break;
                }
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

            int index = Random.Range(0, 8);

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
