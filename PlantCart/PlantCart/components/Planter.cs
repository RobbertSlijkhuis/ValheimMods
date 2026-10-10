using HarmonyLib;
using Jotunn.Managers;
using PlantCart.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlantCart.Components {
    public class Planter : MonoBehaviour
    {
        private ZNetView netView;
        private Vagon vagon;
        public Transform cartTransform;
        private Vector3 lastPosition;
        public float distanceThreshold = 1.1f;

        private Dictionary<string, string> m_plantDict = new Dictionary<string, string>();

        public void Awake()
        {
            netView = gameObject.GetComponent<ZNetView>();
            vagon = gameObject.GetComponent<Vagon>();
            cartTransform = transform;
            lastPosition = cartTransform.position;

            m_plantDict.Add("$item_carrotseeds", "sapling_carrot");
            m_plantDict.Add("$item_carrot", "sapling_seedcarrot");
            m_plantDict.Add("$item_turnipseeds", "sapling_turnip");
            m_plantDict.Add("$item_turnip", "sapling_seedturnip");
            m_plantDict.Add("$item_onionseeds", "sapling_onion");
            m_plantDict.Add("$item_onion", "sapling_seedonion");
            m_plantDict.Add("$item_barley", "sapling_barley");
            m_plantDict.Add("$item_flax", "sapling_flax");
            m_plantDict.Add("$item_magecap", "sapling_magecap");
            m_plantDict.Add("$item_jotunpuffs", "sapling_jotunpuffs");
            m_plantDict.Add("$item_kale", "sapling_seedkale");
            m_plantDict.Add("$item_kaleseeds", "sapling_Kale");
            m_plantDict.Add("$item_poteitrseeds", "sapling_poteitr");
            m_plantDict.Add("$item_oatseeds", "sapling_oat");
        }

        public bool IsValidPlantPoint(RaycastHit raycast)
        {
            if (raycast.collider == null || raycast.collider.gameObject == null || !raycast.collider.gameObject.name.Equals("terrain", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Heightmap heightMap = raycast.collider.gameObject.GetComponent<Heightmap>();

            if (heightMap == null || !heightMap.IsCultivated(raycast.point))
            {
                return false;
            }

            return true;
        }

        public void Update()
        {
            try {
                if (netView.m_ghost)
                    return;

                // Only the cart's owner (whoever attached it) plants. Otherwise every client that has the
                // cart loaded plants its own copy of each crop, and a non-owner's inventory change is never saved.
                // Also only plant while someone is pulling the cart, pushing it does nothing.
                // Keep lastPosition current while not planting, so taking ownership or attaching doesn't cause an instant plant.
                if (!netView.IsValid() || !netView.IsOwner() || !vagon.IsAttached())
                {
                    lastPosition = cartTransform.position;
                    return;
                }

                // Calculate the distance from the last frame
                float distance = Vector3.Distance(lastPosition, cartTransform.position);

                // Check if the player has moved more than the threshold
                if (distance >= distanceThreshold)
                {
                    // Reset last position or do something else here
                    lastPosition = cartTransform.position;

                    List<Transform> plantPoints = new List<Transform>();
                    plantPoints.Add(transform.Find("PlantPointLeft"));
                    plantPoints.Add(transform.Find("PlantPointRight"));

                    foreach (Transform plantTransform in plantPoints)
                    {
                        RaycastHit raycast;
                        RaycastHit raycastBack;
                        RaycastHit raycastFront;
                        Vector3 adjustedPos = plantTransform.position + plantTransform.up * 0.4f;
                        Vector3 adjustedPosBack = adjustedPos + plantTransform.forward * -0.55f;
                        Vector3 adjustedPosFront = adjustedPos + plantTransform.forward * 0.55f;

                        Physics.Raycast(adjustedPos, Vector3.down, out raycast, 1f);
                        Physics.Raycast(adjustedPosBack, Vector3.down, out raycastBack, 1f);
                        Physics.Raycast(adjustedPosFront, Vector3.down, out raycastFront, 1f);

                        //LineRenderer lineRenderer = plantTransform.gameObject.GetComponent<LineRenderer>();
                        //lineRenderer.enabled = true;
                        //lineRenderer.SetPosition(0, adjustedPosFront);
                        //lineRenderer.SetPosition(1, adjustedPosFront + Vector3.down * 1f);

                        if (!IsValidPlantPoint(raycast) || !IsValidPlantPoint(raycastBack) || !IsValidPlantPoint(raycastFront))
                        {
                            continue;
                        }

                        Collider[] objects = Physics.OverlapSphere(raycast.point, 0.5f, LayerMask.GetMask("piece_nonsolid"));

                        if (objects.Length > 0)
                            continue;

                        Container containerComp = transform.Find("Container").GetComponent<Container>();
                        Inventory inv = containerComp.GetInventory();
                        List<ItemDrop.ItemData> items = inv.GetAllItemsInGridOrder();

                        List<ItemDrop.ItemData> plantableList = items.Where(item => m_plantDict.ContainsKey(item.m_shared.m_name)).ToList();

                        if (plantableList.Count == 0)
                        {
                            continue;
                        }

                        ItemDrop.ItemData firstItem = plantableList.First();

                        if (firstItem == null)
                        {
                            continue;
                        }

                        string prefabName = m_plantDict.GetValueSafe(firstItem.m_shared.m_name);

                        if (prefabName == null || prefabName == "")
                        {
                            continue;
                        }

                        Vector3 plantPosition = new Vector3(plantTransform.position.x, plantTransform.position.y, plantTransform.position.z);

                        if (ZoneSystem.instance.FindFloor(plantPosition, out var height))
                        {
                            plantPosition.y = height;
                        }

                        GameObject crop = Instantiate(PrefabManager.Instance.GetPrefab(prefabName), plantPosition, plantTransform.rotation);
                        Piece pieceComp = crop.GetComponent<Piece>();
                        pieceComp.m_placeEffect.Create(plantPosition, plantTransform.rotation);

                        inv.RemoveOneItem(firstItem);

                        if (PluginConfig.piece1.durabilityLoss.Value > 0)
                        {
                            WearNTear wearNTear = gameObject.GetComponent<WearNTear>();
                            wearNTear.ApplyDamage(PluginConfig.piece1.durabilityLoss.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Planter update: " + e);
            }
        }
    }
}