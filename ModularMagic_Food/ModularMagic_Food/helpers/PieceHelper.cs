using Jotunn.Configs;
using Jotunn.Managers;
using ModularMagic_Food.Configs;
using System;
using UnityEngine;

namespace ModularMagic_Food.Helpers
{
    internal class PieceHelper
    {
        /**
         * Adds a food prefab that already carries a Piece + WearNTear to the Serving Tray (Feaster) piece table.
         * The prefab is already registered as an item, so it must not go through PieceManager.AddPiece.
         */
        public static void RegisterOnServingTray(GameObject prefab, FoodConfig config)
        {
            try
            {
                if (!config.enable.Value)
                    return;

                PieceManager.Instance.RegisterPieceInPieceTable(prefab, PieceTables.ServingTray);
                Jotunn.Logger.LogWarning($"[MMF] Serving tray piece registered: {prefab.name}");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"Could not register {prefab.name} on the serving tray: " + e);
            }
        }
    }
}
