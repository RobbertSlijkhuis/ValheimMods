using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchChatting : MonoBehaviour
    {
        private TwitchAuth m_twitchAuth;
        private TwitchChat m_twitchChat;
        private List<TwitchCreatureAssignment> m_assigned = new List<TwitchCreatureAssignment>();
        private List<string> m_blacklist = new List<string>();
        private float m_scanRadius = 30f;
        public bool isEnabled;

        private void Awake()
        {
            m_twitchAuth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            m_twitchChat = Game.instance.gameObject.GetComponent<TwitchChat>();
            isEnabled = PluginConfig.configChattingEnabled.Value;

            DeserializeBlackList(PluginConfig.configChattingBlackList.Value);
            InvokeRepeating(nameof(DetectCreaturesAndAssignUsers), 0f, 3f);
        }


        public void AddAssignedUser(string author, GameObject creature)
        {
            m_assigned.Add(new TwitchCreatureAssignment(author, creature));
        }

        public void ClearAllAssignedUsers()
        {
            foreach (TwitchCreatureAssignment assignment in m_assigned)
            {
                RemoveAssignedUser(assignment);
            }
        }

        public bool ContainsAssignedUser(string author)
        {
            return m_assigned.Find(item => item.author == author) != null;
        }

        public List<TwitchCreatureAssignment> GetAllAssignedUsers()
        {
            return m_assigned;
        }

        public TwitchCreatureAssignment GetAssignedUser(string author)
        {
            return m_assigned.Find(item => item.author == author);
        }

        public void RemoveAssignedUser(TwitchCreatureAssignment entry)
        {
            if (entry != null)
            {
                TwitchCreatureClaim claim = entry.creature.GetComponent<TwitchCreatureClaim>();

                if (claim != null)
                    claim.UnassignClaim();

                m_assigned.Remove(entry);
            }
        }

        public void RemoveAssignedUser(string author)
        {
            TwitchCreatureAssignment entry = m_assigned.Find(item => item.author == author);
            RemoveAssignedUser(entry);
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
                List<string> assigned = m_assigned.Select(item => item.author).ToList();
                authors.RemoveAll(item => assigned.Contains(item));

                if (authors.Count == 0)
                {
                    // Jotunn.Logger.LogWarning("No authors found");
                    return;
                }

                int index = Random.Range(0, authors.Count);
                string chosen = authors[index];

                TwitchCreatureClaim newMonsterClaim = obj.gameObject.AddComponent<TwitchCreatureClaim>();
                newMonsterClaim.Init(chosen);
                // Jotunn.Logger.LogWarning($"Creature {obj.name} is now claimed by {chosen}");
            }
        }

        public List<string> GetBlacklist()
        {
            return m_blacklist;
        }

        public void DeserializeBlackList(string value)
        {
            string[] data = value.Trim().Split(',');
            m_blacklist = new List<string>();
            
            foreach (string entry in data)
            {
                m_blacklist.Add(entry.ToLower());
            }
        }
    }
}
