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

        // Set when the release came from the ZDO (another client released or expired the claim) rather
        // than from this client - the ZDO and Tameable name were already cleaned up by that client.
        private bool m_isRemoteRelease = false;

        public string ClaimedUserName => m_assignment?.userName;

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

        // permanent is set for a "claim:" rename (made by any player, never expires); a claim accepted
        // from chat ("!claim", the scan commands) is timed by chattingClaimDuration and gets stamped with
        // this streamer's channel so that only this streamer's client runs its expiry and numbering.
        public void Init(string userName, bool permanent = false)
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

            m_assignment = new TwitchCreatureAssignment(userName, gameObject, permanent ? 0 : ProfileSettingsHelper.Current.chattingClaimDuration);

            // Only captured on the very first claim (re-claim:-renaming an already-claimed creature
            // calls Init again on this same component) - otherwise re-claiming would only ever restore
            // the *previous* claimant's name instead of the true original once unclaimed/expired.
            if (m_originalName == null)
            {
                m_originalName = GetOriginalName();

                // Tameable.GetHoverName() prefers this ZDO field over Character.m_name whenever it's
                // non-empty, so resetting only m_character.m_name isn't enough to actually change what
                // the player sees once a creature has ever been claim:-renamed - captured here, before
                // any pending claim:-rename gets persisted, so the true original can be restored later.
                if (m_netView != null && m_netView.IsValid())
                    m_originalTamedName = m_netView.GetZDO().GetString(ZDOVars.s_tamedName, "");
            }

            // Null for a non-creature claim (e.g. a Rocky) - its name lives only in the Tameable's ZDO
            // field, which the vanilla SetText that triggered this claim is already writing.
            if (m_character != null)
                m_character.m_name = userName;

            // Persisted whatever the claim duration: the ZDO is also how every other client learns about
            // the claim (TwitchBasePersistentData's claim watcher). A timed claim that is reloaded just
            // gets its expiry timer restarted (see ReInit). A "claim:" rename is left unstamped on
            // purpose - a streamer's client adopts it later, for numbering only (see SetDisplayIndex).
            if (!(gameObject.GetComponent<TwitchBasePersistentData>()?.IsRedeemSpawn ?? false))
            {
                // Refresh persistent data on every manual claim, not just the first - otherwise
                // re-claim:-renaming an already-claimed creature leaves the previous claimant's name
                // (and color, on reload) stuck in the ZDO forever, since SetData() would never run again.
                // Only skip this for a genuinely redeem-spawned creature (IsRedeemSpawn) - one that already
                // has real persistent data (title/prefab/color) which this claim:-rename must not wipe.
                TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>() ?? gameObject.AddComponent<TwitchCreaturePersistentData>();
                persistentData.SetData(m_assignment.userName, claimChannel: permanent ? "" : m_chatting.ClaimChannel, claimPermanent: permanent);
            }

            ApplyRecolor();

            m_chatting.AddCreatureAssignment(m_assignment);
            SetupNpcTalk();
            m_chatting.onNewMessage.AddListener(CheckChatForMessage);

            StartExpiry();
        }

        // Re-claiming an already-claimed creature (claim:-renaming it again) reuses this same
        // component instead of tearing it down first, so a previous claim's color would otherwise
        // stick around if the new name isn't a special viewer - reset to original in that case.
        // RecolorCreature always replaces the material outright, so no reset is needed when the
        // new name does recolor.
        public void ApplyRecolor()
        {
            if (RecolorHelper.IsPetRock(gameObject))
            {
                if (RecolorHelper.CanRecolorPetRock(m_assignment.userName, gameObject))
                    RecolorHelper.RecolorPetRock(m_assignment.userName, gameObject);
                else
                    RecolorHelper.UnColorPetRock(gameObject);
            }
            else if (RecolorHelper.CanRecolorCreature(m_assignment.userName, m_assignment.creature.name))
            {
                RecolorHelper.RecolorCreature(m_assignment.userName, gameObject);
                CreatureHelper.GetValheimCreature(m_assignment.creature.name)?.spawnEffects.Create(transform.position, transform.rotation);
            }
            else if (RecolorHelper.IsCreatureInList(m_assignment.creature.name))
                RecolorHelper.UnColorCreature(gameObject);
        }

        // Duration == 0 means permanent - only self-expire timed claims. Runs on its own schedule
        // instead of only re-checking when the claimed user happens to chat again, so a claim still
        // expires even if they never do. Started on every client that sets the claim up (a timed claim
        // is also restored from the ZDO), but CheckExpiry only acts on the streamer the claim belongs to.
        private void StartExpiry()
        {
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
        //
        // displayName and permanent are only meaningful for a manual claim (!isSpawn): the numbered name
        // the managing streamer's client stored in the ZDO, and whether the claim never expires. Also
        // called again, on an already-set-up component, when the ZDO says another viewer took the
        // creature over - the previous assignment is replaced.
        public void ReInit(string userName, bool isSpawn, CreatureData creatureData = null, string displayName = null, bool permanent = false)
        {
            if (userName == null)
            {
                Jotunn.Logger.LogWarning("Cannot assign a user to this creature claim, user name is null");
                return;
            }

            TwitchCreatureAssignment previousAssignment = m_assignment;

            if (previousAssignment != null)
                m_chatting.onNewMessage.RemoveListener(CheckChatForMessage);

            m_isSpawn = isSpawn;
            m_assignment = new TwitchCreatureAssignment(userName, gameObject, permanent ? 0 : ProfileSettingsHelper.Current.chattingClaimDuration);

            // Null-safe like Init(CreatureData, CustomRewardEvent)'s equivalent line - some
            // rehydration paths (see TwitchCreaturePersistentData.Awake) have no Character component,
            // so m_character is genuinely null here. Only captured once: on a re-assignment
            // Character.m_name already holds the previous claimant's name.
            if (m_originalName == null)
                m_originalName = GetOriginalName();

            // A manually-claimed non-creature (e.g. a Rocky) has no Character.m_name to restore, only
            // the Tameable's ZDO name - which is the claimed name by now. Its unclaimed default is
            // empty, so restore that on release instead of leaving the claimant's name stuck.
            if (!isSpawn && m_character == null)
                m_originalTamedName = "";

            if (previousAssignment != null)
                m_chatting.ReplaceCreatureAssignment(previousAssignment, m_assignment);
            else
                m_chatting.AddCreatureAssignment(m_assignment);

            if (!isSpawn)
                ApplyStoredDisplayName(displayName);

            SetupNpcTalk(creatureData);
            m_chatting.onNewMessage.AddListener(CheckChatForMessage);

            // A manual claim restored from the ZDO can be a timed one (they are persisted too) - restart
            // its expiry timer. Redeem spawns never expire.
            if (!isSpawn)
                StartExpiry();
        }

        // Shows the numbered name that the managing streamer's client stored in the ZDO. That client
        // numbers its own claims in SetDisplayIndex, so it never applies the stored value; everyone else
        // must, because they can't compute the "[n]" suffix (it depends on the owner's other loaded
        // claims). Falls back to the plain claimant name until the numbering client has written one.
        public void ApplyStoredDisplayName(string displayName)
        {
            if (m_isSpawn || m_character == null || IsManagedByThisClient())
                return;

            m_character.m_name = string.IsNullOrEmpty(displayName) ? m_assignment.userName : displayName;
        }

        // Whether this client's streamer is the one this claim is stamped with - the only client that
        // runs its expiry and writes its numbered name. False on every client that isn't logged in to
        // Twitch, and on the other streamer's client when two share a world: they just mirror the ZDO.
        private bool IsManagedByThisClient()
        {
            string channel = m_chatting.ClaimChannel;

            return channel != "" && channel == gameObject.GetComponent<TwitchCreaturePersistentData>()?.ClaimChannel;
        }

        // Whether the streamer on `channel` numbers this claim: it is stamped with that channel, or it
        // is an unstamped claim ("claim:" rename) the streamer may adopt. Used by TwitchChatting to pick
        // the claims it counts for the "[n]" suffix and "!unclaim <n>".
        public bool IsNumberedBy(string channel)
        {
            if (m_isSpawn || channel == "")
                return false;

            TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>();

            if (persistentData == null)
                return false;

            return persistentData.ClaimChannel == "" || persistentData.ClaimChannel == channel;
        }

        // Character.m_name for creatures; the Piece name for a non-creature claim (e.g. a Rocky) so
        // "!unclaim <name>" can still match it by its species name.
        private string GetOriginalName()
        {
            return m_character != null ? m_character.m_name : gameObject.GetComponent<Piece>()?.m_name;
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

                if (m_assignment?.creature != null && RecolorHelper.CanRecolorPetRock(m_assignment.userName, m_assignment.creature))
                    RecolorHelper.UnColorPetRock(m_assignment.creature);
                else if (m_assignment?.creature != null && RecolorHelper.CanRecolorCreature(m_assignment.userName, m_assignment.creature.name))
                    RecolorHelper.UnColorCreature(m_assignment.creature);

                if (m_assignment != null)
                    m_assignment.creature = null;

                if (m_character != null)
                    m_character.m_name = m_originalName;

                // Restores the vanilla Tameable rename field alongside Character.m_name above - only
                // set for claims made via Init(string) (see the capture there for why).
                if (m_originalTamedName != null && !m_isRemoteRelease)
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
                        // A remote release already had its ZDO cleared by the client that released it.
                        if (!m_isRemoteRelease)
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

            m_npcTalk.m_aggravated = ParseTalkMessages(creatureData.talkMessage);

            if (creatureData.talkInterval >= 3f)
                InvokeRepeating(nameof(SayAMessage), 0f, creatureData.talkInterval);
            else if (!creatureData.talkInteract)
                SayAMessage();
        }

        private void SetupSimpleTalk(CreatureData creatureData)
        {
            if (creatureData == null || !creatureData.talks || creatureData.talkMessage == null)
                return;

            m_simpleTalkMessages = ParseTalkMessages(creatureData.talkMessage);

            if (creatureData.talkInterval >= 3f)
                InvokeRepeating(nameof(SaySimpleMessage), 0f, creatureData.talkInterval);
            else
                SaySimpleMessage();
        }

        // Splits a talk message into its ';'-separated parts and replaces {{user}} (the same
        // placeholder every announcement message uses) with the redeemer's name in each. Shared by
        // both talk paths so the placeholder works the same for single and multiple messages.
        private List<string> ParseTalkMessages(string talkMessage)
        {
            List<string> messages = new List<string>(talkMessage.Split(';'));

            for (int i = 0; i < messages.Count; i++)
                messages[i] = MessageHelper.ParseVariables("{{user}}", m_assignment.userName, messages[i]);

            return messages;
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

            Chat.instance.SetNpcText(gameObject, Vector3.up * GetBubbleHeight(),ProfileSettingsHelper.Current.chattingCullingRange, 10f, "", text, large: false);
        }

        // Creatures keep the fixed 2m anchor. A non-creature claim (e.g. a Rocky) can be far smaller, so
        // anchor just above the top of its meshes instead of floating a couple of meters over it.
        private float GetBubbleHeight()
        {
            if (m_character != null)
                return 2f;

            Bounds bounds = default;
            bool hasBounds = false;

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer))
                    continue;

                if (!hasBounds)
                    bounds = renderer.bounds;
                else
                    bounds.Encapsulate(renderer.bounds);

                hasBounds = true;
            }

            return hasBounds ? Mathf.Max(0.5f, bounds.max.y - transform.position.y + 0.5f) : 2f;
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

                BroadcastBubble("Alright... healing!");

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

            // Sent before the local talker check: that check ranks creatures by distance to THIS
            // client's player, and every receiver applies its own.
            BroadcastBubble(message.message);

            if (m_chatting.CanCreatureTalk(gameObject))
                ShowBubbleText(message.message);
        }

        // Chat-driven bubbles only exist on the client that receives viewer chat - tell the others so
        // they show it too (see ChatBubbleSyncHelper). Scripted talk is not broadcast, every client
        // runs that itself.
        private void BroadcastBubble(string text)
        {
            if (m_netView != null && m_netView.IsValid())
                ChatBubbleSyncHelper.Broadcast(m_netView.GetZDO().m_uid, text);
        }

        // Receiving end of ChatBubbleSyncHelper: the streamer's client showed this creature's bubble,
        // so show it here too, subject to this client's own talker limit and culling range.
        public void ShowRemoteBubble(string text)
        {
            if (m_chatting == null || !m_chatting.CanCreatureTalk(gameObject))
                return;

            ShowBubbleText(text);
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

        // Called when the ZDO says this claim was released by another client (chat unclaim, expiry,
        // unclaim:) - see TwitchCreaturePersistentData.OnZdoChanged. Tears down only this client's copy.
        public void ApplyRemoteRelease()
        {
            m_isRemoteRelease = true;
            m_isUnclaimDestroy = true;
            Unassign();
        }

        public void CheckExpiry()
        {
            // The claimant's chat only ever reaches the client of the streamer whose channel they chat in,
            // so only that client can tell they went quiet. Everywhere else - including the other
            // streamer's client when two share a world - the claim just waits for the ZDO to be cleared.
            if (!IsManagedByThisClient())
            {
                m_lastMessageTime = DateTime.Now;
                return;
            }

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
            // Only the streamer this claim belongs to numbers it: the suffix depends on which of the
            // owner's claims that client has loaded, so any other client computing it would overwrite the
            // shared name with a different one. Others read it from the ZDO (ApplyStoredDisplayName).
            string channel = m_chatting.ClaimChannel;
            TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>();

            if (m_isSpawn || channel == "" || persistentData == null)
                return;

            if (persistentData.ClaimChannel == "")
            {
                // An unstamped claim ("claim:" rename) - adopt it so it gets numbered. Needs the ZDO owned
                // for the write; if another streamer adopts it at the same moment the last write wins and
                // this client turns into a mirror (TwitchCreaturePersistentData.OnZdoChanged).
                if (m_netView != null && m_netView.IsValid() && !m_netView.IsOwner())
                    m_netView.ClaimOwnership();

                persistentData.SetClaimChannel(channel);
            }
            else if (persistentData.ClaimChannel != channel)
                return;

            string displayName = index.HasValue ? $"{m_assignment.userName} [{index}]" : m_assignment.userName;

            if (m_character != null)
                m_character.m_name = displayName;

            // Claims ownership first, which the ZDO write below needs too.
            SetTameableText(displayName);
            persistentData.SetClaimDisplayName(displayName);
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
