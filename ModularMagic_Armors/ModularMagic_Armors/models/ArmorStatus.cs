using System.Collections.Generic;

namespace ModularMagic_Utilities.Models
{
    internal class ArmorStatus
    {
        public long playerId;
        public int? set;
        public List<int> items;

        public ArmorStatus(long playerId, int? set, List<int> items)
        {
            this.playerId = playerId;
            this.set = set;
            this.items = items;
        }
    }
}
