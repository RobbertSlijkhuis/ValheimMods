using HarmonyLib;
using ModularMagic_Core.Components;
using ModularMagic_Core.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Shared imbuement API: rune registry and (de)serialization of the imbuements stored on an item.
    ///
    /// Saved format (version 1): "1:DamageSlash@2,,EitrCost@1"
    /// A version, a colon and one entry per slot separated by commas. An entry is "runeId@level" and an empty slot is empty.
    /// Slot count, slot tier and weapon type are not saved, they come from the ImbuementSlots component of the weapon.
    /// </summary>
    public static class ImbuementHelper
    {
        public const string DataKey = "Imbuements_MMC";
        // Player id of the character that put the weapon on the rune table, only this character may edit it.
        // The player id stays the same between sessions, unlike the session id of the connection.
        public const string EditorKey = "MMC_Editor";
        // The unsaved changes of the editor, in the same format as the saved imbuements. It is stored on the stand
        // so the changes (and the runes that were used for them) survive walking away and relogging.
        public const string DraftKey = "MMC_Draft";
        private const int CurrentVersion = 1;

        private static readonly Dictionary<string, ImbuementRune> runes = new Dictionary<string, ImbuementRune>();

        public static void RegisterRune(ImbuementRune rune)
        {
            if (string.IsNullOrEmpty(rune.m_id) || rune.m_id.IndexOfAny(new[] { ':', ',', '@' }) >= 0)
            {
                Jotunn.Logger.LogWarning($"[Imbuements] Rune '{rune.gameObject.name}' has a missing or invalid id: '{rune.m_id}'");
                return;
            }

            if (runes.ContainsKey(rune.m_id))
            {
                Jotunn.Logger.LogWarning($"[Imbuements] Duplicate rune '{rune.m_id}', '{rune.gameObject.name}' conflicts with '{runes[rune.m_id].gameObject.name}'");
                return;
            }

            runes[rune.m_id] = rune;
        }

        // The level of a rune is not part of the rune, it is the quality of the rune item (see DropRune)
        public static ImbuementRune FindRune(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            runes.TryGetValue(id, out ImbuementRune rune);
            return rune;
        }

        // Whether the item is a rune, runes have the ImbuementRune component on their prefab
        public static bool IsRune(ItemDrop itemDrop)
        {
            return itemDrop != null && itemDrop.GetComponent<ImbuementRune>() != null;
        }

        /// <summary>
        /// Drops a rune item with the level as its quality. The rune models and icons show the level of the item.
        /// </summary>
        public static ItemDrop DropRune(ImbuementRune rune, int level, Vector3 position, Quaternion rotation)
        {
            ItemData itemData = rune.GetComponent<ItemDrop>().m_itemData.Clone();
            // Only set on item instances, not on the item data of the prefab
            itemData.m_dropPrefab = rune.gameObject;
            itemData.m_quality = level;

            return ItemDrop.DropItem(itemData, 1, position, rotation);
        }

        // A weapon is imbuable when its drop prefab has the ImbuementSlots component
        public static ImbuementSlots GetSlots(ItemData itemData)
        {
            if (itemData == null || itemData.m_dropPrefab == null)
                return null;

            return itemData.m_dropPrefab.GetComponent<ImbuementSlots>();
        }

        public static bool HasImbuements(ItemData itemData)
        {
            return GetSlots(itemData) != null;
        }

        // Returns 0 when nobody claimed the weapon (for example a staff that was on a table before editors existed)
        public static long GetEditor(ItemData itemData)
        {
            string value = itemData.m_customData.GetValueSafe(EditorKey);

            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long editorId) ? editorId : 0;
        }

        public static void SetEditor(ItemData itemData, long editorId)
        {
            itemData.m_customData[EditorKey] = editorId.ToString(CultureInfo.InvariantCulture);
        }

        // Returns null when the editor has no unsaved changes
        public static string GetDraft(ItemData itemData)
        {
            return itemData.m_customData.GetValueSafe(DraftKey);
        }

        public static void SetDraft(ItemData itemData, string draft)
        {
            if (draft == null)
                itemData.m_customData.Remove(DraftKey);
            else
                itemData.m_customData[DraftKey] = draft;
        }

        /// <summary>
        /// The runes of the draft that are not saved yet, with the slot they are in (the rune and its level are in the
        /// imbuement). These runes were taken from the inventory of the editor, so they have to be dropped again when
        /// the staff leaves the table.
        /// </summary>
        public static List<KeyValuePair<int, Imbuement>> GetUnsavedRunes(string savedData, string draftData, ImbuementSlots slots)
        {
            List<KeyValuePair<int, Imbuement>> unsaved = new List<KeyValuePair<int, Imbuement>>();
            List<Imbuement> saved = Deserialize(savedData, slots);
            List<Imbuement> draft = Deserialize(draftData, slots);

            for (int i = 0; i < draft.Count; i++)
            {
                if (draft[i].rune != null && !draft[i].HasSameRune(saved[i]))
                    unsaved.Add(new KeyValuePair<int, Imbuement>(i, draft[i]));
            }

            return unsaved;
        }

        /// <summary>
        /// Loads the item that is currently attached to an item stand from the ZDO of the stand.
        /// This is the data every client can read, unlike the inventory item that was queued by the player.
        /// </summary>
        public static ItemData LoadItemFromZDO(ZDO zdo)
        {
            int itemHash = zdo.GetInt(ZDOVars.s_item, 0);

            if (itemHash == 0 || ObjectDB.instance == null)
                return null;

            GameObject prefab = ObjectDB.instance.GetItemPrefab(itemHash);
            ItemDrop itemDrop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;

            if (itemDrop == null)
                return null;

            ItemData itemData = itemDrop.m_itemData.Clone();
            // Only set on item instances, not on the item data of the prefab
            itemData.m_dropPrefab = prefab;
            ItemDrop.LoadFromZDO(itemData, zdo);

            return itemData;
        }

        public static List<Imbuement> Read(ItemData itemData)
        {
            ImbuementSlots slots = GetSlots(itemData);

            if (slots == null)
                return new List<Imbuement>();

            return Deserialize(itemData.m_customData.GetValueSafe(DataKey), slots);
        }

        public static string Serialize(List<Imbuement> imbuements)
        {
            List<string> entries = new List<string>();

            foreach (Imbuement imbuement in imbuements)
            {
                entries.Add(string.IsNullOrEmpty(imbuement.runeId) ? "" : $"{imbuement.runeId}@{imbuement.level}");
            }

            return new StringBuilder().Append(CurrentVersion).Append(':').Append(string.Join(",", entries)).ToString();
        }

        public static List<Imbuement> Deserialize(string value, ImbuementSlots slots)
        {
            List<Imbuement> imbuements = new List<Imbuement>();

            for (int i = 0; i < slots.m_slots; i++)
            {
                imbuements.Add(new Imbuement(slots.m_tier, slots.m_weaponType));
            }

            if (string.IsNullOrEmpty(value))
                return imbuements;

            int separator = value.IndexOf(':');

            if (value.Contains("|") || separator < 0 || !int.TryParse(value.Substring(0, separator), out int version) || version != CurrentVersion)
            {
                Jotunn.Logger.LogWarning($"[Imbuements] Unknown or old imbuement format, treating the slots as empty: {value}");
                return imbuements;
            }

            string[] entries = value.Substring(separator + 1).Split(',');

            for (int i = 0; i < entries.Length && i < imbuements.Count; i++)
            {
                if (entries[i] == "")
                    continue;

                int levelSeparator = entries[i].LastIndexOf('@');

                if (levelSeparator < 0 || !int.TryParse(entries[i].Substring(levelSeparator + 1), out int level))
                {
                    Jotunn.Logger.LogWarning($"[Imbuements] Skipping malformed imbuement entry: {entries[i]}");
                    continue;
                }

                string id = entries[i].Substring(0, levelSeparator);

                ImbuementRune rune = FindRune(id);

                if (rune == null || !rune.IsValidLevel(level))
                {
                    Jotunn.Logger.LogWarning($"[Imbuements] Skipping unknown rune or level: {entries[i]}");
                    continue;
                }

                imbuements[i].runeId = id;
                imbuements[i].level = level;
                imbuements[i].saved = true;
            }

            return imbuements;
        }
    }
}
