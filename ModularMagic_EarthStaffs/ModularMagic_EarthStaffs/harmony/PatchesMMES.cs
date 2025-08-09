using HarmonyLib;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Helpers;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Harmony
{
    [HarmonyPatch]
    public class PatchesMMES
    {
        [HarmonyPatch(typeof(Player), "RaiseSkill")]
        [HarmonyPrefix]
        public static void RaiseSkill_Postfix(Player __instance, Skills.SkillType skill, float value = 1f)
        {
            try
            {
                if (__instance == null || skill != Skills.SkillType.ElementalMagic)
                    return;

                ItemData itemData = __instance.GetCurrentWeapon();

                if (itemData == null || (
                    itemData.m_dropPrefab.name != ModularMagic_EarthStaffs.Instance.prefabs.staffEarth0.name && 
                    itemData.m_dropPrefab.name != ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1.name && 
                    itemData.m_dropPrefab.name != ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2.name &&
                    itemData.m_dropPrefab.name != ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3.name
                ))
                    return;

                __instance.RaiseSkill(ModularMagic_EarthStaffs.customSkill, itemData.m_shared.m_attack.m_raiseSkillAmount);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in RaiseSkill_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ref Humanoid __instance, ItemData item)
        {
            try
            {
                if (__instance == null || !__instance.IsPlayer() || item == null)
                    return;

                string imbuementsString = item.m_customData.GetValueSafe(ModularMagic_EarthStaffs.imbuementDataKey);

                if (imbuementsString == null)
                    return;

                ImbuementHelper.ApplyImbuements(item, imbuementsString);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }

        [HarmonyPatch(typeof(Attack), "Start")]
        [HarmonyPrefix]
        public static bool AttackStart_Prefix(Attack __instance, Humanoid character)
        {
            try
            {
                if (__instance == null || character == null)
                    return true;

                ItemData weapon = character.GetCurrentWeapon();

                if (weapon == null || weapon.m_dropPrefab == null)
                    return true;

                if (weapon.m_dropPrefab.name == ModularMagic_EarthStaffs.Instance.prefabs.staffEarth0.name)
                {
                    RandomizeMushroom();
                    return true;
                }

                if (__instance.m_drawStaminaDrain != 8901)
                    return true;

                return CheckForCooldown(character, weapon.m_dropPrefab.name);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in AttackStart_Prefix: " + e);
                return true;
            }
        }

        private static bool CheckForCooldown(Character character, string name)
        {
            float cooldownValue = 0f;
            float eitrValue = 0f;
            bool hasEffect = false;
            StatusEffect statusEffect = null;

            switch (name)
            {
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2.name:
                    cooldownValue = PluginConfig.staffEarth2.secondaryAttackConfig.cooldown.Value;
                    eitrValue = PluginConfig.staffEarth2.secondaryAttackConfig.useEitr.Value;
                    hasEffect = character.GetSEMan().HaveStatusEffect(PluginConfig.staffEarth2.secondaryAttackConfig.cooldownStatusEffectName.GetStableHashCode());
                    statusEffect = ModularMagic_EarthStaffs.Instance.effects.Staff2Cooldown;
                    break;
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3.name:
                    cooldownValue = PluginConfig.staffEarth3.secondaryAttackConfig.cooldown.Value;
                    eitrValue = PluginConfig.staffEarth3.secondaryAttackConfig.useEitr.Value;
                    hasEffect = character.GetSEMan().HaveStatusEffect(PluginConfig.staffEarth3.secondaryAttackConfig.cooldownStatusEffectName.GetStableHashCode());
                    statusEffect = ModularMagic_EarthStaffs.Instance.effects.Staff3Cooldown;
                    break;
            }

            if (statusEffect == null || cooldownValue == 0f)
                return true;

            if (!hasEffect)
            {
                if (!character.HaveEitr(eitrValue))
                {
                    character.Message(MessageHud.MessageType.Center, "You do not have enough Eitr to perform this action!");
                    return true;
                }

                character.GetSEMan().AddStatusEffect(statusEffect);
                return true;
            }

            character.Message(MessageHud.MessageType.Center, "The staff is still recharging!");
            return false;
        }

        private static void RandomizeMushroom()
        {
            Transform mushroom = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/Mushroom");
            Transform mushroomBlue = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/MushroomBlue");
            Transform mushroomYellow = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/MushroomYellow");
            Transform branch = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/Branch");
            Transform dandelion = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/Dandelion");
            Transform stone = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/Stone");
            Transform flint = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/Flint");
            Transform bush = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroom.transform.Find("visual/RaspberryBush");

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
