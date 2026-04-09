using Jotunn.Managers;
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
        public static List<GameObject> prefabList = new List<GameObject>();

        public static void Detonate(DetonateData detonateData, CustomRewardEvent customRewardEvent)
        {
            int layerMask = 0;

            if (detonateData.type == DetonateType.Creature || detonateData.type == DetonateType.CreatureSpawned || detonateData.type == DetonateType.Fish)
                layerMask = LayerMask.GetMask("character");

            if (detonateData.type == DetonateType.ItemType)
                layerMask = LayerMask.GetMask("item");

            if (detonateData.type == DetonateType.Piece)
                layerMask = LayerMask.GetMask("piece");

            Collider[] found = Physics.OverlapSphere(Player.m_localPlayer.transform.position, detonateData.radius, layerMask);

            foreach (var item in found)
            {
                GameObject rootObject = item.transform.root.gameObject;
                // Jotunn.Logger.LogWarning(rootObject.name);

                if (detonateData.type == DetonateType.Creature || detonateData.type == DetonateType.CreatureSpawned)
                {
                    Humanoid humanoid = rootObject.GetComponent<Humanoid>();
                    MonsterAI monsterAI = rootObject.GetComponent<MonsterAI>();

                    if (monsterAI == null || humanoid == null)
                        continue;

                    if (detonateData.type == DetonateType.CreatureSpawned && rootObject.GetComponent<TwitchCreatureClaim>() == null)
                        continue;

                    if (detonateData.values.Count == 0)
                        prefabList.Add(rootObject);

                    else if (detonateData.values.Contains(rootObject.name.Replace("(Clone)", "")))
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
                    {
                        prefabList.Add(rootObject);
                    }
                }
            }

            // Jotunn.Logger.LogWarning("In list: " + prefabList.Count);
            GameObject explosionFX = PrefabManager.Instance.GetPrefab("BlobLava_explosion");
            GameObject explosionSFX = PrefabManager.Instance.GetPrefab("sfx_bloblava_death");

            if (detonateData.announceMessage != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, detonateData.announceMessage), 3000);

            foreach (GameObject prefab in prefabList)
            {
                ZNetView netView = prefab.GetComponent<ZNetView>();
                UnityEngine.Object.Instantiate(explosionFX, prefab.transform.position, prefab.transform.rotation);
                UnityEngine.Object.Instantiate(explosionSFX, prefab.transform.position, prefab.transform.rotation);
                netView.Destroy();
                GameObject.Destroy(prefab);
            }

            prefabList.Clear();
        }
    }
}
