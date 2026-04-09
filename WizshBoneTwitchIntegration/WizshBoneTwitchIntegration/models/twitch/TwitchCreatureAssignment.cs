using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchCreatureAssignment
    {

        public GameObject creature;
        public int duration;
        public string userName;

        public TwitchCreatureAssignment(string userName, GameObject creature, int duration)
        {

            this.creature = creature;
            this.duration = duration;
            this.userName = userName;
        }
    }
}
