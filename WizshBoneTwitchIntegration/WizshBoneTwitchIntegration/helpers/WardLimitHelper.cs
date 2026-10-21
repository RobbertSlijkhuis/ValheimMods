using System.Collections.Generic;
using System.Linq;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Backs the "Max Twitch Wards" setting: counts the Twitch Wards that exist in the world and
    /// decides whether the local player may place another one (see
    /// harmony/SafeZonePatchesWBTI.cs's TryPlacePiece prefix).
    /// </summary>
    internal static class WardLimitHelper
    {
        /// <summary>True when <paramref name="piece"/> is the Twitch Ward's build piece.</summary>
        public static bool IsWardPiece(Piece piece)
        {
            return piece != null
                && Utils.GetPrefabName(piece.gameObject) == WizshBoneTwitchIntegration.Instance.prefabs.GuardStone.name;
        }

        /// <summary>
        /// True when the configured limit (0 = unlimited) is already used up. Counts every ward
        /// ZDO this peer knows about, loaded or not - complete for a host or single-player game, but
        /// a client on a dedicated server only knows the ZDOs of the sectors around it, so it can
        /// under-count there.
        /// </summary>
        public static bool IsLimitReached(out int max)
        {
            max = ProfileSettingsHelper.Current.wardMaxCount;

            if (max <= 0 || ZDOMan.instance == null)
                return false;

            return CountWards() >= max;
        }

        private static int CountWards()
        {
            List<ZDO> wards = new List<ZDO>();
            int index = 0;

            // The iterative overload works through the sectors in time-boxed slices; keep calling
            // until it reports it has covered them all.
            while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(WizshBoneTwitchIntegration.Instance.prefabs.GuardStone.name, wards, ref index))
            {
            }

            // The iterative overload can add a ZDO more than once (it re-scans a sector after
            // cutting a slice short, and its final pass re-adds sector 0), so count distinct ones.
            return wards.Distinct().Count();
        }
    }
}
