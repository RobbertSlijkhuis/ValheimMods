//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using WizshBoneTwitchIntegration.Components;
//using WizshBoneTwitchIntegration.Configs;
//using WizshBoneTwitchIntegration.Helpers;
//using WizshBoneTwitchIntegration.Models;
//using WizshBoneTwitchIntegration.TwitchIntegration;
//using static EnemyHud;

//namespace WizshBoneTwitchIntegration.Harmony
//{
//    [HarmonyPatch]
//    public class PatchesWBTI
//    {
//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(Game), "Awake")]
//        public static void GameAwake_Postfix()
//        {
//            try
//            {
//                Game.instance.gameObject.AddComponent<TwitchChat>();
//                Game.instance.gameObject.AddComponent<TwitchCustomRewards>();
//                Game.instance.gameObject.AddComponent<TwitchAuth>();
//                Game.instance.gameObject.AddComponent<TwitchChatting>();
//                Game.instance.gameObject.AddComponent<TwitchCustomStatusEffect>();
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Could not add Twitch components in GameAwake_Postfix: " + e);
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(Menu), "OnLogoutYes")]
//        public static bool OnLogoutYes_Postfix()
//        {
//            try
//            {
//                Jotunn.Logger.LogWarning("=== OnLogoutYes ===");
//                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();

//                if (auth == null)
//                    return true;

//                if (auth.m_loggedIn)
//                {
//                    Jotunn.Logger.LogWarning("Logged in, clearing redeems");
//                    ExtraConfigHelper.WriteBannedUsersToFile(auth.m_customRewards.m_bannedUsers);
//                    auth.LogoutBackToMainMenu();
//                    return false;
//                }

//                Jotunn.Logger.LogWarning("NOT logged in, proceed as normal");

//                return true;
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Could not clear rewards on OnLogoutYes_Postfix: " + e);
//                return true;
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(Menu), "OnQuitYes")]
//        public static bool OnQuitYes_Prefix(ref Menu __instance)
//        {
//            try
//            {
//                if (__instance == null)
//                    return true;

//                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();

//                if (auth == null)
//                    return true;


//                if (auth.m_loggedIn)
//                {
//                    Jotunn.Logger.LogWarning("Clearing redeems on Quit!");
//                    // __instance.m_quitDialog.transform.gameObject.SetActive(false);
//                    ExtraConfigHelper.WriteBannedUsersToFile(auth.m_customRewards.m_bannedUsers);
//                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Clearing redeems, please wait!", 1000);
//                    auth.LogoutQuitApplication();
//                    return false;
//                }

//                return true;
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Could not clear rewards on OnQuitYes_Prefix: " + e);
//                return true;
//            }
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(ZoneSystem), "GlobalKeyAdd")]
//        public static void GlobalKeyAdd_Postfix(ref ZoneSystem __instance, string keyStr, bool canSaveToServerOptionKeys = true)
//        {
//            try
//            {
//                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
//                string loweredKey = keyStr.ToLower();

//                if (!auth || !auth.m_loggedIn || (!loweredKey.Contains("defeated_") && !loweredKey.Contains("killed")))
//                    return;

//                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

//                if (customRewards == null)
//                    return;

//                if (customRewards.m_enabled)
//                    customRewards.SetRewards();
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Could not update rewards on GlobalKeyAdd_Postfix: " + e);
//            }
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(Character), "GetHoverText")]
//        public static void GetHoverText_Postfix(ref Character __instance, ref string __result)
//        {
//            try
//            {
//                TwitchCreatureInteract creatureInteract = __instance.gameObject.GetComponent<TwitchCreatureInteract>();

//                if (creatureInteract != null)
//                {
//                    __result = creatureInteract.GetHoverText();
//                    return;
//                }
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in GetHoverText_Postfix: " + e);
//            }
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(TextInput), "RequestText")]
//        public static void RequestText_Postfix(ref TextInput __instance, TextReceiver sign, string topic, ref int charLimit)
//        {
//            try
//            {
//                if (sign.ToString().Contains("(Tameable)") && topic == "$hud_rename")
//                    __instance.m_inputField.characterLimit = 60;
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in RequestText_Postfix: " + e);
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(Tameable), "RPC_SetName")]
//        public static void RPC_SetName_Prefix(ref Tameable __instance, long sender, ref string name, string authorId)
//        {
//            try
//            { 
//                if (!name.Contains("claim:"))
//                    return;

//                TwitchCreatureClaim creatureClaim = __instance.gameObject.GetComponent<TwitchCreatureClaim>();

//                if (creatureClaim == null)
//                    creatureClaim = __instance.gameObject.AddComponent<TwitchCreatureClaim>();

//                name = name.Replace("claim:", "");
//                creatureClaim.Init(name);
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in RPC_SetName_Prefix: " + e);
//            }
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(Player), "OnSpawned")]
//        public static void OnSpawned_Postfix(ref Player __instance)
//        {
//            try
//            {
//                if (Player.m_localPlayer.GetPlayerID() != __instance.GetPlayerID())
//                {
//                    Jotunn.Logger.LogWarning($"Not local player! {Player.m_localPlayer.GetPlayerID()} - {__instance.GetPlayerID()}");
//                    return;
//                }

//                TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
//                customStatusEffect.ReApplyStatusEffects();
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in OnSpawned_Postfix: " + e);
//            }
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(StatusEffect), "Stop")]
//        public static void Stop_Postfix(StatusEffect __instance)
//        {
//            try
//            {
//                int nameHash = __instance.NameHash();
//                TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
//                StatusEffectData statusEffect = customStatusEffect.GetStatusEffects().Find(item => item.nameHash == nameHash);

//                if (statusEffect == null)
//                    return;

//                if (__instance.IsDone())
//                {
//                    Jotunn.Logger.LogWarning("Removing StatusEffect: " + statusEffect.name);
//                    customStatusEffect.RemoveStatusEffect(statusEffect, false);
//                }
//                else if (statusEffect.persistsThroughDeath)
//                {
//                    Jotunn.Logger.LogWarning("Setting StatusEffect remaining time: " + statusEffect.name + ", " + __instance.GetRemaningTime());
//                    statusEffect.durationRemaining = __instance.GetRemaningTime();
//                }
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in Stop_Postfix: " + e);
//            }
//        }

//        //[HarmonyPostfix]
//        //[HarmonyPatch(typeof(Player), "OnDamaged")]
//        //public static void OnDamaged_Postfix(Player __instance, HitData hit)
//        //{
//        //    try
//        //    {
//        //        if (__instance == null || hit == null)
//        //            return;

//        //        Jotunn.Logger.LogWarning("Player got hit!");
//        //        Jotunn.Logger.LogWarning($"type: {hit.m_hitType}");
//        //        Jotunn.Logger.LogWarning($"damage: {hit.m_damage}");
//        //        Jotunn.Logger.LogWarning($"attacker: {hit.m_attacker}");
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        Jotunn.Logger.LogError("Something went wrong in OnCollisionEnter_Postfix: " + e);
//        //    }
//        //}

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(WearNTear), "Damage")]
//        public static bool WearNTearDamage_Prefix(WearNTear __instance, HitData hit)
//        {
//            try
//            {
//                if (__instance == null || hit == null)
//                    return true;

//                Character character = hit.GetAttacker();

//                if (character == null)
//                    return true;

//                TwitchCreaturePersistentData persistentData = character.gameObject.GetComponent<TwitchCreaturePersistentData>();
//                Jotunn.Logger.LogWarning(!persistentData.m_allowDamageStructures);
//                return persistentData.m_allowDamageStructures;
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in Damage_Prefix: " + e);
//                return true;
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(Character), "Damage")]
//        public static void CharacterDamage_Prefix(Character __instance, ref HitData hit)
//        {
//            // Jotunn.Logger.LogWarning("Damage on: " + __instance.gameObject.name);
//            Character attacker = hit.GetAttacker();

//            if (attacker == null)
//                return;

//            Jotunn.Logger.LogWarning("Found attacker: " + attacker.gameObject.name);
//            Jotunn.Logger.LogWarning("Hit type: " + hit.GetType());

//            TwitchCreatureClaim creatureClaim = attacker.gameObject.GetComponent<TwitchCreatureClaim>();

//            //Jotunn.Logger.LogWarning($"Is null?: {(creatureClaim == null ? true : false)}");
//            //Jotunn.Logger.LogWarning($"Is spawn?: {creatureClaim?.m_isSpawn}");

//            if (creatureClaim == null || !creatureClaim.m_isSpawn)
//                return;

//            // Jotunn.Logger.LogWarning("Found creature claim!");

//            ValheimCreature creature = CreatureHelper.GetValheimCreature(attacker.gameObject.name);
//            float playerTier = ProgressionHelper.GetPlayerTier();
//            float scale = CreatureHelper.CalculateScale(playerTier, creature.tier, PluginConfig.configCreaturesdamageScale.Value);
//            CreatureHelper.ScaleHitDamage(ref hit, scale);
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(Aoe), "ShouldHit")]
//        public static void ShouldHit_Postfix(Aoe __instance, ref bool __result, Collider collider)
//        {
//            try
//            {
//                TwitchAllowDamage allowDamage = __instance.gameObject.GetComponent<TwitchAllowDamage>();

//                if (allowDamage == null)
//                    return;

//                GameObject gameObject = Projectile.FindHitObject(collider);

//                if (gameObject == null)
//                    return;
                
//                Ship ship = gameObject.GetComponent<Ship>();
//                WearNTear wearNTear = gameObject.GetComponent<WearNTear>();

//                if (ship != null && !allowDamage.m_allowDamageShips)
//                {
//                    Jotunn.Logger.LogWarning("Prevent AOE damage to ship!");
//                    __result = false;
//                }

//                else if (wearNTear != null && !allowDamage.m_allowDamageStructures)
//                {
//                    Jotunn.Logger.LogWarning("Prevent AOE damage to structures!");
//                    __result = false;
//                }
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in OnHit_Postfix: " + e);
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(ImpactEffect), "OnCollisionEnter")]
//        public static bool OnCollisionEnter_Prefix(ImpactEffect __instance, Collision info)
//        {
//            try
//            {
//                ContactPoint contactPoint = info.contacts[0];
//                GameObject gameObject = Projectile.FindHitObject(contactPoint.otherCollider);
//                Ship ship = null;
//                WearNTear wearNTear = null;
//                Piece piece = null;

//                if (gameObject == null)
//                    return true;

//                TwitchAllowDamage allowDamage = __instance.gameObject.GetComponent<TwitchAllowDamage>();

//                if (allowDamage != null)
//                {
//                    ship = gameObject.GetComponent<Ship>();
//                    wearNTear = gameObject.GetComponent<WearNTear>();
//                    piece = gameObject.GetComponent<Piece>();
//                }
//                else
//                {
//                    allowDamage = gameObject.gameObject.GetComponent<TwitchAllowDamage>();
//                    ship = __instance.GetComponent<Ship>();
//                    wearNTear = __instance.GetComponent<WearNTear>();
//                    piece = __instance.GetComponent<Piece>();
//                }

//                if (allowDamage == null || piece == null || piece.GetCreator() == 0)
//                    return true;

//                if (ship != null && !allowDamage.m_allowDamageShips)
//                {
//                    // Jotunn.Logger.LogWarning("Prevent IMPACT damage to ship! " + gameObject.name);
//                    return false;
//                }

//                else if (wearNTear != null && !allowDamage.m_allowDamageStructures)
//                {
//                    // Jotunn.Logger.LogWarning("Prevent IMPACT damage to structures! " + gameObject.name);
//                    return false;
//                }

//                return true;
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in OnCollisionEnter_Prefix: " + e);
//                return true;
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(TerrainOp), "Awake")]
//        public static bool TerrainOpAwake_Prefix(TerrainOp __instance)
//        {
//            try
//            {
//                if (__instance == null) 
//                    return true;

//                if (!__instance.name.Contains("WBTI"))
//                    return true;

//                Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, PluginConfig.configRaiseRadius.Value);

//                foreach (Collider collider in objects)
//                {
//                    if (collider.gameObject.GetComponentInChildren<TwitchSafeZone>())
//                    {
//                        UnityEngine.Object.Destroy(__instance.gameObject);
//                        return false;
//                    }
//                }

//                return true;
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in TerrainOpAwake_Prefix: " + e);
//                return true;
//            }
//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(Emote), "DoEmote")]
//        public static void DoEmote_Postfix(Emotes emote)
//        {
//            try
//            {
//                if (emote == Emotes.ComeHere)
//                {
//                    CreatureHelper.SetFollowInRadius(true);
//                }
//                else if (emote == Emotes.NoNoNo)
//                {
//                    CreatureHelper.SetFollowInRadius(false);
//                }
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in DoEmote_Postfix: " + e);
//            }
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
//        public static void UpdateHuds_Postfix(ref EnemyHud __instance, Player player, Sadle sadle, float dt)
//        {
//            try
//            {
//                RectTransform hudBase = __instance.transform.Find("HudRoot/HudBase") as RectTransform;
//                RectTransform level4Trans = hudBase.transform.Find("level_custom_4") as RectTransform;

//                if (level4Trans == null)
//                    EnemyHudHelper.GenerateLevels(hudBase);

//                foreach (KeyValuePair<Character, HudData> hud in __instance.m_huds)
//                {
//                    HudData value = hud.Value;

//                    if (value == null || value.m_level3 == null)
//                        continue;

//                    int level = value.m_character.GetLevel();

//                    RectTransform hudLevel2 = value.m_level3.parent.Find("level_custom_2") as RectTransform;
//                    RectTransform hudLevel3 = value.m_level3.parent.Find("level_custom_3") as RectTransform;
//                    RectTransform hudLevel4 = value.m_level3.parent.Find("level_custom_4") as RectTransform;
//                    RectTransform hudLevel5 = value.m_level3.parent.Find("level_custom_5") as RectTransform;
//                    RectTransform hudLevel6 = value.m_level3.parent.Find("level_custom_6") as RectTransform;
//                    RectTransform hudLevel7 = value.m_level3.parent.Find("level_custom_7") as RectTransform;
//                    RectTransform hudLevel8 = value.m_level3.parent.Find("level_custom_8") as RectTransform;
//                    RectTransform hudLevel9 = value.m_level3.parent.Find("level_custom_9") as RectTransform;
//                    RectTransform hudLevel10 = value.m_level3.parent.Find("level_custom_10") as RectTransform;

//                    if (hudLevel2 != null)
//                        hudLevel2.gameObject.SetActive(level >= 4);

//                    if (hudLevel3 != null)
//                        hudLevel3.gameObject.SetActive(level >= 4);

//                    if (hudLevel4 != null)
//                        hudLevel4.gameObject.SetActive(level >= 4);

//                    if (hudLevel5 != null)
//                        hudLevel5.gameObject.SetActive(level >= 5);

//                    if (hudLevel6 != null)
//                        hudLevel6.gameObject.SetActive(level >= 6);

//                    if (hudLevel7 != null)
//                        hudLevel7.gameObject.SetActive(level >= 7);

//                    if (hudLevel8 != null)
//                        hudLevel8.gameObject.SetActive(level >= 8);

//                    if (hudLevel9 != null)
//                        hudLevel9.gameObject.SetActive(level >= 9);

//                    if (hudLevel10 != null)
//                        hudLevel10.gameObject.SetActive(level == 10);
//                }
//            }
//            catch (Exception e)
//            {
//                Jotunn.Logger.LogError("Something went wrong in UpdateHuds_Postfix: " + e);
//                return;
//            }
//        }
//    }
//}
