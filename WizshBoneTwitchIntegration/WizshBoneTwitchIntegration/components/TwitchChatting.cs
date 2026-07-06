using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
        public UnityEvent<TwitchChatMessage> onNewMessage = new UnityEvent<TwitchChatMessage>();

        public float m_scanRadius = PluginConfig.configChattingRadius.Value;
        public float m_scanInterval = PluginConfig.configChattingInterval.Value;
        private string m_chosenUser;
        private GameObject m_chosenPrefab;

        public void Awake()
        {
            try
            {
                m_auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                m_chat = Game.instance.gameObject.GetComponent<TwitchChat>();
                m_enabled = PluginConfig.configChattingEnabled.Value;

                DeserializeUserBlackList(PluginConfig.configChattingBlackList.Value);

                foreach (string entry in m_userBlacklist)
                    Jotunn.Logger.LogWarning(entry);

                InvokeRepeatingScan();
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchChatting.Awake failed: " + e);
            }
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

                // Only destroy the claim component if the creature itself is still alive.
                // If the creature is being destroyed, OnDestroy already called us — don't re-trigger it.
                if (creatureClaim != null && assignment.creature.activeInHierarchy)
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

        public bool CanCreatureTalk(GameObject creature)
        {
            int maxTalkers = PluginConfig.configChattingMaxTalkers.Value;

            if (maxTalkers <= 0 || Player.m_localPlayer == null)
                return true;

            Vector3 playerPos = Player.m_localPlayer.transform.position;

            return m_creatureAssignments
                .Where(item => item.creature != null)
                .OrderBy(item => Vector3.Distance(playerPos, item.creature.transform.position))
                .Take(maxTalkers)
                .Any(item => item.creature == creature);
        }

        public void InvokeRepeatingScan()
        {
            CancelInvoke(nameof(ChattingScan));
            InvokeRepeating(nameof(ChattingScan), 0f, m_scanInterval);
        }

        public void ChattingScan()
        {
            try
            {
                ScanAndAssignUsers();
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchChatting.ChattingScan failed: " + e);
            }
        }

        public void ScanAndAssignUsers(bool command = false)
        {
            if (!command && (!m_auth.m_loggedIn || !m_enabled))
                return;

            List<GameObject> creatures = new List<GameObject>();

            // Player is likely dead
            if (Player.m_localPlayer == null)
                return;

            Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, m_scanRadius, LayerMask.GetMask("character"));

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

                if (command)
                {
                    TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
                    TwitchCreatureClaim newCreatureClaimn = obj.gameObject.AddComponent<TwitchCreatureClaim>();
                    newCreatureClaimn.Init(customRewards.m_alias ?? "DeathWizsh");
                    return;
                }

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

            // The offer was validated as unclaimed back at scan time, but time passes between the offer
            // and this accept - re-check here so we can never end up adding a second TwitchCreatureClaim
            // on top of one that already exists (which would silently steal/corrupt an existing claim).
            if (m_chosenPrefab.GetComponent<TwitchCreatureClaim>() != null)
            {
                Jotunn.Logger.LogWarning($"[WBTI] AcceptClaim: {m_chosenUser} tried to claim a creature that's already claimed, ignoring.");
                m_chosenUser = null;
                m_chosenPrefab = null;
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
                m_userBlacklist.Add(entry.ToLower().Trim());
            }
        }
    }
}
