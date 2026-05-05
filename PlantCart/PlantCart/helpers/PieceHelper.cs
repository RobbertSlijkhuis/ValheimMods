using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using PlantCart.Configs;
using System;
using UnityEngine;

namespace PlantCart.Helpers
{
    internal class PieceHelper
    {
        public static void Create(GameObject prefab, BuildPieceConfig config)
        {
            try
            {
                PieceConfig pieceConfig = new PieceConfig();
                pieceConfig.Enabled = config.enable.Value;
                pieceConfig.Name = config.name.Value;
                pieceConfig.Description = config.description.Value;
                pieceConfig.CraftingStation = config.craftingStation.Value;
                pieceConfig.PieceTable = PieceTables.Hammer;
                pieceConfig.Category = PieceCategories.Misc;

                RequirementConfig[] requirements = RecipeHelper.GetAsRequirementConfigArray(config.recipe.Value, null, null);

                if (requirements == null || requirements.Length == 0)
                    Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
                else
                    pieceConfig.Requirements = requirements;

                UpdateEnabled(prefab, config.enable.Value);
                UpdateName(prefab, config.name.Value);
                UpdateDescription(prefab, config.name.Value);
                UpdateCraftingStation(prefab, config.craftingStation.Value);
                UpdateRecipe(prefab, config.recipe.Value);
                UpdateInventorySize(prefab, config.inventoryColumns.Value, config.inventoryRows.Value);
                UpdateMass(prefab, config.cartMass.Value);

                PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, pieceConfig));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not create build piece: " + e);
            }
        }

        public static void UpdateEnabled(GameObject prefab, bool value)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                Piece comp = prefab.GetComponent<Piece>();

                if (comp == null)
                    throw new Exception("Could not find Piece component");

                comp.m_enabled = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update piece enabled: " + e);
            }
        }

        public static void UpdateName(GameObject prefab, string value)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                Piece piece = prefab.GetComponent<Piece>();
                Vagon vagon = prefab.GetComponent<Vagon>();

                if (piece == null || vagon == null)
                    throw new Exception("Could not find Piece or Vagon component");

                piece.m_name = value;
                vagon.m_name = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update piece name: " + e);
            }
        }

        public static void UpdateDescription(GameObject prefab, string value)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                Piece comp = prefab.GetComponent<Piece>();

                if (comp == null)
                    throw new Exception("Could not find Piece component");

                comp.m_description = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update piece description: " + e);
            }
        }

        public static void UpdateCraftingStation(GameObject prefab, string value)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                Piece comp = prefab.GetComponent<Piece>();

                if (comp == null)
                    throw new Exception("Could not find Piece component");

                string pieceName = CraftingStations.GetInternalName(value);
                CraftingStation craftingStation = PrefabManager.Instance.GetPrefab(pieceName).GetComponent<CraftingStation>();

                if (craftingStation == null)
                    throw new Exception("Could not find CraftingStation component");

                comp.m_craftingStation = craftingStation;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update piece crafting station: " + e);
            }
        }

        public static void UpdateRecipe(GameObject prefab, string value)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                Piece comp = prefab.GetComponent<Piece>();

                if (comp == null)
                    throw new Exception("Could not find Piece component");

                Piece.Requirement[] requirements = RecipeHelper.GetAsPieceRequirementArray(value, null, null);

                if (requirements == null || requirements.Length == 0)
                    Jotunn.Logger.LogWarning($"Could not resolve recipe for: {prefab.name}");
                else
                    comp.m_resources = requirements;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update piece recipe: " + e);
            }
        }

        public static void UpdateInventorySize(GameObject prefab, int width, int height)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                if (width <= 0 || height <= 0)
                {
                    Jotunn.Logger.LogWarning("Container size cannot be smaller then 1x1!");
                    return;
                }

                Transform containerTransform = prefab.transform.Find("Container");

                if (containerTransform == null)
                    throw new Exception("Could not find Container object");

                Container container = containerTransform.GetComponent<Container>();

                if (container == null)
                    throw new Exception("Could not find Container component");

                container.m_width = width;
                container.m_height = height;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update inventory size: " + e);
            }
        }

        public static void UpdateMass(GameObject prefab, float value)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                Vagon comp = prefab.GetComponent<Vagon>();

                if (comp == null)
                    throw new Exception("Could not find Vagon component");

                comp.m_baseMass = value;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update piece mass: " + e);
            }
        }
    }
}
