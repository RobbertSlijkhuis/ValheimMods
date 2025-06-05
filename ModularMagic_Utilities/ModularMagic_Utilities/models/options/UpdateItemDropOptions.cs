#nullable enable

using UnityEngine;

namespace ModularMagic_Utilities.Models
{
    internal class UpdateItemDropOptions
    {
        public string? name = null;
        public string? description = null;
        public StatusEffect? equipStatusEffect = ScriptableObject.CreateInstance<StatusEffect>();
        public float? weight = null;
        public float? eitrRegen = null;
        public float? demister = null;

        public UpdateItemDropOptions()
        {
            equipStatusEffect.name = "empty_MMA";
        }
    }
}
