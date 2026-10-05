using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class DetonateHelper
    {
        private const float DelayBetweenDetonations = 0.05f;
        private const int MaxTargets = 100;

        private struct QueuedDetonation
        {
            public DetonateData data;
            public CustomRewardEvent customRewardEvent;
        }

        private static readonly Queue<QueuedDetonation> s_queue = new Queue<QueuedDetonation>();
        private static bool s_isRunning;

        // Called from GameAwake_Postfix so a fresh world load/relog doesn't inherit a queue stuck
        // forever by an abandoned coroutine: ProcessQueue/Detonate run on the TwitchCustomRewards
        // host attached to Game.instance.gameObject, and if that GameObject is destroyed mid-effect
        // (logout, world change), Unity silently kills the coroutine without ever reaching its
        // finally block - leaving s_isRunning stuck true and every future Enqueue() call
        // permanently skipping StartCoroutine. See FlashBangHelper.ResetQueue for the same pattern.
        public static void ResetQueue()
        {
            s_queue.Clear();
            s_isRunning = false;
        }

        public static void Enqueue(MonoBehaviour host, DetonateData detonateData, CustomRewardEvent customRewardEvent)
        {
            s_queue.Enqueue(new QueuedDetonation { data = detonateData, customRewardEvent = customRewardEvent });

            if (!s_isRunning)
                host.StartCoroutine(ProcessQueue(host));
        }

        private static IEnumerator ProcessQueue(MonoBehaviour host)
        {
            s_isRunning = true;
            try
            {
                while (s_queue.Count > 0)
                {
                    QueuedDetonation next = s_queue.Dequeue();
                    yield return host.StartCoroutine(Detonate(next.data, next.customRewardEvent));
                }
            }
            finally
            {
                s_queue.Clear();
                s_isRunning = false;
            }
        }

        public static IEnumerator Detonate(DetonateData detonateData, CustomRewardEvent customRewardEvent)
        {
            if (detonateData.values.Count == 0)
            {
                Jotunn.Logger.LogWarning($"[WBTI] DetonateHelper: '{customRewardEvent.CustomRewardTitle}' has no values configured, skipping detonation.");
                yield break;
            }

            int layerMask = 0;

            if (detonateData.type == DetonateType.Creature || detonateData.type == DetonateType.CreatureSpawned || detonateData.type == DetonateType.Fish)
                layerMask = LayerMask.GetMask("character");

            if (detonateData.type == DetonateType.ItemType)
                layerMask = LayerMask.GetMask("item");

            if (detonateData.type == DetonateType.Piece)
                layerMask = LayerMask.GetMask("piece");

            Collider[] found = Physics.OverlapSphere(Player.m_localPlayer.transform.position, detonateData.radius, layerMask);

            List<GameObject> prefabList = new List<GameObject>();
            HashSet<GameObject> seenRoots = new HashSet<GameObject>();

            foreach (var item in found)
            {
                if (prefabList.Count >= MaxTargets)
                    break;

                GameObject rootObject = item.transform.root.gameObject;

                if (!seenRoots.Add(rootObject))
                    continue;

                if (detonateData.type == DetonateType.Creature || detonateData.type == DetonateType.CreatureSpawned)
                {
                    Humanoid humanoid = rootObject.GetComponent<Humanoid>();
                    MonsterAI monsterAI = rootObject.GetComponent<MonsterAI>();

                    if (monsterAI == null || humanoid == null)
                        continue;

                    if (detonateData.type == DetonateType.CreatureSpawned && rootObject.GetComponent<TwitchCreatureClaim>() == null)
                        continue;

                    if (detonateData.values.Contains(rootObject.name.Replace("(Clone)", "")))
                        prefabList.Add(rootObject);
                }

                if (detonateData.type == DetonateType.ItemType || detonateData.type == DetonateType.Fish)
                {
                    ItemDrop itemDrop = rootObject.GetComponent<ItemDrop>();

                    if (itemDrop != null && detonateData.values.Contains(itemDrop.m_itemData.m_shared.m_itemType.ToString()))
                        prefabList.Add(rootObject);
                }

                if (detonateData.type == DetonateType.Piece)
                {
                    Piece piece = rootObject.GetComponent<Piece>();

                    if (piece != null && detonateData.values.Contains(rootObject.name.Replace("(Clone)", "")))
                        prefabList.Add(rootObject);
                }
            }

            GameObject explosionFX = PrefabManager.Instance.GetPrefab("BlobLava_explosion");
            GameObject explosionSFX = PrefabManager.Instance.GetPrefab("sfx_bloblava_death");

            if (detonateData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, detonateData.announceMessage), 3000);

            foreach (GameObject prefab in prefabList)
            {
                if (prefab == null)
                {
                    yield return new WaitForSeconds(DelayBetweenDetonations);
                    continue;
                }

                // The explosion is the damage carrier for Detonate: it goes through ZNetViewHelper
                // (marks it as a redeem spawn) and carries who redeemed it, so a death it causes
                // can be credited to them.
                GameObject explosionInstance = ZNetViewHelper.Instantiate(explosionFX, prefab.transform.position, prefab.transform.rotation);
                RedeemerTagHelper.Apply(explosionInstance, customRewardEvent);
                UnityEngine.Object.Instantiate(explosionSFX, prefab.transform.position, prefab.transform.rotation);

                if (!detonateData.damageTerrain)
                {
                    foreach (Aoe aoe in explosionInstance.GetComponentsInChildren<Aoe>(true))
                        aoe.m_spawnOnHitTerrain = null;
                }

                if (detonateData.damageData != null)
                    explosionInstance.AddComponent<TwitchPersistentDamage>().SetData(customRewardEvent, detonateData.damageData);

                ZNetViewHelper.Destroy(prefab);

                yield return new WaitForSeconds(DelayBetweenDetonations);
            }
        }
    }
}
