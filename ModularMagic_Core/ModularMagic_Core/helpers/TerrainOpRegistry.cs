using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Custom terrain op prefabs (a prefab with a TerrainOp). The game looks a terrain op up in ObjectDB by prefab name hash when the
    /// op is applied. Jotunn registers it when ZNetScene awakes, but the ObjectDB of the game scene does not exist yet then, and it
    /// builds its table from its own list afterwards, so the op is missing and the terrain edit fails with "Failed to deserialize
    /// TerrainOp settings". A mod registers its terrain op prefab here, and Core adds it to the table of every ObjectDB when it is built.
    /// </summary>
    public static class TerrainOpRegistry
    {
        private static readonly List<GameObject> prefabs = new List<GameObject>();

        /// <summary>
        /// Registers a terrain op prefab. Call it once at startup (in Awake, after the prefab is loaded).
        /// </summary>
        public static void Register(GameObject prefab)
        {
            if (prefab == null || prefab.GetComponent<TerrainOp>() == null)
            {
                Jotunn.Logger.LogWarning("[TerrainOp] Cannot register a prefab without a TerrainOp");
                return;
            }

            if (!prefabs.Contains(prefab))
                prefabs.Add(prefab);
        }

        // Only the table of that ObjectDB is changed: its op list is shared with the prefab ObjectDB (CopyOtherDB), so adding to the
        // list would add the op again on every rebuild
        internal static void AddToTable(ObjectDB db)
        {
            try
            {
                if (db == null)
                    return;

                foreach (GameObject prefab in prefabs)
                {
                    if (prefab == null)
                        continue;

                    TerrainOp terrainOp = prefab.GetComponent<TerrainOp>();

                    if (terrainOp != null)
                        db.m_terrainOpsByHash[db.GetPrefabHash(prefab)] = terrainOp;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not register the terrain ops: " + e);
            }
        }
    }
}
