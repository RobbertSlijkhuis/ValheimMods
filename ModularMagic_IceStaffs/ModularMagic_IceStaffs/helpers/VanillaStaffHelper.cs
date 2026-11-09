using Jotunn.Managers;
using UnityEngine;

namespace ModularMagic_IceStaffs.Helpers
{
    /**
     * Enables or disables the crafting of the vanilla Ice staff (StaffIceShards). Only the recipe is switched,
     * staffs that already exist keep working.
     */
    internal static class VanillaStaffHelper
    {
        private const string VanillaStaffName = "StaffIceShards";

        public static void SetRecipeEnabled(bool enabled)
        {
            try
            {
                if (ObjectDB.instance == null)
                    return;

                GameObject prefab = PrefabManager.Instance.GetPrefab(VanillaStaffName);
                ItemDrop itemDrop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;

                if (itemDrop == null)
                {
                    Jotunn.Logger.LogWarning($"[VanillaStaff] Could not find the prefab {VanillaStaffName}");
                    return;
                }

                Recipe recipe = ObjectDB.instance.GetRecipe(itemDrop.m_itemData);

                if (recipe == null)
                {
                    Jotunn.Logger.LogWarning($"[VanillaStaff] Could not find the recipe of {VanillaStaffName}");
                    return;
                }

                recipe.m_enabled = enabled;
            }
            catch (System.Exception error)
            {
                Jotunn.Logger.LogError($"Could not set the recipe of {VanillaStaffName}: {error}");
            }
        }
    }
}
