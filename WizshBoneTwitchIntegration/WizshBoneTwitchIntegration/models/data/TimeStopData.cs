
namespace WizshBoneTwitchIntegration.Models
{
    internal class TimeStopData : CloneableData
    {
        public float duration = 30f;

        public bool freezeEnemies = true;

        public bool freezePlayer = false;

        public bool freezeProjectiles = true;

        public float radius = 10f;

        public string announceMessage = "";

        public TimeStopData() { }
    }
}
