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

        // Fallback talk used for non-creature prefabs (e.g. the hot tub) that have no MonsterAI -
        // NpcTalk.SayForce() calls m_animator.SetTrigger(), which NREs without one.
        private List<string> m_simpleTalkMessages;
        private int m_simpleTalkIndex;

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
            m_originalName = m_humanoid?.m_name;

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
            if (gameObject.GetComponent<MonsterAI>() == null)
            {
                SetupSimpleTalk(creatureData);
                return;
            }

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

        private void SetupSimpleTalk(CreatureData creatureData)
        {
            if (creatureData == null || !creatureData.talks || creatureData.talkMessage == null)
                return;

            m_simpleTalkMessages = creatureData.talkMessage.Contains(";")
                ? new List<string>(creatureData.talkMessage.Split(';'))
                : new List<string> { creatureData.talkMessage };

            for (int i = 0; i < m_simpleTalkMessages.Count; i++)
                m_simpleTalkMessages[i] = m_simpleTalkMessages[i].Replace("{{userName}}", m_assignment.userName);

            if (creatureData.talkInterval >= 3f)
                InvokeRepeating(nameof(SaySimpleMessage), 0f, creatureData.talkInterval);
            else
                SaySimpleMessage();
        }

        public void SaySimpleMessage()
        {
            if (m_simpleTalkMessages == null || m_simpleTalkMessages.Count == 0 || Chat.instance == null)
                return;

            if (!m_chatting.CanCreatureTalk(gameObject))
                return;

            string text = m_simpleTalkMessages[m_simpleTalkIndex % m_simpleTalkMessages.Count];
            m_simpleTalkIndex++;

            // Same call NpcTalk.Say() makes internally for its speech bubble - only needs a
            // GameObject to anchor to, no Character/MonsterAI/Animator involved (trigger is
            // left empty so NpcTalk's own m_animator.SetTrigger(trigger) call is skipped there).
            Chat.instance.SetNpcText(gameObject, Vector3.up * 2f, PluginConfig.configChattingCullingRange.Value, 10f, "", text, large: false);
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

            if (message.message.Equals("!heal", StringComparison.OrdinalIgnoreCase) && (m_originalName?.Contains("shaman") ?? false))
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

                if (m_chatting.CanCreatureTalk(gameObject))
                    m_npcTalk.SayForce("Alright... healing!", "Aggravated");

                m_humanoid.EquipBestWeapon(m_humanoid, null, m_humanoid, null);
                monsterAI.DoAttack(m_humanoid, true);
                return;
            }

            if (m_npcTalk != null && m_chatting.CanCreatureTalk(gameObject))
                m_npcTalk.SayForce(message.message, "Aggravated");
        }

        public void SayAMessage()
        {
            // Deliberately bypass NpcTalk.OnBecameAggravated here: it only queues the message,
            // and vanilla's actual display is gated by a *static* cooldown shared by every NpcTalk
            // in the scene. That decouples display timing from the CanCreatureTalk distance check
            // done above, letting a farther queued creature win the shared cooldown race over a
            // nearer one. SayForce writes the bubble immediately, keeping display in sync with the check.
            if (!m_chatting.CanCreatureTalk(gameObject) || m_npcTalk.m_aggravated == null || m_npcTalk.m_aggravated.Count == 0)
                return;

            string text = m_npcTalk.m_aggravated[UnityEngine.Random.Range(0, m_npcTalk.m_aggravated.Count)];
            m_npcTalk.SayForce(text, "Aggravated");
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
