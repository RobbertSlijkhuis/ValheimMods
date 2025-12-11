using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchChatting : MonoBehaviour
    {
        private TwitchAuth m_auth;
        private TwitchChat m_chat;
        private List<TwitchCreatureAssignment> m_creatureAssignments = new List<TwitchCreatureAssignment>();
        private List<string> m_userBlacklist = new List<string>();
        public bool m_enabled;

        private float m_scanRadius = 30f;
        private float m_scanInterval = 15f;
        private string m_chosenUser;
        private GameObject m_chosenPrefab;

        private void Awake()
        {
            m_auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
            m_chat = Game.instance.gameObject.GetComponent<TwitchChat>();
            m_enabled = PluginConfig.configChattingEnabled.Value;

            DeserializeUserBlackList(PluginConfig.configChattingBlackList.Value);
            InvokeRepeating(nameof(DetectCreaturesAndAssignUsers), 0f, m_scanInterval);
        }

        public void AddCreatureAssignment(TwitchCreatureAssignment assignment)
        {
            m_creatureAssignments.Add(assignment);
        }

        public void RemoveCreatureAssignment(TwitchCreatureAssignment assignment)
        {
            if (assignment == null)
            {
                Jotunn.Logger.LogWarning("Could not find creature assignment to remove!");
                return;
            }

            if (assignment.creature != null)
            {
                TwitchCreatureClaim creatureClaim = assignment.creature.GetComponent<TwitchCreatureClaim>();

                if (creatureClaim != null)
                    Destroy(creatureClaim);
            }

            m_creatureAssignments.Remove(assignment);
        }

        public void RemoveCreatureAssignment(string userName)
        {
            TwitchCreatureAssignment assignment = m_creatureAssignments.Find(item => item.userName == userName);
            RemoveCreatureAssignment(assignment);
        }

        public bool ContainsCreatureAssignment(TwitchCreatureAssignment assignment)
        {
            return m_creatureAssignments.Contains(assignment);
        }

        public bool ContainsCreatureAssignment(string userName)
        {
            return m_creatureAssignments.Find(item => item.userName == userName) != null;
        }

        public List<TwitchCreatureAssignment> GetAllCreatureAssignments()
        {
            return m_creatureAssignments;
        }

        public TwitchCreatureAssignment GetCreatureAssignment(string userName)
        {
            return m_creatureAssignments.Find(item => item.userName == userName);
        }

        private void DetectCreaturesAndAssignUsers()
        {
            if (!m_auth.isLoggedIn || !m_enabled)
                return;

            Jotunn.Logger.LogWarning("DetectCreaturesAndAssignUsers()");
            List<GameObject> creatures = new List<GameObject>();
            Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, m_scanRadius);

            foreach (Collider obj in objects)
            {
                TwitchCreatureClaim creatureClaim = obj.GetComponent<TwitchCreatureClaim>();
                Humanoid humanoid = obj.gameObject.GetComponent<Humanoid>();
                MonsterAI monsterAI = obj.gameObject.GetComponent<MonsterAI>();

                if (monsterAI == null || creatureClaim != null)
                    continue;

                List<string> users = m_chat.GetUsersInChatHistory();
                List<string> assignedUsers = m_creatureAssignments.Select(item => item.userName).ToList();
                users.RemoveAll(item => assignedUsers.Contains(item));

                if (users.Count == 0)
                {
                    Jotunn.Logger.LogWarning("No authors found");
                    return;
                }

                int index = Random.Range(0, users.Count);
                string chosenUser = users[index];

                if (ContainsCreatureAssignment(chosenUser))
                {
                    Jotunn.Logger.LogWarning("User already has a claim, going for next user");
                    continue;
                }

                m_chosenUser = chosenUser;
                m_chosenPrefab = obj.gameObject;
                m_chat.Send($"{chosenUser} you have been selected to become a {Localization.instance.Localize(humanoid.m_name)}! Type \"!claim\" to accept.");
                break;
            }
        }

        public void AcceptClaim()
        {
            if (m_chosenUser == null || m_chosenPrefab == null)
            {
                return;
            }

            Humanoid humanoid = m_chosenPrefab.GetComponent<Humanoid>();
            m_chat.Send($"Creature {Localization.instance.Localize(humanoid.m_name)} is now claimed by {m_chosenUser}!");

            TwitchCreatureClaim newCreatureClaimn = m_chosenPrefab.AddComponent<TwitchCreatureClaim>();
            newCreatureClaimn.Init(m_chosenUser);

            m_chosenUser = null;
            m_chosenPrefab = null;
        }

        public string GetChosenUser()
        {
            return m_chosenUser;
        }

        public List<string> GetUserBlacklist()
        {
            return m_userBlacklist;
        }

        public void DeserializeUserBlackList(string value)
        {
            string[] data = value.Trim().Split(',');
            m_userBlacklist = new List<string>();
            
            foreach (string entry in data)
            {
                m_userBlacklist.Add(entry.ToLower());
            }
        }
    }
}
