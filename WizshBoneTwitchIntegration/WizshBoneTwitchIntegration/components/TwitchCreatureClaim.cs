using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreatureClaim : MonoBehaviour
    {
        public TwitchChatting m_chatting;
        public TwitchChat m_chat;
        public TwitchCreatureAssignment m_assignment;
        public ZNetView m_netView;
        public NpcTalk m_npcTalk;
        public Humanoid m_humanoid;

        public string m_originalName;
        public bool m_isSpawn = false;
        public DateTime m_lastMessageTime;
        private int m_unclaimTimer = 120;
        private bool m_isUnclaimDestroy = false;

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView != null && m_netView.GetZDO() != null)
            {
                m_chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                m_chat = Game.instance.gameObject.GetComponent<TwitchChat>();
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

            m_isSpawn = true;
            m_assignment = new TwitchCreatureAssignment(options.customReward.RedeemerName, gameObject);
            m_originalName = m_humanoid.m_name;

            TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>();
            persistentData.SetData(m_assignment.userName, options.creatureData);

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk(options.creatureData);
            InvokeRepeating(nameof(CheckChatForMessage), 0f, 3f);
        }

        public void Init(string userName)
        {
            if (userName == null || userName == "")
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, user name is null");
                return;
            }

            m_assignment = new TwitchCreatureAssignment(userName, gameObject);
            m_originalName = m_humanoid.m_name;
            m_humanoid.m_name = userName;

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk();
            InvokeRepeating(nameof(CheckChatForMessage), 0f, 3f);
        }

        public void ReInit(string userName, SpawnCreatureData creatureData)
        {
            if (userName == null || creatureData == null)
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, either user name or options is null");
                return;
            }

            m_isSpawn = true;
            m_assignment = new TwitchCreatureAssignment(userName, gameObject);
            m_originalName = m_humanoid.m_name;

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk(creatureData);
            InvokeRepeating(nameof(CheckChatForMessage), 0f, 3f);
        }

        public void OnDestroy()
        {
            CancelInvoke(nameof(CheckChatForMessage));
            Destroy(m_npcTalk);

            m_assignment.creature = null;
            m_humanoid.m_name = m_originalName;
            m_lastMessageTime = DateTime.MinValue;

            if (!m_isUnclaimDestroy)
                Unassign();
        }

        private void SetupNpcTalk(SpawnCreatureData creatureData = null)
        {
            m_npcTalk = gameObject.AddComponent<NpcTalk>();
            m_npcTalk.m_name = m_assignment.userName;
            m_npcTalk.m_maxRange = 30f;
            m_npcTalk.m_offset = 1f;
            m_npcTalk.m_hideDialogDelay = 10f;

            if (creatureData == null || !creatureData.talks || creatureData.talkMessage == null)
                return;

            if (creatureData.talkMessage.Contains(";"))
            {
                string[] messages = creatureData.talkMessage.Split(';');
                m_npcTalk.m_aggravated = new List<string>();

                foreach (string message in messages)
                {
                    string fixedMessage = message.Replace("{{userName}}", m_assignment.userName);
                    m_npcTalk.m_aggravated.Add(fixedMessage);
                }
            }
            else
            {
                m_npcTalk.m_aggravated = new List<string>() { creatureData.talkMessage };
            }

            if (creatureData.talkInterval >= 3f)
                InvokeRepeating(nameof(InvokeTalkInterval), 0f, creatureData.talkInterval);
            else
                m_npcTalk.OnBecameAggravated(BaseAI.AggravatedReason.Damage);
        }

        private void CheckChatForMessage()
        {
            TwitchChatMessage message = m_chat.GetLastMessageOfUser(m_assignment.userName);

            if (m_lastMessageTime != DateTime.MinValue)
            {
                TimeSpan timeSpan = DateTime.Now.Subtract(m_lastMessageTime);

                if (!m_isSpawn && timeSpan.TotalSeconds > m_unclaimTimer)
                {
                    m_isUnclaimDestroy = true;
                    Unassign();
                    return;
                }
            }

            if (message == null || message.hasBeenBroadcasted)
                return;

            if (message.message.Equals("!unclaim", StringComparison.OrdinalIgnoreCase))
            {
                SetMessageAsBroadcasted(message);
                m_isUnclaimDestroy = true;
                Unassign();
                return;
            }

            if (message.message.Equals("!heal", StringComparison.OrdinalIgnoreCase) && m_originalName.Contains("shaman"))
            {
                if (m_humanoid.InAttack())
                    return;

                MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();

                if (monsterAI == null)
                {
                    Jotunn.Logger.LogError("Could not find monster AI to start healing");
                    return;
                }

                SetMessageAsBroadcasted(message);
                m_npcTalk.Say("Alright... healing!", "Aggravated");
                m_humanoid.EquipBestWeapon(m_humanoid, null, m_humanoid, null);
                monsterAI.DoAttack(m_humanoid, true);
                return;
            }

            SetMessageAsBroadcasted(message);
            m_npcTalk.Say(message.message, "Aggravated");
        }

        private void InvokeTalkInterval()
        {
            m_npcTalk.OnBecameAggravated(BaseAI.AggravatedReason.Damage);
        }

        private void SetMessageAsBroadcasted(TwitchChatMessage message)
        {
            message.hasBeenBroadcasted = true;
            m_lastMessageTime = DateTime.Now;
        }

        private void Unassign()
        {
            if (m_assignment == null || m_assignment.userName == null)
            {
                Jotunn.Logger.LogError("Can not remove creature assignment, either the assignment or userName is null");
                return;
            }
            
            m_chatting.RemoveCreatureAssignment(m_assignment);
        }
    }
}
