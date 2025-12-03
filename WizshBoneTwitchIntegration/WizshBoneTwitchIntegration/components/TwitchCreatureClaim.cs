using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using YamlDotNet.Core.Tokens;

namespace WizshBoneTwitchIntegration.components
{
    internal class TwitchCreatureClaim : MonoBehaviour
    {
        public TwitchChatting m_twitchChatting;
        public TwitchChat m_twitchChat;
        public ZNetView m_netView;
        public NpcTalk m_npcTalk;
        public Humanoid m_humanoid;

        public string m_author;
        public string m_originalName;
        public bool m_isSpawn = false;
        public DateTime m_lastMessageTime;
        private int m_unclaimTimer = 120;

        private bool m_allowDrops;
        private Character.Faction faction;
        private string m_name;

        public readonly int allowDropsHash = "WBTI_CreatureAllowDrops".GetStableHashCode();
        public readonly int factionHash = "WBTI_CreatureAllowDrops".GetStableHashCode();
        public readonly int nameHash = "WBTI_CreatureFaction".GetStableHashCode();

        private void Awake()
        {
            m_netView.gameObject.GetComponent<ZNetView>();

            if (m_netView != null && m_netView.GetZDO() != null)
            {
                m_twitchChatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                m_twitchChat = Game.instance.gameObject.GetComponent<TwitchChat>();
                m_humanoid = gameObject.GetComponent<Humanoid>();

                // m_name = m_netView.GetZDO().GetString(nameHash, m_humanoid.m_name);
            }
        }

        public void Init(string author, bool isSpawn = false)
        {
            if (author == null || author == "")
            {
                Jotunn.Logger.LogWarning("MonsterClaim does not have a user assigned!");
                return;
            }

            m_author = author;
            m_originalName = m_humanoid.m_name;
            m_isSpawn = isSpawn;
            m_humanoid.m_name = author;

            m_npcTalk = gameObject.AddComponent<NpcTalk>();
            m_npcTalk.m_name = author;
            m_npcTalk.m_maxRange = 30f;
            m_npcTalk.m_offset = 1f;
            m_npcTalk.m_hideDialogDelay = 10f;

            m_twitchChatting.AddAssignedUser(author);

            InvokeRepeating(nameof(CheckChatForMessage), 0f, 3f);
        }

        private void OnDestroy()
        {
            Jotunn.Logger.LogWarning("OnDestroy");
            UnassignUser(true);
        }

        private void CheckChatForMessage()
        {
            TwitchChatMessage message = m_twitchChat.GetLatestMessageByAuthor(m_author);

            if (m_lastMessageTime != DateTime.MinValue)
            {
                TimeSpan timeSpan = DateTime.Now.Subtract(m_lastMessageTime);

                if (!m_isSpawn && timeSpan.TotalSeconds > m_unclaimTimer)
                {
                    Jotunn.Logger.LogWarning("Unassign");
                    UnassignUser();
                }
            }

            if (message == null || message.hasBeenBroadcasted)
                return;

            message.hasBeenBroadcasted = true;
            m_lastMessageTime = DateTime.Now;
            m_npcTalk.Say(message.message, "Aggravated");
        }

        private void UnassignUser(bool isDestroy = false)
        {
            CancelInvoke(nameof(CheckChatForMessage));

            if (m_author != null)
                m_twitchChatting.RemoveAssignedUser(m_author);

            if (!isDestroy)
            {
                m_humanoid.m_name = m_originalName;
                m_author = null;
                m_lastMessageTime = DateTime.MinValue;
                Destroy(this);
            }
        }
    }
}
