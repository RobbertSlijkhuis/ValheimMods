using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;
using static ItemDrop;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class CreatureHelper
    {
        public static int hallucinationCount = 0;

        public static void SpawnCreature(CreatureData creatureData, Transform transform, CustomRewardEvent customRewardEvent, bool ignoreWard = false, float force = 0f)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(creatureData.prefabName);

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find prefab to spawn");
                return;
            }

            Vector3 positionToSpawn = creatureData.position == SpawnPositionType.WorldPosition ? creatureData.positionOffset.ToVector() : transform.position;
            GameObject creature = UnityEngine.Object.Instantiate(prefab, positionToSpawn, transform.rotation);

            if (creatureData.position != SpawnPositionType.WorldPosition)
            {
                creature.transform.localPosition = TransformHelper.UpdateSpawnLocation(creatureData.position, creature.transform, creatureData.positionOffset);
                creature.transform.localRotation = TransformHelper.UpdateSpawnRotation(creatureData.position, creature.transform);
            }

            MonsterAI monsterAI = creature.GetComponent<MonsterAI>();
            Humanoid humanoid = creature.GetComponent<Humanoid>();

            //if (Player.m_localPlayer.InInterior())
            //{
            //    string dungeonType = EnvMan.instance.GetCurrentEnvironment().m_name;

            //    Jotunn.Logger.LogWarning("Dungeon: " + dungeonType);
            //    Jotunn.Logger.LogWarning("Name: " + creature.name);

            //    if (dungeonType == DungeonType.BurialChamber || dungeonType == DungeonType.SunkenCrypt) {

            //        if (creature.name == "Greydwarf_Elite(Clone)")
            //        {
            //            creature.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            //            Jotunn.Logger.LogWarning("Down sizing Greyfwarf Brute");
            //        }

            //        if (creature.name == "Greydwarf_Shaman(Clone)")
            //        {
            //            creature.transform.localScale = new Vector3(1f, 1f, 1f);
            //            Jotunn.Logger.LogWarning("Down sizing Greyfwarf Shaman");
            //        }
            //    }
            //}

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

            if (!creatureData.isHallucination)
            {
                TwitchCreatureClaim creatureClaim = creature.AddComponent<TwitchCreatureClaim>();
                creatureClaim.Init(creatureData, customRewardEvent, ignoreWard);

                if (force > 0f)
                {
                    Rigidbody rigidBody = creature.GetComponent<Rigidbody>();
                    rigidBody.AddForce((transform.forward * force) + (transform.up * force), ForceMode.Acceleration);
                }
            }
            else
            {
                TwitchCreaturePersistentData persistentData = creature.GetComponent<TwitchCreaturePersistentData>();
                persistentData.SetData(humanoid.m_name, creatureData, ignoreWard);

                monsterAI.m_huntPlayer = true;
                monsterAI.m_enableHuntPlayer = true;
                monsterAI.SetHuntPlayer(true);
                humanoid.SetHealth(10f);
                humanoid.SetMaxHealth(10f);
            }

            monsterAI.StartCoroutine(monsterAI.WakeUpAfterDelay(1f));
            monsterAI.LookAt(transform.position);

            if (creatureData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, creatureData.announceMessage), 3000);

            WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium.Create(creature.transform.position, creature.transform.rotation);
        }

        public static void SpawnCreatures(CreatureData creatureData, Transform transform, CustomRewardEvent customRewardEvent, bool ignoreWard = false, float force = 0f)
        {                
            for (int i = 0; i < creatureData.amount; i++)
            {
                Jotunn.Logger.LogWarning($"Spawning {creatureData.prefabName}...");
                SpawnCreature(creatureData, transform, customRewardEvent, ignoreWard, force);
            }
        }

        public static void SetFollowInRadius(bool value)
        {
            Collider[] found = Physics.OverlapSphere(Player.m_localPlayer.transform.position, PluginConfig.configCreaturesFollowRadius.Value, LayerMask.GetMask("character"));

            foreach (var item in found)
            {
                GameObject rootObject = item.transform.root.gameObject;
                MonsterAI monsterAI = rootObject.GetComponent<MonsterAI>();

                if (monsterAI != null)
                    monsterAI.SetFollowPlayer(value ? Player.m_localPlayer.gameObject : null);
            }
        }

        public static bool CancelRedeemCauseOfDungeon(List<CreatureData> creatures)
        {
            string dungeonType = EnvMan.instance.GetCurrentEnvironment().m_name;

            if (creatures == null)
                return false;

            List<string> notAllowedList = new List<string>();
            //notAllowedList.Add("greydwarf_elite");
            notAllowedList.Add("abomination");
            notAllowedList.Add("bat");
            notAllowedList.Add("bjorn");
            notAllowedList.Add("bonemawserpent");
            notAllowedList.Add("deathsquito");
            notAllowedList.Add("gjall");
            notAllowedList.Add("goblinbrute");
            notAllowedList.Add("golem");
            notAllowedList.Add("hatchling");
            notAllowedList.Add("lox");
            notAllowedList.Add("seekerbrute");
            notAllowedList.Add("serpent");
            notAllowedList.Add("troll");
            notAllowedList.Add("unbjorn");

            switch (dungeonType)
            {
                case nameof(DungeonType.FrostCave):
                case nameof(DungeonType.HowlingCavern):
                    notAllowedList.Remove("golem");
                    break;
                case nameof(DungeonType.InfestedMine):
                    notAllowedList.Remove("seekerbrute");
                    notAllowedList.Remove("golem");
                    break;
                case nameof(DungeonType.Queen):
                    notAllowedList.Remove("bat");
                    notAllowedList.Remove("deathsquito");
                    notAllowedList.Remove("gjall");
                    notAllowedList.Remove("golem");
                    notAllowedList.Remove("seekerbrute");
                    break;
            }

            foreach (CreatureData creature in creatures)
            {
                if (notAllowedList.Contains(creature.prefabName.ToLower()))
                    return true;
            }

            return false;
        }

        public static int GetNrOfTwitchInstances(float maxRange = 100f)
        {
            int num = 0;

            foreach (BaseAI creature in BaseAI.BaseAIInstances)
            {
                if (creature.GetComponent<TwitchCreatureClaim>() == null)
                    continue;

                if (maxRange > 0f && Vector3.Distance(Player.m_localPlayer.transform.position, creature.transform.position) > maxRange)
                {
                    Jotunn.Logger.LogWarning("Out of range!");
                    continue;
                }

                num++;
            }

            return num;
        }

        public static int GetNrOfSpecificTwitchInstances(string prefabName, float maxRange = 100f)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);
            int num = 0;

            if (prefab == null)
            {
                Jotunn.Logger.LogWarning($"Could not find prefab: {prefabName} ");
                return 0;
            }

            foreach (BaseAI creature in BaseAI.BaseAIInstances)
            {
                if (creature.GetComponent<TwitchCreatureClaim>() == null)
                    continue;

                Jotunn.Logger.LogWarning($"{creature.name} - {prefabName}(Clone)");
                if (creature.name != prefabName + "(Clone)")
                    continue;

                if (maxRange > 0f && Vector3.Distance(Player.m_localPlayer.transform.position, creature.transform.position) > maxRange)
                {
                    Jotunn.Logger.LogWarning("Out of range!");
                    continue;
                }

                num++;
            }

            return num;
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
            CreatureData creature = new CreatureData(monster);
            creature.isHallucination = true;
            creature.position = SpawnPositionType.RandomBehind;
            creature.rename = false;
            SpawnCreature(creature, Player.m_localPlayer.transform, customReward);
        }

        public static void StartHallucinations()
        {
            if (hallucinationCount > 6)
                Game.instance.CancelInvoke(nameof(StartHallucinations));
            else
                SpawnHallucination();
        }
    }
}
