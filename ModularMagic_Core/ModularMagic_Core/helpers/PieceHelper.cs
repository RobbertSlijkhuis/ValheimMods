using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using ModularMagic_Core.Configs;
using ModularMagic_Core.Models;
using System;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class PieceHelper
    {
        public static void Create(GameObject prefab, BuildPieceConfig config, bool renderIcon = false)
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
                pieceConfig.AddRequirement("Stone", 20);
                pieceConfig.AddRequirement("MMC_EitrCrude", 12);
                pieceConfig.AddRequirement("Copper", 8);
                pieceConfig.AddRequirement("Tin", 2);

                if (renderIcon)
                {
                    RenderManager.RenderRequest request = new RenderManager.RenderRequest(prefab);
                    request.Rotation = RenderManager.IsometricRotation;
                    // request.UseCache = true;
                    pieceConfig.Icon = RenderManager.Instance.Render(request);
                }

                //RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

                //if (requirements == null || requirements.Length == 0)
                //    Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
                //else
                //    pieceConfig.Requirements = requirements;

                PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, pieceConfig));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not create build piece: " + e);
            }
        }
    }
}
