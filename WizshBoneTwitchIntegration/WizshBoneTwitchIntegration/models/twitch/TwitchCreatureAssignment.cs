using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchCreatureAssignment
    {
        public string userName;
        public GameObject creature;

        public TwitchCreatureAssignment(string userName, GameObject creature)
        {
            this.userName = userName;
            this.creature = creature;
        }
    }
}
