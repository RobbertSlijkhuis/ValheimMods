using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SeedCart.Components {
    public class MovementChecker : MonoBehaviour
    {
        private ZNetView netView;
        public Transform cartTransform;
        private Vector3 lastPosition;
        public float distanceThreshold = 0.9f;

        private List<string> m_allowedList = new List<string>();
        private Dictionary<string, string> m_plantDict = new Dictionary<string, string>();

        private void Awake()
        {
            netView = gameObject.GetComponent<ZNetView>();
            cartTransform = transform;
            lastPosition = cartTransform.position;

            m_allowedList.Add("barley");
            m_allowedList.Add("carrot");
            m_allowedList.Add("flax");
            m_allowedList.Add("onion");
            m_allowedList.Add("turnip");

            m_plantDict.Add("$item_carrotseeds", "sapling_carrot");
            m_plantDict.Add("$item_carrot", "sapling_seedcarrot");
            m_plantDict.Add("$item_turnipseeds", "sapling_turnip");
            m_plantDict.Add("$item_turnip", "sapling_seedturnip");
            m_plantDict.Add("$item_onionseeds", "sapling_onion");
            m_plantDict.Add("$item_onion", "sapling_seedonion");
            m_plantDict.Add("$item_barley", "sapling_barley");
            m_plantDict.Add("$item_flax", "sapling_flax");
        }

        private void Update()
        {
            try {
                if (netView.m_ghost)
                    return;

                // Calculate the distance from the last frame
                float distance = Vector3.Distance(lastPosition, cartTransform.position);

                // Check if the player has moved more than the threshold
                if (distance >= distanceThreshold)
                {
                    Jotunn.Logger.LogWarning("Player has moved " + distance + " meters!");
                    // Reset last position or do something else here
                    lastPosition = cartTransform.position;

                    List<Transform> plantPoints = new List<Transform>();
                    plantPoints.Add(transform.Find("PlantPointLeft"));
                    plantPoints.Add(transform.Find("PlantPointCenter"));
                    plantPoints.Add(transform.Find("PlantPointRight"));

                    // Jotunn.Logger.LogWarning("plantPoints: " + plantPoints.Count);

                    foreach (Transform plantTransform in plantPoints)
                    {
                        RaycastHit hitInfo;
                        Vector3 adjustedPos = plantTransform.position + Vector3.up * 0.5f;
                        Physics.Raycast(adjustedPos, Vector3.down, out hitInfo, 1f);

                        LineRenderer lineRenderer = plantTransform.gameObject.GetComponent<LineRenderer>();
                        lineRenderer.SetPosition(0, adjustedPos);
                        lineRenderer.SetPosition(1, adjustedPos + Vector3.down * 1f);

                        Jotunn.Logger.LogWarning(hitInfo.collider?.gameObject?.name);

                        if (hitInfo.collider == null || hitInfo.collider.gameObject == null || !hitInfo.collider.gameObject.name.Equals("terrain", StringComparison.OrdinalIgnoreCase))
                        {
                            // Jotunn.Logger.LogWarning("No gameObject found! " + plantTransform.name);
                            continue;
                        }

                        Heightmap heightmapComp = hitInfo.collider.gameObject.GetComponent<Heightmap>();

                        if (heightmapComp != null && !heightmapComp.IsCultivated(hitInfo.point))
                        {
                            // Jotunn.Logger.LogWarning("Nope... " + plantTransform.name);
                            continue;
                        }
                        else
                        {
                            Jotunn.Logger.LogWarning("CULTIVATED! " + plantTransform.name);
                        }

                        Container containerComp = transform.Find("Container").GetComponent<Container>();
                        Inventory inv = containerComp.GetInventory();
                        List<ItemDrop.ItemData> items = inv.GetAllItemsInGridOrder();
                        items.Reverse();

                        List<ItemDrop.ItemData> plantableList = items.Where(item => m_allowedList.Any(other => item.m_shared.m_name.Contains(other))).ToList();

                        if (plantableList.Count == 0)
                        {
                            Jotunn.Logger.LogWarning("No applicable item found!");
                            continue;
                        }

                        foreach (ItemDrop.ItemData item in plantableList)
                        {
                            Jotunn.Logger.LogWarning(item.m_shared.m_name);
                        }

                        ItemDrop.ItemData firstItem = plantableList.First();

                        if (firstItem == null)
                        {
                            Jotunn.Logger.LogError("Could not find first item in cart container!");
                            continue;
                        }

                        string prefabName = m_plantDict.GetValueSafe(firstItem.m_shared.m_name);

                        if (prefabName == null || prefabName == "")
                        {
                            Jotunn.Logger.LogError("Could not find corresponding prefab to plant!");
                            continue;
                        }

                        Vector3 plantPosition = new Vector3(plantTransform.position.x, plantTransform.position.y, plantTransform.position.z);

                        if (ZoneSystem.instance.FindFloor(plantPosition, out var height))
                        {
                            plantPosition.y = height;
                        }

                        GameObject onion = Instantiate(PrefabManager.Instance.GetPrefab(prefabName), plantPosition, plantTransform.rotation);
                        Piece pieceComp = onion.GetComponent<Piece>();
                        pieceComp.m_placeEffect.Create(plantPosition, plantTransform.rotation);

                        inv.RemoveOneItem(firstItem);
                    }
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Update: " + e);
            }
        }
    }
}