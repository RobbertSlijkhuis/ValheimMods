using HarmonyLib;
using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class CreatureHelper
    {
        public static int hallucinationCount = 0;
        private static Dictionary<string, ValheimCreature> _valheimCreatures;
        public static Dictionary<string, ValheimCreature> valheimCreatures
        {
            get
            {
                if (_valheimCreatures == null)
                    _valheimCreatures = BuildValheimCreatures();
                return _valheimCreatures;
            }
        }

        private static Dictionary<string, ValheimCreature> BuildValheimCreatures()
        {
            var e = WizshBoneTwitchIntegration.Instance.effectLists;
            return new Dictionary<string, ValheimCreature>
            {
                // MEADOWS
                { ValheimCreatureType.Boar, new ValheimCreature(1.0f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Eikthyr, new ValheimCreature(1.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Greyling, new ValheimCreature(1.1f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Neck, new ValheimCreature(1.0f, e.SpawnEffectSmall) },

                // BLACK FOREST
                { ValheimCreatureType.Bjorn, new ValheimCreature(2.8f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Ghost, new ValheimCreature(2.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Greydwarf, new ValheimCreature(2.0f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Greydwarf_Elite, new ValheimCreature(2.4f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Greydwarf_Shaman, new ValheimCreature(2.4f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Serpent, new ValheimCreature(2.2f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Skeleton, new ValheimCreature(2.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Skeleton_Poison, new ValheimCreature(2.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.TheElder, new ValheimCreature(2.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Troll, new ValheimCreature(2.6f, e.SpawnEffectMedium) },

                // SWAMP
                { ValheimCreatureType.Abomination, new ValheimCreature(3.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Blob, new ValheimCreature(3.0f, e.SpawnEffectSmall) },
                { ValheimCreatureType.BlobElite, new ValheimCreature(3.0f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Bonemass, new ValheimCreature(3.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Draugr, new ValheimCreature(3.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Draugr_Elite, new ValheimCreature(3.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Draugr_Ranged, new ValheimCreature(3.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Leech, new ValheimCreature(3.0f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Surtling, new ValheimCreature(3.5f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Wraith, new ValheimCreature(3.6f, e.SpawnEffectSmall) },

                // MOUNTAIN
                { ValheimCreatureType.Bat, new ValheimCreature(4.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Fenring, new ValheimCreature(4.3f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Fenring_Cultist, new ValheimCreature(4.6f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Hatchling, new ValheimCreature(4.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Moder, new ValheimCreature(4.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.StoneGolem, new ValheimCreature(4.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Ulv, new ValheimCreature(4.4f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Wolf, new ValheimCreature(4.2f, e.SpawnEffectSmall) },

                // PLAINS
                { ValheimCreatureType.Deathsquito, new ValheimCreature(5.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Goblin, new ValheimCreature(5.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.GoblinBrute, new ValheimCreature(5.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.GoblinShaman, new ValheimCreature(5.4f, e.SpawnEffectSmall) },
                { ValheimCreatureType.BlobTar, new ValheimCreature(5.3f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Lox, new ValheimCreature(5.6f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Unbjorn, new ValheimCreature(5.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Yagluth, new ValheimCreature(5.6f, e.SpawnEffectMedium) },

                // MISTLANDS
                { ValheimCreatureType.Dverger, new ValheimCreature(6.4f, e.SpawnEffectSmall) },
                { ValheimCreatureType.DvergerMage, new ValheimCreature(6.4f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Gjall, new ValheimCreature(6.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Queen, new ValheimCreature(6.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Seeker, new ValheimCreature(6.3f, e.SpawnEffectSmall) },
                { ValheimCreatureType.SeekerBrood, new ValheimCreature(6.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.SeekerBrute, new ValheimCreature(6.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Tick, new ValheimCreature(6.2f, e.SpawnEffectSmall) },

                // ASHLANDS
                { ValheimCreatureType.Asksvin, new ValheimCreature(7.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.BlobLava, new ValheimCreature(7.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.BonemawSerpent, new ValheimCreature(7.2f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Charred_Archer, new ValheimCreature(7.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Charred_Melee, new ValheimCreature(7.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Charred_Mage, new ValheimCreature(7.4f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Charred_Twitcher, new ValheimCreature(7.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Charred_Twitcher_Summoned, new ValheimCreature(7.2f, e.SpawnEffectSmall) },
                { ValheimCreatureType.Fader, new ValheimCreature(7.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.LordReto, new ValheimCreature(7.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Morgen, new ValheimCreature(7.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.FallenValkyrie, new ValheimCreature(7.6f, e.SpawnEffectMedium) },
                { ValheimCreatureType.Volture, new ValheimCreature(7.3f, e.SpawnEffectMedium) },
            };
        }

        public static ValheimCreature GetValheimCreature(string name)
        {
            return valheimCreatures.GetValueSafe(name.Replace("(Clone)", ""));
        }

        public static void HandleSpawnCreatureRedeem(RedeemData redeem, CustomRewardEvent customRewardEvent, TwitchChat chat)
        {
            if (redeem.creatureData == null)
                throw new RedeemException("Could not find creature data for SpawnCreature", ExceptionType.Error);

            if (GetNrOfTwitchInstances(ProfileSettingsHelper.Current.creaturesMaxRadius) >= ProfileSettingsHelper.Current.creaturesMaxAmount)
            {
                chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the maximum spawned creature limit has been reached! {(ProfileSettingsHelper.Current.autoResolveRedeems ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                throw new RedeemException("To many spawned creatures", ExceptionType.Warning);
            }

            List<CreatureData> spawnList = ResolveSpawnList(redeem.creatureData);

            if (Player.m_localPlayer.InInterior() && CancelRedeemCauseOfDungeon(spawnList))
            {
                chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is inside a dungeon and this redeem is not allowed in dungeons! {(ProfileSettingsHelper.Current.autoResolveRedeems ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                throw new RedeemException("Player is in dungeon and redeem is not allowed", ExceptionType.Warning);
            }

            foreach (CreatureData creature in spawnList)
            {
                if (!ProgressionHelper.IsAllowedByGlobalKeys(creature.globalKeyAdd, creature.globalKeyRemove))
                    continue;

                if (creature.maxSpawned > 0 && GetNrOfSpecificTwitchInstances(creature.prefabName) >= creature.maxSpawned)
                {
                    chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the maximum spawned limit of {creature.prefabName} has been reached! {(ProfileSettingsHelper.Current.autoResolveRedeems ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                    throw new RedeemException("To many of the same spawned creatures", ExceptionType.Warning);
                }

                if (redeem.userInput)
                    creature.talkMessage = chat.GetLastMessageOfUser(customRewardEvent.RedeemerName)?.message;

                if (creature.amount > 0)
                    SpawnCreatures(creature, Player.m_localPlayer.transform, customRewardEvent, redeem.ignoreWard);
            }
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

            string positionType = creatureData.position;
            float positionRadius = creatureData.positionRadius;

            if (Player.m_localPlayer.InInterior())
            {
                // Dungeon ceilings/floors break the open-world ground snapping used by other
                // position types, so force a (radius-limited) Random spawn near the player.
                if (positionType != SpawnPositionType.OnPlayer && positionType != SpawnPositionType.Random)
                    positionType = SpawnPositionType.Random;

                if (positionType == SpawnPositionType.Random)
                    positionRadius = Mathf.Min(positionRadius, 10f);
            }

            if (positionType == SpawnPositionType.WorldPosition)
                spawnPosition = creatureData.positionOffset.ToVector();
            else
            {
                spawnPosition = TransformHelper.UpdateSpawnLocation(positionType, transform, creatureData.positionOffset, positionRadius);
                spawnRotation = TransformHelper.UpdateSpawnRotation(positionType, spawnRotation);
            }

            GameObject creature = ZNetViewHelper.Instantiate(prefab, spawnPosition, spawnRotation);
            MonsterAI monsterAI = creature.GetComponent<MonsterAI>();
            Humanoid humanoid = creature.GetComponent<Humanoid>();

            if (creatureData.requireMonsterComponents)
            {
                if (monsterAI == null)
                {
                    ZNetViewHelper.Destroy(creature);
                    throw new System.Exception("No monster AI available for creature spawn!");
                }

                if (humanoid == null)
                {
                    ZNetViewHelper.Destroy(creature);
                    throw new System.Exception("No humanoid available for creature spawn!");
                }
            }

            // Init runs before SetData so TwitchCreatureClaim.m_originalName captures the true
            // pre-rename species name - SetData's ApplyHumanoid can rename the creature (via
            // creatureData.rename) and running it first would make m_originalName just capture
            // that already-renamed name instead, breaking !unclaim <name> matching against the
            // creature's real species (mirrors the ordering TwitchCreaturePersistentData.Awake()
            // already uses on reload: ReInit before ApplyCreatureData).
            TwitchCreatureClaim creatureClaim = creature.AddComponent<TwitchCreatureClaim>();
            creatureClaim.Init(creatureData, customRewardEvent);

            TwitchCreaturePersistentData persistentData = creature.AddComponent<TwitchCreaturePersistentData>();
            persistentData.SetData(customRewardEvent.RedeemerName, creatureData, customRewardEvent.CustomRewardTitle, ignoreWard);

            if (force > 0f)
            {
                Rigidbody rigidBody = creature.GetComponent<Rigidbody>();
                rigidBody.AddForce((transform.forward * force) + (transform.up * force), ForceMode.Acceleration);
            }

            if (monsterAI != null)
            {
                monsterAI.StartCoroutine(monsterAI.WakeUpAfterDelay(1f));
                monsterAI.LookAt(transform.position);
            }

            if (creatureData.smelterFuelAmount > 0f)
                FillSmelterFuel(creature, creatureData.smelterFuelAmount);

            if (creatureData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, creatureData.announceMessage), 3000);

            ValheimCreature valheimCreature = GetValheimCreature(creature.name);

            if (valheimCreature == null)
            {
                Jotunn.Logger.LogWarning($"No ValheimCreature entry found for {creature.name}, skipping spawn effect.");
                return;
            }

            valheimCreature.spawnEffects.Create(creature.transform.position, creature.transform.rotation);
        }

        // Temporary hack: fills a spawned Smelter-based prefab's (e.g. the hot tub's) fuel tank.
        // Smelter.GetFuel/SetFuel are private, but they just read/write the vanilla ZDOVars.s_fuel
        // ZDO field - written once here on the spawning/owning client, same pattern as
        // TwitchCreaturePersistentData's one-time persistent-data write right after spawn.
        private static void FillSmelterFuel(GameObject creature, float fuelAmount)
        {
            Smelter smelter = creature.GetComponent<Smelter>();

            if (smelter == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] CreatureHelper: no Smelter component found to fill fuel on");
                return;
            }

            ZNetView netView = creature.GetComponent<ZNetView>();

            if (netView == null || !netView.IsValid())
                return;

            netView.GetZDO().Set(ZDOVars.s_fuel, Mathf.Min(fuelAmount, smelter.m_maxFuel));
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

        /// <summary>
        /// Calculates a linear health/damage multiplier based on the tier gap between
        /// the player and the creature.
        /// Returns <c>1.0</c> when both are in the same biome band (no scaling applied).
        /// A positive delta (creature below player) scales the stat <b>up</b>;
        /// a negative delta (creature above player) scales it <b>down</b>.
        /// <c>percentagePerTier</c> is the fraction applied per unit of tier difference,
        /// e.g. <c>0.15</c> = 15% per tier.
        /// </summary>
        public static float CalculateScale(float playerTier, float creatureTier, float percentagePerTier)
        {
            // Same biome band — leave the creature completely untouched
            if (Mathf.Floor(playerTier) == Mathf.Floor(creatureTier))
                return 1f;

            float delta = playerTier - creatureTier;
            float scale = 1f + (delta * percentagePerTier);

            // Never reduce below 10% or scale past 500% to avoid absurd edge cases
            return Mathf.Clamp(scale, 0.1f, 5f);
        }

        // Creatures summoned by another creature's own vanilla SpawnAbility (a Warlock/Oozer/Gjall's
        // reinforcements) bypass CreatureHelper.SpawnCreature entirely - SpawnAbility.Spawn() calls
        // UnityEngine.Object.Instantiate directly, so the child never gets TwitchCreaturePersistentData/
        // TwitchCreatureClaim and is invisible to both health scaling and CharacterDamage_Prefix's
        // damage scaling. TryResolveParentCreatureData + ApplyInheritedScaling (called from
        // SpawnAbilityExtension.Spawn2, via SpawnAbilityPatchesWBTI which only redirects there when
        // the summoning creature is itself a Twitch spawn) make the child an actual tracked spawn
        // too, inheriting everything the parent's own redeem configured (color, rename, health/damage
        // scale, etc.) under the child's own real prefab identity.

        /// <summary>
        /// Resolves the CreatureData the summoning creature's own redeem configured for it, so its
        /// children can inherit the same settings. Returns false (nothing to inherit) when the
        /// summoner isn't itself resolvable to a real redeem entry - e.g. a wild creature only
        /// claim:-renamed via chat, or one spawned via a redeem type this doesn't cover.
        /// </summary>
        public static bool TryResolveParentCreatureData(Character owner, out CreatureData parentCreatureData, out string redeemerName, out string redeemTitle, out string lookupPrefabName)
        {
            parentCreatureData = null;
            redeemerName = null;
            redeemTitle = null;
            lookupPrefabName = null;

            TwitchCreaturePersistentData ownerData = owner?.gameObject.GetComponent<TwitchCreaturePersistentData>();

            if (ownerData == null || string.IsNullOrEmpty(ownerData.RedeemTitle))
                return false;

            RedeemData redeem = RedeemHelper.GetRedeemByTitle(ownerData.RedeemTitle);

            if (redeem == null)
                return false;

            List<CreatureData> resolvedList = RedeemHelper.GetResolvedCreatureList(redeem.creatureData);
            CreatureData found = resolvedList.Find(c => c.prefabName == ownerData.SavedPrefabName);

            if (found == null)
                return false;

            parentCreatureData = found;
            redeemerName = ownerData.RedeemerName;
            redeemTitle = ownerData.RedeemTitle;
            lookupPrefabName = ownerData.SavedPrefabName;
            return true;
        }

        /// <summary>
        /// Makes <paramref name="child"/> (a creature just spawned by <paramref name="owner"/>'s own
        /// SpawnAbility) inherit <paramref name="parentCreatureData"/> - clones it, swaps in the
        /// child's own real prefab identity, and applies it the same way a direct redeem spawn would.
        /// </summary>
        public static void ApplyInheritedScaling(GameObject child, GameObject prefab, CreatureData parentCreatureData, string redeemerName, string redeemTitle, string lookupPrefabName)
        {
            // SpawnAbility isn't creature-exclusive (e.g. an AOE puddle or projectile) - only
            // inherit onto actual creatures, same gate the redeem-shower Spawn2 overload uses.
            if (child.GetComponent<MonsterAI>() == null || child.GetComponent<Humanoid>() == null)
                return;

            CreatureData childCreatureData = parentCreatureData.Clone<CreatureData>();
            childCreatureData.prefabName = prefab.name;

            TwitchCreaturePersistentData childData = child.GetComponent<TwitchCreaturePersistentData>() ?? child.AddComponent<TwitchCreaturePersistentData>();
            childData.SetInherited(redeemerName, redeemTitle, lookupPrefabName, childCreatureData);
        }

        public static void SetFollowInRadius(bool value)
        {
            Collider[] found = Physics.OverlapSphere(Player.m_localPlayer.transform.position, ProfileSettingsHelper.Current.creaturesFollowRadius, LayerMask.GetMask("character"));

            foreach (var item in found)
            {
                GameObject rootObject = item.transform.root.gameObject;
                MonsterAI monsterAI = rootObject.GetComponent<MonsterAI>();

                if (monsterAI == null)
                    continue;

                TwitchCreaturePersistentData persistentData = rootObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData != null)
                {
                    persistentData.SetFollowing(value);
                    monsterAI.SetFollowTarget(value ? Player.m_localPlayer.gameObject : null);
                }
            }
        }

        public static bool CancelRedeemCauseOfDungeon(List<CreatureData> creatures)
        {
            if (creatures == null)
                return false;

            List<string> notAllowedList = GetDungeonForbiddenCreatures();

            foreach (CreatureData creature in creatures)
            {
                if (notAllowedList.Contains(creature.prefabName))
                    return true;
            }

            return false;
        }

        public static List<string> GetDungeonForbiddenCreatures()
        {
            string dungeonType = EnvMan.instance.GetCurrentEnvironment().m_name;

            List<string> notAllowedList = new List<string>
            {
                ValheimCreatureType.Greydwarf_Elite,
                ValheimCreatureType.Abomination,
                ValheimCreatureType.Bat,
                ValheimCreatureType.Bjorn,
                ValheimCreatureType.BonemawSerpent,
                ValheimCreatureType.Deathsquito,
                ValheimCreatureType.Gjall,
                ValheimCreatureType.GoblinBrute,
                ValheimCreatureType.StoneGolem,
                ValheimCreatureType.Hatchling,
                ValheimCreatureType.Lox,
                ValheimCreatureType.SeekerBrute,
                ValheimCreatureType.Serpent,
                ValheimCreatureType.Troll,
                ValheimCreatureType.Unbjorn,
            };

            switch (dungeonType)
            {
                case nameof(DungeonType.FrostCave):
                case nameof(DungeonType.HowlingCavern):
                    notAllowedList.Remove(ValheimCreatureType.StoneGolem);
                    break;
                case nameof(DungeonType.InfestedMine):
                    notAllowedList.Remove(ValheimCreatureType.SeekerBrute);
                    notAllowedList.Remove(ValheimCreatureType.StoneGolem);
                    break;
                case nameof(DungeonType.Queen):
                    notAllowedList.Remove(ValheimCreatureType.Bat);
                    notAllowedList.Remove(ValheimCreatureType.Deathsquito);
                    notAllowedList.Remove(ValheimCreatureType.Gjall);
                    notAllowedList.Remove(ValheimCreatureType.StoneGolem);
                    notAllowedList.Remove(ValheimCreatureType.SeekerBrute);
                    break;
            }

            return notAllowedList;
        }

        public static int GetNrOfTwitchInstances(float maxRange = 100f)
        {
            int num = 0;

            foreach (BaseAI creature in BaseAI.BaseAIInstances)
            {
                if (creature.GetComponent<TwitchCreatureClaim>() == null)
                    continue;

                if (Player.m_localPlayer != null && maxRange > 0f && Vector3.Distance(Player.m_localPlayer.transform.position, creature.transform.position) > maxRange)
                    continue;

                num++;
            }

            return num;
        }

        public static int GetNrOfSpecificTwitchInstances(string prefabName, float maxRange = 100f)
        {
            int num = 0;

            foreach (BaseAI creature in BaseAI.BaseAIInstances)
            {
                if (creature.GetComponent<TwitchCreatureClaim>() == null)
                    continue;

                if (creature.name != prefabName + "(Clone)")
                    continue;

                if (Player.m_localPlayer != null && maxRange > 0f && Vector3.Distance(Player.m_localPlayer.transform.position, creature.transform.position) > maxRange)
                    continue;

                num++;
            }

            return num;
        }

        private static List<CreatureData> ResolveSpawnList(SpawnCreatureData creatureData)
        {
            List<CreatureData> spawnList = new List<CreatureData>();
            List<CreatureData> source = creatureData.list;

            if (creatureData.random)
            {
                if (Player.m_localPlayer.InInterior())
                {
                    List<string> forbidden = GetDungeonForbiddenCreatures();
                    source = source.FindAll(c => !forbidden.Contains(c.prefabName));
                }

                source = source.FindAll(c => c.maxSpawned <= 0 || GetNrOfSpecificTwitchInstances(c.prefabName) < c.maxSpawned);

                if (source.Count == 0)
                    throw new RedeemException("No valid creatures available to spawn!", ExceptionType.Warning);

                source = new List<CreatureData> { GetRandomCreatureData(source) };
            }

            foreach (CreatureData entry in source)
            {
                if (entry.group != null)
                {
                    CreatureGroupData creatureGroup = RedeemHelper.creatureGroups.Find(item => item.group == entry.group);

                    if (creatureGroup == null)
                        throw new RedeemException("Could not find referenced group!", ExceptionType.Error);

                    foreach (CreatureData groupCreature in creatureGroup.list)
                    {
                        groupCreature.group = creatureGroup.group;
                        spawnList.Add(groupCreature);
                    }
                }
                else
                    spawnList.Add(entry);
            }

            return spawnList;
        }
    }
}
