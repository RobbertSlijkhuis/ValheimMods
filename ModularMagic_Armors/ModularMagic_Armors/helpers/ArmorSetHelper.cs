using ModularMagic_Armors.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    /**
     * Answers "does this player wear the full set" for every client.
     * Status effects are not replicated to other players, but the worn item hashes are: VisEquipment reads them from the ZDO of every player
     * (and from its own stored hashes when there is no ZDO, as in the main-menu preview).
     * Pieces are identified by the hash of their prefab name, the same hash vanilla writes (m_dropPrefab.name).
     */
    internal class ArmorSetHelper
    {
        private class SetPieces
        {
            public int helmet;
            public int shoulder;
            public int chest;
            public int legs;
        }

        private static readonly Dictionary<ArmorSetId, SetPieces> sets = new Dictionary<ArmorSetId, SetPieces>();

        public static void Init()
        {
            sets.Clear();

            Models.CustomPrefabs prefabs = ModularMagic_Armors.Instance.prefabs;

            Register(ArmorSetId.Shaman, prefabs.ShamanHelmetPrefab, prefabs.ShamanCapePrefab, prefabs.ShamanChestPrefab, prefabs.ShamanLegsPrefab);
            Register(ArmorSetId.Wraith, prefabs.WraithHelmetPrefab, prefabs.WraithCapePrefab, prefabs.WraithChestPrefab, prefabs.WraithLegsPrefab);
            Register(ArmorSetId.FrostWolf, prefabs.FrostWolfHelmetPrefab, prefabs.FrostWolfCapePrefab, prefabs.FrostWolfChestPrefab, prefabs.FrostWolfLegsPrefab);
            Register(ArmorSetId.DarkWizard, prefabs.DarkWizardHelmetPrefab, prefabs.DarkWizardCapePrefab, prefabs.DarkWizardChestPrefab, prefabs.DarkWizardLegsPrefab);
        }

        /**
         * Reads the worn pieces from the same place VisEquipment does: the ZDO, and its own stored hashes when there is no ZDO.
         * The second case is the player preview in the main menu, where the set effects are shown too.
         */
        public static bool IsSetWorn(ArmorSetId set, VisEquipment visEquipment)
        {
            if (visEquipment == null || !sets.TryGetValue(set, out SetPieces pieces))
                return false;

            ZNetView netView = visEquipment.m_nview;
            ZDO zdo = netView != null ? netView.GetZDO() : null;

            if (zdo != null)
            {
                // Helmet first, it is the cheapest way to bail out for players not wearing the set (this runs every frame per player)
                if (zdo.GetInt(ZDOVars.s_helmetItem) != pieces.helmet)
                    return false;

                return zdo.GetInt(ZDOVars.s_chestItem) == pieces.chest
                    && zdo.GetInt(ZDOVars.s_legItem) == pieces.legs
                    && zdo.GetInt(ZDOVars.s_shoulderItem) == pieces.shoulder;
            }

            return visEquipment.m_helmetItem == pieces.helmet
                && visEquipment.m_chestItem == pieces.chest
                && visEquipment.m_legItem == pieces.legs
                && visEquipment.m_shoulderItem == pieces.shoulder;
        }

        /**
         * False when there is no valid ZNetView/ZDO. Used where only the network object is available (footsteps).
         */
        public static bool IsSetWorn(ArmorSetId set, ZNetView netView)
        {
            if (netView == null || !netView.IsValid() || !sets.TryGetValue(set, out SetPieces pieces))
                return false;

            ZDO zdo = netView.GetZDO();

            // Helmet first, it is the cheapest way to bail out for players not wearing the set (this runs every frame per player)
            int helmet = zdo.GetInt(ZDOVars.s_helmetItem);

            if (helmet != pieces.helmet)
                return false;

            return zdo.GetInt(ZDOVars.s_chestItem) == pieces.chest
                && zdo.GetInt(ZDOVars.s_legItem) == pieces.legs
                && zdo.GetInt(ZDOVars.s_shoulderItem) == pieces.shoulder;
        }

        private static void Register(ArmorSetId set, GameObject helmet, GameObject shoulder, GameObject chest, GameObject legs)
        {
            sets[set] = new SetPieces()
            {
                helmet = helmet.name.GetStableHashCode(),
                shoulder = shoulder.name.GetStableHashCode(),
                chest = chest.name.GetStableHashCode(),
                legs = legs.name.GetStableHashCode(),
            };
        }
    }
}
