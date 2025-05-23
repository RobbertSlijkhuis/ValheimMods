using HarmonyLib;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Linq;
using static ItemDrop;

namespace ModularMagic_Utilities.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            if (__instance == null)
                return;

            SetEitr(__instance, ConfigUtilities.spellbook1.magicStatusEffectName, ConfigUtilities.spellbook1.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.spellbook2.magicStatusEffectName, ConfigUtilities.spellbook2.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.spellbook3.magicStatusEffectName, ConfigUtilities.spellbook3.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.lantern1.magicStatusEffectName, ConfigUtilities.lantern1.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.lantern2.magicStatusEffectName, ConfigUtilities.lantern2.eitr.Value, ref eitr);
            SetEitr(__instance, ConfigUtilities.lantern3.magicStatusEffectName, ConfigUtilities.lantern3.eitr.Value, ref eitr);
        }

        [HarmonyPatch(typeof(Skills), "GetSkillLevel")]
        [HarmonyPostfix]
        public static void GetSkillLevel_Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
        {
            if (skillType != Skills.SkillType.ElementalMagic && skillType != Skills.SkillType.BloodMagic)
                return;

            float value = 0f;
            float currentLevel = __instance.GetSkill(skillType).m_level;

            if (HaveStatusEffect(__instance, ConfigUtilities.spellbook1.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.spellbook1);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.spellbook2.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.spellbook2);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.spellbook3.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.spellbook3);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.lantern1.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.lantern1);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.lantern2.magicStatusEffectName))
            {
               value = GetValueFromConfig(skillType, ConfigUtilities.lantern2);
            }
            else if (HaveStatusEffect(__instance, ConfigUtilities.lantern3.magicStatusEffectName))
            {
                value = GetValueFromConfig(skillType, ConfigUtilities.lantern3);
            }

            if (value == 0f)
            {
                __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref currentLevel);
                __result = currentLevel;
                return;
            }

            float newLevel = currentLevel + value;
            __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref newLevel);
            __result = newLevel;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "SpawnPlayer")]
        public static void SpawnPlayer_Postfix()
        {
            try
            {
                Jotunn.Logger.LogWarning($"Player is spawned!");
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError(error);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(ref Player __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                if (!ModularMagic_Utilities.Instance.lanternStatusDictionary.ContainsKey(__instance.GetPlayerID()))
                {
                    Jotunn.Logger.LogWarning($"Adding {__instance.GetPlayerName()} to lantern dictionary! {__instance.GetPlayerID()}");
                    ModularMagic_Utilities.Instance.lanternStatusDictionary.Add(__instance.GetPlayerID(), true);
                }
                else
                {
                    Jotunn.Logger.LogWarning($"{__instance.GetPlayerName()} is already in the to lantern dictionary!");
                }

                //Jotunn.Logger.LogWarning("Created new player: " + __instance.GetPlayerName());
                //var inventory = __instance.GetInventory();
                //var equipedItems = inventory.GetEquippedItems();

                //var lantern = equipedItems.Find(item => 
                //    item.m_shared.m_name == ConfigUtilities.lantern1.name.Value ||
                //    item.m_shared.m_name == ConfigUtilities.lantern2.name.Value || 
                //    item.m_shared.m_name == ConfigUtilities.lantern3.name.Value);

                //if (lantern != null)
                //{

                //}
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError(error);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static void UnequipItem_Postfix(ItemData item)
        {
            try
            {
                if (item == null) 
                    return;

                long playerId = Player.m_localPlayer.GetPlayerID();

                if (ModularMagic_Utilities.Instance.lanternStatusDictionary[playerId]) 
                    return;

                ZPackage package = new ZPackage();
                int type = RPCHelper.GetLanternType(item);

                if (type != 0)
                {
                    package.Write($"{playerId},{type},true,false");
                    ModularMagic_Utilities.Instance.lanternRPC.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), package);
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError(error);
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

        private static bool HaveStatusEffect(Skills __instance, string statusEffect)
        {
            return __instance.m_player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(statusEffect));
        }

        private static float GetValueFromConfig(Skills.SkillType skillType, UtilitiesConfig config)
        {
            if (skillType == Skills.SkillType.ElementalMagic)
                return config.elementalMagic.Value;
            else if (skillType == Skills.SkillType.BloodMagic)
                return config.bloodMagic.Value;
            else return 0f;
        }
    }
}
