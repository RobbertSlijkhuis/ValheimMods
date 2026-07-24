using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class TwitchCreatureAssignment
    {

        public GameObject creature;
        public int duration;
        public string userName;

        // Stable sort key for TwitchChatting's live per-owner claim index (!unclaim <index>) -
        // never persisted, so a claim that unloads/reloads never carries a stale reserved number.
        public DateTime claimedAt = DateTime.Now;

        public TwitchCreatureAssignment(string userName, GameObject creature, int duration)
        {

            this.creature = creature;
            this.duration = duration;
            this.userName = userName;
        }
    }
}
