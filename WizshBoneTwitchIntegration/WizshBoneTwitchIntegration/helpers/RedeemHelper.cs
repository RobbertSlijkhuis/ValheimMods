using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RedeemHelper
    {
        public static PlayerSnapshot playerSnapshot;

        public static List<SurpriseChestSpawnData> GetSurpriseChestSpawnDataByTitle(string value)
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (customRewards == null)
            {
                Jotunn.Logger.LogError("Could not find custom rewards component to retrieve surprise chest spawn data");
                return null;
            }

            RedeemEntry redeem = customRewards.m_redeems.list.Find(item => item.title == value);

            if (redeem == null)
            {
                Jotunn.Logger.LogError("Could not find redeem to retrieve surprise chest spawn data");
                return null;
            }

            return redeem.chestData.items;
        }

        public static void SpawnSupriseChest(GameObject prefab, SurpriseChestData chestData, CustomRewardEvent customRewardEvent)
        {
            Transform transform = Player.m_localPlayer.transform;
            GameObject chest = UnityEngine.Object.Instantiate(prefab, transform.position, transform.rotation);
            chest.transform.localPosition = TransformHelper.UpdateSpawnLocation(SpawnPositionType.InFrontOfPlayerHigh, chest.transform, new PositionOffsetData());
            chest.transform.localRotation = TransformHelper.UpdateSpawnRotation(SpawnPositionType.InFrontOfPlayerHigh, chest.transform);

            TwitchSurpriseChest surpriseChest = chest.GetComponent<TwitchSurpriseChest>();
            surpriseChest.Init(chestData, customRewardEvent);
        }

        public static void SetPlayerSpeed(float multiplier)
        {
            if (playerSnapshot == null)
                playerSnapshot = new PlayerSnapshot(Player.m_localPlayer);

            Player.m_localPlayer.m_jumpForce = playerSnapshot.jumpForce * multiplier;
            Player.m_localPlayer.m_jumpForceForward = playerSnapshot.jumpForceForward * multiplier;

            Player.m_localPlayer.m_crouchSpeed = playerSnapshot.crouchSpeed * multiplier;
            Player.m_localPlayer.m_runSpeed = playerSnapshot.runSpeed * multiplier;
            Player.m_localPlayer.m_speed = playerSnapshot.speed * multiplier;
            Player.m_localPlayer.m_walkSpeed = playerSnapshot.walkSpeed * multiplier;

            Player.m_localPlayer.m_swimDepth = playerSnapshot.swimDepth * multiplier;
            Player.m_localPlayer.m_swimSpeed = playerSnapshot.swimSpeed * multiplier;

            // Player.m_localPlayer.m_maxCarryWeight = playerSnapshot.maxCarryWeight * multiplier;
        }

        public static void ResetPlayerSpeed(Player player)
        {
            playerSnapshot.Apply(player);
        }

        private static void ClearDamage(GameObject attack)
        {
            ItemDrop itemDrop = attack.GetComponent<ItemDrop>();
            Aoe aoe = attack.GetComponent<Aoe>();

            if (itemDrop != null)
            {
                // HitData.DamageTypes damages = itemDrop.m_itemData.m_shared.m_damages;

                //damages.m_blunt = damages.m_blunt * 0.5f;
                //damages.m_chop = damages.m_chop * 0.5f;
                //damages.m_damage = damages.m_damage * 0.5f;
                //damages.m_fire = damages.m_fire * 0.5f;
                //damages.m_frost = damages.m_frost * 0.5f;
                //damages.m_lightning = damages.m_lightning * 0.5f;
                //damages.m_pickaxe = damages.m_pickaxe * 0.5f;
                //damages.m_pierce = damages.m_pierce * 0.5f;
                //damages.m_poison = damages.m_poison * 0.5f;
                //damages.m_slash = damages.m_slash * 0.5f;
                //damages.m_spirit = damages.m_spirit * 0.5f;

                itemDrop.m_itemData.m_shared.m_damages.m_blunt = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_chop = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_damage = 1f;
                itemDrop.m_itemData.m_shared.m_damages.m_fire = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_frost = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_lightning = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_pickaxe = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_pierce = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_poison = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_slash = 0f;
                itemDrop.m_itemData.m_shared.m_damages.m_spirit = 0f;

                // itemDrop.m_itemData.m_shared.m_damages = new HitData.DamageTypes();
            }

            if (aoe != null)
            {
                // HitData.DamageTypes damages = aoe.m_damage;

                //damages.m_blunt = damages.m_blunt * 0.5f;
                //damages.m_chop = damages.m_chop * 0.5f;
                //damages.m_damage = damages.m_damage * 0.5f;
                //damages.m_fire = damages.m_fire * 0.5f;
                //damages.m_frost = damages.m_frost * 0.5f;
                //damages.m_lightning = damages.m_lightning * 0.5f;
                //damages.m_pickaxe = damages.m_pickaxe * 0.5f;
                //damages.m_pierce = damages.m_pierce * 0.5f;
                //damages.m_poison = damages.m_poison * 0.5f;
                //damages.m_slash = damages.m_slash * 0.5f;
                //damages.m_spirit = damages.m_spirit * 0.5f;

                aoe.m_damage.m_blunt = 0f;
                aoe.m_damage.m_chop = 0f;
                aoe.m_damage.m_damage = 1f;
                aoe.m_damage.m_fire = 0f;
                aoe.m_damage.m_frost = 0f;
                aoe.m_damage.m_lightning = 0f;
                aoe.m_damage.m_pickaxe = 0f;
                aoe.m_damage.m_pierce = 0f;
                aoe.m_damage.m_poison = 0f;
                aoe.m_damage.m_slash = 0f;
                aoe.m_damage.m_spirit = 0f;

                // aoe.m_damage = new HitData.DamageTypes();
            }
        }

        private static void DeepSearchForAttacks(GameObject attack)
        {
            ItemDrop itemDrop = attack.GetComponent<ItemDrop>();

            if (itemDrop != null)
            {
                ClearDamage(attack);

                if (itemDrop.m_itemData.m_shared.m_spawnOnHit != null)
                {
                    GameObject spawnOnHit = UnityEngine.Object.Instantiate(itemDrop.m_itemData.m_shared.m_spawnOnHit);
                    ClearDamage(spawnOnHit);
                    itemDrop.m_itemData.m_shared.m_spawnOnHit = spawnOnHit;
                }

                if (itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain)
                {
                    GameObject spawnOnHitTerrain = UnityEngine.Object.Instantiate(itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain);
                    ClearDamage(spawnOnHitTerrain);
                    itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain = spawnOnHitTerrain;
                }

                if (itemDrop.m_itemData.m_shared.m_triggerEffect.m_effectPrefabs.Length > 0)
                {
                    List<EffectList.EffectData> effectList = new List<EffectList.EffectData>();

                    foreach (var effect in itemDrop.m_itemData.m_shared.m_triggerEffect.m_effectPrefabs)
                    {
                        EffectList.EffectData effectData = new EffectList.EffectData();
                        GameObject effectPrefab = UnityEngine.Object.Instantiate(effect.m_prefab);
                        ClearDamage(effectPrefab);
                        effectData.m_prefab = effectPrefab;
                        effectData.m_enabled = true;
                        effectData.m_variant = -1;
                        effectList.Add(effectData);
                    }

                    itemDrop.m_itemData.m_shared.m_triggerEffect.m_effectPrefabs = effectList.ToArray();
                }
            }
        }
    }
}
