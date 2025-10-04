using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RedeemHelper
    {
        public static PlayerSnapshot playerSnapshot;
        public static List<GameObject> fishList = new List<GameObject>();

        public static Vector3 GenerateSpawnLocation(Transform transform, string type)
        {
            Vector3 position;
            switch (type)
            {
                case nameof(SpawnPositionType.Flying):
                    position = (transform.forward * 10f) + (transform.up * 7f) + transform.position;
                    return position;
                case nameof(SpawnPositionType.Random):
                    position = (transform.forward * Random.Range(-30, -50f)) + (transform.right * Random.Range(-50, 50f)) + transform.position;

                    if (ZoneSystem.instance.FindFloor(position, out var height))
                    {
                        position.y = height;
                    }

                    return position;
                default:
                    return transform.position;
            }
        }

        public static void SpawnCreature(SpawnOptions options)
        {
            Transform transform = options.transform;
            GameObject prefab = PrefabManager.Instance.GetPrefab(options.creatureData.prefabName);

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find prefab to spawn");
            }

            GameObject creature = UnityEngine.Object.Instantiate(prefab, GenerateSpawnLocation(transform, options.creatureData.position), transform.rotation);
            Humanoid humanComp = creature.GetComponent<Humanoid>();

            if (!options.creatureData.isHallucination) {
                humanComp.m_name = options.customReward.RedeemerName;
                humanComp.m_faction = Character.Faction.Boss;
                humanComp.m_level = options.creatureData.level; 
            }
            else
            {
                MonsterAI monster = creature.GetComponent<MonsterAI>();
                monster.m_huntPlayer = true;
                monster.m_enableHuntPlayer = true;
                monster.SetHuntPlayer(true);

                List<GameObject> defaultItemsList = new List<GameObject>();
                foreach (GameObject item in humanComp.m_defaultItems)
                {
                    GameObject attack = UnityEngine.Object.Instantiate(item);
                    DeepSearchForAttacks(attack);
                    defaultItemsList.Add(attack);
                }
                humanComp.m_defaultItems = defaultItemsList.ToArray();

                List<GameObject> randomWeaponList = new List<GameObject>();
                foreach (GameObject item in humanComp.m_randomWeapon)
                {
                    GameObject attack = UnityEngine.Object.Instantiate(item);
                    DeepSearchForAttacks(attack);
                    randomWeaponList.Add(attack);
                }
                humanComp.m_randomWeapon = randomWeaponList.ToArray();

                List<Humanoid.ItemSet> listOfItemSets = new List<Humanoid.ItemSet>();
                foreach (Humanoid.ItemSet itemSet in humanComp.m_randomSets)
                {
                    List<GameObject> itemSetList = new List<GameObject>();
                    foreach (GameObject item in itemSet.m_items)
                    {
                        GameObject attack = UnityEngine.Object.Instantiate(item);
                        DeepSearchForAttacks(attack);
                        itemSetList.Add(attack);
                    }
                    itemSet.m_items = itemSetList.ToArray();
                    listOfItemSets.Add(itemSet);
                }
                humanComp.m_randomSets = listOfItemSets.ToArray();
            }

            if (!options.creatureData.allowDrops)
            {
                CharacterDrop dropComp = creature.GetComponent<CharacterDrop>();
                dropComp.m_drops = new List<CharacterDrop.Drop>();
            }

            if (options.creatureData.talks)
            {
                NpcTalk talkComp = creature.AddComponent<NpcTalk>();
                talkComp.m_name = options.customReward.RedeemerName;
                talkComp.m_maxRange = 20f;
                talkComp.m_offset = 3f;
                talkComp.m_hideDialogDelay = 10f;
                talkComp.m_aggravated = new List<string>() {
                    $"{options.customReward.RedeemerName} told me you bad! You DIE now!",
                    $"{options.customReward.RedeemerName} send me here for food... AH food!",
                    $"Troll on duty, cuty Betu... AAAARRRRGGGH something!",
                    $"Its smashing time! Hehe-eh",
                };

                if (options.creatureData.talkMessage != null && options.creatureData.talkMessage.Trim() != "" && options.creatureData.talkMessage.Trim().Length > 2)
                    talkComp.m_aggravated = new List<string>() { options.creatureData.talkMessage };

                talkComp.OnBecameAggravated(BaseAI.AggravatedReason.Damage);
            }
        }

        public static void SpawnCreatures(SpawnOptions options)
        {
            for (int i = 0; i < options.creatureData.count; i++)
            {
                SpawnCreature(options);
            }
        }

        public static void DetonateFish()
        {
            Collider[] found = Physics.OverlapSphere(Player.m_localPlayer.transform.position, 20f);

            foreach (var item in found)
            {
                GameObject rootObject = item.transform.root.gameObject;
                ItemDrop itemDrop = rootObject.GetComponent<ItemDrop>();

                if (itemDrop != null && itemDrop.m_itemData.m_shared.m_itemType.ToString() == "Fish")
                    fishList.Add(rootObject);
            }

            Jotunn.Logger.LogWarning("Fish in list: " + fishList.Count);
            GameObject explosion = PrefabManager.Instance.GetPrefab("BlobLava_explosion");

            foreach (GameObject fish in fishList)
            {
                UnityEngine.Object.Instantiate(explosion, fish.transform);
            }
        }

        public static int GetRandomStatusEffect()
        {
            List<int> hashList = new List<int>();
            hashList.Add(1458612846); // BarleyWine
            hashList.Add(WizshBoneTwitchIntegration.Instance.effects.Burning.NameHash()); // Burning
            hashList.Add(-1768438774); // FrostResist
            hashList.Add(WizshBoneTwitchIntegration.Instance.effects.Freezing.NameHash()); // Freezing
            hashList.Add(-568360536); // PoisonResist
            hashList.Add(WizshBoneTwitchIntegration.Instance.effects.Poison.NameHash()); // Poison
            hashList.Add(-404287610); // Ratatosk
            hashList.Add(-629027225); // Puke
            hashList.Add(-1325774533); // Bzerker
            hashList.Add(-1273337594); // Wet
            hashList.Add(2062111878); // Lightfoot
            hashList.Add(-1779147092); // Tared
            // hashList.Add(-291236605); // Tasty
            hashList.Add(-2079273775); // Rested
            hashList.Add(-1779147092); // Tared

            int index = Random.Range(0, hashList.Count);
            int hash = hashList[index];
            Jotunn.Logger.LogWarning("Random: " + index);
            Jotunn.Logger.LogWarning("Hash: " + hash);
            return hash;
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
                itemDrop.m_itemData.m_shared.m_damages = new HitData.DamageTypes();
            }

            if (aoe != null)
            {
                aoe.m_damage = new HitData.DamageTypes();
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
