using System;
using UnityEngine;

namespace PavedRoadNoLevel.Helpers
{
    internal class TerrainToolHelper
    {
        public static void SetSmooth(GameObject prefab, bool value)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            TerrainOp terrainOp = prefab.GetComponent<TerrainOp>();

            if (terrainOp == null)
                throw new Exception("Could not find terrain op componenent");

            terrainOp.m_settings.m_smooth = value;
        }

        public static void SetStonecutter(GameObject prefab, bool value)
        {
            if (prefab == null)
                throw new Exception("Prefab is null");

            Piece piece = prefab.GetComponent<Piece>();

            if (piece == null)
                throw new Exception("Could not find piece componenent");

            piece.m_allowAltGroundPlacement = value;
            piece.m_craftingStation = PavedRoadNoLevel.Instance.configRequireStoncutter.Value ? PavedRoadNoLevel.Instance.stonecutterPiece : null;
        }
    }
}
