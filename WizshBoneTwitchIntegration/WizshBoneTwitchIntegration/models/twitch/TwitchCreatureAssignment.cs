using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchCreatureAssignment
    {
        public string author;
        public GameObject creature;

        public TwitchCreatureAssignment(string author, GameObject creature)
        {
            this.author = author;
            this.creature = creature;
        }
    }
}
