using HarmonyLib;
using ModularMagic_Utilities.Components;
using ModularMagic_Utilities.Configs;
using System;
//using System;
//using ModularMagic_Utilities.Helpers;
//using System;
//using static ItemDrop;

namespace ModularMagic_Utilities.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            try
            {
                if (__instance == null)
                    return;

                _SetEitr(__instance, PluginConfig.spellbook1.magicStatusEffectName, PluginConfig.spellbook1.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.spellbook2.magicStatusEffectName, PluginConfig.spellbook2.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.spellbook3.magicStatusEffectName, PluginConfig.spellbook3.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.lantern1.magicStatusEffectName, PluginConfig.lantern1.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.lantern2.magicStatusEffectName, PluginConfig.lantern2.eitr.Value, ref eitr);
                _SetEitr(__instance, PluginConfig.lantern3.magicStatusEffectName, PluginConfig.lantern3.eitr.Value, ref eitr);
            }
            catch (Exception e) 
            {
                Jotunn.Logger.LogError("Could not update Eitr in GetTotalFoodValue_Postfix: " + e);
            }
        }

        [HarmonyPatch(typeof(Skills), "GetSkillLevel")]
        [HarmonyPostfix]
        public static void GetSkillLevel_Postfix(Skills __instance, Skills.SkillType skillType, ref float __result)
        {
            try
            {
                if (skillType != Skills.SkillType.ElementalMagic && skillType != Skills.SkillType.BloodMagic)
                    return;

                float value = 0f;

                if (_HaveStatusEffect(__instance, PluginConfig.spellbook1.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.spellbook1);
                else if (_HaveStatusEffect(__instance, PluginConfig.spellbook2.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.spellbook2);
                else if (_HaveStatusEffect(__instance, PluginConfig.spellbook3.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.spellbook3);
                else if (_HaveStatusEffect(__instance, PluginConfig.lantern1.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.lantern1);
                else if (_HaveStatusEffect(__instance, PluginConfig.lantern2.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.lantern2);
                else if (_HaveStatusEffect(__instance, PluginConfig.lantern3.magicStatusEffectName))
                    value = _GetValueFromConfig(skillType, PluginConfig.lantern3);

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
        [HarmonyPatch(typeof(PlayerController), "Awake")]
        public static void AwakePlayerController_Postfix(ref PlayerController __instance)
        {
            try
            {
                __instance.gameObject.AddComponent<LanternMMU>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add component in AwakePlayerController_Postfix: " + e);
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Game), "SpawnPlayer")]
        //public static void SpawnPlayer_Postfix()
        //{
        //    try
        //    {
        //        Jotunn.Logger.LogWarning($"Player is spawned!");
        //    }
        //    catch (Exception e)
        //    {
        //        Jotunn.Logger.LogError(e);
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Player), "OnSpawned")]
        //public static void OnSpawned_Postfix(ref Player __instance)
        //{
        //    try
        //    {
        //        if (__instance == null)
        //            return;

        //        if (!ModularMagic_Utilities.Instance.lanternStatusDict.ContainsKey(__instance.GetPlayerID()))
        //        {
        //            Jotunn.Logger.LogWarning($"Adding {__instance.GetPlayerName()} to lantern dictionary! {__instance.GetPlayerID()}");
        //            ModularMagic_Utilities.Instance.lanternStatusDict.Add(__instance.GetPlayerID(), true);
        //        }
        //        else
        //        {
        //            Jotunn.Logger.LogWarning($"{__instance.GetPlayerName()} is already in the to lantern dictionary!");
        //        }

        //        //Jotunn.Logger.LogWarning("Created new player: " + __instance.GetPlayerName());
        //        //var inventory = __instance.GetInventory();
        //        //var equipedItems = inventory.GetEquippedItems();

        //        //var lantern = equipedItems.Find(item => 
        //        //    item.m_shared.m_name == ConfigUtilities.lantern1.name.Value ||
        //        //    item.m_shared.m_name == ConfigUtilities.lantern2.name.Value || 
        //        //    item.m_shared.m_name == ConfigUtilities.lantern3.name.Value);

        //        //if (lantern != null)
        //        //{

        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        Jotunn.Logger.LogError(e);
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        //public static void UnequipItem_Postfix(ItemData item)
        //{
        //    try
        //    {
        //        if (item == null) 
        //            return;

        //        long playerId = Player.m_localPlayer.GetPlayerID();

        //        if (ModularMagic_Utilities.Instance.lanternStatusDictionary[playerId]) 
        //            return;

        //        ZPackage package = new ZPackage();
        //        int type = UpdateHelper.GetLanternType(item);

        //        if (type != 0)
        //        {
        //            package.Write($"{playerId},{type},true,false");
        //            ModularMagic_Utilities.Instance.lanternRPC.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), package);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Jotunn.Logger.LogError(e);
        //    }
        //}

        private static void _SetEitr(Player player, string name, float amount, ref float eitr)
        {
            if (player == null)
                return;

            bool hasEffect = player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));

            if (!hasEffect)
                return;

            eitr += amount;
        }

        private static bool _HaveStatusEffect(Skills __instance, string name)
        {
            return __instance.m_player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));
        }

        private static float _GetValueFromConfig(Skills.SkillType skillType, UtilitiesConfig config)
        {
            if (skillType == Skills.SkillType.ElementalMagic)
                return config.elementalMagic.Value;
            else if (skillType == Skills.SkillType.BloodMagic)
                return config.bloodMagic.Value;
            else return 0f;
        }
    }
}
