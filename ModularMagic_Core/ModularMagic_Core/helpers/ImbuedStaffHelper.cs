using System;
using System.Collections.Generic;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The imbuable staffs of the staff mods. A staff mod registers once which items are its staffs and how the runes are applied
    /// to one. Core then applies the runes when a player equips such a staff (the item data of a staff is changed in place, so the
    /// runes have to be applied again on every equip), and again on the staff in the hand when a config value changed.
    /// </summary>
    public static class ImbuedStaffHelper
    {
        private class Entry
        {
            public Func<ItemData, bool> isStaff;
            public Action<ItemData> apply;
        }

        private static readonly List<Entry> entries = new List<Entry>();

        /// <summary>
        /// Set when the game has started. The equip patch also runs while the player is being loaded, and the staffs of a mod
        /// are not ready by then.
        /// </summary>
        public static bool GameIsReady { get; internal set; }

        /// <summary>
        /// Registers the staffs of a mod. Call it once at startup (in Awake).
        /// </summary>
        /// <param name="isStaff">Whether an item is an imbuable staff of this mod. Other staff mods share the imbuement API, so each only handles its own.</param>
        /// <param name="apply">Applies the runes of the staff to its item data.</param>
        public static void Register(Func<ItemData, bool> isStaff, Action<ItemData> apply)
        {
            if (isStaff == null || apply == null)
            {
                Jotunn.Logger.LogWarning("[Imbuements] Cannot register staffs without isStaff and apply");
                return;
            }

            entries.Add(new Entry() { isStaff = isStaff, apply = apply });
        }

        /// <summary>
        /// Applies the runes again on the staff in the hand of the local player, so a changed config value reaches it together with the runes.
        /// </summary>
        public static void ReapplyOnHeldStaff()
        {
            try
            {
                Player player = Player.m_localPlayer;

                if (player == null || !GameIsReady)
                    return;

                ApplyAndRefreshPose(player, player.GetCurrentWeapon());
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not apply the imbuements to the staff in the hand: " + e);
            }
        }

        internal static void OnEquipped(Humanoid humanoid, ItemData item)
        {
            try
            {
                if (humanoid == null || !humanoid.IsPlayer() || item == null || !GameIsReady)
                    return;

                ApplyAndRefreshPose(humanoid, item);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem: " + e);
            }
        }

        // The game sets how the character holds its weapon (the animation state of the item) when the weapon is equipped, before the runes
        // are applied in the postfix of EquipItem. A rune that changes how the weapon is held (Greatsword) would only show the next time the
        // equipment is set up, so it is set again here when the runes changed it. The state is synced to the other clients by the animator
        private static void ApplyAndRefreshPose(Humanoid humanoid, ItemData item)
        {
            if (item == null)
                return;

            ItemData.AnimationState before = item.m_shared.m_animationState;

            ApplyIfStaff(item);

            if (item.m_shared.m_animationState != before && (humanoid.m_rightItem == item || humanoid.m_leftItem == item))
                humanoid.SetupAnimationState();
        }

        private static void ApplyIfStaff(ItemData item)
        {
            if (item == null || !ImbuementHelper.HasImbuements(item))
                return;

            foreach (Entry entry in entries)
            {
                if (!entry.isStaff(item))
                    continue;

                entry.apply(item);
                return;
            }
        }
    }
}
