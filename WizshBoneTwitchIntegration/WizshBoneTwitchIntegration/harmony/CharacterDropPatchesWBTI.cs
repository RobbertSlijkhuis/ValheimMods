using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class CharacterDropPatchesWBTI
    {
        // CharacterDrop.m_drops isn't just item loot - some creatures (e.g. the Oozer's on-death
        // split into two Blobs) use it to spawn another creature as a core mechanic (see
        // TwitchCreaturePersistentData.ApplyAllowDrops). That path is completely separate from
        // SpawnAbility (plain UnityEngine.Object.Instantiate, no coroutine), so
        // SpawnAbilityPatchesWBTI never sees it - redirect it the same way here, only when the
        // dying creature is itself a Twitch spawn, so its drop-spawned children also inherit its
        // redeem-configured CreatureData (see CreatureHelper.ApplyInheritedScaling).
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
        public static bool OnDeath_Prefix(CharacterDrop __instance)
        {
            try
            {
                Character character = __instance.GetComponent<Character>();

                if (character == null)
                    return true;

                TwitchCreatureClaim ownerClaim = character.gameObject.GetComponent<TwitchCreatureClaim>();

                if (ownerClaim == null || !ownerClaim.m_isSpawn)
                    return true;

                if (!__instance.m_dropsEnabled)
                    return false;

                List<KeyValuePair<GameObject, int>> drops = __instance.GenerateDropList();
                Vector3 centerPos = character.GetCenterPoint() + __instance.transform.TransformVector(__instance.m_spawnOffset);

                DropItemsWithInheritance(drops, centerPos, 0.5f, character);
                return false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in OnDeath_Prefix: " + e);
                return true;
            }
        }

        // Line-for-line port of vanilla CharacterDrop.DropItems, with a call to
        // CreatureHelper.ApplyInheritedScaling right after each Instantiate - mirrors
        // SpawnAbilityExtension.Spawn2(Character) for the same reason (see CreatureHelper.ApplyInheritedScaling).
        private static void DropItemsWithInheritance(List<KeyValuePair<GameObject, int>> drops, Vector3 centerPos, float dropArea, Character owner)
        {
            bool hasParentData = CreatureHelper.TryResolveParentCreatureData(owner, out CreatureData parentCreatureData, out string redeemerName, out string redeemTitle, out string lookupPrefabName);

            foreach (KeyValuePair<GameObject, int> drop in drops)
            {
                for (int i = 0; i < drop.Value; i++)
                {
                    Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f);
                    Vector3 offset = UnityEngine.Random.insideUnitSphere * dropArea;
                    GameObject gameObject = ZNetViewHelper.Instantiate(drop.Key, centerPos + offset, rotation);

                    ItemDrop itemDrop = gameObject.GetComponent<ItemDrop>();
                    if ((object)itemDrop != null)
                        itemDrop.m_itemData.m_worldLevel = (byte)Game.m_worldLevel;

                    if (hasParentData)
                        CreatureHelper.ApplyInheritedScaling(gameObject, drop.Key, parentCreatureData, redeemerName, redeemTitle, lookupPrefabName);

                    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
                    if (rigidbody != null)
                    {
                        Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
                        if (insideUnitSphere.y < 0f)
                            insideUnitSphere.y = -insideUnitSphere.y;

                        rigidbody.AddForce(insideUnitSphere * 5f, ForceMode.VelocityChange);
                    }
                }
            }
        }
    }
}
