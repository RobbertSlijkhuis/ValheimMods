using System;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Extensions;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreatureClaim : MonoBehaviour
    {
        private TwitchChatting m_chatting;
        private TwitchChat m_chat;
        private TwitchCreatureInteract m_creatureInteract;
        private TwitchCreatureAssignment m_assignment;
        private ZNetView m_netView;
        private NpcTalk m_npcTalk;
        private Humanoid m_humanoid;

        public bool m_isSpawn = false;
        private string m_originalName;
        private DateTime m_lastMessageTime;
        private bool m_isUnclaimDestroy = false;

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                m_chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                m_chat = Game.instance.gameObject.GetComponent<TwitchChat>();
                m_humanoid = gameObject.GetComponent<Humanoid>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchCreatureClaim.Awake failed: " + e);
            }
        }

        public void Init(CreatureData creatureData, CustomRewardEvent customRewardEvent)
        {
            if (creatureData == null || customRewardEvent == null)
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, options is null");
                return;
            }

            m_isSpawn = true;
            m_assignment = new TwitchCreatureAssignment(customRewardEvent.RedeemerName, gameObject, PluginConfig.configChattingClaimDuration.Value);
            m_originalName = m_humanoid.m_name;

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk(creatureData);
            m_chatting.onNewMessage.AddListener(CheckChatForMessage);
        }

        public void Init(string userName)
        {
            if (userName == null || userName == "")
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, user name is null");
                return;
            }

            m_assignment = new TwitchCreatureAssignment(userName, gameObject, PluginConfig.configChattingClaimDuration.Value);
            m_originalName = m_humanoid.m_name;
            m_humanoid.m_name = userName;

            if (PluginConfig.configChattingClaimDuration.Value == 0 && gameObject.GetComponent<TwitchCreaturePersistentData>() == null)
            {
                // Only create fresh persistent data for a genuinely wild creature. If one already exists,
                // this creature was twitch-spawned earlier (e.g. claimed, then !unclaim'd, then re-claimed
                // here as if wild) - overwriting it would wipe its real redeem title/prefab/color, silently
                // corrupting its identity on the next reload.
                gameObject.AddComponent<TwitchCreaturePersistentData>().SetData(m_assignment.userName);
            }

            if (RecolorHelper.CanRecolorCreature(m_assignment.userName, m_assignment.creature.name))
                RecolorHelper.RecolorCreature(m_assignment.userName, gameObject);

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk();
            m_chatting.onNewMessage.AddListener(CheckChatForMessage);
        }

        public void ReInit(string userName, CreatureData creatureData = null)
        {
            if (userName == null)
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, user name is null");
                return;
            }

            m_isSpawn = true;
            m_assignment = new TwitchCreatureAssignment(userName, gameObject, PluginConfig.configChattingClaimDuration.Value);
            m_originalName = m_humanoid.m_name;

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk(creatureData);
            m_chatting.onNewMessage.AddListener(CheckChatForMessage);
        }

        public void OnDestroy()
        {
            try
            {
                if (m_chatting != null)
                    m_chatting.onNewMessage.RemoveListener(CheckChatForMessage);

                Destroy(m_npcTalk);

                if (m_creatureInteract != null)
                    Destroy(m_creatureInteract);

                if (m_assignment?.creature != null && RecolorHelper.CanRecolorCreature(m_assignment.userName, m_assignment.creature.name))
                    RecolorHelper.UnColorCreature(m_assignment.creature);

                if (m_assignment != null)
                    m_assignment.creature = null;

                if (m_humanoid != null)
                    m_humanoid.m_name = m_originalName;

                m_lastMessageTime = DateTime.MinValue;

                if (!m_isUnclaimDestroy)
                    Unassign();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchCreatureClaim.OnDestroy failed: " + e);
            }
        }

        private void SetupNpcTalk(CreatureData creatureData = null)
        {
            NpcTalk npcTalk = gameObject.GetComponent<NpcTalk>();

            if (npcTalk == null)
            {
                m_npcTalk = gameObject.AddComponent<NpcTalk>();
                m_npcTalk.m_name = m_assignment.userName;
                m_npcTalk.m_maxRange = 30f;
                m_npcTalk.m_offset = 1f;
                m_npcTalk.m_hideDialogDelay = 10f;
            }
            else
                m_npcTalk = npcTalk;

            if (creatureData == null || !creatureData.talks || creatureData.talkMessage == null)
                return;

            if (creatureData.talkInteract)
                m_creatureInteract = gameObject.AddComponent<TwitchCreatureInteract>();

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
                InvokeRepeating(nameof(SayAMessage), 0f, creatureData.talkInterval);
            else if (!creatureData.talkInteract)
                SayAMessage();
        }

        private void CheckChatForMessage(TwitchChatMessage message)
        {
            if (m_assignment.userName.ToLower() != message.userName)
                return;

            if (m_lastMessageTime != DateTime.MinValue)
            {
                TimeSpan timeSpan = DateTime.Now.Subtract(m_lastMessageTime);

                if (!m_isSpawn && timeSpan.TotalSeconds > m_assignment.duration)
                {
                    m_isUnclaimDestroy = true;
                    Unassign();
                    return;
                }
            }

            if (message == null)
                return;

            if (message.message.Equals("!unclaim", StringComparison.OrdinalIgnoreCase))
            {
                m_isUnclaimDestroy = true;
                Unassign();
                return;
            }

            if (message.message.Equals("!heal", StringComparison.OrdinalIgnoreCase) && m_originalName.Contains("shaman"))
            {
                if (m_humanoid == null)
                {
                    Jotunn.Logger.LogError("Cannot finish command !heal, humanoid is null");
                    return;
                }

                if (m_humanoid.InAttack())
                    return;

                MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();

                if (monsterAI == null)
                {
                    Jotunn.Logger.LogError("Could not find monster AI to start healing");
                    return;
                }

                m_npcTalk.SayForce("Alright... healing!", "Aggravated");
                m_humanoid.EquipBestWeapon(m_humanoid, null, m_humanoid, null);
                monsterAI.DoAttack(m_humanoid, true);
                return;
            }

            m_npcTalk.SayForce(message.message, "Aggravated");
        }

        public void SayAMessage()
        {
            m_npcTalk.OnBecameAggravated(BaseAI.AggravatedReason.Damage);
        }

        private void Unassign()
        {
            if (m_assignment == null || m_assignment.userName == null)
            {
                Jotunn.Logger.LogError("Can not remove creature assignment, either the assignment or userName is null");
                return;
            }

            if (m_chatting == null)
            {
                Jotunn.Logger.LogError("Can not remove creature assignment, m_chatting is null");
                return;
            }

            m_chatting.RemoveCreatureAssignment(m_assignment);
        }
    }
}
