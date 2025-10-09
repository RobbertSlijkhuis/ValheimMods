using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchChatting : MonoBehaviour
    {
        // DONE: Detect objects around the player
        // DONE: Filter whatever so its only creatures
        // DONE: Get a list of Twitch users
        // DONE: Attach Twitch user to the creature
        // DONE: Add NpcTalk component to that creature
        // DONE: Check if Twitch said anything in chat ever few seconds
        // DONE: Retrieve the last message of that Twitch user
        // DONE: Make creature talk
        // DONE: Once a creature has a user assigned, exlude it from the list
        // DONE: Prevent the creature from spamming the same message
        // DONE: Prevent the user to be attached to multiple creatures
        // DONE: Unassign user when creature is killed or user hasn't messaged for x amount seconds

        private TwitchAuth m_twitchAuth;
        private TwitchChat m_twitchChat;
        private Dictionary<string, GameObject> m_assigned = new Dictionary<string, GameObject>();

        private void Awake()
        {
            m_twitchAuth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            m_twitchChat = Game.instance.gameObject.GetComponent<TwitchChat>();
            InvokeRepeating(nameof(DetectCreaturesAndAssignUsers), 0f, 3f);
        }

        public void RemoveAssignedUser(string author)
        {
            m_assigned.Remove(author);
        }

        private void DetectCreaturesAndAssignUsers()
        {
            if (!m_twitchAuth.isLoggedIn)
                return;

            List<GameObject> creatures = new List<GameObject>();
            Collider[] objects = Physics.OverlapSphere(transform.position, 30f);

            foreach (Collider obj in objects)
            {
                MonsterAI monsterComp = obj.gameObject.GetComponent<MonsterAI>();
                AnimalAI animalComp = obj.gameObject.GetComponent<AnimalAI>();
                Fish fishComp = obj.gameObject.GetComponent<Fish>();
                bool hasTwitchUser = obj.gameObject.GetComponent<TwitchCheckForMessage>() != null;

                if (hasTwitchUser || (monsterComp == null && animalComp == null && fishComp == null))
                {
                    continue;
                }

                List<string> authors = m_twitchChat.GetAuthorsInChat();
                authors.RemoveAll(item => m_assigned.ContainsKey(item));

                if (authors.Count == 0)
                {
                    Jotunn.Logger.LogWarning("No authors found");
                    return;
                }

                int index = Random.Range(0, authors.Count);
                string chosen = authors[index];

                TwitchCheckForMessage messageComp = obj.gameObject.AddComponent<TwitchCheckForMessage>();
                messageComp.Init(chosen);
                m_assigned.Add(chosen, obj.gameObject);

                Jotunn.Logger.LogWarning("Creature:" + obj.name);
            }
        }
    }
}
