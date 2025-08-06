#nullable enable
using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class BuildPieceConfigOptions
    {
        public GameObject prefab;
        public string sectionName;
        public string recipeName;

        public bool enable = true;
        public string name;
        public string? description;
        public string? craftingStation;
        public string recipe;
        public string? lightColorPreset;

        public BuildPieceConfigOptions(GameObject prefab, string name, string recipe)
        {
            this.prefab = prefab;
            this.name = name;
            sectionName = $"{name.Replace("'", "")}";
            this.recipe = recipe;
            recipeName = $"Recipe_{prefab.name}";
        }
    }
}
