namespace ModularMagic_Utilities.Models
{
    internal class LanternStatus
    {
        public long playerId;
        public bool status;

        public LanternStatus(long playerId, bool status)
        {
            this.playerId = playerId;
            this.status = status;
        }
    }
}
