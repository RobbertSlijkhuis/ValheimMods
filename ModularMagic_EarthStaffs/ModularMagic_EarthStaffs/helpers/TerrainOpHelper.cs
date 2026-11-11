using System;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Helpers
{
    /// <summary>
    /// The terrain op of the nova (script_levelTerrain_MMES). The game looks a terrain op up in ObjectDB by prefab name hash when the op is
    /// applied. Jotunn registers it when ZNetScene awakes, but the ObjectDB of the game scene does not exist yet then, and it builds its table
    /// from its own list afterwards, so the op is added to every ObjectDB table when it is built (see Register).
    /// </summary>
    internal static class TerrainOpHelper
    {
        // Adds the terrain op of the nova to the table of an ObjectDB that was just built. Only the table of that ObjectDB is changed:
        // its op list is shared with the prefab ObjectDB (CopyOtherDB), so adding to the list would add the op again on every rebuild
        public static void Register(ObjectDB db)
        {
            try
            {
                GameObject prefab = ModularMagic_EarthStaffs.prefabs?.LevelTerrain;

                if (db == null || prefab == null)
                    return;

                TerrainOp terrainOp = prefab.GetComponent<TerrainOp>();

                if (terrainOp == null)
                    return;

                db.m_terrainOpsByHash[db.GetPrefabHash(prefab)] = terrainOp;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not register the terrain op of the nova: " + e);
            }
        }

        public static void LogRegistration(string moment)
        {
            try
            {
                GameObject prefab = ModularMagic_EarthStaffs.prefabs?.LevelTerrain;
                ObjectDB db = ObjectDB.instance;

                if (prefab == null)
                {
                    Jotunn.Logger.LogWarning($"[TerrainOp] {moment}: the LevelTerrain prefab is not loaded yet");
                    return;
                }

                if (db == null)
                {
                    Jotunn.Logger.LogWarning($"[TerrainOp] {moment}: there is no ObjectDB.instance");
                    return;
                }

                bool known = db.TryGetTerrainOp(prefab.name, out _);

                Jotunn.Logger.LogWarning($"[TerrainOp] {moment}: ObjectDB {db.GetInstanceID()} knows {prefab.name}: {known} ({db.m_terrainOpsByHash.Count} ops in the hash table, {db.m_terrainOps.Count} in the list)");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not log the terrain op registration: " + e);
            }
        }
    }
}
