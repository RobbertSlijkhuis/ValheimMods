using ModularMagic_Core.Helpers;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    /// <summary>
    /// Shows the model of the level of a rune item. The level is the quality of the item, which is stored in the ZDO
    /// of the item, so every client shows the same model.
    /// </summary>
    internal class RuneItemModel : MonoBehaviour
    {
        // Start instead of Awake, the ItemDrop loads its quality in its own Awake, and a dropped rune gets its quality right after it is created
        public void Start()
        {
            ItemDrop itemDrop = GetComponent<ItemDrop>();
            Transform attach = transform.Find("attach");

            if (itemDrop == null || attach == null)
                return;

            if (RuneModelHelper.ShowModel(attach, itemDrop.m_itemData.m_quality) == null)
                Jotunn.Logger.LogWarning($"[Imbuements] Rune '{name}' has no model for level {itemDrop.m_itemData.m_quality}");
        }
    }
}
