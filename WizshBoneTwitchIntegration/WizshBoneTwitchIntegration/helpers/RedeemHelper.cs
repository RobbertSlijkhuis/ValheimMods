using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RedeemHelper
    {
        public static PlayerSnapshot playerSnapshot;
        public static List<GameObject> fishList = new List<GameObject>();
        public static int hallucinationCount = 0;

        public static Vector3 UpdateSpawnLocation(Transform transform, string type, PositionOffsetData positionOffset)
        {
            Vector3 position;
            Vector3 offset = positionOffset.ToVector();

            switch (type)
            {
                case nameof(SpawnPositionType.InFrontOfPlayer):
                    position = (transform.forward * (3f + offset.z)) + (transform.up * offset.y) + (transform.right * offset.x) + transform.position;
                    return position;
                case nameof(SpawnPositionType.InFrontOfPlayerHigh):
                    position = (transform.forward * (3f + offset.z)) + (transform.up * (3f + offset.y)) + (transform.right * offset.x) + transform.position;
                    return position;
                case nameof(SpawnPositionType.Flying):
                    position = (transform.forward * (10f + offset.z)) + (transform.up * (7f + offset.y)) + (transform.right * offset.x) + transform.position;
                    return position;
                case nameof(SpawnPositionType.RandomBehind):
                    position = (transform.forward * Random.Range(-30, -50f)) + (transform.right * Random.Range(-50, 50f)) + positionOffset.ToVector() + transform.position;

                    if (ZoneSystem.instance.FindFloor(position, out var height))
                        position.y = height;

                    return (transform.forward * offset.z) + (transform.up * offset.y) + (transform.right * offset.x) + position;
                default:
                    return (transform.forward * offset.z) + (transform.up * offset.y) + (transform.right * offset.x) + transform.position;
            }
        }

        public static Quaternion UpdateSpawnRotation(Transform transform, string type)
        {
            switch (type)
            {
                case nameof(SpawnPositionType.InFrontOfPlayer):
                case nameof(SpawnPositionType.InFrontOfPlayerHigh):
                case nameof(SpawnPositionType.Flying):
                    transform.Rotate(Vector3.up, 180f);
                    return transform.rotation;
                default:
                    return transform.rotation;
            }
        }

        public static void SpawnCreature(SpawnOptions options)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(options.creatureData.prefabName);

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find prefab to spawn");
            }

            Vector3 positionToSpawn = options.creatureData.position == SpawnPositionType.WorldPosition ? options.creatureData.positionOffset.ToVector() : options.transform.position;
            GameObject creature = UnityEngine.Object.Instantiate(prefab, positionToSpawn, options.transform.rotation);

            if (options.creatureData.position != SpawnPositionType.WorldPosition)
            {
                creature.transform.localPosition = UpdateSpawnLocation(creature.transform, options.creatureData.position, options.creatureData.positionOffset);
                creature.transform.localRotation = UpdateSpawnRotation(creature.transform, options.creatureData.position);
            }

            MonsterAI monsterAI = creature.GetComponent<MonsterAI>();
            Humanoid humanoid = creature.GetComponent<Humanoid>();

            if (Player.m_localPlayer.InInterior())
            {
                string dungeonType = EnvMan.instance.GetCurrentEnvironment().m_name;

                Jotunn.Logger.LogWarning("Dungeon: " + dungeonType);
                Jotunn.Logger.LogWarning("Name: " + creature.name);

                if (dungeonType == DungeonType.Queen && creature.name == "Gjall(Clone)")
                    creature.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }

            if (monsterAI == null)
            {
                GameObject.Destroy(creature);
                throw new System.Exception("No monster AI available for creature spawn!");
            }

            if (humanoid == null)
            {
                GameObject.Destroy(creature);
                throw new System.Exception("No humanoid available for creature spawn!");
            }

            if (!options.creatureData.isHallucination)
            {
                TwitchCreatureClaim creatureClaim = creature.AddComponent<TwitchCreatureClaim>();
                creatureClaim.Init(options);
            }
            else 
            {
                TwitchCreaturePersistentData persistentData = creature.GetComponent<TwitchCreaturePersistentData>();
                persistentData.SetData(humanoid.m_name, options.creatureData, options.ignoreWard);

                monsterAI.m_huntPlayer = true;
                monsterAI.m_enableHuntPlayer = true;
                monsterAI.SetHuntPlayer(true);
                humanoid.SetHealth(10f);
                humanoid.SetMaxHealth(10f);
            }

            monsterAI.LookAt(options.transform.position);
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
                    monsterList.Add("Deathsquito");

                    if (isNight)
                        monsterList.Add("Unbjorn");

                    monsterList.Add("Goblin");
                    monsterList.Add("Lox");
                    monsterList.Add("Deathsquito");

                    if (isNight)
                        monsterList.Add("Unbjorn");
                    break;
                case Heightmap.Biome.Mistlands:
                    monsterList.Add("Seeker");
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
            creature.rename = false;
            SpawnCreature(new SpawnOptions(creature, Player.m_localPlayer.transform, customReward));
        }

        public static void SpawnMist(SpawnMistData options)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab("MistArea");

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find mist prefab to spawn");
                throw new System.Exception("Could not find mist prefab to spawn");
            }

            GameObject mist = UnityEngine.Object.Instantiate(prefab, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
            Mister mister = mist.GetComponent<Mister>();
            TwitchMisterDestruction misterDestruction = mist.GetComponent<TwitchMisterDestruction>();
            misterDestruction.SetStarted(options.duration ?? 60);

            if (options.height != null)
                mister.m_height = (float)options.height;

            if (options.radius != null)
                mister.m_radius = (float)options.radius;
        }

        public static void SpawnSupriseChest(GameObject prefab, ChestData chestData)
        {
            Transform transform = Player.m_localPlayer.transform;
            GameObject chest = UnityEngine.Object.Instantiate(prefab, transform.position, transform.rotation);
            chest.transform.localPosition = UpdateSpawnLocation(chest.transform, SpawnPositionType.InFrontOfPlayerHigh, new PositionOffsetData());
            chest.transform.localRotation = UpdateSpawnRotation(chest.transform, SpawnPositionType.InFrontOfPlayerHigh);

            Minimap.instance.DiscoverLocation(chest.transform.localPosition, Minimap.PinType.Icon3, "Surprise Chest", false);

            TwitchSurpriseChest surpriseChest = chest.GetComponent<TwitchSurpriseChest>();
            surpriseChest.Init(chestData);
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
