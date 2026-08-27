using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreaturePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private readonly int creatureDataHash = "CreatureData_WBTI".GetStableHashCode();

        private string m_redeemerName;
        private string m_redeemTitle;
        private string m_savedPrefabName;

        // Non-empty only for a creature summoned by another creature's own vanilla SpawnAbility
        // (e.g. a Warlock/Oozer/Gjall's reinforcements - see CreatureHelper.ApplyInheritedScaling).
        // m_savedPrefabName stays the *parent's* lookup prefab (the one that actually resolves in
        // the redeem's creature list); this holds the child's own real prefab, used to clone the
        // resolved entry and override just its prefabName before applying.
        private string m_actualPrefabName = "";

        private string m_colorOverride;
        private bool m_forceColorOverride;
        public bool m_ignoreWard;
        public bool m_isFollowing;
        public bool m_allowDamageStructures = true;
        public float m_damageScale = 0;
        public float m_healthScale = 0;

        // Permanent feature (see CreatureData.fullyPassive). Read by
        // harmony/SpecialRedeemPatchesWBTI.cs's IsEnemy prefix.
        public bool IsFullyPassive;

        // Permanent feature (see CreatureData.alwaysFollowOwner). Read by
        // harmony/SpecialRedeemPatchesWBTI.cs's Follow prefix, to make this creature keep closing
        // the gap instead of stopping ~3m out like a normal followed creature.
        public bool WantsToCloseDistance;

        public string RedeemerName => m_redeemerName;
        public string RedeemTitle => m_redeemTitle;
        public string SavedPrefabName => m_savedPrefabName;

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                string creatureStringData = m_netView.GetZDO().GetString(creatureDataHash, "");

                if (creatureStringData == "")
                    return;

                string[] data = creatureStringData.Split('|');
                m_redeemerName     = data[0];
                m_redeemTitle      = data[1];
                m_savedPrefabName  = data[2];
                m_ignoreWard       = bool.Parse(data[3]);
                m_isFollowing      = data.Length > 4 && bool.Parse(data[4]);
                m_actualPrefabName = data.Length > 5 ? data[5] : "";

                TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();

                if (m_redeemTitle == "")
                {
                    // This is the rehydration path for a permanent manual claim (chattingClaimDuration
                    // == 0), which can be on an AnimalAI creature (e.g. Deer) with no Humanoid component.
                    // ReInit's AddCreatureAssignment call already sets the display name itself, bracket-
                    // aware, via TwitchChatting.RefreshIndexDisplayForOwner -> SetDisplayIndex - do not
                    // also set Character.m_name here, it would clobber a just-applied [n] suffix back to
                    // the plain name whenever this happens to be the last of its owner's simultaneously-
                    // loaded manual claims to rehydrate.
                    creatureClaim.ReInit(m_redeemerName, isSpawn: false);
                    return;
                }

                RedeemData redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);

                if (redeem == null)
                {
                    Jotunn.Logger.LogError($"Could not find redeem '{m_redeemTitle}' in TwitchCreaturePersistentData, skipping setup.");
                    return;
                }

                if (m_savedPrefabName == "")
                {
                    // SpawnAbility redeems (e.g. biome-prefab showers like "Roots on the line!") use a
                    // placeholder CreatureData with no prefabName, so there's nothing to look up or reapply.
                    creatureClaim.ReInit(m_redeemerName, isSpawn: true);
                    return;
                }

                List<CreatureData> resolvedList = RedeemHelper.GetResolvedCreatureList(redeem.creatureData);
                CreatureData lookupData = resolvedList.Find(c => c.prefabName == m_savedPrefabName);

                if (lookupData == null)
                {
                    Jotunn.Logger.LogError($"Could not find creature '{m_savedPrefabName}' in resolved list for redeem '{m_redeemTitle}'");
                    return;
                }

                // A non-empty m_actualPrefabName means this instance is a creature summoned by
                // another creature's own SpawnAbility (see CreatureHelper.ApplyInheritedScaling) -
                // m_savedPrefabName is the parent's own lookup prefab (the one that resolves in the
                // redeem's list above), not this instance's actual prefab. Clone the resolved entry
                // and swap in the real prefab identity before applying.
                CreatureData creatureData = lookupData;

                if (m_actualPrefabName != "")
                {
                    creatureData = lookupData.Clone<CreatureData>();
                    creatureData.prefabName = m_actualPrefabName;
                }

                creatureClaim.ReInit(m_redeemerName, isSpawn: true, creatureData);
                ApplyCreatureData(creatureData);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError($"Error in TwitchCreaturePersistentData: {e}");
                Jotunn.Logger.LogError($"{m_redeemerName}, {m_redeemTitle}, {m_savedPrefabName}");
            }
        }

        // Shared "apply a resolved CreatureData" tail - used by Awake()'s rehydration (direct redeem
        // spawns and inherited summon-children alike) and by SetInherited (the owner's own client,
        // applying immediately rather than waiting for its own Awake()/ZDO round-trip).
        private void ApplyCreatureData(CreatureData creatureData)
        {
            m_colorOverride = creatureData.color;
            m_forceColorOverride = creatureData.forceColor;

            ApplyVariables(creatureData);
            ApplyHumanoid(m_redeemerName, creatureData);
            ApplyMonsterAI(creatureData);
            ApplyAllowDrops(creatureData);
            ApplyTameable(creatureData);
        }

        // Called once by CreatureHelper.ApplyInheritedScaling, on the owning client, right after a
        // creature summoned by another creature's own SpawnAbility is instantiated (a Warlock/Oozer/
        // Gjall's reinforcements). redeemerName/redeemTitle/lookupPrefabName come from the summoning
        // creature's own TwitchCreaturePersistentData; resolvedChildCreatureData is the parent's
        // resolved CreatureData already cloned with prefabName overridden to this child's own prefab.
        public void SetInherited(string redeemerName, string redeemTitle, string lookupPrefabName, CreatureData resolvedChildCreatureData)
        {
            m_redeemerName     = redeemerName;
            m_redeemTitle      = redeemTitle;
            m_savedPrefabName  = lookupPrefabName;
            m_actualPrefabName = resolvedChildCreatureData.prefabName;
            m_ignoreWard       = false;
            m_isFollowing      = false;

            WriteZDO();

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.Creature, true);

            TwitchCreatureClaim creatureClaim = gameObject.GetComponent<TwitchCreatureClaim>() ?? gameObject.AddComponent<TwitchCreatureClaim>();
            creatureClaim.ReInit(m_redeemerName, isSpawn: true, resolvedChildCreatureData);

            ApplyCreatureData(resolvedChildCreatureData);
        }

        public void Start()
        {
            try
            {
                if (m_redeemerName != null && RecolorHelper.CanRecolorCreature(m_redeemerName, gameObject.name, m_colorOverride))
                    RecolorHelper.RecolorCreature(m_redeemerName, gameObject, m_colorOverride, m_forceColorOverride);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchCreaturePersistentData.Start failed: " + e);
            }
        }

        public void SetData(string name, CreatureData creatureData = null, string redeemTitle = "", bool ignoreWard = false)
        {
            m_redeemerName    = name;
            m_redeemTitle     = redeemTitle;
            m_ignoreWard      = ignoreWard;
            m_savedPrefabName = creatureData?.prefabName ?? "";
            m_colorOverride   = creatureData?.color;
            m_forceColorOverride = creatureData?.forceColor ?? false;

            // isFollowing defaults to commandable for new spawns
            m_isFollowing = creatureData?.commandable ?? false;

            WriteZDO();

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.Creature, true);

            if (creatureData == null)
                return;

            ApplyVariables(creatureData);
            ApplyHumanoid(name, creatureData);
            ApplyMonsterAI(creatureData);
            ApplyAllowDrops(creatureData);
            ApplyTameable(creatureData);
        }

        // Called by TwitchCreatureClaim.OnDestroy() when a manually-claimed (non-spawn) creature is
        // genuinely unclaimed - without this, the ZDO string field this component reads on Awake()
        // (and the Creature flag that makes TwitchBasePersistentData re-add this component at all)
        // would still be set, so the next reload would re-hydrate the claim right back into existence.
        public void ClearClaimData()
        {
            if (m_netView == null || !m_netView.IsOwner())
                return;

            m_netView.GetZDO().Set(creatureDataHash, "");

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.Creature, false);
        }

        public void SetFollowing(bool following)
        {
            m_isFollowing = following;
            WriteZDO();
        }

        private void WriteZDO()
        {
            if (!m_netView.IsOwner())
                return;

            m_netView.GetZDO().Set(creatureDataHash, $"{m_redeemerName}|{m_redeemTitle}|{m_savedPrefabName}|{m_ignoreWard}|{m_isFollowing}|{m_actualPrefabName}");
        }

        private void ApplyVariables(CreatureData creatureData)
        {
            // m_isFollowing is managed via SetData/SetFollowing — not overwritten here
            m_allowDamageStructures = creatureData.allowDamageStructures;
            m_damageScale           = creatureData.damageScale;
            m_healthScale           = creatureData.healthScale;
            transform.localScale    = new Vector3(creatureData.size, creatureData.size, creatureData.size);

            if (creatureData.speedMultiplier != 1f)
            {
                Character character = gameObject.GetComponent<Character>();

                if (character != null)
                {
                    character.m_speed    *= creatureData.speedMultiplier;
                    character.m_runSpeed *= creatureData.speedMultiplier;
                }
            }
        }

        private void ApplyHumanoid(string redeemerName, CreatureData creatureData)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();

            if (humanoid == null)
            {
                Jotunn.Logger.LogWarning("Humanoid component is null on " + gameObject.name);
                return;
            }

            humanoid.SetLevel(creatureData.level);
            humanoid.m_boss               = creatureData.isBoss;
            humanoid.m_bossEvent          = creatureData.bossEvent;
            humanoid.m_defeatSetGlobalKey = "";

            if (ProfileSettingsHelper.Current.creaturesSameFaction)
                humanoid.m_faction = Character.Faction.Boss;

            if (creatureData.friendly)
            {
                humanoid.m_faction = Character.Faction.Players;
                humanoid.m_group   = WizshBoneTwitchIntegration.HumanoidGroupSpawnFriendly;
            }

            if (creatureData.group != null)
                humanoid.m_group = creatureData.group;

            if (creatureData.maxHealth > 0)
                humanoid.SetMaxHealth(creatureData.maxHealth);

            if (creatureData.rename)
                humanoid.m_name = creatureData.name ?? redeemerName;

            if (ProfileSettingsHelper.Current.creaturesScaling)
            {
                ValheimCreature valheimCreature = CreatureHelper.GetValheimCreature(gameObject.name);

                if (valheimCreature == null)
                {
                    Jotunn.Logger.LogWarning($"No ValheimCreature entry found for {gameObject.name}, skipping health scaling.");
                    return;
                }

                float playerTier  = ProgressionHelper.GetPlayerTier();
                float healthScale = m_healthScale != 0 ? m_healthScale : ProfileSettingsHelper.Current.creaturesHealthScale;
                float scale       = CreatureHelper.CalculateScale(playerTier, valheimCreature.tier, healthScale);

                humanoid.SetMaxHealth(humanoid.GetMaxHealth() * scale);
                humanoid.SetHealth(humanoid.GetMaxHealth());
            }
        }

        private void ApplyMonsterAI(CreatureData creatureData)
        {
            MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();

            if (monsterAI == null)
            {
                Jotunn.Logger.LogWarning("MonsterAI component is null on " + gameObject.name);
                return;
            }

            monsterAI.m_aggravatable = creatureData.aggravatable;
            monsterAI.m_mistVision   = creatureData.mistVision;

            // See the field comment on CreatureData.fullyPassive.
            IsFullyPassive = creatureData.fullyPassive;

            if (creatureData.idleSoundInterval > 0)
            {
                monsterAI.m_idleSoundChance = 1f;
                monsterAI.m_idleSoundInterval = creatureData.idleSoundInterval;

                // BaseAI.Awake() already scheduled DoIdleSound via InvokeRepeating using its prefab
                // default interval before we get here - just changing m_idleSoundInterval wouldn't
                // affect that already-running timer, so re-arm it to actually apply the override.
                monsterAI.CancelInvoke("DoIdleSound");
                monsterAI.InvokeRepeating("DoIdleSound", creatureData.idleSoundInterval, creatureData.idleSoundInterval);
            }
        }

        private void ApplyAllowDrops(CreatureData creatureData)
        {
            if (creatureData.allowDrops)
                return;

            CharacterDrop characterDrop = gameObject.GetComponent<CharacterDrop>();

            if (characterDrop == null)
            {
                Jotunn.Logger.LogWarning("CharacterDrop component is null on " + gameObject.name);
                return;
            }

            // CharacterDrop.m_drops isn't just loot - some creatures (e.g. the Oozer's on-death
            // split into two Blobs) use it to spawn another creature as a core mechanic, not an item
            // drop. allowDrops is meant to suppress loot farming, not break that - only strip entries
            // whose prefab is an actual item (has ItemDrop), leaving creature-spawn entries intact.
            characterDrop.m_drops.RemoveAll(drop => drop.m_prefab != null && drop.m_prefab.GetComponent<ItemDrop>() != null);
        }

        private void ApplyTameable(CreatureData creatureData)
        {
            if (!creatureData.friendly)
                return;

            Tameable tameable = gameObject.GetComponent<Tameable>();

            if (tameable == null)
                tameable = gameObject.AddComponent<Tameable>();

            tameable.m_monsterAI.MakeTame();

            if (creatureData.commandable)
            {
                tameable.m_commandable = true;

                if (m_isFollowing && Player.m_localPlayer != null)
                    tameable.m_monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
            }

            // See the field comment on CreatureData.alwaysFollowOwner. Player.m_localPlayer
            // is only assigned later by an explicit SetLocalPlayer() call, not during Awake() - on a
            // relog/zone-reload this Awake()-driven rehydration can easily run before that happens, so a
            // one-shot null check here silently drops the follow target. Retry briefly instead.
            if (creatureData.alwaysFollowOwner)
                StartCoroutine(SetFollowTargetWhenPlayerReady(tameable.m_monsterAI));

            WantsToCloseDistance = creatureData.alwaysFollowOwner;
        }

        // See the comment on the ApplyTameable call site above. Bounded wait
        // (10s) rather than an indefinite one, in case Player.m_localPlayer genuinely never shows up
        // (e.g. this instance rehydrating in a context with no local player at all).
        private IEnumerator SetFollowTargetWhenPlayerReady(MonsterAI monsterAI)
        {
            float timeout = 10f;

            while (Player.m_localPlayer == null && timeout > 0f)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            if (Player.m_localPlayer != null && monsterAI != null)
                monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
        }
    }
}
