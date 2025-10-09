using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchCheckForMessage : MonoBehaviour
    {
        TwitchChatting m_twitchChatting;
        TwitchChat m_twitchChat;
        NpcTalk m_npcTalk;
        Humanoid m_humanoid;

        string m_author;
        string m_originalName;
        DateTime m_lastMessageTime;
        private int m_resetTimer = 60;

        private void Awake()
        {
            m_twitchChatting = Player.m_localPlayer.gameObject.GetComponent<TwitchChatting>();
            m_twitchChat = Game.instance.gameObject.GetComponent<TwitchChat>();
            m_humanoid = gameObject.GetComponent<Humanoid>();
        }

        public void Init(string author)
        {
            if (author == null || author == "")
            {
                Jotunn.Logger.LogWarning("TwitchCheckForMessage does not have author!");
                return;
            }

            m_npcTalk = gameObject.AddComponent<NpcTalk>();
            m_author = author;
            m_originalName = m_humanoid.m_name;

            m_humanoid.m_name = author;
            m_npcTalk.m_name = author;
            m_npcTalk.m_maxRange = 30f;
            m_npcTalk.m_offset = 1f;
            m_npcTalk.m_hideDialogDelay = 10f;

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
                Jotunn.Logger.LogWarning("Time span seconds: " + timeSpan.TotalSeconds);

                if (timeSpan.TotalSeconds > m_resetTimer)
                {
                    Jotunn.Logger.LogWarning("Unassign");
                    UnassignUser();
                }
            }

            if (message == null || message.hasBeenBroadcasted || message.message == null)
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
