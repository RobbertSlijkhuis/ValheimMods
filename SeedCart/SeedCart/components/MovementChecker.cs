using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class MovementChecker : MonoBehaviour
{
    public Transform playerTransform; // Assign your player's transform here
    private Vector3 lastPosition;
    public float distanceThreshold = 2f;

    private void Awake()
    {
        // Store the initial position
        playerTransform = transform;
        lastPosition = playerTransform.position;
    }

    private void Update()
    {
        // Calculate the distance from the last frame
        float distance = Vector3.Distance(lastPosition, playerTransform.position);

        // Check if the player has moved more than the threshold
        if (distance >= distanceThreshold)
        {
           
            Jotunn.Logger.LogWarning("Player has moved " + distance + " meters!");
            // Reset last position or do something else here
            lastPosition = playerTransform.position;

            RaycastHit hitInfo;
            Vector3 adjustedPos = playerTransform.position + Vector3.up * 0.5f;
            Physics.Raycast(adjustedPos, Vector3.down, out hitInfo, 1f);

            LineRenderer lineRenderer = transform.Find("DebugRayCast").gameObject.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, adjustedPos);
            lineRenderer.SetPosition(1, playerTransform.position + Vector3.down);

            Jotunn.Logger.LogWarning(hitInfo.collider?.gameObject?.name);

            if (hitInfo.collider == null || hitInfo.collider.gameObject == null)
            {
                Jotunn.Logger.LogWarning("No gameObject found!");
                return;
            }

            //var components = hitInfo.collider.GetComponents(typeof(UnityEngine.Component));
            //foreach (UnityEngine.Component component in components)
            //{
            //    Jotunn.Logger.LogWarning(component.ToString());
            //}

            Heightmap heightmapComp = hitInfo.collider.gameObject.GetComponent<Heightmap>();

            if (heightmapComp != null && heightmapComp.IsCultivated(hitInfo.point))
            {
                Jotunn.Logger.LogWarning("CULTIVATED!");
            }
            else
            {
                Jotunn.Logger.LogWarning("Nope...!");
            }

            Container containerComp = transform.Find("Container").GetComponent<Container>();
            Inventory inv = containerComp.GetInventory();
            List<ItemDrop.ItemData> items = inv.GetAllItemsInGridOrder();
            items.Reverse();

            List<string> filterList = new List<string>();
            filterList.Add("barley");
            filterList.Add("carrot");
            filterList.Add("flax");
            filterList.Add("onion");
            filterList.Add("turnip");

            // List<ItemDrop.ItemData> plantableList = items.Where(item => filterList.Contains(item.m_shared.m_name)).ToList();
            // List<ItemDrop.ItemData> plantableList = items.FindAll(item => filterList.Contains(item.m_shared.m_name));
            List<ItemDrop.ItemData> plantableList = items.Where(item => filterList.Any(other => item.m_shared.m_name.Contains(other))).ToList();

            if (plantableList.Count == 0)
            {
                Jotunn.Logger.LogWarning("No applicable item found!");
                return;
            }

            foreach (ItemDrop.ItemData item in plantableList)
            {
                Jotunn.Logger.LogWarning(item.m_shared.m_name);
            }

        }
    }
}