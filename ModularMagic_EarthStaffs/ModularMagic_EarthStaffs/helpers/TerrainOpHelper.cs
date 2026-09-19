using UnityEngine;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class TerrainOpHelper
    {
        private static bool fallbackLogged = false;

        /// <summary>
        /// Valheim resolves the settings of a TerrainOp by prefab hash through ObjectDB.TryGetTerrainOp. The ObjectDB
        /// that is active when the op is applied does not contain our prefab, even after registering it manually, so
        /// this answers the lookup for our own terrain op prefab and bypasses ObjectDB.
        /// </summary>
        public static bool TryGetOwnTerrainOp(int hash, out TerrainOp terrainOp)
        {
            terrainOp = null;

            GameObject prefab = ModularMagic_EarthStaffs.prefabs.LevelTerrain;

            if (prefab == null || hash != prefab.name.GetStableHashCode())
                return false;

            terrainOp = prefab.GetComponent<TerrainOp>();

            if (terrainOp == null)
            {
                Jotunn.Logger.LogWarning($"[TerrainOp] {prefab.name} has no TerrainOp component on its root");
                return false;
            }

            if (!fallbackLogged)
            {
                fallbackLogged = true;
                bool inObjectDB = ObjectDB.instance != null && ObjectDB.instance.m_terrainOpsByHash.ContainsKey(hash);
                Jotunn.Logger.LogWarning($"[TerrainOp] Resolving {prefab.name} ({hash}) directly, ObjectDB knows it: {inObjectDB}");
            }

            return true;
        }
    }
}
