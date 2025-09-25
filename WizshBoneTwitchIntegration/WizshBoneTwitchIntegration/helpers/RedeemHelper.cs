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

        public static Vector3 GenerateSpawnLocation(Transform transform, string type)
        {
            switch (type)
            {
                case nameof(SpawnPositionType.Flying):
                    Vector3 position = (transform.forward * 10f) + (transform.up * 7f) + transform.position;
                    return position;
                default:
                    return transform.position;
            }
        }

        public static void SpawnCreature(SpawnOptions options)
        {
            Transform transform = options.transform;
            GameObject prefab = PrefabManager.Instance.GetPrefab(options.prefabName);

            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find prefab to spawn");
            }

            GameObject creature = UnityEngine.Object.Instantiate(prefab, GenerateSpawnLocation(transform, options.creatureData.position), transform.rotation);
            Humanoid humanComp = creature.GetComponent<Humanoid>();
            humanComp.m_name = options.customReward.RedeemerName;
            humanComp.m_faction = Character.Faction.Boss;
            humanComp.m_level = options.creatureData.level;

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
    }
}
