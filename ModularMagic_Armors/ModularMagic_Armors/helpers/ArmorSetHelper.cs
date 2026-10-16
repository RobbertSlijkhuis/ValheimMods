using ModularMagic_Armors.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    /**
     * Answers "does this player wear the full set" for every client.
     * Status effects are not replicated to other players, but the worn item hashes are: VisEquipment reads them from the ZDO of every player.
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
        private static int diagnosticsLeft = 3;

        public static void Init()
        {
            sets.Clear();

            Models.CustomPrefabs prefabs = ModularMagic_Armors.Instance.prefabs;

            Register(ArmorSetId.Shaman, prefabs.ShamanHelmetPrefab, prefabs.ShamanCapePrefab, prefabs.ShamanChestPrefab, prefabs.ShamanLegsPrefab);
            Register(ArmorSetId.Wraith, prefabs.WraithHelmetPrefab, prefabs.WraithCapePrefab, prefabs.WraithChestPrefab, prefabs.WraithLegsPrefab);
            Register(ArmorSetId.FrostWolf, prefabs.FrostWolfHelmetPrefab, prefabs.FrostWolfCapePrefab, prefabs.FrostWolfChestPrefab, prefabs.FrostWolfLegsPrefab);
            Register(ArmorSetId.DarkWizard, prefabs.DarkWizardHelmetPrefab, prefabs.DarkWizardCapePrefab, prefabs.DarkWizardChestPrefab, prefabs.DarkWizardLegsPrefab);
        }

        public static bool IsSetWorn(ArmorSetId set, VisEquipment visEquipment)
        {
            if (visEquipment == null)
                return false;

            return IsSetWorn(set, visEquipment.m_nview);
        }

        /**
         * False when there is no valid ZNetView/ZDO, which is the case for the player preview in the main menu.
         */
        public static bool IsSetWorn(ArmorSetId set, ZNetView netView)
        {
            if (netView == null || !netView.IsValid() || !sets.TryGetValue(set, out SetPieces pieces))
                return false;

            ZDO zdo = netView.GetZDO();

            // Helmet first, it is the cheapest way to bail out for players not wearing the set (this runs every frame per player)
            int helmet = zdo.GetInt(ZDOVars.s_helmetItem);

            if (diagnosticsLeft > 0 && helmet != 0)
            {
                diagnosticsLeft--;
                Jotunn.Logger.LogWarning($"[ArmorSet] ZDO helmet hash {helmet}, {set} helmet hash {pieces.helmet}");
            }

            if (helmet != pieces.helmet)
                return false;

            return zdo.GetInt(ZDOVars.s_chestItem) == pieces.chest
                && zdo.GetInt(ZDOVars.s_legItem) == pieces.legs
                && zdo.GetInt(ZDOVars.s_shoulderItem) == pieces.shoulder;
        }

        // Temporary diagnostic
        public static string Describe(ArmorSetId set, VisEquipment visEquipment)
        {
            if (visEquipment == null || visEquipment.m_nview == null || !visEquipment.m_nview.IsValid() || !sets.TryGetValue(set, out SetPieces pieces))
                return "no valid ZDO";

            ZDO zdo = visEquipment.m_nview.GetZDO();

            return $"worn helmet/shoulder/chest/legs = {zdo.GetInt(ZDOVars.s_helmetItem)}/{zdo.GetInt(ZDOVars.s_shoulderItem)}/{zdo.GetInt(ZDOVars.s_chestItem)}/{zdo.GetInt(ZDOVars.s_legItem)}, expected {pieces.helmet}/{pieces.shoulder}/{pieces.chest}/{pieces.legs}";
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

            Jotunn.Logger.LogWarning($"[ArmorSet] Registered {set}: {helmet.name} {sets[set].helmet}, {shoulder.name} {sets[set].shoulder}, {chest.name} {sets[set].chest}, {legs.name} {sets[set].legs}");
        }
    }
}
