using System;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
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
        private Character m_character;

        public bool m_isSpawn = false;
        private string m_originalName;
        private string m_originalTamedName;
        private DateTime m_lastMessageTime;
        private bool m_isUnclaimDestroy = false;

        // Fallback talk used for non-creature prefabs that have no MonsterAI -
        // NpcTalk.SayForce() calls m_animator.SetTrigger(), which NREs without one.
        private List<string> m_simpleTalkMessages;

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                m_chatting = Game.instance.gameObject.GetComponent<TwitchChatting>();
                m_chat = Game.instance.gameObject.GetComponent<TwitchChat>();
                m_character = gameObject.GetComponent<Character>();
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
            m_assignment = new TwitchCreatureAssignment(customRewardEvent.RedeemerName, gameObject, ProfileSettingsHelper.Current.chattingClaimDuration);
            m_originalName = m_character?.m_name;

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

            // This creature already existed before being claimed (wild tame renamed, or an
            // auto-scanned offer accepted via !claim) so the local client isn't guaranteed to
            // already own its ZDO - claim it so the persistent-data write below actually takes
            // effect, regardless of who owned it up to this point.
            if (m_netView != null && m_netView.IsValid() && !m_netView.IsOwner())
                m_netView.ClaimOwnership();

            m_assignment = new TwitchCreatureAssignment(userName, gameObject, ProfileSettingsHelper.Current.chattingClaimDuration);

            // Only captured on the very first claim (re-claim:-renaming an already-claimed creature
            // calls Init again on this same component) - otherwise re-claiming would only ever restore
            // the *previous* claimant's name instead of the true original once unclaimed/expired.
            if (m_originalName == null)
            {
                m_originalName = m_character.m_name;

                // Tameable.GetHoverName() prefers this ZDO field over Character.m_name whenever it's
                // non-empty, so resetting only m_character.m_name isn't enough to actually change what
                // the player sees once a creature has ever been claim:-renamed - captured here, before
                // any pending claim:-rename gets persisted, so the true original can be restored later.
                if (m_netView != null && m_netView.IsValid())
                    m_originalTamedName = m_netView.GetZDO().GetString(ZDOVars.s_tamedName, "");
            }

            m_character.m_name = userName;

            if (ProfileSettingsHelper.Current.chattingClaimDuration == 0 && !(gameObject.GetComponent<TwitchBasePersistentData>()?.IsRedeemSpawn ?? false))
            {
                // Refresh persistent data on every manual claim, not just the first - otherwise
                // re-claim:-renaming an already-claimed creature leaves the previous claimant's name
                // (and color, on reload) stuck in the ZDO forever, since SetData() would never run again.
                // Only skip this for a genuinely redeem-spawned creature (IsRedeemSpawn) - one that already
                // has real persistent data (title/prefab/color) which this claim:-rename must not wipe.
                TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>() ?? gameObject.AddComponent<TwitchCreaturePersistentData>();
                persistentData.SetData(m_assignment.userName);
            }

            // Re-claiming an already-claimed creature (claim:-renaming it again) reuses this same
            // component instead of tearing it down first, so a previous claim's color would otherwise
            // stick around if the new name isn't a special viewer - reset to original in that case.
            // RecolorCreature always replaces the material outright, so no reset is needed when the
            // new name does recolor.
            if (RecolorHelper.CanRecolorCreature(m_assignment.userName, m_assignment.creature.name))
                RecolorHelper.RecolorCreature(m_assignment.userName, gameObject);
            else if (RecolorHelper.IsCreatureInList(m_assignment.creature.name))
                RecolorHelper.UnColorCreature(gameObject);

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk();
            m_chatting.onNewMessage.AddListener(CheckChatForMessage);

            // Duration == 0 means permanent (see the persistent-data check above) - only
            // self-expire timed claims. Runs on its own schedule instead of only re-checking when
            // the claimed user happens to chat again, so a claim still expires even if they never do.
            m_lastMessageTime = DateTime.Now;

            // Re-claiming (claim:-renaming an already-claimed creature) reuses this same component
            // and calls Init again - cancel any previous timer first so they don't stack.
            CancelInvoke(nameof(CheckExpiry));

            if (m_assignment.duration > 0)
                InvokeRepeating(nameof(CheckExpiry), ProfileSettingsHelper.Current.chattingInterval, ProfileSettingsHelper.Current.chattingInterval);
        }

        // isSpawn must be passed explicitly rather than inferred from creatureData - the SpawnAbility
        // placeholder-redeem rehydration path (TwitchCreaturePersistentData.Awake, m_savedPrefabName
        // == "") is a genuine spawn with no CreatureData to reapply, so "creatureData != null" isn't
        // a reliable signal. Getting this wrong excludes/includes a claim from index/bracket numbering
        // incorrectly (see TwitchChatting.NonSpawnSiblings).
        public void ReInit(string userName, bool isSpawn, CreatureData creatureData = null)
        {
            if (userName == null)
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, user name is null");
                return;
            }

            m_isSpawn = isSpawn;
            m_assignment = new TwitchCreatureAssignment(userName, gameObject, ProfileSettingsHelper.Current.chattingClaimDuration);

            // Null-safe like Init(CreatureData, CustomRewardEvent)'s equivalent line - some
            // rehydration paths (see TwitchCreaturePersistentData.Awake) have no Character component,
            // so m_character is genuinely null here.
            m_originalName = m_character?.m_name;

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

                if (m_character != null)
                    m_character.m_name = m_originalName;

                // Restores the vanilla Tameable rename field alongside Character.m_name above - only
                // set for claims made via Init(string) (see the capture there for why).
                if (m_originalTamedName != null)
                    SetTameableText(m_originalTamedName);

                m_lastMessageTime = DateTime.MinValue;
                CancelInvoke(nameof(CheckExpiry));

                // A genuine unclaim (chat, WBTIUnclaim, or unclaim:) on a manually-claimed (non-spawn)
                // creature must also clear the persisted claim data, not just this component - otherwise
                // TwitchCreaturePersistentData's ZDO string field (and the Creature flag that makes
                // TwitchBasePersistentData re-add it) survive, and the next reload re-hydrates the claim
                // right back into existence under the same name. Only for !m_isSpawn - a redeem-spawned
                // creature that was also claim:-renamed keeps its own real persistent data intact.
                if (m_isUnclaimDestroy && !m_isSpawn)
                {
                    TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>();

                    if (persistentData != null)
                    {
                        persistentData.ClearClaimData();
                        Destroy(persistentData);
                    }
                }

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

                // A prefab that never had NpcTalk built in (e.g. Deathsquito) never had m_animator
                // wired up in the editor either - without this, NpcTalkExtension.SayForce's
                // m_animator.SetTrigger() NREs the first time this creature talks.
                m_npcTalk.m_animator = gameObject.GetComponentInChildren<Animator>();
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

            string text = m_simpleTalkMessages[UnityEngine.Random.Range(0, m_simpleTalkMessages.Count)];

            ShowBubbleText(text);
        }

        // Shared speech-bubble relay for both talk paths set up in SetupNpcTalk: creatures with a
        // MonsterAI get a real NpcTalk (SayForce), everything else (e.g. AnimalAI-driven creatures
        // like Deer, which never get an NpcTalk) falls back to the same direct Chat.instance.SetNpcText()
        // call NpcTalk.Say() makes internally - only needs a GameObject to anchor to, no
        // Character/MonsterAI/Animator involved (trigger left empty so NpcTalk's own
        // m_animator.SetTrigger(trigger) call is skipped there).
        private void ShowBubbleText(string text)
        {
            if (m_npcTalk != null)
            {
                m_npcTalk.SayForce(text, "Aggravated");
                return;
            }

            if (Chat.instance == null)
                return;

            Chat.instance.SetNpcText(gameObject, Vector3.up * 2f, ProfileSettingsHelper.Current.chattingCullingRange, 10f, "", text, large: false);
        }

        private void CheckChatForMessage(TwitchChatMessage message)
        {
            if (m_assignment.userName.ToLower() != message.userName)
                return;

            // Chatting again keeps a timed claim alive - CheckExpiry is what actually decides
            // whether too much time has passed since this was last refreshed.
            m_lastMessageTime = DateTime.Now;

            if (message == null)
                return;

            // "!unclaim" itself is intercepted centrally in TwitchChat.cs (calls
            // TwitchChatting.UnclaimForUser directly) before ever reaching this per-creature
            // broadcast - resolving it here independently per claim caused a releasing claim to
            // shift another still-pending claim's live index into matching within the same broadcast.

            if (message.message.Equals("!heal", StringComparison.OrdinalIgnoreCase) && (m_originalName?.Contains("shaman") ?? false))
            {
                if (m_character == null)
                {
                    Jotunn.Logger.LogError("Cannot finish command !heal, humanoid is null");
                    return;
                }

                if (m_character.InAttack())
                    return;

                MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();

                if (monsterAI == null)
                {
                    Jotunn.Logger.LogError("Could not find monster AI to start healing");
                    return;
                }

                if (m_chatting.CanCreatureTalk(gameObject))
                    m_npcTalk.SayForce("Alright... healing!", "Aggravated");

                // EquipBestWeapon is Humanoid-only - this easter egg is gated to shaman-type
                // creatures above (always Humanoid+MonsterAI), so the cast always succeeds in
                // practice; the null-conditional just keeps this compiling now that m_character
                // is typed as the shared Character base (for AnimalAI creature support).
                (m_character as Humanoid)?.EquipBestWeapon(m_character, null, m_character, null);
                monsterAI.DoAttack(m_character, true);
                return;
            }

            if (m_chatting.CanCreatureTalk(gameObject))
                ShowBubbleText(message.message);
        }

        // Called by TwitchChatting.UnclaimForUser once it has already decided this claim should be
        // released - no matching happens here, the decision was made centrally against a stable
        // snapshot (see UnclaimForUser for why that matters).
        public void Release()
        {
            m_isUnclaimDestroy = true;
            Unassign();
        }

        // Name-matching half of "!unclaim <name>" targeting - the index half lives entirely in
        // TwitchChatting.UnclaimForUser now (it needs every sibling's position at once, not just
        // this one claim's own state). Matches the localized species name rather than the raw
        // prefab id (avoids "FallenValkyrie"-style confusion); spawned creatures also accept their
        // current display name, since a redeem can rename them to something like "Fluffy" that
        // m_originalName no longer reflects.
        public bool MatchesNameTarget(string target)
        {
            string originalName = Localization.instance.Localize(m_originalName ?? "");

            if (originalName.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (m_isSpawn)
            {
                string currentName = Localization.instance.Localize(m_character?.m_name ?? "");
                return currentName.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
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

        public void CheckExpiry()
        {
            if (DateTime.Now.Subtract(m_lastMessageTime).TotalSeconds <= m_assignment.duration)
                return;

            m_isUnclaimDestroy = true;
            Unassign();
        }

        // Renders (or clears) this claim's owner+index disambiguation suffix, e.g. "Alice [2]" -
        // called by TwitchChatting.RefreshIndexDisplayForOwner whenever the owner's number of
        // simultaneously-loaded manual claims changes. Never called for redeem spawns (m_isSpawn),
        // those are excluded from indexing entirely. Writes both Character.m_name (what a non-tame
        // creature's hover text reads) and, if present, the Tameable's ZDO-backed name via SetText -
        // Tameable.GetHoverName() prefers that ZDO field over Character.m_name whenever it's set, so
        // a claim:-renamed tame needs both written or the bracket silently wouldn't show.
        public void SetDisplayIndex(int? index)
        {
            if (m_isSpawn || m_character == null)
                return;

            string displayName = index.HasValue ? $"{m_assignment.userName} [{index}]" : m_assignment.userName;
            m_character.m_name = displayName;
            SetTameableText(displayName);
        }

        // Ensures ownership before writing to Tameable's ZDO-backed name, mirroring the same
        // ClaimOwnership() guard Init(string) already uses before its own claim takes effect -
        // Tameable.SetText() ultimately routes through RPC_SetName, which only writes the ZDO on
        // the peer that owns it. Skipping this can silently no-op, leaving a stale name (e.g. a
        // previous claimant's) stuck in the ZDO forever - which a later re-claim would then wrongly
        // capture as the creature's "original" name, since that's only ever captured once.
        private void SetTameableText(string text)
        {
            if (m_netView != null && m_netView.IsValid() && !m_netView.IsOwner())
                m_netView.ClaimOwnership();

            gameObject.GetComponent<Tameable>()?.SetText(text);
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
