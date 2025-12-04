using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RedeemHelper
    {
        public static PlayerSnapshot playerSnapshot;
        public static List<GameObject> fishList = new List<GameObject>();
        public static int hallucinationCount = 0;

        public static Vector3 GenerateSpawnLocation(Transform transform, string type)
        {
            Vector3 position;
            switch (type)
            {
                case nameof(SpawnPositionType.Flying):
                    position = (transform.forward * 10f) + (transform.up * 7f) + transform.position;
                    return position;
                case nameof(SpawnPositionType.RandomBehind):
                    position = (transform.forward * Random.Range(-30, -50f)) + (transform.right * Random.Range(-50, 50f)) + transform.position;

                    if (ZoneSystem.instance.FindFloor(position, out var height))
                        position.y = height;

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
            ZNetView netView = creature.GetComponent<ZNetView>();
            Humanoid humanoid = creature.GetComponent<Humanoid>();

            if (!options.creatureData.isHallucination) {
                TwitchCreatureClaim monsterClaim = creature.AddComponent<TwitchCreatureClaim>();
                monsterClaim.Init(options);
                humanoid.m_name = options.customReward.RedeemerName;
                humanoid.m_faction = Character.Faction.Boss;
                humanoid.SetLevel(options.creatureData.level);
                humanoid.m_level = options.creatureData.level;
            }
            else
            {
                MonsterAI monsterAI = creature.GetComponent<MonsterAI>();
                monsterAI.m_huntPlayer = true;
                monsterAI.m_enableHuntPlayer = true;
                monsterAI.SetHuntPlayer(true);
                humanoid.SetHealth(10f);
                humanoid.SetMaxHealth(10f);

                List<GameObject> defaultItemsList = new List<GameObject>();
                foreach (GameObject item in humanoid.m_defaultItems)
                {
                    GameObject attack = UnityEngine.Object.Instantiate(item);
                    DeepSearchForAttacks(attack);
                    defaultItemsList.Add(attack);
                }
                humanoid.m_defaultItems = defaultItemsList.ToArray();

                List<GameObject> randomWeaponList = new List<GameObject>();
                foreach (GameObject item in humanoid.m_randomWeapon)
                {
                    GameObject attack = UnityEngine.Object.Instantiate(item);
                    DeepSearchForAttacks(attack);
                    randomWeaponList.Add(attack);
                }
                humanoid.m_randomWeapon = randomWeaponList.ToArray();

                List<Humanoid.ItemSet> listOfItemSets = new List<Humanoid.ItemSet>();
                foreach (Humanoid.ItemSet itemSet in humanoid.m_randomSets)
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
                humanoid.m_randomSets = listOfItemSets.ToArray();
            }

            if (!options.creatureData.allowDrops)
            {
                CharacterDrop characterDrop = creature.GetComponent<CharacterDrop>();
                characterDrop.m_drops = new List<CharacterDrop.Drop>();
            }

            if (options.creatureData.talks)
            {
                NpcTalk npcTalk = creature.AddComponent<NpcTalk>();
                npcTalk.m_name = options.customReward.RedeemerName;
                npcTalk.m_maxRange = 30f;
                npcTalk.m_offset = 3f;
                npcTalk.m_hideDialogDelay = 10f;
                npcTalk.m_aggravated = new List<string>() {
                    $"{options.customReward.RedeemerName} told me you bad! You DIE now!",
                    $"{options.customReward.RedeemerName} send me here for food... AH food!",
                    $"Troll on duty, cuty Betu... AAAARRRRGGGH something!",
                    $"Its smashing time! Hehe-eh",
                };

                if (options.creatureData.talkMessage != null && options.creatureData.talkMessage.Trim() != "" && options.creatureData.talkMessage.Trim().Length > 2)
                    npcTalk.m_aggravated = new List<string>() { options.creatureData.talkMessage };

                npcTalk.OnBecameAggravated(BaseAI.AggravatedReason.Damage);
            }
        }

        public static void SpawnCreatures(SpawnOptions options)
        {
            for (int i = 0; i < options.creatureData.amount; i++)
            {
                SpawnCreature(options);
            }
        }

        public static void SpawnHallucination()
        {
            hallucinationCount++;

            if (hallucinationCount > 6)
                return;

            Jotunn.Logger.LogWarning("Activated hallucinations");
            Heightmap.Biome biome = Player.m_localPlayer.GetCurrentBiome();
            Jotunn.Logger.LogWarning("Current biome: " + biome);
            bool isNight = EnvMan.IsNight();
            Jotunn.Logger.LogWarning("Is night: " + isNight);

            List<string> monsterList = new List<string>();

            switch (biome)
            {
                case Heightmap.Biome.Meadows:
                    monsterList.Add("Neck");
                    monsterList.Add("Greyling");
                    monsterList.Add("Boar");
                    break;
                case Heightmap.Biome.BlackForest:
                    monsterList.Add("Greydwarf");
                    monsterList.Add("Bjorn");
                    monsterList.Add("Troll");
                    monsterList.Add("Greydwarf_Shaman");
                    monsterList.Add("Bjorn");
                    monsterList.Add("Troll");
                    monsterList.Add("Greydwarf_Elite");
                    monsterList.Add("Bjorn");
                    monsterList.Add("Skeleton");
                    break;
                case Heightmap.Biome.Swamp:
                    monsterList.Add("Draugr");

                    if (isNight)
                        monsterList.Add("Wraith");

                    monsterList.Add("Abomination");
                    monsterList.Add("BlobElite");
                    monsterList.Add("Blob");
                    monsterList.Add("Abomination");
                    monsterList.Add("Draugr_Elite");

                    if (isNight)
                        monsterList.Add("Wraith");
                    break;
                case Heightmap.Biome.Mountain:
                    monsterList.Add("Wolf");
                    monsterList.Add("Hatchling");
                    monsterList.Add("StoneGolem");
                    monsterList.Add("Wolf");

                    if (isNight)
                        monsterList.Add("Fenring");
                    break;
                case Heightmap.Biome.Plains:
                    // monsterList.Add("Deathsquito");

                    if (isNight)
                        monsterList.Add("Unbjorn");

                    monsterList.Add("Goblin");
                    monsterList.Add("Lox");
                    // monsterList.Add("Deathsquito");

                    if (isNight)
                        monsterList.Add("Unbjorn");

                    monsterList.Add("GoblinBrute");
                    break;
                case Heightmap.Biome.Mistlands:
                    monsterList.Add("Seeker");
                    monsterList.Add("Tick");
                    monsterList.Add("SeekerBrute");
                    monsterList.Add("Gjall");
                    break;
                case Heightmap.Biome.AshLands:
                    monsterList.Add("Charred_Melee");
                    monsterList.Add("FallenValkyrie");
                    monsterList.Add("Asksvin");
                    monsterList.Add("Charred_Archer");
                    monsterList.Add("BlobLava");
                    monsterList.Add("Morgen");
                    monsterList.Add("Charred_Twitcher");
                    monsterList.Add("Volture");
                    break;
            }

            int index = UnityEngine.Random.Range(0, monsterList.Count);
            string monster = monsterList[index];

            CustomRewardEvent customReward = new CustomRewardEvent();
            SpawnCreatureData creature = new SpawnCreatureData(monster);
            creature.isHallucination = true;
            creature.position = SpawnPositionType.RandomBehind;
            SpawnCreature(new SpawnOptions(creature, Player.m_localPlayer.transform, customReward));
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
