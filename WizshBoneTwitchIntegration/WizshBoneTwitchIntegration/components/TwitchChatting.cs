using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.components;
using WizshBoneTwitchIntegration.Configs;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchChatting : MonoBehaviour
    {
        private TwitchAuth m_twitchAuth;
        private TwitchChat m_twitchChat;
        private List<string> m_assigned = new List<string>();
        private float m_scanRadius = 30f;
        public bool isEnabled;

        private void Awake()
        {
            m_twitchAuth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            m_twitchChat = Game.instance.gameObject.GetComponent<TwitchChat>();
            isEnabled = PluginConfig.configChattingEnabled.Value;

            InvokeRepeating(nameof(DetectCreaturesAndAssignUsers), 0f, 3f);
        }

        public void AddAssignedUser(string author)
        {
            m_assigned.Add(author);
        }

        public void RemoveAssignedUser(string author)
        {
            m_assigned.Remove(author);
        }

        public void ClearAssignedUsers()
        {
            m_assigned.Clear();
        }

        private void DetectCreaturesAndAssignUsers()
        {
            if (!m_twitchAuth.isLoggedIn || !isEnabled)
                return;

            List<GameObject> creatures = new List<GameObject>();
            Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, m_scanRadius);

            foreach (Collider obj in objects)
            {
                TwitchCreatureClaim monsterClaim = obj.GetComponent<TwitchCreatureClaim>();
                Humanoid humanoid = obj.gameObject.GetComponent<Humanoid>();
                MonsterAI monsterAI = obj.gameObject.GetComponent<MonsterAI>();
                bool isClaimed = monsterClaim != null;

                if (monsterAI == null || isClaimed)
                    continue;

                List<string> authors = m_twitchChat.GetAuthorsInChat();
                authors.RemoveAll(item => m_assigned.Contains(item));

                if (authors.Count == 0)
                {
                    Jotunn.Logger.LogWarning("No authors found");
                    return;
                }

                int index = Random.Range(0, authors.Count);
                string chosen = authors[index];

                TwitchCreatureClaim newMonsterClaim = obj.gameObject.AddComponent<TwitchCreatureClaim>();
                newMonsterClaim.Init(chosen);
                Jotunn.Logger.LogWarning($"Creature {obj.name} is now claimed by {chosen}");
            }
        }
    }
}
