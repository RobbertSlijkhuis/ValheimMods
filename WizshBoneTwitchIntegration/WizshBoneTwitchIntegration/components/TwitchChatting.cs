using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Helpers;
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

        public float m_scanRadius = ProfileSettingsHelper.Current.chattingRadius;
        public float m_scanInterval = ProfileSettingsHelper.Current.chattingInterval;
        private string m_chosenUser;
        private GameObject m_chosenPrefab;

        public void Awake()
        {
            try
            {
                m_auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                m_chat = Game.instance.gameObject.GetComponent<TwitchChat>();
                m_enabled = ProfileSettingsHelper.Current.chattingEnabled;

                DeserializeUserBlackList(ProfileSettingsHelper.Current.chattingBlackList);

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
            RefreshIndexDisplayForOwner(assignment.userName);
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
            RefreshIndexDisplayForOwner(assignment.userName);
        }

        // Only counts manually-claimed (non-spawn) creatures currently loaded near a player.
        // Lowercased on both sides since callers pass either the raw stored assignment.userName
        // (from AddCreatureAssignment/RemoveCreatureAssignment) or an already-lowercased incoming
        // chat/command username (from UnclaimForUser) - never assume either side's casing.
        private List<TwitchCreatureAssignment> NonSpawnSiblings(string userName)
        {
            return m_creatureAssignments
                .Where(a => a.userName.ToLower() == userName.ToLower() && !(a.creature.GetComponent<TwitchCreatureClaim>()?.m_isSpawn ?? true))
                .OrderBy(a => a.claimedAt)
                .ToList();
        }

        // Renumbers (or clears) the [n] suffix shown in a claimed creature's name for every one of
        // this owner's other currently-loaded manual claims - called whenever one is added/removed,
        // since the whole set's numbering can shift when membership changes.
        private void RefreshIndexDisplayForOwner(string userName)
        {
            List<TwitchCreatureAssignment> siblings = NonSpawnSiblings(userName);

            for (int i = 0; i < siblings.Count; i++)
            {
                TwitchCreatureClaim claim = siblings[i].creature?.GetComponent<TwitchCreatureClaim>();
                claim?.SetDisplayIndex(siblings.Count >= 2 ? (int?)(i + 1) : null);
            }
        }

        // Single resolution point for both the real "!unclaim" chat command (TwitchChat.cs) and the
        // WBTIUnclaim admin command - resolves the full set of matching assignments up front, from
        // one stable snapshot, THEN releases them. Releasing a claim renumbers its still-remaining
        // siblings (RefreshIndexDisplayForOwner) - resolving everyone who matches before releasing
        // anyone means that renumbering can never change which claim(s) this call already decided
        // to target, unlike re-deriving "who's number N" fresh after each release mid-loop.
        public void UnclaimForUser(string userName, string target)
        {
            List<TwitchCreatureAssignment> owned = m_creatureAssignments
                .Where(a => a.userName.ToLower() == userName.ToLower())
                .ToList();

            List<TwitchCreatureAssignment> toRelease;

            if (string.IsNullOrEmpty(target))
            {
                toRelease = owned;
            }
            else if (int.TryParse(target, out int index))
            {
                List<TwitchCreatureAssignment> nonSpawnSiblings = NonSpawnSiblings(userName);
                TwitchCreatureAssignment match = index >= 1 && index <= nonSpawnSiblings.Count ? nonSpawnSiblings[index - 1] : null;
                toRelease = match != null ? new List<TwitchCreatureAssignment> { match } : new List<TwitchCreatureAssignment>();
            }
            else
            {
                toRelease = owned
                    .Where(a => a.creature?.GetComponent<TwitchCreatureClaim>()?.MatchesNameTarget(target) ?? false)
                    .ToList();
            }

            foreach (TwitchCreatureAssignment assignment in toRelease)
                assignment.creature?.GetComponent<TwitchCreatureClaim>()?.Release();
        }

        public bool ContainsCreatureAssignment(TwitchCreatureAssignment assignment)
        {
            return m_creatureAssignments.Contains(assignment);
        }

        public bool ContainsCreatureAssignment(string userName)
        {
            return m_creatureAssignments.Find(item => item.userName.ToLower() == userName.ToLower()) != null;
        }

        public List<TwitchCreatureAssignment> GetAllCreatureAssignments()
        {
            return m_creatureAssignments;
        }

        public TwitchCreatureAssignment GetCreatureAssignment(string userName)
        {
            return m_creatureAssignments.Find(item => item.userName.ToLower() == userName.ToLower());
        }

        public bool CanCreatureTalk(GameObject creature)
        {
            int maxTalkers = ProfileSettingsHelper.Current.chattingMaxTalkers;

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

        public void ScanAndAssignUsers(bool bypassGate = false, bool forceClaim = false, string forceClaimUserName = null)
        {
            if (!bypassGate && (!m_auth.m_loggedIn || !m_enabled))
                return;

            List<GameObject> creatures = new List<GameObject>();

            // Player is likely dead
            if (Player.m_localPlayer == null)
                return;

            Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, m_scanRadius, LayerMask.GetMask("character"));

            foreach (Collider obj in objects)
            {
                TwitchCreatureClaim creatureClaim = obj.GetComponent<TwitchCreatureClaim>();
                Character character = obj.gameObject.GetComponent<Character>();
                // BaseAI (not MonsterAI) so AnimalAI-driven creatures (e.g. Deer) are eligible too -
                // claiming itself never touches MonsterAI-specific behavior (see TwitchCreatureClaim).
                BaseAI baseAI = obj.gameObject.GetComponent<BaseAI>();

                if (baseAI == null || creatureClaim != null)
                    continue;

                // Tamed creatures are never eligible for the auto-scan offer, regardless of settings -
                // viewers should never be able to accidentally claim someone's tame this way.
                if (character != null && character.m_tamed)
                    continue;

                if (forceClaim)
                {
                    TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
                    TwitchCreatureClaim newCreatureClaimn = obj.gameObject.AddComponent<TwitchCreatureClaim>();
                    newCreatureClaimn.Init(forceClaimUserName ?? (customRewards.m_alias ?? "DeathWizsh"));
                    return;
                }

                // Free-for-all: no user is pre-selected - the creature just becomes claimable by
                // whoever types "!claim" first (see AcceptClaim/TwitchChat's dispatch).
                if (ProfileSettingsHelper.Current.chattingClaimFreeForAll)
                {
                    m_chosenUser = null;
                    m_chosenPrefab = obj.gameObject;
                    m_chat.Send($"A {CreatureHelper.StripColorTags(Localization.instance.Localize(character.m_name))} is up for claiming! Type \"!claim\" to claim it!");
                    break;
                }

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
                m_chat.Send($"{chosenUser} you have been selected to become a {CreatureHelper.StripColorTags(Localization.instance.Localize(character.m_name))}! Type \"!claim\" to accept.");
                break;
            }
        }

        // userName is the actual chatter who typed "!claim" - TwitchChat.cs has already gated on
        // whether that's allowed to trigger this at all (only m_chosenUser, unless free-for-all).
        public void AcceptClaim(string userName)
        {
            if (m_chosenPrefab == null)
                return;

            // Free-for-all: a user who already has a claim just falls through, leaving the offer
            // open for someone else's "!claim" instead of stealing/duplicating their own claim.
            if (ContainsCreatureAssignment(userName))
            {
                Jotunn.Logger.LogWarning($"[WBTI] AcceptClaim: {userName} already has a claim, ignoring.");
                return;
            }

            // The offer was validated as unclaimed back at scan time, but time passes between the offer
            // and this accept - re-check here so we can never end up adding a second TwitchCreatureClaim
            // on top of one that already exists (which would silently steal/corrupt an existing claim).
            if (m_chosenPrefab.GetComponent<TwitchCreatureClaim>() != null)
            {
                Jotunn.Logger.LogWarning($"[WBTI] AcceptClaim: {userName} tried to claim a creature that's already claimed, ignoring.");
                m_chosenUser = null;
                m_chosenPrefab = null;
                return;
            }

            Character character = m_chosenPrefab.GetComponent<Character>();
            m_chat.Send($"Creature {CreatureHelper.StripColorTags(Localization.instance.Localize(character.m_name))} is now claimed by {userName}!");

            TwitchCreatureClaim newCreatureClaimn = m_chosenPrefab.AddComponent<TwitchCreatureClaim>();
            newCreatureClaimn.Init(userName);

            m_chosenUser = null;
            m_chosenPrefab = null;
        }

        public string GetChosenUser()
        {
            return m_chosenUser;
        }

        // Whether there's a pending auto-scan claim offer waiting to be accepted - used by the
        // free-for-all "!claim" dispatch in TwitchChat.cs, which has no single pre-selected user
        // to compare against.
        public bool HasOpenClaim()
        {
            return m_chosenPrefab != null;
        }

        public List<string> GetUserBlacklist()
        {
            return m_userBlacklist;
        }

        public void DeserializeUserBlackList(string value)
        {
            m_userBlacklist = new List<string>();

            if (string.IsNullOrEmpty(value))
                return;

            foreach (string entry in value.Split(','))
            {
                string trimmed = entry.Trim();
                if (trimmed.Length > 0)
                    m_userBlacklist.Add(trimmed.ToLower());
            }
        }

        /// <summary>
        /// A blank list (no entries) is valid - it just blacklists no one. Anything else is only
        /// valid when every comma-separated entry has actual content once trimmed, so a trailing/
        /// leading/double comma (an empty entry) is rejected - see <see cref="DeserializeUserBlackList"/>,
        /// which silently drops those same empty entries rather than blacklisting an empty name.
        /// </summary>
        public static bool IsUserBlacklistValid(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
                return true;

            foreach (string entry in value.Split(','))
            {
                if (entry.Trim().Length == 0)
                    return false;
            }

            return true;
        }
    }
}
