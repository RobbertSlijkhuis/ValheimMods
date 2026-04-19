using HarmonyLib;
using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class CreatureHelper
    {
        public static int hallucinationCount = 0;
        public static readonly Dictionary<string, ValheimCreature> valheimCreatures = new()
        {
            // MEADOWS
            { ValheimCreatureType.Boar, new ValheimCreature(1.0f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Eikthyr, new ValheimCreature(1.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Greyling, new ValheimCreature(1.1f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Neck, new ValheimCreature(1.0f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },

            // BLACK FOREST
            { ValheimCreatureType.Bjorn, new ValheimCreature(2.8f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Ghost, new ValheimCreature(2.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Greydwarf, new ValheimCreature(2.0f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Greydwarf_Elite, new ValheimCreature(2.4f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Greydwarf_Shaman, new ValheimCreature(2.4f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Serpent, new ValheimCreature(2.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Skeleton, new ValheimCreature(2.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Skeleton_Poison, new ValheimCreature(2.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.TheElder, new ValheimCreature(2.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Troll, new ValheimCreature(2.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },

            // SWAMP
            { ValheimCreatureType.Abomination, new ValheimCreature(3.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Blob, new ValheimCreature(3.0f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.BlobElite, new ValheimCreature(3.0f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Bonemass, new ValheimCreature(3.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Draugr, new ValheimCreature(3.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Draugr_Elite, new ValheimCreature(3.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Draugr_Ranged, new ValheimCreature(3.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Leech, new ValheimCreature(3.0f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Surtling, new ValheimCreature(3.5f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Wraith, new ValheimCreature(3.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },

            // MOUNTAIN
            { ValheimCreatureType.Bat, new ValheimCreature(4.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Fenring, new ValheimCreature(4.3f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Fenring_Cultist, new ValheimCreature(4.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Hatchling, new ValheimCreature(4.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Moder, new ValheimCreature(4.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.StoneGolem, new ValheimCreature(4.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Ulv, new ValheimCreature(4.4f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Wolf, new ValheimCreature(4.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },

            // PLAINS
            { ValheimCreatureType.Deathsquito, new ValheimCreature(5.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Goblin, new ValheimCreature(5.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.GoblinBrute, new ValheimCreature(5.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.GoblinShaman, new ValheimCreature(5.4f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.BlobTar, new ValheimCreature(5.3f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Lox, new ValheimCreature(5.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Unbjorn, new ValheimCreature(5.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Yagluth, new ValheimCreature(5.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },

            // MISTLANDS
            { ValheimCreatureType.Dverger, new ValheimCreature(6.4f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.DvergerMage, new ValheimCreature(6.4f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Gjall, new ValheimCreature(6.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Queen, new ValheimCreature(6.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Seeker, new ValheimCreature(6.3f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.SeekerBrood, new ValheimCreature(6.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.SeekerBrute, new ValheimCreature(6.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Tick, new ValheimCreature(6.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },

            // ASHLANDS
            { ValheimCreatureType.Asksvin, new ValheimCreature(7.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.BlobLava, new ValheimCreature(7.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.BonemawSerpent, new ValheimCreature(7.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Charred_Archer, new ValheimCreature(7.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Charred_Melee, new ValheimCreature(7.2f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Charred_Mage, new ValheimCreature(7.4f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectSmall) },
            { ValheimCreatureType.Fader, new ValheimCreature(7.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.LordReto, new ValheimCreature(7.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Morgen, new ValheimCreature(7.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.FallenValkyrie, new ValheimCreature(7.6f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
            { ValheimCreatureType.Volture, new ValheimCreature(7.3f, WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium) },
        };

        public static ValheimCreature GetValheimCreature(string name)
        {
            return valheimCreatures.GetValueSafe(name.Replace("(Clone)", ""));
        }

        public static void SpawnCreature(CreatureData creatureData, Transform transform, CustomRewardEvent customRewardEvent, bool ignoreWard = false, float force = 0f)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(creatureData.prefabName);

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find prefab to spawn");
                return;
            }

            Vector3 spawnPosition = transform.position;
            Quaternion spawnRotation = transform.rotation;

            if (creatureData.position == SpawnPositionType.WorldPosition)
                spawnPosition = creatureData.positionOffset.ToVector();
            else
            {
                spawnPosition = TransformHelper.UpdateSpawnLocation(creatureData.position, transform, creatureData.positionOffset, creatureData.positionRadius);
                spawnRotation = TransformHelper.UpdateSpawnRotation(creatureData.position, spawnRotation);
            }

            GameObject creature = UnityEngine.Object.Instantiate(prefab, spawnPosition, spawnRotation);
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

            TwitchCreatureClaim creatureClaim = creature.AddComponent<TwitchCreatureClaim>();
            creatureClaim.Init(creatureData, customRewardEvent, ignoreWard);

            if (force > 0f)
            {
                Rigidbody rigidBody = creature.GetComponent<Rigidbody>();
                rigidBody.AddForce((transform.forward * force) + (transform.up * force), ForceMode.Acceleration);
            }

            monsterAI.StartCoroutine(monsterAI.WakeUpAfterDelay(1f));
            monsterAI.LookAt(transform.position);

            if (creatureData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, creatureData.announceMessage), 3000);

            ValheimCreature valheimCreature = GetValheimCreature(creature.name);
            valheimCreature.spawnEffects.Create(creature.transform.position, creature.transform.rotation);
        }

        public static void SpawnCreatures(CreatureData creatureData, Transform transform, CustomRewardEvent customRewardEvent, bool ignoreWard = false, float force = 0f)
        {                
            for (int i = 0; i < creatureData.amount; i++)
            {
                SpawnCreature(creatureData, transform, customRewardEvent, ignoreWard, force);
            }
        }

        public static CreatureData GetRandomCreatureData(List<CreatureData> creatureList)
        {
            int index = Random.Range(0, creatureList.Count);
            return creatureList[index];
        }

        public static void ScaleHitDamage(ref HitData hit, float scale)
        {
            //Jotunn.Logger.LogWarning(hit.GetTotalDamage());

            hit.m_damage.m_blunt *= scale;
            hit.m_damage.m_slash *= scale;
            hit.m_damage.m_pierce *= scale;

            hit.m_damage.m_fire *= scale;
            hit.m_damage.m_frost *= scale;
            hit.m_damage.m_lightning *= scale;
            hit.m_damage.m_poison *= scale;
            hit.m_damage.m_spirit *= scale;

            //Jotunn.Logger.LogWarning(hit.GetTotalDamage());
        }

        public static float CalculateScale(float playerTier, float creatureTier, float strength)
        {
            float delta = (playerTier - creatureTier) * PluginConfig.configCreaturesDeltaScale.Value;
            float scale = Mathf.Pow(1f + strength, delta);

            //Jotunn.Logger.LogWarning($"playerTier: {playerTier}");
            //Jotunn.Logger.LogWarning($"creatureTier: {creatureTier}");
            //Jotunn.Logger.LogWarning($"Strenght: {strength}");
            //Jotunn.Logger.LogWarning($"delta: {delta}");
            //Jotunn.Logger.LogWarning($"Scale: {scale}");

            //return Mathf.Clamp(scale, 0.50f, 1.60f);
            return scale;
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
            notAllowedList.Add("greydwarf_elite");
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
                    //Jotunn.Logger.LogWarning("Out of range!");
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
                //Jotunn.Logger.LogWarning($"Could not find prefab: {prefabName} ");
                return 0;
            }

            foreach (BaseAI creature in BaseAI.BaseAIInstances)
            {
                if (creature.GetComponent<TwitchCreatureClaim>() == null)
                    continue;

                //Jotunn.Logger.LogWarning($"{creature.name} - {prefabName}(Clone)");
                if (creature.name != prefabName + "(Clone)")
                    continue;

                if (maxRange > 0f && Vector3.Distance(Player.m_localPlayer.transform.position, creature.transform.position) > maxRange)
                {
                    //Jotunn.Logger.LogWarning("Out of range!");
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
            //Jotunn.Logger.LogWarning("Current biome: " + biome);
            bool isNight = EnvMan.IsNight();
            //Jotunn.Logger.LogWarning("Is night: " + isNight);

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
            CreatureData creature = new CreatureData();
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
