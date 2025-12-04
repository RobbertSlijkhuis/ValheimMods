using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

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

        private void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView != null && m_netView.GetZDO() != null)
            {
                m_twitchChatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                m_twitchChat = Game.instance.gameObject.GetComponent<TwitchChat>();
                m_humanoid = gameObject.GetComponent<Humanoid>();
            }
        }

        public void Init(SpawnOptions options)
        {
            if (options == null)
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, options is null");
                return;
            }

            m_author = options.customReward.RedeemerName;

            TwitchPersistentData persistentData = gameObject.GetComponent<TwitchPersistentData>();
            persistentData.SetData(m_author, options.creatureData.allowDrops);

            m_npcTalk = gameObject.AddComponent<NpcTalk>();
            m_npcTalk.m_name = m_author;
            m_npcTalk.m_maxRange = 30f;
            m_npcTalk.m_offset = 1f;
            m_npcTalk.m_hideDialogDelay = 10f;

            m_twitchChatting.AddAssignedUser(m_author, gameObject);

            InvokeRepeating(nameof(CheckChatForMessage), 0f, 3f);
        }

        public void Init(string author)
        {
            if (author == null || author == "")
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, user is null");
                return;
            }

            m_author = author;
            m_originalName = m_humanoid.m_name;
            m_humanoid.m_name = author;

            m_npcTalk = gameObject.AddComponent<NpcTalk>();
            m_npcTalk.m_name = author;
            m_npcTalk.m_maxRange = 30f;
            m_npcTalk.m_offset = 1f;
            m_npcTalk.m_hideDialogDelay = 10f;

            m_twitchChatting.AddAssignedUser(author, gameObject);

            InvokeRepeating(nameof(CheckChatForMessage), 0f, 3f);
        }

        public void OnDestroy()
        {
            UnassignUser(true);
        }

        private void CheckChatForMessage()
        {
            TwitchChatMessage message = m_twitchChat.GetLatestMessageByAuthor(m_author);

            if (m_lastMessageTime != DateTime.MinValue)
            {
                TimeSpan timeSpan = DateTime.Now.Subtract(m_lastMessageTime);

                if (!m_isSpawn && timeSpan.TotalSeconds > m_unclaimTimer)
                    UnassignUser();
            }

            if (message == null || message.hasBeenBroadcasted)
                return;

            message.hasBeenBroadcasted = true;
            m_lastMessageTime = DateTime.Now;
            m_npcTalk.Say(message.message, "Aggravated");
        }

        public void UnassignUser(bool isAlreadyDestroyed = false)
        {
            if (m_author != null)
                m_twitchChatting.RemoveAssignedUser(m_author);

            // Prevent multiple destructions when creature is killed
            if (!isAlreadyDestroyed)
                PrepareForDestruction();
        }

        public void UnassignClaim()
        {
            if (m_author != null)
                PrepareForDestruction();
        }

        private void PrepareForDestruction()
        {
            m_humanoid.m_name = m_originalName;
            m_author = null;
            m_lastMessageTime = DateTime.MinValue;
            Destroy(m_npcTalk);
            Destroy(this);
        }
    }
}
