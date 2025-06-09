using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Utilities.Configs;
using ModularMagic_Utilities.Models;
using ModularMagic_Utilities.StatusEffects;
using System;
using UnityEngine;

namespace ModularMagic_Utilities.Helpers
{
    internal class PieceHelper
    {
        public static void Create(GameObject prefab, BuildPieceConfig config, bool isWeatherZone = false)
        {
            try
            {
                PieceConfig pieceConfig = new PieceConfig();
                pieceConfig.Enabled = config.enable.Value;
                pieceConfig.Name = config.name.Value;
                pieceConfig.Description = config.description.Value;
                pieceConfig.CraftingStation = config.craftingStation.Value;
                pieceConfig.PieceTable = PieceTables.Hammer;
                pieceConfig.Category = PieceCategories.Furniture;
                RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

                if (requirements == null || requirements.Length == 0)
                    Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
                else
                    pieceConfig.Requirements = requirements;

                PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, pieceConfig));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not create build piece: " + e);
            }
        }
    }
}
